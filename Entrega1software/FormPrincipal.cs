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
            bool isAdmin = false;
            foreach (var p in SesionManager.Instancia.UsuarioActual.Permisos)
            {
                if (p.Codigo == "Admin") isAdmin = true;
            }

            if (isAdmin)
            {
                btnUsuarios.Visible = true;
                btnPerfiles.Visible = true;
                btnVentas.Visible = true;
                btnReporte.Visible = true;
                btnApuesta.Visible = true;
                btnCambios.Visible = true;
                btnIdiomas.Visible = true;
            }
            else // Operador
            {
                btnUsuarios.Visible = false;
                btnPerfiles.Visible = false;
                btnVentas.Visible = false;
                btnReporte.Visible = false;
                btnApuesta.Visible = true;
                btnCambios.Visible = false;
                btnIdiomas.Visible = false;
            }
        }
        public void ActualizarIdioma(Idioma idioma)
        {
            btnUsuarios.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_usuarios");
            btnPerfiles.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_perfiles");
            btnVentas.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_ventas");
            btnReporte.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_reporte");
            btnApuesta.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_apuesta");
            btnLogout.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_logout");
            btnIdiomas.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_idiomas") == "btn_idiomas" ? "Gestion Idiomas" : IdiomaManager.Instancia.ObtenerMensaje("btn_idiomas");
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

        private void btnIdiomas_Click(object sender, System.EventArgs e)
        {
            FormControlIdiomas fci = new FormControlIdiomas();
            fci.MdiParent = this;
            fci.Show();
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
        private void btnCambios_Click(object sender, System.EventArgs e)
        {
            FormControlCambios fcc = new FormControlCambios();
            fcc.MdiParent = this;
            fcc.Show();
        }
    }
}
