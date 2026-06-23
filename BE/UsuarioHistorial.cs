using System;

namespace BE
{
    public class UsuarioHistorial : Usuario
    {
        public int IdHistorial { get; set; }
        public DateTime FechaCambio { get; set; }
        public string ResponsableNombre { get; set; }
        public string Operacion { get; set; }
    }
}
