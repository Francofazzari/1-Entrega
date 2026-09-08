namespace BE
{
    // Un numero cargado dentro de una Apuesta: de 1 a 4 cifras, con el rango
    // (posicion del sorteo a la que apunta: 1, 5, 10 o 20) y el monto con el
    // que se juega ese numero en particular.
    public class Jugada
    {
        public string Numero { get; set; }
        public int Rango { get; set; }
        public decimal Monto { get; set; }
    }
}
