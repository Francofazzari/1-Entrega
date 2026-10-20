using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class ApuestaBLL
    {
        public const decimal MontoMinimo = 100;
        public const decimal MontoMaximo = 5000;
        public const string Directa = "Directa";
        public const string Redoblona = "Redoblona";
        public const string Vigente = "Vigente";
        public const string Pagada = "Pagada";
        public const string Anulada = "Anulada";
        public static readonly int[] RangosValidos = { 1, 5, 10, 20 };

        private readonly ApuestaDAL dal = new ApuestaDAL();
        private readonly SorteoBLL sorteoBll = new SorteoBLL();
        private readonly BitacoraBLL bitacora = new BitacoraBLL();

        // Arma y guarda el ticket. Las "plantillas" son las jugadas que cargo el vendedor, todavia
        // sin sorteo: en Quiniela se repiten para cada combinacion de loteria y turno elegida (una
        // jugada por sorteo); en los demas juegos van al sorteo del dia. Todas las jugadas de un
        // ticket quedan asi del mismo juego.
        public Apuesta Vender(Juego juego, List<Loteria> loterias, List<Turno> turnos, List<Jugada> plantillas, Usuario vendedor)
        {
            if (plantillas == null || plantillas.Count == 0)
                throw new Exception("Cargá al menos una jugada antes de confirmar la venta.");
            foreach (Jugada p in plantillas)
                ValidarJugada(juego, p);

            List<Sorteo> sorteos = new List<Sorteo>();
            if (juego.UsaLoteriaTurno)
            {
                if (loterias == null || loterias.Count == 0)
                    throw new Exception("Elegí al menos una lotería.");
                if (turnos == null || turnos.Count == 0)
                    throw new Exception("Elegí al menos un horario.");
                foreach (Loteria l in loterias)
                    foreach (Turno t in turnos)
                        sorteos.Add(sorteoBll.ObtenerParaVender(juego, l, t));
            }
            else
            {
                sorteos.Add(sorteoBll.ObtenerParaVender(juego, null, null));
            }

            Apuesta a = new Apuesta
            {
                CodigoTicket = Guid.NewGuid(),
                UsuarioId = vendedor.Id,
                UsuarioNombre = (vendedor.Nombre + " " + vendedor.Apellido).Trim(),
                FechaHora = DateTime.Now,
                Estado = Vigente,
                Juego = juego
            };

            int orden = 1;
            foreach (Sorteo s in sorteos)
            {
                foreach (Jugada p in plantillas)
                {
                    a.Jugadas.Add(new Jugada
                    {
                        Sorteo = s,
                        Tipo = juego.UsaRango ? p.Tipo : null,
                        EsAutomatica = p.EsAutomatica,
                        Monto = juego.Precio ?? p.Monto,
                        Orden = orden++,
                        Numeros = p.Numeros.OrderBy(n => n.Orden).Select(n => new NumeroElegido
                        {
                            Numero = juego.UsaRango ? n.Numero : JuegoBLL.FormatearNumero(juego, int.Parse(n.Numero)),
                            Orden = n.Orden,
                            Rango = juego.UsaRango ? n.Rango : null
                        }).ToList()
                    });
                }
            }

            a.Total = a.Jugadas.Sum(j => j.Monto);
            dal.Insertar(a);

            bitacora.Registrar(vendedor.Id, vendedor.Nombre, "Venta",
                juego.Nombre + " - ticket " + CodigoCorto(a) + " - " + a.Jugadas.Count + " jugada(s) - Total $" + a.Total.ToString("N2"));
            return a;
        }

        // Jugada automatica: la maquina elige los numeros (distintos y dentro del rango del juego).
        // Se devuelven ordenados de menor a mayor.
        public List<int> GenerarNumerosAutomaticos(Juego juego)
        {
            if (juego.UsaRango)
                throw new Exception("En la Quiniela los números los elige el cliente.");
            if (juego.NumeroMax - juego.NumeroMin + 1 < juego.CantidadNumeros)
                throw new Exception("Las reglas de " + juego.Nombre + " no permiten armar una jugada.");

            SortedSet<int> numeros = new SortedSet<int>();
            while (numeros.Count < juego.CantidadNumeros)
                numeros.Add(Azar.Entre(juego.NumeroMin, juego.NumeroMax));
            return numeros.ToList();
        }

        // Busca un ticket por su codigo completo (GUID) o por sus primeros caracteres, que son
        // los que figuran como "codigo corto" en el ticket impreso.
        public Apuesta BuscarPorCodigo(string texto)
        {
            string codigoTexto = (texto ?? "").Trim();
            if (codigoTexto.Length == 0)
                throw new Exception("Ingresá el código del ticket.");

            Guid codigo;
            if (!Guid.TryParse(codigoTexto, out codigo))
            {
                if (codigoTexto.Length < 6 || !codigoTexto.All(c => Uri.IsHexDigit(c) || c == '-'))
                    throw new Exception("El código no es válido. Ingresá el código completo o, al menos, sus primeros 6 caracteres.");

                List<Guid> encontrados = dal.BuscarCodigosPorPrefijo(codigoTexto.ToUpperInvariant());
                if (encontrados.Count == 0)
                    throw new Exception("No existe ningún ticket con ese código.");
                if (encontrados.Count > 1)
                    throw new Exception("Hay más de un ticket que empieza así. Ingresá el código completo.");
                codigo = encontrados[0];
            }

            Apuesta a = dal.ObtenerPorCodigo(codigo);
            if (a == null)
                throw new Exception("No existe ningún ticket con ese código.");
            return a;
        }

        // Por que no se puede cancelar el ticket, o null si se puede: tiene que estar vigente y
        // ninguno de sus sorteos puede haber cerrado.
        public string MotivoNoAnulable(Apuesta a)
        {
            if (a.Estado == Anulada) return "Este ticket ya fue cancelado.";
            if (a.Estado == Pagada) return "Este ticket ya fue pagado: no se puede cancelar.";

            Jugada cerrada = a.Jugadas.FirstOrDefault(j => j.Sorteo.Estado != SorteoBLL.Abierto);
            if (cerrada != null)
                return "El sorteo " + SorteoBLL.Descripcion(cerrada.Sorteo) + " ya cerró: el ticket no se puede cancelar.";
            return null;
        }

        public void Anular(Apuesta a, Usuario usuario)
        {
            string motivo = MotivoNoAnulable(a);
            if (motivo != null)
                throw new Exception(motivo);
            if (!dal.CambiarEstado(a.Id, Anulada, Vigente))
                throw new Exception("El ticket cambió de estado mientras lo consultabas. Buscalo de nuevo.");

            a.Estado = Anulada;
            bitacora.Registrar(usuario.Id, usuario.Nombre, "Cancelacion",
                NombreJuego(a) + " - ticket " + CodigoCorto(a) + " - $" + a.Total.ToString("N2"));
        }

        public void RegistrarReimpresion(Apuesta a, Usuario usuario)
        {
            bitacora.Registrar(usuario.Id, usuario.Nombre, "Reimpresion",
                NombreJuego(a) + " - ticket " + CodigoCorto(a));
        }

        public List<Apuesta> ListarVentas(DateTime desde, DateTime hasta, Juego juego)
        {
            if (desde.Date > hasta.Date)
                throw new Exception("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
            return dal.ListarVentas(desde.Date, hasta.Date.AddDays(1), juego != null ? juego.Id : (int?)null);
        }

        // Los primeros 8 caracteres del GUID, para mostrar y para buscar el ticket a mano.
        public static string CodigoCorto(Apuesta a)
        {
            return a.CodigoTicket.ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        public static string CodigoCompleto(Apuesta a)
        {
            return a.CodigoTicket.ToString("D").ToUpperInvariant();
        }

        // 1 = "a la primera"; 5, 10 o 20 = "a los 5", "a los 10", "a los 20".
        public static string DescribirRango(int? rango)
        {
            return rango == 1 ? "a la primera" : "a los " + rango;
        }

        private static string NombreJuego(Apuesta a)
        {
            return a.Juego != null ? a.Juego.Nombre : "Ticket";
        }

        private static void ValidarJugada(Juego juego, Jugada j)
        {
            if (j.Numeros == null || j.Numeros.Count == 0)
                throw new Exception("Hay una jugada sin números.");

            if (juego.UsaRango)
            {
                if (j.Monto < MontoMinimo || j.Monto > MontoMaximo)
                    throw new Exception("El monto de cada jugada debe estar entre $" + MontoMinimo.ToString("N0") +
                        " y $" + MontoMaximo.ToString("N0") + ".");

                if (j.Tipo == Directa)
                {
                    if (j.Numeros.Count != 1)
                        throw new Exception("La jugada directa lleva un solo número.");
                    ValidarNumeroQuiniela(j.Numeros[0], false);
                }
                else if (j.Tipo == Redoblona)
                {
                    if (j.Numeros.Count != 2)
                        throw new Exception("La redoblona lleva exactamente dos números.");
                    foreach (NumeroElegido n in j.Numeros)
                        ValidarNumeroQuiniela(n, true);
                }
                else
                {
                    throw new Exception("El tipo de jugada debe ser Directa o Redoblona.");
                }
                return;
            }

            if (juego.Precio == null)
                throw new Exception(juego.Nombre + " no tiene precio cargado.");
            if (j.Numeros.Count != juego.CantidadNumeros)
                throw new Exception("En " + juego.Nombre + " se eligen " + juego.CantidadNumeros + " números por jugada.");

            HashSet<int> vistos = new HashSet<int>();
            foreach (NumeroElegido n in j.Numeros)
            {
                int valor;
                if (!int.TryParse(n.Numero, out valor) || valor < juego.NumeroMin || valor > juego.NumeroMax)
                    throw new Exception("El número " + n.Numero + " no es válido en " + juego.Nombre + " (van del " +
                        JuegoBLL.FormatearNumero(juego, juego.NumeroMin) + " al " + JuegoBLL.FormatearNumero(juego, juego.NumeroMax) + ").");
                if (!vistos.Add(valor))
                    throw new Exception("El número " + n.Numero + " está repetido en la jugada.");
            }
        }

        private static void ValidarNumeroQuiniela(NumeroElegido n, bool esRedoblona)
        {
            string s = n.Numero ?? "";
            if (s.Length < 1 || s.Length > 4 || !s.All(char.IsDigit))
                throw new Exception("El número '" + s + "' debe tener entre 1 y 4 cifras.");
            if (esRedoblona && s.Length != 2)
                throw new Exception("En la redoblona cada número tiene que ser de 2 cifras.");
            if (n.Rango == null || !RangosValidos.Contains(n.Rango.Value))
                throw new Exception("La posición debe ser a la primera, a los 5, a los 10 o a los 20.");
        }
    }
}
