namespace Entrega1software
{
    partial class FormLogin
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
            this.txtTerminal = new System.Windows.Forms.TextBox();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.btnIngresar = new System.Windows.Forms.Button();
            this.lblTerminal = new System.Windows.Forms.Label();
            this.lblClave = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblTitulo
            this.lblTitulo.Text = "Sistema Agencia de Quiniela";
            this.lblTitulo.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTitulo.Location = new System.Drawing.Point(60, 30);
            this.lblTitulo.Size = new System.Drawing.Size(360, 30);
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblTerminal
            this.lblTerminal.Text = "Número de terminal";
            this.lblTerminal.Font = new System.Drawing.Font("Arial", 9F);
            this.lblTerminal.Location = new System.Drawing.Point(80, 90);
            this.lblTerminal.Size = new System.Drawing.Size(150, 20);

            // txtTerminal
            this.txtTerminal.Location = new System.Drawing.Point(80, 112);
            this.txtTerminal.Size = new System.Drawing.Size(320, 26);
            this.txtTerminal.Font = new System.Drawing.Font("Arial", 10F);
            this.txtTerminal.Name = "txtTerminal";

            // lblClave
            this.lblClave.Text = "Contraseña";
            this.lblClave.Font = new System.Drawing.Font("Arial", 9F);
            this.lblClave.Location = new System.Drawing.Point(80, 155);
            this.lblClave.Size = new System.Drawing.Size(150, 20);

            // txtClave
            this.txtClave.Location = new System.Drawing.Point(80, 177);
            this.txtClave.Size = new System.Drawing.Size(320, 26);
            this.txtClave.Font = new System.Drawing.Font("Arial", 10F);
            this.txtClave.PasswordChar = '*';
            this.txtClave.Name = "txtClave";

            // btnIngresar
            this.btnIngresar.Text = "Ingresar al sistema";
            this.btnIngresar.Location = new System.Drawing.Point(80, 230);
            this.btnIngresar.Size = new System.Drawing.Size(320, 38);
            this.btnIngresar.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);

            // FormLogin
            this.ClientSize = new System.Drawing.Size(480, 320);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblTerminal);
            this.Controls.Add(this.txtTerminal);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.btnIngresar);
            this.Text = "Login - Agencia de Quiniela";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormLogin";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox txtTerminal;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Button btnIngresar;
        private System.Windows.Forms.Label lblTerminal;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.Label lblTitulo;
    }
}