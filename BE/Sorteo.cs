using System;

namespace BE
{
    // Un evento concreto de sorteo: un juego, en una fecha y, si el juego lo necesita, de una
    // loteria y un turno. Todas las jugadas, sean del juego que sean, apuntan a un sorteo.
    public class Sorteo
    {
        public int Id { get; set; }
        public Juego Juego { get; set; }
        public Loteria Loteria { get; set; }   // null en los juegos que no se sortean por loteria
        public Turno Turno { get; set; }       // null en los juegos que no tienen turnos
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }     // Abierto, Cerrado o Sorteado
    }
}
