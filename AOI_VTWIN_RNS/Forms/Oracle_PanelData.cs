using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Vtwin;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage
{
    public partial class Oracle_PanelData : Form
    {
        public VTWIN vtwin = null;

        public Oracle_PanelData(VTWIN _vtwin)
        {
            InitializeComponent();
            vtwin = _vtwin;
        }

        private void TestOracle_Load(object sender, EventArgs e)
        {
            
        }

        private void inCodigo_KeyUp(object sender, KeyEventArgs e)
        {
            RichLog log = new RichLog(logGeneral);

            if(e.KeyCode == Keys.Enter)
            {
                log.reset();
                try
                {
                    DataTable dt = vtwin.PanelBarcodeInfo(inCodigo.Text.ToString());
                    gridOracle.DataSource = dt;
                    if(dt.Rows.Count>0)
                    {

                        foreach (DataRow dr in dt.Rows)
                        {
                            int idFindMachine = int.Parse(dr["TEST_MACHINE_ID"].ToString());

                            // Lista de maquinas VTWIN
                            Machine machine = Machine.list.Where(obj => obj.tipo == "W" && obj.oracle_id == idFindMachine).FirstOrDefault();

                            VtwinPanel panel = new VtwinPanel(vtwin.oracle, dr, machine);

                            log.info(
                                string.Format("+ Programa: [{0}] | Barcode: {1} | Bloques: {2} | Pendiente: {3}", panel.programa, panel.barcode, panel.totalBloques, panel.pendiente)
                            );

                            log.notify(
                              string.Format("+ Aoi: {0} | Inspector: {1} | Falsos: {3} | Reales: {4} | Pendiente: {2}",
                                  panel.revisionAoi,
                                  panel.revisionIns,
                                  panel.pendiente,
                                  panel.totalErroresFalsos,
                                  panel.totalErroresReales
                              ));

                            log.debug(
                                string.Format("========================================================")
                            );
                        }
                    } else
                    {
                        log.warning(
                            string.Format("No hay datos para el barcode solicitado")
                        );
                    }
                }
                catch (Exception ex)
                {
                    log.stack(
                        string.Format("ERROR"),
                        this,ex
                    );
                }
            }
        }
    }
}
