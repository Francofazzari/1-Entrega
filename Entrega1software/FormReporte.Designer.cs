namespace Entrega1software
{
    partial class FormReporte
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        // Esqueleto de la pantalla: encabezado, pestanias y el area donde se muestra cada
        // reporte. El contenido de cada reporte se arma en FormReporte.cs.
        private void InitializeComponent()
        {
            this.tlpRaiz = new System.Windows.Forms.TableLayoutPanel();
            this.pnlEncabezado = Estilos.Encabezado(Estilos.Turquesa, out this.lblTitulo, out this.lblSubtitulo);
            this.flpPestanias = new System.Windows.Forms.FlowLayoutPanel();
            this.btnVentas = Estilos.Toggle("Ventas en línea", 10.5F);
            this.btnPagos = Estilos.Toggle("Pagos en línea", 10.5F);
            this.btnGanadores = Estilos.Toggle("Números ganadores", 10.5F);
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.tlpRaiz.SuspendLayout();
            this.flpPestanias.SuspendLayout();
            this.SuspendLayout();

            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 1;
            this.tlpRaiz.RowCount = 3;
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.Controls.Add(this.flpPestanias, 0, 1);
            this.tlpRaiz.Controls.Add(this.pnlContenido, 0, 2);
            this.tlpRaiz.Name = "tlpRaiz";

            // flpPestanias: tres botones que funcionan como pestanias
            this.flpPestanias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPestanias.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.flpPestanias.WrapContents = false;
            this.flpPestanias.Controls.Add(this.btnVentas);
            this.flpPestanias.Controls.Add(this.btnPagos);
            this.flpPestanias.Controls.Add(this.btnGanadores);
            this.flpPestanias.Name = "flpPestanias";

            this.btnVentas.Size = new System.Drawing.Size(190, 42);
            this.btnVentas.Margin = new System.Windows.Forms.Padding(2, 0, 6, 0);
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);

            this.btnPagos.Size = new System.Drawing.Size(190, 42);
            this.btnPagos.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnPagos.Name = "btnPagos";
            this.btnPagos.Click += new System.EventHandler(this.btnPagos_Click);

            this.btnGanadores.Size = new System.Drawing.Size(210, 42);
            this.btnGanadores.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.btnGanadores.Name = "btnGanadores";
            this.btnGanadores.Click += new System.EventHandler(this.btnGanadores_Click);

            // pnlContenido: aca se muestra uno solo de los tres reportes a la vez
            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Padding = new System.Windows.Forms.Padding(16, 8, 16, 16);
            this.pnlContenido.Margin = new System.Windows.Forms.Padding(0);
            this.pnlContenido.Name = "pnlContenido";

            // FormReporte
            this.ClientSize = new System.Drawing.Size(1180, 760);
            this.MinimumSize = new System.Drawing.Size(980, 600);
            this.Controls.Add(this.tlpRaiz);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reportes";
            this.Name = "FormReporte";
            this.tlpRaiz.ResumeLayout(false);
            this.flpPestanias.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.FlowLayoutPanel flpPestanias;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnPagos;
        private System.Windows.Forms.Button btnGanadores;
        private System.Windows.Forms.Panel pnlContenido;
    }
}
