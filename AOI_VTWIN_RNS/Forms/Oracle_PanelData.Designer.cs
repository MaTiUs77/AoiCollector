namespace CollectorPackage
{
    partial class Oracle_PanelData
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.logGeneral = new System.Windows.Forms.RichTextBox();
            this.gridOracle = new System.Windows.Forms.DataGridView();
            this.inCodigo = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOracle)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.logGeneral, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.gridOracle, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.inCodigo, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.80851F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78.19149F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 282F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(951, 483);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // logGeneral
            // 
            this.logGeneral.BackColor = System.Drawing.Color.Black;
            this.logGeneral.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.logGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logGeneral.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logGeneral.ForeColor = System.Drawing.Color.White;
            this.logGeneral.Location = new System.Drawing.Point(3, 203);
            this.logGeneral.Name = "logGeneral";
            this.logGeneral.ReadOnly = true;
            this.logGeneral.Size = new System.Drawing.Size(945, 277);
            this.logGeneral.TabIndex = 8;
            this.logGeneral.Text = "";
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
            this.gridOracle.Location = new System.Drawing.Point(3, 46);
            this.gridOracle.Name = "gridOracle";
            this.gridOracle.ReadOnly = true;
            this.gridOracle.RowHeadersVisible = false;
            this.gridOracle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridOracle.Size = new System.Drawing.Size(945, 151);
            this.gridOracle.TabIndex = 7;
            // 
            // inCodigo
            // 
            this.inCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inCodigo.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inCodigo.Location = new System.Drawing.Point(3, 3);
            this.inCodigo.Name = "inCodigo";
            this.inCodigo.Size = new System.Drawing.Size(945, 35);
            this.inCodigo.TabIndex = 0;
            this.inCodigo.KeyUp += new System.Windows.Forms.KeyEventHandler(this.inCodigo_KeyUp);
            // 
            // Oracle_PanelData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 483);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Oracle_PanelData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ingresar codigo de placa";
            this.Load += new System.EventHandler(this.TestOracle_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOracle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView gridOracle;
        private System.Windows.Forms.TextBox inCodigo;
        private System.Windows.Forms.RichTextBox logGeneral;
    }
}