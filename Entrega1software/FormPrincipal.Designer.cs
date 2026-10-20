namespace Entrega1software
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.tlpTop = new System.Windows.Forms.TableLayoutPanel();
            this.lblApp = new System.Windows.Forms.Label();
            this.flpUsuario = new System.Windows.Forms.FlowLayoutPanel();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.cmbIdiomas = new System.Windows.Forms.ComboBox();
            this.flpAdmin = new System.Windows.Forms.FlowLayoutPanel();
            this.btnUsuarios = new System.Windows.Forms.Button();
            this.btnPerfiles = new System.Windows.Forms.Button();
            this.btnVentas = new System.Windows.Forms.Button();
            this.btnReporte = new System.Windows.Forms.Button();
            this.btnApuesta = new System.Windows.Forms.Button();
            this.btnIdiomas = new System.Windows.Forms.Button();
            this.btnBitacora = new System.Windows.Forms.Button();
            this.btnCambios = new System.Windows.Forms.Button();
            this.flpSesion = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAyuda = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.panelIzquierdo = new System.Windows.Forms.Panel();
            this.flpMenu = new System.Windows.Forms.FlowLayoutPanel();
            this.lblVender = new System.Windows.Forms.Label();
            this.flpJuegos = new System.Windows.Forms.FlowLayoutPanel();
            this.lblOperaciones = new System.Windows.Forms.Label();
            this.btnPagar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnReimprimir = new System.Windows.Forms.Button();
            this.lblConsultas = new System.Windows.Forms.Label();
            this.btnReportes = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.tlpTop.SuspendLayout();
            this.flpUsuario.SuspendLayout();
            this.flpAdmin.SuspendLayout();
            this.flpSesion.SuspendLayout();
            this.panelIzquierdo.SuspendLayout();
            this.flpMenu.SuspendLayout();
            this.SuspendLayout();

            // ================= barra superior =================
            // panelTop: titulo y usuario arriba; abajo los botones de administracion y cerrar sesion
            this.panelTop.BackColor = Estilos.Azul;
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 100;
            this.panelTop.Controls.Add(this.tlpTop);
            this.panelTop.Name = "panelTop";

            this.tlpTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTop.ColumnCount = 2;
            this.tlpTop.RowCount = 2;
            this.tlpTop.Padding = new System.Windows.Forms.Padding(16, 4, 16, 8);
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTop.Controls.Add(this.lblApp, 0, 0);
            this.tlpTop.Controls.Add(this.flpUsuario, 1, 0);
            this.tlpTop.Controls.Add(this.flpAdmin, 0, 1);
            this.tlpTop.Controls.Add(this.flpSesion, 1, 1);
            this.tlpTop.Name = "tlpTop";

            this.lblApp.Text = "AGENCIA DE QUINIELA";
            this.lblApp.Font = Estilos.Negrita(15F);
            this.lblApp.ForeColor = System.Drawing.Color.White;
            this.lblApp.AutoSize = true;
            this.lblApp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblApp.Name = "lblApp";

            this.flpUsuario.AutoSize = true;
            this.flpUsuario.WrapContents = false;
            this.flpUsuario.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpUsuario.Margin = new System.Windows.Forms.Padding(0);
            this.flpUsuario.Controls.Add(this.lblUsuario);
            this.flpUsuario.Controls.Add(this.cmbIdiomas);
            this.flpUsuario.Name = "flpUsuario";

            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Font = Estilos.Fuente(10F);
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Margin = new System.Windows.Forms.Padding(0, 9, 12, 0);
            this.lblUsuario.Name = "lblUsuario";

            this.cmbIdiomas.FormattingEnabled = true;
            this.cmbIdiomas.Font = Estilos.Fuente(9.75F);
            this.cmbIdiomas.Size = new System.Drawing.Size(140, 26);
            this.cmbIdiomas.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.cmbIdiomas.Name = "cmbIdiomas";
            this.cmbIdiomas.TabIndex = 6;
            this.cmbIdiomas.SelectedIndexChanged += new System.EventHandler(this.cmbIdiomas_SelectedIndexChanged);

            // flpAdmin: los botones se acomodan solos; los que el perfil no ve no dejan huecos
            this.flpAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAdmin.WrapContents = false;
            this.flpAdmin.Margin = new System.Windows.Forms.Padding(0);
            this.flpAdmin.Controls.Add(this.btnUsuarios);
            this.flpAdmin.Controls.Add(this.btnPerfiles);
            this.flpAdmin.Controls.Add(this.btnVentas);
            this.flpAdmin.Controls.Add(this.btnReporte);
            this.flpAdmin.Controls.Add(this.btnApuesta);
            this.flpAdmin.Controls.Add(this.btnIdiomas);
            this.flpAdmin.Controls.Add(this.btnBitacora);
            this.flpAdmin.Controls.Add(this.btnCambios);
            this.flpAdmin.Name = "flpAdmin";

            EstilizarBotonBarra(this.btnUsuarios, "Gestion Usuarios");
            this.btnUsuarios.Name = "btnUsuarios";
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);

            EstilizarBotonBarra(this.btnPerfiles, "Perfiles");
            this.btnPerfiles.Name = "btnPerfiles";
            this.btnPerfiles.Click += new System.EventHandler(this.btnPerfiles_Click);

            EstilizarBotonBarra(this.btnVentas, "Ventas en Linea");
            this.btnVentas.Name = "btnVentas";
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);

            EstilizarBotonBarra(this.btnReporte, "Reporte Semanal");
            this.btnReporte.Name = "btnReporte";
            this.btnReporte.Click += new System.EventHandler(this.btnReporte_Click);

            EstilizarBotonBarra(this.btnApuesta, "Cargar Apuesta");
            this.btnApuesta.Name = "btnApuesta";
            this.btnApuesta.Click += new System.EventHandler(this.btnApuesta_Click);

            EstilizarBotonBarra(this.btnIdiomas, "Gestion Idiomas");
            this.btnIdiomas.Name = "btnIdiomas";
            this.btnIdiomas.Click += new System.EventHandler(this.btnIdiomas_Click);

            EstilizarBotonBarra(this.btnBitacora, "Bitacora");
            this.btnBitacora.Name = "btnBitacora";
            this.btnBitacora.Click += new System.EventHandler(this.btnBitacora_Click);

            EstilizarBotonBarra(this.btnCambios, "Control Cambios");
            this.btnCambios.Name = "btnCambios";
            this.btnCambios.Click += new System.EventHandler(this.btnCambios_Click);

            // flpSesion: ayuda en linea y cerrar sesion, a la derecha de la barra
            this.flpSesion.AutoSize = true;
            this.flpSesion.WrapContents = false;
            this.flpSesion.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpSesion.Margin = new System.Windows.Forms.Padding(0);
            this.flpSesion.Controls.Add(this.btnAyuda);
            this.flpSesion.Controls.Add(this.btnLogout);
            this.flpSesion.Name = "flpSesion";

            EstilizarBotonBarra(this.btnAyuda, "Ayuda (F1)");
            Estilos.ColorearBoton(this.btnAyuda, Estilos.Turquesa);
            this.btnAyuda.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);
            this.btnAyuda.Name = "btnAyuda";
            this.btnAyuda.Click += new System.EventHandler(this.btnAyuda_Click);

            EstilizarBotonBarra(this.btnLogout, "Cerrar sesion");
            Estilos.ColorearBoton(this.btnLogout, Estilos.Rojo);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // ================= menu de la agencia (izquierda) =================
            // Comun a todos los juegos: vender, operar sobre un ticket y consultar reportes.
            this.panelIzquierdo.BackColor = Estilos.AzulOscuro;
            this.panelIzquierdo.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelIzquierdo.Width = 230;
            this.panelIzquierdo.Controls.Add(this.flpMenu);
            this.panelIzquierdo.Name = "panelIzquierdo";

            this.flpMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMenu.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpMenu.WrapContents = false;
            this.flpMenu.AutoScroll = true;
            this.flpMenu.Padding = new System.Windows.Forms.Padding(16, 18, 16, 16);
            this.flpMenu.Controls.Add(this.lblVender);
            this.flpMenu.Controls.Add(this.flpJuegos);
            this.flpMenu.Controls.Add(this.lblOperaciones);
            this.flpMenu.Controls.Add(this.btnPagar);
            this.flpMenu.Controls.Add(this.btnCancelar);
            this.flpMenu.Controls.Add(this.btnReimprimir);
            this.flpMenu.Controls.Add(this.lblConsultas);
            this.flpMenu.Controls.Add(this.btnReportes);
            this.flpMenu.Name = "flpMenu";

            EstilizarTituloMenu(this.lblVender, "VENDER", 0);
            this.lblVender.Name = "lblVender";

            // flpJuegos: un boton por cada juego activo (se crean en el constructor)
            this.flpJuegos.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpJuegos.WrapContents = false;
            this.flpJuegos.AutoSize = true;
            this.flpJuegos.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpJuegos.Margin = new System.Windows.Forms.Padding(0);
            this.flpJuegos.Name = "flpJuegos";

            EstilizarTituloMenu(this.lblOperaciones, "OPERACIONES", 14);
            this.lblOperaciones.Name = "lblOperaciones";

            EstilizarBotonMenu(this.btnPagar, "Pagar", Estilos.Verde);
            this.btnPagar.Name = "btnPagar";
            this.btnPagar.Click += new System.EventHandler(this.btnPagar_Click);

            EstilizarBotonMenu(this.btnCancelar, "Cancelar", Estilos.Rojo);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            EstilizarBotonMenu(this.btnReimprimir, "Reimprimir", Estilos.AzulMedio);
            this.btnReimprimir.Name = "btnReimprimir";
            this.btnReimprimir.Click += new System.EventHandler(this.btnReimprimir_Click);

            EstilizarTituloMenu(this.lblConsultas, "CONSULTAS", 14);
            this.lblConsultas.Name = "lblConsultas";

            EstilizarBotonMenu(this.btnReportes, "Reportes", Estilos.Turquesa);
            this.btnReportes.Name = "btnReportes";
            this.btnReportes.Click += new System.EventHandler(this.btnReportes_Click);

            // FormPrincipal
            // Orden de Controls.Add importante para el docking: WinForms acomoda primero el ULTIMO
            // control agregado. panelTop va al final para que se ubique primero y ocupe todo el
            // ancho; panelIzquierdo se acomoda despues, debajo de la barra. El area MDI (donde se
            // abren las ventanas) toma automaticamente el espacio que queda.
            this.ClientSize = new System.Drawing.Size(1400, 760);
            this.Controls.Add(this.panelIzquierdo);
            this.Controls.Add(this.panelTop);
            this.Font = Estilos.Fuente(9.75F);
            this.Text = "Sistema Agencia de Quiniela";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Name = "FormPrincipal";
            this.panelTop.ResumeLayout(false);
            this.tlpTop.ResumeLayout(false);
            this.tlpTop.PerformLayout();
            this.flpUsuario.ResumeLayout(false);
            this.flpUsuario.PerformLayout();
            this.flpAdmin.ResumeLayout(false);
            this.flpSesion.ResumeLayout(false);
            this.flpSesion.PerformLayout();
            this.panelIzquierdo.ResumeLayout(false);
            this.flpMenu.ResumeLayout(false);
            this.flpMenu.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TableLayoutPanel tlpTop;
        private System.Windows.Forms.Label lblApp;
        private System.Windows.Forms.FlowLayoutPanel flpUsuario;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.ComboBox cmbIdiomas;
        private System.Windows.Forms.FlowLayoutPanel flpAdmin;
        private System.Windows.Forms.Button btnUsuarios;
        private System.Windows.Forms.Button btnPerfiles;
        private System.Windows.Forms.Button btnVentas;
        private System.Windows.Forms.Button btnReporte;
        private System.Windows.Forms.Button btnApuesta;
        private System.Windows.Forms.Button btnIdiomas;
        private System.Windows.Forms.Button btnBitacora;
        private System.Windows.Forms.Button btnCambios;
        private System.Windows.Forms.FlowLayoutPanel flpSesion;
        private System.Windows.Forms.Button btnAyuda;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel panelIzquierdo;
        private System.Windows.Forms.FlowLayoutPanel flpMenu;
        private System.Windows.Forms.Label lblVender;
        private System.Windows.Forms.FlowLayoutPanel flpJuegos;
        private System.Windows.Forms.Label lblOperaciones;
        private System.Windows.Forms.Button btnPagar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnReimprimir;
        private System.Windows.Forms.Label lblConsultas;
        private System.Windows.Forms.Button btnReportes;
    }
}
