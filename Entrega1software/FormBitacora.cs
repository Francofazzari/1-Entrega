using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormBitacora : Form
    {
        private BitacoraBLL bll = new BitacoraBLL();
        private PerfilBLL perfilBll = new PerfilBLL();

        public FormBitacora()
        {
            InitializeComponent();
            // Busca todo del día de hoy por defecto
            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;
            // DataBindingComplete se dispara siempre que el grid termina de enlazar su
            // DataSource, sin importar el orden con el constructor/Load - ahi se agrega la
            // columna calculada "Perfil".
            dgvBitacora.DataBindingComplete += DgvBitacora_DataBindingComplete;
            Buscar();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            Buscar();
        }

        private void Buscar()
        {
            try
            {
                dgvBitacora.DataSource = bll.Listar(
                    dtpDesde.Value,
                    dtpHasta.Value,
                    txtBuscar.Text.Trim()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DgvBitacora_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvBitacora.Columns.Contains("Perfil"))
                dgvBitacora.Columns.Add("Perfil", "Perfil");

            foreach (DataGridViewRow fila in dgvBitacora.Rows)
            {
                Bitacora b = fila.DataBoundItem as Bitacora;
                if (b == null) continue;
                fila.Cells["Perfil"].Value = perfilBll.ObtenerPerfilDeUsuario(b.UsuarioId);
            }
        }
    }
}