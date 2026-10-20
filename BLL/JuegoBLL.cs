using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class JuegoBLL
    {
        // Codigo fijo de la Quiniela en la tabla JUEGOS: el sistema compara contra el codigo y
        // no contra el nombre, que se puede cambiar.
        public const string CodigoQuiniela = "QUINIELA";

        private JuegoDAL dal = new JuegoDAL();

        public List<Juego> ObtenerActivos()
        {
            return dal.ObtenerActivos();
        }

        public Juego ObtenerQuiniela()
        {
            Juego quiniela = ObtenerActivos().FirstOrDefault(j => j.Codigo == CodigoQuiniela);
            if (quiniela == null)
                throw new Exception("La Quiniela no está cargada en la base de datos.");
            return quiniela;
        }

        // Numero con los ceros a la izquierda que usa el juego: en el Quini 6 (00 al 45) el 5 es "05".
        public static string FormatearNumero(Juego juego, int numero)
        {
            return numero.ToString(new string('0', juego.NumeroMax.ToString().Length));
        }
    }
}
