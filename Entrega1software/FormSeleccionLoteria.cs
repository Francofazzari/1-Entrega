using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Paso 1 de la Quiniela: elegir las loterias y los horarios. Cada combinacion loteria x
    // horario es un sorteo distinto, y la jugada se va a repetir en cada uno.
    public partial class FormSeleccionLoteria : Form
    {
        private readonly Juego juego;
        private readonly Color colorJuego;
        private readonly LoteriaBLL loteriaBll = new LoteriaBLL();
        private readonly TurnoBLL turnoBll = new TurnoBLL();

        private List<Loteria> todasLasLoterias = new List<Loteria>();
        private readonly HashSet<int> idsLoteriasSeleccionadas = new HashSet<int>();
        private Button btnTodasLoterias;

        private List<Turno> todosLosTurnos = new List<Turno>();
        private readonly HashSet<int> idsTurnosSeleccionados = new HashSet<int>();
        private Button btnTodosTurnos;

        public FormSeleccionLoteria(Juego juego)
        {
            InitializeComponent();
            this.juego = juego;
            colorJuego = Estilos.ColorJuego(juego.Codigo);
            pnlEncabezado.BackColor = colorJuego;
            lblTitulo.Text = juego.Nombre;
            lblSubtitulo.Text = "Elegí las loterías y los horarios en los que se va a jugar";

            CargarLoterias();
            CargarTurnos();
            ActualizarResumen();
        }

        private void CargarLoterias()
        {
            todasLasLoterias = loteriaBll.ObtenerActivas();
            foreach (Loteria l in todasLasLoterias)
            {
                Button btn = CrearBotonToggle(l.Nombre, 150);
                btn.Tag = l.Id;
                btn.Click += BtnLoteria_Click;
                pnlLoterias.Controls.Add(btn);
            }

            btnTodasLoterias = CrearBotonToggle("Todas", 150);
            btnTodasLoterias.Click += BtnTodasLoterias_Click;
            pnlLoterias.Controls.Add(btnTodasLoterias);
        }

        private void CargarTurnos()
        {
            todosLosTurnos = turnoBll.ObtenerActivos();
            foreach (Turno t in todosLosTurnos)
            {
                Button btn = CrearBotonToggle(t.Nombre, 122);
                btn.Tag = t.Id;
                btn.Click += BtnTurno_Click;
                pnlTurnos.Controls.Add(btn);
            }

            btnTodosTurnos = CrearBotonToggle("Todos", 122);
            btnTodosTurnos.Click += BtnTodosTurnos_Click;
            pnlTurnos.Controls.Add(btnTodosTurnos);
        }

        private static Button CrearBotonToggle(string texto, int ancho)
        {
            Button b = Estilos.Toggle(texto, 10.5f);
            b.Size = new Size(ancho, 52);
            b.Margin = new Padding(6);
            return b;
        }

        private void BtnLoteria_Click(object sender, EventArgs e)
        {
            ToggleSeleccion((Button)sender, idsLoteriasSeleccionadas);
            ActualizarBotonTodos(btnTodasLoterias, idsLoteriasSeleccionadas, todasLasLoterias.Count);
            ActualizarResumen();
        }

        private void BtnTodasLoterias_Click(object sender, EventArgs e)
        {
            ToggleTodos(pnlLoterias, btnTodasLoterias, idsLoteriasSeleccionadas, todasLasLoterias.Count);
            ActualizarResumen();
        }

        private void BtnTurno_Click(object sender, EventArgs e)
        {
            ToggleSeleccion((Button)sender, idsTurnosSeleccionados);
            ActualizarBotonTodos(btnTodosTurnos, idsTurnosSeleccionados, todosLosTurnos.Count);
            ActualizarResumen();
        }

        private void BtnTodosTurnos_Click(object sender, EventArgs e)
        {
            ToggleTodos(pnlTurnos, btnTodosTurnos, idsTurnosSeleccionados, todosLosTurnos.Count);
            ActualizarResumen();
        }

        private void ToggleSeleccion(Button btn, HashSet<int> idsSeleccionados)
        {
            int id = (int)btn.Tag;
            bool activar = !idsSeleccionados.Contains(id);
            if (activar) idsSeleccionados.Add(id); else idsSeleccionados.Remove(id);
            Estilos.MarcarToggle(btn, activar, colorJuego);
        }

        private void ToggleTodos(FlowLayoutPanel panel, Button btnTodos, HashSet<int> idsSeleccionados, int totalDisponibles)
        {
            bool activarTodos = idsSeleccionados.Count < totalDisponibles;

            idsSeleccionados.Clear();
            foreach (Control c in panel.Controls)
            {
                if (c is Button btn && btn != btnTodos)
                {
                    if (activarTodos) idsSeleccionados.Add((int)btn.Tag);
                    Estilos.MarcarToggle(btn, activarTodos, colorJuego);
                }
            }
            ActualizarBotonTodos(btnTodos, idsSeleccionados, totalDisponibles);
        }

        private void ActualizarBotonTodos(Button btnTodos, HashSet<int> idsSeleccionados, int totalDisponibles)
        {
            bool todos = totalDisponibles > 0 && idsSeleccionados.Count == totalDisponibles;
            Estilos.MarcarToggle(btnTodos, todos, Estilos.Oscurecer(colorJuego, 0.25f));
        }

        private void ActualizarResumen()
        {
            int loterias = idsLoteriasSeleccionadas.Count, turnos = idsTurnosSeleccionados.Count;
            if (loterias == 0 || turnos == 0)
            {
                lblResumen.ForeColor = Estilos.TextoSuave;
                lblResumen.Text = "Elegí al menos una lotería y un horario.";
                return;
            }

            int sorteos = loterias * turnos;
            lblResumen.ForeColor = Estilos.Texto;
            lblResumen.Text = "Vas a jugar en " + sorteos + (sorteos == 1 ? " sorteo" : " sorteos") + ":  " +
                              loterias + (loterias == 1 ? " lotería" : " loterías") + " × " +
                              turnos + (turnos == 1 ? " horario" : " horarios") + ".";
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {
            if (idsLoteriasSeleccionadas.Count == 0 || idsTurnosSeleccionados.Count == 0)
            {
                MessageBox.Show("Elegí al menos una lotería y un horario.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Loteria> loteriasElegidas = todasLasLoterias.Where(l => idsLoteriasSeleccionadas.Contains(l.Id)).ToList();
            List<Turno> turnosElegidos = todosLosTurnos.Where(t => idsTurnosSeleccionados.Contains(t.Id)).ToList();

            Estilos.AbrirEnMdi(new FormApuesta(juego, loteriasElegidas, turnosElegidos), MdiParent);
            Close();
        }
    }
}
