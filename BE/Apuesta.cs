using System;
using System.Collections.Generic;

namespace BE
{
    // El ticket que se lleva el cliente. Es de un solo juego y contiene una o varias jugadas.
    // Se identifica con un codigo unico (GUID): con ese codigo se paga, se cancela o se
    // reimprime, sea el juego que sea.
    public class Apuesta
    {
        public int Id { get; set; }
        public Guid CodigoTicket { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }   // vendedor (para mostrar)
        public DateTime FechaHora { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }          // Vigente, Pagada o Anulada
        public Juego Juego { get; set; }            // se deduce de sus jugadas (jugada -> sorteo -> juego)
        public List<Jugada> Jugadas { get; set; } = new List<Jugada>();
    }
}
