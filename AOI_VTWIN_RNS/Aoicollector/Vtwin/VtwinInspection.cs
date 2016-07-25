using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vtwin.Controller;
using System.Data;
using System.Text.RegularExpressions;
using AOI_VTWIN_RNS.Src.Util.Crypt;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Vtwin
{
    public class VtwinInspection : AoiController
    {
        public void HandleInspection(Machine inspMachine)
        {
            DateTime last_oracle_inspection = new DateTime(1, 1, 1);
            aoiLog.Area("Inspeccionando SMD-" + inspMachine.linea + " | maquina: " + inspMachine.maquina + " | Ultima inspeccion: " + inspMachine.ultima_inspeccion);

            string query = OracleQuery.ListLastInspections(inspMachine.oracle_id, inspMachine.ultima_inspeccion);
            DataTable dt = oracle.Query(query);
            int totalRows = dt.Rows.Count;
            ProgressTotal(totalRows);

            if (totalRows > 0)
            {
                aoiLog.Area("+ Se encontraron (" + totalRows + ") inspecciones.");
                int count = 0;

                #region CREATE_INSPECTION_OBJECT
                foreach (DataRow r in dt.Rows)              
                {
                    // Reinicio inspectionObject
                    ResetInspection();

                    count++;
                    CreateInspectionObject(r, inspMachine);

                    aoiLog.Area("+ Programa: [" + inspectionObj.programa + "] - Barcode: " + inspectionObj.panelBarcode);

                    TrazaSave(aoiConfig.xmlExportPath);
                    ProgressInc(count);

                    // Ultima inspeccion realizada en la maquina ORACLE.
                   last_oracle_inspection = DateTime.Parse(inspectionObj.fecha + " " + inspectionObj.hora);
                }

                if (last_oracle_inspection.Year.Equals(1))
                {
                    last_oracle_inspection = oracle.GetSysDate();
                }
                #endregion

                aoiLog.Area("+ Actualizando fecha de ultima inspeccion en maquina: "+ last_oracle_inspection.ToString("HH:mm:ss"));
                if (!Config.debugMode)
                {
                    Machine.UpdateInspectionDate(inspMachine.mysql_id, last_oracle_inspection);
                }
            }
            else
            {
                aoiLog.Area("+ No se encontraron inspecciones.");
            }

            aoiLog.Area("");
            Machine.Ping(inspMachine.mysql_id);
        }

        public void HandlePendientInspection()
        {
            // Descargo lista de pendientes VTWIN
            List<Pendiente> pendList = Pendiente.Download(aoiConfig.machineNameKey);
            aoiLog.Area("Verificando inspecciones pendientes. Total: " + pendList.Count);

            if (pendList.Count > 0)
            {
                int count = 0;
                ProgressTotal(pendList.Count);

                foreach (Pendiente pend in pendList)
                {
                    // Reinicio inspectionObject
                    ResetInspection();

                    // Busco ultimo estado del barcode en ORACLE.
                    string query = OracleQuery.ListLastInspections(0, "", pend);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        count++;

                        DataRow oracleLastRow = dt.Rows[totalRows - 1];
                        int oracle_id = int.Parse(oracleLastRow["TEST_MACHINE_ID"].ToString());
                        Machine inspMachine = Machine.list.Single(obj => obj.tipo == aoiConfig.machineNameKey && obj.oracle_id == oracle_id);

                        if (Config.isByPassMode(inspMachine.linea))
                        {
                            // SKIP MACHINE
                            aoiLog.Area("+ Maquina en ByPass: " + inspMachine.linea + " PendientMode: STOP");
                        }
                        else
                        {
                            CreateInspectionObject(oracleLastRow, inspMachine);

                            if (inspectionObj.pendiente)
                            {
                                // Aun sigue pendiente... no hago nada...
                            }
                            else
                            {
                                // No esta mas pendiente!!, se realizo la inspeccion!! Guardo datos.
                                aoiLog.Area("Inspeccion detectada! Barcode: " + inspectionObj.panelBarcode);
                                inspectionObj.pendienteDelete = true;

                                TrazaSave(aoiConfig.xmlExportPath);
                            }

                            ProgressInc(count);
                        }
                    }
                    else
                    {
                        aoiLog.Area("+ Pendiente sin cambios [" + pend.programa + "] Barcode: " + pend.barcode);
                    }
                }
            }
        }

        /// <summary>
        /// Completa los datos del panel inspeccionado
        /// </summary>
        /// <param name="r"></param>
        /// <param name="inspMachine"></param>
        public void CreateInspectionObject(DataRow r, Machine inspMachine)
        {
            inspectionObj.programa = r["programa"].ToString();
            inspectionObj.fecha = r["aoi_fecha"].ToString();
            inspectionObj.hora = r["aoi_hora"].ToString();
            inspectionObj.inspFecha = r["insp_fecha"].ToString();
            inspectionObj.inspHora = r["insp_hora"].ToString();

            // Si no tengo fecha de inspeccion, el panel se encuentra pendiente de inspeccion.
            if (inspectionObj.inspFecha.Equals(""))
            {
                inspectionObj.pendiente = true;
            }

            inspectionObj.machine = inspMachine;
            inspectionObj.maquina = inspMachine.maquina;
            inspectionObj.panelBarcode = r["barcode"].ToString();

            BlockBarcode.validateBarcode(inspectionObj);

            inspectionObj.panelNro = int.Parse(r["pcb_no"].ToString());
            inspectionObj.revisionIns = "NG";
            inspectionObj.revisionAoi = r["test_result"].ToString();

            // Si AOI no tiene errores las placas estan bien. 
            if (inspectionObj.revisionAoi.Equals(""))
            {
                inspectionObj.revisionAoi = "OK";
                inspectionObj.pendiente = false;
            }
            //            insp.revision_ins = r["revise_result"].ToString();

            // Informacion especifica para maquinas tipo vtwin
            inspectionObj.vtwin_program_name_id = int.Parse(r["program_name_id"].ToString());
            inspectionObj.vtwin_save_machine_id = int.Parse(r["saved_machine_id"].ToString());
            inspectionObj.vtwin_revision_no = int.Parse(r["revision_no"].ToString());
            inspectionObj.vtwin_serial_no = int.Parse(r["serial_no"].ToString());
            inspectionObj.vtwin_load_count = int.Parse(r["load_count"].ToString());

            //            inspectionObj.inspection_machine = inspMachine;

            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
            PcbInfo pcb_info = PcbInfo.list.Find(obj => obj.nombre.Equals(inspectionObj.programa) && obj.tipoMaquina.Equals(aoiConfig.machineNameKey));
            if (pcb_info != null)
            {
                inspectionObj.pcbInfo = pcb_info;
            }

            // Obtiene detalle de errores del panel completo 
            inspectionObj.detailList = GetInspectionDetail();

            // Lista de BLOCK_ID de ORACLE, adjunta Barcodes de cada bloque
            // En caso de tener varios bloques, y una sola etiqueta, genera etiquetas virtuales para el resto de los bloques
            inspectionObj.blockBarcodeList = GetOracleBlockBarcodes(inspectionObj.panelBarcode);

            inspectionObj.ProccessPanelStatus();
        }

        /// <summary>
        /// Obtiene los detalles de la inspeccion
        /// </summary>
        /// <param name="i"></param>
        /// <returns>List<InspectionDetail></returns>
        private List<Detail> GetInspectionDetail()
        {
            string query = OracleQuery.ListFaultInfo(inspectionObj);

            DataTable dt = oracle.Query(query);

            List<Detail> ldet = new List<Detail>();
            if (dt.Rows.Count > 0)
            {
                #region FILL_ERROR_DETAIL
                foreach (DataRow r in dt.Rows)
                {
                    int bid = int.Parse(r["bloque"].ToString());
                    Detail det = new Detail();
                    det.faultcode = r["fault_code"].ToString();
                    det.estado = r["resultado"].ToString();
                    det.referencia = r["COMPONENT_NAME"].ToString();
                    det.bloqueId = bid;
                    //det.total_faultcode = int.Parse(r["total"].ToString());
                    det.descripcionFaultcode = Faultcode.Description(det.faultcode);

                    ldet.Add(det);
                }
                #endregion
            }
            return ldet;
        }

        /// <summary>
        /// Obtiene los bloques del panel inspeccionado 
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private List<BlockBarcode> GetOracleBlockBarcodes(string codigo)
        {
            List<BlockBarcode> list = new List<BlockBarcode>();

            if (inspectionObj.pcbInfo.bloques == 1)
            {
                //BlockBarcode b = new BlockBarcode();
                //b.bloqueId = 1;
                //b.barcode = inspectionObj.panelBarcode;
                //b.validateBarcode();
                //list.Add(b);

                string blockId = "1"; 
                List<int> posibleBlockId = inspectionObj.detailList.Select(o => o.bloqueId).Distinct().ToList();

                if (posibleBlockId.Count > 0)
                {
                    blockId = posibleBlockId.First().ToString();
                }

                BlockBarcode b = new BlockBarcode();
                b.bloqueId = int.Parse(blockId);
                b.barcode = inspectionObj.panelBarcode; ;
                b.validateBarcode();
                list.Add(b);
            }
            else
            {
                if (inspectionObj.pcbInfo.bloques > 1)
                {
                    string query = OracleQuery.ListBlockBarcode(codigo);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        #region CREATE_BLOCKBARCODE_OBJECT
                        foreach (DataRow r in dt.Rows)
                        {
                            BlockBarcode b = new BlockBarcode();
                            b.bloqueId = int.Parse(r["bloque"].ToString());
                            b.barcode = r["block_barcode"].ToString();
                            b.validateBarcode();
                            list.Add(b);
                        }
                        #endregion
                    }
                }
            }

            // Encontre barcodes con etiqueta en los bloques?!
            // Si no encontre... genero bloques virtuales 
            if (list.Count == 0)
            {
                #region CREATE_BLOCKBARCODE_OBJECT
                for (int i = 1; i <= inspectionObj.pcbInfo.bloques; i++)
                {
                    BlockBarcode b = new BlockBarcode();
                    b.bloqueId = i;
                    b.barcode = inspectionObj.panelBarcode + "-" + i;
                    b.validateBarcode();
                    list.Add(b);
                }
                #endregion
            }

            return list;
        }

        
    }
}