using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Src.Util.Crypt;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using AOI_VTWIN_RNS.Aoicollector.Rns.Controller;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Rns
{
    public class RnsInspection : AoiController
    {
        public void HandleInspection(FileInfo file)
        {
            // Reinicio inspectionObject
            ResetInspection();

            inspectionObj.csvFilePath = file;

            DataTable contenidoCsv = null;

            #region INTENTA ABRIR ARCHIVO CSV
            try
            {
                contenidoCsv = FilesHandler.FileToTable(inspectionObj.csvFilePath.FullName, ',');
                //    string newFile = @"\\vt-rns-srv\CGSData\InspectionCSVFiles\" + file.Name;
                //    File.Copy(file.FullName,newFile, true);
            }
            catch (Exception ex)
            {
                aoiLog.Area("No fue posible leer: " + inspectionObj.csvFilePath.FullName + " Message: " + ex.Message.ToString(), "error");
            }
            #endregion

            CreateInspectionObject(contenidoCsv);

            if (inspectionObj.machine.linea != null)
            {
                // Filtro maquinas a inspeccionar
                if (Config.isByPassMode(inspectionObj.machine.linea))
                {
                    // SKIP MACHINE
                    aoiLog.Area("+ Maquina en ByPass: " + inspectionObj.machine.linea + " / Se detiene el proceso de inspeccion");
                }
                else
                {
                    TrazaSave(aoiConfig.xmlExportPath);
                    Machine.Ping(inspectionObj.machine.mysql_id);

                    aoiLog.Area("+ Actualizando fecha de ultima inspeccion en maquina");
                    Machine.UpdateInspectionDate(inspectionObj.machine.mysql_id);

                    if (!Config.debugMode)
                    {
                        // Elimino el archivo luego de procesarlo.
                        File.Delete(inspectionObj.csvFilePath.FullName);
                    }
                }
            }            
        }

        private void CreateInspectionObject(DataTable contenidoCsv)
        {
            // Solo si el archivo tiene al menos una fila de informacion
            if (contenidoCsv != null)
            {
                if (contenidoCsv.Rows.Count > 0)
                {
                    #region LEE COLUMNAS DE ARCHIVO CSV
                    DataRow info = contenidoCsv.Rows[0];

                    inspectionObj.csvFile = inspectionObj.csvFilePath.Name;
                    inspectionObj.csvDatetime = inspectionObj.csvFilePath.LastWriteTime;
                    inspectionObj.fecha = inspectionObj.csvDatetime.ToString("yyyy-MM-dd");
                    inspectionObj.hora = inspectionObj.csvDatetime.ToString("HH:mm:ss");

                    inspectionObj.maquina = info[5].ToString().Replace("\"", "").Trim();
                    inspectionObj.programa = info[7].ToString().Replace("\"", "").Trim();

                    inspectionObj.csvDateSaved = DateTime.Parse(info[15].ToString().Replace("U", ""));
                    inspectionObj.csvDateCreate = DateTime.Parse(info[18].ToString().Replace("U", ""));

                    inspectionObj.panelBarcode = info[36].ToString().Replace("\"", "").Trim();

                    string panelNroReemp = info[38].ToString().Replace("\"", "").Trim();
                    if (!panelNroReemp.Equals(""))
                    {
                        inspectionObj.panelNro = int.Parse(panelNroReemp);
                    }
                    #endregion

                    if (!inspectionObj.panelBarcode.Equals(""))
                    {
                        BlockBarcode.validateBarcode(inspectionObj);

                        // Adjunto informacion de maquina
                        inspectionObj.machine = Machine.list.Where(obj => obj.maquina == inspectionObj.maquina).FirstOrDefault();
                        if (inspectionObj.machine == null)
                        {
                            aoiLog.Area("No existe la maquina: " + inspectionObj.maquina + " en la base de datos MySQL", "atencion");
                        }
                        else
                        {
                            aoiLog.Area("Inspeccionando SMD-" + inspectionObj.machine.linea + " - Programa: " + inspectionObj.programa + " - Barcode: " + inspectionObj.panelBarcode);

                            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
                            PcbInfo pcb_info = PcbInfo.list.Find(obj => obj.nombre.Equals(inspectionObj.programa) && obj.tipoMaquina.Equals(aoiConfig.machineNameKey));
                            if (pcb_info != null)
                            {
                                inspectionObj.pcbInfo = pcb_info;
                            }

                            // Obtiene detalle de errores del panel completo
                            inspectionObj.detailList = GetInspectionDetail(contenidoCsv);

                            if (!inspectionObj.maquina.Contains("PT"))
                            {
                                InspectionResultFile inspResult = new InspectionResultFile(this);
                                inspectionObj.blockBarcodeList = inspResult.GetBlockBarcodes();
                            }

                            inspectionObj.ProccessPanelStatus();                                                     
                        }
                    } // IF panelBarcode != ""
                } // IF csv has rows
                else
                {
                    aoiLog.Area("El archivo " + inspectionObj.csvFilePath.FullName + " no contiene filas", "error");
                }
            }            
        }

        /// <summary>
        /// Obtiene los detalles de la inspeccion
        /// </summary>
        /// <param name="contenidoCSV"></param>
        /// <returns>List<InspectionDetail></returns>
        private List<Detail> GetInspectionDetail(DataTable contenidoCsv)
        {
            List<Detail> detalles = new List<Detail>();
            // Recorro todos los detalles de la inspeccion
            foreach (DataRow r in contenidoCsv.Rows)
            {
                #region Inspection DETAIL

                Detail det = new Detail();
                det.referencia = r[42].ToString().Replace("\"", "").Trim();
                if (!det.referencia.Trim().Equals(""))
                {

                    det.bloqueId = int.Parse(r[39].ToString().Replace("\"", "").Trim());
                    det.faultcode = r[49].ToString().Replace("\"", "").Trim();
                    det.realFaultcode = r[50].ToString().Replace("\"", "").Trim();
                    det.descripcionFaultcode = Faultcode.Description(det.faultcode);

                    // Si el real_faultcode no se detecto el error es FALSO 
                    if (det.realFaultcode.Equals("0") || det.realFaultcode.Equals(""))
                    {
                        det.estado = "FALSO";
                    }
                    else
                    {
                        det.estado = "REAL";
                    }

                    if (!DuplicatedInspectionDetail(det, detalles))
                    {
                        detalles.Add(det);
                    }
                }
                #endregion
            }
            return detalles;
        }

        /// <summary>
        /// Verifica si el faultcode esta duplicado, esto evita obtener muchos errores de un mismo componente 
        /// en el mismo panel, por ejemplo un componente con muchas patas con error de fillet.
        /// </summary>
        /// <param name="det"></param>
        /// <param name="detalles"></param>
        /// <returns>bool</returns>
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
    }
}
