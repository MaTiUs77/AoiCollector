using System;
using System.Data;

using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.IAServer;
using System.Diagnostics;
using CollectorPackage.Aoicollector.Inspection.Model;
using AOICollector.Src.Redis;
using Newtonsoft.Json.Linq;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class InspectionController : Panel
    {
        /// <summary>
        /// La placa que llego a TrazaSave, no se encuentra pendiente de inspeccion.
        /// </summary>
        /// <param name="path"></param>
        public void TrazaSave(string path)
        {
            Stopwatch tiempoEjecucion = Stopwatch.StartNew();

            history = new History();

            // Solo proceso si la etiqueta es FISICA, las etiquetas virtuales no se aceptan mas
            if (tipoBarcode.Equals("E"))
            {
                // Si se detectaron bloques
                if(bloqueList.Count>0)
                {
                    machine.LogBroadcast("verbose",
                       string.Format("Aoi: {0} | Inspector: {1} | Falsos: {3} | Reales: {4} | Pendiente: {2}",
                           revisionAoi,
                           revisionIns,
                           pendiente,
                           totalErroresFalsos,
                           totalErroresReales
                       )
                    );

                    if (pendiente)
                    {
                        machine.LogBroadcast("warning",
                            string.Format("Agregando panel a estado pendiente, se verificara en la siguiente ronda")
                        );

                        Pendiente.Save(this);
                    } else {
                        /*
                        Se consume el service y se obtiene toda la info actual de produccion
                        OP, Declaracion, Rutas, Regex, Inspector logueado etc...
                        */
                        machine.GetProductionInfoFromIAServer();
                        if (machine.prodService.error == null) // El stack lo muestro en el metodo GetProductionInfoFromIAServer, si es null simplemente finaliza operaciones
                        {
                            if (machine.prodService.result.error == null)
                            {
                                // Existe configuracion de produccion?
                                if (machine.prodService.result.produccion != null)
                                {
                                    // Existe alguna configuracion de ruta?
                                    if (machine.prodService.routeMode != null)
                                    {
                                        PanelHandlerService(path);
                                    } else {
                                        machine.LogBroadcast("error",
                                           string.Format("Se cancela la operacion, no existe puesto de produccion para {0}", machine.prodService.result.produccion.op)
                                       );
                                    }
                                } else {
                                    machine.LogBroadcast("error",
                                        string.Format("Error al obtener datos de OP en produccion del service, se cancela la operacion")
                                    );
                                }
                            } else {
                                machine.LogBroadcast("error",
                                    string.Format("Error al obtener resultados del service {0}", machine.prodService.result.error)
                                );
                            }
                        }
                    }
                } else {
                    machine.LogBroadcast("error",
                        string.Format("Error no se detectaron bloques")
                    );
                }                 
            } else {
                machine.LogBroadcast("error",
                    string.Format("Etiqueta virtual {0} en {1} | Maquina: {2}, se cancela la operacion", barcode, programa, machine.maquina)
                );
            }

            #region MONITOR REDIS
            string tiempoStyle = "success";
            if (tiempoEjecucion.ElapsedMilliseconds > 4000) { tiempoStyle = "warning"; }
            if (tiempoEjecucion.ElapsedMilliseconds > 6000) { tiempoStyle = "error"; }

            machine.LogBroadcast(tiempoStyle,
                string.Format("Tiempo de ejecucion (ms) {0} ",
                tiempoEjecucion.ElapsedMilliseconds)
            );

            JObject json = new JObject();
            json["tiempoEjecucion"] = tiempoEjecucion.ElapsedMilliseconds.ToString();
            json["aoibarcode"] = machine.line_barcode.ToString();
            json["smd"] = machine.smd.ToString();
            json["tipo"] = machine.tipo.ToString();
            Realtime.send(json.ToString());
            #endregion
        }

        private PanelService PanelHandlerService(string path)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Verifica si existe la placa en iaserver
            PanelService panelService = GetBarcodeInfoFromIAServer();
            sw.Stop();

            machine.LogBroadcast("success",
                string.Format("API GetBarcodeInfo Tiempo de respuesta: (ms) {0} ",
                (long)sw.ElapsedMilliseconds)
            );

            // Solo proceso si el servicio respondio sin problemas
            if (panelService.error == null)
            {
                machine.LogBroadcast("debug",
                    string.Format("Panel Modo: {0}", spMode)
                );

                try
                {
                    if (spMode.Equals("insert"))
                    {
                        // Placa nueva, solo procedo si la OP no esta completa...
                        /*
                        if (machine.prodService.isFinished())
                        {
                            machine.LogBroadcast("warning",
                                string.Format("Produccion finalizada")
                            );
                        }
                        */
                        SavePanel(panelService);
                    }
                    else
                    {
                        // La placa ya existe, actualizo sus datos
                        SavePanel(panelService);
                    }
                } catch (Exception ex) {
                    machine.log.stack(
                        string.Format("+ SavePanel exception"
                    ), this, ex);
                }

                // Si la linea tiene configurada la Trazabilidad por Cogiscan, valida ruta
                #region Validacion con rutas de cogiscan
                /*
                if (machine.prodService.result.produccion.cogiscan.Equals("T"))
                {
                    machine.LogBroadcast("warning",
                        string.Format("Esta ruta requiere validacion con cogiscan")
                    );
                    if (panelService.result.cogiscan.product.attributes.operation.Equals("AOI"))
                    {
                        SavePanel(panelService);
                    } else {
                        machine.LogBroadcast("warning",
                            string.Format("+ El panel {0} no se encuentra en la ruta AOI", barcode)
                        );
                    }
                } else {
                    SavePanel(panelService);
                }
                */
                #endregion
            }
            else { // IF ERROR EN SERVICE
                machine.log.stack(
                    string.Format("+ PanelService exception"
                ), this, panelService.error);
            }

            // Solo guardo bloques, si se realizaron correctamente las tareas anteriores
            if (panelId > 0 && panelService.error == null)
            {
                SaveBlocks(path);

                // Verifica si la ruta declara   
                if (machine.prodService.routeDeclare)
                {                     
                    // Verifica si la produccion no termino AOI_PROD >= AOI_QTY
                    if (!machine.prodService.isFinished())
                    {
                        machine.LogBroadcast("info",
                            string.Format("Declarando panel")
                        );

                        Declarar declarar = new Declarar();
                        declarar.withPanelBarcode(panelService.result.panel.panel_barcode);
                    } else
                    {
                        machine.LogBroadcast("warning",
                            string.Format("No se puede declarar el panel {0} se detecto produccion finalizada!", panelService.result.panel.panel_barcode)
                        );
                    }
                }

                #region Legacy mode, exporta XML
                /*
                foreach (Bloque bloque in bloqueList)
                {
                    if (machine.prodService.routeMode.Equals("sfcs"))
                    {
                        Export.toXML(this, bloque, path);
                    }
                }
                */
                #endregion
            }

            return panelService;
        }

        private PanelService SavePanel(PanelService panelService)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (Config.debugMode)
            {
                machine.LogBroadcast("warning", "Debug mode ON, no ejecuta StoreProcedure para guardar panel");
            } else { 
                #region SAVE PANELS ON DB
                string query = @"CALL sp_setInspectionPanel_optimizando('" + panelId + "','" + machine.mysql_id + "',  '" + barcode + "',  '" + programa + "',  '" + fecha + "',  '" + hora + "',  '',  '" + revisionAoi + "',  '" + revisionIns + "',  '" + totalErrores + "',  '" + totalErroresFalsos + "',  '" + totalErroresReales + "',  '" + pcbInfo.bloques + "',  '" + tipoBarcode + "',  '" + Convert.ToInt32(pendiente) + "' ,  '" + machine.oracle_id + "' ,  '" + vtwinProgramNameId + "' ,  '" + spMode + "'  );";

                machine.LogBroadcast("debug", 
                    string.Format("Ejecutando StoreProcedure: sp_setInspectionPanel_optimizando({0}) =>", barcode)
                );

                MySqlConnector sql = new MySqlConnector();
                sql.LoadConfig("IASERVER");
                DataTable sp = sql.Query(query);
                if (sql.rows)
                {
                    // En caso de insert, informo el id_panel creado, si fue un update, seria el mismo id_panel...
                    panelId= int.Parse(sp.Rows[0]["id_panel"].ToString());
                    //if (pendiente)
                    //{                           
                    //    Pendiente.Save(this);
                    //}

                    if (pendienteDelete)
                    {
                        Pendiente.Delete(this);
                    }

                    if (panelId > 0)
                    {
                        try
                        {
                            history.SavePanel(panelId, spMode);
                        }
                        catch (Exception ex)
                        {
                            machine.log.stack(
                                string.Format("+ Error al ejecutar history.SavePanel({0}, {1}) ", panelId, spMode
                            ), this, ex);
                        }
                    }
                }
                #endregion
            }

            // Es necesario volver a ejecutar el service, para obtener la OP asignada al panel
            if(spMode == "insert")
            {
                machine.LogBroadcast("debug", "Actualizando datos de panel insertado");
                panelService = GetBarcodeInfoFromIAServer();
            }

            sw.Stop();

            machine.LogBroadcast("success",
                string.Format("Panel guardado: {0} (ms) {1} ",
                barcode,
                (long)sw.ElapsedMilliseconds)
            );

            // Retorno ID, 0 si no pudo insertar, o actualizar
            return panelService;
        }

        private void SaveBlocks(string path)
        {
            foreach (Bloque bloque in bloqueList)
            {
                Stopwatch sw = Stopwatch.StartNew();

                if (Config.debugMode)
                {
                    machine.LogBroadcast("warning",
                        string.Format("Debug mode: ON, no se guarda el bloque")
                    );
                } else { 

                    machine.LogBroadcast("debug",
                        string.Format("Ejecutando sp_addInspectionBlock({0}) ", bloque.barcode)
                    );

                    string query = @"CALL sp_addInspectionBlock('" + panelId + "',  '" + bloque.barcode + "',  '" + bloque.tipoBarcode + "',  '" + bloque.revisionAoi + "',  '" + bloque.revisionIns + "',  '" + bloque.totalErrores + "',  '" + bloque.totalErroresFalsos + "',  '" + bloque.totalErroresReales + "',  '" + bloque.bloqueId + "' );";

                    #region GUARDA EN DB
                    MySqlConnector sql = new MySqlConnector();
                    sql.LoadConfig("IASERVER");
                    DataTable sp = sql.Query(query);
                    if (sql.rows)
                    {
                        int id_inspeccion_bloque = 0;
                        id_inspeccion_bloque = int.Parse(sp.Rows[0]["id"].ToString());

                        if (id_inspeccion_bloque > 0)
                        {
                            history.SaveBloque(id_inspeccion_bloque);
                        }

                        if (bloque.totalErrores > 0)
                        {
                            // EXISTEN ERRORES REALES O FALSOS
                            SaveDetail(id_inspeccion_bloque, bloque);
                        }
                    }
                    #endregion                    
                }

                sw.Stop();

                machine.LogBroadcast("success",
                    string.Format("Bloque guardado: {0} (ms) {1} ",
                    bloque.barcode,
                    (long)sw.ElapsedMilliseconds)
                );
            }
        }

        private void SaveDetail(int id_inspeccion_bloque, Bloque bloque)
        {
            foreach (Detail detail in bloque.detailList)
            {
                string query = @"CALL sp_addInspectionDetail('" + id_inspeccion_bloque + "',  '" + detail.referencia + "',  '" + detail.faultcode + "',  '" + detail.estado + "');";
                MySqlConnector sql = new MySqlConnector();
                sql.LoadConfig("IASERVER");
                DataTable sp = sql.Query(query);
            }

            // Una vez insertados los detalles de inspeccion del bloque, genero un historial 
            history.SaveDetalle(id_inspeccion_bloque);
        }
    }
}
