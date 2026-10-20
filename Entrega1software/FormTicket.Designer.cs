namespace Entrega1software
{
    partial class FormTicket
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
            this.pnlMesa = new System.Windows.Forms.Panel();
            this.pnlPapel = new System.Windows.Forms.Panel();
            this.tlpBotones = new System.Windows.Forms.TableLayoutPanel();
            this.btnCopiar = Estilos.BotonSecundario("Copiar código", Estilos.Azul);
            this.lblCopiado = new System.Windows.Forms.Label();
            this.btnImprimir = Estilos.Boton("Imprimir", Estilos.AzulMedio);
            this.btnCerrar = Estilos.Boton("Cerrar", Estilos.Azul);
            this.tlpRaiz.SuspendLayout();
            this.pnlMesa.SuspendLayout();
            this.tlpBotones.SuspendLayout();
            this.SuspendLayout();

            // tlpRaiz: encabezado / ticket / botones (filas fijas: no depende del orden de docking)
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 1;
            this.tlpRaiz.RowCount = 3;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.Controls.Add(this.pnlMesa, 0, 1);
            this.tlpRaiz.Controls.Add(this.tlpBotones, 0, 2);
            this.tlpRaiz.Name = "tlpRaiz";

            // pnlMesa: fondo gris donde se apoya el "papel" del ticket
            this.pnlMesa.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMesa.BackColor = Estilos.Fondo;
            this.pnlMesa.AutoScroll = true;
            this.pnlMesa.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMesa.Controls.Add(this.pnlPapel);
            this.pnlMesa.Name = "pnlMesa";
            this.pnlMesa.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMesa_Paint);
            this.pnlMesa.Resize += new System.EventHandler(this.pnlMesa_Resize);

            // pnlPapel: el ticket propiamente dicho (se dibuja en pnlPapel_Paint)
            this.pnlPapel.BackColor = System.Drawing.Color.White;
            this.pnlPapel.Name = "pnlPapel";
            this.pnlPapel.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPapel_Paint);

            // tlpBotones: [Copiar codigo] [aviso] ............ [Imprimir] [Cerrar]
            this.tlpBotones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBotones.BackColor = System.Drawing.Color.White;
            this.tlpBotones.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.tlpBotones.ColumnCount = 4;
            this.tlpBotones.RowCount = 1;
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpBotones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpBotones.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBotones.Controls.Add(this.btnCopiar, 0, 0);
            this.tlpBotones.Controls.Add(this.lblCopiado, 1, 0);
            this.tlpBotones.Controls.Add(this.btnImprimir, 2, 0);
            this.tlpBotones.Controls.Add(this.btnCerrar, 3, 0);
            this.tlpBotones.Name = "tlpBotones";

            this.btnCopiar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCopiar.Name = "btnCopiar";
            this.btnCopiar.Click += new System.EventHandler(this.btnCopiar_Click);

            this.lblCopiado.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCopiado.Font = Estilos.Fuente(9.5F);
            this.lblCopiado.ForeColor = Estilos.Verde;
            this.lblCopiado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCopiado.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblCopiado.Name = "lblCopiado";

            this.btnImprimir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnImprimir.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);

            this.btnCerrar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.DialogResult = System.Windows.Forms.DialogResult.OK;

            // FormTicket
            this.ClientSize = new System.Drawing.Size(520, 760);
            this.MinimumSize = new System.Drawing.Size(480, 480);
            this.Controls.Add(this.tlpRaiz);
            this.AcceptButton = this.btnCerrar;
            this.CancelButton = this.btnCerrar;
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ticket";
            this.Name = "FormTicket";
            this.tlpRaiz.ResumeLayout(false);
            this.pnlMesa.ResumeLayout(false);
            this.tlpBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Panel pnlMesa;
        private System.Windows.Forms.Panel pnlPapel;
        private System.Windows.Forms.TableLayoutPanel tlpBotones;
        private System.Windows.Forms.Button btnCopiar;
        private System.Windows.Forms.Label lblCopiado;
        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.Button btnCerrar;
    }
}
