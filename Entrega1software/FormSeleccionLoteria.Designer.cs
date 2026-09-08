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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblLoteriaHeader = new System.Windows.Forms.Label();
            this.pnlLoterias = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTurnoHeader = new System.Windows.Forms.Label();
            this.pnlTurnos = new System.Windows.Forms.FlowLayoutPanel();
            this.btnContinuar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblTitulo (Dock=Top: siempre ocupa todo el ancho disponible)
            this.lblTitulo.Text = "Loteria y Horario";
            this.lblTitulo.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Height = 45;
            this.lblTitulo.Padding = new System.Windows.Forms.Padding(15, 10, 0, 0);
            this.lblTitulo.Name = "lblTitulo";

            // lblLoteriaHeader
            this.lblLoteriaHeader.Text = "Seleccione una o mas loterias:";
            this.lblLoteriaHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLoteriaHeader.Height = 25;
            this.lblLoteriaHeader.Padding = new System.Windows.Forms.Padding(15, 5, 0, 0);
            this.lblLoteriaHeader.Name = "lblLoteriaHeader";

            // pnlLoterias (FlowLayoutPanel: reacomoda los botones solo cuando se achica el ancho)
            this.pnlLoterias.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLoterias.Height = 135;
            this.pnlLoterias.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlLoterias.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlLoterias.WrapContents = true;
            this.pnlLoterias.AutoScroll = true;
            this.pnlLoterias.Name = "pnlLoterias";

            // lblTurnoHeader
            this.lblTurnoHeader.Text = "Seleccione uno o mas horarios:";
            this.lblTurnoHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTurnoHeader.Height = 25;
            this.lblTurnoHeader.Padding = new System.Windows.Forms.Padding(15, 5, 0, 0);
            this.lblTurnoHeader.Name = "lblTurnoHeader";

            // pnlTurnos
            this.pnlTurnos.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTurnos.Height = 135;
            this.pnlTurnos.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlTurnos.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlTurnos.WrapContents = true;
            this.pnlTurnos.AutoScroll = true;
            this.pnlTurnos.Name = "pnlTurnos";

            // btnContinuar (Dock=Bottom: siempre visible y a todo el ancho, aunque se achique el form)
            this.btnContinuar.Text = "Continuar";
            this.btnContinuar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnContinuar.Height = 48;
            this.btnContinuar.Margin = new System.Windows.Forms.Padding(15);
            this.btnContinuar.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnContinuar.ForeColor = System.Drawing.Color.White;
            this.btnContinuar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContinuar.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.btnContinuar.Name = "btnContinuar";
            this.btnContinuar.Click += new System.EventHandler(this.btnContinuar_Click);

            // FormSeleccionLoteria
            // Orden de Controls.Add importante para el docking: el PRIMER control Dock=Top
            // agregado queda mas cerca del borde superior real; por eso se agregan en el
            // mismo orden en que deben verse de arriba hacia abajo. btnContinuar (Dock=Bottom)
            // puede ir en cualquier posicion del listado, no compite por el mismo borde.
            this.ClientSize = new System.Drawing.Size(560, 480);
            this.MinimumSize = new System.Drawing.Size(480, 420);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblLoteriaHeader);
            this.Controls.Add(this.pnlLoterias);
            this.Controls.Add(this.lblTurnoHeader);
            this.Controls.Add(this.pnlTurnos);
            this.Controls.Add(this.btnContinuar);
            this.Text = "Seleccionar Loteria y Horario";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormSeleccionLoteria";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblLoteriaHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlLoterias;
        private System.Windows.Forms.Label lblTurnoHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlTurnos;
        private System.Windows.Forms.Button btnContinuar;
    }
}
