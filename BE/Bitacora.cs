using System;

namespace BE
{
    public class Bitacora
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public string Actividad { get; set; }
        public string Detalle { get; set; }
    }
}