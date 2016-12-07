using System;
using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Zenith.Controller;
using System.Data;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Zenith
{
    public class ZenithInspection : AoiController
    {
        public void HandleInspection(Machine inspMachine)
        {
            DateTime new_last_inspection = new DateTime(1, 1, 1);

            //inspMachine.LogBroadcast("info", 
            //    string.Format("{0} | Maquina {1} | Ultima inspeccion {2}", 
            //        inspMachine.smd, 
            //        inspMachine.line_barcode, 
            //        inspMachine.ultima_inspeccion
            //    )
            //);

            string query = SqlServerQuery.ListLastInspections(inspMachine.maquina, inspMachine.ultima_inspeccion);
            DataTable dt = sqlserver.Query(query);
            int totalRows = dt.Rows.Count;

            aoiWorker.SetProgressTotal(totalRows);

            if (totalRows > 0)
            {
                inspMachine.LogBroadcast("notify", 
                    string.Format("Se encontraron ({0}) inspecciones.", totalRows)
                );

                int count = 0;

                #region CREATE_INSPECTION_OBJECT
                foreach (DataRow r in dt.Rows)              
                {
                    count++;
                    ZenithPanel panel = new ZenithPanel(sqlserver,r, inspMachine);

                    inspMachine.LogBroadcast("info", 
                        string.Format("Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                    );

                    panel.TrazaSave(aoiConfig.xmlExportPath);
                    aoiWorker.SetProgressWorking(count);

                    // Ultima inspeccion realizada en la maquina ORACLE.
                    new_last_inspection = DateTime.Parse(panel.fecha + " " + panel.hora);
                }

                if (new_last_inspection.Year.Equals(1))
                {
                    new_last_inspection = oracle.GetSysDate();
                }
                #endregion

                if (!Config.debugMode)
                {
                    inspMachine.LogBroadcast("debug",
                       string.Format("Actualizando horario de ultima inspeccion de maquina {0}", new_last_inspection.ToString("HH:mm:ss"))
                    );
                    Machine.UpdateInspectionDate(inspMachine.mysql_id, new_last_inspection);
                }
            }
            else
            {
                inspMachine.LogBroadcast("notify", "No se encontraron inspecciones");
            }

            inspMachine.Ping();
        }

        public void HandlePendientInspection()
        {
            List<Pendiente> pendList = Pendiente.Download(aoiConfig.machineNameKey);
            aoiLog.debug("Verificando inspecciones pendientes. Total: " + pendList.Count);

            if (pendList.Count > 0)
            {
                int count = 0;
                aoiWorker.SetProgressTotal(pendList.Count);

                foreach (Pendiente pend in pendList)
                {
                    // Busco ultimo estado del barcode
                    string query = SqlServerQuery.ListPanelBarcodeInfo(pend);
                    DataTable dt = sqlserver.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        count++;
                        DataRow lastRow = dt.Rows[0];
                        Machine inspMachine = Machine.list.Single(obj => obj.tipo == aoiConfig.machineNameKey && obj.mysql_id == pend.idMaquina);

                        if (Config.isByPassMode(inspMachine))
                        {
                            // SKIP MACHINE
                            inspMachine.LogBroadcast("warning",
                                string.Format("{0} {1} | En ByPass, no se analiza modo pendiente de {2}",
                                inspMachine.maquina,
                                inspMachine.smd,
                                pend.barcode)
                            );
                        }
                        else
                        {
                            ZenithPanel panel = new ZenithPanel(sqlserver, lastRow, inspMachine);

                            if(panel.pendiente)
                            {
                                inspMachine.LogBroadcast("info",
                                   string.Format("+ Sigue pendiente Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                               );
                            } else
                            {
                                inspMachine.LogBroadcast("info",
                                    string.Format("+ Inspeccion detectada! Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                                );
                                panel.pendienteDelete = true;

                                panel.TrazaSave(aoiConfig.xmlExportPath);
                            }
                        }
                    }
                    else
                    {
                        aoiLog.debug("No se detectaron actualizaciones de estado");
                    }
                }
            }
        }
    }
}