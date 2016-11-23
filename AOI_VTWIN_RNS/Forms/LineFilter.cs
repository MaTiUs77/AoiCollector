using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage
{
    public partial class LineFilter : Form
    {
        public LineFilter()
        {
            InitializeComponent();
        }

        private void LineFilter_Load(object sender, EventArgs e)
        {
            fillGrid();
        }

        private void fillGrid() 
        {
            List<Machine> allmachines = Machine.list;
            IOrderedEnumerable<Machine> lineasOrdenadas = allmachines.OrderBy(o => o.nroLinea);

            foreach (Machine machine in lineasOrdenadas)
            {
                bool check = false;
                if (Config.byPassLine.Count > 0)
                {
                    if (Config.isByPassMode(machine))
                    {
                        check = true;
                    }
                }

                int row = gridLine.Rows.Add(
                    check,
                    machine.maquina,
                    machine.smd,
                    machine.tipo,
                    ( check ? "ByPass" : "Procesar")
                );

                if (check)
                {
                    gridLine.Rows[row].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#f26a68");
                }
                else
                {
                    gridLine.Rows[row].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#a4e8a4");
                }
            }
        }

        private void gridLine_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int columna = e.ColumnIndex;
            int fila = e.RowIndex;

            if (columna >= 0 && fila >= 0)
            {
                DataGridViewRow row = gridLine.Rows[fila];

                bool bypassmode = (bool)row.Cells["colCheck"].Value;

                if (bypassmode)
                {
                    modeProcesar(row);
                }
                else
                {
                    modeByPass(row);
                }
            }
        }

        private void modeByPass(DataGridViewRow row)
        {
            row.Cells["colCheck"].Value = true;
            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#f26a68");
            string aoicode = row.Cells["idCol"].Value.ToString();
            Config.byPassLine.Add(Machine.findByCode(aoicode));
            row.Cells["colByPass"].Value = "ByPass";
        }

        private void modeProcesar(DataGridViewRow row)
        {
            row.Cells["colCheck"].Value = false;
            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#a4e8a4");
            string aoicode = row.Cells["idCol"].Value.ToString();

            if (Config.byPassLine.Count > 0)
            {
                Config.byPassLine.Remove(Machine.findByCode(aoicode));
                row.Cells["colByPass"].Value = "Procesar";
            } 
        }

        /**** BYPASS ****/
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.byPassLine = new List<Machine>();
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                modeByPass(row);
            }
        }

        private void rnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byPassMachineType("R");
        }

        private void vtwinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byPassMachineType("W");
        }

        private void zenithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byPassMachineType("Z");
        }

        private void byPassMachineType(string type)
        {
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                if (row.Cells["colTipo"].Value.Equals(type))
                {
                    modeByPass(row);
                }
            }
        }

        /**** REMOVER BYPASS ****/
        private void verTodasLasMaquinasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                modeProcesar(row);
            }
        }
    }
}
