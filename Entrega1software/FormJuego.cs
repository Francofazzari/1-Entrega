using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Venta de los juegos que se juegan eligiendo varios numeros (Quini 6, Loto 5, Telekino,
    // Quiniela Plus...). La misma pantalla sirve para todos: el tablero se arma con las reglas
    // del juego (cuantos numeros se eligen y de que rango).
    public partial class FormJuego : Form
    {
        private readonly Juego juego;
        private readonly Color colorJuego;
        private readonly ApuestaBLL bll = new ApuestaBLL();

        private readonly SortedSet<int> seleccion = new SortedSet<int>();
        private readonly Dictionary<int, Button> botones = new Dictionary<int, Button>();
        private readonly List<Jugada> plantillas = new List<Jugada>();
        private bool seleccionAutomatica;

        public FormJuego(Juego juego)
        {
            InitializeComponent();
            this.juego = juego;
            colorJuego = Estilos.ColorJuego(juego.Codigo);

            pnlEncabezado.BackColor = colorJuego;
            lblSubtitulo.ForeColor = Estilos.Aclarar(colorJuego, 0.82f);
            lblTitulo.Text = juego.Nombre;
            lblSubtitulo.Text = "Elegí " + juego.CantidadNumeros + " números del " + Formatear(juego.NumeroMin) +
                                " al " + Formatear(juego.NumeroMax) + "   ·   " + Estilos.Plata(juego.Precio ?? 0) +
                                " por jugada   ·   Sorteo del " + DateTime.Today.ToString("dd/MM/yyyy");
            Text = juego.Nombre;

            btnAutomatica.ForeColor = colorJuego;
            btnAutomatica.FlatAppearance.BorderColor = Estilos.Aclarar(colorJuego, 0.35f);
            btnAutomatica.FlatAppearance.MouseOverBackColor = Estilos.Aclarar(colorJuego, 0.90f);

            CrearTablero();
            CrearColumnas();
            ActualizarSeleccion();
            ActualizarTotales();
        }

        private string Formatear(int numero)
        {
            return JuegoBLL.FormatearNumero(juego, numero);
        }

        // Una grilla de botones como la boleta: 5 columnas para los juegos chicos (Telekino, 25
        // numeros) y 10 para el resto.
        private void CrearTablero()
        {
            int cantidad = juego.NumeroMax - juego.NumeroMin + 1;
            int columnas = cantidad <= 25 ? 5 : 10;
            int filas = (cantidad + columnas - 1) / columnas;
            float tamFuente = cantidad > 60 ? 10f : 12f;

            tlpNumeros.SuspendLayout();
            tlpNumeros.ColumnCount = columnas;
            tlpNumeros.RowCount = filas;
            for (int c = 0; c < columnas; c++)
                tlpNumeros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columnas));
            for (int f = 0; f < filas; f++)
                tlpNumeros.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / filas));

            for (int i = 0; i < cantidad; i++)
            {
                int numero = juego.NumeroMin + i;
                Button b = Estilos.Toggle(Formatear(numero), tamFuente);
                b.Dock = DockStyle.Fill;
                b.Margin = new Padding(3);
                b.Tag = numero;
                b.Click += BtnNumero_Click;
                botones[numero] = b;
                tlpNumeros.Controls.Add(b, i % columnas, i / columnas);
            }
            tlpNumeros.ResumeLayout();
        }

        private void CrearColumnas()
        {
            Estilos.EstilizarGrilla(dgvJugadas);
            dgvJugadas.Columns.Add(Estilos.Columna("N°", 6, centro: true, minimo: 38));
            DataGridViewTextBoxColumn numeros = Estilos.Columna("Números", 60, minimo: 150);
            numeros.DefaultCellStyle.Font = Estilos.Mono(10.5f, true);
            numeros.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvJugadas.Columns.Add(numeros);
            dgvJugadas.Columns.Add(Estilos.Columna("Modalidad", 17, minimo: 92));
            dgvJugadas.Columns.Add(Estilos.Columna("Precio", 17, derecha: true, minimo: 90));
            dgvJugadas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void BtnNumero_Click(object sender, EventArgs e)
        {
            int numero = (int)((Button)sender).Tag;
            if (seleccion.Contains(numero))
            {
                seleccion.Remove(numero);
            }
            else if (seleccion.Count >= juego.CantidadNumeros)
            {
                ActualizarSeleccion("Ya elegiste los " + juego.CantidadNumeros + " números. Quitá uno para cambiarlo.");
                return;
            }
            else
            {
                seleccion.Add(numero);
            }
            seleccionAutomatica = false;
            ActualizarSeleccion();
        }

        private void btnAutomatica_Click(object sender, EventArgs e)
        {
            seleccion.Clear();
            foreach (int n in bll.GenerarNumerosAutomaticos(juego))
                seleccion.Add(n);
            seleccionAutomatica = true;
            ActualizarSeleccion();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            seleccion.Clear();
            seleccionAutomatica = false;
            ActualizarSeleccion();
        }

        private void ActualizarSeleccion(string aviso = null)
        {
            foreach (KeyValuePair<int, Button> par in botones)
                Estilos.MarcarToggle(par.Value, seleccion.Contains(par.Key), colorJuego);

            int faltan = juego.CantidadNumeros - seleccion.Count;
            if (aviso != null)
            {
                lblContador.ForeColor = Estilos.Rojo;
                lblContador.Text = aviso;
            }
            else if (faltan == 0)
            {
                lblContador.ForeColor = Estilos.Verde;
                lblContador.Text = seleccionAutomatica ? "Jugada automática completa" : "Jugada completa";
            }
            else
            {
                lblContador.ForeColor = Estilos.TextoSuave;
                lblContador.Text = seleccion.Count + " de " + juego.CantidadNumeros + " elegidos";
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            int faltan = juego.CantidadNumeros - seleccion.Count;
            if (faltan > 0)
            {
                ActualizarSeleccion(faltan == 1 ? "Falta elegir 1 número." : "Faltan elegir " + faltan + " números.");
                return;
            }

            Jugada j = new Jugada { EsAutomatica = seleccionAutomatica, Monto = juego.Precio ?? 0 };
            int orden = 1;
            foreach (int n in seleccion)
                j.Numeros.Add(new NumeroElegido { Numero = Formatear(n), Orden = orden++ });
            plantillas.Add(j);

            int fila = dgvJugadas.Rows.Add(
                plantillas.Count,
                string.Join(" ", j.Numeros.Select(n => n.Numero)),
                j.EsAutomatica ? "Automática" : "Manual",
                Estilos.Plata(j.Monto));
            dgvJugadas.Rows[fila].Tag = j;
            dgvJugadas.ClearSelection();

            seleccion.Clear();
            seleccionAutomatica = false;
            ActualizarSeleccion();
            ActualizarTotales();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            if (dgvJugadas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccioná en la lista la jugada que querés quitar.", "Quitar jugada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DataGridViewRow fila = dgvJugadas.SelectedRows[0];
            plantillas.Remove((Jugada)fila.Tag);
            dgvJugadas.Rows.Remove(fila);
            for (int i = 0; i < dgvJugadas.Rows.Count; i++)
                dgvJugadas.Rows[i].Cells[0].Value = i + 1;
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            lblTotalValor.Text = Estilos.Plata(plantillas.Sum(j => j.Monto));
            lblResumen.Text = plantillas.Count == 0
                ? "TOTAL DEL TICKET"
                : "TOTAL  ·  " + plantillas.Count + (plantillas.Count == 1 ? " jugada" : " jugadas");
            dgvJugadas.Invalidate();
        }

        private void dgvJugadas_Paint(object sender, PaintEventArgs e)
        {
            if (dgvJugadas.Rows.Count > 0) return;
            Rectangle area = new Rectangle(10, dgvJugadas.ColumnHeadersHeight, dgvJugadas.Width - 20, dgvJugadas.Height - dgvJugadas.ColumnHeadersHeight);
            using (Font f = Estilos.Fuente(10.5f))
            using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                e.Graphics.DrawString("Elegí los números en el tablero (o tocá Automática) y después AGREGAR JUGADA.", f, b, area, sf);
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                Apuesta a = bll.Vender(juego, null, null, plantillas, SesionManager.Instancia.UsuarioActual);
                using (FormTicket ticket = new FormTicket(a, VistaTicket.Venta))
                    ticket.ShowDialog(this);

                plantillas.Clear();
                dgvJugadas.Rows.Clear();
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo registrar la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
