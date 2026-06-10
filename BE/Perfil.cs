using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public PermisoCompleto PermisoRaiz { get; set; }

        public int PermisoRaizId { get; set; }
     

    }
}
