using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS
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
            List<Machine> vtwinlist = Machine.list;//.Where(o => o.tipo == "W").ToList();
            List<int> lines = vtwinlist.Select(o => int.Parse(o.linea)).Distinct().ToList();
            lines.Sort();

            foreach (int line in lines)
            {
                bool check = false;
                if (Config.byPassLine.Count > 0)
                {
                    if (Config.isByPassMode(line.ToString()))
                    {
                        check = true;
                    }
                    //IEnumerable<int> bypass = Config.byPassLine.Where(o => o == line);
                    //if (Array.Exists(bypass.ToArray(),n => n == line))
                    //{
                    //}
                }

                Machine currMachine = Machine.list.Where(o => o.linea.ToString() == line.ToString()).First();

                //if (currMachine.active)
                //{
                //    check = false;
                //}
                //else
                //{
                //    check = true;
                //}

                int row = gridLine.Rows.Add(
                    check,
                    "SMD-" + line.ToString(),
                    line,
                    currMachine.tipo,
                    ( check ? "ByPass" : "Procesar")
                );

                if (check)
                {
                    gridLine.Rows[row].DefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#f26a68");
                }
                else
                {
                    gridLine.Rows[row].DefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#a4e8a4");
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
            row.DefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#f26a68");
            int linea = int.Parse(row.Cells["colIntLine"].Value.ToString());
            Config.byPassLine.Add(linea);
            row.Cells["colByPass"].Value = "ByPass";
        }

        private void modeProcesar(DataGridViewRow row)
        {
            row.Cells["colCheck"].Value = false;
            row.DefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#a4e8a4");
            int linea = int.Parse(row.Cells["colIntLine"].Value.ToString());

            if (Config.byPassLine.Count > 0)
            {
                int index = Config.byPassLine.IndexOf(linea);
                if (index > -1)
                {
                    Config.byPassLine.RemoveAt(index);
                    row.Cells["colByPass"].Value = "Procesar";
                }
            } 
        }

        /**** BYPASS ****/
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.byPassLine = new List<int>();
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                modeByPass(row);
            }
        }
        private void rnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                if (row.Cells["colTipo"].Value.Equals("R"))
                {
                    modeByPass(row);
                }
            }
        }
        private void vtwinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in gridLine.Rows)
            {
                if (row.Cells["colTipo"].Value.Equals("W"))
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
