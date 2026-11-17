using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormBitacora : Form
    {
        private BitacoraBLL bll = new BitacoraBLL();

        // Filtros de la ultima busqueda: se guardan en el JSON exportado junto con los registros.
        private DateTime ultimoDesde;
        private DateTime ultimoHasta;
        private string ultimoFiltro = "";

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
                // La columna "Perfil" viene completada desde BitacoraBLL.Listar.
                dgvBitacora.DataSource = bll.Listar(
                    dtpDesde.Value,
                    dtpHasta.Value,
                    txtBuscar.Text.Trim()
                );
                ultimoDesde = dtpDesde.Value;
                ultimoHasta = dtpHasta.Value;
                ultimoFiltro = txtBuscar.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Exporta a un archivo JSON los registros que se estan viendo en la grilla.
        private void btnExportar_Click(object sender, EventArgs e)
        {
            List<Bitacora> registros = dgvBitacora.DataSource as List<Bitacora>;
            if (registros == null || registros.Count == 0)
            {
                MessageBox.Show("No hay registros para exportar. Realizá una búsqueda primero.",
                    "Exportar bitácora", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Exportar bitácora";
                dlg.Filter = "Archivo JSON (*.json)|*.json";
                dlg.DefaultExt = "json";
                dlg.AddExtension = true;
                dlg.FileName = "Bitacora_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    bll.ExportarJson(registros, ultimoDesde, ultimoHasta, ultimoFiltro, dlg.FileName);
                    MessageBox.Show("Se exportaron " + registros.Count + " registros a:\n" + dlg.FileName,
                        "Exportar bitácora", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo exportar la bitácora: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
