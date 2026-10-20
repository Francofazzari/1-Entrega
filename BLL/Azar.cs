using System;
using System.Security.Cryptography;

namespace BLL
{
    // Fuente de azar unica para las jugadas automaticas y los sorteos simulados. Usa el
    // generador criptografico del sistema: dos Random comunes creados en el mismo instante
    // arrancan con la misma semilla y repiten la misma secuencia, y una jugada automatica
    // terminaria "adivinando" el resultado del sorteo.
    internal static class Azar
    {
        private static readonly RandomNumberGenerator generador = RandomNumberGenerator.Create();
        private static readonly object candado = new object();

        // Entero al azar entre min y max, ambos incluidos, sin sesgo.
        public static int Entre(int min, int max)
        {
            uint rango = (uint)(max - min + 1);
            uint limite = uint.MaxValue - uint.MaxValue % rango;   // descarta los valores que sesgarian el resto
            byte[] buffer = new byte[4];
            uint valor;
            lock (candado)
            {
                do
                {
                    generador.GetBytes(buffer);
                    valor = BitConverter.ToUInt32(buffer, 0);
                } while (valor >= limite);
            }
            return (int)(min + valor % rango);
        }
    }
}
