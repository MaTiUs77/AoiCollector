using System;
using System.Collections.Generic;
using System.Linq;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vtwin.Controller;
using System.Data;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Vtwin
{
    public class VtwinInspection : AoiController
    {
        public void HandleInspection(Machine inspMachine)
        {
            DateTime last_oracle_inspection = new DateTime(1, 1, 1);
            inspMachine.LogBroadcast("info", 
                string.Format("{0} | Maquina {1} | Ultima inspeccion {2}", inspMachine.smd, inspMachine.line_barcode, inspMachine.ultima_inspeccion)
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
                        string.Format("+ Programa: [{0}] - Barcode: {1} - Bloques: {2}", panel.programa, panel.barcode, panel.totalBloques, inspMachine.smd)
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
                    string.Format("Actualizando horario de ultima inspeccion: {0}", last_oracle_inspection.ToString("HH:mm:ss"))
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

                        if (Config.isByPassMode(inspMachine.linea))
                        {
                            // SKIP MACHINE
                            LogBroadcast(inspMachine,"warning",string.Format("{1} | Maquina en ByPass, no se analiza modo pendiente de {0}", pend.barcode, inspMachine.smd));
                        }
                        else
                        {
                            VtwinPanel panel = new VtwinPanel(oracle, oracleLastRow, inspMachine);

                            if (panel.pendiente)
                            {
                                // Aun sigue pendiente... no hago nada...
                                LogBroadcast(inspMachine,"log", string.Format("Sigue pendiente {0} desde la fecha {1}", pend.barcode, pend.endDate));
                            }
                            else
                            {
                                // No esta mas pendiente!!, se realizo la inspeccion!! Guardo datos.
                                LogBroadcast(inspMachine,"notify",string.Format("Inspeccion detectada! {0}", pend.barcode));

                                panel.pendienteDelete = true;

                                panel.TrazaSave(aoiConfig.xmlExportPath);
                            }

                            aoiWorker.SetProgressWorking(count);
                        }
                    }
                    else
                    {
                        aoiLog.log("No se detectaron actualizaciones de estado");
                    }
                }
            }
        }
    }
}