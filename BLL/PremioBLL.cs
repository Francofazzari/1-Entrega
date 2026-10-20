using System;
using System.Collections.Generic;
using System.Linq;
using BE;

namespace BLL
{
    // Calcula lo que le corresponde a cada jugada de un ticket comparandola con el resultado
    // de su sorteo.
    public class PremioBLL
    {
        // Quiniela: cuantas veces se multiplica lo apostado segun las cifras del numero, si sale
        // "a la primera". Si se juega a los 5, 10 o 20, el premio se divide por esa posicion.
        private static readonly Dictionary<int, decimal> PagaQuiniela = new Dictionary<int, decimal>
        {
            { 1, 7 }, { 2, 70 }, { 3, 600 }, { 4, 3500 }
        };

        // Demas juegos: cuantas veces se paga el precio de la jugada segun la cantidad de aciertos.
        // Son tablas de demostracion: los juegos reales reparten un pozo entre los ganadores.
        private static readonly Dictionary<string, Dictionary<int, decimal>> PagaPorAciertos =
            new Dictionary<string, Dictionary<int, decimal>>
            {
                { "QUINI6",   new Dictionary<int, decimal> { { 6, 5000 }, { 5, 100 }, { 4, 5 } } },
                { "LOTO5",    new Dictionary<int, decimal> { { 5, 2000 }, { 4, 50 }, { 3, 3 } } },
                { "TELEKINO", new Dictionary<int, decimal> { { 15, 10000 }, { 14, 200 }, { 13, 20 }, { 12, 4 }, { 11, 1 } } },
                { "QPLUS",    new Dictionary<int, decimal> { { 8, 5000 }, { 7, 300 }, { 6, 30 }, { 5, 3 } } }
            };

        private readonly SorteoBLL sorteoBll = new SorteoBLL();

        public List<Premio> Calcular(Apuesta a)
        {
            List<Premio> premios = new List<Premio>();
            Dictionary<int, List<NumeroGanador>> resultados = new Dictionary<int, List<NumeroGanador>>();

            foreach (Jugada j in a.Jugadas)
            {
                Premio p = new Premio { Jugada = j };
                if (j.Sorteo.Estado != SorteoBLL.Sorteado)
                {
                    p.Detalle = "Todavía no se sorteó";
                    premios.Add(p);
                    continue;
                }

                List<NumeroGanador> ganadores;
                if (!resultados.TryGetValue(j.Sorteo.Id, out ganadores))
                {
                    ganadores = sorteoBll.ObtenerNumerosGanadores(j.Sorteo);
                    resultados[j.Sorteo.Id] = ganadores;
                }

                p.SorteoRealizado = true;
                if (j.Sorteo.Juego.UsaRango)
                    CalcularQuiniela(j, ganadores, p);
                else
                    CalcularPorAciertos(j, ganadores, p);
                p.Importe = Math.Round(p.Importe, 2);
                premios.Add(p);
            }
            return premios;
        }

        private static void CalcularQuiniela(Jugada j, List<NumeroGanador> ganadores, Premio p)
        {
            List<NumeroElegido> numeros = j.Numeros.OrderBy(n => n.Orden).ToList();

            if (j.Tipo == ApuestaBLL.Redoblona)
            {
                // Redoblona: lo que gana el primer numero se vuelve a apostar al segundo.
                NumeroElegido primero = numeros[0], segundo = numeros[1];
                NumeroGanador g1 = Buscar(primero, ganadores);
                NumeroGanador g2 = Buscar(segundo, ganadores);
                if (g1 != null && g2 != null)
                {
                    decimal premioPrimero = j.Monto * PagaQuiniela[2] / primero.Rango.Value;
                    p.Importe = premioPrimero * PagaQuiniela[2] / segundo.Rango.Value;
                    p.Detalle = "Salieron el " + primero.Numero + " (" + g1.Posicion + "°) y el " + segundo.Numero + " (" + g2.Posicion + "°)";
                }
                else if (g1 == null)
                    p.Detalle = "El " + primero.Numero + " no salió " + ApuestaBLL.DescribirRango(primero.Rango);
                else
                    p.Detalle = "Salió el " + primero.Numero + ", pero el " + segundo.Numero + " no salió " + ApuestaBLL.DescribirRango(segundo.Rango);
                return;
            }

            NumeroElegido n1 = numeros[0];
            NumeroGanador g = Buscar(n1, ganadores);
            if (g != null)
            {
                p.Importe = j.Monto * PagaQuiniela[n1.Numero.Length] / n1.Rango.Value;
                p.Detalle = "Salió en el puesto " + g.Posicion + " (" + g.Numero + ")";
            }
            else
            {
                p.Detalle = "El " + n1.Numero + " no salió " + ApuestaBLL.DescribirRango(n1.Rango);
            }
        }

        // Un numero de Quiniela "sale" si alguno de los ganadores dentro de su posicion termina
        // con sus cifras: el 32 sale si salio el 4532 entre los primeros puestos jugados.
        private static NumeroGanador Buscar(NumeroElegido n, List<NumeroGanador> ganadores)
        {
            return ganadores
                .Where(g => g.Posicion <= n.Rango && g.Numero.EndsWith(n.Numero))
                .OrderBy(g => g.Posicion)
                .FirstOrDefault();
        }

        private static void CalcularPorAciertos(Jugada j, List<NumeroGanador> ganadores, Premio p)
        {
            HashSet<string> salieron = new HashSet<string>(ganadores.Select(g => g.Numero));
            int aciertos = j.Numeros.Count(n => salieron.Contains(n.Numero));

            decimal veces = 0;
            Dictionary<int, decimal> tabla;
            if (PagaPorAciertos.TryGetValue(j.Sorteo.Juego.Codigo, out tabla))
                tabla.TryGetValue(aciertos, out veces);

            p.Importe = j.Monto * veces;
            p.Detalle = aciertos == 1 ? "1 acierto" : aciertos + " aciertos";
        }
    }
}
