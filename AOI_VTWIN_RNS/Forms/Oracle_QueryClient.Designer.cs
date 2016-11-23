namespace CollectorPackage
{
    partial class Oracle_QueryClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gridOracle = new System.Windows.Forms.DataGridView();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ejecutarQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.querysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inspeccionesDeLineaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detalleDeInspeccionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.detalleAgrupadoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridOracle)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridOracle
            // 
            this.gridOracle.AllowUserToAddRows = false;
            this.gridOracle.AllowUserToDeleteRows = false;
            this.gridOracle.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.gridOracle.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gridOracle.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridOracle.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridOracle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOracle.Location = new System.Drawing.Point(0, 0);
            this.gridOracle.Name = "gridOracle";
            this.gridOracle.ReadOnly = true;
            this.gridOracle.RowHeadersVisible = false;
            this.gridOracle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridOracle.Size = new System.Drawing.Size(951, 286);
            this.gridOracle.TabIndex = 0;
            this.gridOracle.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridOracle_CellContentClick);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(951, 169);
            this.textBox1.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ejecutarQueryToolStripMenuItem,
            this.querysToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(951, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ejecutarQueryToolStripMenuItem
            // 
            this.ejecutarQueryToolStripMenuItem.Name = "ejecutarQueryToolStripMenuItem";
            this.ejecutarQueryToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.ejecutarQueryToolStripMenuItem.Size = new System.Drawing.Size(94, 20);
            this.ejecutarQueryToolStripMenuItem.Text = "Ejecutar query";
            this.ejecutarQueryToolStripMenuItem.Click += new System.EventHandler(this.ejecutarQueryToolStripMenuItem_Click);
            // 
            // querysToolStripMenuItem
            // 
            this.querysToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inspeccionesDeLineaToolStripMenuItem,
            this.detalleDeInspeccionToolStripMenuItem,
            this.detalleAgrupadoToolStripMenuItem});
            this.querysToolStripMenuItem.Name = "querysToolStripMenuItem";
            this.querysToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.querysToolStripMenuItem.Text = "Querys";
            // 
            // inspeccionesDeLineaToolStripMenuItem
            // 
            this.inspeccionesDeLineaToolStripMenuItem.Name = "inspeccionesDeLineaToolStripMenuItem";
            this.inspeccionesDeLineaToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.inspeccionesDeLineaToolStripMenuItem.Text = "Inspecciones de linea";
            this.inspeccionesDeLineaToolStripMenuItem.Click += new System.EventHandler(this.inspeccionesDeLineaToolStripMenuItem_Click);
            // 
            // detalleDeInspeccionToolStripMenuItem
            // 
            this.detalleDeInspeccionToolStripMenuItem.Name = "detalleDeInspeccionToolStripMenuItem";
            this.detalleDeInspeccionToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.detalleDeInspeccionToolStripMenuItem.Text = "Detalle de inspeccion";
            this.detalleDeInspeccionToolStripMenuItem.Click += new System.EventHandler(this.detalleDeInspeccionToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gridOracle);
            this.splitContainer1.Size = new System.Drawing.Size(951, 459);
            this.splitContainer1.SplitterDistance = 169;
            this.splitContainer1.TabIndex = 3;
            // 
            // detalleAgrupadoToolStripMenuItem
            // 
            this.detalleAgrupadoToolStripMenuItem.Name = "detalleAgrupadoToolStripMenuItem";
            this.detalleAgrupadoToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.detalleAgrupadoToolStripMenuItem.Text = "Detalle agrupado";
            this.detalleAgrupadoToolStripMenuItem.Click += new System.EventHandler(this.detalleAgrupadoToolStripMenuItem_Click);
            // 
            // TestOracle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 483);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TestOracle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TestOracle";
            this.Load += new System.EventHandler(this.TestOracle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridOracle)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridOracle;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ejecutarQueryToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem querysToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inspeccionesDeLineaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detalleDeInspeccionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detalleAgrupadoToolStripMenuItem;
    }
}