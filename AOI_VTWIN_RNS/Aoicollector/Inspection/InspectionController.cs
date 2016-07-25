using System;
using System.Data;

using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.IAServer;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class InspectionController
    {
        public InspectionObject inspectionObj = new InspectionObject();

        /// <summary>
        /// Separo la inspeccion del panel por bloques y guardo la informacion de cada bloque por separado 
        /// </summary>
        /// <param name="path"></param>
        public void TrazaSave(string path)
        {
            InspectionService inspectionService = new InspectionService();

            inspectionObj.history = new History();

            #region GUARDA PANEL CON ETIQUETA FISICA
            if (inspectionObj.tipoPanelBarcode.Equals("E"))
            {
                inspectionService = SavePanel();
            }
            else
            {
                Log.Sys(inspectionObj.panelBarcode + " en " + inspectionObj.programa + " | " + inspectionObj.machine.maquina + " | tiene etiqueta virtual!!", "atencion");
            }
            #endregion

            // Proceso si el service se ejecuto, y tengo idPanel desde service, o desde un nuevo insert
            if (inspectionObj.idPanel > 0)
            {
                // Si ejecuto correctamente el service continuo
                if (inspectionService.hasResponse)
                {
                    inspectionObj.op = inspectionService.AssignedOp();

                    // Obtengo datos de produccion de aoi
                    ProductionService productionService = GetProductionInfoFromIAServer();

                    if (productionService.hasResponse)
                    {
                        if (inspectionObj.machine.op.Equals(string.Empty))
                        {
                            Log.Sys(inspectionObj.panelBarcode + " tiene " + inspectionObj.op + " | produccion no tiene OP", "atencion");
                        }
                        else
                        {
                            // Verifico que la OP del panel inspeccionado, corresponda a la OP configurada en produccion
                            // Ya que si difiere, los datos de PuestoId y LineId van a ser incorrectos en scfs 
                            if (inspectionObj.op == inspectionObj.machine.op)
                            {
                                SaveBlocks(path);
                            }
                            else
                            {
                                Log.Sys(inspectionObj.panelBarcode + " tiene " + inspectionObj.op + " | diferente de produccion " + inspectionObj.machine.op, "atencion");
                            }
                        }
                    }
                    else
                    {
                        Log.Sys(inspectionObj.panelBarcode + " no se pudo obtener datos de produccion | "+ inspectionObj.machine.line_barcode +" "+ inspectionObj.machine.op, "atencion");
                    }
                    
                }
                else
                {
                    Log.Sys(inspectionObj.panelBarcode + " en " + inspectionObj.programa + " | " + inspectionObj.machine.maquina + " | No se ejecuto el service de inspeccion correctamente", "atencion");
                }
            }
            else
            {
                Log.Sys(inspectionObj.panelBarcode + " en " + inspectionObj.programa + " | " + inspectionObj.machine.maquina + " | El panel no tiene ID", "atencion");
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
            if (inspectionObj.idPanel == 0)
            {
                spMode = "insert";
            } 
            #endregion

            // Solo proceso si el servicio respondio sin problemas
            #region INSERTAR O ACTUALIZA DATOS DE PANEL
            if (inspectionService.hasResponse)
            {              
                if (!Config.debugMode)
                {
                    #region SAVE PANELS ON DB
                    string query = @"CALL sp_setInspectionPanel_optimizando('" + inspectionObj.idPanel + "','" + inspectionObj.machine.mysql_id + "',  '" + inspectionObj.panelBarcode + "',  '" + inspectionObj.programa + "',  '" + inspectionObj.fecha + "',  '" + inspectionObj.hora + "',  '',  '" + inspectionObj.revisionAoi + "',  '" + inspectionObj.revisionIns + "',  '" + inspectionObj.totalErrores + "',  '" + inspectionObj.totalErroresFalsos + "',  '" + inspectionObj.totalErroresReales + "',  '" + inspectionObj.pcbInfo.bloques + "',  '" + inspectionObj.tipoPanelBarcode + "',  '" + Convert.ToInt32(inspectionObj.pendiente) + "' ,  '" + inspectionObj.machine.oracle_id + "' ,  '" + inspectionObj.vtwin_program_name_id + "' ,  '" + spMode + "'  );";

                    MySqlConnector sql = new MySqlConnector();
                    DataTable sp = sql.Select(query);
                    if (sql.rows)
                    {
                        // En caso de insert, informo el id_panel creado, si fue un update, seria el mismo id_panel...
                        inspectionObj.idPanel = int.Parse(sp.Rows[0]["id_panel"].ToString());
                        if (inspectionObj.pendiente)
                        {
                            Pendiente.Save(inspectionObj);
                        }

                        if (inspectionObj.pendienteDelete)
                        {
                            Pendiente.Delete(inspectionObj.idPanel);
                        }

                        // Solo cuando se inserta por primera vez, y no es un panel pendiente de inspeccion
                        if (inspectionObj.idPanel > 0 && !inspectionObj.pendiente)
                        {
                            try
                            {
                                inspectionObj.history.SavePanel(inspectionObj.idPanel, spMode);
                            }
                            catch (Exception ex)
                            {
                                Log.Sys("insp.history.panel(" + inspectionObj.idPanel + ", " + spMode + ") " + ex.Message, "error");
                            }
                        }
                    }
                    #endregion
                }

                // Es necesario volver a ejecutar el service, para obtener la OP asignada al panel
                if(spMode == "insert")
                {
                    inspectionService = GetBarcodeInfoFromIAServer();
                }
            }
            #endregion

            // Retorno ID, 0 si no pudo insertar, o actualizar
            return inspectionService;
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
                inspectionService.GetInspectionInfo(inspectionObj.panelBarcode);
                inspectionObj.idPanel = inspectionService.IdPanel();

                inspectionService.hasResponse = true;
            }
            catch (Exception ex)
            {
                Log.Sys("CATCH - GetBarcodeInfoFromIAServer(" + inspectionObj.panelBarcode + ") " + ex.Message, "error");
                Log.Stack(this, ex);
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
                productionService.GetAoiProduction(inspectionObj.machine.line_barcode);

                inspectionObj.machine.op = productionService.Op();
                inspectionObj.machine.opActive = productionService.Active();
                inspectionObj.machine.opDeclara = productionService.Declara();

                inspectionObj.puestoId = productionService.SfcsPuestoId();
                inspectionObj.lineId = productionService.SfcsLineId();

                productionService.hasResponse= true;
            }
            catch (Exception ex)
            {
                Log.Sys("CATCH - GetProductionInfoFromIAServer(" + inspectionObj.panelBarcode + ") " + ex.Message, "error");
                Log.Stack(this, ex);
            }

            return productionService;
        }

        /// <summary>
        /// Procesa cada bloque del panel
        /// </summary>
        /// <param name="path"></param>
        private void SaveBlocks(string path)
        {
            foreach (BlockBarcode blockBarcode in inspectionObj.blockBarcodeList)
            {
                #region GUARDA EN DB
                if (!inspectionObj.pendiente)
                {
                    if (!Config.debugMode)
                    {
                        string query = @"CALL sp_addInspectionBlock('" + inspectionObj.idPanel + "',  '" + blockBarcode.barcode + "',  '" + blockBarcode.tipoBarcode + "',  '" + blockBarcode.revision_aoi + "',  '" + blockBarcode.revision_ins + "',  '" + blockBarcode.total_errores + "',  '" + blockBarcode.total_errores_falsos + "',  '" + blockBarcode.total_errores_reales + "',  '" + blockBarcode.bloqueId + "' );";

                        MySqlConnector sql = new MySqlConnector();
                        DataTable sp = sql.Select(query);
                        if (sql.rows)
                        {
                            int id_inspeccion_bloque = 0;
                            id_inspeccion_bloque = int.Parse(sp.Rows[0]["id"].ToString());

                            if (id_inspeccion_bloque > 0)
                            {
                                inspectionObj.history.SaveBloque(id_inspeccion_bloque);
                            }

                            if (blockBarcode.total_errores > 0)
                            {
                                // EXISTEN ERRORES REALES O FALSOS
                                SaveDetail(id_inspeccion_bloque, blockBarcode);
                            }
                        }
                    }
                }
                #endregion

                Export.toXML(inspectionObj, blockBarcode, path);

                if (!inspectionObj.pendiente && inspectionObj.machine.opDeclara)
                {
                    //Trazabilidad traza = new Trazabilidad();
                    //traza.declareIfNeeded(blockBarcode.barcode);
                }
            }
        }

        /// <summary>
        /// Guarda los detalles de la inspeccion por cada bloque
        /// </summary>
        private void SaveDetail(int id_inspeccion_bloque, BlockBarcode blockBarcode)
        {
            foreach (Detail detail in blockBarcode.detailList)
            {
                string query = @"CALL sp_addInspectionDetail('" + id_inspeccion_bloque + "',  '" + detail.referencia + "',  '" + detail.faultcode + "',  '" + detail.estado + "');";
                MySqlConnector sql = new MySqlConnector();
                DataTable sp = sql.Select(query);
            }

            // Una vez insertados los detalles de inspeccion del bloque, genero un historial 
            inspectionObj.history.SaveDetalle(id_inspeccion_bloque);
        }

        

    }
}
