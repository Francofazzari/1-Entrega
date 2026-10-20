using System;

namespace BE
{
    // El cobro de un ticket premiado. Un ticket se paga una sola vez.
    public class Pago
    {
        public int Id { get; set; }
        public int IdApuesta { get; set; }
        public Guid CodigoTicket { get; set; }      // para mostrar
        public string JuegoNombre { get; set; }     // para mostrar
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }   // quien lo pago (para mostrar)
        public DateTime FechaHora { get; set; }
        public decimal Monto { get; set; }
    }
}
