using System;
using System.IO;
using System.Data;

using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Rns
{
    public class RnsInspection : AoiController
    {
        public void HandleInspection(FileInfo file)
        {
            RnsPanel panel = new RnsPanel(file,this);

            if (panel.machine.linea != null)
            {
                // Filtro maquinas a inspeccionar
                if (Config.isByPassMode(panel.machine.linea))
                {
                    // SKIP MACHINE
                    aoiLog.warning("Maquina en ByPass: " + panel.machine.linea + " / Se detiene el proceso de inspeccion");
                }
                else
                {
                    /*
                        RNS no tiene inspecciones pendientes, directamente no se procesan porque el archivo se encuentra sin filas   
                        por lo que se realiza el TrazaSave directamente sin verificar estados pendientes                 
                    */
                    if(panel.pcbInfo.etiquetas==panel.totalBloques)
                    {
                        panel.TrazaSave(aoiConfig.xmlExportPath);
                    } else
                    {
                        panel.machine.LogBroadcast("warning",
                            string.Format("No se leyeron todas las etiquetas | Solicitadas: {0} | Leidas: {1}", panel.pcbInfo.etiquetas,panel.totalBloques)
                        );
                    }

                    aoiLog.log("Actualizando fecha de ultima inspeccion en maquina");
                    Machine.UpdateInspectionDate(panel.machine.mysql_id);
                    panel.machine.Ping();

                    if (!Config.debugMode)
                    {
                        // Elimino el archivo luego de procesarlo.
                        File.Delete(panel.csvFilePath.FullName);
                        aoiLog.debug("Eliminando: "+ panel.csvFilePath.FullName);
                    }
                }
            }            
        }
    }
}
