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
        
        // Determina si es Familia (True) o Patente (False)
        public bool EsPadre { get; set; }

        public abstract void MostrarArbol(int nivel, System.Windows.Forms.TreeNodeCollection nodos);
    }
}
