namespace Entrega1software
{
    partial class FormApuesta
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblLoterias = new System.Windows.Forms.Label();
            this.lblTurnos = new System.Windows.Forms.Label();
            this.dgvJugadas = new System.Windows.Forms.DataGridView();
            this.pnlDerecha = new System.Windows.Forms.Panel();
            this.lblTipo = new System.Windows.Forms.Label();
            this.cmbTipo = new System.Windows.Forms.ComboBox();
            this.lblMonto = new System.Windows.Forms.Label();
            this.nudMonto = new System.Windows.Forms.NumericUpDown();
            this.lblNumero = new System.Windows.Forms.Label();
            this.txtNumero = new System.Windows.Forms.TextBox();
            this.btnAgregarNumero = new System.Windows.Forms.Button();
            this.lblRango = new System.Windows.Forms.Label();
            this.tlpRango = new System.Windows.Forms.TableLayoutPanel();
            this.tlpTeclado = new System.Windows.Forms.TableLayoutPanel();
            this.lblApuestaCaption = new System.Windows.Forms.Label();
            this.lblApuestaValor = new System.Windows.Forms.Label();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.lblTotalValor = new System.Windows.Forms.Label();
            this.btnEnviar = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).BeginInit();
            this.pnlDerecha.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMonto)).BeginInit();
            this.SuspendLayout();

            // pnlHeader (Dock=Top: siempre ocupa todo el ancho disponible)
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 85;
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.lblLoterias);
            this.pnlHeader.Controls.Add(this.lblTurnos);
            this.pnlHeader.Name = "pnlHeader";

            // lblTitulo
            this.lblTitulo.Text = "Quiniela";
            this.lblTitulo.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTitulo.Location = new System.Drawing.Point(15, 10);
            this.lblTitulo.Size = new System.Drawing.Size(500, 32);
            this.lblTitulo.Name = "lblTitulo";

            // lblLoterias
            this.lblLoterias.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.lblLoterias.Location = new System.Drawing.Point(15, 45);
            this.lblLoterias.Size = new System.Drawing.Size(600, 18);
            this.lblLoterias.Name = "lblLoterias";

            // lblTurnos
            this.lblTurnos.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Italic);
            this.lblTurnos.Location = new System.Drawing.Point(15, 63);
            this.lblTurnos.Size = new System.Drawing.Size(600, 18);
            this.lblTurnos.Name = "lblTurnos";

            // dgvJugadas (Dock=Fill: crece con el formulario en ambas direcciones)
            this.dgvJugadas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJugadas.Name = "dgvJugadas";
            this.dgvJugadas.ReadOnly = true;
            this.dgvJugadas.AllowUserToAddRows = false;
            this.dgvJugadas.AllowUserToDeleteRows = false;
            this.dgvJugadas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJugadas.BackgroundColor = System.Drawing.Color.White;
            this.dgvJugadas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // AutoSizeColumnsMode se setea en el constructor (FormApuesta.cs), despues de
            // asignar el DataSource: hacerlo aca, antes de que existan columnas y antes de
            // que el Dock=Fill resuelva el ancho real del control, dejaba las columnas con
            // ancho cero y la grilla se veia en blanco.
            this.dgvJugadas.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.dgvJugadas.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvJugadas.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);

            // pnlDerecha (Dock=Right: ancho fijo, con scroll si el alto no alcanza)
            this.pnlDerecha.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlDerecha.Width = 380;
            this.pnlDerecha.AutoScroll = true;
            this.pnlDerecha.Padding = new System.Windows.Forms.Padding(15);
            this.pnlDerecha.Controls.Add(this.lblTipo);
            this.pnlDerecha.Controls.Add(this.cmbTipo);
            this.pnlDerecha.Controls.Add(this.lblMonto);
            this.pnlDerecha.Controls.Add(this.nudMonto);
            this.pnlDerecha.Controls.Add(this.lblNumero);
            this.pnlDerecha.Controls.Add(this.txtNumero);
            this.pnlDerecha.Controls.Add(this.btnAgregarNumero);
            this.pnlDerecha.Controls.Add(this.lblRango);
            this.pnlDerecha.Controls.Add(this.tlpRango);
            this.pnlDerecha.Controls.Add(this.tlpTeclado);
            this.pnlDerecha.Controls.Add(this.lblApuestaCaption);
            this.pnlDerecha.Controls.Add(this.lblApuestaValor);
            this.pnlDerecha.Controls.Add(this.lblTotalCaption);
            this.pnlDerecha.Controls.Add(this.lblTotalValor);
            this.pnlDerecha.Controls.Add(this.btnEnviar);
            this.pnlDerecha.Name = "pnlDerecha";

            // lblTipo
            this.lblTipo.Text = "Tipo";
            this.lblTipo.Location = new System.Drawing.Point(15, 15);
            this.lblTipo.Size = new System.Drawing.Size(150, 18);
            this.lblTipo.Name = "lblTipo";

            // cmbTipo
            this.cmbTipo.Location = new System.Drawing.Point(15, 35);
            this.cmbTipo.Size = new System.Drawing.Size(170, 26);
            this.cmbTipo.Name = "cmbTipo";
            this.cmbTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTipo.Items.AddRange(new object[] { "Directa", "Redoblona" });
            this.cmbTipo.SelectedIndex = 0;

            // lblMonto
            this.lblMonto.Text = "Monto $ (min 100 - max 5000)";
            this.lblMonto.Location = new System.Drawing.Point(15, 75);
            this.lblMonto.Size = new System.Drawing.Size(250, 18);
            this.lblMonto.Name = "lblMonto";

            // nudMonto
            this.nudMonto.Location = new System.Drawing.Point(15, 95);
            this.nudMonto.Size = new System.Drawing.Size(170, 26);
            this.nudMonto.Name = "nudMonto";
            this.nudMonto.Minimum = 100;
            this.nudMonto.Maximum = 5000;
            this.nudMonto.Increment = 50;
            this.nudMonto.DecimalPlaces = 2;
            this.nudMonto.Value = 100;
            this.nudMonto.ThousandsSeparator = true;
            this.nudMonto.ValueChanged += new System.EventHandler(this.nudMonto_ValueChanged);

            // lblNumero
            this.lblNumero.Text = "Numero (1 a 4 cifras)";
            this.lblNumero.Location = new System.Drawing.Point(15, 135);
            this.lblNumero.Size = new System.Drawing.Size(180, 18);
            this.lblNumero.Name = "lblNumero";

            // lblRango
            this.lblRango.Text = "Rango";
            this.lblRango.Location = new System.Drawing.Point(200, 135);
            this.lblRango.Size = new System.Drawing.Size(150, 18);
            this.lblRango.Name = "lblRango";

            // txtNumero
            this.txtNumero.Location = new System.Drawing.Point(15, 155);
            this.txtNumero.Size = new System.Drawing.Size(90, 40);
            this.txtNumero.Name = "txtNumero";
            this.txtNumero.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.txtNumero.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtNumero.MaxLength = 4;
            this.txtNumero.ReadOnly = true;

            // btnAgregarNumero
            this.btnAgregarNumero.Text = "Agregar";
            this.btnAgregarNumero.Location = new System.Drawing.Point(15, 200);
            this.btnAgregarNumero.Size = new System.Drawing.Size(90, 32);
            this.btnAgregarNumero.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnAgregarNumero.ForeColor = System.Drawing.Color.White;
            this.btnAgregarNumero.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarNumero.Name = "btnAgregarNumero";
            this.btnAgregarNumero.Click += new System.EventHandler(this.btnAgregarNumero_Click);

            // tlpRango (2x2: 1 / 5 / 10 / 20 - posicion del sorteo a la que apunta el numero)
            this.tlpRango.Location = new System.Drawing.Point(200, 155);
            this.tlpRango.Size = new System.Drawing.Size(165, 110);
            this.tlpRango.Name = "tlpRango";
            this.tlpRango.ColumnCount = 2;
            this.tlpRango.RowCount = 2;
            this.tlpRango.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRango.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRango.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRango.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            int[] rangos = { 1, 5, 10, 20 };
            for (int i = 0; i < rangos.Length; i++)
            {
                System.Windows.Forms.Button btnRango = new System.Windows.Forms.Button();
                btnRango.Text = rangos[i].ToString("00");
                btnRango.Tag = rangos[i];
                btnRango.Dock = System.Windows.Forms.DockStyle.Fill;
                btnRango.Margin = new System.Windows.Forms.Padding(4);
                btnRango.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
                btnRango.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btnRango.BackColor = i == 0
                    ? System.Drawing.Color.FromArgb(29, 158, 117)
                    : System.Drawing.Color.FromArgb(240, 244, 248);
                btnRango.ForeColor = i == 0 ? System.Drawing.Color.White : System.Drawing.Color.Black;
                btnRango.Click += new System.EventHandler(this.BtnRango_Click);
                this.tlpRango.Controls.Add(btnRango, i % 2, i / 2);
            }

            // tlpTeclado (teclado numerico: 1-9, borrar, 0)
            this.tlpTeclado.Location = new System.Drawing.Point(15, 280);
            this.tlpTeclado.Size = new System.Drawing.Size(240, 180);
            this.tlpTeclado.Name = "tlpTeclado";
            this.tlpTeclado.ColumnCount = 3;
            this.tlpTeclado.RowCount = 4;
            for (int i = 0; i < 3; i++)
                this.tlpTeclado.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            for (int i = 0; i < 4; i++)
                this.tlpTeclado.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));

            string[] teclas = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "←", "0", "" };
            for (int i = 0; i < teclas.Length; i++)
            {
                if (teclas[i] == "") continue;
                System.Windows.Forms.Button tecla = new System.Windows.Forms.Button();
                tecla.Text = teclas[i];
                tecla.Dock = System.Windows.Forms.DockStyle.Fill;
                tecla.Margin = new System.Windows.Forms.Padding(3);
                tecla.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
                tecla.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                tecla.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);
                if (teclas[i] == "←")
                    tecla.Click += new System.EventHandler(this.btnBorrar_Click);
                else
                    tecla.Click += new System.EventHandler(this.TeclaNumerica_Click);
                this.tlpTeclado.Controls.Add(tecla, i % 3, i / 3);
            }

            // lblApuestaCaption / lblApuestaValor
            this.lblApuestaCaption.Text = "Apuesta $";
            this.lblApuestaCaption.Location = new System.Drawing.Point(15, 475);
            this.lblApuestaCaption.Size = new System.Drawing.Size(100, 22);
            this.lblApuestaCaption.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lblApuestaCaption.Name = "lblApuestaCaption";

            this.lblApuestaValor.Location = new System.Drawing.Point(115, 475);
            this.lblApuestaValor.Size = new System.Drawing.Size(140, 22);
            this.lblApuestaValor.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lblApuestaValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblApuestaValor.Name = "lblApuestaValor";

            // lblTotalCaption / lblTotalValor
            this.lblTotalCaption.Text = "Total $";
            this.lblTotalCaption.Location = new System.Drawing.Point(15, 500);
            this.lblTotalCaption.Size = new System.Drawing.Size(100, 26);
            this.lblTotalCaption.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalCaption.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTotalCaption.Name = "lblTotalCaption";

            this.lblTotalValor.Location = new System.Drawing.Point(115, 500);
            this.lblTotalValor.Size = new System.Drawing.Size(140, 26);
            this.lblTotalValor.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalValor.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTotalValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotalValor.Name = "lblTotalValor";

            // btnEnviar
            this.btnEnviar.Text = "ENVIAR";
            this.btnEnviar.Location = new System.Drawing.Point(15, 540);
            this.btnEnviar.Size = new System.Drawing.Size(320, 42);
            this.btnEnviar.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnEnviar.ForeColor = System.Drawing.Color.White;
            this.btnEnviar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnviar.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);

            // FormApuesta
            // Orden de Controls.Add importante para el docking: pnlHeader (Top) primero, para
            // que ocupe todo el ancho; pnlDerecha (Right) despues, dentro del espacio restante;
            // dgvJugadas (Fill) al final, para que tome todo lo que sobra y crezca con el form.
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.MinimumSize = new System.Drawing.Size(820, 600);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlDerecha);
            this.Controls.Add(this.dgvJugadas);
            this.Text = "Cargar Apuesta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormApuesta";
            this.pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).EndInit();
            this.pnlDerecha.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudMonto)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblLoterias;
        private System.Windows.Forms.Label lblTurnos;
        private System.Windows.Forms.DataGridView dgvJugadas;
        private System.Windows.Forms.Panel pnlDerecha;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.ComboBox cmbTipo;
        private System.Windows.Forms.Label lblMonto;
        private System.Windows.Forms.NumericUpDown nudMonto;
        private System.Windows.Forms.Label lblNumero;
        private System.Windows.Forms.TextBox txtNumero;
        private System.Windows.Forms.Button btnAgregarNumero;
        private System.Windows.Forms.Label lblRango;
        private System.Windows.Forms.TableLayoutPanel tlpRango;
        private System.Windows.Forms.TableLayoutPanel tlpTeclado;
        private System.Windows.Forms.Label lblApuestaCaption;
        private System.Windows.Forms.Label lblApuestaValor;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblTotalValor;
        private System.Windows.Forms.Button btnEnviar;
    }
}
