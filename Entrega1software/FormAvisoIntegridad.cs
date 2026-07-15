using System;
using System.Drawing;
using System.Windows.Forms;

namespace Entrega1software
{
    // Cartel de error de integridad para el Administrador. Un MessageBox comun no permite
    // poner texto propio en el boton, por eso este formulario chico reemplaza al "Si/No" por
    // un unico boton "Recalcular Digito".
    public class FormAvisoIntegridad : Form
    {
        private Label lblDetalle;
        private Button btnRecalcular;
        private Label lblAyuda;

        public FormAvisoIntegridad(string detalle)
        {
            InitializeComponent();
            lblDetalle.Text = detalle;
            AjustarAlto();
        }

        private void InitializeComponent()
        {
            this.lblDetalle = new Label();
            this.btnRecalcular = new Button();
            this.lblAyuda = new Label();
            this.SuspendLayout();

            // lblDetalle (texto plano, no un cuadro de texto)
            this.lblDetalle.AutoSize = false;
            this.lblDetalle.Location = new Point(12, 12);
            this.lblDetalle.Size = new Size(460, 60);

            // lblAyuda
            this.lblAyuda.Text = "Si ya corrigio el dato en la base, puede recalcular los digitos verificadores "
                + "(horizontal y vertical) y continuar.";
            this.lblAyuda.Location = new Point(12, 80);
            this.lblAyuda.Size = new Size(460, 40);

            // btnRecalcular
            this.btnRecalcular.Text = "Recalcular Digito";
            this.btnRecalcular.Location = new Point(12, 125);
            this.btnRecalcular.Size = new Size(460, 32);
            this.btnRecalcular.BackColor = Color.FromArgb(29, 158, 117);
            this.btnRecalcular.ForeColor = Color.White;
            this.btnRecalcular.FlatStyle = FlatStyle.Flat;
            this.btnRecalcular.Click += new EventHandler(this.btnRecalcular_Click);

            // FormAvisoIntegridad
            this.ClientSize = new Size(484, 170);
            this.Controls.Add(this.lblDetalle);
            this.Controls.Add(this.lblAyuda);
            this.Controls.Add(this.btnRecalcular);
            this.Text = "Error de integridad";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ResumeLayout(false);
        }

        // El detalle es de largo variable (una linea por cada inconsistencia encontrada), asi
        // que se mide el texto ya cargado y se acomoda el resto de los controles debajo.
        private void AjustarAlto()
        {
            Size medido = TextRenderer.MeasureText(lblDetalle.Text, lblDetalle.Font,
                new Size(lblDetalle.Width, 0), TextFormatFlags.WordBreak);
            lblDetalle.Height = Math.Max(40, medido.Height + 10);

            lblAyuda.Top = lblDetalle.Bottom + 15;
            btnRecalcular.Top = lblAyuda.Bottom + 15;
            this.ClientSize = new Size(this.ClientSize.Width, btnRecalcular.Bottom + 12);
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
