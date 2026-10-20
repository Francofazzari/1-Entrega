namespace BE
{
    // Lo que le corresponde a una jugada al compararla con el resultado de su sorteo. No se
    // guarda: se calcula cuando se consulta o se paga el ticket.
    public class Premio
    {
        public Jugada Jugada { get; set; }
        public bool SorteoRealizado { get; set; }
        public string Detalle { get; set; }
        public decimal Importe { get; set; }
    }
}
