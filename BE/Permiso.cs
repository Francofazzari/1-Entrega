using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public abstract class Permiso
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public int? PermisoPadreId { get; set; }

        public abstract void MostrarArbol(int nivel, System.Windows.Forms.TreeNodeCollection nodos);
    }
}
