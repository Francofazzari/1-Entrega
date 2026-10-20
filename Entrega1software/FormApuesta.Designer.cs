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
            this.tlpRaiz = new System.Windows.Forms.TableLayoutPanel();
            this.pnlEncabezado = Estilos.Encabezado(Estilos.AzulMedio, out this.lblTitulo, out this.lblSubtitulo);
            this.pnlIzquierda = Estilos.Tarjeta();
            this.tlpIzquierda = new System.Windows.Forms.TableLayoutPanel();
            this.lblJugadasTitulo = Estilos.Seccion("Jugadas del ticket");
            this.dgvJugadas = new System.Windows.Forms.DataGridView();
            this.tlpPie = new System.Windows.Forms.TableLayoutPanel();
            this.btnQuitar = Estilos.BotonSecundario("Quitar jugada", Estilos.Rojo);
            this.tlpTotal = new System.Windows.Forms.TableLayoutPanel();
            this.lblResumen = new System.Windows.Forms.Label();
            this.lblTotalValor = new System.Windows.Forms.Label();
            this.btnEnviar = Estilos.Boton("CONFIRMAR VENTA", Estilos.Verde, 12F);
            this.pnlDerecha = Estilos.Tarjeta();
            this.tlpDerecha = new System.Windows.Forms.TableLayoutPanel();
            this.lblTipoTitulo = Estilos.Seccion("Tipo de jugada");
            this.tlpTipo = new System.Windows.Forms.TableLayoutPanel();
            this.btnDirecta = Estilos.Toggle("Directa", 10.5F);
            this.btnRedoblona = Estilos.Toggle("Redoblona", 10.5F);
            this.lblAyudaTipo = new System.Windows.Forms.Label();
            this.lblNumeroTitulo = Estilos.Seccion("Número");
            this.lblNumero = new System.Windows.Forms.Label();
            this.lblRedoblona = new System.Windows.Forms.Label();
            this.tlpTeclado = new System.Windows.Forms.TableLayoutPanel();
            this.lblPosicionTitulo = Estilos.Seccion("Posición");
            this.tlpRango = new System.Windows.Forms.TableLayoutPanel();
            this.lblMontoTitulo = Estilos.Seccion("Monto por jugada");
            this.nudMonto = new System.Windows.Forms.NumericUpDown();
            this.btnAgregar = Estilos.Boton("AGREGAR AL TICKET", Estilos.AzulMedio, 11F);
            this.lblAviso = new System.Windows.Forms.Label();
            this.tlpRaiz.SuspendLayout();
            this.pnlIzquierda.SuspendLayout();
            this.tlpIzquierda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).BeginInit();
            this.tlpPie.SuspendLayout();
            this.tlpTotal.SuspendLayout();
            this.pnlDerecha.SuspendLayout();
            this.tlpDerecha.SuspendLayout();
            this.tlpTipo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMonto)).BeginInit();
            this.SuspendLayout();

            // tlpRaiz: encabezado arriba (dos columnas) y abajo la lista del ticket (crece) y el
            // panel de carga (ancho fijo)
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 2;
            this.tlpRaiz.RowCount = 2;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 410F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.SetColumnSpan(this.pnlEncabezado, 2);
            this.tlpRaiz.Controls.Add(this.pnlIzquierda, 0, 1);
            this.tlpRaiz.Controls.Add(this.pnlDerecha, 1, 1);
            this.tlpRaiz.Name = "tlpRaiz";

            // ---------------- izquierda: jugadas del ticket ----------------
            this.pnlIzquierda.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIzquierda.Margin = new System.Windows.Forms.Padding(16, 16, 8, 16);
            this.pnlIzquierda.Controls.Add(this.tlpIzquierda);
            this.pnlIzquierda.Name = "pnlIzquierda";

            this.tlpIzquierda.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpIzquierda.ColumnCount = 1;
            this.tlpIzquierda.RowCount = 3;
            this.tlpIzquierda.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIzquierda.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpIzquierda.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIzquierda.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tlpIzquierda.Controls.Add(this.lblJugadasTitulo, 0, 0);
            this.tlpIzquierda.Controls.Add(this.dgvJugadas, 0, 1);
            this.tlpIzquierda.Controls.Add(this.tlpPie, 0, 2);
            this.tlpIzquierda.Name = "tlpIzquierda";

            this.dgvJugadas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJugadas.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.dgvJugadas.Name = "dgvJugadas";
            this.dgvJugadas.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvJugadas_Paint);

            // pie: [Quitar seleccionada] ...... resumen y total ...... [CONFIRMAR VENTA]
            this.tlpPie.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPie.ColumnCount = 3;
            this.tlpPie.RowCount = 1;
            this.tlpPie.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.tlpPie.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
            this.tlpPie.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpPie.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tlpPie.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPie.Controls.Add(this.btnQuitar, 0, 0);
            this.tlpPie.Controls.Add(this.tlpTotal, 1, 0);
            this.tlpPie.Controls.Add(this.btnEnviar, 2, 0);
            this.tlpPie.Name = "tlpPie";

            this.btnQuitar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnQuitar.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Click += new System.EventHandler(this.btnQuitar_Click);

            this.tlpTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTotal.ColumnCount = 1;
            this.tlpTotal.RowCount = 2;
            this.tlpTotal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTotal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpTotal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTotal.Controls.Add(this.lblResumen, 0, 0);
            this.tlpTotal.Controls.Add(this.lblTotalValor, 0, 1);
            this.tlpTotal.Name = "tlpTotal";

            this.lblResumen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResumen.Font = Estilos.Fuente(9.5F);
            this.lblResumen.ForeColor = Estilos.TextoSuave;
            this.lblResumen.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblResumen.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.lblResumen.Name = "lblResumen";

            this.lblTotalValor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalValor.Font = Estilos.Negrita(20F);
            this.lblTotalValor.ForeColor = Estilos.Texto;
            this.lblTotalValor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotalValor.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.lblTotalValor.Name = "lblTotalValor";

            this.btnEnviar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEnviar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);

            // ---------------- derecha: carga de la jugada ----------------
            this.pnlDerecha.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDerecha.Margin = new System.Windows.Forms.Padding(8, 16, 16, 16);
            this.pnlDerecha.AutoScroll = true;
            this.pnlDerecha.AutoScrollMinSize = new System.Drawing.Size(0, 630);
            this.pnlDerecha.Controls.Add(this.tlpDerecha);
            this.pnlDerecha.Name = "pnlDerecha";

            // tlpDerecha ocupa todo el alto del panel: la fila del teclado (alto 0 en la lista) toma
            // el espacio que sobra (minimo 160 px), y si no entra aparece la barra de desplazamiento.
            this.tlpDerecha.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDerecha.ColumnCount = 1;
            this.tlpDerecha.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            float[] altos = { 26F, 44F, 36F, 28F, 56F, 24F, 0F, 30F, 44F, 30F, 40F, 52F, 28F };
            this.tlpDerecha.RowCount = altos.Length;
            foreach (float alto in altos)
                this.tlpDerecha.RowStyles.Add(alto > 0
                    ? new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, alto)
                    : new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDerecha.Controls.Add(this.lblTipoTitulo, 0, 0);
            this.tlpDerecha.Controls.Add(this.tlpTipo, 0, 1);
            this.tlpDerecha.Controls.Add(this.lblAyudaTipo, 0, 2);
            this.tlpDerecha.Controls.Add(this.lblNumeroTitulo, 0, 3);
            this.tlpDerecha.Controls.Add(this.lblNumero, 0, 4);
            this.tlpDerecha.Controls.Add(this.lblRedoblona, 0, 5);
            this.tlpDerecha.Controls.Add(this.tlpTeclado, 0, 6);
            this.tlpDerecha.Controls.Add(this.lblPosicionTitulo, 0, 7);
            this.tlpDerecha.Controls.Add(this.tlpRango, 0, 8);
            this.tlpDerecha.Controls.Add(this.lblMontoTitulo, 0, 9);
            this.tlpDerecha.Controls.Add(this.nudMonto, 0, 10);
            this.tlpDerecha.Controls.Add(this.btnAgregar, 0, 11);
            this.tlpDerecha.Controls.Add(this.lblAviso, 0, 12);
            this.tlpDerecha.Name = "tlpDerecha";

            this.tlpTipo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTipo.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTipo.ColumnCount = 2;
            this.tlpTipo.RowCount = 1;
            this.tlpTipo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTipo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTipo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTipo.Controls.Add(this.btnDirecta, 0, 0);
            this.tlpTipo.Controls.Add(this.btnRedoblona, 1, 0);
            this.tlpTipo.Name = "tlpTipo";

            this.btnDirecta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDirecta.Margin = new System.Windows.Forms.Padding(0, 3, 4, 3);
            this.btnDirecta.Name = "btnDirecta";
            this.btnDirecta.Click += new System.EventHandler(this.btnDirecta_Click);

            this.btnRedoblona.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRedoblona.Margin = new System.Windows.Forms.Padding(4, 3, 0, 3);
            this.btnRedoblona.Name = "btnRedoblona";
            this.btnRedoblona.Click += new System.EventHandler(this.btnRedoblona_Click);

            this.lblAyudaTipo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAyudaTipo.Font = Estilos.Fuente(8.75F);
            this.lblAyudaTipo.ForeColor = Estilos.TextoSuave;
            this.lblAyudaTipo.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblAyudaTipo.Name = "lblAyudaTipo";

            // lblNumero: "visor" del numero que se va tipeando
            this.lblNumero.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNumero.Font = Estilos.Mono(30F, true);
            this.lblNumero.BackColor = Estilos.Fondo;
            this.lblNumero.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblNumero.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblNumero.Name = "lblNumero";
            this.lblNumero.Paint += new System.Windows.Forms.PaintEventHandler(this.lblNumero_Paint);

            this.lblRedoblona.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRedoblona.Font = Estilos.Negrita(9F);
            this.lblRedoblona.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRedoblona.Margin = new System.Windows.Forms.Padding(0);
            this.lblRedoblona.Name = "lblRedoblona";

            // tlpTeclado y tlpRango: los botones se crean en el constructor
            this.tlpTeclado.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTeclado.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.tlpTeclado.Name = "tlpTeclado";

            this.tlpRango.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRango.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRango.Name = "tlpRango";

            this.nudMonto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudMonto.Font = Estilos.Negrita(13F);
            this.nudMonto.Increment = 50;
            this.nudMonto.DecimalPlaces = 2;
            this.nudMonto.ThousandsSeparator = true;
            this.nudMonto.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudMonto.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.nudMonto.Name = "nudMonto";

            this.btnAgregar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAgregar.Margin = new System.Windows.Forms.Padding(0, 10, 0, 2);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);

            this.lblAviso.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAviso.Font = Estilos.Negrita(9F);
            this.lblAviso.ForeColor = Estilos.Rojo;
            this.lblAviso.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAviso.Name = "lblAviso";

            // FormApuesta
            this.ClientSize = new System.Drawing.Size(1100, 800);
            this.MinimumSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.tlpRaiz);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.KeyPreview = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quiniela - Cargar jugadas";
            this.Name = "FormApuesta";
            this.tlpRaiz.ResumeLayout(false);
            this.pnlIzquierda.ResumeLayout(false);
            this.tlpIzquierda.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).EndInit();
            this.tlpPie.ResumeLayout(false);
            this.tlpTotal.ResumeLayout(false);
            this.pnlDerecha.ResumeLayout(false);
            this.tlpDerecha.ResumeLayout(false);
            this.tlpTipo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudMonto)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel pnlIzquierda;
        private System.Windows.Forms.TableLayoutPanel tlpIzquierda;
        private System.Windows.Forms.Label lblJugadasTitulo;
        private System.Windows.Forms.DataGridView dgvJugadas;
        private System.Windows.Forms.TableLayoutPanel tlpPie;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.TableLayoutPanel tlpTotal;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Label lblTotalValor;
        private System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.Panel pnlDerecha;
        private System.Windows.Forms.TableLayoutPanel tlpDerecha;
        private System.Windows.Forms.Label lblTipoTitulo;
        private System.Windows.Forms.TableLayoutPanel tlpTipo;
        private System.Windows.Forms.Button btnDirecta;
        private System.Windows.Forms.Button btnRedoblona;
        private System.Windows.Forms.Label lblAyudaTipo;
        private System.Windows.Forms.Label lblNumeroTitulo;
        private System.Windows.Forms.Label lblNumero;
        private System.Windows.Forms.Label lblRedoblona;
        private System.Windows.Forms.TableLayoutPanel tlpTeclado;
        private System.Windows.Forms.Label lblPosicionTitulo;
        private System.Windows.Forms.TableLayoutPanel tlpRango;
        private System.Windows.Forms.Label lblMontoTitulo;
        private System.Windows.Forms.NumericUpDown nudMonto;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Label lblAviso;
    }
}
