using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Carga del resultado de un sorteo: se escriben los numeros que salieron (el que informa la
    // loteria) y los puestos que quedan vacios se completan al azar.
    public partial class FormResultado : Form
    {
        private const int AltoFila = 46;

        private readonly Sorteo sorteo;
        private readonly SorteoBLL bll = new SorteoBLL();
        private readonly List<TextBox> casilleros = new List<TextBox>();

        public FormResultado(Sorteo sorteo)
        {
            InitializeComponent();
            this.sorteo = sorteo;
            Juego juego = sorteo.Juego;

            lblTitulo.Text = "Cargar resultado";
            lblSubtitulo.Text = SorteoBLL.Descripcion(sorteo);
            lblAyuda.Text = juego.UsaRango
                ? "Escribí el número que salió en cada puesto (de 1 a 4 cifras: un 32 se guarda como 0032). " +
                  "Los puestos que dejes vacíos se completan al azar."
                : "Escribí los " + juego.CantidadSorteados + " números que salieron, en el orden de salida (del " +
                  JuegoBLL.FormatearNumero(juego, juego.NumeroMin) + " al " + JuegoBLL.FormatearNumero(juego, juego.NumeroMax) +
                  "). Los que dejes vacíos se completan al azar.";

            ConstruirCasilleros();
        }

        // Quiniela: 20 puestos en dos columnas (del 1 al 10 y del 11 al 20), como la pizarra.
        // Demas juegos: los numeros en filas de 5, en el orden de salida.
        private void ConstruirCasilleros()
        {
            Juego juego = sorteo.Juego;
            int cantidad = juego.CantidadSorteados;
            int columnas = juego.UsaRango ? 2 : Math.Min(5, cantidad);
            int filas = (cantidad + columnas - 1) / columnas;
            int cifras = juego.NumeroMax.ToString().Length;

            tlpNumeros.SuspendLayout();
            tlpNumeros.ColumnCount = columnas;
            tlpNumeros.RowCount = filas;
            for (int c = 0; c < columnas; c++)
                tlpNumeros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columnas));
            for (int f = 0; f < filas; f++)
                tlpNumeros.RowStyles.Add(new RowStyle(SizeType.Absolute, AltoFila));

            for (int i = 0; i < cantidad; i++)
            {
                int columna = juego.UsaRango ? i / filas : i % columnas;
                int fila = juego.UsaRango ? i % filas : i / columnas;
                tlpNumeros.Controls.Add(CrearCasillero(i + 1, cifras), columna, fila);
            }
            tlpNumeros.ResumeLayout();

            ClientSize = new Size(columnas == 2 ? 600 : 660, 84 + 56 + filas * AltoFila + 16 + 68);
        }

        private Control CrearCasillero(int posicion, int cifras)
        {
            TableLayoutPanel celda = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(6, 3, 6, 3) };
            celda.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
            celda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            celda.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label lblPosicion = new Label
            {
                Text = posicion + "°",
                Dock = DockStyle.Fill,
                Font = Estilos.Negrita(10.5f),
                ForeColor = posicion == 1 && sorteo.Juego.UsaRango ? Estilos.Turquesa : Estilos.TextoSuave,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 6, 0)
            };
            TextBox casillero = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = Estilos.Mono(15f, true),
                TextAlign = HorizontalAlignment.Center,
                MaxLength = cifras,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0)
            };
            casillero.KeyPress += Casillero_KeyPress;
            casillero.KeyDown += Casillero_KeyDown;
            casilleros.Add(casillero);

            celda.Controls.Add(lblPosicion, 0, 0);
            celda.Controls.Add(casillero, 1, 0);
            return celda;
        }

        // Solo numeros.
        private void Casillero_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        // Enter pasa al puesto siguiente.
        private void Casillero_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            int i = casilleros.IndexOf((TextBox)sender);
            if (i < casilleros.Count - 1)
                casilleros[i + 1].Focus();
            else
                btnGuardar.Focus();
        }

        private List<string> Textos()
        {
            return casilleros.Select(c => c.Text.Trim()).ToList();
        }

        // Muestra en pantalla como quedaria el resultado completo, sin guardarlo.
        private void btnAzar_Click(object sender, EventArgs e)
        {
            try
            {
                List<NumeroGanador> completo = bll.CompletarResultado(sorteo.Juego, Textos());
                for (int i = 0; i < casilleros.Count; i++)
                    casilleros[i].Text = completo[i].Numero;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cargar resultado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            foreach (TextBox c in casilleros)
                c.Clear();
            casilleros[0].Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            List<string> textos = Textos();
            int vacios = textos.Count(string.IsNullOrEmpty);
            string aviso = vacios == textos.Count
                ? "No cargaste ningún número: todos los puestos se completan al azar."
                : vacios > 0
                    ? (vacios == 1 ? "El puesto vacío se completa" : "Los " + vacios + " puestos vacíos se completan") + " al azar."
                    : "Se guardan los números cargados.";
            if (MessageBox.Show(aviso + "\n\nUna vez guardado, el sorteo queda como sorteado y no se puede modificar. ¿Guardar el resultado?",
                    "Cargar resultado", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                bll.CargarResultado(sorteo, textos, SesionManager.Instancia.UsuarioActual);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cargar resultado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
