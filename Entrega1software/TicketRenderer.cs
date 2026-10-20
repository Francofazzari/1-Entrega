using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using BE;
using BLL;

namespace Entrega1software
{
    // Dibuja el ticket como el comprobante angosto que sale de la terminal. Se usa igual en
    // pantalla y al imprimir, para que lo que se ve sea exactamente lo que se imprime.
    internal static class TicketRenderer
    {
        public const int Ancho = 300;   // unos 80 mm, como el papel de las terminales

        private static readonly Color Tinta = Color.FromArgb(25, 25, 25);
        private static readonly Color TintaSuave = Color.FromArgb(95, 95, 95);

        // Dibuja el ticket desde (x, y) y devuelve el alto que ocupo.
        public static int Dibujar(Graphics g, Apuesta a, bool reimpresion, int x, int y)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using (Font fChica = new Font("Segoe UI", 7.5f))
            using (Font fNormal = new Font("Segoe UI", 8.5f))
            using (Font fNegrita = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (Font fTitulo = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (Font fJuego = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (Font fTotal = new Font("Segoe UI", 12.5f, FontStyle.Bold))
            using (Font fNumeros = new Font("Consolas", 10f, FontStyle.Bold))
            using (Font fCodigo = new Font("Consolas", 7.5f))
            using (Font fCodigoCorto = new Font("Consolas", 10f, FontStyle.Bold))
            using (SolidBrush tinta = new SolidBrush(Tinta))
            using (SolidBrush suave = new SolidBrush(TintaSuave))
            {
                int cy = y;

                cy += Centrado(g, "AGENCIA DE QUINIELA", fTitulo, tinta, x, cy);
                cy += Centrado(g, "Comprobante de apuesta", fChica, suave, x, cy) + 6;
                cy += Punteada(g, x, cy);

                // Nombre del juego en negativo, como en los tickets reales
                string juego = (a.Juego != null ? a.Juego.Nombre : "Ticket").ToUpper();
                int altoJuego = (int)g.MeasureString(juego, fJuego).Height + 6;
                g.FillRectangle(tinta, x, cy, Ancho, altoJuego);
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(juego, fJuego, Brushes.White, new RectangleF(x, cy, Ancho, altoJuego), sf);
                cy += altoJuego + 6;

                if (reimpresion)
                    cy += Centrado(g, "***  REIMPRESIÓN  ***", fNegrita, tinta, x, cy) + 2;

                cy += Fila(g, "Fecha", a.FechaHora.ToString("dd/MM/yyyy  HH:mm"), fNormal, fNormal, tinta, x, cy);
                cy += Fila(g, "Vendedor", a.UsuarioNombre, fNormal, fNormal, tinta, x, cy);
                cy += Fila(g, "Ticket", ApuestaBLL.CodigoCorto(a), fNormal, fNegrita, tinta, x, cy) + 2;
                cy += Punteada(g, x, cy);

                int inicioJugadas = cy;
                foreach (IGrouping<int, Jugada> grupo in a.Jugadas.GroupBy(j => j.Sorteo.Id))
                {
                    Sorteo s = grupo.First().Sorteo;
                    cy += Texto(g, TituloSorteo(s), fNegrita, tinta, x, cy, Ancho) + 2;

                    foreach (Jugada j in grupo)
                        cy += DibujarJugada(g, j, fNormal, fNumeros, fChica, tinta, suave, x, cy);
                    cy += 6;
                }
                int finJugadas = cy;

                cy += Punteada(g, x, cy);
                cy += Fila(g, "TOTAL", Estilos.Plata(a.Total), fTotal, fTotal, tinta, x, cy) + 2;
                cy += Punteada(g, x, cy) + 4;

                cy += CodigoDeBarras(g, a, tinta, x, cy) + 4;
                cy += Centrado(g, ApuestaBLL.CodigoCompleto(a), fCodigo, tinta, x, cy);
                cy += Centrado(g, "Código corto: " + ApuestaBLL.CodigoCorto(a), fCodigoCorto, tinta, x, cy) + 6;
                cy += Centrado(g, "Ticket al portador. Consérvelo para cobrar.", fChica, suave, x, cy);

                if (a.Estado != ApuestaBLL.Vigente)
                    Sello(g, a.Estado == ApuestaBLL.Pagada ? "PAGADO" : "CANCELADO",
                        Estilos.ColorEstadoTicket(a.Estado), x, inicioJugadas, finJugadas);

                return cy - y + 4;
            }
        }

        private static string TituloSorteo(Sorteo s)
        {
            if (s.Loteria != null || s.Turno != null)
            {
                List<string> partes = new List<string>();
                if (s.Loteria != null) partes.Add(s.Loteria.Nombre);
                if (s.Turno != null) partes.Add(s.Turno.Nombre);
                partes.Add(s.Fecha.ToString("dd/MM/yyyy"));
                return string.Join(" · ", partes);
            }
            return "Sorteo del " + s.Fecha.ToString("dd/MM/yyyy");
        }

        private static int DibujarJugada(Graphics g, Jugada j, Font fNormal, Font fNumeros, Font fChica,
                                         Brush tinta, Brush suave, int x, int y)
        {
            List<NumeroElegido> numeros = j.Numeros.OrderBy(n => n.Orden).ToList();
            int cy = y;

            if (j.Sorteo.Juego.UsaRango)
            {
                if (j.Tipo == ApuestaBLL.Redoblona)
                {
                    cy += Fila(g, "  Redoblona", Estilos.Plata(j.Monto), fNormal, fNormal, tinta, x, cy);
                    string detalle = "    " + numeros[0].Numero + " " + ApuestaBLL.DescribirRango(numeros[0].Rango) +
                                     "  +  " + numeros[1].Numero + " " + ApuestaBLL.DescribirRango(numeros[1].Rango);
                    cy += Texto(g, detalle, fNormal, tinta, x, cy, Ancho);
                }
                else
                {
                    NumeroElegido n = numeros[0];
                    int alto = (int)g.MeasureString(n.Numero, fNumeros).Height;
                    g.DrawString("  " + n.Numero, fNumeros, tinta, x, cy);
                    g.DrawString(ApuestaBLL.DescribirRango(n.Rango), fNormal, tinta, x + 90, cy + 2);
                    using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Far })
                        g.DrawString(Estilos.Plata(j.Monto), fNormal, tinta, new RectangleF(x, cy + 2, Ancho, alto), sf);
                    cy += alto;
                }
                return cy - y;
            }

            string titulo = "  Jugada " + (j.EsAutomatica ? "automática" : "manual");
            cy += Fila(g, titulo, Estilos.Plata(j.Monto), fNormal, fNormal, tinta, x, cy);
            string lista = "    " + string.Join("  ", numeros.Select(n => n.Numero));
            cy += Texto(g, lista, fNumeros, tinta, x, cy, Ancho) + 2;
            return cy - y;
        }

        private static int Centrado(Graphics g, string texto, Font f, Brush b, int x, int y)
        {
            SizeF tam = g.MeasureString(texto, f, Ancho);
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString(texto, f, b, new RectangleF(x, y, Ancho, tam.Height), sf);
            return (int)tam.Height;
        }

        private static int Texto(Graphics g, string texto, Font f, Brush b, int x, int y, int ancho)
        {
            SizeF tam = g.MeasureString(texto, f, ancho);
            g.DrawString(texto, f, b, new RectangleF(x, y, ancho, tam.Height));
            return (int)tam.Height;
        }

        // Texto a la izquierda y valor a la derecha, en el mismo renglon.
        private static int Fila(Graphics g, string izquierda, string derecha, Font fIzq, Font fDer, Brush b, int x, int y)
        {
            float alto = System.Math.Max(g.MeasureString(izquierda, fIzq).Height, g.MeasureString(derecha, fDer).Height);
            g.DrawString(izquierda, fIzq, b, x, y);
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Far })
                g.DrawString(derecha, fDer, b, new RectangleF(x, y, Ancho, alto), sf);
            return (int)alto;
        }

        private static int Punteada(Graphics g, int x, int y)
        {
            using (Pen p = new Pen(TintaSuave, 1) { DashPattern = new float[] { 3, 3 } })
                g.DrawLine(p, x, y + 4, x + Ancho, y + 4);
            return 10;
        }

        // Codigo de barras armado con los bits del GUID: cada ticket tiene uno distinto.
        private static int CodigoDeBarras(Graphics g, Apuesta a, Brush tinta, int x, int y)
        {
            const int alto = 42, anchoBit = 2;
            byte[] bytes = a.CodigoTicket.ToByteArray();
            int anchoTotal = (bytes.Length * 8 + 6) * anchoBit;
            int cx = x + (Ancho - anchoTotal) / 2;

            SmoothingMode modo = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;
            g.FillRectangle(tinta, cx, y, anchoBit, alto);            // guardas de inicio
            g.FillRectangle(tinta, cx + 2 * anchoBit, y, anchoBit, alto);
            cx += 3 * anchoBit;
            foreach (byte b in bytes)
            {
                for (int bit = 7; bit >= 0; bit--)
                {
                    if ((b & (1 << bit)) != 0)
                        g.FillRectangle(tinta, cx, y, anchoBit, alto);
                    cx += anchoBit;
                }
            }
            g.FillRectangle(tinta, cx, y, anchoBit, alto);            // guardas de fin
            g.FillRectangle(tinta, cx + 2 * anchoBit, y, anchoBit, alto);
            g.SmoothingMode = modo;
            return alto;
        }

        // Sello cruzado sobre las jugadas cuando el ticket ya fue pagado o cancelado.
        private static void Sello(Graphics g, string texto, Color color, int x, int desde, int hasta)
        {
            GraphicsState estado = g.Save();
            float cx = x + Ancho / 2f;
            float cy = desde + (hasta - desde) / 2f;
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-18);
            using (Font f = new Font("Segoe UI", 22f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(150, color)))
            using (Pen p = new Pen(Color.FromArgb(150, color), 3))
            {
                SizeF tam = g.MeasureString(texto, f);
                RectangleF marco = new RectangleF(-tam.Width / 2 - 10, -tam.Height / 2 - 4, tam.Width + 20, tam.Height + 8);
                g.DrawRectangle(p, marco.X, marco.Y, marco.Width, marco.Height);
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(texto, f, b, marco, sf);
            }
            g.Restore(estado);
        }
    }
}
