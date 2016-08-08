using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vts500.Controller;
using System.Data;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection;

namespace AOI_VTWIN_RNS.Aoicollector.Vts500
{
    public class Vts500Inspection : AoiController
    {
        /*
        public void HandleInspection(Machine inspMachine)
        {
            // Dummy date
            DateTime last_oracle_inspection = new DateTime(1, 1, 1);
            aoiLog.info("Inspeccionando SMD-" + inspMachine.linea);
            aoiLog.log("+ Maquina: " + inspMachine.maquina);
            aoiLog.log("+ Ultima inspeccion: " + inspMachine.ultima_inspeccion);

            string query = OracleQuery.ListLastInspections(inspMachine);
            DataTable dt = oracle.Query(query);
            int totalRows = dt.Rows.Count;
            ProgressTotal(totalRows);

            if (totalRows > 0)
            {
                aoiLog.info("Se encontraron (" + totalRows + ") inspecciones.");
                int count = 0;

                #region CREATE_INSPECTION_OBJECT
                foreach (DataRow r in dt.Rows)
                {
                    count++;
                    CreateInspectionObject(r, inspMachine);
                    aoiLog.info("Programa: [" + inspectionObj.programa + "] - Barcode: " + inspectionObj.panelBarcode);

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

                aoiLog.log("Actualizando horario de ultima inspeccion: " + last_oracle_inspection.ToString("HH:mm:ss"));
                if (!Config.debugMode)
                {
                    Machine.UpdateInspectionDate(inspMachine.mysql_id, last_oracle_inspection);
                }
            }
            else
            {
                aoiLog.log("No se encontraron inspecciones.");
            }

            Machine.Ping(inspMachine.mysql_id);
        }

        public void HandlePendientInspection()
        {
            List<Pendiente> pendList = Pendiente.Download(aoiConfig.machineNameKey);
            Machine inspMachine = Machine.list.Single(obj => obj.tipo == aoiConfig.machineNameKey);

            aoiLog.info("Verificando inspecciones pendientes. Total: " + pendList.Count);

            if (pendList.Count > 0)
            {
                int count = 0;
                ProgressTotal(pendList.Count);

                foreach (Pendiente pend in pendList)
                {
                    ResetInspection();
                    // Busco ultimo estado del barcode en ORACLE.
                    string query = OracleQuery.ListLastInspections(inspMachine, pend);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        count++;

                        DataRow oracleLastRow = dt.Rows[totalRows - 1];

                        if (Config.isByPassMode(inspMachine.linea))
                        {
                            // SKIP MACHINE
                            aoiLog.warning("Maquina en ByPass: " + inspMachine.linea + ", no se analiza modo pendiente");
                        }
                        else
                        {
                            CreateInspectionObject(oracleLastRow, inspMachine);

                            if (inspectionObj.pendiente)
                            {
                                // Aun sigue pendiente... no hago nada...
                                aoiLog.log("Inspeccion pendiente: " + inspectionObj.panelBarcode);
                            }
                            else
                            {
                                // No esta mas pendiente!!, se realizo la inspeccion!! Guardo datos.
                                aoiLog.log("Inspeccion realizada! Barcode: " + inspectionObj.panelBarcode);
                                inspectionObj.pendienteDelete = true;

                                TrazaSave(aoiConfig.xmlExportPath);
                            }

                            ProgressInc(count);
                        }
                    }
                    else
                    {
                        aoiLog.log("Pendiente sin cambios [" + pend.programa + "] Barcode: " + pend.barcode);
                    }
                }
            }
        }

        /// <summary>
        /// Creo un objeto de inspeccion
        /// </summary>
        /// <param name="r"></param>
        /// <param name="inspMachine"></param>
        /// <returns>Inspection</returns>
        private void CreateInspectionObject(DataRow r, Machine inspMachine)
        {
            inspectionObj.programa = r["programa"].ToString();
            inspectionObj.fecha = r["aoi_fecha"].ToString();
            inspectionObj.hora = r["aoi_hora"].ToString();
            inspectionObj.inspFecha = r["insp_fecha"].ToString();
            inspectionObj.inspHora = r["insp_hora"].ToString();

            bool revised = Convert.ToBoolean(r["revised"]);

            // Si no fue revisado, se encuentra pendiente...
            if (!revised)
            {
                inspectionObj.pendiente = true;
            }

            inspectionObj.machine = inspMachine;
            inspectionObj.maquina = inspMachine.maquina;
            inspectionObj.panelBarcode = r["barcode"].ToString().Trim();

            BlockBarcode.validateBarcode(inspectionObj);

            inspectionObj.revisionIns = "NG";
            inspectionObj.revisionAoi = r["test_result"].ToString();
            inspectionObj.vts500_oracle_insp_id = int.Parse(r["insp_id"].ToString());
            inspectionObj.vts500_oracle_pg_item_id = int.Parse(r["PROGRAMA_ID"].ToString());

            inspectionObj.pcbInfo = CreatePCBInfo();

            inspectionObj.detailList = GetInspectionDetail();
            
            // Lista de BLOCK_ID de ORACLE, adjunta Barcodes de cada bloque
            if (inspectionObj.pcbInfo.bloques > 0)
            {
                inspectionObj.blockBarcodeList = GetOracleBlockBarcodes();

                var segmentos = (from BlockBarcode segid in inspectionObj.blockBarcodeList select segid.bloqueId).Distinct();
                inspectionObj.pcbInfo.segmentos = String.Join(",", segmentos);
            }

            inspectionObj.ProccessPanelStatus();
        }

        /// <summary>
        /// Creo un objeto PCB Virtual, al contrario de VTWIN y RNS en donde hay que obtener los datos del archivo .PCB
        /// </summary>
        /// <param name="insp"></param>
        /// <returns></returns>
        private PcbInfo CreatePCBInfo()
        {
            string query = OracleQuery.ListBlocks(inspectionObj);
            DataTable dt = oracle.Query(query);
            int totalRows = dt.Rows.Count;

            int bloques = (from DataRow r in dt.Rows select int.Parse(r["seg_no"].ToString())).Distinct().Count();
            //var segmentos = (from DataRow r in dt.Rows select int.Parse(r["seg_id"].ToString())).Distinct();

            PcbInfo pcb = new PcbInfo();
            pcb.bloques = bloques;
            pcb.nombre = inspectionObj.programa;
            pcb.programa = inspectionObj.programa;
            pcb.id = inspectionObj.vts500_oracle_pg_item_id;
            pcb.tipoMaquina = aoiConfig.machineNameKey;
            //pcb.segmentos = String.Join(",",segmentos);
            return pcb;
        }

        /// <summary>
        /// Obtiene lista de lectura de etiquetas fisicas
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns> List<InspectionBlockBarcode> </returns>
        private List<BlockBarcode> GetOracleBlockBarcodes()
        {
            List<BlockBarcode> list = new List<BlockBarcode>();

            if (inspectionObj.pcbInfo.bloques == 1)
            {
                BlockBarcode b = new BlockBarcode();
                b.bloqueId = 1;
                b.barcode = inspectionObj.panelBarcode;
                b.validateBarcode();
                list.Add(b);
            }
            else
            {
                if (inspectionObj.pcbInfo.bloques > 1)
                {
                    string query = OracleQuery.ListBlockBarcode(inspectionObj);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        #region CREATE_BLOCKBARCODE_OBJECT
                        foreach (DataRow r in dt.Rows)
                        {
                            BlockBarcode b = new BlockBarcode();
                            b.bloqueId = int.Parse(r["SEG_ID"].ToString());
                            b.barcode = r["SEG_BARCODE"].ToString().Trim();
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
                    int bid = int.Parse(r["SEG_ID"].ToString());
                    Detail det = new Detail();

                    det.faultcode = r["faultcode"].ToString();
                    det.estado = r["resultado"].ToString();
                    det.referencia = r["referencia"].ToString();
                    det.bloqueId = bid;
                    //det.total_faultcode = int.Parse(r["total"].ToString());
                    det.descripcionFaultcode = Faultcode.Description(det.faultcode);

                    ldet.Add(det);
                }
                #endregion
            }
            return ldet;
        }
        */
    }
}
