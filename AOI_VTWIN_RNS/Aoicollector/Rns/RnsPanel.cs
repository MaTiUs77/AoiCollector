﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Aoicollector.Rns.Controller;

namespace AOI_VTWIN_RNS.Aoicollector.Rns
{
    public class RnsPanel : InspectionController
    {
        private RnsInspection rnsi;

        /// <summary>
        /// Completa los datos del panel inspeccionado
        /// </summary>
        /// <param name="r"></param>
        /// <param name="inspMachine"></param>
        public RnsPanel(FileInfo fileInfo, RnsInspection _rnsInspection)
        {
            rnsi = _rnsInspection;
            CreateInspectionObject(fileInfo);
        }

        private void CreateInspectionObject(FileInfo fileInfo)
        {
            DataTable contenidoCsv = null;

            #region INTENTA ABRIR ARCHIVO CSV
            try
            {
                rnsi.aoiLog.debug("Leyendo: " + csvFilePath.FullName);
                contenidoCsv = FilesHandler.FileToTable(csvFilePath.FullName, ',');
                //    string newFile = @"\\vt-rns-srv\CGSData\InspectionCSVFiles\" + file.Name;
                //    File.Copy(file.FullName,newFile, true);
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
                    #region LEE COLUMNAS DE ARCHIVO CSV
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

                    string panelNroReemp = info[38].ToString().Replace("\"", "").Trim();
                    if (!panelNroReemp.Equals(""))
                    {
                        panelNro = int.Parse(panelNroReemp);
                    }
                    #endregion

                    if (!barcode.Equals(""))
                    {
                        //BlockBarcode.validateBarcode(aoiInsp.inspectionObj);

                        // Adjunto informacion de maquina
                        machine = Machine.list.Where(obj => obj.maquina == maquina).FirstOrDefault();
                        if (machine == null)
                        {
                            rnsi.aoiLog.warning("No existe la maquina: " + maquina + " en la base de datos MySQL");
                        }
                        else
                        {
                            rnsi.DynamicTab(machine);

                            machine.LogBroadcast("info",
                                string.Format("{0} | Maquina {1} | Ultima inspeccion {2}", machine.smd, machine.line_barcode, machine.ultima_inspeccion)
                            );

                            machine.LogBroadcast("info",
                               string.Format("Programa: [{0}] - Barcode: {1}", programa, barcode)
                            );

                            // Adjunto informacion del PCB usado para inspeccionar, contiene numero de bloques y block_id entre otros datos.
                            PcbInfo pcb_info = PcbInfo.list.Find(obj => obj.nombre.Equals(programa) && obj.tipoMaquina.Equals(rnsi.aoiConfig.machineNameKey));
                            if (pcb_info != null)
                            {
                                pcbInfo = pcb_info;
                            }

                            // Obtiene detalle de errores del panel completo
                            detailList = GetInspectionDetail(contenidoCsv);

                            if (!maquina.Contains("PT"))
                            {
                                InspectionResultFile inspResult = new InspectionResultFile(this);
                                bloqueList = inspResult.GetBlockBarcodes();
                            }

                            MakeRevisionToAll();

                            rnsi.aoiLog.debug("CreateaoiInsp.inspectionObject(): complete");
                        }
                    } // IF panelBarcode != ""
                } // IF csv has rows
                else
                {
                    rnsi.aoiLog.warning("El archivo " + csvFilePath.FullName + " no tiene filas");
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