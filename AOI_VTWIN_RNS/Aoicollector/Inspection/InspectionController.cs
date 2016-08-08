using System;
using System.Data;

using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.IAServer;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class InspectionController : Panel
    {
        public void TrazaSave(string path)
        {
            history = new History();
            InspectionService inspectionService = new InspectionService();

            #region GUARDA PANEL CON ETIQUETA FISICA
            if (tipoBarcode.Equals("E"))
            {
                inspectionService = SavePanel();
            }
            else
            {
                machine.LogBroadcast("warning", 
                    string.Format("+ Etiqueta virtual {0} en {1} | Maquina: {2}", barcode, programa, machine.maquina)
                );
            }
            #endregion

            // Proceso si el service se ejecuto, y tengo idPanel desde service, o desde un nuevo insert
            if (panelId > 0)
            {
                // Si ejecuto correctamente el service continuo
                if (inspectionService.hasResponse)
                {
                    op = inspectionService.AssignedOp();

                    machine.LogBroadcast("info", 
                        string.Format("+ OP Asignada {0}", op)
                    );

                    // Obtengo datos de produccion de aoi
                    ProductionService productionService = GetProductionInfoFromIAServer();

                    if (productionService.hasResponse)
                    {
                        if (machine.op.Equals(string.Empty))
                        {
                            machine.LogBroadcast("warning",
                                string.Format("+ El panel {0} esta registrado con {1}, pero la produccion no definio OP ", barcode, op)
                            );
                        }
                        else
                        {
                            // Verifico que la OP del panel inspeccionado, corresponda a la OP configurada en produccion
                            // Ya que si difiere, los datos de PuestoId y LineId van a ser incorrectos en scfs 
                            if (op == machine.op)
                            {
                                SaveBlocks(path);
                            }
                            else
                            {
                                machine.LogBroadcast("warning",
                                    string.Format("+ El panel {0} esta registrado con {1}, es diferente de {1} en produccion", barcode, op, machine.op)
                                );
                            }
                        }
                    }
                    else
                    {
                        machine.LogBroadcast("warning", 
                            string.Format("+ No se pudo obtener datos de produccion de {0} ", barcode)
                        );
                    }
                }
                else
                {
                    Log.system.warning(barcode + " en " + programa + " | " + machine.maquina + " | No se ejecuto el service de inspeccion correctamente");
                }
            }
            else
            {
                Log.system.warning(barcode + " en " + programa + " | " + machine.maquina + " | El panel no tiene ID");
            }
        }


        /// <summary>
        /// Inserta o actualiza el panel en la base de datos
        /// </summary>
        /// <param name="insp"></param>
        private InspectionService SavePanel()
        {
            // Por defecto se realiza un update y no se encuentra pendiente
            string spMode = "update";

            // Verifica si existe el barcode en paneles o en bloques. en tal caso retorna ID PANEL
            InspectionService inspectionService = GetBarcodeInfoFromIAServer();

            #region ESTABLECE MODO INSERT, SI EL PANEL NO EXISTE, Y DEFINE SI SE ENCUENTRA EN MODO PENDIENTE
            if (panelId == 0)
            {
                spMode = "insert";
            } 
            #endregion

            // Solo proceso si el servicio respondio sin problemas
            #region INSERTAR O ACTUALIZA DATOS DE PANEL
            if (inspectionService.hasResponse)
            {
                machine.LogBroadcast("debug", 
                    string.Format("+ Modo: {0}", spMode)
                );

                if (Config.debugMode)
                {
                    machine.LogBroadcast("warning", "+ Debug mode ON, no ejecuta StoreProcedure para guardar panel");
                } else { 
                    #region SAVE PANELS ON DB
                    string query = @"CALL sp_setInspectionPanel_optimizando('" + panelId + "','" + machine.mysql_id + "',  '" + barcode + "',  '" + programa + "',  '" + fecha + "',  '" + hora + "',  '',  '" + revisionAoi + "',  '" + revisionIns + "',  '" + totalErrores + "',  '" + totalErroresFalsos + "',  '" + totalErroresReales + "',  '" + pcbInfo.bloques + "',  '" + tipoBarcode + "',  '" + Convert.ToInt32(pendiente) + "' ,  '" + machine.oracle_id + "' ,  '" + vtwinProgramNameId + "' ,  '" + spMode + "'  );";

                    machine.LogBroadcast("debug", 
                        string.Format("+ Ejecutando StoreProcedure: sp_setInspectionPanel_optimizando() =>", barcode)
                    );

                    MySqlConnector sql = new MySqlConnector();
                    DataTable sp = sql.Select(query);
                    if (sql.rows)
                    {
                        // En caso de insert, informo el id_panel creado, si fue un update, seria el mismo id_panel...
                        panelId= int.Parse(sp.Rows[0]["id_panel"].ToString());
                        if (pendiente)
                        {                           
                            Pendiente.Save(this);
                        }

                        if (pendienteDelete)
                        {
                            Pendiente.Delete(this);
                        }

                        // Solo cuando se inserta por primera vez, y no es un panel pendiente de inspeccion
                        if (panelId > 0 && !pendiente)
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
                    machine.LogBroadcast("debug", "+ Actualizando datos desde service");
                    inspectionService = GetBarcodeInfoFromIAServer();
                }
            } else
            {
                machine.LogBroadcast("error", 
                    string.Format("+ Service Response: ERROR | Modo: {0}", spMode)
                );
            }
            #endregion

            // Retorno ID, 0 si no pudo insertar, o actualizar
            return inspectionService;
        }

        /// <summary>
        /// Procesa cada bloque del panel
        /// </summary>
        /// <param name="path"></param>
        private void SaveBlocks(string path)
        {
            machine.LogBroadcast("info",
               string.Format("+ Se detectaron: {0} bloques en el panel {1}", bloqueList.Count, barcode)
            );
            foreach (Bloque bloque in bloqueList)
            {
                if (pendiente)
                {
                    machine.LogBroadcast("notify",
                        string.Format("+ La inspeccion se encuentra pendiente ({0}), no se guarda el bloque ", bloque.barcode)
                    );
                } else {
                    if (Config.debugMode)
                    {
                        machine.LogBroadcast("warning",
                           string.Format("+ Debug mode: ON, no se guarda el bloque")
                       );
                    } else { 

                        machine.LogBroadcast("debug",
                            string.Format("+ sp_addInspectionBlock({0}) ", bloque.barcode)
                        );

                        string query = @"CALL sp_addInspectionBlock('" + panelId + "',  '" + bloque.barcode + "',  '" + bloque.tipoBarcode + "',  '" + bloque.revisionAoi + "',  '" + bloque.revisionIns + "',  '" + bloque.totalErrores + "',  '" + bloque.totalErroresFalsos + "',  '" + bloque.totalErroresReales + "',  '" + bloque.bloqueId + "' );";

                        #region GUARDA EN DB
                        MySqlConnector sql = new MySqlConnector();
                        DataTable sp = sql.Select(query);
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

                        if (machine.opDeclara)
                        {
                            machine.LogBroadcast("notify",
                                string.Format("+ {0} declarar con {1}", bloque.barcode,machine.op)
                            );
                            //Trazabilidad traza = new Trazabilidad();
                            //traza.declareIfNeeded(blockBarcode.barcode);
                        }

                        //Export.toXML(inspectionObj, blockBarcode, path);
                    }
                } 
                
                
            }
        }

        /// <summary>
        /// Guarda los detalles de la inspeccion por cada bloque
        /// </summary>
        private void SaveDetail(int id_inspeccion_bloque, Bloque bloque)
        {
            machine.LogBroadcast("verbose",
               string.Format("+ SaveDetail()")
            );

            foreach (Detail detail in bloque.detailList)
            {
                string query = @"CALL sp_addInspectionDetail('" + id_inspeccion_bloque + "',  '" + detail.referencia + "',  '" + detail.faultcode + "',  '" + detail.estado + "');";
                MySqlConnector sql = new MySqlConnector();
                DataTable sp = sql.Select(query);
            }

            // Una vez insertados los detalles de inspeccion del bloque, genero un historial 
            history.SaveDetalle(id_inspeccion_bloque);
        }

        /// <summary>
        /// Obtiene datos de panel desde el webservice
        /// </summary>
        /// <returns>InspectionService</returns>
        private InspectionService GetBarcodeInfoFromIAServer()
        {
            InspectionService inspectionService = new InspectionService();

            try
            {
                inspectionService.GetInspectionInfo(barcode);
                panelId = inspectionService.IdPanel();

                inspectionService.hasResponse = true;

                machine.LogBroadcast("debug",
                    string.Format("+ GetBarcodeInfoFromIAServer({0}) => OK ", barcode)
                );
            }
            catch (Exception ex)
            {
                machine.log.stack(
                    string.Format("+ GetBarcodeInfoFromIAServer({0}) ", barcode
                ), this, ex);
            }

            return inspectionService;
        }

        /// <summary>
        /// Obtiene datos de produccion de aoi desde el webservice
        /// </summary>
        /// <returns></returns>
        private ProductionService GetProductionInfoFromIAServer()
        {
            ProductionService productionService = new ProductionService();
            try
            {
                productionService.GetAoiProduction(machine.line_barcode);

                machine.op = productionService.Op();
                machine.opActive = productionService.Active();
                machine.opDeclara = productionService.Declara();

                puestoId = productionService.SfcsPuestoId();
                lineId = productionService.SfcsLineId();

                productionService.hasResponse = true;

                machine.LogBroadcast("debug",
                    string.Format("+ GetProductionInfoFromIAServer({0}) => OK", barcode)
                );
            }
            catch (Exception ex)
            {
                machine.log.stack(
                    string.Format("+ GetProductionInfoFromIAServer({0})", barcode
                ), this, ex);
            }

            return productionService;
        }
    }
}
