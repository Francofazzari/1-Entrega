using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Operaciones de post-venta sobre un ticket, todas a partir de su codigo.
    public enum ModoOperacion { Pagar, Cancelar, Reimprimir }

    // Una sola pantalla para pagar, cancelar o reimprimir: se busca el ticket por su codigo (o se
    // elige de la lista de hoy), se ve su detalle y se aplica la operacion si las reglas lo permiten.
    public partial class FormOperacionTicket : Form
    {
        private static readonly Color ColorDeshabilitado = Color.FromArgb(196, 204, 214);

        private readonly ModoOperacion modo;
        private readonly Color colorModo;
        private readonly ApuestaBLL apuestaBll = new ApuestaBLL();
        private readonly PremioBLL premioBll = new PremioBLL();
        private readonly PagoBLL pagoBll = new PagoBLL();

        private Apuesta apuesta;
        private List<Premio> premios;
        private Label lblJuegoValor, lblFechaValor, lblVendedorValor, lblCodigoValor, lblTotalValor, lblEstadoValor;
        private DataGridViewColumn colResultado, colPremio;

        public FormOperacionTicket(ModoOperacion modo)
        {
            InitializeComponent();
            this.modo = modo;

            switch (modo)
            {
                case ModoOperacion.Pagar:
                    colorModo = Estilos.Verde;
                    lblTitulo.Text = "Pagar ticket";
                    lblSubtitulo.Text = "Buscá el ticket por su código: si ganó y sus sorteos ya se realizaron, se le paga al cliente.";
                    break;
                case ModoOperacion.Cancelar:
                    colorModo = Estilos.Rojo;
                    lblTitulo.Text = "Cancelar ticket";
                    lblSubtitulo.Text = "Se puede cancelar un ticket vigente mientras sus sorteos sigan abiertos. Se le devuelve el importe.";
                    break;
                default:
                    colorModo = Estilos.AzulMedio;
                    lblTitulo.Text = "Reimprimir ticket";
                    lblSubtitulo.Text = "Volvé a imprimir un ticket, por ejemplo si falló la impresora al venderlo.";
                    break;
            }
            pnlEncabezado.BackColor = colorModo;
            lblSubtitulo.ForeColor = Estilos.Aclarar(colorModo, 0.82f);
            Text = lblTitulo.Text;

            CrearResumen();
            CrearColumnas();
            MostrarTicket(null);
            CargarTicketsDeHoy();
        }

        private void CrearResumen()
        {
            tlpResumen.ColumnCount = 3;
            tlpResumen.RowCount = 4;
            for (int i = 0; i < 3; i++)
                tlpResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            tlpResumen.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            lblJuegoValor = Campo("Juego", 0, 0);
            lblFechaValor = Campo("Fecha y hora", 1, 0);
            lblVendedorValor = Campo("Vendedor", 2, 0);
            lblCodigoValor = Campo("Código", 0, 2);
            lblTotalValor = Campo("Total del ticket", 1, 2);
            lblEstadoValor = Campo("Estado", 2, 2);
            lblJuegoValor.Font = Estilos.Negrita(15f);
            lblCodigoValor.Font = Estilos.Mono(13f, true);
        }

        private Label Campo(string titulo, int columna, int fila)
        {
            Label lblTitulo = Estilos.Seccion(titulo);
            lblTitulo.Margin = new Padding(0, 0, 8, 0);
            Label valor = new Label
            {
                Dock = DockStyle.Fill,
                Font = Estilos.Negrita(12f),
                ForeColor = Estilos.Texto,
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = true,
                Margin = new Padding(0, 2, 8, 0)
            };
            tlpResumen.Controls.Add(lblTitulo, columna, fila);
            tlpResumen.Controls.Add(valor, columna, fila + 1);
            return valor;
        }

        private void CrearColumnas()
        {
            Estilos.EstilizarGrilla(dgvHoy);
            Estilos.CompactarGrilla(dgvHoy);
            dgvHoy.Columns.Add(Estilos.Columna("Hora", 12, minimo: 48));
            DataGridViewTextBoxColumn codigo = Estilos.Columna("Código", 20, minimo: 80);
            codigo.DefaultCellStyle.Font = Estilos.Mono(9.5f, true);
            dgvHoy.Columns.Add(codigo);
            dgvHoy.Columns.Add(Estilos.Columna("Juego", 26, minimo: 90));
            dgvHoy.Columns.Add(Estilos.Columna("Total", 22, derecha: true, minimo: 82));
            dgvHoy.Columns.Add(Estilos.Columna("Estado", 20, minimo: 76));

            // Las jugadas pueden ocupar dos renglones (resultado largo o 15 numeros del Telekino).
            Estilos.EstilizarGrilla(dgvJugadas);
            Estilos.CompactarGrilla(dgvJugadas);
            dgvJugadas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvJugadas.Columns.Add(Estilos.Columna("Sorteo", 20, minimo: 148));
            DataGridViewTextBoxColumn jugada = Estilos.Columna("Jugada", 28, minimo: 130);
            jugada.DefaultCellStyle.Font = Estilos.Mono(9.5f, true);
            jugada.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvJugadas.Columns.Add(jugada);
            dgvJugadas.Columns.Add(Estilos.Columna("Monto", 13, derecha: true, minimo: 80));
            dgvJugadas.Columns.Add(Estilos.Columna("Estado", 13, minimo: 76));
            colResultado = Estilos.Columna("Resultado", 30, minimo: 140);
            colResultado.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colPremio = Estilos.Columna("Premio", 14, derecha: true, minimo: 88);
            dgvJugadas.Columns.Add(colResultado);
            dgvJugadas.Columns.Add(colPremio);
            colResultado.Visible = colPremio.Visible = modo == ModoOperacion.Pagar;
        }

        private void CargarTicketsDeHoy()
        {
            dgvHoy.Rows.Clear();
            try
            {
                foreach (Apuesta a in apuestaBll.ListarVentas(DateTime.Today, DateTime.Today, null))
                {
                    int fila = dgvHoy.Rows.Add(a.FechaHora.ToString("HH:mm"), ApuestaBLL.CodigoCorto(a), a.Juego.Nombre,
                                               Estilos.Plata(a.Total), "");
                    Estilos.PintarEstado(dgvHoy.Rows[fila].Cells[4], Estilos.TextoEstadoTicket(a.Estado), Estilos.ColorEstadoTicket(a.Estado));
                    dgvHoy.Rows[fila].Tag = a;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudieron cargar los tickets de hoy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dgvHoy.ClearSelection();
        }

        private void dgvHoy_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            Apuesta elegido = (Apuesta)dgvHoy.Rows[e.RowIndex].Tag;
            txtCodigo.Text = ApuestaBLL.CodigoCompleto(elegido);
            Buscar();
        }

        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            Buscar();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            Buscar();
        }

        private void Buscar()
        {
            try
            {
                MostrarTicket(apuestaBll.BuscarPorCodigo(txtCodigo.Text));
            }
            catch (Exception ex)
            {
                MostrarTicket(null);
                lblVacio.ForeColor = Estilos.Rojo;
                lblVacio.Text = ex.Message;
            }
        }

        private void MostrarTicket(Apuesta a)
        {
            apuesta = a;
            tlpDetalle.Visible = a != null;
            lblVacio.Visible = a == null;
            if (a == null)
            {
                lblVacio.ForeColor = Estilos.TextoSuave;
                lblVacio.Text = "Buscá un ticket por su código o elegilo de la lista de tickets de hoy.";
                return;
            }

            lblJuegoValor.Text = a.Juego.Nombre;
            lblJuegoValor.ForeColor = Estilos.ColorJuego(a.Juego.Codigo);
            lblFechaValor.Text = a.FechaHora.ToString("dd/MM/yyyy  HH:mm");
            lblVendedorValor.Text = a.UsuarioNombre;
            lblCodigoValor.Text = ApuestaBLL.CodigoCorto(a);
            lblTotalValor.Text = Estilos.Plata(a.Total);
            lblEstadoValor.Text = Estilos.TextoEstadoTicket(a.Estado);
            lblEstadoValor.ForeColor = Estilos.ColorEstadoTicket(a.Estado);

            premios = modo == ModoOperacion.Pagar ? premioBll.Calcular(a) : null;
            dgvJugadas.Rows.Clear();
            for (int i = 0; i < a.Jugadas.Count; i++)
            {
                Jugada j = a.Jugadas[i];
                int fila = dgvJugadas.Rows.Add(Estilos.SorteoCorto(j.Sorteo), Estilos.DescribirJugada(j), Estilos.Plata(j.Monto), "", "", "");
                DataGridViewRow r = dgvJugadas.Rows[fila];
                Estilos.PintarEstado(r.Cells[3], j.Sorteo.Estado, Estilos.ColorEstadoSorteo(j.Sorteo.Estado));
                if (premios != null)
                {
                    r.Cells[4].Value = premios[i].Detalle;
                    r.Cells[5].Value = premios[i].Importe > 0 ? Estilos.Plata(premios[i].Importe) : "-";
                    if (premios[i].Importe > 0)
                        Estilos.PintarEstado(r.Cells[5], Estilos.Plata(premios[i].Importe), Estilos.Verde);
                }
            }
            dgvJugadas.ClearSelection();

            string motivo;
            string listo;
            switch (modo)
            {
                case ModoOperacion.Pagar:
                    decimal total = premios.Sum(p => p.Importe);
                    motivo = pagoBll.MotivoNoPagable(a, premios);
                    listo = "Ticket premiado: se le pagan " + Estilos.Plata(total) + " al cliente.";
                    btnAccion.Text = "PAGAR  " + Estilos.Plata(total);
                    break;
                case ModoOperacion.Cancelar:
                    motivo = apuestaBll.MotivoNoAnulable(a);
                    listo = "El ticket se puede cancelar: se le devuelven " + Estilos.Plata(a.Total) + " al cliente.";
                    btnAccion.Text = "CANCELAR TICKET";
                    break;
                default:
                    motivo = null;
                    listo = a.Estado == ApuestaBLL.Vigente
                        ? "La reimpresión sale marcada como REIMPRESIÓN."
                        : "El ticket está " + Estilos.TextoEstadoTicket(a.Estado).ToLower() + ": la reimpresión sale con ese sello.";
                    btnAccion.Text = "REIMPRIMIR TICKET";
                    break;
            }

            lblMotivo.Text = motivo ?? listo;
            lblMotivo.ForeColor = motivo != null ? Estilos.Rojo : (modo == ModoOperacion.Reimprimir ? Estilos.TextoSuave : Estilos.Verde);
            btnAccion.Enabled = motivo == null;
            Estilos.ColorearBoton(btnAccion, btnAccion.Enabled ? colorModo : ColorDeshabilitado);
        }

        private void btnAccion_Click(object sender, EventArgs e)
        {
            if (apuesta == null) return;
            Usuario usuario = SesionManager.Instancia.UsuarioActual;
            string codigo = ApuestaBLL.CodigoCorto(apuesta);

            try
            {
                switch (modo)
                {
                    case ModoOperacion.Pagar:
                        decimal aPagar = premios.Sum(p => p.Importe);
                        if (!Confirmar("¿Pagar " + Estilos.Plata(aPagar) + " del ticket " + codigo + "?", "Pagar ticket"))
                            return;
                        decimal pagado = pagoBll.Pagar(apuesta, usuario);
                        using (FormFelicitaciones festejo = new FormFelicitaciones(apuesta, pagado, premios))
                            festejo.ShowDialog(this);
                        break;

                    case ModoOperacion.Cancelar:
                        if (!Confirmar("¿Cancelar el ticket " + codigo + " por " + Estilos.Plata(apuesta.Total) + "?\nEsta operación no se puede deshacer.", "Cancelar ticket"))
                            return;
                        apuestaBll.Anular(apuesta, usuario);
                        MessageBox.Show("El ticket " + codigo + " quedó cancelado. Devolvé " + Estilos.Plata(apuesta.Total) + " al cliente.",
                            "Ticket cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    default:
                        apuestaBll.RegistrarReimpresion(apuesta, usuario);
                        using (FormTicket ticket = new FormTicket(apuesta, VistaTicket.Reimpresion))
                            ticket.ShowDialog(this);
                        break;
                }

                // Se vuelve a leer el ticket para mostrar su estado actualizado.
                MostrarTicket(apuestaBll.BuscarPorCodigo(ApuestaBLL.CodigoCompleto(apuesta)));
                CargarTicketsDeHoy();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Confirmar(string pregunta, string titulo)
        {
            return MessageBox.Show(pregunta, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
