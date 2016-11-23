using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using System.Data;
using CollectorPackage.Aoicollector.Inspection;
using CollectorPackage.Src.Database;
using System;
using CollectorPackage.Aoicollector.Vts500.Controller;

namespace CollectorPackage.Aoicollector.Vts500
{
    public class Vts500Panel : InspectionController
    {
        public string machineNameKey = "V";
        private OracleConnector _oracle;

        public Vts500Panel(OracleConnector oracle, DataRow r, Machine inspMachine)
        {
            _oracle = oracle;
            CreateInspectionObject(r, inspMachine);
        }

        private void CreateInspectionObject(DataRow r, Machine inspMachine)
        {
            programa = r["programa"].ToString();
            fecha = r["aoi_fecha"].ToString();
            hora = r["aoi_hora"].ToString();
            inspFecha = r["insp_fecha"].ToString();
            inspHora = r["insp_hora"].ToString();

            bool revised = Convert.ToBoolean(r["revised"]);
            if (!revised)
            {
                pendiente = true;
            }

            machine = inspMachine;
            maquina = inspMachine.maquina;
            barcode = r["barcode"].ToString();

            BarcodeValidate();

            vtsOracleInspId = int.Parse(r["insp_id"].ToString());
            vtsOraclePgItemId = int.Parse(r["PROGRAMA_ID"].ToString());

            revisionIns = "NG";
            revisionAoi = r["test_result"].ToString();

            // Si AOI no tiene errores las placas estan bien. 
            if (revisionAoi.Equals(""))
            {
                revisionAoi = "OK";
                pendiente = false;
            }

            pcbInfo = CreatePCBInfo();

            // Obtiene detalle de errores del panel completo 
            detailList = GetInspectionDetail();

            // Lista de BLOCK_ID de ORACLE, adjunta Barcodes de cada bloque
            // En caso de tener varios bloques, y una sola etiqueta, genera etiquetas virtuales para el resto de los bloques
            bloqueList = GetBloquesFromOracle();

            MakeRevisionToAll();
        }

        private PcbInfo CreatePCBInfo()
        {
            string query = OracleQuery.ListBlocks(this);
            DataTable dt = _oracle.Query(query);
            int totalRows = dt.Rows.Count;

            int bloques = (from DataRow r in dt.Rows select int.Parse(r["seg_no"].ToString())).Distinct().Count();
            //var segmentos = (from DataRow r in dt.Rows select int.Parse(r["seg_id"].ToString())).Distinct();

            PcbInfo pcb = new PcbInfo();
            pcb.bloques = bloques;
            pcb.nombre = programa;
            pcb.programa = programa;
            pcb.id = vtsOraclePgItemId;
            pcb.tipoMaquina = machineNameKey;
            return pcb;
        }

        /// <summary>
        /// Detalle de inspeccion del panel general
        /// </summary>
        /// <returns></returns>
        private List<Detail> GetInspectionDetail()
        {
            string query = OracleQuery.ListFaultInfo(this);

            DataTable dt = _oracle.Query(query);

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

        /// <summary>
        /// Obtiene bloqueId, y barcode, no procesa informacion de inspeccion
        /// </summary>
        /// <returns></returns>
        private List<Bloque> GetBloquesFromOracle()
        {
            List<Bloque> list = new List<Bloque>();

            if (pcbInfo.bloques == 1)
            {
                string blockId = "1";
                List<int> posibleBlockId = detailList.Select(o => o.bloqueId).Distinct().ToList();

                if (posibleBlockId.Count > 0)
                {
                    blockId = posibleBlockId.First().ToString();
                }

                Bloque b = new Bloque(barcode);
                b.bloqueId = int.Parse(blockId);
                list.Add(b);
            }
            else
            {
                if (pcbInfo.bloques > 1)
                {
                    string query = OracleQuery.ListBlockBarcode(this);
                    DataTable dt = _oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        #region CREATE_BLOCKBARCODE_OBJECT
                        foreach (DataRow r in dt.Rows)
                        {
                            Bloque b = new Bloque(r["SEG_BARCODE"].ToString());
                            b.bloqueId = int.Parse(r["SEG_ID"].ToString());
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
                for (int i = 1; i <= pcbInfo.bloques; i++)
                {
                    Bloque b = new Bloque(barcode + "-" + i);
                    b.bloqueId = i;
                    list.Add(b);
                }
                #endregion
            }

            return list;
        }
    }
}