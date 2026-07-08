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
            this.cmbIdiomas = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtTerminal
            // 
            this.txtTerminal.Font = new System.Drawing.Font("Arial", 10F);
            this.txtTerminal.Location = new System.Drawing.Point(80, 112);
            this.txtTerminal.Name = "txtTerminal";
            this.txtTerminal.Size = new System.Drawing.Size(320, 27);
            this.txtTerminal.TabIndex = 2;
            // 
            // txtClave
            // 
            this.txtClave.Font = new System.Drawing.Font("Arial", 10F);
            this.txtClave.Location = new System.Drawing.Point(80, 177);
            this.txtClave.Name = "txtClave";
            this.txtClave.PasswordChar = '*';
            this.txtClave.Size = new System.Drawing.Size(320, 27);
            this.txtClave.TabIndex = 4;
            // 
            // btnIngresar
            // 
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(78)))), ((int)(((byte)(121)))));
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.Location = new System.Drawing.Point(80, 230);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(320, 38);
            this.btnIngresar.TabIndex = 5;
            this.btnIngresar.Text = "xIngresar al sistema";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
            // 
            // lblTerminal
            // 
            this.lblTerminal.Font = new System.Drawing.Font("Arial", 9F);
            this.lblTerminal.Location = new System.Drawing.Point(80, 90);
            this.lblTerminal.Name = "lblTerminal";
            this.lblTerminal.Size = new System.Drawing.Size(150, 20);
            this.lblTerminal.TabIndex = 1;
            this.lblTerminal.Text = "Número de terminal";
            // 
            // lblClave
            // 
            this.lblClave.Font = new System.Drawing.Font("Arial", 9F);
            this.lblClave.Location = new System.Drawing.Point(80, 155);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(150, 20);
            this.lblClave.TabIndex = 3;
            this.lblClave.Text = "Contraseña";
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(78)))), ((int)(((byte)(121)))));
            this.lblTitulo.Location = new System.Drawing.Point(60, 30);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(360, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Sistema Agencia de Quiniela";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbIdiomas
            // 
            this.cmbIdiomas.FormattingEnabled = true;
            this.cmbIdiomas.Location = new System.Drawing.Point(184, 284);
            this.cmbIdiomas.Name = "cmbIdiomas";
            this.cmbIdiomas.Size = new System.Drawing.Size(121, 24);
            this.cmbIdiomas.TabIndex = 6;
            this.cmbIdiomas.SelectedIndexChanged += new System.EventHandler(this.cmbIdiomas_SelectedIndexChanged_1);
            // 
            // FormLogin
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(480, 320);
            this.Controls.Add(this.cmbIdiomas);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblTerminal);
            this.Controls.Add(this.txtTerminal);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.btnIngresar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login - Agencia de Quiniela";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtTerminal;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Button btnIngresar;
        private System.Windows.Forms.Label lblTerminal;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.ComboBox cmbIdiomas;
    }
}