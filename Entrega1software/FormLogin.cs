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
                if (!ok)
                {
                    MessageBox.Show("Terminal o contraseña incorrectos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!PuedeContinuarLuegoDeVerificarIntegridad())
                {
                    SesionManager.Instancia.CerrarSesion();
                    return;
                }

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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Verifica la integridad de la base ya sabiendo quien inicio sesion, para poder
        // diferenciar el comportamiento segun el perfil. Devuelve true si el login puede continuar.
        private bool PuedeContinuarLuegoDeVerificarIntegridad()
        {
            List<string> errores;
            try
            {
                errores = IdiomaManager.Instancia.VerificarIntegridad();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo verificar la integridad de la base de datos:\n" + ex.Message,
                    "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (errores.Count == 0) return true;

            string detalle = "Se detectaron problemas de integridad en la base de datos:\n\n" +
                              string.Join("\n", errores);

            string perfil = new PerfilBLL().ObtenerPerfilDeUsuario(SesionManager.Instancia.UsuarioActual.Id);

            if (perfil == "Administrador")
            {
                using (FormAvisoIntegridad dlg = new FormAvisoIntegridad(detalle))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        IdiomaManager.Instancia.RecalcularTodosLosDigitos();
                        MessageBox.Show(
                            "Digitos verificadores recalculados (horizontal y vertical).",
                            "Digito Verificador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    return false;
                }
            }

            MessageBox.Show(
                detalle + "\n\nContacte al administrador del sistema. No se puede continuar.",
                "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private void cmbIdiomas_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }


    }
}
