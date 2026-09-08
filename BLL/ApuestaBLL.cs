using System;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class ApuestaBLL
    {
        public const decimal MontoMinimo = 100;
        public const decimal MontoMaximo = 5000;
        private static readonly int[] RangosValidos = { 1, 5, 10, 20 };

        private ApuestaDAL dal = new ApuestaDAL();

        public bool CargarApuesta(Apuesta a)
        {
            if (a.Loterias == null || a.Loterias.Count == 0)
                throw new Exception("Debe seleccionar al menos una loteria.");

            if (a.Turnos == null || a.Turnos.Count == 0)
                throw new Exception("Debe seleccionar al menos un horario.");

            if (a.Jugadas == null || a.Jugadas.Count == 0)
                throw new Exception("Debe cargar al menos un numero.");

            foreach (Jugada j in a.Jugadas)
            {
                if (string.IsNullOrEmpty(j.Numero) || j.Numero.Length > 4 || !j.Numero.All(char.IsDigit))
                    throw new Exception("El numero '" + j.Numero + "' debe tener entre 1 y 4 cifras.");

                if (!RangosValidos.Contains(j.Rango))
                    throw new Exception("El rango debe ser 1, 5, 10 o 20.");

                if (j.Monto < MontoMinimo || j.Monto > MontoMaximo)
                    throw new Exception("El monto de cada numero debe estar entre $" + MontoMinimo + " y $" + MontoMaximo + ".");
            }

            if (a.Tipo == "Redoblona" && a.Jugadas.Count < 2)
                throw new Exception("La modalidad Redoblona requiere cargar al menos 2 numeros.");

            a.Total = a.Jugadas.Sum(j => j.Monto) * a.Loterias.Count * a.Turnos.Count;

            return dal.Insertar(a);
        }
    }
}
