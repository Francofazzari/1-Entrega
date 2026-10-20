namespace Entrega1software
{
    partial class FormJuego
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRaiz = new System.Windows.Forms.TableLayoutPanel();
            this.pnlEncabezado = Estilos.Encabezado(Estilos.Verde, out this.lblTitulo, out this.lblSubtitulo);
            this.pnlTablero = Estilos.Tarjeta();
            this.tlpTablero = new System.Windows.Forms.TableLayoutPanel();
            this.lblTableroTitulo = Estilos.Seccion("Tu jugada");
            this.lblContador = new System.Windows.Forms.Label();
            this.tlpNumeros = new System.Windows.Forms.TableLayoutPanel();
            this.tlpAcciones = new System.Windows.Forms.TableLayoutPanel();
            this.btnAutomatica = Estilos.BotonSecundario("Automática", Estilos.Verde, 10.5F);
            this.btnLimpiar = Estilos.BotonSecundario("Limpiar", Estilos.TextoSuave, 10.5F);
            this.btnAgregar = Estilos.Boton("AGREGAR JUGADA", Estilos.AzulMedio, 11F);
            this.pnlTicket = Estilos.Tarjeta();
            this.tlpTicket = new System.Windows.Forms.TableLayoutPanel();
            this.lblTicketTitulo = Estilos.Seccion("Jugadas del ticket");
            this.dgvJugadas = new System.Windows.Forms.DataGridView();
            this.btnQuitar = Estilos.BotonSecundario("Quitar jugada", Estilos.Rojo);
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblTotalValor = new System.Windows.Forms.Label();
            this.btnConfirmar = Estilos.Boton("CONFIRMAR VENTA", Estilos.Verde, 12F);
            this.tlpRaiz.SuspendLayout();
            this.pnlTablero.SuspendLayout();
            this.tlpTablero.SuspendLayout();
            this.tlpAcciones.SuspendLayout();
            this.pnlTicket.SuspendLayout();
            this.tlpTicket.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).BeginInit();
            this.SuspendLayout();

            // tlpRaiz: encabezado arriba; abajo el tablero (izquierda) y el ticket (derecha)
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 2;
            this.tlpRaiz.RowCount = 2;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56F));
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.SetColumnSpan(this.pnlEncabezado, 2);
            this.tlpRaiz.Controls.Add(this.pnlTablero, 0, 1);
            this.tlpRaiz.Controls.Add(this.pnlTicket, 1, 1);
            this.tlpRaiz.Name = "tlpRaiz";

            // ---------------- tablero de numeros ----------------
            this.pnlTablero.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTablero.Margin = new System.Windows.Forms.Padding(16, 16, 8, 16);
            this.pnlTablero.Controls.Add(this.tlpTablero);
            this.pnlTablero.Name = "pnlTablero";

            this.tlpTablero.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTablero.ColumnCount = 2;
            this.tlpTablero.RowCount = 3;
            this.tlpTablero.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpTablero.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpTablero.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpTablero.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTablero.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tlpTablero.Controls.Add(this.lblTableroTitulo, 0, 0);
            this.tlpTablero.Controls.Add(this.lblContador, 1, 0);
            this.tlpTablero.Controls.Add(this.tlpNumeros, 0, 1);
            this.tlpTablero.SetColumnSpan(this.tlpNumeros, 2);
            this.tlpTablero.Controls.Add(this.tlpAcciones, 0, 2);
            this.tlpTablero.SetColumnSpan(this.tlpAcciones, 2);
            this.tlpTablero.Name = "tlpTablero";

            this.lblContador.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContador.Font = Estilos.Negrita(9.5F);
            this.lblContador.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblContador.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblContador.Name = "lblContador";

            // tlpNumeros: un boton por numero; filas y columnas se arman en el constructor
            this.tlpNumeros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpNumeros.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.tlpNumeros.Name = "tlpNumeros";

            this.tlpAcciones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAcciones.ColumnCount = 3;
            this.tlpAcciones.RowCount = 1;
            this.tlpAcciones.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.tlpAcciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpAcciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpAcciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAcciones.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAcciones.Controls.Add(this.btnAutomatica, 0, 0);
            this.tlpAcciones.Controls.Add(this.btnLimpiar, 1, 0);
            this.tlpAcciones.Controls.Add(this.btnAgregar, 2, 0);
            this.tlpAcciones.Name = "tlpAcciones";

            this.btnAutomatica.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAutomatica.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnAutomatica.Name = "btnAutomatica";
            this.btnAutomatica.Click += new System.EventHandler(this.btnAutomatica_Click);

            this.btnLimpiar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);

            this.btnAgregar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAgregar.Margin = new System.Windows.Forms.Padding(0);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);

            // ---------------- jugadas del ticket ----------------
            this.pnlTicket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTicket.Margin = new System.Windows.Forms.Padding(8, 16, 16, 16);
            this.pnlTicket.Controls.Add(this.tlpTicket);
            this.pnlTicket.Name = "pnlTicket";

            this.tlpTicket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTicket.ColumnCount = 2;
            this.tlpTicket.RowCount = 5;
            this.tlpTicket.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tlpTicket.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTicket.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpTicket.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTicket.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tlpTicket.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpTicket.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpTicket.Controls.Add(this.lblTicketTitulo, 0, 0);
            this.tlpTicket.SetColumnSpan(this.lblTicketTitulo, 2);
            this.tlpTicket.Controls.Add(this.dgvJugadas, 0, 1);
            this.tlpTicket.SetColumnSpan(this.dgvJugadas, 2);
            this.tlpTicket.Controls.Add(this.btnQuitar, 0, 2);
            this.tlpTicket.Controls.Add(this.lblResumen, 1, 2);
            this.tlpTicket.Controls.Add(this.lblTotalValor, 0, 3);
            this.tlpTicket.SetColumnSpan(this.lblTotalValor, 2);
            this.tlpTicket.Controls.Add(this.btnConfirmar, 0, 4);
            this.tlpTicket.SetColumnSpan(this.btnConfirmar, 2);
            this.tlpTicket.Name = "tlpTicket";

            this.dgvJugadas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJugadas.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.dgvJugadas.Name = "dgvJugadas";
            this.dgvJugadas.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvJugadas_Paint);

            this.btnQuitar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnQuitar.Margin = new System.Windows.Forms.Padding(0, 6, 0, 4);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);

            this.lblResumen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResumen.Font = Estilos.Fuente(9.5F);
            this.lblResumen.ForeColor = Estilos.TextoSuave;
            this.lblResumen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblResumen.Name = "lblResumen";

            this.lblTotalValor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalValor.Font = Estilos.Negrita(20F);
            this.lblTotalValor.ForeColor = Estilos.Texto;
            this.lblTotalValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotalValor.Name = "lblTotalValor";

            this.btnConfirmar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConfirmar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 2);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            // FormJuego
            this.ClientSize = new System.Drawing.Size(1100, 720);
            this.MinimumSize = new System.Drawing.Size(940, 600);
            this.Controls.Add(this.tlpRaiz);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Vender";
            this.Name = "FormJuego";
            this.tlpRaiz.ResumeLayout(false);
            this.pnlTablero.ResumeLayout(false);
            this.tlpTablero.ResumeLayout(false);
            this.tlpAcciones.ResumeLayout(false);
            this.pnlTicket.ResumeLayout(false);
            this.tlpTicket.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel pnlTablero;
        private System.Windows.Forms.TableLayoutPanel tlpTablero;
        private System.Windows.Forms.Label lblTableroTitulo;
        private System.Windows.Forms.Label lblContador;
        private System.Windows.Forms.TableLayoutPanel tlpNumeros;
        private System.Windows.Forms.TableLayoutPanel tlpAcciones;
        private System.Windows.Forms.Button btnAutomatica;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Panel pnlTicket;
        private System.Windows.Forms.TableLayoutPanel tlpTicket;
        private System.Windows.Forms.Label lblTicketTitulo;
        private System.Windows.Forms.DataGridView dgvJugadas;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblTotalValor;
        private System.Windows.Forms.Button btnConfirmar;
    }
}
