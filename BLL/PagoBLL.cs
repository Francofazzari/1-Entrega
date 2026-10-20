using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class PagoBLL
    {
        private readonly PagoDAL dal = new PagoDAL();
        private readonly PremioBLL premioBll = new PremioBLL();
        private readonly BitacoraBLL bitacora = new BitacoraBLL();

        // Por que no se puede pagar el ticket, o null si se puede: tiene que estar vigente, todos
        // sus sorteos tienen que haberse realizado y tiene que tener premio.
        public string MotivoNoPagable(Apuesta a, List<Premio> premios)
        {
            if (a.Estado == ApuestaBLL.Anulada) return "El ticket está cancelado: no se puede pagar.";
            if (a.Estado == ApuestaBLL.Pagada) return "Este ticket ya fue pagado.";

            // Se nombran todos los sorteos que faltan (no solo el primero), hasta cuatro.
            List<Sorteo> pendientes = premios.Where(p => !p.SorteoRealizado).Select(p => p.Jugada.Sorteo)
                                             .GroupBy(s => s.Id).Select(g => g.First()).ToList();
            if (pendientes.Count == 1)
                return "Todavía no se realizó el sorteo " + SorteoBLL.Descripcion(pendientes[0]) + ".";
            if (pendientes.Count > 1)
                return "Todavía faltan " + pendientes.Count + " sorteos de este ticket: " +
                       string.Join(", ", pendientes.Take(4).Select(SorteoBLL.DescripcionCorta)) +
                       (pendientes.Count > 4 ? " y " + (pendientes.Count - 4) + " más" : "") + ".";
            if (premios.Sum(p => p.Importe) <= 0)
                return "El ticket no tiene premio.";
            return null;
        }

        // Paga el ticket: registra el pago por el total de sus premios y lo deja como pagado.
        public decimal Pagar(Apuesta a, Usuario cajero)
        {
            List<Premio> premios = premioBll.Calcular(a);
            string motivo = MotivoNoPagable(a, premios);
            if (motivo != null)
                throw new Exception(motivo);

            decimal total = premios.Sum(p => p.Importe);
            Pago pago = new Pago { IdApuesta = a.Id, UsuarioId = cajero.Id, FechaHora = DateTime.Now, Monto = total };
            if (!dal.Registrar(pago, ApuestaBLL.Pagada, ApuestaBLL.Vigente))
                throw new Exception("El ticket ya no está vigente. Buscalo de nuevo.");

            a.Estado = ApuestaBLL.Pagada;
            bitacora.Registrar(cajero.Id, cajero.Nombre, "Pago",
                (a.Juego != null ? a.Juego.Nombre : "Ticket") + " - ticket " + ApuestaBLL.CodigoCorto(a) + " - $" + total.ToString("N2"));
            return total;
        }

        public List<Pago> Listar(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
                throw new Exception("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
            return dal.Listar(desde.Date, hasta.Date.AddDays(1));
        }
    }
}
