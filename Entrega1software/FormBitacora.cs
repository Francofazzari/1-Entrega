using System;
using System.Windows.Forms;
using BLL;

namespace Entrega1software
{
    public partial class FormBitacora : Form
    {
        private BitacoraBLL bll = new BitacoraBLL();

        public FormBitacora()
        {
            InitializeComponent();
            // Busca todo del día de hoy por defecto
            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;
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
    }
}