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
    public enum VistaReporte { Ventas, Pagos, Ganadores }

    // Reportes de la agencia: ventas en linea, pagos en linea y numeros ganadores de cada sorteo.
    // No guardan nada: se arman leyendo los tickets, los pagos y los resultados.
    public partial class FormReporte : Form
    {
        private static readonly Color ColorDeshabilitado = Color.FromArgb(196, 204, 214);

        private readonly ApuestaBLL apuestaBll = new ApuestaBLL();
        private readonly PagoBLL pagoBll = new PagoBLL();
        private readonly SorteoBLL sorteoBll = new SorteoBLL();
        private readonly List<Juego> juegos;
        private readonly bool esAdmin;
        private bool cargando = true;

        // Ventas en linea
        private TableLayoutPanel pnlVentas;
        private DateTimePicker dtpVentasDesde, dtpVentasHasta;
        private ComboBox cmbVentasJuego;
        private DataGridView dgvVentas;
        private Label lblVendidos, lblTotalVendido, lblPagados, lblCancelados;

        // Pagos en linea
        private TableLayoutPanel pnlPagos;
        private DateTimePicker dtpPagosDesde, dtpPagosHasta;
        private DataGridView dgvPagos;
        private Label lblCantidadPagos, lblTotalPagado;

        // Numeros ganadores
        private TableLayoutPanel pnlGanadores;
        private ComboBox cmbGanJuego, cmbGanLoteria, cmbGanTurno;
        private Label lblGanLoteria, lblGanTurno, lblSorteo;
        private DateTimePicker dtpGanFecha;
        private Button btnCerrarVentas, btnCargarResultado, btnRealizarSorteo;
        private PanelDibujo pnlPizarra;
        private Juego juegoGanadores;
        private Sorteo sorteoActual;                                   // cuando se ve un solo sorteo
        private List<NumeroGanador> ganadores = new List<NumeroGanador>();
        private List<Celda> celdas = new List<Celda>();               // todos los sorteos elegidos

        public FormReporte() : this(VistaReporte.Ventas, DateTime.Today) { }

        public FormReporte(VistaReporte vista, DateTime desde)
        {
            InitializeComponent();
            lblTitulo.Text = "Reportes";
            lblSubtitulo.Text = "Ventas, pagos y resultados de los sorteos de la agencia";

            Usuario usuario = SesionManager.Instancia.UsuarioActual;
            esAdmin = usuario != null && usuario.Permisos.Any(p => p.Codigo == "Admin");
            juegos = new JuegoBLL().ObtenerActivos();

            pnlVentas = ConstruirVentas(desde);
            pnlPagos = ConstruirPagos(desde);
            pnlGanadores = ConstruirGanadores();
            pnlContenido.Controls.Add(pnlVentas);
            pnlContenido.Controls.Add(pnlPagos);
            pnlContenido.Controls.Add(pnlGanadores);
            cargando = false;

            CargarVentas();
            CargarPagos();
            MostrarVista(vista);
        }

        // ================================================================== pestanias

        private void btnVentas_Click(object sender, EventArgs e) { MostrarVista(VistaReporte.Ventas); }
        private void btnPagos_Click(object sender, EventArgs e) { MostrarVista(VistaReporte.Pagos); }
        private void btnGanadores_Click(object sender, EventArgs e) { MostrarVista(VistaReporte.Ganadores); }

        private void MostrarVista(VistaReporte vista)
        {
            pnlVentas.Visible = vista == VistaReporte.Ventas;
            pnlPagos.Visible = vista == VistaReporte.Pagos;
            pnlGanadores.Visible = vista == VistaReporte.Ganadores;
            Estilos.MarcarToggle(btnVentas, vista == VistaReporte.Ventas, Estilos.Turquesa);
            Estilos.MarcarToggle(btnPagos, vista == VistaReporte.Pagos, Estilos.Turquesa);
            Estilos.MarcarToggle(btnGanadores, vista == VistaReporte.Ganadores, Estilos.Turquesa);
            if (vista == VistaReporte.Ganadores)
                VerSorteo();
        }

        // ================================================================== ventas en linea

        private TableLayoutPanel ConstruirVentas(DateTime desde)
        {
            TableLayoutPanel t = Tabla(50, 104, -1, 24);

            dtpVentasDesde = Fecha(desde);
            dtpVentasHasta = Fecha(DateTime.Today);
            cmbVentasJuego = Combo(180);
            cmbVentasJuego.Items.Add(new Juego { Id = 0, Nombre = "Todos los juegos" });
            foreach (Juego j in juegos) cmbVentasJuego.Items.Add(j);
            cmbVentasJuego.SelectedIndex = 0;
            Button buscar = BotonFiltro("Buscar");
            buscar.Click += (s, e) => CargarVentas();
            t.Controls.Add(FilaFiltros(EtiquetaFiltro("Desde"), dtpVentasDesde, EtiquetaFiltro("Hasta"), dtpVentasHasta,
                                       EtiquetaFiltro("Juego"), cmbVentasJuego, buscar), 0, 0);

            TableLayoutPanel tarjetas = FilaTarjetas();
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Tickets vendidos", Estilos.AzulMedio, out lblVendidos), 0, 0);
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Total vendido", Estilos.Verde, out lblTotalVendido), 1, 0);
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Pagados", Estilos.Turquesa, out lblPagados), 2, 0);
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Cancelados", Estilos.Rojo, out lblCancelados), 3, 0);
            t.Controls.Add(tarjetas, 0, 1);

            dgvVentas = Grilla();
            dgvVentas.Columns.Add(Estilos.Columna("Fecha y hora", 18));
            dgvVentas.Columns.Add(ColumnaCodigo());
            dgvVentas.Columns.Add(Estilos.Columna("Juego", 16));
            dgvVentas.Columns.Add(Estilos.Columna("Vendedor", 20));
            dgvVentas.Columns.Add(Estilos.Columna("Total", 15, derecha: true));
            dgvVentas.Columns.Add(Estilos.Columna("Estado", 13));
            dgvVentas.CellDoubleClick += (s, e) => AbrirTicket(dgvVentas, e.RowIndex);
            t.Controls.Add(Marco(dgvVentas), 0, 2);
            t.Controls.Add(Ayuda("Doble clic en un ticket para verlo completo."), 0, 3);
            return t;
        }

        private void CargarVentas()
        {
            try
            {
                Juego juego = cmbVentasJuego.SelectedItem as Juego;
                if (juego != null && juego.Id == 0) juego = null;
                List<Apuesta> ventas = apuestaBll.ListarVentas(dtpVentasDesde.Value, dtpVentasHasta.Value, juego);

                dgvVentas.Rows.Clear();
                foreach (Apuesta a in ventas)
                {
                    int fila = dgvVentas.Rows.Add(a.FechaHora.ToString("dd/MM/yyyy HH:mm"), ApuestaBLL.CodigoCorto(a), "",
                                                  a.UsuarioNombre, Estilos.Plata(a.Total), "");
                    DataGridViewRow r = dgvVentas.Rows[fila];
                    Estilos.PintarEstado(r.Cells[2], a.Juego.Nombre, Estilos.ColorJuego(a.Juego.Codigo));
                    Estilos.PintarEstado(r.Cells[5], Estilos.TextoEstadoTicket(a.Estado), Estilos.ColorEstadoTicket(a.Estado));
                    r.Tag = a.CodigoTicket;
                }
                dgvVentas.ClearSelection();

                List<Apuesta> validas = ventas.Where(v => v.Estado != ApuestaBLL.Anulada).ToList();
                lblVendidos.Text = validas.Count.ToString("N0");
                lblTotalVendido.Text = Estilos.Plata(validas.Sum(v => v.Total));
                lblPagados.Text = ventas.Count(v => v.Estado == ApuestaBLL.Pagada).ToString("N0");
                lblCancelados.Text = ventas.Count(v => v.Estado == ApuestaBLL.Anulada).ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ventas en línea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ================================================================== pagos en linea

        private TableLayoutPanel ConstruirPagos(DateTime desde)
        {
            TableLayoutPanel t = Tabla(50, 104, -1, 24);

            dtpPagosDesde = Fecha(desde);
            dtpPagosHasta = Fecha(DateTime.Today);
            Button buscar = BotonFiltro("Buscar");
            buscar.Click += (s, e) => CargarPagos();
            t.Controls.Add(FilaFiltros(EtiquetaFiltro("Desde"), dtpPagosDesde, EtiquetaFiltro("Hasta"), dtpPagosHasta, buscar), 0, 0);

            TableLayoutPanel tarjetas = FilaTarjetas();
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Pagos realizados", Estilos.AzulMedio, out lblCantidadPagos), 0, 0);
            tarjetas.Controls.Add(Estilos.TarjetaResumen("Total pagado", Estilos.Verde, out lblTotalPagado), 1, 0);
            t.Controls.Add(tarjetas, 0, 1);

            dgvPagos = Grilla();
            dgvPagos.Columns.Add(Estilos.Columna("Fecha y hora", 20));
            dgvPagos.Columns.Add(ColumnaCodigo());
            dgvPagos.Columns.Add(Estilos.Columna("Juego", 18));
            dgvPagos.Columns.Add(Estilos.Columna("Pagado por", 24));
            dgvPagos.Columns.Add(Estilos.Columna("Monto", 18, derecha: true));
            dgvPagos.CellDoubleClick += (s, e) => AbrirTicket(dgvPagos, e.RowIndex);
            t.Controls.Add(Marco(dgvPagos), 0, 2);
            t.Controls.Add(Ayuda("Doble clic en un pago para ver el ticket."), 0, 3);
            return t;
        }

        private void CargarPagos()
        {
            try
            {
                List<Pago> pagos = pagoBll.Listar(dtpPagosDesde.Value, dtpPagosHasta.Value);
                dgvPagos.Rows.Clear();
                foreach (Pago p in pagos)
                {
                    int fila = dgvPagos.Rows.Add(p.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                        p.CodigoTicket.ToString("N").Substring(0, 8).ToUpperInvariant(), p.JuegoNombre, p.UsuarioNombre, "");
                    Estilos.PintarEstado(dgvPagos.Rows[fila].Cells[4], Estilos.Plata(p.Monto), Estilos.Verde);
                    dgvPagos.Rows[fila].Tag = p.CodigoTicket;
                }
                dgvPagos.ClearSelection();
                lblCantidadPagos.Text = pagos.Count.ToString("N0");
                lblTotalPagado.Text = Estilos.Plata(pagos.Sum(p => p.Monto));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Pagos en línea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AbrirTicket(DataGridView grilla, int fila)
        {
            if (fila < 0) return;
            try
            {
                Apuesta a = apuestaBll.BuscarPorCodigo(((Guid)grilla.Rows[fila].Tag).ToString());
                using (FormTicket ticket = new FormTicket(a, VistaTicket.Consulta))
                    ticket.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ================================================================== numeros ganadores

        private TableLayoutPanel ConstruirGanadores()
        {
            TableLayoutPanel t = Tabla(50, 64, -1);

            cmbGanJuego = Combo(160);
            foreach (Juego j in juegos) cmbGanJuego.Items.Add(j);
            lblGanLoteria = EtiquetaFiltro("Lotería");
            cmbGanLoteria = Combo(140);
            // La primera opcion de cada lista (Id 0) es "Todas" / "Todos".
            List<Loteria> loterias = new LoteriaBLL().ObtenerActivas();
            cmbGanLoteria.Items.Add(new Loteria { Id = 0, Nombre = "Todas" });
            foreach (Loteria l in loterias) cmbGanLoteria.Items.Add(l);
            lblGanTurno = EtiquetaFiltro("Turno");
            cmbGanTurno = Combo(130);
            cmbGanTurno.Items.Add(new Turno { Id = 0, Nombre = "Todos" });
            foreach (Turno tu in new TurnoBLL().ObtenerActivos()) cmbGanTurno.Items.Add(tu);
            dtpGanFecha = Fecha(DateTime.Today);

            if (cmbGanJuego.Items.Count > 0) cmbGanJuego.SelectedIndex = 0;
            if (cmbGanLoteria.Items.Count > 1)
                cmbGanLoteria.SelectedIndex = 1 + Math.Max(0, loterias.FindIndex(l => l.Codigo == "PBA"));
            if (cmbGanTurno.Items.Count > 1) cmbGanTurno.SelectedIndex = 1;

            cmbGanJuego.SelectedIndexChanged += (s, e) => VerSorteo();
            cmbGanLoteria.SelectedIndexChanged += (s, e) => VerSorteo();
            cmbGanTurno.SelectedIndexChanged += (s, e) => VerSorteo();
            dtpGanFecha.ValueChanged += (s, e) => VerSorteo();
            Button ver = BotonFiltro("Actualizar");
            ver.Click += (s, e) => VerSorteo();
            t.Controls.Add(FilaFiltros(EtiquetaFiltro("Juego"), cmbGanJuego, lblGanLoteria, cmbGanLoteria, lblGanTurno, cmbGanTurno,
                                       EtiquetaFiltro("Fecha"), dtpGanFecha, ver), 0, 0);

            // Estado del sorteo y acciones del administrador
            TableLayoutPanel estado = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = new Padding(8, 0, 8, 0) };
            estado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            estado.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            estado.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            estado.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            estado.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblSorteo = new Label { Dock = DockStyle.Fill, Font = Estilos.Negrita(12.5f), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            btnCerrarVentas = Estilos.BotonSecundario("Cerrar ventas", Estilos.Ambar, 10f);
            btnCerrarVentas.Dock = DockStyle.Fill;
            btnCerrarVentas.Margin = new Padding(6, 10, 0, 10);
            btnCerrarVentas.Click += (s, e) => CerrarVentas();
            btnCargarResultado = Estilos.BotonSecundario("Cargar resultado", Estilos.Turquesa, 10f);
            btnCargarResultado.Dock = DockStyle.Fill;
            btnCargarResultado.Margin = new Padding(8, 10, 0, 10);
            btnCargarResultado.Click += (s, e) => CargarResultado();
            btnRealizarSorteo = Estilos.Boton("Sortear al azar", Estilos.Turquesa, 10.5f);
            btnRealizarSorteo.Dock = DockStyle.Fill;
            btnRealizarSorteo.Margin = new Padding(8, 10, 0, 10);
            btnRealizarSorteo.Click += (s, e) => RealizarSorteo();
            btnCerrarVentas.Visible = btnCargarResultado.Visible = btnRealizarSorteo.Visible = esAdmin;
            estado.Controls.Add(lblSorteo, 0, 0);
            estado.Controls.Add(btnCerrarVentas, 1, 0);
            estado.Controls.Add(btnCargarResultado, 2, 0);
            estado.Controls.Add(btnRealizarSorteo, 3, 0);
            t.Controls.Add(estado, 0, 1);

            pnlPizarra = new PanelDibujo { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
            pnlPizarra.Paint += DibujarPizarra;
            t.Controls.Add(Marco(pnlPizarra), 0, 2);
            return t;
        }

        // Loterias y turnos elegidos ("Todas" / "Todos" = todos). Los juegos sin loteria ni turno
        // tienen un unico sorteo por fecha: devuelven [null].
        private List<Loteria> LoteriasElegidas()
        {
            if (juegoGanadores == null || !juegoGanadores.UsaLoteriaTurno) return new List<Loteria> { null };
            Loteria elegida = cmbGanLoteria.SelectedItem as Loteria;
            if (elegida == null || elegida.Id != 0) return new List<Loteria> { elegida };
            return cmbGanLoteria.Items.Cast<Loteria>().Where(l => l.Id != 0).ToList();
        }

        private List<Turno> TurnosElegidos()
        {
            if (juegoGanadores == null || !juegoGanadores.UsaLoteriaTurno) return new List<Turno> { null };
            Turno elegido = cmbGanTurno.SelectedItem as Turno;
            if (elegido == null || elegido.Id != 0) return new List<Turno> { elegido };
            return cmbGanTurno.Items.Cast<Turno>().Where(t => t.Id != 0).ToList();
        }

        private static bool Pendiente(Celda c)
        {
            return c.Sorteo == null || c.Sorteo.Estado != SorteoBLL.Sorteado;
        }

        private void VerSorteo()
        {
            if (cargando) return;
            juegoGanadores = cmbGanJuego.SelectedItem as Juego;
            if (juegoGanadores == null) return;

            bool usaLoteria = juegoGanadores.UsaLoteriaTurno;
            lblGanLoteria.Visible = cmbGanLoteria.Visible = lblGanTurno.Visible = cmbGanTurno.Visible = usaLoteria;

            try
            {
                DateTime fecha = dtpGanFecha.Value.Date;
                celdas = new List<Celda>();
                foreach (Loteria l in LoteriasElegidas())
                {
                    foreach (Turno t in TurnosElegidos())
                    {
                        Celda c = new Celda { Loteria = l, Turno = t, Sorteo = sorteoBll.Buscar(juegoGanadores, l, t, fecha) };
                        if (c.Sorteo != null && c.Sorteo.Estado == SorteoBLL.Sorteado)
                            c.Numeros = sorteoBll.ObtenerNumerosGanadores(c.Sorteo);
                        celdas.Add(c);
                    }
                }

                bool uno = celdas.Count == 1;
                sorteoActual = uno ? celdas[0].Sorteo : null;
                ganadores = uno ? celdas[0].Numeros : new List<NumeroGanador>();

                if (uno)
                {
                    Sorteo descripto = sorteoActual ?? Virtual(celdas[0], fecha);
                    lblSorteo.Text = SorteoBLL.Descripcion(descripto) + "     " +
                                     (sorteoActual == null ? "Sin apuestas todavía" : sorteoActual.Estado);
                    lblSorteo.ForeColor = sorteoActual == null ? Estilos.TextoSuave : Estilos.ColorEstadoSorteo(sorteoActual.Estado);
                }
                else
                {
                    int sorteados = celdas.Count(c => !Pendiente(c));
                    Loteria l = cmbGanLoteria.SelectedItem as Loteria;
                    Turno t = cmbGanTurno.SelectedItem as Turno;
                    string cuales = l.Id == 0 && t.Id == 0 ? "Todas las loterías y turnos"
                                  : (l.Id == 0 ? "Todas las loterías" : l.Nombre) + " · " + (t.Id == 0 ? "Todos los turnos" : t.Nombre);
                    lblSorteo.Text = juegoGanadores.Nombre + " · " + cuales + " · " + fecha.ToString("dd/MM/yyyy") +
                                     "     " + sorteados + " de " + celdas.Count + " sorteados";
                    lblSorteo.ForeColor = sorteados == celdas.Count ? Estilos.ColorEstadoSorteo(SorteoBLL.Sorteado)
                                        : sorteados == 0 ? Estilos.TextoSuave : Estilos.Ambar;
                }

                // Sortear al azar y cerrar ventas actuan sobre todos los sorteos elegidos; la carga
                // manual del resultado es de a un sorteo.
                bool futuro = fecha > DateTime.Today;
                btnRealizarSorteo.Enabled = !futuro && celdas.Any(Pendiente);
                btnCargarResultado.Enabled = uno && btnRealizarSorteo.Enabled;
                btnCerrarVentas.Enabled = celdas.Any(c => c.Sorteo != null && c.Sorteo.Estado == SorteoBLL.Abierto);
                btnCerrarVentas.FlatAppearance.BorderColor = btnCerrarVentas.Enabled ? Estilos.Aclarar(Estilos.Ambar, 0.35f) : Estilos.Borde;
                Estilos.ColorearBoton(btnRealizarSorteo, btnRealizarSorteo.Enabled ? Estilos.Turquesa : ColorDeshabilitado);
                btnCargarResultado.FlatAppearance.BorderColor = btnCargarResultado.Enabled ? Estilos.Aclarar(Estilos.Turquesa, 0.35f) : Estilos.Borde;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Números ganadores", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            pnlPizarra.Invalidate();
        }

        // Sorteo "de muestra" para describir una combinacion que todavia no tiene sorteo creado.
        private Sorteo Virtual(Celda c, DateTime fecha)
        {
            return new Sorteo { Juego = juegoGanadores, Loteria = c.Loteria, Turno = c.Turno, Fecha = fecha };
        }

        private void CerrarVentas()
        {
            List<Sorteo> abiertos = celdas.Where(c => c.Sorteo != null && c.Sorteo.Estado == SorteoBLL.Abierto).Select(c => c.Sorteo).ToList();
            if (abiertos.Count == 0) return;
            try
            {
                foreach (Sorteo s in abiertos)
                    sorteoBll.CerrarVentas(s, SesionManager.Instancia.UsuarioActual);
                VerSorteo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cerrar ventas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Abre la carga del resultado (de un solo sorteo): los numeros que se escriben quedan en su
        // puesto y el resto sale al azar. Si el sorteo todavia no existe (no tuvo ventas), se crea.
        private void CargarResultado()
        {
            if (juegoGanadores == null || celdas.Count != 1) return;
            try
            {
                Celda c = celdas[0];
                Sorteo s = c.Sorteo ?? sorteoBll.ObtenerOCrear(juegoGanadores, c.Loteria, c.Turno, dtpGanFecha.Value.Date);
                using (FormResultado carga = new FormResultado(s))
                    carga.ShowDialog(this);
                VerSorteo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cargar resultado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Sortea al azar todos los sorteos elegidos que todavia no se realizaron (con "Todas", por
        // ejemplo, las cinco loterias de un turno; los ya sorteados se dejan como estan).
        private void RealizarSorteo()
        {
            if (juegoGanadores == null) return;
            DateTime fecha = dtpGanFecha.Value.Date;
            List<Celda> pendientes = celdas.Where(Pendiente).ToList();
            if (pendientes.Count == 0) return;

            string pregunta = pendientes.Count == 1
                ? "¿Sortear al azar " + SorteoBLL.Descripcion(pendientes[0].Sorteo ?? Virtual(pendientes[0], fecha)) + "?"
                : "¿Sortear al azar los " + pendientes.Count + " sorteos que faltan?\n\n" +
                  string.Join("\n", pendientes.Select(c => "•  " + SorteoBLL.DescripcionCorta(c.Sorteo ?? Virtual(c, fecha))));
            if (MessageBox.Show(pregunta + "\n\nSe generan los números ganadores y dejan de aceptar apuestas.", "Sortear al azar",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                foreach (Celda c in pendientes)
                {
                    Sorteo s = c.Sorteo ?? sorteoBll.ObtenerOCrear(juegoGanadores, c.Loteria, c.Turno, fecha);
                    sorteoBll.RealizarSorteo(s, SesionManager.Instancia.UsuarioActual);
                }
                VerSorteo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sortear al azar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                VerSorteo();
            }
        }

        private void DibujarPizarra(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Rectangle area = pnlPizarra.ClientRectangle;
            area.Inflate(-28, -22);
            if (juegoGanadores == null || area.Width <= 0 || area.Height <= 0) return;

            if (celdas.Count > 1)
            {
                DibujarVarias(g, area, Estilos.ColorJuego(juegoGanadores.Codigo));
                return;
            }

            if (ganadores.Count == 0)
            {
                string texto = sorteoActual == null
                    ? "Todavía no hay apuestas ni resultado para este sorteo."
                    : "El sorteo todavía no se realizó.";
                if (esAdmin && btnRealizarSorteo.Enabled)
                    texto += "\nPodés cargar el resultado o sortearlo al azar con los botones de arriba.";
                using (Font f = Estilos.Fuente(12f))
                using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(texto, f, b, area, sf);
                return;
            }

            Color color = Estilos.ColorJuego(juegoGanadores.Codigo);
            if (juegoGanadores.UsaRango)
                DibujarQuiniela(g, area, color);
            else
                DibujarBolillas(g, area, color);
        }

        // Varias loterias y/o turnos: una columna por sorteo con sus 20 numeros, como las tablas de
        // resultados de los diarios. Las columnas de sorteos que faltan dicen "Sin sortear".
        private void DibujarVarias(Graphics g, Rectangle area, Color color)
        {
            int columnas = celdas.Count, filas = juegoGanadores.CantidadSorteados;
            const int anchoPos = 40, altoCab = 42;
            int anchoCol = Math.Max(40, Math.Min(170, (area.Width - anchoPos) / columnas));
            int altoFila = Math.Max(13, Math.Min(30, (area.Height - altoCab) / filas));
            int x0 = area.Left + Math.Max(0, (area.Width - (anchoPos + anchoCol * columnas)) / 2);
            int y0 = area.Top;

            bool variasLoterias = celdas.Select(c => c.Loteria != null ? c.Loteria.Id : 0).Distinct().Count() > 1;
            bool variosTurnos = celdas.Select(c => c.Turno != null ? c.Turno.Id : 0).Distinct().Count() > 1;

            using (Font fCab = Estilos.Negrita(Math.Max(7f, Math.Min(10f, anchoCol / 10f))))
            using (Font fNum = Estilos.Mono(Math.Max(7f, Math.Min(13f, Math.Min(altoFila * 0.5f, anchoCol / 5.2f))), true))
            using (Font fPos = Estilos.Negrita(Math.Max(7f, Math.Min(9.5f, altoFila * 0.36f))))
            using (Font fPend = Estilos.Fuente(Math.Max(7f, Math.Min(9.5f, anchoCol / 11f))))
            using (SolidBrush texto = new SolidBrush(Estilos.Texto))
            using (SolidBrush suave = new SolidBrush(Estilos.TextoSuave))
            using (StringFormat centro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            {
                // filas alternadas y numero de puesto a la izquierda; la cabeza (1er puesto) resaltada
                for (int f = 0; f < filas; f++)
                {
                    int y = y0 + altoCab + f * altoFila;
                    Color fondo = f == 0 ? Estilos.Aclarar(color, 0.86f) : (f % 2 == 0 ? Color.FromArgb(246, 248, 251) : Color.White);
                    using (SolidBrush b = new SolidBrush(fondo))
                        g.FillRectangle(b, x0, y, anchoPos + anchoCol * columnas, altoFila);
                    g.DrawString((f + 1) + "°", fPos, suave, new RectangleF(x0, y, anchoPos, altoFila), centro);
                }

                for (int c = 0; c < columnas; c++)
                {
                    Celda celda = celdas[c];
                    int x = x0 + anchoPos + c * anchoCol;
                    bool sorteado = !Pendiente(celda);

                    // encabezado: loteria y/o turno, en el color del juego si ya se sorteo
                    Rectangle cab = new Rectangle(x + 2, y0, anchoCol - 4, altoCab - 4);
                    using (SolidBrush b = new SolidBrush(sorteado ? color : Estilos.ToggleApagado))
                        g.FillRectangle(b, cab);
                    // Si la columna es angosta se usa la sigla de la loteria (PBA, CABA...) y el turno abreviado.
                    bool angosta = anchoCol < 80;
                    string loteria = celda.Loteria == null ? "" : (angosta ? celda.Loteria.Codigo : celda.Loteria.Nombre);
                    string turno = celda.Turno == null ? "" : (angosta && celda.Turno.Nombre.Length > 5 ? celda.Turno.Nombre.Substring(0, 4) + "." : celda.Turno.Nombre);
                    string titulo = variasLoterias && variosTurnos ? loteria + "\n" + turno : variasLoterias ? loteria : turno;
                    using (SolidBrush b = new SolidBrush(sorteado ? Color.White : Estilos.TextoSuave))
                        g.DrawString(titulo, fCab, b, cab, centro);

                    if (!sorteado)
                    {
                        string estado = celda.Sorteo == null ? "Sin sortear" : "Sin sortear\n(" + celda.Sorteo.Estado.ToLower() + ")";
                        g.DrawString(estado, fPend, suave, new RectangleF(x, y0 + altoCab, anchoCol, filas * altoFila), centro);
                        continue;
                    }
                    foreach (NumeroGanador n in celda.Numeros)
                    {
                        int y = y0 + altoCab + (n.Posicion - 1) * altoFila;
                        g.DrawString(n.Numero, fNum, texto, new RectangleF(x, y, anchoCol, altoFila), centro);
                    }
                }

                // separadores verticales entre columnas
                using (Pen p = new Pen(Estilos.Borde))
                    for (int c = 1; c < columnas; c++)
                    {
                        int x = x0 + anchoPos + c * anchoCol;
                        g.DrawLine(p, x, y0 + altoCab, x, y0 + altoCab + filas * altoFila);
                    }
            }
        }

        // Pizarra de la Quiniela: los 20 numeros en dos columnas (del 1 al 10 y del 11 al 20). El
        // primer puesto, "la cabeza", va resaltado.
        private void DibujarQuiniela(Graphics g, Rectangle area, Color color)
        {
            const int filas = 10, separacion = 36;
            int alto = Math.Max(24, Math.Min(50, area.Height / filas));
            int anchoColumna = Math.Min(380, (area.Width - separacion) / 2);
            int x0 = area.Left + (area.Width - (2 * anchoColumna + separacion)) / 2;
            int y0 = area.Top + Math.Max(0, (area.Height - filas * alto) / 2);

            using (Font fPosicion = Estilos.Negrita(Math.Max(8f, alto / 4.2f)))
            using (Font fNumero = Estilos.Mono(Math.Max(11f, alto / 2.3f), true))
            using (SolidBrush texto = new SolidBrush(Estilos.Texto))
            using (StringFormat centro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (StringFormat izquierda = new StringFormat { LineAlignment = StringAlignment.Center })
            {
                foreach (NumeroGanador n in ganadores)
                {
                    int i = n.Posicion - 1;
                    int x = x0 + (i / filas) * (anchoColumna + separacion);
                    int y = y0 + (i % filas) * alto;

                    Color fondo = n.Posicion == 1 ? Estilos.Aclarar(color, 0.86f) : (i % 2 == 0 ? Color.FromArgb(246, 248, 251) : Color.White);
                    using (SolidBrush b = new SolidBrush(fondo))
                        g.FillRectangle(b, x, y + 2, anchoColumna, alto - 4);

                    int d = alto - 12;
                    Rectangle circulo = new Rectangle(x + 10, y + 6, d, d);
                    using (SolidBrush b = new SolidBrush(n.Posicion == 1 ? color : Estilos.Aclarar(color, 0.72f)))
                        g.FillEllipse(b, circulo);
                    using (SolidBrush b = new SolidBrush(n.Posicion == 1 ? Color.White : Estilos.Oscurecer(color, 0.2f)))
                        g.DrawString(n.Posicion + "°", fPosicion, b, circulo, centro);

                    g.DrawString(n.Numero, fNumero, texto, new RectangleF(x + d + 26, y, anchoColumna - d - 30, alto), izquierda);
                }
            }
        }

        // Demas juegos: los numeros que salieron como bolillas, de menor a mayor, y abajo el orden
        // en que fueron saliendo.
        private void DibujarBolillas(Graphics g, Rectangle area, Color color)
        {
            List<NumeroGanador> ordenados = ganadores.OrderBy(n => int.Parse(n.Numero)).ToList();
            int separacion = 18, d = 84, porFila, filas;
            do
            {
                porFila = Math.Max(1, Math.Min(ordenados.Count, (area.Width + separacion) / (d + separacion)));
                filas = (ordenados.Count + porFila - 1) / porFila;
                if (filas * (d + separacion) <= area.Height - 40 || d <= 40) break;
                d -= 4;
            } while (true);

            int y0 = area.Top + Math.Max(0, (area.Height - 40 - filas * (d + separacion)) / 2);
            using (Font fNumero = Estilos.Negrita(d / 4.6f))
            using (StringFormat centro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip })
            {
                for (int i = 0; i < ordenados.Count; i++)
                {
                    int fila = i / porFila, col = i % porFila;
                    int enEstaFila = Math.Min(porFila, ordenados.Count - fila * porFila);
                    int anchoFila = enEstaFila * d + (enEstaFila - 1) * separacion;
                    int x = area.Left + (area.Width - anchoFila) / 2 + col * (d + separacion);
                    int y = y0 + fila * (d + separacion);

                    Rectangle bolilla = new Rectangle(x, y, d, d);
                    using (LinearGradientBrush lg = new LinearGradientBrush(bolilla, Estilos.Aclarar(color, 0.25f), Estilos.Oscurecer(color, 0.18f), 50f))
                        g.FillEllipse(lg, bolilla);
                    int interior = (int)(d * 0.64);
                    Rectangle centroBlanco = new Rectangle(x + (d - interior) / 2, y + (d - interior) / 2, interior, interior);
                    g.FillEllipse(Brushes.White, centroBlanco);
                    using (SolidBrush b = new SolidBrush(Estilos.Oscurecer(color, 0.15f)))
                        g.DrawString(ordenados[i].Numero, fNumero, b, centroBlanco, centro);
                }
            }

            string orden = "Orden de salida:  " + string.Join("  ·  ", ganadores.OrderBy(n => n.Posicion).Select(n => n.Numero));
            using (Font f = Estilos.Fuente(10f))
            using (SolidBrush b = new SolidBrush(Estilos.TextoSuave))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString(orden, f, b, new RectangleF(area.Left, area.Bottom - 26, area.Width, 26), sf);
        }

        // ================================================================== ayudas de armado

        // Tabla de una columna con filas de alto fijo; -1 es la fila que ocupa el resto.
        private static TableLayoutPanel Tabla(params int[] altos)
        {
            TableLayoutPanel t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = altos.Length, Margin = new Padding(0) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int alto in altos)
                t.RowStyles.Add(alto < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, alto));
            return t;
        }

        private static TableLayoutPanel FilaTarjetas()
        {
            TableLayoutPanel t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = new Padding(0) };
            for (int i = 0; i < 4; i++)
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return t;
        }

        private static FlowLayoutPanel FilaFiltros(params Control[] controles)
        {
            FlowLayoutPanel f = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0), Padding = new Padding(8, 6, 0, 0) };
            f.Controls.AddRange(controles);
            return f;
        }

        private static Label EtiquetaFiltro(string texto)
        {
            return new Label { Text = texto, AutoSize = true, Font = Estilos.Negrita(9.5f), ForeColor = Estilos.TextoSuave, Margin = new Padding(0, 10, 6, 0) };
        }

        private static DateTimePicker Fecha(DateTime valor)
        {
            return new DateTimePicker { Format = DateTimePickerFormat.Short, Value = valor, Width = 128, Font = Estilos.Fuente(10.5f), Margin = new Padding(0, 4, 18, 0) };
        }

        private static ComboBox Combo(int ancho)
        {
            return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = ancho, Font = Estilos.Fuente(10.5f), DisplayMember = "Nombre", Margin = new Padding(0, 4, 18, 0) };
        }

        private static Button BotonFiltro(string texto)
        {
            Button b = Estilos.Boton(texto, Estilos.Azul, 10f);
            b.Size = new Size(120, 34);
            b.Margin = new Padding(0, 2, 8, 0);
            return b;
        }

        private static DataGridView Grilla()
        {
            DataGridView g = new DataGridView { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Estilos.EstilizarGrilla(g);
            return g;
        }

        private static DataGridViewTextBoxColumn ColumnaCodigo()
        {
            DataGridViewTextBoxColumn c = Estilos.Columna("Código", 14);
            c.DefaultCellStyle.Font = Estilos.Mono(10f, true);
            return c;
        }

        // Marco blanco con borde alrededor de una grilla o de la pizarra.
        private static Panel Marco(Control contenido)
        {
            Panel p = Estilos.Tarjeta();
            p.Dock = DockStyle.Fill;
            p.Padding = new Padding(1);
            p.Margin = new Padding(8, 6, 8, 4);
            p.Controls.Add(contenido);
            return p;
        }

        private static Label Ayuda(string texto)
        {
            return new Label { Text = texto, Dock = DockStyle.Fill, Font = Estilos.Fuente(9f), ForeColor = Estilos.TextoSuave, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };
        }

        // Un sorteo elegido en la pestania de numeros ganadores: la combinacion de loteria y turno y,
        // si ya existe, el sorteo con sus numeros.
        private class Celda
        {
            public Loteria Loteria;
            public Turno Turno;
            public Sorteo Sorteo;
            public List<NumeroGanador> Numeros = new List<NumeroGanador>();
        }

        // Panel con doble buffer, para que la pizarra no parpadee al redimensionar.
        private class PanelDibujo : Panel
        {
            public PanelDibujo()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}
