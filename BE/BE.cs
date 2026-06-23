using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NroTerminal { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
        public bool Activo { get; set; }
    }
}
