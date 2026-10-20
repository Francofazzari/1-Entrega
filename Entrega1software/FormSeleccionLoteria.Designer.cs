namespace Entrega1software
{
    partial class FormSeleccionLoteria
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
            this.lblLoteriaHeader = Estilos.Seccion("Loterías");
            this.pnlLoterias = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTurnoHeader = Estilos.Seccion("Horarios");
            this.pnlTurnos = new System.Windows.Forms.FlowLayoutPanel();
            this.lblResumen = new System.Windows.Forms.Label();
            this.btnContinuar = Estilos.Boton("CONTINUAR", Estilos.Verde, 11.5F);
            this.tlpRaiz.SuspendLayout();
            this.SuspendLayout();

            // tlpRaiz: una fila por seccion, de arriba hacia abajo. Con TableLayoutPanel el orden
            // visual lo dan las filas y no depende del orden en que se agregan los controles.
            this.tlpRaiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRaiz.ColumnCount = 1;
            this.tlpRaiz.RowCount = 7;
            this.tlpRaiz.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.tlpRaiz.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));   // encabezado
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));   // "Loterias"
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 132F));  // botones de loterias
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));   // "Horarios"
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));   // botones de horarios
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));   // resumen
            this.tlpRaiz.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));   // continuar
            this.tlpRaiz.Controls.Add(this.pnlEncabezado, 0, 0);
            this.tlpRaiz.Controls.Add(this.lblLoteriaHeader, 0, 1);
            this.tlpRaiz.Controls.Add(this.pnlLoterias, 0, 2);
            this.tlpRaiz.Controls.Add(this.lblTurnoHeader, 0, 3);
            this.tlpRaiz.Controls.Add(this.pnlTurnos, 0, 4);
            this.tlpRaiz.Controls.Add(this.lblResumen, 0, 5);
            this.tlpRaiz.Controls.Add(this.btnContinuar, 0, 6);
            this.tlpRaiz.Name = "tlpRaiz";

            this.lblLoteriaHeader.Margin = new System.Windows.Forms.Padding(24, 0, 24, 6);
            this.lblTurnoHeader.Margin = new System.Windows.Forms.Padding(24, 0, 24, 6);

            // pnlLoterias / pnlTurnos: los botones se generan en el constructor, uno por loteria/turno
            this.pnlLoterias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLoterias.Margin = new System.Windows.Forms.Padding(18, 0, 18, 0);
            this.pnlLoterias.WrapContents = true;
            this.pnlLoterias.AutoScroll = true;
            this.pnlLoterias.Name = "pnlLoterias";

            this.pnlTurnos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTurnos.Margin = new System.Windows.Forms.Padding(18, 0, 18, 0);
            this.pnlTurnos.WrapContents = true;
            this.pnlTurnos.AutoScroll = true;
            this.pnlTurnos.Name = "pnlTurnos";

            // lblResumen: cuantos sorteos se van a jugar con lo elegido
            this.lblResumen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResumen.Font = Estilos.Fuente(11F);
            this.lblResumen.ForeColor = Estilos.TextoSuave;
            this.lblResumen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblResumen.Margin = new System.Windows.Forms.Padding(24, 6, 24, 6);
            this.lblResumen.Name = "lblResumen";

            this.btnContinuar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnContinuar.Margin = new System.Windows.Forms.Padding(24, 4, 24, 0);
            this.btnContinuar.Name = "btnContinuar";
            this.btnContinuar.Click += new System.EventHandler(this.btnContinuar_Click);

            // FormSeleccionLoteria
            this.ClientSize = new System.Drawing.Size(740, 560);
            this.MinimumSize = new System.Drawing.Size(700, 540);
            this.Controls.Add(this.tlpRaiz);
            this.BackColor = Estilos.Fondo;
            this.Font = Estilos.Fuente(9.75F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quiniela - Lotería y horario";
            this.Name = "FormSeleccionLoteria";
            this.tlpRaiz.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel tlpRaiz;
        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblLoteriaHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlLoterias;
        private System.Windows.Forms.Label lblTurnoHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlTurnos;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.Button btnContinuar;
    }
}
