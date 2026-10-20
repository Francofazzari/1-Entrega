using System.Collections.Generic;

namespace BE
{
    // Una apuesta concreta a UN sorteo, dentro de un ticket. En Quiniela tiene tipo (Directa o
    // Redoblona); en todos los juegos puede ser manual o automatica (la maquina elige).
    public class Jugada
    {
        public int Id { get; set; }
        public Sorteo Sorteo { get; set; }
        public string Tipo { get; set; }            // Directa / Redoblona (solo Quiniela); null en los demas
        public bool EsAutomatica { get; set; }
        public decimal Monto { get; set; }
        public int Orden { get; set; }
        public List<NumeroElegido> Numeros { get; set; } = new List<NumeroElegido>();
    }
}
