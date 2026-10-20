namespace Entrega1software
{
    partial class FormFelicitaciones
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.btnAceptar = Estilos.Boton("ACEPTAR", Estilos.Verde, 12F);
            this.SuspendLayout();

            // timer: anima el papel picado y el importe (unos 60 cuadros por segundo)
            this.timer.Interval = 16;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);

            // btnAceptar: la posicion final se calcula en el constructor segun el alto del cartel
            this.btnAceptar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAceptar.Name = "btnAceptar";

            // FormFelicitaciones: todo lo demas se dibuja en OnPaint
            this.ClientSize = new System.Drawing.Size(540, 560);
            this.Controls.Add(this.btnAceptar);
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnAceptar;
            this.BackColor = System.Drawing.Color.White;
            this.DoubleBuffered = true;
            this.Font = Estilos.Fuente(9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ticket pagado";
            this.Name = "FormFelicitaciones";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button btnAceptar;
    }
}
