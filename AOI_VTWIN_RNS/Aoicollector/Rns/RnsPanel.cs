using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Inspection;
using CollectorPackage.Src.Util.Files;
using CollectorPackage.Aoicollector.Rns.Controller;
using System.Text.RegularExpressions;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Rns
{
    public class RnsPanel : InspectionController
    {
        private RnsInspection rnsi;

        /// <summary>
        /// Busca todos los datos de inspeccion relacionados con el panel
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="_rnsInspection"></param>
        public RnsPanel(FileInfo fileInfo, RnsInspection _rnsInspection)
        {
            rnsi = _rnsInspection;
            csvFilePath = fileInfo;
            CreateInspectionObject();
        }

        private void CreateInspectionObject()
        {
            DataTable contenidoCsv = null;

            #region ABRE Y LEE TODAS LAS FILAS DEL ARCHIVO CSV
            try
            {
                rnsi.aoiLog.debug("Leyendo: " + csvFilePath.FullName);
                contenidoCsv = FilesHandler.FileToTable(csvFilePath.FullName, ',');
            }
            catch (Exception ex)
            {
                rnsi.aoiLog.stack("No fue posible leer: " + csvFilePath.FullName, this, ex);
            }
            #endregion

            // Solo si el archivo tiene al menos una fila de informacion
            if (contenidoCsv != null)
            {
                if (contenidoCsv.Rows.Count > 0)
                {
                    #region LEE COLUMNAS DE ARCHIVO CSV Y VALIDA BARCODE
                    DataRow info = contenidoCsv.Rows[0];

                    csvFile = csvFilePath.Name;
                    csvDatetime = csvFilePath.LastWriteTime;
                    csvDateSaved = DateTime.Parse(info[15].ToString().Replace("U", ""));
                    csvDateCreate = DateTime.Parse(info[18].ToString().Replace("U", ""));

                    fecha = csvDatetime.ToString("yyyy-MM-dd");
                    hora = csvDatetime.ToString("HH:mm:ss");

                    maquina = info[5].ToString().Replace("\"", "").Trim();
                    programa = info[7].ToString().Replace("\"", "").Trim();

                    barcode = info[36].ToString().Replace("\"", "").Trim();

                    BarcodeValidate();                    

                    string panelNroReemp = info[38].ToString().Replace("\"", "").Trim();
                    if (!panelNroReemp.Equals(""))
                    {
                        panelNro = int.Parse(panelNroReemp);
                    }
                    #endregion

                    if (!barcode.Equals(""))
                    {
                        // Adjunto informacion de maquina
                        machine = Machine.list.Where(obj => obj.maquina == maquina).FirstOrDefault();
                        if (machine == null)
                        {
                            machine.LogBroadcast("warning",
                                string.Format("No existe la maquina: {0} en la base de datos MySQL", machine.maquina)
                            );
                        }
                        else
                        {
                            rnsi.DynamicTab(machine);

                            machine.LogBroadcast("info",
                                string.Format("{0} | Maquina {1} | Ultima inspeccion {2}",
                                    machine.smd,
                                    machine.line_barcode,
                                    machine.ultima_inspeccion
                                )
                            );

                            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
                            PcbInfo pcb_info = PcbInfo.list.Find(obj => obj.nombre.Equals(programa) && obj.tipoMaquina.Equals(rnsi.aoiConfig.machineNameKey));
                            if (pcb_info != null)
                            {
                                pcbInfo = pcb_info;
                            }

                            if (!Config.isByPassMode(machine))
                            {
                                // Adhiere las rutas a las carpetas de inspecciones
                                InspectionResult inspResult = new InspectionResult(this, rnsi);

                                if (inspResult.located)
                                {
                                    detailList = GetInspectionDetail(contenidoCsv);

                                    if (!maquina.Contains("PT"))
                                    {

                                        bloqueList = inspResult.GetBlockBarcodes();
                                    }

                                    MakeRevisionToAll();

                                    machine.LogBroadcast("info",
                                       string.Format("Programa: [{0}] | Barcode: {1} | Bloques: {2}",
                                       programa,
                                       barcode,
                                       totalBloques
                                       )
                                    );
                                }
                            }
                        }
                    }
                    else
                    {
                        rnsi.aoiLog.warning("El archivo no tiene codigo de panel");
                    }
                }
                else
                {
                    rnsi.aoiLog.warning("El archivo " + csvFilePath.FullName + " no tiene filas");
                }
            }
        }


        /// <summary>
        /// Obtiene detalle de errores del panel completo (extraidos del resumen del csv)
        /// </summary>
        /// <param name="contenidoCsv"></param>
        /// <returns></returns>
        private List<Detail> GetInspectionDetail(DataTable contenidoCsv)
        {
            //List<FileInfo> imageFiles = rnsCurrentInspectionResultDirectory.GetFiles("*.jpg").ToList();

            List<Detail> detalles = new List<Detail>();
            // Recorro todos los detalles de la inspeccion
            foreach (DataRow r in contenidoCsv.Rows)
            {
                #region Inspection DETAIL

                Detail det = new Detail();
                det.referencia = r[42].ToString().Replace("\"", "").Trim();
                if (!det.referencia.Trim().Equals(""))
                {
                    det.componentNo = int.Parse(r[41].ToString().Replace("\"", "").Trim());
                    det.bloqueId = int.Parse(r[39].ToString().Replace("\"", "").Trim());
                    det.faultcode = r[49].ToString().Replace("\"", "").Trim();
                    det.realFaultcode = r[50].ToString().Replace("\"", "").Trim();
                    det.descripcionFaultcode = Faultcode.Description(det.faultcode);

                    //det.faultImages = imageFiles.Where(x => x.Name.Contains(det.componentNo+"-")).ToList();

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