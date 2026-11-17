using System;
using System.Drawing;
using System.Windows.Forms;

namespace Entrega1software
{
    // Cartel de error de integridad para el Administrador. Un MessageBox comun no permite
    // poner texto propio en los botones, por eso este formulario chico ofrece las dos salidas:
    // "Restaurar backup" (volver los datos a como estaban en un backup) o "Recalcular Digito"
    // (aceptar los datos actuales como correctos). Cerrar la ventana cancela el ingreso.
    public class FormAvisoIntegridad : Form
    {
        private Label lblDetalle;
        private Button btnRestaurar;
        private Button btnRecalcular;
        private Label lblAyuda;

        // true si el administrador eligio "Restaurar backup"; false si eligio "Recalcular Digito".
        // Solo tiene sentido cuando ShowDialog devolvio OK.
        public bool EligioRestaurar { get; private set; }

        public FormAvisoIntegridad(string detalle)
        {
            InitializeComponent();
            lblDetalle.Text = detalle;
            AjustarAlto();
        }

        private void InitializeComponent()
        {
            this.lblDetalle = new Label();
            this.btnRestaurar = new Button();
            this.btnRecalcular = new Button();
            this.lblAyuda = new Label();
            this.SuspendLayout();

            // lblDetalle (texto plano, no un cuadro de texto)
            this.lblDetalle.AutoSize = false;
            this.lblDetalle.Location = new Point(12, 12);
            this.lblDetalle.Size = new Size(460, 60);

            // lblAyuda
            this.lblAyuda.Text = "Puede restaurar un backup (los datos vuelven a como estaban cuando se hizo) o, "
                + "si ya corrigio el dato en la base, recalcular los digitos verificadores "
                + "(horizontal y vertical) y continuar.";
            this.lblAyuda.Location = new Point(12, 80);
            this.lblAyuda.Size = new Size(460, 48);

            // btnRestaurar
            this.btnRestaurar.Text = "Restaurar backup";
            this.btnRestaurar.Location = new Point(12, 135);
            this.btnRestaurar.Size = new Size(225, 32);
            this.btnRestaurar.BackColor = Color.FromArgb(185, 48, 48);
            this.btnRestaurar.ForeColor = Color.White;
            this.btnRestaurar.FlatStyle = FlatStyle.Flat;
            this.btnRestaurar.Click += new EventHandler(this.btnRestaurar_Click);

            // btnRecalcular
            this.btnRecalcular.Text = "Recalcular Digito";
            this.btnRecalcular.Location = new Point(247, 135);
            this.btnRecalcular.Size = new Size(225, 32);
            this.btnRecalcular.BackColor = Color.FromArgb(29, 158, 117);
            this.btnRecalcular.ForeColor = Color.White;
            this.btnRecalcular.FlatStyle = FlatStyle.Flat;
            this.btnRecalcular.Click += new EventHandler(this.btnRecalcular_Click);

            // FormAvisoIntegridad
            this.ClientSize = new Size(484, 180);
            this.Controls.Add(this.lblDetalle);
            this.Controls.Add(this.lblAyuda);
            this.Controls.Add(this.btnRestaurar);
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
            btnRestaurar.Top = lblAyuda.Bottom + 12;
            btnRecalcular.Top = btnRestaurar.Top;
            this.ClientSize = new Size(this.ClientSize.Width, btnRecalcular.Bottom + 12);
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            EligioRestaurar = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            EligioRestaurar = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
