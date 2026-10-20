namespace BE
{
    // Un numero de una jugada, elegido por el cliente o por la maquina. En Quiniela lleva la
    // posicion buscada (Rango): 1 = a la primera, 5, 10 o 20. En la redoblona cada numero
    // tiene la suya, y el Orden dice cual es el primero y cual el segundo.
    public class NumeroElegido
    {
        public string Numero { get; set; }
        public int Orden { get; set; }
        public int? Rango { get; set; }
    }
}
