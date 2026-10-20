using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Cartel que se muestra al pagar un ticket premiado: medalla, papel picado cayendo y el importe
    // ganado, que sube animado hasta el total. Debajo, las jugadas que ganaron.
    public partial class FormFelicitaciones : Form
    {
        private const int AltoFranja = 150;
        private const int MaxLineas = 3;
        private const double SegundosConteo = 1.3;
        private const double SegundosLluvia = 3.5;

        // Azar solo para la animacion (los sorteos usan su propia fuente de azar en la BLL).
        private static readonly Random azar = new Random();
        private static readonly Color[] ColoresPapel =
        {
            Estilos.Verde, Estilos.AzulMedio, Estilos.Naranja, Estilos.Violeta, Estilos.Bordo, Color.FromArgb(245, 197, 24)
        };

        private readonly Apuesta apuesta;
        private readonly decimal total;
        private readonly List<Premio> ganadores;
        private readonly List<Papel> papeles = new List<Papel>();
        private readonly int yDetalle;
        private readonly int altoDetalle;
        private DateTime inicio;

        private class Papel
        {
            public float X, Y, Vx, Vy, Angulo, Giro, Ancho, Alto;
            public Color Color;
        }

        public FormFelicitaciones(Apuesta apuesta, decimal total, List<Premio> premios)
        {
            InitializeComponent();
            this.apuesta = apuesta;
            this.total = total;
            ganadores = premios.Where(p => p.Importe > 0).ToList();
            Text = "Ticket " + ApuestaBLL.CodigoCorto(apuesta) + " pagado";

            // El alto del cartel depende de cuantas jugadas ganadoras se listan.
            int lineas = Math.Min(MaxLineas, ganadores.Count) + (ganadores.Count > MaxLineas ? 1 : 0);
            yDetalle = AltoFranja + 214;
            altoDetalle = 18 + lineas * 26;
            ClientSize = new Size(540, yDetalle + altoDetalle + 46 + 48 + 22);
            btnAceptar.SetBounds(40, ClientSize.Height - 48 - 22, ClientSize.Width - 80, 48);

            for (int i = 0; i < 110; i++)
                papeles.Add(NuevoPapel(true));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            inicio = DateTime.Now;
            timer.Start();
        }

        private Papel NuevoPapel(bool alInicio)
        {
            return new Papel
            {
                X = (float)(azar.NextDouble() * ClientSize.Width),
                Y = alInicio ? (float)(-azar.NextDouble() * ClientSize.Height) : -15f,
                Vx = (float)(azar.NextDouble() - 0.5) * 1.2f,
                Vy = 2.2f + (float)azar.NextDouble() * 3f,
                Angulo = (float)(azar.NextDouble() * 360),
                Giro = (float)(azar.NextDouble() - 0.5) * 14f,
                Ancho = 6 + (float)azar.NextDouble() * 6,
                Alto = 3 + (float)azar.NextDouble() * 4,
                Color = ColoresPapel[azar.Next(ColoresPapel.Length)]
            };
        }

        private double Segundos
        {
            get { return inicio == DateTime.MinValue ? 0 : (DateTime.Now - inicio).TotalSeconds; }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            foreach (Papel p in papeles)
            {
                p.X += p.Vx + (float)Math.Sin((p.Y + p.Angulo) / 30.0) * 0.6f;
                p.Y += p.Vy;
                p.Angulo += p.Giro;
            }

            // Durante unos segundos el papel que sale por abajo vuelve a caer desde arriba; despues
            // se deja terminar de caer y se apaga la animacion.
            if (Segundos < SegundosLluvia)
            {
                for (int i = 0; i < papeles.Count; i++)
                    if (papeles[i].Y > ClientSize.Height + 10)
                        papeles[i] = NuevoPapel(false);
            }
            else
            {
                papeles.RemoveAll(p => p.Y > ClientSize.Height + 10);
                if (papeles.Count == 0 && Segundos > SegundosConteo)
                    timer.Stop();
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            int ancho = ClientSize.Width;

            using (StringFormat centro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Franja superior
                using (LinearGradientBrush franja = new LinearGradientBrush(new Rectangle(0, 0, ancho, AltoFranja),
                           Estilos.Aclarar(Estilos.Verde, 0.12f), Estilos.Oscurecer(Estilos.Verde, 0.22f), 90f))
                    g.FillRectangle(franja, 0, 0, ancho, AltoFranja);
                // a los costados, para que las cintas de la medalla no tapen el texto
                using (Font f = Estilos.Negrita(9.5f))
                using (SolidBrush b = new SolidBrush(Estilos.Aclarar(Estilos.Verde, 0.75f)))
                using (StringFormat derecha = new StringFormat { Alignment = StringAlignment.Far })
                {
                    g.DrawString("PREMIO PAGADO", f, b, 22, 16);
                    g.DrawString("TICKET " + ApuestaBLL.CodigoCorto(apuesta), f, b, new RectangleF(0, 16, ancho - 22, 22), derecha);
                }

                DibujarMedalla(g, ancho / 2, AltoFranja - 4, 54);

                // Titulo, ticket e importe (el importe sube de 0 al total)
                using (Font f = Estilos.Negrita(26f))
                using (SolidBrush b = new SolidBrush(Estilos.Texto))
                    g.DrawString("¡Felicitaciones!", f, b, new RectangleF(0, AltoFranja + 64, ancho, 50), centro);

                string juego = apuesta.Juego != null ? apuesta.Juego.Nombre : "";
                using (Font f = Estilos.Fuente(11f))
                using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
                    g.DrawString("El ticket " + ApuestaBLL.CodigoCorto(apuesta) + (juego.Length > 0 ? " de " + juego : "") + " ganó",
                                 f, b, new RectangleF(0, AltoFranja + 112, ancho, 26), centro);

                double t = Math.Min(1.0, Segundos / SegundosConteo);
                double suave = 1 - Math.Pow(1 - t, 3);
                decimal mostrado = t >= 1 ? total : Math.Round(total * (decimal)suave, 2);
                using (Font f = Estilos.Negrita(34f))
                using (SolidBrush b = new SolidBrush(Estilos.Verde))
                    g.DrawString(Estilos.Plata(mostrado), f, b, new RectangleF(0, AltoFranja + 140, ancho, 64), centro);

                // Jugadas que ganaron
                Rectangle caja = new Rectangle(40, yDetalle, ancho - 80, altoDetalle);
                using (GraphicsPath borde = Redondeado(caja, 10))
                using (SolidBrush fondo = new SolidBrush(Color.FromArgb(243, 247, 250)))
                using (Pen linea = new Pen(Estilos.Borde))
                {
                    g.FillPath(fondo, borde);
                    g.DrawPath(linea, borde);
                }
                using (Font f = Estilos.Fuente(9.75f))
                using (Font fImporte = Estilos.Negrita(9.75f))
                using (SolidBrush texto = new SolidBrush(Estilos.Texto))
                using (SolidBrush verde = new SolidBrush(Estilos.Verde))
                using (StringFormat izquierda = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                using (StringFormat derecha = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
                {
                    int y = caja.Top + 9;
                    foreach (Premio p in ganadores.Take(MaxLineas))
                    {
                        RectangleF renglon = new RectangleF(caja.Left + 14, y, caja.Width - 28, 26);
                        g.DrawString(Estilos.SorteoCorto(p.Jugada.Sorteo) + "    " + Estilos.DescribirJugada(p.Jugada),
                                     f, texto, new RectangleF(renglon.X, renglon.Y, renglon.Width - 120, renglon.Height), izquierda);
                        g.DrawString("+ " + Estilos.Plata(p.Importe), fImporte, verde, renglon, derecha);
                        y += 26;
                    }
                    if (ganadores.Count > MaxLineas)
                    {
                        int resto = ganadores.Count - MaxLineas;
                        g.DrawString("y " + resto + (resto == 1 ? " jugada ganadora más" : " jugadas ganadoras más"),
                                     f, texto, new RectangleF(caja.Left + 14, y, caja.Width - 28, 26), izquierda);
                    }
                }

                using (Font f = Estilos.Fuente(9f))
                using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
                    g.DrawString("Entregale el importe al cliente. El ticket quedó como PAGADO.", f, b,
                                 new RectangleF(0, caja.Bottom + 8, ancho, 30), centro);
            }

            // Papel picado, por encima de todo lo dibujado
            foreach (Papel p in papeles)
            {
                GraphicsState estado = g.Save();
                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(p.Angulo);
                using (SolidBrush b = new SolidBrush(p.Color))
                    g.FillRectangle(b, -p.Ancho / 2, -p.Alto / 2, p.Ancho, p.Alto);
                g.Restore(estado);
            }
        }

        // Medalla dorada colgando de dos cintas, con una estrella en el centro.
        private static void DibujarMedalla(Graphics g, int cx, int cy, int radio)
        {
            PointF[] cintaIzq = { new PointF(cx - 46, 0), new PointF(cx - 14, 0), new PointF(cx + 6, cy - radio + 8), new PointF(cx - 22, cy - radio + 14) };
            PointF[] cintaDer = { new PointF(cx + 14, 0), new PointF(cx + 46, 0), new PointF(cx + 22, cy - radio + 14), new PointF(cx - 6, cy - radio + 8) };
            using (SolidBrush b = new SolidBrush(Estilos.AzulMedio)) g.FillPolygon(b, cintaIzq);
            using (SolidBrush b = new SolidBrush(Estilos.Rojo)) g.FillPolygon(b, cintaDer);

            Rectangle exterior = new Rectangle(cx - radio, cy - radio, radio * 2, radio * 2);
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                g.FillEllipse(sombra, exterior.X + 3, exterior.Y + 5, exterior.Width, exterior.Height);
            using (LinearGradientBrush oro = new LinearGradientBrush(exterior, Color.FromArgb(255, 226, 120), Color.FromArgb(214, 158, 0), 60f))
                g.FillEllipse(oro, exterior);
            using (Pen canto = new Pen(Color.FromArgb(176, 128, 0), 3))
                g.DrawEllipse(canto, exterior);

            int r2 = (int)(radio * 0.76);
            Rectangle interior = new Rectangle(cx - r2, cy - r2, r2 * 2, r2 * 2);
            using (LinearGradientBrush oro = new LinearGradientBrush(interior, Color.FromArgb(232, 172, 10), Color.FromArgb(255, 214, 90), 60f))
                g.FillEllipse(oro, interior);

            PointF[] estrella = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angulo = (-90 + i * 36) * Math.PI / 180;
                double r = i % 2 == 0 ? radio * 0.5 : radio * 0.22;
                estrella[i] = new PointF(cx + (float)(Math.Cos(angulo) * r), cy + (float)(Math.Sin(angulo) * r));
            }
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillPolygon(b, estrella);
            using (Pen p = new Pen(Color.FromArgb(200, 140, 0), 1.5f)) g.DrawPolygon(p, estrella);
        }

        private static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            int d = radio * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
