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

        public FormPerfiles()
        {
            InitializeComponent();
            lstFamilias.SelectedIndexChanged += LstFamilias_SelectedIndexChanged;
            CargarDatos();
        }

        private void LstFamilias_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeViewPreview.Nodes.Clear();
            if (lstFamilias.SelectedItem != null)
            {
                Permiso p = (Permiso)lstFamilias.SelectedItem;
                TreeNode root = new TreeNode(p.Nombre);
                root.Tag = p;
                
                // Cargar los hijos actuales de la familia para previsualizar si ya tiene
                PermisoCompleto arbol = bll.ObtenerArbol(p.Id) as PermisoCompleto;
                if (arbol != null)
                {
                    ConstruirNodosHijos(root, arbol);
                }

                treeViewPreview.Nodes.Add(root);
                treeViewPreview.ExpandAll();
            }
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

            // Cargar TreeView izquierdo (todos)
            treeViewPermisos.Nodes.Clear();
            foreach (var familia in todosLosPermisos.Where(p => p.EsPadre))
            {
                PermisoCompleto arbol = bll.ObtenerArbol(familia.Id) as PermisoCompleto;
                if (arbol != null)
                {
                    TreeNode nodo = new TreeNode(arbol.Nombre);
                    nodo.Tag = arbol;
                    ConstruirNodosHijos(nodo, arbol);
                    treeViewPermisos.Nodes.Add(nodo);
                }
            }
            if(lstFamilias.SelectedItem != null) {
                LstFamilias_SelectedIndexChanged(null, null);
            }
        }

        private void ConstruirNodosHijos(TreeNode padre, PermisoCompleto permisoCompleto)
        {
            foreach (Permiso hijo in permisoCompleto.SubPermisos)
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
            if (lstFamilias.SelectedItem != null)
            {
                Permiso p = (Permiso)lstFamilias.SelectedItem;
                AgregarAlPreview(p);
            }
        }

        private void btnAgregarPatentePreview_Click(object sender, EventArgs e)
        {
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
                if (arbol != null) ConstruirNodosHijos(nodo, arbol);
            }
            
            root.Nodes.Add(nodo);
            root.ExpandAll();
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
            if (lstFamilias.SelectedItem == null || treeViewPreview.Nodes.Count == 0)
            {
                MessageBox.Show("Debe seleccionar una familia de la lista central para guardar su configuracion.");
                return;
            }

            Permiso familiaDestino = (Permiso)lstFamilias.SelectedItem;
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
