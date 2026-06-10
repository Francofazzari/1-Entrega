using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BE
{
    public class PermisoSimple : Permiso
    {
        public override void MostrarArbol(int nivel, TreeNodeCollection nodos)
        {
            TreeNode nodo = new TreeNode($"[{Codigo}] {Nombre}");
            nodo.Tag = this;
            nodos.Add(nodo);
        }
    }
}