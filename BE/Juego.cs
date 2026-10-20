namespace BE
{
    // Un juego de la agencia (Quiniela, Quini 6, Loto 5...). Define las reglas con las que
    // se arman sus jugadas y se realizan sus sorteos.
    public class Juego
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public int CantidadNumeros { get; set; }     // cuantos numeros se eligen por jugada
        public int NumeroMin { get; set; }
        public int NumeroMax { get; set; }
        public int CantidadSorteados { get; set; }   // cuantos numeros salen en cada sorteo
        public bool UsaRango { get; set; }           // se juega a una posicion (solo Quiniela)
        public bool UsaLoteriaTurno { get; set; }    // se sortea por loteria y turno (solo Quiniela)
        public decimal? Precio { get; set; }         // precio fijo de la jugada; null = lo elige el cliente
        public bool Activo { get; set; }
    }
}
