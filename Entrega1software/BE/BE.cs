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
        public int PerfilId { get; set; }
        public string Perfil { get; set; }
        public bool Activo { get; set; }
    }
}
