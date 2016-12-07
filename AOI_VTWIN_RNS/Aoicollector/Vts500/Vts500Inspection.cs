using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Vts500.Controller;
using System.Data;
using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Aoicollector.Inspection;

namespace CollectorPackage.Aoicollector.Vts500
{
    public class Vts500Inspection : AoiController
    {
        public void HandleInspection(Machine inspMachine)
        {
            // Dummy date
            DateTime last_oracle_inspection = new DateTime(1, 1, 1);
            inspMachine.LogBroadcast("info",
               string.Format("{0} | Maquina {1} | Ultima inspeccion {2}",
                   inspMachine.smd,
                   inspMachine.line_barcode,
                   inspMachine.ultima_inspeccion
               )
           );

            string query = OracleQuery.ListLastInspections(inspMachine);
            DataTable dt = oracle.Query(query);
            int totalRows = dt.Rows.Count;

            aoiWorker.SetProgressTotal(totalRows);

            if (totalRows > 0)
            {
                inspMachine.LogBroadcast("notify",
                   string.Format("Se encontraron({0}) inspecciones.", totalRows)
                );

                int count = 0;

                #region CREATE_INSPECTION_OBJECT
                foreach (DataRow r in dt.Rows)
                {
                    count++;
                    Vts500Panel panel = new Vts500Panel(oracle, r, inspMachine);

                    inspMachine.LogBroadcast("info",
                        string.Format("+ Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                    );

                    panel.TrazaSave(aoiConfig.xmlExportPath);
                    aoiWorker.SetProgressWorking(count);

                    // Ultima inspeccion realizada en la maquina ORACLE.
                    last_oracle_inspection = DateTime.Parse(panel.fecha + " " + panel.hora);
                }

                if (last_oracle_inspection.Year.Equals(1))
                {
                    last_oracle_inspection = oracle.GetSysDate();
                }
                #endregion

                inspMachine.LogBroadcast("debug",
                    string.Format("Actualizando horario de ultima inspeccion de maquina {0}", last_oracle_inspection.ToString("HH:mm:ss"))
                );

                if (!Config.debugMode)
                {
                    Machine.UpdateInspectionDate(inspMachine.mysql_id, last_oracle_inspection);
                }
            }
            else
            {
                inspMachine.LogBroadcast("notify",
                    string.Format("{0} No se encontraron inspecciones", inspMachine.smd)
                );
            }

            inspMachine.Ping();
        }
        
        public void HandlePendientInspection()
        {
            List<Pendiente> pendList = Pendiente.Download(aoiConfig.machineNameKey);
            Machine inspMachine = Machine.list.Single(obj => obj.tipo == aoiConfig.machineNameKey);

            aoiLog.info("Verificando inspecciones pendientes. Total: " + pendList.Count);

            if (pendList.Count > 0)
            {
                int count = 0;
                aoiWorker.SetProgressTotal(pendList.Count);

                foreach (Pendiente pend in pendList)
                {
                    // Busco ultimo estado del barcode en ORACLE.
                    string query = OracleQuery.ListLastInspections(inspMachine, pend);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        count++;

                        DataRow oracleLastRow = dt.Rows[totalRows - 1];

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
                            Vts500Panel panel = new Vts500Panel(oracle, oracleLastRow, inspMachine);

                            if (panel.pendiente)
                            {
                                inspMachine.LogBroadcast("info",
                                  string.Format("+ Sigue pendiente Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                                );
                            }
                            else
                            {
                                inspMachine.LogBroadcast("info",
                                   string.Format("+ Inspeccion detectada! Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                                );
                                panel.pendienteDelete = true;

                                panel.TrazaSave(aoiConfig.xmlExportPath);
                            }

                            aoiWorker.SetProgressWorking(count);
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
