namespace Entrega1software
{
    partial class FormOperacionTicket
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
            this.pnlBuscar = Estilos.Tarjeta();
            this.tlpBuscar = new System.Windows.Forms.TableLayoutPanel();
            this.lblBuscarTitulo = Estilos.Seccion("Buscar por código");
            this.tlpCodigo = new System.Windows.Forms.TableLayoutPanel();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.btnBuscar = Estilos.Boton("Buscar", Estilos.Azul, 10F);
            this.lblAyuda = new System.Windows.Forms.Label();
            this.lblHoyTitulo = Estilos.Seccion("Tickets de hoy");
            this.dgvHoy = new System.Windows.Forms.DataGridView();
            this.pnlDetalle = Estilos.Tarjeta();
            this.lblVacio = new System.Windows.Forms.Label();
            this.tlpDetalle = new System.Windows.Forms.TableLayoutPanel();
            this.tlpResumen = new System.Windows.Forms.TableLayoutPanel();
            this.lblJugadasTitulo = Estilos.Seccion("Jugadas");
            this.dgvJugadas = new System.Windows.Forms.DataGridView();
            this.lblMotivo = new System.Windows.Forms.Label();
            this.btnAccion = Estilos.Boton("", Estilos.Verde, 12F);
            this.tlpRaiz.SuspendLayout();
            this.pnlBuscar.SuspendLayout();
            this.tlpBuscar.SuspendLayout();
            this.tlpCodigo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHoy)).BeginInit();
            this.pnlDetalle.SuspendLayout();
            this.tlpDetalle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).BeginInit();
            this.SuspendLayout();

            // tlpRaiz: encabezado; abajo, la busqueda (izquierda) y el detalle del ticket (derecha)
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 2;
            this.tlpRaiz.RowCount = 2;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 440F));
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.SetColumnSpan(this.pnlEncabezado, 2);
            this.tlpRaiz.Controls.Add(this.pnlBuscar, 0, 1);
            this.tlpRaiz.Controls.Add(this.pnlDetalle, 1, 1);
            this.tlpRaiz.Name = "tlpRaiz";

            // ---------------- busqueda ----------------
            this.pnlBuscar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBuscar.Margin = new System.Windows.Forms.Padding(16, 16, 8, 16);
            this.pnlBuscar.Controls.Add(this.tlpBuscar);
            this.pnlBuscar.Name = "pnlBuscar";

            this.tlpBuscar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBuscar.ColumnCount = 1;
            this.tlpBuscar.RowCount = 5;
            this.tlpBuscar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBuscar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpBuscar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpBuscar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBuscar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpBuscar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBuscar.Controls.Add(this.lblBuscarTitulo, 0, 0);
            this.tlpBuscar.Controls.Add(this.tlpCodigo, 0, 1);
            this.tlpBuscar.Controls.Add(this.lblAyuda, 0, 2);
            this.tlpBuscar.Controls.Add(this.lblHoyTitulo, 0, 3);
            this.tlpBuscar.Controls.Add(this.dgvHoy, 0, 4);
            this.tlpBuscar.Name = "tlpBuscar";

            this.tlpCodigo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCodigo.Margin = new System.Windows.Forms.Padding(0);
            this.tlpCodigo.ColumnCount = 2;
            this.tlpCodigo.RowCount = 1;
            this.tlpCodigo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCodigo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tlpCodigo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCodigo.Controls.Add(this.txtCodigo, 0, 0);
            this.tlpCodigo.Controls.Add(this.btnBuscar, 1, 0);
            this.tlpCodigo.Name = "tlpCodigo";

            this.txtCodigo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtCodigo.Font = Estilos.Mono(12.5F);
            this.txtCodigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCodigo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCodigo.MaxLength = 38;
            this.txtCodigo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodigo_KeyDown);

            this.btnBuscar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);

            this.lblAyuda.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAyuda.Font = Estilos.Fuente(8.75F);
            this.lblAyuda.ForeColor = Estilos.TextoSuave;
            this.lblAyuda.Text = "Pegá el código completo o escribí el código corto (los primeros 8 caracteres) que figura en el ticket.";
            this.lblAyuda.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblAyuda.Name = "lblAyuda";

            this.dgvHoy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHoy.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.dgvHoy.Name = "dgvHoy";
            this.dgvHoy.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHoy_CellClick);

            // ---------------- detalle del ticket ----------------
            // lblVacio y tlpDetalle ocupan el mismo lugar: se muestra uno u otro, nunca los dos.
            this.pnlDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetalle.Margin = new System.Windows.Forms.Padding(8, 16, 16, 16);
            this.pnlDetalle.Controls.Add(this.lblVacio);
            this.pnlDetalle.Controls.Add(this.tlpDetalle);
            this.pnlDetalle.Name = "pnlDetalle";

            this.lblVacio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVacio.Font = Estilos.Fuente(11F);
            this.lblVacio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblVacio.Name = "lblVacio";

            this.tlpDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDetalle.ColumnCount = 1;
            this.tlpDetalle.RowCount = 5;
            this.tlpDetalle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDetalle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpDetalle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpDetalle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDetalle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpDetalle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpDetalle.Controls.Add(this.tlpResumen, 0, 0);
            this.tlpDetalle.Controls.Add(this.lblJugadasTitulo, 0, 1);
            this.tlpDetalle.Controls.Add(this.dgvJugadas, 0, 2);
            this.tlpDetalle.Controls.Add(this.lblMotivo, 0, 3);
            this.tlpDetalle.Controls.Add(this.btnAccion, 0, 4);
            this.tlpDetalle.Name = "tlpDetalle";

            // tlpResumen: 3 columnas x (titulo + valor) x 2; los campos se crean en el constructor
            this.tlpResumen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpResumen.Margin = new System.Windows.Forms.Padding(0);
            this.tlpResumen.Name = "tlpResumen";

            this.dgvJugadas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJugadas.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.dgvJugadas.Name = "dgvJugadas";

            this.lblMotivo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMotivo.Font = Estilos.Negrita(10.5F);
            this.lblMotivo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMotivo.Name = "lblMotivo";

            this.btnAccion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAccion.Margin = new System.Windows.Forms.Padding(0, 4, 0, 2);
            this.btnAccion.Name = "btnAccion";
            this.btnAccion.Click += new System.EventHandler(this.btnAccion_Click);

            // FormOperacionTicket
            this.ClientSize = new System.Drawing.Size(1240, 720);
            this.MinimumSize = new System.Drawing.Size(1000, 560);
            this.Controls.Add(this.tlpRaiz);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "FormOperacionTicket";
            this.tlpRaiz.ResumeLayout(false);
            this.pnlBuscar.ResumeLayout(false);
            this.tlpBuscar.ResumeLayout(false);
            this.tlpCodigo.ResumeLayout(false);
            this.tlpCodigo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHoy)).EndInit();
            this.pnlDetalle.ResumeLayout(false);
            this.tlpDetalle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJugadas)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel pnlBuscar;
        private System.Windows.Forms.TableLayoutPanel tlpBuscar;
        private System.Windows.Forms.Label lblBuscarTitulo;
        private System.Windows.Forms.TableLayoutPanel tlpCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Label lblHoyTitulo;
        private System.Windows.Forms.DataGridView dgvHoy;
        private System.Windows.Forms.Panel pnlDetalle;
        private System.Windows.Forms.Label lblVacio;
        private System.Windows.Forms.TableLayoutPanel tlpDetalle;
        private System.Windows.Forms.TableLayoutPanel tlpResumen;
        private System.Windows.Forms.Label lblJugadasTitulo;
        private System.Windows.Forms.DataGridView dgvJugadas;
        private System.Windows.Forms.Label lblMotivo;
        private System.Windows.Forms.Button btnAccion;
    }
}
