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
    public partial class FormPrincipal : Form
    {
        private UsuarioBLL bll = new UsuarioBLL();
        public FormPrincipal()
        {
            InitializeComponent();

            this.IsMdiContainer = true;
            lblUsuario.Text = "Usuario: " +
                SesionManager.Instancia.UsuarioActual.Nombre + " " +
                SesionManager.Instancia.UsuarioActual.Apellido;
        }
        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            FormUsuarios fu = new FormUsuarios();
            fu.MdiParent = this;
            fu.Show();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            bll.Logout();
            FormLogin fl = new FormLogin();
            fl.Show();
            this.Close();
        }
    }
}
