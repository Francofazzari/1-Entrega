namespace Entrega1software
{
    partial class FormUsuarios
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
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.txtTerminal = new System.Windows.Forms.TextBox();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.cmbPerfil = new System.Windows.Forms.ComboBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnModificar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.lblTerminal = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblApellido = new System.Windows.Forms.Label();
            this.lblClave = new System.Windows.Forms.Label();
            this.lblPerfil = new System.Windows.Forms.Label();
            this.panelForm = new System.Windows.Forms.Panel();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();

            // panelForm (izquierda)
            this.panelForm.Location = new System.Drawing.Point(10, 10);
            this.panelForm.Size = new System.Drawing.Size(280, 420);
            this.panelForm.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
            this.panelForm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelForm.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTerminal, this.txtTerminal,
                this.lblNombre,   this.txtNombre,
                this.lblApellido, this.txtApellido,
                this.lblClave,    this.txtClave,
                this.lblPerfil,   this.cmbPerfil,
                this.btnAgregar,  this.btnModificar, this.btnEliminar
            });

            // Labels y TextBoxes
            this.lblTerminal.Text = "Nro. Terminal";
            this.lblTerminal.Location = new System.Drawing.Point(10, 15);
            this.lblTerminal.Size = new System.Drawing.Size(120, 18);
            this.txtTerminal.Location = new System.Drawing.Point(10, 35);
            this.txtTerminal.Size = new System.Drawing.Size(255, 26);
            this.txtTerminal.Name = "txtTerminal";

            this.lblNombre.Text = "Nombre";
            this.lblNombre.Location = new System.Drawing.Point(10, 70);
            this.lblNombre.Size = new System.Drawing.Size(120, 18);
            this.txtNombre.Location = new System.Drawing.Point(10, 90);
            this.txtNombre.Size = new System.Drawing.Size(255, 26);
            this.txtNombre.Name = "txtNombre";

            this.lblApellido.Text = "Apellido";
            this.lblApellido.Location = new System.Drawing.Point(10, 125);
            this.lblApellido.Size = new System.Drawing.Size(120, 18);
            this.txtApellido.Location = new System.Drawing.Point(10, 145);
            this.txtApellido.Size = new System.Drawing.Size(255, 26);
            this.txtApellido.Name = "txtApellido";

            this.lblClave.Text = "Contrasena";
            this.lblClave.Location = new System.Drawing.Point(10, 180);
            this.lblClave.Size = new System.Drawing.Size(120, 18);
            this.txtClave.Location = new System.Drawing.Point(10, 200);
            this.txtClave.Size = new System.Drawing.Size(255, 26);
            this.txtClave.PasswordChar = '*';
            this.txtClave.Name = "txtClave";

            this.lblPerfil.Text = "Perfil";
            this.lblPerfil.Location = new System.Drawing.Point(10, 235);
            this.lblPerfil.Size = new System.Drawing.Size(120, 18);
            this.cmbPerfil.Location = new System.Drawing.Point(10, 255);
            this.cmbPerfil.Size = new System.Drawing.Size(255, 26);
            this.cmbPerfil.Name = "cmbPerfil";
            this.cmbPerfil.Items.AddRange(new object[] { "Admin", "Operador" });
            this.cmbPerfil.SelectedIndex = 1;
            this.cmbPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            // Botones
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.Location = new System.Drawing.Point(10, 300);
            this.btnAgregar.Size = new System.Drawing.Size(120, 32);
            this.btnAgregar.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);

            this.btnModificar.Text = "Modificar";
            this.btnModificar.Location = new System.Drawing.Point(145, 300);
            this.btnModificar.Size = new System.Drawing.Size(120, 32);
            this.btnModificar.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnModificar.ForeColor = System.Drawing.Color.White;
            this.btnModificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModificar.Name = "btnModificar";
            this.btnModificar.Click += new System.EventHandler(this.btnModificar_Click);

            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Location = new System.Drawing.Point(10, 345);
            this.btnEliminar.Size = new System.Drawing.Size(255, 32);
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(163, 45, 45);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);

            // dgvUsuarios (derecha)
            this.dgvUsuarios.Location = new System.Drawing.Point(305, 10);
            this.dgvUsuarios.Size = new System.Drawing.Size(560, 420);
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.BackgroundColor = System.Drawing.Color.White;
            this.dgvUsuarios.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.dgvUsuarios.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUsuarios_CellClick);

            // FormUsuarios
            this.ClientSize = new System.Drawing.Size(880, 450);
            this.Controls.Add(this.panelForm);
            this.Controls.Add(this.dgvUsuarios);
            this.Text = "Gestion de Usuarios";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormUsuarios";
            this.panelForm.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.TextBox txtTerminal;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.ComboBox cmbPerfil;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnModificar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Label lblTerminal;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.Label lblPerfil;
        private System.Windows.Forms.Panel panelForm;
    }
}