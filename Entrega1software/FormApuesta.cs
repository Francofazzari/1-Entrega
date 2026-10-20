using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Paso 2 de la Quiniela: cargar las jugadas del ticket. Cada jugada cargada aca es una
    // "plantilla": al confirmar, la BLL la repite en cada sorteo (loteria x horario) elegido.
    public partial class FormApuesta : Form
    {
        private const string TeclaBorrar = "←";
        private const string TeclaLimpiar = "C";

        private readonly Juego juego;
        private readonly List<Loteria> loterias;
        private readonly List<Turno> turnos;
        private readonly int cantidadSorteos;
        private readonly Color colorJuego;
        private readonly ApuestaBLL bll = new ApuestaBLL();

        private readonly List<Jugada> plantillas = new List<Jugada>();
        private readonly List<int> rangosSeleccionados = new List<int> { 1 };
        private readonly List<Button> botonesRango = new List<Button>();
        private Button btnTodosRangos;
        private string tipo = ApuestaBLL.Directa;
        private string numeroActual = "";
        private NumeroElegido primerNumeroRedoblona;

        public FormApuesta(Juego juego, List<Loteria> loterias, List<Turno> turnos)
        {
            InitializeComponent();
            this.juego = juego;
            this.loterias = loterias;
            this.turnos = turnos;
            cantidadSorteos = loterias.Count * turnos.Count;
            colorJuego = Estilos.ColorJuego(juego.Codigo);

            pnlEncabezado.BackColor = colorJuego;
            lblSubtitulo.ForeColor = Estilos.Aclarar(colorJuego, 0.82f);
            lblTitulo.Text = juego.Nombre + (cantidadSorteos > 1 ? " múltiple" : "");
            lblSubtitulo.Text = DescribirSorteos();

            nudMonto.Minimum = ApuestaBLL.MontoMinimo;
            nudMonto.Maximum = ApuestaBLL.MontoMaximo;
            nudMonto.Value = ApuestaBLL.MontoMinimo;
            lblMontoTitulo.Text = ("Monto por jugada (" + Estilos.Plata(ApuestaBLL.MontoMinimo).Replace(",00", "") +
                                   " a " + Estilos.Plata(ApuestaBLL.MontoMaximo).Replace(",00", "") + ")").ToUpper();

            CrearColumnas();
            CrearTeclado();
            CrearBotonesRango();
            AplicarTipo(ApuestaBLL.Directa);
            ActualizarTotales();
        }

        private string DescribirSorteos()
        {
            string textoLoterias = loterias.Count > 1 && loterias.Count == new LoteriaBLL().ObtenerActivas().Count
                ? "Todas las loterías"
                : string.Join(", ", loterias.Select(l => l.Nombre));
            string textoTurnos = string.Join(", ", turnos.Select(t => t.Nombre));
            return textoLoterias + "   ·   " + textoTurnos + "   ·   " + cantidadSorteos +
                   (cantidadSorteos == 1 ? " sorteo" : " sorteos") + " del " + DateTime.Today.ToString("dd/MM/yyyy");
        }

        private void CrearColumnas()
        {
            Estilos.EstilizarGrilla(dgvJugadas);
            // La cantidad de sorteos es la misma para todas las jugadas: se muestra en el encabezado
            // y en el total, y el subtotal de cada fila ya la incluye.
            dgvJugadas.Columns.Add(Estilos.Columna("Tipo", 15, minimo: 86));
            DataGridViewTextBoxColumn numeros = Estilos.Columna("Números", 17, minimo: 90);
            numeros.DefaultCellStyle.Font = Estilos.Mono(11.5f, true);
            dgvJugadas.Columns.Add(numeros);
            dgvJugadas.Columns.Add(Estilos.Columna("Posición", 30, minimo: 122));
            dgvJugadas.Columns.Add(Estilos.Columna("Monto", 18, derecha: true, minimo: 84));
            dgvJugadas.Columns.Add(Estilos.Columna("Subtotal", 20, derecha: true, minimo: 92));
        }

        private void CrearTeclado()
        {
            tlpTeclado.ColumnCount = 3;
            tlpTeclado.RowCount = 4;
            for (int i = 0; i < 3; i++)
                tlpTeclado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            for (int i = 0; i < 4; i++)
                tlpTeclado.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            string[] teclas = { "1", "2", "3", "4", "5", "6", "7", "8", "9", TeclaBorrar, "0", TeclaLimpiar };
            for (int i = 0; i < teclas.Length; i++)
            {
                Button tecla = Estilos.Toggle(teclas[i], 13f);
                tecla.Dock = DockStyle.Fill;
                tecla.Margin = new Padding(3);
                if (teclas[i] == TeclaBorrar || teclas[i] == TeclaLimpiar)
                    tecla.ForeColor = Estilos.TextoSuave;
                string valor = teclas[i];
                tecla.Click += (s, e) => Tecla(valor);
                tlpTeclado.Controls.Add(tecla, i % 3, i / 3);
            }
        }

        private void CrearBotonesRango()
        {
            tlpRango.ColumnCount = ApuestaBLL.RangosValidos.Length + 1;
            tlpRango.RowCount = 1;
            for (int i = 0; i < tlpRango.ColumnCount; i++)
                tlpRango.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tlpRango.ColumnCount));
            tlpRango.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            int col = 0;
            foreach (int rango in ApuestaBLL.RangosValidos)
            {
                Button b = Estilos.Toggle(rango == 1 ? "A la 1ra" : "A los " + rango, 9f);
                b.Tag = rango;
                b.Dock = DockStyle.Fill;
                b.Margin = new Padding(2, 3, 2, 3);
                b.Click += BtnRango_Click;
                botonesRango.Add(b);
                tlpRango.Controls.Add(b, col++, 0);
            }

            btnTodosRangos = Estilos.Toggle("Todas", 9f);
            btnTodosRangos.Dock = DockStyle.Fill;
            btnTodosRangos.Margin = new Padding(2, 3, 0, 3);
            btnTodosRangos.Click += BtnTodosRangos_Click;
            tlpRango.Controls.Add(btnTodosRangos, col, 0);
        }

        // ------------------------------------------------------------- tipo de jugada

        private void btnDirecta_Click(object sender, EventArgs e) { AplicarTipo(ApuestaBLL.Directa); }

        private void btnRedoblona_Click(object sender, EventArgs e) { AplicarTipo(ApuestaBLL.Redoblona); }

        private void AplicarTipo(string nuevoTipo)
        {
            tipo = nuevoTipo;
            bool directa = tipo == ApuestaBLL.Directa;
            Estilos.MarcarToggle(btnDirecta, directa, colorJuego);
            Estilos.MarcarToggle(btnRedoblona, !directa, colorJuego);
            lblAyudaTipo.Text = directa
                ? "Un número de 1 a 4 cifras. Si elegís varias posiciones, se arma una jugada por cada una."
                : "Dos números de 2 cifras, cada uno con su posición. Lo que gana el primero se le apuesta al segundo.";

            // En la redoblona cada numero lleva una sola posicion.
            if (!directa && rangosSeleccionados.Count != 1)
            {
                int primera = rangosSeleccionados.Count > 0 ? rangosSeleccionados.Min() : 1;
                rangosSeleccionados.Clear();
                rangosSeleccionados.Add(primera);
            }
            btnTodosRangos.Enabled = directa;

            primerNumeroRedoblona = null;
            numeroActual = "";
            ActualizarRangos();
            ActualizarNumero();
            MostrarAviso("");
        }

        // ------------------------------------------------------------- posiciones

        private void BtnRango_Click(object sender, EventArgs e)
        {
            int rango = (int)((Button)sender).Tag;
            if (tipo == ApuestaBLL.Redoblona)
            {
                rangosSeleccionados.Clear();
                rangosSeleccionados.Add(rango);
            }
            else if (rangosSeleccionados.Contains(rango))
            {
                rangosSeleccionados.Remove(rango);
            }
            else
            {
                rangosSeleccionados.Add(rango);
            }
            ActualizarRangos();
            MostrarAviso("");
        }

        private void BtnTodosRangos_Click(object sender, EventArgs e)
        {
            bool estabanTodas = rangosSeleccionados.Count == ApuestaBLL.RangosValidos.Length;
            rangosSeleccionados.Clear();
            if (!estabanTodas)
                rangosSeleccionados.AddRange(ApuestaBLL.RangosValidos);
            ActualizarRangos();
            MostrarAviso("");
        }

        private void ActualizarRangos()
        {
            foreach (Button b in botonesRango)
                Estilos.MarcarToggle(b, rangosSeleccionados.Contains((int)b.Tag), colorJuego);

            bool todas = rangosSeleccionados.Count == ApuestaBLL.RangosValidos.Length;
            Estilos.MarcarToggle(btnTodosRangos, todas && btnTodosRangos.Enabled, Estilos.Oscurecer(colorJuego, 0.25f));
            if (!btnTodosRangos.Enabled)
                btnTodosRangos.BackColor = Color.FromArgb(244, 246, 249);
        }

        // ------------------------------------------------------------- numero

        private int MaximoCifras { get { return tipo == ApuestaBLL.Redoblona ? 2 : 4; } }

        private void Tecla(string tecla)
        {
            if (tecla == TeclaBorrar)
            {
                if (numeroActual.Length > 0)
                    numeroActual = numeroActual.Substring(0, numeroActual.Length - 1);
            }
            else if (tecla == TeclaLimpiar)
            {
                numeroActual = "";
            }
            else if (numeroActual.Length < MaximoCifras)
            {
                numeroActual += tecla;
            }
            ActualizarNumero();
            MostrarAviso("");
        }

        private void ActualizarNumero()
        {
            lblNumero.Text = numeroActual.Length > 0 ? numeroActual : new string('-', MaximoCifras);
            lblNumero.ForeColor = numeroActual.Length > 0 ? Estilos.Texto : Estilos.Borde;

            if (tipo != ApuestaBLL.Redoblona)
            {
                lblRedoblona.Text = "";
            }
            else if (primerNumeroRedoblona == null)
            {
                lblRedoblona.ForeColor = Estilos.TextoSuave;
                lblRedoblona.Text = "Cargá el 1° número con su posición.";
            }
            else
            {
                lblRedoblona.ForeColor = colorJuego;
                lblRedoblona.Text = "1° número: " + primerNumeroRedoblona.Numero + " " +
                                    ApuestaBLL.DescribirRango(primerNumeroRedoblona.Rango) + ".  Ahora cargá el 2°.";
            }
        }

        private void lblNumero_Paint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(Estilos.Borde))
                e.Graphics.DrawRectangle(p, 0, 0, lblNumero.Width - 1, lblNumero.Height - 1);
        }

        // Teclado fisico: numeros, borrar, Escape para limpiar y Enter para agregar. Si el foco
        // esta en el monto, las teclas van al monto.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!nudMonto.ContainsFocus)
            {
                if (keyData >= Keys.D0 && keyData <= Keys.D9) { Tecla(((int)(keyData - Keys.D0)).ToString()); return true; }
                if (keyData >= Keys.NumPad0 && keyData <= Keys.NumPad9) { Tecla(((int)(keyData - Keys.NumPad0)).ToString()); return true; }
                if (keyData == Keys.Back) { Tecla(TeclaBorrar); return true; }
                if (keyData == Keys.Escape || keyData == Keys.Delete) { Tecla(TeclaLimpiar); return true; }
                if (keyData == Keys.Enter) { Agregar(); return true; }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------- jugadas del ticket

        private void btnAgregar_Click(object sender, EventArgs e) { Agregar(); }

        private void Agregar()
        {
            if (numeroActual.Length == 0) { MostrarAviso("Ingresá un número con el teclado."); return; }
            if (rangosSeleccionados.Count == 0) { MostrarAviso("Elegí al menos una posición."); return; }
            decimal monto = nudMonto.Value;

            if (tipo == ApuestaBLL.Directa)
            {
                foreach (int rango in rangosSeleccionados.OrderBy(r => r))
                {
                    Jugada j = new Jugada { Tipo = ApuestaBLL.Directa, Monto = monto };
                    j.Numeros.Add(new NumeroElegido { Numero = numeroActual, Orden = 1, Rango = rango });
                    AgregarPlantilla(j);
                }
            }
            else
            {
                if (numeroActual.Length != 2) { MostrarAviso("En la redoblona cada número es de 2 cifras."); return; }
                NumeroElegido numero = new NumeroElegido { Numero = numeroActual, Rango = rangosSeleccionados[0] };
                if (primerNumeroRedoblona == null)
                {
                    numero.Orden = 1;
                    primerNumeroRedoblona = numero;
                    numeroActual = "";
                    ActualizarNumero();
                    MostrarAviso("");
                    return;
                }

                numero.Orden = 2;
                Jugada j = new Jugada { Tipo = ApuestaBLL.Redoblona, Monto = monto };
                j.Numeros.Add(primerNumeroRedoblona);
                j.Numeros.Add(numero);
                AgregarPlantilla(j);
                primerNumeroRedoblona = null;
            }

            numeroActual = "";
            ActualizarNumero();
            MostrarAviso("");
        }

        private void AgregarPlantilla(Jugada j)
        {
            plantillas.Add(j);
            List<NumeroElegido> numeros = j.Numeros.OrderBy(n => n.Orden).ToList();
            int fila = dgvJugadas.Rows.Add(
                j.Tipo,
                string.Join(" - ", numeros.Select(n => n.Numero)),
                string.Join(" · ", numeros.Select(n => Estilos.Capitalizar(ApuestaBLL.DescribirRango(n.Rango)))),
                Estilos.Plata(j.Monto),
                Estilos.Plata(j.Monto * cantidadSorteos));
            dgvJugadas.Rows[fila].Tag = j;
            dgvJugadas.ClearSelection();
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
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            decimal total = plantillas.Sum(j => j.Monto) * cantidadSorteos;
            lblTotalValor.Text = Estilos.Plata(total);
            lblResumen.Text = plantillas.Count == 0
                ? "TOTAL DEL TICKET"
                : "TOTAL  ·  " + plantillas.Count + (plantillas.Count == 1 ? " jugada" : " jugadas") +
                  " × " + cantidadSorteos + (cantidadSorteos == 1 ? " sorteo" : " sorteos");
            dgvJugadas.Invalidate();
        }

        // Grilla vacia: una indicacion de que hacer, en lugar de un recuadro blanco.
        private void dgvJugadas_Paint(object sender, PaintEventArgs e)
        {
            if (dgvJugadas.Rows.Count > 0) return;
            Rectangle area = new Rectangle(0, dgvJugadas.ColumnHeadersHeight, dgvJugadas.Width, dgvJugadas.Height - dgvJugadas.ColumnHeadersHeight);
            using (Font f = Estilos.Fuente(10.5f))
            using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                e.Graphics.DrawString("Cargá un número, elegí la posición y tocá AGREGAR AL TICKET.", f, b, area, sf);
        }

        private void MostrarAviso(string texto)
        {
            lblAviso.Text = texto;
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            if (primerNumeroRedoblona != null)
            {
                MostrarAviso("Falta cargar el 2° número de la redoblona.");
                return;
            }

            try
            {
                Apuesta a = bll.Vender(juego, loterias, turnos, plantillas, SesionManager.Instancia.UsuarioActual);
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
