using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormApuesta : Form
    {
        private static readonly Color ColorRangoNormal = Color.FromArgb(240, 244, 248);
        private static readonly Color ColorRangoSeleccionado = Color.FromArgb(29, 158, 117);

        private readonly List<Loteria> loteriasSeleccionadas;
        private readonly List<Turno> turnosSeleccionados;
        private readonly ApuestaBLL bll = new ApuestaBLL();
        private readonly DataTable tablaJugadas = new DataTable();
        private readonly List<Jugada> jugadas = new List<Jugada>();
        private int rangoSeleccionado = 1;

        public FormApuesta(List<Loteria> loterias, List<Turno> turnos)
        {
            InitializeComponent();

            loteriasSeleccionadas = loterias;
            turnosSeleccionados = turnos;

            tablaJugadas.Columns.Add("Numero", typeof(string));
            tablaJugadas.Columns.Add("Rango", typeof(string));
            tablaJugadas.Columns.Add("Precio", typeof(decimal));
            dgvJugadas.DataSource = tablaJugadas;
            dgvJugadas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvJugadas.BringToFront();

            lblTitulo.Text = loteriasSeleccionadas.Count > 1 ? "Quiniela Multiples" : "Quiniela";
            lblLoterias.Text = loteriasSeleccionadas.Count == 5
                ? "Todas las loterias seleccionadas"
                : "Loterias: " + string.Join(", ", loteriasSeleccionadas.Select(l => l.Nombre));
            lblTurnos.Text = "Horarios: " + string.Join(", ", turnosSeleccionados.Select(t => t.Nombre));

            ActualizarTotales();
        }

        private void BtnRango_Click(object sender, EventArgs e)
        {
            Button btnElegido = (Button)sender;
            rangoSeleccionado = (int)btnElegido.Tag;

            foreach (Control c in tlpRango.Controls)
            {
                if (c is Button btn)
                {
                    bool esElElegido = btn == btnElegido;
                    btn.BackColor = esElElegido ? ColorRangoSeleccionado : ColorRangoNormal;
                    btn.ForeColor = esElElegido ? Color.White : Color.Black;
                }
            }
        }

        private void TeclaNumerica_Click(object sender, EventArgs e)
        {
            if (txtNumero.Text.Length >= 4) return;
            txtNumero.Text += ((Button)sender).Text;
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (txtNumero.Text.Length > 0)
                txtNumero.Text = txtNumero.Text.Substring(0, txtNumero.Text.Length - 1);
        }

        private void btnAgregarNumero_Click(object sender, EventArgs e)
        {
            string numero = txtNumero.Text.Trim();
            if (numero.Length < 1 || numero.Length > 4 || !numero.All(char.IsDigit))
            {
                MessageBox.Show("Ingrese un numero de 1 a 4 cifras.", "Atencion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Jugada jugada = new Jugada { Numero = numero, Rango = rangoSeleccionado, Monto = nudMonto.Value };
            jugadas.Add(jugada);

            decimal precio = jugada.Monto * loteriasSeleccionadas.Count * turnosSeleccionados.Count;
            tablaJugadas.Rows.Add(jugada.Numero, DescripcionRango(jugada.Rango), precio);

            txtNumero.Text = "";
            ActualizarTotales();
        }

        private static string DescripcionRango(int rango)
        {
            return rango == 1 ? "A la primera" : "A los " + rango;
        }

        private void nudMonto_ValueChanged(object sender, EventArgs e)
        {
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            lblApuestaValor.Text = (nudMonto.Value * loteriasSeleccionadas.Count * turnosSeleccionados.Count).ToString("N2");

            decimal total = 0;
            foreach (DataRow fila in tablaJugadas.Rows)
                total += (decimal)fila["Precio"];
            lblTotalValor.Text = total.ToString("N2");
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                if (jugadas.Count == 0)
                {
                    MessageBox.Show("Cargue al menos un numero antes de enviar.", "Atencion",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Apuesta a = new Apuesta
                {
                    UsuarioId = SesionManager.Instancia.UsuarioActual.Id,
                    Tipo = cmbTipo.SelectedItem.ToString(),
                    Loterias = loteriasSeleccionadas,
                    Turnos = turnosSeleccionados,
                    Jugadas = jugadas
                };

                bll.CargarApuesta(a);

                BitacoraBLL bitacora = new BitacoraBLL();
                bitacora.Registrar(
                    SesionManager.Instancia.UsuarioActual.Id,
                    SesionManager.Instancia.UsuarioActual.Nombre,
                    "Cargar Apuesta",
                    "Apuesta Nro " + a.Id + " - " + a.Tipo + " - Total $" + a.Total.ToString("N2")
                );

                MessageBox.Show(
                    "Apuesta cargada correctamente.\nComprobante Nro " + a.Id + " - Total $" + a.Total.ToString("N2"),
                    "Apuesta enviada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                jugadas.Clear();
                tablaJugadas.Rows.Clear();
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
