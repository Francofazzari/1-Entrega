using System;
using System.Collections.Generic;

namespace BE
{
    public class Apuesta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaHora { get; set; }
        public string Tipo { get; set; }
        public decimal Total { get; set; }
        public bool Activa { get; set; } = true;
        public List<Loteria> Loterias { get; set; } = new List<Loteria>();
        public List<Turno> Turnos { get; set; } = new List<Turno>();
        public List<Jugada> Jugadas { get; set; } = new List<Jugada>();
    }
}
