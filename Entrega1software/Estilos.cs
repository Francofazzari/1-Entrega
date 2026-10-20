using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Paleta, fuentes y controles comunes del modulo comercial (venta, cobro y reportes), para
    // que todas sus pantallas se vean iguales.
    internal static class Estilos
    {
        public static readonly Color Azul = Color.FromArgb(31, 78, 121);
        public static readonly Color AzulOscuro = Color.FromArgb(19, 47, 74);
        public static readonly Color AzulMedio = Color.FromArgb(24, 95, 165);
        public static readonly Color Verde = Color.FromArgb(29, 158, 117);
        public static readonly Color Rojo = Color.FromArgb(185, 48, 48);
        public static readonly Color Naranja = Color.FromArgb(226, 106, 0);
        public static readonly Color Violeta = Color.FromArgb(106, 27, 154);
        public static readonly Color Bordo = Color.FromArgb(173, 20, 87);
        public static readonly Color Turquesa = Color.FromArgb(0, 128, 140);
        public static readonly Color Ambar = Color.FromArgb(196, 124, 0);

        public static readonly Color Fondo = Color.FromArgb(241, 244, 248);
        public static readonly Color Superficie = Color.White;
        public static readonly Color Borde = Color.FromArgb(214, 221, 230);
        public static readonly Color ToggleApagado = Color.FromArgb(234, 239, 245);
        public static readonly Color Texto = Color.FromArgb(31, 41, 55);
        public static readonly Color TextoSuave = Color.FromArgb(102, 114, 130);

        public const string Familia = "Segoe UI";

        public static Font Fuente(float tam) { return new Font(Familia, tam); }
        public static Font Negrita(float tam) { return new Font(Familia, tam, FontStyle.Bold); }
        public static Font Mono(float tam, bool negrita = false)
        {
            return new Font("Consolas", tam, negrita ? FontStyle.Bold : FontStyle.Regular);
        }

        // Color de cada juego (botones del menu, encabezados y reportes).
        public static Color ColorJuego(string codigo)
        {
            switch (codigo)
            {
                case "QUINIELA": return AzulMedio;
                case "QPLUS": return Violeta;
                case "QUINI6": return Verde;
                case "LOTO5": return Naranja;
                case "TELEKINO": return Bordo;
                default: return Azul;
            }
        }

        public static Color ColorEstadoTicket(string estado)
        {
            switch (estado)
            {
                case "Pagada": return Verde;
                case "Anulada": return Rojo;
                default: return AzulMedio;
            }
        }

        public static Color ColorEstadoSorteo(string estado)
        {
            switch (estado)
            {
                case "Sorteado": return AzulMedio;
                case "Cerrado": return Ambar;
                default: return Verde;
            }
        }

        // Texto del estado del ticket tal como lo ve el usuario.
        public static string TextoEstadoTicket(string estado)
        {
            switch (estado)
            {
                case "Pagada": return "Pagado";
                case "Anulada": return "Cancelado";
                default: return "Vigente";
            }
        }

        public static Color Oscurecer(Color c, float cuanto)
        {
            return Color.FromArgb(c.A, (int)(c.R * (1 - cuanto)), (int)(c.G * (1 - cuanto)), (int)(c.B * (1 - cuanto)));
        }

        public static Color Aclarar(Color c, float cuanto)
        {
            return Color.FromArgb(c.A, c.R + (int)((255 - c.R) * cuanto), c.G + (int)((255 - c.G) * cuanto), c.B + (int)((255 - c.B) * cuanto));
        }

        // Abre una ventana hija dentro del area MDI del formulario principal, centrada y achicada
        // si no entra (por ejemplo en pantallas de 1366x768). Sin padre MDI se abre normal.
        public static void AbrirEnMdi(Form hija, Form padre)
        {
            MdiClient area = padre != null ? padre.Controls.OfType<MdiClient>().FirstOrDefault() : null;
            if (area == null)
            {
                hija.Show();
                return;
            }

            hija.MdiParent = padre;
            hija.StartPosition = FormStartPosition.Manual;
            hija.Width = Math.Min(hija.Width, Math.Max(hija.MinimumSize.Width, area.ClientSize.Width - 8));
            hija.Height = Math.Min(hija.Height, Math.Max(hija.MinimumSize.Height, area.ClientSize.Height - 8));
            hija.Location = new Point(Math.Max(0, (area.ClientSize.Width - hija.Width) / 2),
                                      Math.Max(0, (area.ClientSize.Height - hija.Height) / 2));
            hija.Show();
        }

        public static string Plata(decimal importe)
        {
            return "$ " + importe.ToString("N2");
        }

        // "a los 10" -> "A los 10"
        public static string Capitalizar(string texto)
        {
            return string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0]) + texto.Substring(1);
        }

        // Boton de accion: fondo de color, texto blanco en negrita y sin borde.
        public static Button Boton(string texto, Color fondo, float tamFuente = 10.5f)
        {
            Button b = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Negrita(tamFuente),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Oscurecer(fondo, 0.10f);
            b.FlatAppearance.MouseDownBackColor = Oscurecer(fondo, 0.20f);
            return b;
        }

        // Cambia el color de un boton de accion ya creado (incluidos los colores al pasar el mouse).
        public static void ColorearBoton(Button b, Color fondo)
        {
            b.BackColor = fondo;
            b.FlatAppearance.MouseOverBackColor = Oscurecer(fondo, 0.10f);
            b.FlatAppearance.MouseDownBackColor = Oscurecer(fondo, 0.20f);
        }

        // Sorteo sin el nombre del juego: "Provincia · Nocturna" (Quiniela) o "Sorteo del 19/10".
        public static string SorteoCorto(Sorteo s)
        {
            if (s.Loteria == null && s.Turno == null)
                return "Sorteo del " + s.Fecha.ToString("dd/MM");
            string texto = s.Loteria != null ? s.Loteria.Nombre : "";
            if (s.Turno != null) texto += (texto.Length > 0 ? " · " : "") + s.Turno.Nombre;
            return texto;
        }

        // Una jugada en una linea: "232 a los 10", "23 a los 5 + 45 a los 20" o "03 11 18 27 35 41".
        public static string DescribirJugada(Jugada j)
        {
            var numeros = j.Numeros.OrderBy(n => n.Orden).ToList();
            if (j.Sorteo != null && j.Sorteo.Juego.UsaRango)
                return string.Join("  +  ", numeros.Select(n => n.Numero + " " + ApuestaBLL.DescribirRango(n.Rango)));
            return string.Join("  ", numeros.Select(n => n.Numero)) + (j.EsAutomatica ? "  (aut.)" : "");
        }

        // Boton secundario: blanco, con borde y texto del color dado.
        public static Button BotonSecundario(string texto, Color color, float tamFuente = 10f)
        {
            Button b = new Button
            {
                Text = texto,
                BackColor = Superficie,
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Font = Negrita(tamFuente),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderColor = Aclarar(color, 0.35f);
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Aclarar(color, 0.90f);
            b.FlatAppearance.MouseDownBackColor = Aclarar(color, 0.80f);
            return b;
        }

        // Boton que se prende y se apaga (loterias, horarios, posiciones, numeros).
        public static Button Toggle(string texto, float tamFuente = 10f)
        {
            Button b = new Button
            {
                Text = texto,
                FlatStyle = FlatStyle.Flat,
                Font = Negrita(tamFuente),
                Cursor = Cursors.Hand,
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 1;
            MarcarToggle(b, false, AzulMedio);
            return b;
        }

        public static void MarcarToggle(Button b, bool activo, Color colorActivo)
        {
            b.BackColor = activo ? colorActivo : ToggleApagado;
            b.ForeColor = activo ? Color.White : Texto;
            b.FlatAppearance.BorderColor = activo ? colorActivo : Borde;
            b.FlatAppearance.MouseOverBackColor = activo ? Oscurecer(colorActivo, 0.08f) : Color.FromArgb(222, 230, 239);
            b.FlatAppearance.MouseDownBackColor = activo ? Oscurecer(colorActivo, 0.16f) : Color.FromArgb(210, 220, 232);
        }

        // Titulo chico de seccion, en mayusculas y gris.
        public static Label Seccion(string texto)
        {
            return new Label
            {
                Text = texto.ToUpper(),
                Font = Negrita(8.75f),
                ForeColor = TextoSuave,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 0, 4)
            };
        }

        // Panel blanco con borde fino.
        public static Panel Tarjeta()
        {
            Panel p = new Panel { BackColor = Superficie, Padding = new Padding(16), Margin = new Padding(8) };
            p.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Borde))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            p.Resize += (s, e) => p.Invalidate();
            return p;
        }

        // Tarjeta de resumen de los reportes: titulo chico, valor grande y una franja de color.
        public static Panel TarjetaResumen(string titulo, Color acento, out Label valor)
        {
            Panel p = Tarjeta();
            p.Padding = new Padding(20, 12, 12, 12);
            p.Dock = DockStyle.Fill;
            p.Paint += (s, e) =>
            {
                using (SolidBrush b = new SolidBrush(acento))
                    e.Graphics.FillRectangle(b, 0, 0, 5, p.Height);
            };

            Label lblTitulo = new Label
            {
                Text = titulo.ToUpper(),
                Font = Negrita(8.5f),
                ForeColor = TextoSuave,
                AutoSize = true,
                Location = new Point(20, 14)
            };
            valor = new Label
            {
                Text = "-",
                Font = Negrita(18f),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(18, 36)
            };
            p.Controls.Add(lblTitulo);
            p.Controls.Add(valor);
            return p;
        }

        // Franja de encabezado de cada pantalla: color del juego u operacion, titulo y subtitulo.
        public static Panel Encabezado(Color color, out Label titulo, out Label subtitulo)
        {
            Panel p = new Panel { BackColor = color, Dock = DockStyle.Fill, Margin = new Padding(0) };
            titulo = new Label
            {
                Font = Negrita(19f),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 10),
                BackColor = Color.Transparent
            };
            subtitulo = new Label
            {
                Font = Fuente(10.5f),
                ForeColor = Aclarar(color, 0.82f),
                AutoSize = true,
                Location = new Point(25, 50),
                BackColor = Color.Transparent
            };
            p.Controls.Add(titulo);
            p.Controls.Add(subtitulo);
            return p;
        }

        public static void EstilizarGrilla(DataGridView g)
        {
            g.BackgroundColor = Superficie;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(232, 236, 241);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Azul;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Azul;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = Negrita(9.5f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.DefaultCellStyle.Font = Fuente(10f);
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.BackColor = Superficie;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(212, 230, 248);
            g.DefaultCellStyle.SelectionForeColor = Texto;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            g.RowTemplate.Height = 34;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Variante mas compacta (letra y margenes mas chicos) para grillas con muchas columnas.
        public static void CompactarGrilla(DataGridView g)
        {
            g.DefaultCellStyle.Font = Fuente(9.25f);
            g.DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
            g.ColumnHeadersDefaultCellStyle.Font = Negrita(9f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
        }

        // Columna de texto para las grillas: peso relativo de ancho y alineacion.
        public static DataGridViewTextBoxColumn Columna(string titulo, float peso, bool derecha = false, bool centro = false, int minimo = 0)
        {
            DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn
            {
                HeaderText = titulo,
                FillWeight = peso,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            if (minimo > 0)
                c.MinimumWidth = minimo;
            if (derecha)
            {
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else if (centro)
            {
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            return c;
        }

        // Pinta una celda como "etiqueta" de estado (texto en negrita del color del estado).
        public static void PintarEstado(DataGridViewCell celda, string texto, Color color)
        {
            celda.Value = texto;
            celda.Style.ForeColor = color;
            celda.Style.SelectionForeColor = color;
            celda.Style.Font = Negrita(9.5f);
        }
    }
}
