using System;
using System.Data;

using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using System.Threading.Tasks;
using AOI_VTWIN_RNS.Aoicollector.IAServer;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
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
            PanelService panelService = new PanelService();

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

                if(!pendiente)
                {
                    // Obtiene datos de produccion de la linea
                    machine.GetProductionInfoFromIAServer();
                    if(machine.service.exception == null)
                    {
                        // Solo si la OP se encuentra activa, procedo
                        if (machine.service.result.produccion.wip.active)
                        {
                            // Verifico que la OP tenga placas restantes
                            panelService = PanelHandlerService();

                            if (panelId > 0 && panelService.exception == null && machine.service.exception == null)
                            {
                               
                                #region SAVE BLOCKS
                                // Verifico que la OP del panel inspeccionado, corresponda a la OP configurada en produccion
                                if (op == machine.service.result.produccion.op)
                                {
                                    SaveBlocks(path);
                                }
                                else
                                {
                                    machine.LogBroadcast("warning",
                                        string.Format("+ El panel {0} esta registrado con {1}, es diferente de {2} en produccion",
                                        barcode,
                                        op,
                                        machine.service.result.produccion.op)
                                    );
                                }
                                #endregion
                            }
                        } else
                        {
                            machine.LogBroadcast("warning",
                                string.Format("+ La {0} definida en produccion, no se encuentra activa!, se cancela la operacion", op)
                            );
                        }
                    }                    
                } else
                {
                    machine.LogBroadcast("warning",
                        string.Format("+ El panel {0} se encuentra en estado PENDIENTE de inspeccion, se cancela la operacion", barcode)
                    );
                }
            } else
            {
                machine.LogBroadcast("warning", 
                    string.Format("+ Etiqueta virtual {0} en {1} | Maquina: {2}, se cancela la operacion", barcode, programa, machine.maquina)
                );
            }
        }

        private PanelService PanelHandlerService()
        {
            PanelService panelService = GetBarcodeInfoFromIAServer();

            return panelService;
        }

        private PanelService SavePanel()
        {
            // Verifica si existe el barcode IAServer y retorna ID PANEL
            PanelService panelService = GetBarcodeInfoFromIAServer();

            // Solo proceso si el servicio respondio sin problemas
            #region INSERTAR O ACTUALIZA DATOS DE PANEL
            if (panelService.exception == null)
            {
                if(machine.service.result.produccion.cogiscan.Equals("T"))
                {
                    machine.LogBroadcast("info",
                        string.Format("+ Validando ruta de cogiscan")
                    );

                    if (panelService.result.cogiscan.product.attributes.operation.Equals("AOI"))
                    {

                    } else
                    {
                        machine.LogBroadcast("warning",
                            string.Format("+ El panel {0} no se encuentra en la ruta AOI", barcode)
                        );
                    }
                }

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
                    panelService = GetBarcodeInfoFromIAServer();
                }
            } else
            {
                machine.LogBroadcast("error", 
                    string.Format("+ Service Response: ERROR | Modo: {0}", spMode)
                );
            }
            #endregion

            // Retorno ID, 0 si no pudo insertar, o actualizar
            return panelService;
        }

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

                        if (machine.service.result.produccion.declara.Equals("1"))
                        {
                            machine.LogBroadcast("notify",
                                string.Format("+ {0} declarar con {1}", 
                                bloque.barcode,
                                machine.service.result.produccion.op
                            ));
                            //Trazabilidad traza = new Trazabilidad();
                            //traza.declareIfNeeded(blockBarcode.barcode);
                        }
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
                DataTable sp = sql.Select(query);
            }

            // Una vez insertados los detalles de inspeccion del bloque, genero un historial 
            history.SaveDetalle(id_inspeccion_bloque);
        }

    }
}
