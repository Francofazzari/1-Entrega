using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entrega1software
{
    public partial class FormLogin : Form ,IObservadorIdioma
    {

        private UsuarioBLL bll = new UsuarioBLL();
        public FormLogin()
        {
            InitializeComponent();

            // Suscribir este formulario
            IdiomaManager.Instancia.Suscribir(this);
            // Cargar los idiomas disponibles en el ComboBox
            List<Idioma> idiomas = IdiomaManager.Instancia.ObtenerIdiomas();
            cmbIdiomas.DataSource = idiomas;
            cmbIdiomas.DisplayMember = "Nombre";
            cmbIdiomas.SelectedIndexChanged += cmbIdiomas_SelectedIndexChanged;

        }
        public void ActualizarIdioma(Idioma idioma)
        {
            // Actualizar etiquetas del Login
            lblTerminal.Text = IdiomaManager.Instancia.ObtenerMensaje("lbl_terminal");
            lblClave.Text = IdiomaManager.Instancia.ObtenerMensaje("lbl_clave");
            btnIngresar.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_ingresar");
        }
        private void cmbIdiomas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIdiomas.SelectedItem != null)
            {
                Idioma seleccionado = (Idioma)cmbIdiomas.SelectedItem;
                IdiomaManager.Instancia.CambiarIdioma(seleccionado); // Esto notifica a todos
            }
        }
        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                bool ok = bll.Login(txtTerminal.Text.Trim(), txtClave.Text);
                if (ok)
                {
                    BitacoraBLL bitacora = new BitacoraBLL();
                    bitacora.Registrar(
                        SesionManager.Instancia.UsuarioActual.Id,
                        SesionManager.Instancia.UsuarioActual.Nombre,
                        "Login",
                        "Inicio de sesion exitoso"
                    );
                    FormPrincipal fp = new FormPrincipal();
                    fp.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Terminal o contraseña incorrectos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbIdiomas_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }


    }
}
