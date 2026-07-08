using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormPerfiles : Form
    {
        private PerfilBLL bll = new PerfilBLL();
        private List<Permiso> todosLosPermisos = new List<Permiso>();

        // Familia que se esta armando/editando en el treeViewPreview. Se fija con doble clic
        // en lstFamilias, para que un clic simple (usado para elegir que agregar) no la pise.
        private Permiso familiaEnEdicion;

        public FormPerfiles()
        {
            InitializeComponent();
            lstFamilias.DoubleClick += LstFamilias_DoubleClick;
            CargarDatos();
        }

        private void LstFamilias_DoubleClick(object sender, EventArgs e)
        {
            if (lstFamilias.SelectedItem != null)
            {
                CargarFamiliaEnPreview((Permiso)lstFamilias.SelectedItem);
            }
        }

        private void CargarFamiliaEnPreview(Permiso familia)
        {
            familiaEnEdicion = familia;
            lblEditando.Text = "Editando: " + familia.Nombre;

            treeViewPreview.Nodes.Clear();
            TreeNode root = new TreeNode(familia.Nombre);
            root.Tag = familia;

            // Cargar los hijos actuales de la familia para previsualizar si ya tiene
            PermisoCompleto arbol = bll.ObtenerArbol(familia.Id) as PermisoCompleto;
            if (arbol != null)
            {
                ConstruirNodosHijos(root, arbol);
            }

            treeViewPreview.Nodes.Add(root);
            treeViewPreview.ExpandAll();
        }

        private void CargarDatos()
        {
            todosLosPermisos = bll.ObtenerTodos();
            
            // Cargar listas
            lstFamilias.DataSource = null;
            lstFamilias.DataSource = todosLosPermisos.Where(p => p.EsPadre).ToList();
            lstFamilias.DisplayMember = "Nombre";

            lstPatentes.DataSource = null;
            lstPatentes.DataSource = todosLosPermisos.Where(p => !p.EsPadre).ToList();
            lstPatentes.DisplayMember = "Nombre";

            // Cargar TreeView izquierdo (todos), ordenado alfabeticamente
            treeViewPermisos.Nodes.Clear();
            TreeNode systemRoot = new TreeNode("S.Y.S.T.E.M.");
            foreach (var familia in todosLosPermisos.Where(p => p.EsPadre).OrderBy(p => p.Nombre))
            {
                PermisoCompleto arbol = bll.ObtenerArbol(familia.Id) as PermisoCompleto;
                if (arbol != null)
                {
                    TreeNode nodo = new TreeNode(arbol.Nombre);
                    nodo.Tag = arbol;
                    ConstruirNodosHijos(nodo, arbol);
                    systemRoot.Nodes.Add(nodo);
                }
            }
            treeViewPermisos.Nodes.Add(systemRoot);
            treeViewPermisos.ExpandAll();

            // Si habia una familia en edicion, refrescar su preview con los datos ya guardados
            if (familiaEnEdicion != null)
            {
                Permiso actualizada = todosLosPermisos.FirstOrDefault(p => p.Id == familiaEnEdicion.Id);
                if (actualizada != null)
                {
                    CargarFamiliaEnPreview(actualizada);
                }
            }
        }

        private void ConstruirNodosHijos(TreeNode padre, PermisoCompleto permisoCompleto)
        {
            foreach (Permiso hijo in permisoCompleto.SubPermisos.OrderBy(p => p.Nombre))
            {
                TreeNode nodoHijo = new TreeNode(hijo.Nombre);
                nodoHijo.Tag = hijo;
                padre.Nodes.Add(nodoHijo);

                if (hijo is PermisoCompleto pc)
                {
                    ConstruirNodosHijos(nodoHijo, pc);
                }
            }
        }

        private void btnAgregarFamiliaLista_Click(object sender, EventArgs e)
        {
            try
            {
                bll.AgregarFamilia(txtNombreFamilia.Text.Trim());
                MessageBox.Show("Familia creada");
                txtNombreFamilia.Text = "";
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAgregarFamiliaPreview_Click(object sender, EventArgs e)
        {
            if (familiaEnEdicion == null)
            {
                MessageBox.Show("Primero haga doble clic en una familia de la lista para editarla.");
                return;
            }
            if (lstFamilias.SelectedItem != null)
            {
                Permiso p = (Permiso)lstFamilias.SelectedItem;
                AgregarAlPreview(p);
            }
        }

        private void btnAgregarPatentePreview_Click(object sender, EventArgs e)
        {
            if (familiaEnEdicion == null)
            {
                MessageBox.Show("Primero haga doble clic en una familia de la lista para editarla.");
                return;
            }
            if (lstPatentes.SelectedItem != null)
            {
                Permiso p = (Permiso)lstPatentes.SelectedItem;
                AgregarAlPreview(p);
            }
        }

        private void AgregarAlPreview(Permiso p)
        {
            if (treeViewPreview.Nodes.Count == 0) return;
            TreeNode root = treeViewPreview.Nodes[0];

            if (p.Id == familiaEnEdicion.Id)
            {
                MessageBox.Show("Una familia no puede contenerse a si misma.");
                return;
            }

            // Evitar duplicados directos
            foreach (TreeNode n in root.Nodes)
            {
                if (((Permiso)n.Tag).Id == p.Id) return;
            }

            TreeNode nodo = new TreeNode(p.Nombre);
            nodo.Tag = p;

            // Si es familia, la mostramos completa en el preview
            if (p.EsPadre)
            {
                PermisoCompleto arbol = bll.ObtenerArbol(p.Id) as PermisoCompleto;
                if (arbol != null)
                {
                    if (ContieneA(arbol, familiaEnEdicion.Id))
                    {
                        MessageBox.Show(
                            "'" + p.Nombre + "' ya contiene (directa o indirectamente) a '" + familiaEnEdicion.Nombre +
                            "'. Agregarla generaria un ciclo.");
                        return;
                    }
                    ConstruirNodosHijos(nodo, arbol);
                }
            }

            root.Nodes.Add(nodo);
            root.ExpandAll();
        }

        // Recorre recursivamente una familia para saber si contiene (en cualquier nivel) al permiso indicado.
        private bool ContieneA(PermisoCompleto familia, int idBuscado)
        {
            foreach (Permiso hijo in familia.SubPermisos)
            {
                if (hijo.Id == idBuscado) return true;
                if (hijo is PermisoCompleto sub && ContieneA(sub, idBuscado)) return true;
            }
            return false;
        }

        private void btnEliminarSeleccionado_Click(object sender, EventArgs e)
        {
            if (treeViewPreview.SelectedNode != null && treeViewPreview.SelectedNode.Parent != null)
            {
                treeViewPreview.SelectedNode.Remove();
            }
        }

        private void btnGuardarFamilia_Click(object sender, EventArgs e)
        {
            if (familiaEnEdicion == null || treeViewPreview.Nodes.Count == 0)
            {
                MessageBox.Show("Debe hacer doble clic en una familia de la lista central para editarla antes de guardar.");
                return;
            }

            Permiso familiaDestino = familiaEnEdicion;
            TreeNode root = treeViewPreview.Nodes[0];

            List<int> idsHijos = new List<int>();
            foreach (TreeNode n in root.Nodes)
            {
                idsHijos.Add(((Permiso)n.Tag).Id);
            }

            if (bll.GuardarFamilia(familiaDestino.Id, idsHijos))
            {
                MessageBox.Show("Familia guardada correctamente");
                CargarDatos();
            }
            else
            {
                MessageBox.Show("Error al guardar la familia");
            }
        }
    }
}
