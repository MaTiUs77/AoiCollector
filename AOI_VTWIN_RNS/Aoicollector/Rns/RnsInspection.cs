using System;
using System.IO;
using System.Data;

using CollectorPackage.Src.Util.Files;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Rns
{
    public class RnsInspection : AoiController
    {
        public void HandleInspection(FileInfo file)
        {
            RnsPanel panel = new RnsPanel(file,this);
            if (panel.machine.maquina != null)
            {
                // Filtro maquinas a inspeccionar
                if (Config.isByPassMode(panel.machine))
                {
                    // SKIP MACHINE
                    aoiLog.warning(
                        string.Format("{0} {1} | En ByPass / Se detiene el proceso de inspeccion", panel.machine.maquina, panel.machine.smd)
                    );
                }
                else
                {
                    /*
                        RNS no tiene inspecciones pendientes, directamente no se procesan porque el archivo se encuentra sin filas   
                        por lo que se realiza el TrazaSave directamente sin verificar estados pendientes                 
                    */
                    //if(panel.pcbInfo.etiquetas==panel.totalBloques)
                    //{
                    panel.TrazaSave(aoiConfig.xmlExportPath);
                    //} else
                    //{
                    //    panel.machine.LogBroadcast("warning",
                    //        string.Format("No se leyeron todas las etiquetas | Solicitadas: {0} | Leidas: {1}", panel.pcbInfo.etiquetas,panel.totalBloques)
                    //    );
                    //}


                    if (!Config.debugMode)
                    {
                        aoiLog.log("Actualizando fecha de ultima inspeccion en maquina");
                        Machine.UpdateInspectionDate(panel.machine.mysql_id);
                        panel.machine.Ping();

                        // Elimino el archivo luego de procesarlo.
                        File.Delete(panel.csvFilePath.FullName);
                        aoiLog.debug("Eliminando: " + panel.csvFilePath.FullName);
                    }
                }
            }
        }
    }
}
