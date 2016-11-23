using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Zenith.Controller;
using System.Data;
using CollectorPackage.Aoicollector.Inspection;
using CollectorPackage.Src.Database;
using System;

namespace CollectorPackage.Aoicollector.Zenith
{
    public class ZenithPanel : InspectionController
    {
        public string machineNameKey = "W";
        private SqlServerConnector _sqlserver;

        public ZenithPanel(SqlServerConnector sqlserver, DataRow r, Machine inspMachine)
        {
            _sqlserver = sqlserver;
            CreateInspectionObject(r, inspMachine);
        }

        private void CreateInspectionObject(DataRow r, Machine inspMachine)
        {
            // Informacion especifica para este tipo de maquinas 
            zenithPcbguid = r["PCBGuid"].ToString();
            zenithImageDb = r["ImageDBName"].ToString();
            zenithResultDb = r["ResultDBName"].ToString();
            zenithPcbRepair = r["PcbRepair"].ToString();
            //---------------------------------------------- 
            programa = r["programa"].ToString();
            fecha = r["aoi_fecha"].ToString();
            if(!fecha.Equals(""))
            {
                fecha = fecha.Split(' ')[0];
            }

            hora = r["aoi_hora"].ToString();
            inspFecha = r["insp_fecha"].ToString();
            inspHora = r["insp_hora"].ToString();          

            machine = inspMachine;
            maquina = inspMachine.maquina;
            barcode = r["barcode"].ToString();
            BarcodeValidate();

            revisionAoi = r["revision_aoi"].ToString();
            revisionIns = r["revision_ins"].ToString();

            // Si no tengo fecha de inspeccion, el panel se encuentra pendiente de inspeccion.
            if (revisionIns.Equals("NG") && zenithPcbRepair.Equals("0"))
            {
                pendiente = true;
            }

            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
            PcbInfo pcb_info = new PcbInfo();
            pcbInfo.bloques = 1;
            pcbInfo.tipoMaquina = "Z";
            pcbInfo.programa = "HARDCODED";

            // Obtiene detalle de errores del panel completo 
            if(revisionAoi.Equals("NG") || revisionIns.Equals("NG"))
            {
                detailList = GetInspectionDetail();
            }

            // Lista de BLOCK_ID de ORACLE, adjunta Barcodes de cada bloque
            // En caso de tener varios bloques, y una sola etiqueta, genera etiquetas virtuales para el resto de los bloques
            bloqueList = GetBloquesFromDatabase();

            MakeRevisionToAll();
        }

        /// <summary>
        /// Detalle de inspeccion del panel general
        /// </summary>
        /// <returns></returns>
        private List<Detail> GetInspectionDetail()
        {
            string query = SqlServerQuery.ListFaultInfo(this);

            DataTable dt = _sqlserver.Query(query);

            List<Detail> detalles = new List<Detail>();
            if (dt.Rows.Count > 0)
            {
                #region FILL_ERROR_DETAIL
                foreach (DataRow r in dt.Rows)
                {
                    int bid = int.Parse(r["bloque"].ToString());
                    Detail det = new Detail();
                    det.faultcode = r["faultcode"].ToString();
                    det.estado = r["estado"].ToString();
                    det.referencia = r["uname"].ToString();
                    det.bloqueId = bid;
                    //det.total_faultcode = int.Parse(r["total"].ToString());
                    det.descripcionFaultcode = r["descripcion"].ToString();

                    if (!DuplicatedInspectionDetail(det, detalles))
                    {
                        detalles.Add(det);
                    }
                }
                #endregion
            }
            return detalles;
        }

        private bool DuplicatedInspectionDetail(Detail det, List<Detail> detalles)
        {
            bool duplicado = false;

            var rs = from x in detalles
                     where
                     x.bloqueId == det.bloqueId &&
                     x.faultcode == det.faultcode &&
                     x.estado == det.estado &&
                     x.referencia == det.referencia
                     select x;

            if (rs.Count() > 0)
            {
                duplicado = true;
            }
            return duplicado;
        }

        /// <summary>
        /// Obtiene bloqueId, y barcode, no procesa informacion de inspeccion
        /// </summary>
        /// <returns></returns>
        private List<Bloque> GetBloquesFromDatabase()
        {
            List<Bloque> list = new List<Bloque>();
           
            string blockId = "1"; 
            List<int> posibleBlockId = detailList.Select(o => o.bloqueId).Distinct().ToList();

            if (posibleBlockId.Count > 0)
            {
                blockId = posibleBlockId.First().ToString();
            }

            Bloque b = new Bloque(barcode);
            b.bloqueId = int.Parse(blockId);
            list.Add(b);

            return list;
        }
    }
}