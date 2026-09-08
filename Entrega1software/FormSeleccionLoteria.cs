using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormSeleccionLoteria : Form
    {
        private static readonly Color ColorNormal = Color.FromArgb(240, 244, 248);
        private static readonly Color ColorSeleccionada = Color.FromArgb(29, 158, 117);

        private LoteriaBLL loteriaBll = new LoteriaBLL();
        private TurnoBLL turnoBll = new TurnoBLL();

        private List<Loteria> todasLasLoterias = new List<Loteria>();
        private HashSet<int> idsLoteriasSeleccionadas = new HashSet<int>();
        private Button btnTodasLoterias;

        private List<Turno> todosLosTurnos = new List<Turno>();
        private HashSet<int> idsTurnosSeleccionados = new HashSet<int>();
        private Button btnTodosTurnos;

        public FormSeleccionLoteria()
        {
            InitializeComponent();
            CargarLoterias();
            CargarTurnos();
        }

        private void CargarLoterias()
        {
            todasLasLoterias = loteriaBll.ObtenerActivas();

            foreach (Loteria l in todasLasLoterias)
            {
                Button btn = CrearBotonToggle(l.Nombre);
                btn.Tag = l.Id;
                btn.Click += BtnLoteria_Click;
                pnlLoterias.Controls.Add(btn);
            }

            btnTodasLoterias = CrearBotonToggle("Todos");
            btnTodasLoterias.Click += BtnTodasLoterias_Click;
            pnlLoterias.Controls.Add(btnTodasLoterias);
        }

        private void CargarTurnos()
        {
            todosLosTurnos = turnoBll.ObtenerActivos();

            foreach (Turno t in todosLosTurnos)
            {
                Button btn = CrearBotonToggle(t.Nombre);
                btn.Tag = t.Id;
                btn.Click += BtnTurno_Click;
                pnlTurnos.Controls.Add(btn);
            }

            btnTodosTurnos = CrearBotonToggle("Todos");
            btnTodosTurnos.Click += BtnTodosTurnos_Click;
            pnlTurnos.Controls.Add(btnTodosTurnos);
        }

        private Button CrearBotonToggle(string texto)
        {
            return new Button
            {
                Text = texto,
                Size = new Size(150, 55),
                Margin = new Padding(5),
                BackColor = ColorNormal,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10F, FontStyle.Bold)
            };
        }

        private void BtnLoteria_Click(object sender, EventArgs e)
        {
            ToggleSeleccion((Button)sender, idsLoteriasSeleccionadas);
            ActualizarBotonTodos(btnTodasLoterias, idsLoteriasSeleccionadas, todasLasLoterias.Count);
        }

        private void BtnTodasLoterias_Click(object sender, EventArgs e)
        {
            ToggleTodos(pnlLoterias, btnTodasLoterias, idsLoteriasSeleccionadas, todasLasLoterias.Count);
        }

        private void BtnTurno_Click(object sender, EventArgs e)
        {
            ToggleSeleccion((Button)sender, idsTurnosSeleccionados);
            ActualizarBotonTodos(btnTodosTurnos, idsTurnosSeleccionados, todosLosTurnos.Count);
        }

        private void BtnTodosTurnos_Click(object sender, EventArgs e)
        {
            ToggleTodos(pnlTurnos, btnTodosTurnos, idsTurnosSeleccionados, todosLosTurnos.Count);
        }

        private void ToggleSeleccion(Button btn, HashSet<int> idsSeleccionados)
        {
            int id = (int)btn.Tag;
            if (idsSeleccionados.Contains(id))
            {
                idsSeleccionados.Remove(id);
                btn.BackColor = ColorNormal;
            }
            else
            {
                idsSeleccionados.Add(id);
                btn.BackColor = ColorSeleccionada;
            }
        }

        private void ToggleTodos(FlowLayoutPanel panel, Button btnTodos, HashSet<int> idsSeleccionados, int totalDisponibles)
        {
            bool activarTodos = idsSeleccionados.Count < totalDisponibles;

            idsSeleccionados.Clear();
            foreach (Control c in panel.Controls)
            {
                if (c is Button btn && btn != btnTodos)
                {
                    if (activarTodos)
                    {
                        idsSeleccionados.Add((int)btn.Tag);
                        btn.BackColor = ColorSeleccionada;
                    }
                    else
                    {
                        btn.BackColor = ColorNormal;
                    }
                }
            }

            ActualizarBotonTodos(btnTodos, idsSeleccionados, totalDisponibles);
        }

        private void ActualizarBotonTodos(Button btnTodos, HashSet<int> idsSeleccionados, int totalDisponibles)
        {
            btnTodos.BackColor = idsSeleccionados.Count == totalDisponibles && totalDisponibles > 0
                ? ColorSeleccionada
                : ColorNormal;
        }

        private void btnContinuar_Click(object sender, EventArgs e)
        {
            if (idsLoteriasSeleccionadas.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una loteria.", "Atencion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (idsTurnosSeleccionados.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un horario.", "Atencion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Loteria> loteriasElegidas = todasLasLoterias.Where(l => idsLoteriasSeleccionadas.Contains(l.Id)).ToList();
            List<Turno> turnosElegidos = todosLosTurnos.Where(t => idsTurnosSeleccionados.Contains(t.Id)).ToList();

            FormApuesta fa = new FormApuesta(loteriasElegidas, turnosElegidos);
            fa.MdiParent = this.MdiParent;
            fa.Show();
            this.Close();
        }
    }
}
