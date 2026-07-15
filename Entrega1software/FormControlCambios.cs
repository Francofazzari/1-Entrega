using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public class FormControlCambios : Form
    {
        private UsuarioBLL usuarioBLL = new UsuarioBLL();
        private PerfilBLL perfilBLL = new PerfilBLL();
        
        private ComboBox cmbUsuarios;
        private DataGridView dgvHistorial;
        private Button btnRestaurar;
        private Label lblTitulo;

        public FormControlCambios()
        {
            InitializeComponent();
            // DataBindingComplete se dispara siempre que dgvHistorial termina de enlazar su
            // DataSource (sin importar el orden con el constructor/Load) - ahi se agrega la
            // columna calculada "Perfil".
            dgvHistorial.DataBindingComplete += DgvHistorial_DataBindingComplete;
            CargarUsuarios();
        }

        private void InitializeComponent()
        {
            this.cmbUsuarios = new System.Windows.Forms.ComboBox();
            this.dgvHistorial = new System.Windows.Forms.DataGridView();
            this.btnRestaurar = new System.Windows.Forms.Button();
            this.lblTitulo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).BeginInit();
            this.SuspendLayout();
            
            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Location = new System.Drawing.Point(12, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(100, 13);
            this.lblTitulo.Text = "Seleccione Usuario:";

            // cmbUsuarios
            this.cmbUsuarios.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUsuarios.FormattingEnabled = true;
            this.cmbUsuarios.Location = new System.Drawing.Point(120, 12);
            this.cmbUsuarios.Name = "cmbUsuarios";
            this.cmbUsuarios.Size = new System.Drawing.Size(200, 21);
            this.cmbUsuarios.SelectedIndexChanged += new System.EventHandler(this.cmbUsuarios_SelectedIndexChanged);
            
            // dgvHistorial
            this.dgvHistorial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistorial.Location = new System.Drawing.Point(12, 50);
            this.dgvHistorial.Name = "dgvHistorial";
            this.dgvHistorial.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistorial.MultiSelect = false;
            this.dgvHistorial.ReadOnly = true;
            this.dgvHistorial.Size = new System.Drawing.Size(760, 350);
            
            // btnRestaurar
            this.btnRestaurar.Location = new System.Drawing.Point(12, 415);
            this.btnRestaurar.Name = "btnRestaurar";
            this.btnRestaurar.Size = new System.Drawing.Size(150, 30);
            this.btnRestaurar.Text = "Restaurar Version";
            this.btnRestaurar.UseVisualStyleBackColor = true;
            this.btnRestaurar.Click += new System.EventHandler(this.btnRestaurar_Click);
            
            // FormControlCambios
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.btnRestaurar);
            this.Controls.Add(this.dgvHistorial);
            this.Controls.Add(this.cmbUsuarios);
            this.Name = "FormControlCambios";
            this.Text = "Control de Cambios - Historial de Usuarios";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CargarUsuarios()
        {
            var usuarios = usuarioBLL.ObtenerTodos();
            cmbUsuarios.DataSource = usuarios;
            cmbUsuarios.DisplayMember = "Nombre";
        }

        private void cmbUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUsuarios.SelectedItem != null)
            {
                Usuario u = (Usuario)cmbUsuarios.SelectedItem;
                dgvHistorial.DataSource = usuarioBLL.ObtenerHistorial(u.Id);
            }
        }

        private void DgvHistorial_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dgvHistorial.Columns.Contains("Perfil"))
                dgvHistorial.Columns.Add("Perfil", "Perfil");

            foreach (DataGridViewRow fila in dgvHistorial.Rows)
            {
                UsuarioHistorial uh = fila.DataBoundItem as UsuarioHistorial;
                if (uh == null) continue;

                List<int> ids = (uh.PermisosIds ?? "")
                    .Split(',')
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
                fila.Cells["Perfil"].Value = perfilBLL.ObtenerPerfilDesdeIds(ids);
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (dgvHistorial.SelectedRows.Count > 0)
            {
                var row = dgvHistorial.SelectedRows[0];
                int idHistorial = Convert.ToInt32(row.Cells["IdHistorial"].Value);
                
                if (usuarioBLL.RestaurarVersion(idHistorial))
                {
                    MessageBox.Show("Version restaurada correctamente.");
                    cmbUsuarios_SelectedIndexChanged(null, null); // refrescar
                }
                else
                {
                    MessageBox.Show("Error al restaurar.");
                }
            }
            else
            {
                MessageBox.Show("Seleccione una version del historial.");
            }
        }
    }
}
