namespace Entrega1software
{
    partial class FormBitacora
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvBitacora = new System.Windows.Forms.DataGridView();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.lblDesde = new System.Windows.Forms.Label();
            this.lblHasta = new System.Windows.Forms.Label();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).BeginInit();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 60;
            this.panelTop.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDesde, this.dtpDesde,
                this.lblHasta, this.dtpHasta,
                this.lblBuscar, this.txtBuscar,
                this.btnBuscar
            });

            // lblDesde
            this.lblDesde.Text = "Desde:";
            this.lblDesde.ForeColor = System.Drawing.Color.White;
            this.lblDesde.Font = new System.Drawing.Font("Arial", 9F);
            this.lblDesde.Location = new System.Drawing.Point(10, 20);
            this.lblDesde.Size = new System.Drawing.Size(50, 20);

            // dtpDesde
            this.dtpDesde.Location = new System.Drawing.Point(65, 17);
            this.dtpDesde.Size = new System.Drawing.Size(140, 26);
            this.dtpDesde.Font = new System.Drawing.Font("Arial", 9F);
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Name = "dtpDesde";

            // lblHasta
            this.lblHasta.Text = "Hasta:";
            this.lblHasta.ForeColor = System.Drawing.Color.White;
            this.lblHasta.Font = new System.Drawing.Font("Arial", 9F);
            this.lblHasta.Location = new System.Drawing.Point(215, 20);
            this.lblHasta.Size = new System.Drawing.Size(50, 20);

            // dtpHasta
            this.dtpHasta.Location = new System.Drawing.Point(265, 17);
            this.dtpHasta.Size = new System.Drawing.Size(140, 26);
            this.dtpHasta.Font = new System.Drawing.Font("Arial", 9F);
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Name = "dtpHasta";

            // lblBuscar
            this.lblBuscar.Text = "Buscar:";
            this.lblBuscar.ForeColor = System.Drawing.Color.White;
            this.lblBuscar.Font = new System.Drawing.Font("Arial", 9F);
            this.lblBuscar.Location = new System.Drawing.Point(415, 20);
            this.lblBuscar.Size = new System.Drawing.Size(55, 20);

            // txtBuscar
            this.txtBuscar.Location = new System.Drawing.Point(472, 17);
            this.txtBuscar.Size = new System.Drawing.Size(180, 26);
            this.txtBuscar.Font = new System.Drawing.Font("Arial", 9F);
            this.txtBuscar.Name = "txtBuscar";

            // btnBuscar
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Location = new System.Drawing.Point(662, 15);
            this.btnBuscar.Size = new System.Drawing.Size(100, 30);
            this.btnBuscar.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            // dgvBitacora
            this.dgvBitacora.Location = new System.Drawing.Point(10, 70);
            this.dgvBitacora.Size = new System.Drawing.Size(960, 450);
            this.dgvBitacora.Name = "dgvBitacora";
            this.dgvBitacora.ReadOnly = true;
            this.dgvBitacora.AllowUserToAddRows = false;
            this.dgvBitacora.AllowUserToDeleteRows = false;
            this.dgvBitacora.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBitacora.BackgroundColor = System.Drawing.Color.White;
            this.dgvBitacora.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvBitacora.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);

            // FormBitacora
            this.ClientSize = new System.Drawing.Size(980, 540);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.dgvBitacora);
            this.Text = "Bitacora del sistema";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormBitacora";
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvBitacora;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.Panel panelTop;
    }
}