namespace Entrega1software
{
    partial class FormVentas
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.lblMensaje = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblMensaje.Text = "Bienvenido a Ventas en Linea";
            this.lblMensaje.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblMensaje.Location = new System.Drawing.Point(80, 120);
            this.lblMensaje.Size = new System.Drawing.Size(440, 40);
            this.lblMensaje.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.ClientSize = new System.Drawing.Size(600, 300);
            this.Controls.Add(this.lblMensaje);
            this.Text = "Ventas en Linea";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormVentas";
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label lblMensaje;
    }
}