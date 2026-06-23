namespace Entrega1software
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblUsuario = new System.Windows.Forms.Label();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnPerfiles = new System.Windows.Forms.Button();
            this.btnVentas = new System.Windows.Forms.Button();
            this.btnReporte = new System.Windows.Forms.Button();
            this.btnApuesta = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnBitacora = new System.Windows.Forms.Button();
            this.btnCambios = new System.Windows.Forms.Button();
            this.btnIdiomas = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 100;
            this.panelTop.Controls.Add(this.lblUsuario);
            this.panelTop.Controls.Add(this.btnUsuarios);
            this.panelTop.Controls.Add(this.btnLogout);
            this.panelTop.Controls.Add(this.btnPerfiles);
            this.panelTop.Controls.Add(this.btnVentas);
            this.panelTop.Controls.Add(this.btnReporte);
            this.panelTop.Controls.Add(this.btnApuesta);
            this.panelTop.Controls.Add(this.btnBitacora);
            this.panelTop.Controls.Add(this.btnCambios);
            this.panelTop.Controls.Add(this.btnIdiomas);

            // lblUsuario
            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Font = new System.Drawing.Font("Arial", 10F);
            this.lblUsuario.Location = new System.Drawing.Point(10, 14);
            this.lblUsuario.Size = new System.Drawing.Size(300, 22);
            this.lblUsuario.Name = "lblUsuario";

            // btnUsuarios
            this.btnUsuarios.Text = "Gestion Usuarios";
            this.btnUsuarios.Location = new System.Drawing.Point(10, 50);
            this.btnUsuarios.Size = new System.Drawing.Size(140, 30);
            this.btnUsuarios.Font = new System.Drawing.Font("Arial", 9F);
            this.btnUsuarios.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnUsuarios.ForeColor = System.Drawing.Color.White;
            this.btnUsuarios.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);

            // btnPerfiles
            this.btnPerfiles.Text = "Perfiles";
            this.btnPerfiles.Location = new System.Drawing.Point(160, 50);
            this.btnPerfiles.Size = new System.Drawing.Size(120, 30);
            this.btnPerfiles.Font = new System.Drawing.Font("Arial", 9F);
            this.btnPerfiles.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnPerfiles.ForeColor = System.Drawing.Color.White;
            this.btnPerfiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPerfiles.Name = "btnPerfiles";
            this.btnPerfiles.Click += new System.EventHandler(this.btnPerfiles_Click);

            // btnVentas
            this.btnVentas.Text = "Ventas en Linea";
            this.btnVentas.Location = new System.Drawing.Point(290, 50);
            this.btnVentas.Size = new System.Drawing.Size(130, 30);
            this.btnVentas.Font = new System.Drawing.Font("Arial", 9F);
            this.btnVentas.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnVentas.ForeColor = System.Drawing.Color.White;
            this.btnVentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);

            // btnReporte
            this.btnReporte.Text = "Reporte Semanal";
            this.btnReporte.Location = new System.Drawing.Point(430, 50);
            this.btnReporte.Size = new System.Drawing.Size(130, 30);
            this.btnReporte.Font = new System.Drawing.Font("Arial", 9F);
            this.btnReporte.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnReporte.ForeColor = System.Drawing.Color.White;
            this.btnReporte.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReporte.Name = "btnReporte";
            this.btnReporte.Click += new System.EventHandler(this.btnReporte_Click);

            // btnApuesta
            this.btnApuesta.Text = "Cargar Apuesta";
            this.btnApuesta.Location = new System.Drawing.Point(570, 50);
            this.btnApuesta.Size = new System.Drawing.Size(130, 30);
            this.btnApuesta.Font = new System.Drawing.Font("Arial", 9F);
            this.btnApuesta.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnApuesta.ForeColor = System.Drawing.Color.White;
            this.btnApuesta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApuesta.Name = "btnApuesta";
            this.btnApuesta.Click += new System.EventHandler(this.btnApuesta_Click);
            
            // btnIdiomas
            this.btnIdiomas.Text = "Gestion Idiomas";
            this.btnIdiomas.Location = new System.Drawing.Point(710, 50);
            this.btnIdiomas.Size = new System.Drawing.Size(130, 30);
            this.btnIdiomas.Font = new System.Drawing.Font("Arial", 9F);
            this.btnIdiomas.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.btnIdiomas.ForeColor = System.Drawing.Color.White;
            this.btnIdiomas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIdiomas.Name = "btnIdiomas";
            this.btnIdiomas.Click += new System.EventHandler(this.btnIdiomas_Click);

            // btnBitacora
            this.btnBitacora.Text = "Bitacora";
            this.btnBitacora.Location = new System.Drawing.Point(850, 50);
            this.btnBitacora.Size = new System.Drawing.Size(100, 30);
            this.btnBitacora.Font = new System.Drawing.Font("Arial", 9F);
            this.btnBitacora.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.btnBitacora.ForeColor = System.Drawing.Color.White;
            this.btnBitacora.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBitacora.Name = "btnBitacora";
            this.btnBitacora.Click += new System.EventHandler(this.btnBitacora_Click);

            // btnCambios
            this.btnCambios.Text = "Control Cambios";
            this.btnCambios.Location = new System.Drawing.Point(960, 50);
            this.btnCambios.Size = new System.Drawing.Size(130, 30);
            this.btnCambios.Font = new System.Drawing.Font("Arial", 9F);
            this.btnCambios.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.btnCambios.ForeColor = System.Drawing.Color.White;
            this.btnCambios.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCambios.Name = "btnCambios";
            this.btnCambios.Click += new System.EventHandler(this.btnCambios_Click);

            // btnLogout
            this.btnLogout.Text = "Cerrar sesion";
            this.btnLogout.Location = new System.Drawing.Point(1100, 50);
            this.btnLogout.Size = new System.Drawing.Size(120, 30);
            this.btnLogout.Font = new System.Drawing.Font("Arial", 9F);
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(163, 45, 45);
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // FormPrincipal
            this.ClientSize = new System.Drawing.Size(1400, 600);
            this.Controls.Add(this.panelTop);
            this.Text = "Sistema Agencia de Quiniela";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Name = "FormPrincipal";
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnPerfiles;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnReporte;
        private System.Windows.Forms.Button btnApuesta;
        private System.Windows.Forms.Button btnBitacora;
        private System.Windows.Forms.Button btnCambios;
        private System.Windows.Forms.Button btnIdiomas;

    }
}
