using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BE
{
    public class PermisoCompleto : Permiso
    {
        public List<Permiso> SubPermisos { get; set; } = new List<Permiso>();

        public void Agregar(Permiso p)
        {
            SubPermisos.Add(p);
        }

        public void Eliminar(Permiso p)
        {
            SubPermisos.Remove(p);
        }

        public override void MostrarArbol(int nivel, TreeNodeCollection nodos)
        {
            // Crea el nodo del compuesto
            TreeNode nodo = new TreeNode($"[{Codigo}] {Nombre}");
            nodo.Tag = this;
            nodos.Add(nodo);

            // Llama recursivamente a cada hijo
            foreach (Permiso sub in SubPermisos)
            {
                sub.MostrarArbol(nivel + 1, nodo.Nodes);
            }
        }
    }
}