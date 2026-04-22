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
    public partial class FormLogin : Form
    {

        private UsuarioBLL bll = new UsuarioBLL();
        public FormLogin()
        {
            InitializeComponent();
        }
        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                bool ok = bll.Login(txtTerminal.Text.Trim(), txtClave.Text);
                if (ok)
                {
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



    }
}
