using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Vtwin.Controller;
using System.Data;
using CollectorPackage.Aoicollector.Inspection;
using CollectorPackage.Src.Database;
using System;

namespace CollectorPackage.Aoicollector.Vtwin
{
    public class VtwinPanel : InspectionController
    {
        public string machineNameKey = "W";
        private OracleConnector _oracle;

        public VtwinPanel(OracleConnector oracle, DataRow r, Machine inspMachine)
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

            // Si no tengo fecha de inspeccion, el panel se encuentra pendiente de inspeccion.
            if (inspFecha.Equals(""))
            {
                pendiente = true;
            }

            machine = inspMachine;
            maquina = inspMachine.maquina;
            barcode = r["barcode"].ToString();
            BarcodeValidate();

            //inspection.validateBarcode();

            panelNro = int.Parse(r["pcb_no"].ToString());
            revisionIns = "NG";
            revisionAoi = r["test_result"].ToString();

            // Si AOI no tiene errores las placas estan bien. 
            if (revisionAoi.Equals(""))
            {
                revisionAoi = "OK";
                pendiente = false;
            }
            //            insp.revision_ins = r["revise_result"].ToString();

            // Informacion especifica para maquinas tipo vtwin
            vtwinProgramNameId = int.Parse(r["program_name_id"].ToString());
            vtwinSaveMachineId = int.Parse(r["saved_machine_id"].ToString());
            vtwinRevisionNo = int.Parse(r["revision_no"].ToString());
            vtwinSerialNo = int.Parse(r["serial_no"].ToString());
            vtwinLoadCount = int.Parse(r["load_count"].ToString());

            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
            PcbInfo pcb_info = PcbInfo.list.Find(obj => obj.nombre.Equals(programa) && obj.tipoMaquina.Equals(machineNameKey));
            if (pcb_info != null)
            {
                pcbInfo = pcb_info;
            }

            // Obtiene detalle de errores del panel completo 
            detailList = GetInspectionDetail();

            // Lista de BLOCK_ID de ORACLE, adjunta Barcodes de cada bloque
            // En caso de tener varios bloques, y una sola etiqueta, genera etiquetas virtuales para el resto de los bloques
            bloqueList = GetBloquesFromOracle();

            MakeRevisionToAll();
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
                    string query = OracleQuery.ListBlockBarcode(barcode);
                    DataTable dt = _oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        #region CREATE_BLOCKBARCODE_OBJECT
                        foreach (DataRow r in dt.Rows)
                        {
                            Bloque b = new Bloque(r["block_barcode"].ToString());
                            b.bloqueId = int.Parse(r["bloque"].ToString());
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