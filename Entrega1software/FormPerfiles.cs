using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormPerfiles : Form
    {
        private PerfilBLL bll = new PerfilBLL();
        private UsuarioBLL usuarioBLL = new UsuarioBLL();

        // Guarda Id del usuario por nombre
        private Dictionary<string, int> empleadosIds = new Dictionary<string, int>();
        private string empleadoSeleccionado = null;

        public FormPerfiles()
        {
            InitializeComponent();
            CargarFuncionesDisponibles();
            CargarArbolDesdeDB();
        }

        private void CargarFuncionesDisponibles()
        {
            lstFunciones.Items.Clear();
            lstFunciones.Items.Add("Gestion de Empleados");
            lstFunciones.Items.Add("Cargar Empleado");
            lstFunciones.Items.Add("Modificar Empleado");
            lstFunciones.Items.Add("Eliminar Empleado");
            lstFunciones.Items.Add("Ventas en Linea");
            lstFunciones.Items.Add("Ver Ventas de Operadores");
            lstFunciones.Items.Add("Reporte Semanal");
            lstFunciones.Items.Add("Apuestas");
            lstFunciones.Items.Add("Cargar Apuesta");
            lstFunciones.Items.Add("Cobrar Jugada");
        }

        private void CargarArbolDesdeDB()
        {
            treeViewPermisos.Nodes.Clear();
            empleadosIds.Clear();

            // Carga todos los usuarios de la BD
            List<Usuario> usuarios = usuarioBLL.ObtenerTodos();

            foreach (Usuario u in usuarios)
            {
                TreeNode nodoUsuario = new TreeNode($"{u.Nombre} [{u.NroTerminal}]");
                nodoUsuario.Tag = u.Id;

                // Carga las funciones asignadas al usuario
                List<string> funciones = bll.ObtenerFuncionesDeUsuario(u.Id);
                foreach (string f in funciones)
                    nodoUsuario.Nodes.Add(f);

                treeViewPermisos.Nodes.Add(nodoUsuario);
                empleadosIds[u.Nombre] = u.Id;
            }

            treeViewPermisos.ExpandAll();

            treeViewPermisos.NodeMouseClick += (s, e) =>
            {
                if (e.Node.Level == 0)
                    empleadoSeleccionado = e.Node.Text;
            };
        }

        private void btnAgregarEmpleado_Click(object sender, EventArgs e)
        {
            string nombre = txtNombreEmpleado.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingrese el nombre del empleado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Genera nro de terminal automático
            string nroTerminal = "T" + DateTime.Now.Ticks.ToString().Substring(10, 4);

            try
            {
                int nuevoId = bll.CrearEmpleado(nombre, nroTerminal);
                if (nuevoId > 0)
                {
                    empleadosIds[nombre] = nuevoId;
                    TreeNode nodo = new TreeNode($"{nombre} [{nroTerminal}]");
                    nodo.Tag = nuevoId;
                    treeViewPermisos.Nodes.Add(nodo);
                    treeViewPermisos.ExpandAll();
                    empleadoSeleccionado = $"{nombre} [{nroTerminal}]";
                    txtNombreEmpleado.Text = "";
                    MessageBox.Show($"Empleado '{nombre}' agregado. Terminal: {nroTerminal}\nClave por defecto: 1234");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAsignar_Click(object sender, EventArgs e)
        {
            if (empleadoSeleccionado == null)
            {
                MessageBox.Show("Primero haga clic en un empleado del árbol.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstFunciones.CheckedItems.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una función.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtiene el nodo seleccionado
            TreeNode nodoEmpleado = null;
            foreach (TreeNode n in treeViewPermisos.Nodes)
            {
                if (n.Text == empleadoSeleccionado)
                {
                    nodoEmpleado = n;
                    break;
                }
            }
            if (nodoEmpleado == null) return;

            int usuarioId = (int)nodoEmpleado.Tag;

            // Arma lista de funciones
            List<string> funciones = new List<string>();
            foreach (string f in lstFunciones.CheckedItems)
                funciones.Add(f);

            try
            {
                // Obtiene el nombre sin el terminal
                string nombreEmpleado = empleadoSeleccionado.Split('[')[0].Trim();
                bool ok = bll.AsignarFunciones(usuarioId, nombreEmpleado, funciones);

                if (ok)
                {
                    // Actualiza el TreeView
                    foreach (string f in funciones)
                        nodoEmpleado.Nodes.Add(f);

                    treeViewPermisos.ExpandAll();

                    // Destilda los checks
                    for (int i = 0; i < lstFunciones.Items.Count; i++)
                        lstFunciones.SetItemChecked(i, false);

                    MessageBox.Show("Funciones asignadas correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}