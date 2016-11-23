using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

using CollectorPackage.Src.Util.Files;
using CollectorPackage.Aoicollector.Inspection.Model;

namespace CollectorPackage.Aoicollector.Core
{
    /// <summary>
    /// Las maquinas RNS y VTWIN tienen las informacion de los programas en archivos .PCB, lo cual 
    /// hace posible obtener la cantidad de bloques (segmentos) de cada programa de inspeccion. 
    /// Esto sirve para verificar si se obtuvo una lectura correcta de barcodes segun la cantidad 
    /// de bloques del programa o si quedo alguno pendiente.
    /// </summary>
    class PcbData 
    {
        private AoiController aoi;

        public PcbData(AoiController _aoi) 
        {
            aoi = _aoi;
        }

        /// <summary>
        /// Los segmentos son los bloques definidos en el archivo .pcb, los dos primeros segmentos 
        /// se refieren a la placa y a los fiduciales, se descarta el resto de los segmentos, contiene el block_id designado a cada bloque
        /// </summary>
        private List<string> BuildSegment(string content)
        {
            List<string> segmentList = new List<string>();
            // Remuevo los dos primeros segmentos.
            content = content.Replace("[SEG 1]", "REMOVED").Replace("[SEG 2]", "REMOVED");
            MatchCollection idCollection = Regex.Matches(content, @"(\[SEG (?<numero>\d+)\]\r\n(?<id>\d+))");

            foreach (Match mc in idCollection)
            {
                GroupCollection gr = mc.Groups;
                segmentList.Add(gr["id"].Value);
            }

            return segmentList;
        }

        private string LibraryName(string content)
        {
            MatchCollection idLibrary = Regex.Matches(content, @"(\[PARTS-(.*)\]\r\n(.*)\r\n(?<library>\w+))");
            string libraryName = "";
            foreach (Match mc in idLibrary)
            {
                GroupCollection gr = mc.Groups;
                libraryName = gr["library"].Value;
            }

            return libraryName;
        }

        private int EtiquetasEnPrograma(string content)
        {
            MatchCollection match = Regex.Matches(content, @"(2DBL_\d+)");
            int etiquetas = 0;
            foreach (Match mc in match)
            {
                etiquetas++;
            }

            return etiquetas;
        }

        private int EtiquetaPcbIdEnPrograma(string content)
        {
            MatchCollection match = Regex.Matches(content, @"(2DBD_\d+)");
            int etiquetas = 0;
            foreach (Match mc in match)
            {
                etiquetas++;
            }

            return etiquetas;
        }

        /// <summary>
        /// Verifica y actualiza si es necesario la informacion de todas las PCB de VTWIN y RNS
        /// </summary>
        public bool VerifyPcbFiles() 
        {
            // Verifico archivos PCB 
            IOrderedEnumerable<FileInfo> pcbList = FilesHandler.GetFiles("*.pcb", aoi.aoiConfig.dataProgPath);
            int totalPcb = pcbList.Count();
            bool reload = false;
            aoi.aoiWorker.SetProgressTotal(totalPcb);

            #region PCBLIST_FILES_DETECTED
            if (totalPcb > 0)
            {
                int countPcb = 0;
                foreach (FileInfo pcb in pcbList)
                {

                    //if (pcb.Name.Equals("NT1010E-M-BOT-SBAST-LR.pcb"))
                    //{
                    //    countPcb = 12344;
                    //}
                    countPcb++;

                    string fechaModificacion = pcb.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Verifico si existe el pcb en la base de datos.
                    PcbInfo pcbInfo = PcbInfo.list.Find(o =>
                        o.programa.Equals(pcb.Name) &&
                        o.tipoMaquina.Equals(aoi.aoiConfig.machineNameKey)
                    );

                    if (pcbInfo == null)
                    {
                        // Si no existe en DB, lo inserto
                        Insert(pcb, fechaModificacion);
                        reload = true;
                    } 
                    else
                    {
                        string newHash = "";

                        // Si existe, verifico que la fecha sea diferente
                        if (!pcbInfo.fechaModificacion.Equals(fechaModificacion))
                        {
                            // Si la fecha de modificacion en SQL no es igual a la fecha de modificacion actual.                            
                            // Comparo HASH de archivo, contra el guardado en la DB
                            newHash = PcbInfo.Hash(pcb.FullName);

                            if (pcbInfo.hash.Equals(newHash))
                            {
                                // Si el hash es similar, no hay cambios en el archivo, solo cambia la fecha
                                UpdateDate(pcbInfo, fechaModificacion);
                                reload = true;
                            }
                            else
                            {
                                // Actualizo la informacion del PCB
                                Update(pcb, pcbInfo, newHash, fechaModificacion);
                                reload = true;
                            }
                        }
                    }

                    aoi.aoiWorker.SetProgressWorking(countPcb);

                } // END FOREACH
            }
            else
            {
                aoi.aoiLog.warning("No se encontraron archivos PCB en: " + aoi.aoiConfig.dataProgPath);
            }
            #endregion

            return reload;
        }

        private void Insert(FileInfo pcb, string fechaModificacion) 
        {
            // Si no existe genero un Hash que identifica el contenido del archivo.
            // mas adelante se compara el hash nuevo con el anterior para verificar
            // si se modifico alguna linea.
            string pcbContent = FilesHandler.ReadFile(pcb.FullName);
            string newHash = PcbInfo.Hash(pcb.FullName);

            List<string> buildSegmentos = BuildSegment(pcbContent);
            int totalBloques = buildSegmentos.Count;
            string segmentos = string.Join(",", buildSegmentos.ToArray());

            PcbInfo pcbInfo = new PcbInfo();

            pcbInfo.programa = pcb.Name;
            pcbInfo.bloques = totalBloques;
            pcbInfo.segmentos = segmentos;
            pcbInfo.hash = newHash;
            pcbInfo.tipoMaquina = aoi.aoiConfig.machineNameKey;
            pcbInfo.fechaModificacion = fechaModificacion;

            PcbInfo.Insert(pcbInfo);
        }

        private void UpdateDate(PcbInfo pcbInfo, string fechaModificacion)
        {
            // El HASH no ha cambiado.
            aoi.aoiLog.verbose("-- Fecha modificada " + pcbInfo.nombre + " | No hay cambios.");
            pcbInfo.fechaModificacion = fechaModificacion;
            PcbInfo.Update(pcbInfo);
        }

        private void Update(FileInfo pcb, PcbInfo pcbInfo, string newHash, string fechaModificacion)
        {
            // El HASH es diferente, hay cambios.
            string pcbContent = FilesHandler.ReadFile(pcb.FullName);

            List<string> buildSegmentos = BuildSegment(pcbContent);
            int totalBloques = buildSegmentos.Count;
            string segmentos = string.Join(",", buildSegmentos.ToArray());

            string libraryName = LibraryName(pcbContent);
            int totalEtiquetas = EtiquetasEnPrograma(pcbContent);
            int totalPcbId = EtiquetaPcbIdEnPrograma(pcbContent);

            // No hay etiquetas en bloques, pero si tengo etiqueta de pcb id 
            if (totalEtiquetas == 0 && totalPcbId > 0)
            {
                totalEtiquetas = 1;

                // Probablemente se trate de una placa secundaria si tiene varios bloques
                if(totalBloques > 1)
                {
                    pcbInfo.secundaria = 1;
                }
            }

            aoi.aoiLog.verbose("-- Actualizando informacion de: " + pcb.Name);

            pcbInfo.programa = pcb.Name;
            pcbInfo.bloques = totalBloques;
            pcbInfo.segmentos = segmentos;
            pcbInfo.hash = newHash;
            pcbInfo.fechaModificacion = fechaModificacion;
            pcbInfo.libreria = libraryName;
            pcbInfo.etiquetas = totalEtiquetas;

            PcbInfo.Update(pcbInfo);
        }
    }
}
