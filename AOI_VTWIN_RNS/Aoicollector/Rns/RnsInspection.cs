using System.IO;
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
                    panel.TrazaSave(aoiConfig.xmlExportPath);

                    if (!Config.debugMode)
                    {
                        aoiLog.debug("Actualizando fecha de ultima inspeccion en maquina");
                        Machine.UpdateInspectionDate(panel.machine.mysql_id);
                        panel.machine.Ping();

                        // Elimino el archivo luego de procesarlo.
                        File.Delete(panel.csvFilePath.FullName);
                        aoiLog.debug("Eliminando: " + panel.csvFilePath.FullName);
                    }
                }
            } else
            {
                // Elimino el archivo luego de procesarlo.
                File.Delete(panel.csvFilePath.FullName);
                aoiLog.debug("Eliminando: " + panel.csvFilePath.FullName);
            }
        }
    }
}
