using System;
using System.Data;

using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.IAServer;
using System.Diagnostics;
using CollectorPackage.Aoicollector.Inspection.Model;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class InspectionController : Panel
    {
        /// <summary>
        /// La placa que llego a TrazaSave, no se encuentra pendiente de inspeccion.
        /// Antes de guardar los datos de inspeccion en IAServer es preciso verificar si la OP se encuentra ACTIVA, y si 
        /// no se completo la OP
        /// </summary>
        /// <param name="path"></param>
        public void TrazaSave(string path)
        {
            history = new History();

            // Solo proceso si la etiqueta es FISICA, las etiquetas virtuales no se aceptan mas
            if (tipoBarcode.Equals("E"))
            {
                machine.LogBroadcast("notify",
                   string.Format("+ Aoi: {0} | Inspector: {1} | Falsos: {3} | Reales: {4} | Pendiente: {2}",
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
                        string.Format("+ Agregando panel a estado pendiente, se verificara en la siguiente ronda")
                    );

                    Pendiente.Save(this);
                }
                else
                { 
                    machine.GetProductionInfoFromIAServer();
                    if (machine.prodService.error == null) // El stack lo muestro en el metodo GetProductionInfoFromIAServer
                    {
                        if (machine.prodService.result.error == null)
                        {
                            // Existe configuracion de produccion?
                            if (machine.prodService.result.produccion != null)
                            {
                                PanelHandlerService(path);
                            }
                            else
                            {
                                machine.LogBroadcast("warning",
                                    string.Format("+ Error al obtener datos de {0} produccion del service, se cancela la operacion",op)
                                );
                            }
                        } else
                        {
                            machine.LogBroadcast("error",
                                string.Format("+ Error al obtener resultados del service {0}", machine.prodService.result.error)
                            );
                        }
                    } 
                } 
            }
            else
            {
                machine.LogBroadcast("warning",
                    string.Format("+ Etiqueta virtual {0} en {1} | Maquina: {2}, se cancela la operacion", barcode, programa, machine.maquina)
                );
            }
        }

        private PanelService PanelHandlerService(string path)
        {
            Stopwatch sw = Stopwatch.StartNew();
            // Si la ruta declara, obtiene estado de declaracion del panel si es que existe en IAServer
            PanelService panelService = GetBarcodeInfoFromIAServer(machine.serviceDeclaraToBool());
            sw.Stop();

            machine.LogBroadcast("verbose",
                string.Format("+ API GetBarcodeInfo Tiempo de respuesta: (ms) {0} ",
                (long)sw.ElapsedMilliseconds)
            );

            // Solo proceso si el servicio respondio sin problemas
            if (panelService.error == null)
            {
                machine.LogBroadcast("debug",
                    string.Format("+ Panel Modo: {0}", spMode)
                );

                // Si la linea tiene configurada la Trazabilidad por Cogiscan, valida ruta
                if (machine.prodService.result.produccion.cogiscan.Equals("T"))
                {
                    machine.LogBroadcast("info",
                        string.Format("+ Validando ruta de cogiscan")
                    );

                    // Comentado hasta que repare el service
                    //if (panelService.result.cogiscan.product.attributes.operation.Equals("AOI"))
                    //{
                    //    SavePanel(panelService);
                    //}
                    //else
                    //{
                    //    machine.LogBroadcast("warning",
                    //        string.Format("+ El panel {0} no se encuentra en la ruta AOI", barcode)
                    //    );
                    //}
                } else
                {
                    SavePanel(panelService);
                }
            } else
            {
                machine.log.stack(
                    string.Format("+ PanelService exception"
                ), this, panelService.error);
            }


            if (panelId > 0 && panelService.error == null)
            {
                // Verifico que la OP del panel inspeccionado, corresponda a la OP configurada en produccion
                if (op == machine.prodService.result.produccion.op)
                {
                    SaveBlocks(path);
                }
                else
                {
                    machine.LogBroadcast("warning",
                        string.Format("+ El panel {0} esta registrado con {1}, es diferente de {2} en produccion",
                        barcode,
                        op,
                        machine.prodService.result.produccion.op)
                    );
                }
            }

            return panelService;
        }

        private PanelService SavePanel(PanelService panelService)
        {
            if (Config.debugMode)
            {
                machine.LogBroadcast("warning", "+ Debug mode ON, no ejecuta StoreProcedure para guardar panel");
            } else { 
                #region SAVE PANELS ON DB
                string query = @"CALL sp_setInspectionPanel_optimizando('" + panelId + "','" + machine.mysql_id + "',  '" + barcode + "',  '" + programa + "',  '" + fecha + "',  '" + hora + "',  '',  '" + revisionAoi + "',  '" + revisionIns + "',  '" + totalErrores + "',  '" + totalErroresFalsos + "',  '" + totalErroresReales + "',  '" + pcbInfo.bloques + "',  '" + tipoBarcode + "',  '" + Convert.ToInt32(pendiente) + "' ,  '" + machine.oracle_id + "' ,  '" + vtwinProgramNameId + "' ,  '" + spMode + "'  );";

                machine.LogBroadcast("debug", 
                    string.Format("+ Ejecutando StoreProcedure: sp_setInspectionPanel_optimizando({0}) =>", barcode)
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
                machine.LogBroadcast("debug", "+ Actualizando datos de panel insertado");
                panelService = GetBarcodeInfoFromIAServer();
            }
            
            // Retorno ID, 0 si no pudo insertar, o actualizar
            return panelService;
        }

        private void SaveBlocks(string path)
        {
            foreach (Bloque bloque in bloqueList)
            {
                if (Config.debugMode)
                {
                    machine.LogBroadcast("warning",
                        string.Format("+ Debug mode: ON, no se guarda el bloque")
                    );
                } else { 

                    machine.LogBroadcast("debug",
                        string.Format("+ Ejecutando sp_addInspectionBlock({0}) ", bloque.barcode)
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

                    if (machine.prodService.result.produccion.sfcs.declara.Equals("1"))
                    {
                        machine.LogBroadcast("notify",
                            string.Format("+ {0} declarar con {1}", 
                            bloque.barcode,
                            machine.prodService.result.produccion.op
                        ));
                        //Trazabilidad traza = new Trazabilidad();
                        //traza.declareIfNeeded(blockBarcode.barcode);
                    }
                }

                Export.toXML(this, bloque, path);
            }
        }

        private void SaveDetail(int id_inspeccion_bloque, Bloque bloque)
        {
            machine.LogBroadcast("verbose",
               string.Format("+ SaveDetail()")
            );

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
