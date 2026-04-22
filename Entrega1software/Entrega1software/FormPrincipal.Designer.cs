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
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 50;
            this.panelTop.Controls.Add(this.lblUsuario);
            this.panelTop.Controls.Add(this.btnUsuarios);
            this.panelTop.Controls.Add(this.btnLogout);

            // lblUsuario
            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Font = new System.Drawing.Font("Arial", 10F);
            this.lblUsuario.Location = new System.Drawing.Point(10, 14);
            this.lblUsuario.Size = new System.Drawing.Size(300, 22);
            this.lblUsuario.Name = "lblUsuario";

            // btnUsuarios
            this.btnUsuarios.Text = "Gestión Usuarios";
            this.btnUsuarios.Location = new System.Drawing.Point(320, 10);
            this.btnUsuarios.Size = new System.Drawing.Size(140, 30);
            this.btnUsuarios.Font = new System.Drawing.Font("Arial", 9F);
            this.btnUsuarios.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnUsuarios.ForeColor = System.Drawing.Color.White;
            this.btnUsuarios.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);

            // btnLogout
            this.btnLogout.Text = "Cerrar sesión";
            this.btnLogout.Location = new System.Drawing.Point(470, 10);
            this.btnLogout.Size = new System.Drawing.Size(120, 30);
            this.btnLogout.Font = new System.Drawing.Font("Arial", 9F);
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(163, 45, 45);
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // FormPrincipal
            this.ClientSize = new System.Drawing.Size(900, 600);
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
    }
}