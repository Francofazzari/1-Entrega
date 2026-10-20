using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entrega1software
{
    public partial class FormPrincipal : Form , IObservadorIdioma
    {

        private UsuarioBLL bll = new UsuarioBLL();
        private bool isInitializingIdioma = false;

        public FormPrincipal()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            foreach (MdiClient area in Controls.OfType<MdiClient>())
                area.BackColor = Estilos.Fondo;

            lblUsuario.Text = "Usuario: " +
                SesionManager.Instancia.UsuarioActual.Nombre + " " +
                SesionManager.Instancia.UsuarioActual.Apellido;

            ConfigurarSegunPerfil();
            
            isInitializingIdioma = true;
            List<Idioma> idiomas = IdiomaManager.Instancia.ObtenerIdiomas();
            cmbIdiomas.DataSource = idiomas;
            cmbIdiomas.DisplayMember = "Nombre";
            // Preseleccionar el idioma actual
            if(IdiomaManager.Instancia.IdiomaActual != null) {
                foreach(Idioma i in cmbIdiomas.Items) {
                    if (i.Id == IdiomaManager.Instancia.IdiomaActual.Id) {
                        cmbIdiomas.SelectedItem = i;
                        break;
                    }
                }
            }
            isInitializingIdioma = false;

            IdiomaManager.Instancia.Suscribir(this);
            CargarJuegos();
        }

        // Un boton por cada juego activo de la tabla JUEGOS: si se agrega un juego nuevo, aparece
        // solo en el menu. Si la base todavia no tiene el modelo nuevo, se avisa en el menu.
        private void CargarJuegos()
        {
            try
            {
                foreach (Juego juego in new JuegoBLL().ObtenerActivos())
                {
                    Button b = new Button();
                    EstilizarBotonMenu(b, juego.Nombre, Estilos.ColorJuego(juego.Codigo));
                    b.Tag = juego;
                    b.Click += BtnJuego_Click;
                    flpJuegos.Controls.Add(b);
                }
            }
            catch (Exception)
            {
                flpJuegos.Controls.Add(new Label
                {
                    Text = "No se pudieron cargar los juegos. Actualizá la base con SQLQuery8_TodosLosJuegos.sql.",
                    ForeColor = Color.FromArgb(255, 205, 120),
                    Font = Estilos.Fuente(9F),
                    Size = new Size(198, 64),
                    Margin = new Padding(0, 0, 0, 8)
                });
            }
        }

        private void BtnJuego_Click(object sender, EventArgs e)
        {
            Juego juego = (Juego)((Button)sender).Tag;
            if (juego.UsaLoteriaTurno)
                AbrirHijo(() => new FormSeleccionLoteria(juego));
            else
                AbrirHijo(() => new FormJuego(juego));
        }

        // Abre una pantalla dentro del area MDI. Si no se puede crear (por ejemplo, por un error de
        // base de datos), se avisa en lugar de cerrar el sistema.
        private void AbrirHijo(Func<Form> crear)
        {
            Form hija;
            try
            {
                hija = crear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la pantalla: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Estilos.AbrirEnMdi(hija, this);
        }

        // ---- ayuda en linea ----

        private void btnAyuda_Click(object sender, EventArgs e)
        {
            AbrirAyuda();
        }

        // F1 abre la ayuda desde cualquier pantalla abierta dentro del sistema: las teclas que una
        // ventana hija no usa suben hasta el formulario principal.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                AbrirAyuda();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Abre el manual de usuario (PDF que se instala junto al programa, en la carpeta Ayuda)
        // con el lector de PDF predeterminado de Windows.
        private void AbrirAyuda()
        {
            string manual = Path.Combine(Application.StartupPath, "Ayuda", "ManualDeUsuario.pdf");
            if (!File.Exists(manual))
            {
                MessageBox.Show("No se encontró el manual de usuario en:\n" + manual, "Ayuda en línea",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(manual) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el manual de usuario: " + ex.Message +
                    "\nVerificá que haya un lector de PDF instalado.", "Ayuda en línea",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPagar_Click(object sender, EventArgs e)
        {
            AbrirHijo(() => new FormOperacionTicket(ModoOperacion.Pagar));
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            AbrirHijo(() => new FormOperacionTicket(ModoOperacion.Cancelar));
        }

        private void btnReimprimir_Click(object sender, EventArgs e)
        {
            AbrirHijo(() => new FormOperacionTicket(ModoOperacion.Reimprimir));
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            AbrirHijo(() => new FormReporte());
        }

        // ---- estilos de la barra superior y del menu (se usan desde InitializeComponent) ----

        private static void EstilizarBotonBarra(Button b, string texto)
        {
            b.Text = texto;
            b.AutoSize = true;
            b.Size = new Size(96, 34);
            b.Padding = new Padding(8, 0, 8, 0);
            b.Margin = new Padding(0, 6, 6, 0);
            b.Font = Estilos.Fuente(9.5F);
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            Estilos.ColorearBoton(b, Estilos.Aclarar(Estilos.Azul, 0.14f));
        }

        private static void EstilizarTituloMenu(Label l, string texto, int margenArriba)
        {
            l.Text = texto;
            l.AutoSize = true;
            l.Font = Estilos.Negrita(8.5F);
            l.ForeColor = Color.FromArgb(138, 168, 198);
            l.Margin = new Padding(2, margenArriba, 0, 8);
        }

        private static void EstilizarBotonMenu(Button b, string texto, Color color)
        {
            b.Text = texto;
            b.Size = new Size(198, 40);
            b.Margin = new Padding(0, 0, 0, 7);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(12, 0, 0, 0);
            b.Font = Estilos.Negrita(10.5F);
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            Estilos.ColorearBoton(b, color);
        }

        private void ConfigurarSegunPerfil()
        {
            bool isAdmin = false;
            foreach (var p in SesionManager.Instancia.UsuarioActual.Permisos)
            {
                if (p.Codigo == "Admin") isAdmin = true;
            }

            if (isAdmin)
            {
                btnUsuarios.Visible = true;
                btnPerfiles.Visible = true;
                btnVentas.Visible = true;
                btnReporte.Visible = true;
                btnApuesta.Visible = true;
                btnCambios.Visible = true;
                btnIdiomas.Visible = true;
            }
            else // Operador
            {
                btnUsuarios.Visible = false;
                btnPerfiles.Visible = false;
                btnVentas.Visible = false;
                btnReporte.Visible = false;
                btnApuesta.Visible = true;
                btnCambios.Visible = false;
                btnIdiomas.Visible = false;
            }
        }
        public void ActualizarIdioma(Idioma idioma)
        {
            btnUsuarios.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_usuarios");
            btnPerfiles.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_perfiles");
            btnVentas.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_ventas");
            btnReporte.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_reporte");
            btnApuesta.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_apuesta");
            btnLogout.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_logout");
            btnIdiomas.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_idiomas") == "btn_idiomas" ? "Gestion Idiomas" : IdiomaManager.Instancia.ObtenerMensaje("btn_idiomas");
            btnAyuda.Text = IdiomaManager.Instancia.ObtenerMensaje("btn_ayuda") == "btn_ayuda" ? "Ayuda (F1)" : IdiomaManager.Instancia.ObtenerMensaje("btn_ayuda");

            if (!isInitializingIdioma && idioma != null)
            {
                isInitializingIdioma = true;
                foreach(Idioma i in cmbIdiomas.Items) {
                    if (i.Id == idioma.Id) {
                        cmbIdiomas.SelectedItem = i;
                        break;
                    }
                }
                isInitializingIdioma = false;
            }
        }

        private void cmbIdiomas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitializingIdioma && cmbIdiomas.SelectedItem != null)
            {
                Idioma seleccionado = (Idioma)cmbIdiomas.SelectedItem;
                IdiomaManager.Instancia.CambiarIdioma(seleccionado); 
            }
        }

        private void btnUsuarios_Click(object sender, System.EventArgs e)
        {
            FormUsuarios fu = new FormUsuarios();
            fu.MdiParent = this;
            fu.Show();
        }

        private void btnPerfiles_Click(object sender, System.EventArgs e)
        {
            FormPerfiles fp = new FormPerfiles();
            fp.MdiParent = this;
            fp.Show();
        }

        private void btnVentas_Click(object sender, System.EventArgs e)
        {
            AbrirHijo(() => new FormReporte(VistaReporte.Ventas, DateTime.Today));
        }

        // Reporte semanal: las ventas de los ultimos 7 dias.
        private void btnReporte_Click(object sender, System.EventArgs e)
        {
            AbrirHijo(() => new FormReporte(VistaReporte.Ventas, DateTime.Today.AddDays(-6)));
        }

        private void btnApuesta_Click(object sender, System.EventArgs e)
        {
            AbrirHijo(() => new FormSeleccionLoteria(new JuegoBLL().ObtenerQuiniela()));
        }

        private void btnIdiomas_Click(object sender, System.EventArgs e)
        {
            FormControlIdiomas fci = new FormControlIdiomas();
            fci.MdiParent = this;
            fci.Show();
        }

        private void btnLogout_Click(object sender, System.EventArgs e)
        {
            BitacoraBLL bitacora = new BitacoraBLL();
            bitacora.Registrar(
                SesionManager.Instancia.UsuarioActual.Id,
                SesionManager.Instancia.UsuarioActual.Nombre,
                "Logout",
                "Cierre de sesion"
            );
            bll.Logout();
            FormLogin fl = new FormLogin();
            fl.Show();
            this.Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            IdiomaManager.Instancia.Desuscribir(this);
            base.OnFormClosing(e);
        }
        private void btnBitacora_Click(object sender, System.EventArgs e)
        {
            FormBitacora fb = new FormBitacora();
            fb.MdiParent = this;
            fb.Show();
        }
        private void btnCambios_Click(object sender, System.EventArgs e)
        {
            FormControlCambios fcc = new FormControlCambios();
            fcc.MdiParent = this;
            fcc.Show();
        }
    }
}
