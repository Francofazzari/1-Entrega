namespace Entrega1software
{
    partial class FormBackupRestore
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.pnlBackup = new System.Windows.Forms.Panel();
            this.lblBackupTitulo = new System.Windows.Forms.Label();
            this.lblBackupDesc = new System.Windows.Forms.Label();
            this.txtRutaBackup = new System.Windows.Forms.TextBox();
            this.btnExaminarBackup = Estilos.BotonSecundario("Examinar...", Estilos.AzulMedio, 9.5F);
            this.btnBackup = Estilos.Boton("Realizar backup", Estilos.Verde, 10F);
            this.pnlRestore = new System.Windows.Forms.Panel();
            this.lblRestoreTitulo = new System.Windows.Forms.Label();
            this.lblRestoreDesc = new System.Windows.Forms.Label();
            this.txtRutaRestore = new System.Windows.Forms.TextBox();
            this.btnExaminarRestore = Estilos.BotonSecundario("Examinar...", Estilos.AzulMedio, 9.5F);
            this.btnRestaurar = Estilos.Boton("Restaurar backup", Estilos.Rojo, 10F);
            this.lblNota = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.pnlBackup.SuspendLayout();
            this.pnlRestore.SuspendLayout();
            this.SuspendLayout();

            // ================= encabezado =================
            this.panelTop.BackColor = Estilos.Azul;
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 66;
            this.panelTop.Controls.Add(this.lblTitulo);
            this.panelTop.Controls.Add(this.lblSubtitulo);
            this.panelTop.Name = "panelTop";

            this.lblTitulo.Text = "Backup y restore de la base de datos";
            this.lblTitulo.Font = Estilos.Negrita(13F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Location = new System.Drawing.Point(14, 8);
            this.lblTitulo.Name = "lblTitulo";

            // el texto (base y servidor) se completa en el constructor
            this.lblSubtitulo.Font = Estilos.Fuente(9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(190, 210, 230);
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Location = new System.Drawing.Point(16, 38);
            this.lblSubtitulo.Name = "lblSubtitulo";

            // ================= backup =================
            this.pnlBackup.BackColor = Estilos.Superficie;
            this.pnlBackup.Location = new System.Drawing.Point(16, 82);
            this.pnlBackup.Size = new System.Drawing.Size(668, 152);
            this.pnlBackup.Controls.Add(this.lblBackupTitulo);
            this.pnlBackup.Controls.Add(this.lblBackupDesc);
            this.pnlBackup.Controls.Add(this.txtRutaBackup);
            this.pnlBackup.Controls.Add(this.btnExaminarBackup);
            this.pnlBackup.Controls.Add(this.btnBackup);
            this.pnlBackup.Name = "pnlBackup";

            this.lblBackupTitulo.Text = "Realizar backup";
            this.lblBackupTitulo.Font = Estilos.Negrita(11F);
            this.lblBackupTitulo.ForeColor = Estilos.Texto;
            this.lblBackupTitulo.AutoSize = true;
            this.lblBackupTitulo.Location = new System.Drawing.Point(14, 12);
            this.lblBackupTitulo.Name = "lblBackupTitulo";

            this.lblBackupDesc.Text = "Genera un archivo .bak con una copia completa de la base de datos.";
            this.lblBackupDesc.Font = Estilos.Fuente(9.5F);
            this.lblBackupDesc.ForeColor = Estilos.TextoSuave;
            this.lblBackupDesc.AutoSize = true;
            this.lblBackupDesc.Location = new System.Drawing.Point(15, 38);
            this.lblBackupDesc.Name = "lblBackupDesc";

            this.txtRutaBackup.Font = Estilos.Fuente(10F);
            this.txtRutaBackup.Location = new System.Drawing.Point(16, 68);
            this.txtRutaBackup.Size = new System.Drawing.Size(506, 25);
            this.txtRutaBackup.Name = "txtRutaBackup";

            this.btnExaminarBackup.Location = new System.Drawing.Point(532, 66);
            this.btnExaminarBackup.Size = new System.Drawing.Size(120, 30);
            this.btnExaminarBackup.Name = "btnExaminarBackup";
            this.btnExaminarBackup.Click += new System.EventHandler(this.btnExaminarBackup_Click);

            this.btnBackup.Location = new System.Drawing.Point(16, 106);
            this.btnBackup.Size = new System.Drawing.Size(190, 34);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);

            // ================= restore =================
            this.pnlRestore.BackColor = Estilos.Superficie;
            this.pnlRestore.Location = new System.Drawing.Point(16, 248);
            this.pnlRestore.Size = new System.Drawing.Size(668, 152);
            this.pnlRestore.Controls.Add(this.lblRestoreTitulo);
            this.pnlRestore.Controls.Add(this.lblRestoreDesc);
            this.pnlRestore.Controls.Add(this.txtRutaRestore);
            this.pnlRestore.Controls.Add(this.btnExaminarRestore);
            this.pnlRestore.Controls.Add(this.btnRestaurar);
            this.pnlRestore.Name = "pnlRestore";

            this.lblRestoreTitulo.Text = "Restaurar backup";
            this.lblRestoreTitulo.Font = Estilos.Negrita(11F);
            this.lblRestoreTitulo.ForeColor = Estilos.Texto;
            this.lblRestoreTitulo.AutoSize = true;
            this.lblRestoreTitulo.Location = new System.Drawing.Point(14, 12);
            this.lblRestoreTitulo.Name = "lblRestoreTitulo";

            this.lblRestoreDesc.Text = "Reemplaza TODOS los datos actuales por los del backup. Después hay que volver a iniciar sesión.";
            this.lblRestoreDesc.Font = Estilos.Fuente(9.5F);
            this.lblRestoreDesc.ForeColor = Estilos.Rojo;
            this.lblRestoreDesc.AutoSize = true;
            this.lblRestoreDesc.Location = new System.Drawing.Point(15, 38);
            this.lblRestoreDesc.Name = "lblRestoreDesc";

            this.txtRutaRestore.Font = Estilos.Fuente(10F);
            this.txtRutaRestore.Location = new System.Drawing.Point(16, 68);
            this.txtRutaRestore.Size = new System.Drawing.Size(506, 25);
            this.txtRutaRestore.Name = "txtRutaRestore";

            this.btnExaminarRestore.Location = new System.Drawing.Point(532, 66);
            this.btnExaminarRestore.Size = new System.Drawing.Size(120, 30);
            this.btnExaminarRestore.Name = "btnExaminarRestore";
            this.btnExaminarRestore.Click += new System.EventHandler(this.btnExaminarRestore_Click);

            this.btnRestaurar.Location = new System.Drawing.Point(16, 106);
            this.btnRestaurar.Size = new System.Drawing.Size(190, 34);
            this.btnRestaurar.Name = "btnRestaurar";
            this.btnRestaurar.Click += new System.EventHandler(this.btnRestaurar_Click);

            // ================= nota =================
            // el texto (con la carpeta por defecto) se completa en el constructor
            this.lblNota.Font = Estilos.Fuente(8.75F);
            this.lblNota.ForeColor = Estilos.TextoSuave;
            this.lblNota.Location = new System.Drawing.Point(16, 410);
            this.lblNota.Size = new System.Drawing.Size(668, 48);
            this.lblNota.Name = "lblNota";

            // FormBackupRestore
            this.ClientSize = new System.Drawing.Size(700, 466);
            this.Controls.Add(this.lblNota);
            this.Controls.Add(this.pnlRestore);
            this.Controls.Add(this.pnlBackup);
            this.Controls.Add(this.panelTop);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Backup / Restore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "FormBackupRestore";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.pnlBackup.ResumeLayout(false);
            this.pnlBackup.PerformLayout();
            this.pnlRestore.ResumeLayout(false);
            this.pnlRestore.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel pnlBackup;
        private System.Windows.Forms.Label lblBackupTitulo;
        private System.Windows.Forms.Label lblBackupDesc;
        private System.Windows.Forms.TextBox txtRutaBackup;
        private System.Windows.Forms.Button btnExaminarBackup;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.Panel pnlRestore;
        private System.Windows.Forms.Label lblRestoreTitulo;
        private System.Windows.Forms.Label lblRestoreDesc;
        private System.Windows.Forms.TextBox txtRutaRestore;
        private System.Windows.Forms.Button btnExaminarRestore;
        private System.Windows.Forms.Button btnRestaurar;
        private System.Windows.Forms.Label lblNota;
    }
}
