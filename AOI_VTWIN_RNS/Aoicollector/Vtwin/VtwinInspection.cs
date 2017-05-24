using System;
using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Vtwin.Controller;
using System.Data;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Vtwin
{
    public class VtwinInspection : AoiController
    {
        public void HandleInspection(Machine inspMachine)
        {
            DateTime last_oracle_inspection = new DateTime(1, 1, 1);
            inspMachine.LogBroadcast("info", 
                string.Format("{0} | Maquina {1} | Ultima inspeccion {2}", 
                    inspMachine.smd, 
                    inspMachine.line_barcode, 
                    inspMachine.ultima_inspeccion
                )
            );

            string query = OracleQuery.ListLastInspections(inspMachine.oracle_id, inspMachine.ultima_inspeccion);
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
                    VtwinPanel panel = new VtwinPanel(oracle,r, inspMachine);

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
            aoiLog.info("Verificando inspecciones pendientes. Total: " + pendList.Count);
            string queryy = OracleQuery.ListLastInspections(0, "", null);
            if (pendList.Count > 0)
            {
                int count = 0;
                aoiWorker.SetProgressTotal(pendList.Count);

                foreach (Pendiente pend in pendList)
                {
                    // Busco ultimo estado del barcode en ORACLE.
                    string query = OracleQuery.ListLastInspections(0, "", pend);
                    DataTable dt = oracle.Query(query);
                    int totalRows = dt.Rows.Count;

                    if (totalRows > 0)
                    {
                        count++;

                        DataRow oracleLastRow = dt.Rows[totalRows - 1];
                        int oracle_id = int.Parse(oracleLastRow["TEST_MACHINE_ID"].ToString());
                        Machine inspMachine = Machine.list.Single(obj => obj.tipo == aoiConfig.machineNameKey && obj.oracle_id == oracle_id);

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
                            VtwinPanel panel = new VtwinPanel(oracle, oracleLastRow, inspMachine);

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

        public DataTable PanelBarcodeInfo(string barcode)
        {
            string query = OracleQuery.ListPanelBarcodeInfo(barcode);
            DataTable dt = oracle.Query(query);

            return dt;
        }
    }
}