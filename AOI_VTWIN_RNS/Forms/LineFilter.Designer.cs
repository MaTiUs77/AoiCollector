namespace CollectorPackage
{
    partial class LineFilter
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
            this.gridLine = new System.Windows.Forms.DataGridView();
            this.colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.idCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLinea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTipo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colByPass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.byPassallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vtwinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verTodasLasMaquinasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zENITHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridLine)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridLine
            // 
            this.gridLine.AllowUserToAddRows = false;
            this.gridLine.AllowUserToDeleteRows = false;
            this.gridLine.AllowUserToResizeColumns = false;
            this.gridLine.AllowUserToResizeRows = false;
            this.gridLine.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLine.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCheck,
            this.idCol,
            this.colLinea,
            this.colTipo,
            this.colByPass});
            this.gridLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLine.Location = new System.Drawing.Point(0, 24);
            this.gridLine.MultiSelect = false;
            this.gridLine.Name = "gridLine";
            this.gridLine.ReadOnly = true;
            this.gridLine.RowHeadersVisible = false;
            this.gridLine.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridLine.Size = new System.Drawing.Size(259, 418);
            this.gridLine.TabIndex = 0;
            this.gridLine.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridLine_CellContentClick);
            // 
            // colCheck
            // 
            this.colCheck.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colCheck.HeaderText = "";
            this.colCheck.Name = "colCheck";
            this.colCheck.ReadOnly = true;
            this.colCheck.Width = 5;
            // 
            // idCol
            // 
            this.idCol.HeaderText = "id";
            this.idCol.Name = "idCol";
            this.idCol.ReadOnly = true;
            this.idCol.Visible = false;
            // 
            // colLinea
            // 
            this.colLinea.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLinea.HeaderText = "Linea";
            this.colLinea.Name = "colLinea";
            this.colLinea.ReadOnly = true;
            this.colLinea.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colLinea.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colTipo
            // 
            this.colTipo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colTipo.HeaderText = "Tipo";
            this.colTipo.Name = "colTipo";
            this.colTipo.ReadOnly = true;
            this.colTipo.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colTipo.Width = 53;
            // 
            // colByPass
            // 
            this.colByPass.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colByPass.HeaderText = "ByPass";
            this.colByPass.Name = "colByPass";
            this.colByPass.ReadOnly = true;
            this.colByPass.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colByPass.Width = 67;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.byPassallToolStripMenuItem,
            this.verTodasLasMaquinasToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(259, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // byPassallToolStripMenuItem
            // 
            this.byPassallToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.rnsToolStripMenuItem,
            this.vtwinToolStripMenuItem,
            this.zENITHToolStripMenuItem});
            this.byPassallToolStripMenuItem.Name = "byPassallToolStripMenuItem";
            this.byPassallToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.byPassallToolStripMenuItem.Text = "ByPass (all)";
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.allToolStripMenuItem.Text = "All";
            this.allToolStripMenuItem.Click += new System.EventHandler(this.allToolStripMenuItem_Click);
            // 
            // rnsToolStripMenuItem
            // 
            this.rnsToolStripMenuItem.Name = "rnsToolStripMenuItem";
            this.rnsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rnsToolStripMenuItem.Text = "RNS";
            this.rnsToolStripMenuItem.Click += new System.EventHandler(this.rnsToolStripMenuItem_Click);
            // 
            // vtwinToolStripMenuItem
            // 
            this.vtwinToolStripMenuItem.Name = "vtwinToolStripMenuItem";
            this.vtwinToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.vtwinToolStripMenuItem.Text = "VTWIN";
            this.vtwinToolStripMenuItem.Click += new System.EventHandler(this.vtwinToolStripMenuItem_Click);
            // 
            // verTodasLasMaquinasToolStripMenuItem
            // 
            this.verTodasLasMaquinasToolStripMenuItem.Name = "verTodasLasMaquinasToolStripMenuItem";
            this.verTodasLasMaquinasToolStripMenuItem.Size = new System.Drawing.Size(128, 20);
            this.verTodasLasMaquinasToolStripMenuItem.Text = "Remover ByPass (all)";
            this.verTodasLasMaquinasToolStripMenuItem.Click += new System.EventHandler(this.verTodasLasMaquinasToolStripMenuItem_Click);
            // 
            // zENITHToolStripMenuItem
            // 
            this.zENITHToolStripMenuItem.Name = "zENITHToolStripMenuItem";
            this.zENITHToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.zENITHToolStripMenuItem.Text = "ZENITH";
            this.zENITHToolStripMenuItem.Click += new System.EventHandler(this.zenithToolStripMenuItem_Click);
            // 
            // LineFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(259, 442);
            this.Controls.Add(this.gridLine);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LineFilter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ByPass Aoi";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LineFilter_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridLine)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridLine;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem verTodasLasMaquinasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byPassallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rnsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vtwinToolStripMenuItem;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn idCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLinea;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTipo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colByPass;
        private System.Windows.Forms.ToolStripMenuItem zENITHToolStripMenuItem;
    }
}