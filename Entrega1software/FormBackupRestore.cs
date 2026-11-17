using System;
using System.IO;
using System.Windows.Forms;
using BLL;

namespace Entrega1software
{
    // Backup (.bak) y restore de la base de datos. Solo lo usa el Administrador: desde el boton
    // "Backup" del menu principal, o desde el aviso de error de integridad al iniciar sesion.
    public partial class FormBackupRestore : Form
    {
        private BackupBLL bll = new BackupBLL();
        private string carpetaPorDefecto = "";

        // true si en esta ventana se restauro la base (lo consulta el login).
        public bool SeRestauro { get; private set; }

        public FormBackupRestore()
        {
            InitializeComponent();

            lblSubtitulo.Text = "Base: " + bll.NombreBase + "     Servidor: " + bll.Servidor;

            try
            {
                carpetaPorDefecto = bll.CarpetaPorDefecto();
            }
            catch (Exception)
            {
                // si no se pudo crear la carpeta, el usuario elige otra con "Examinar..."
            }

            txtRutaBackup.Text = Path.Combine(carpetaPorDefecto, bll.NombreArchivoSugerido());
            ProponerUltimoBackup();
            lblNota.Text = "El archivo .bak lo escribe y lo lee el servicio de SQL Server, no el programa: " +
                           "tiene que estar en una carpeta a la que el servicio tenga acceso. Se recomienda usar " +
                           "la carpeta por defecto: " + carpetaPorDefecto;
        }

        // ---- backup ----

        private void btnExaminarBackup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Guardar backup";
                dlg.Filter = "Backup de SQL Server (*.bak)|*.bak";
                dlg.DefaultExt = "bak";
                dlg.AddExtension = true;
                dlg.InitialDirectory = CarpetaDe(txtRutaBackup.Text);
                dlg.FileName = bll.NombreArchivoSugerido();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    txtRutaBackup.Text = dlg.FileName;
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            string ruta = txtRutaBackup.Text.Trim();
            if (!Ejecutar(() => bll.RealizarBackup(ruta))) return;

            MessageBox.Show("Backup realizado correctamente en:\n" + ruta, "Backup",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // El proximo backup se propone con un nombre nuevo, para no pisar el que se acaba de hacer,
            // y el que se acaba de hacer queda propuesto para restaurar.
            txtRutaBackup.Text = Path.Combine(CarpetaDe(ruta), bll.NombreArchivoSugerido());
            txtRutaRestore.Text = ruta;
        }

        // Propone para restaurar el backup mas reciente de la carpeta por defecto (si hay alguno).
        private void ProponerUltimoBackup()
        {
            try
            {
                txtRutaRestore.Text = bll.UltimoBackup() ?? "";
            }
            catch (Exception)
            {
                txtRutaRestore.Text = "";
            }
        }

        // ---- restore ----

        private void btnExaminarRestore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Elegir backup a restaurar";
                dlg.Filter = "Backup de SQL Server (*.bak)|*.bak";
                dlg.InitialDirectory = CarpetaDe(txtRutaRestore.Text);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    txtRutaRestore.Text = dlg.FileName;
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            string ruta = txtRutaRestore.Text.Trim();
            if (ruta.Length == 0)
            {
                MessageBox.Show("Elegí el archivo .bak a restaurar.", "Restore",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirma = MessageBox.Show(
                "Se va a reemplazar la base de datos \"" + bll.NombreBase + "\" por el contenido de:\n\n" + ruta +
                "\n\nTodo lo registrado después de ese backup se pierde y la sesión se cerrará.\n¿Desea continuar?",
                "Confirmar restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirma != DialogResult.Yes) return;

            if (!Ejecutar(() => bll.RealizarRestore(ruta))) return;
            SeRestauro = true;

            MessageBox.Show("La base de datos se restauró correctamente.\n" +
                            "Volvé a iniciar sesión para trabajar con los datos restaurados.",
                "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Los usuarios, perfiles y datos pueden haber cambiado: se vuelve al login (que ademas
            // verifica la integridad de la base restaurada).
            FormPrincipal principal = this.MdiParent as FormPrincipal;
            if (principal != null)
            {
                principal.CerrarSesion();
            }
            else
            {
                // abierta desde el login, como ventana modal
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // ---- comun ----

        // Ejecuta la operacion con el cursor de espera y los botones deshabilitados. Si falla,
        // muestra el error y devuelve false.
        private bool Ejecutar(Action operacion)
        {
            HabilitarBotones(false);
            this.Cursor = Cursors.WaitCursor;
            try
            {
                operacion();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                HabilitarBotones(true);
            }
        }

        private void HabilitarBotones(bool habilitar)
        {
            btnBackup.Enabled = habilitar;
            btnRestaurar.Enabled = habilitar;
            btnExaminarBackup.Enabled = habilitar;
            btnExaminarRestore.Enabled = habilitar;
        }

        // Carpeta del archivo indicado, si existe; si no, la carpeta de backups por defecto.
        private string CarpetaDe(string ruta)
        {
            try
            {
                string carpeta = Path.GetDirectoryName(ruta.Trim());
                if (!string.IsNullOrEmpty(carpeta) && Directory.Exists(carpeta)) return carpeta;
            }
            catch (Exception)
            {
                // ruta vacia o mal escrita: se usa la carpeta por defecto
            }
            return carpetaPorDefecto;
        }
    }
}
