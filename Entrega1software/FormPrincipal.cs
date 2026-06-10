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
    public partial class FormPrincipal : Form , IObservadorIdioma
    {

        private UsuarioBLL bll = new UsuarioBLL();

        public FormPrincipal()
        {
            InitializeComponent();
            this.IsMdiContainer = true;

            lblUsuario.Text = "Usuario: " +
                SesionManager.Instancia.UsuarioActual.Nombre + " " +
                SesionManager.Instancia.UsuarioActual.Apellido;

            ConfigurarSegunPerfil();
            IdiomaManager.Instancia.Suscribir(this);
        }

        private void ConfigurarSegunPerfil()
        {
            string perfil = SesionManager.Instancia.UsuarioActual.Perfil;
            int perfilId = SesionManager.Instancia.UsuarioActual.PerfilId;

            if (perfil == "Admin")
                if (perfil == "Admin" || perfilId == 1)
                {
                    btnUsuarios.Visible = true;
                    btnPerfiles.Visible = true;
                    btnVentas.Visible = true;
                    btnReporte.Visible = true;
                    btnApuesta.Visible = true;
                }
                else // Operador
                {
                    btnUsuarios.Visible = false;
                    btnPerfiles.Visible = false;
                    btnVentas.Visible = false;
                    btnReporte.Visible = false;
                    btnApuesta.Visible = true;
                }
        }
        public void ActualizarIdioma(Idioma idioma)
        {
            // Pedimos a la BLL el texto de cada botón usando su clave
            btnUsuarios.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_usuarios");
            btnPerfiles.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_perfiles");
            btnVentas.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_ventas");
            btnReporte.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_reporte");
            btnApuesta.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_apuesta");
            btnLogout.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_logout");
        }
        private void btnUsuarios_Click(object sender, System.EventArgs e)
        {
            FormUsuarios fu = new FormUsuarios();
            fu.MdiParent = this;
            fu.Show();
        }

        private void btnPerfiles_Click(object sender, System.EventArgs e)
        {
            FormPerfiles fp = new FormPerfiles();
            fp.MdiParent = this;
            fp.Show();
        }

        private void btnVentas_Click(object sender, System.EventArgs e)
        {
            FormVentas fv = new FormVentas();
            fv.MdiParent = this;
            fv.Show();
        }

        private void btnReporte_Click(object sender, System.EventArgs e)
        {
            FormReporte fr = new FormReporte();
            fr.MdiParent = this;
            fr.Show();
        }

        private void btnApuesta_Click(object sender, System.EventArgs e)
        {
            FormApuesta fa = new FormApuesta();
            fa.MdiParent = this;
            fa.Show();
        }

        private void btnLogout_Click(object sender, System.EventArgs e)
        {
            BitacoraBLL bitacora = new BitacoraBLL();
            bitacora.Registrar(
                SesionManager.Instancia.UsuarioActual.Id,
                SesionManager.Instancia.UsuarioActual.Nombre,
                "Logout",
                "Cierre de sesion"
            );
            bll.Logout();
            FormLogin fl = new FormLogin();
            fl.Show();
            this.Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            IdiomaManager.Instancia.Desuscribir(this);
            base.OnFormClosing(e);
        }
        private void btnBitacora_Click(object sender, System.EventArgs e)
        {
            FormBitacora fb = new FormBitacora();
            fb.MdiParent = this;
            fb.Show();
        }

    }
}


