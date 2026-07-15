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
    public partial class FormUsuarios : Form
    {
        private UsuarioBLL bll = new UsuarioBLL();
        private int idSeleccionado = 0;
        public FormUsuarios()
        {
            InitializeComponent();
            // DataBindingComplete se dispara siempre que el grid termina de enlazar su
            // DataSource (sin importar si el formulario ya tiene handle de ventana o no,
            // ni cuantas veces se reabra) - es el lugar correcto para agregar una columna
            // calculada como "Perfil", que no viene del binding.
            dgvUsuarios.DataBindingComplete += DgvUsuarios_DataBindingComplete;
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            List<Usuario> usuarios = bll.ObtenerTodos();
            dgvUsuarios.DataSource = usuarios;
        }

        private void DgvUsuarios_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvUsuarios.Columns["Clave"] != null)
                dgvUsuarios.Columns["Clave"].Visible = false;
            if (dgvUsuarios.Columns["Permisos"] != null)
                dgvUsuarios.Columns["Permisos"].Visible = false;

            if (!dgvUsuarios.Columns.Contains("Perfil"))
                dgvUsuarios.Columns.Add("Perfil", "Perfil");

            PerfilBLL perfilBll = new PerfilBLL();
            foreach (DataGridViewRow fila in dgvUsuarios.Rows)
            {
                Usuario u = fila.DataBoundItem as Usuario;
                if (u == null) continue;
                fila.Cells["Perfil"].Value = perfilBll.ObtenerPerfilDeUsuario(u.Id);
            }
        }

        private static string CodigoPerfil(string nombrePerfil)
        {
            if (nombrePerfil == "Administrador") return "Admin";
            if (nombrePerfil == "Operador") return "Operador";
            return null;
        }
       
        
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                Usuario u = new Usuario
                {
                    NroTerminal = txtTerminal.Text.Trim(),
                    Clave = txtClave.Text,
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim()
                };
                bll.AgregarUsuario(u);
                new PerfilBLL().AsignarPerfil(u.Id, cmbPerfil.SelectedItem.ToString());
                MessageBox.Show("Usuario agregado correctamente.");
                BitacoraBLL bitacora = new BitacoraBLL();
                bitacora.Registrar(
                    SesionManager.Instancia.UsuarioActual.Id,
                    SesionManager.Instancia.UsuarioActual.Nombre,
                    "Alta Usuario",
                    $"Se creo el usuario {u.NroTerminal}"
                );
                CargarUsuarios();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (idSeleccionado == 0)
                {
                    MessageBox.Show("Seleccione un usuario de la lista.");
                    return;
                }
                Usuario u = new Usuario
                {
                    Id = idSeleccionado,
                    NroTerminal = txtTerminal.Text.Trim(),
                    Clave = txtClave.Text,
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Activo = true // este panel no tiene control para desactivar (eso lo hace "Eliminar")
                };
                bll.ModificarUsuario(u);
                new PerfilBLL().AsignarPerfil(u.Id, cmbPerfil.SelectedItem.ToString());
                MessageBox.Show("Usuario modificado correctamente.");
                CargarUsuarios();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un usuario de la lista.");
                return;
            }
            var confirm = MessageBox.Show("¿Desea eliminar el usuario?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                bll.EliminarUsuario(idSeleccionado);
                MessageBox.Show("Usuario eliminado correctamente.");
                CargarUsuarios();
                LimpiarCampos();
            }
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvUsuarios.Rows[e.RowIndex];
                idSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);
                txtTerminal.Text = fila.Cells["NroTerminal"].Value.ToString();
                txtNombre.Text = fila.Cells["Nombre"].Value.ToString();
                txtApellido.Text = fila.Cells["Apellido"].Value.ToString();
                txtClave.Text = "";

                string perfilActual = fila.Cells["Perfil"].Value?.ToString();
                string codigo = CodigoPerfil(perfilActual);
                if (codigo != null) cmbPerfil.SelectedItem = codigo;
            }
        }

        private void LimpiarCampos()
        {
            idSeleccionado = 0;
            txtTerminal.Text = "";
            txtClave.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            cmbPerfil.SelectedIndex = 1;
        }

    }
}
