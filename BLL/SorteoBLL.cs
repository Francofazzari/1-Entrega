using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class SorteoBLL
    {
        public const string Abierto = "Abierto";
        public const string Cerrado = "Cerrado";
        public const string Sorteado = "Sorteado";

        private readonly SorteoDAL dal = new SorteoDAL();
        private readonly BitacoraBLL bitacora = new BitacoraBLL();

        // Devuelve el sorteo del juego en esa fecha (y de esa loteria y turno, si el juego los
        // usa). Si todavia no existe lo crea abierto: los sorteos se generan a medida que se usan.
        public Sorteo ObtenerOCrear(Juego juego, Loteria loteria, Turno turno, DateTime fecha)
        {
            if (juego.UsaLoteriaTurno && (loteria == null || turno == null))
                throw new Exception(juego.Nombre + " se sortea por lotería y turno: elegí ambos.");
            if (!juego.UsaLoteriaTurno)
            {
                loteria = null;
                turno = null;
            }

            Sorteo existente = Buscar(juego, loteria, turno, fecha);
            if (existente != null)
                return existente;

            Sorteo nuevo = new Sorteo { Juego = juego, Loteria = loteria, Turno = turno, Fecha = fecha.Date, Estado = Abierto };
            nuevo.Id = dal.Insertar(nuevo);
            return nuevo;
        }

        public Sorteo Buscar(Juego juego, Loteria loteria, Turno turno, DateTime fecha)
        {
            return dal.Buscar(juego.Id,
                juego.UsaLoteriaTurno && loteria != null ? loteria.Id : (int?)null,
                juego.UsaLoteriaTurno && turno != null ? turno.Id : (int?)null,
                fecha.Date);
        }

        // Sorteo del dia sobre el que se va a vender: tiene que seguir abierto.
        public Sorteo ObtenerParaVender(Juego juego, Loteria loteria, Turno turno)
        {
            Sorteo s = ObtenerOCrear(juego, loteria, turno, DateTime.Today);
            if (s.Estado != Abierto)
                throw new Exception("El sorteo " + Descripcion(s) + " ya no acepta apuestas (está " + s.Estado.ToLower() + ").");
            return s;
        }

        public void CerrarVentas(Sorteo s, Usuario usuario)
        {
            if (s.Estado != Abierto)
                throw new Exception("Solo se pueden cerrar las ventas de un sorteo abierto.");
            dal.CambiarEstado(s.Id, Cerrado);
            s.Estado = Cerrado;
            bitacora.Registrar(usuario.Id, usuario.Nombre, "Cierre de sorteo", Descripcion(s));
        }

        // Carga el resultado del sorteo (el que informa la loteria). "cargados" trae un texto por
        // posicion; las que vienen vacias se completan al azar. Guarda los numeros y deja el sorteo
        // como sorteado: desde ese momento ya no se vende y se pueden pagar los premios.
        public List<NumeroGanador> CargarResultado(Sorteo s, IList<string> cargados, Usuario usuario)
        {
            if (s.Estado == Sorteado)
                throw new Exception("Ese sorteo ya se realizó.");
            if (s.Fecha.Date > DateTime.Today)
                throw new Exception("No se puede realizar un sorteo de una fecha futura.");

            List<NumeroGanador> numeros = CompletarResultado(s.Juego, cargados);
            if (!dal.GuardarResultado(s.Id, numeros, Sorteado))
                throw new Exception("Ese sorteo ya se realizó.");

            s.Estado = Sorteado;
            int aMano = cargados.Count(c => !string.IsNullOrWhiteSpace(c));
            bitacora.Registrar(usuario.Id, usuario.Nombre, "Sorteo realizado", Descripcion(s) +
                (aMano > 0 ? " (" + aMano + " número(s) cargado(s) a mano)" : " (al azar)"));
            return numeros;
        }

        // Sorteo simulado: todas las posiciones salen al azar.
        public List<NumeroGanador> RealizarSorteo(Sorteo s, Usuario usuario)
        {
            return CargarResultado(s, new string[s.Juego.CantidadSorteados], usuario);
        }

        // Arma el resultado completo sin guardarlo: valida los numeros cargados (rango del juego y
        // sin repetir) y completa al azar las posiciones vacias. En la Quiniela salen 20 numeros de
        // 4 cifras, del 1er al 20mo puesto: un 32 cargado a mano queda como 0032.
        public List<NumeroGanador> CompletarResultado(Juego juego, IList<string> cargados)
        {
            if (cargados == null || cargados.Count != juego.CantidadSorteados)
                throw new Exception("En " + juego.Nombre + " salen " + juego.CantidadSorteados + " números por sorteo.");

            int?[] valores = new int?[cargados.Count];
            HashSet<int> usados = new HashSet<int>();
            for (int i = 0; i < cargados.Count; i++)
            {
                string texto = (cargados[i] ?? "").Trim();
                if (texto.Length == 0) continue;

                int valor;
                if (!texto.All(char.IsDigit) || !int.TryParse(texto, out valor) || valor < juego.NumeroMin || valor > juego.NumeroMax)
                    throw new Exception("El número del puesto " + (i + 1) + " (" + texto + ") no es válido: van del " +
                        JuegoBLL.FormatearNumero(juego, juego.NumeroMin) + " al " + JuegoBLL.FormatearNumero(juego, juego.NumeroMax) + ".");
                if (!usados.Add(valor))
                    throw new Exception("El número " + JuegoBLL.FormatearNumero(juego, valor) + " está repetido.");
                valores[i] = valor;
            }

            // En la Quiniela, los numeros al azar no terminan en las mismas dos cifras que los cargados a
            // mano: asi un numero cargado (por ejemplo, el 32 en el puesto 8) es el unico que lo hace
            // ganar, y el premio se explica con el puesto que se cargo.
            HashSet<int> terminaciones = juego.UsaRango
                ? new HashSet<int>(valores.Where(v => v != null).Select(v => v.Value % 100))
                : new HashSet<int>();

            List<NumeroGanador> resultado = new List<NumeroGanador>();
            for (int i = 0; i < valores.Length; i++)
            {
                if (valores[i] == null)
                {
                    int n;
                    do { n = Azar.Entre(juego.NumeroMin, juego.NumeroMax); }
                    while (terminaciones.Contains(n % 100) || !usados.Add(n));
                    valores[i] = n;
                }
                resultado.Add(new NumeroGanador { Numero = JuegoBLL.FormatearNumero(juego, valores[i].Value), Posicion = i + 1 });
            }
            return resultado;
        }

        public List<NumeroGanador> ObtenerNumerosGanadores(Sorteo s)
        {
            return dal.ObtenerNumerosGanadores(s.Id);
        }

        // Sin el juego ni la fecha, para listas: "Provincia · Nocturna". Si el juego no usa loteria
        // ni turno, devuelve la descripcion completa.
        public static string DescripcionCorta(Sorteo s)
        {
            List<string> partes = new List<string>();
            if (s.Loteria != null) partes.Add(s.Loteria.Nombre);
            if (s.Turno != null) partes.Add(s.Turno.Nombre);
            return partes.Count > 0 ? string.Join(" · ", partes) : Descripcion(s);
        }

        // Ejemplo: "Quiniela · Provincia · Nocturna · 19/10/2026" o "Quini 6 · 19/10/2026".
        public static string Descripcion(Sorteo s)
        {
            List<string> partes = new List<string> { s.Juego.Nombre };
            if (s.Loteria != null) partes.Add(s.Loteria.Nombre);
            if (s.Turno != null) partes.Add(s.Turno.Nombre);
            partes.Add(s.Fecha.ToString("dd/MM/yyyy"));
            return string.Join(" · ", partes);
        }
    }
}
