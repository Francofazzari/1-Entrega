namespace Entrega1software
{
    partial class FormResultado
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
            this.pnlEncabezado = Estilos.Encabezado(Estilos.Turquesa, out this.lblTitulo, out this.lblSubtitulo);
            this.lblAyuda = new System.Windows.Forms.Label();
            this.tlpNumeros = new System.Windows.Forms.TableLayoutPanel();
            this.tlpBotones = new System.Windows.Forms.TableLayoutPanel();
            this.btnAzar = Estilos.BotonSecundario("Completar al azar", Estilos.Turquesa, 10F);
            this.btnLimpiar = Estilos.BotonSecundario("Limpiar", Estilos.Azul, 10F);
            this.btnCancelar = Estilos.BotonSecundario("Cancelar", Estilos.Azul, 10F);
            this.btnGuardar = Estilos.Boton("GUARDAR RESULTADO", Estilos.Turquesa, 10.5F);
            this.tlpRaiz.SuspendLayout();
            this.tlpBotones.SuspendLayout();
            this.SuspendLayout();

            // tlpRaiz: encabezado / ayuda / puestos / botones
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 1;
            this.tlpRaiz.RowCount = 4;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.Controls.Add(this.lblAyuda, 0, 1);
            this.tlpRaiz.Controls.Add(this.tlpNumeros, 0, 2);
            this.tlpRaiz.Controls.Add(this.tlpBotones, 0, 3);
            this.tlpRaiz.Name = "tlpRaiz";

            this.lblAyuda.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAyuda.Font = Estilos.Fuente(9.5F);
            this.lblAyuda.ForeColor = Estilos.TextoSuave;
            this.lblAyuda.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAyuda.Margin = new System.Windows.Forms.Padding(20, 4, 20, 0);
            this.lblAyuda.Name = "lblAyuda";

            // tlpNumeros: un casillero por puesto; filas y columnas se arman en el constructor
            this.tlpNumeros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpNumeros.Margin = new System.Windows.Forms.Padding(16, 4, 16, 4);
            this.tlpNumeros.Name = "tlpNumeros";

            // tlpBotones: [Completar al azar] [Limpiar] ........ [Cancelar] [GUARDAR RESULTADO]
            this.tlpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBotones.BackColor = System.Drawing.Color.White;
            this.tlpBotones.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this.tlpBotones.ColumnCount = 5;
            this.tlpBotones.RowCount = 1;
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tlpBotones.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBotones.Controls.Add(this.btnAzar, 0, 0);
            this.tlpBotones.Controls.Add(this.btnLimpiar, 1, 0);
            this.tlpBotones.Controls.Add(this.btnCancelar, 3, 0);
            this.tlpBotones.Controls.Add(this.btnGuardar, 4, 0);
            this.tlpBotones.Name = "tlpBotones";

            this.btnAzar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAzar.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnAzar.Name = "btnAzar";
            this.btnAzar.Click += new System.EventHandler(this.btnAzar_Click);

            this.btnLimpiar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(0);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);

            this.btnCancelar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.btnGuardar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(0);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            // FormResultado (el tamanio final se ajusta en el constructor segun el juego)
            this.ClientSize = new System.Drawing.Size(640, 640);
            this.Controls.Add(this.tlpRaiz);
            this.CancelButton = this.btnCancelar;
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cargar resultado";
            this.Name = "FormResultado";
            this.tlpRaiz.ResumeLayout(false);
            this.tlpBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.TableLayoutPanel tlpNumeros;
        private System.Windows.Forms.TableLayoutPanel tlpBotones;
        private System.Windows.Forms.Button btnAzar;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnGuardar;
    }
}
