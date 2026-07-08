using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

namespace Entrega1software
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<string> erroresIntegridad;
            try
            {
                erroresIntegridad = IdiomaManager.Instancia.VerificarIntegridad();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo verificar la integridad de la base de datos:\n" + ex.Message,
                    "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (erroresIntegridad.Count > 0)
            {
                MessageBox.Show(
                    "Se detectaron problemas de integridad en la base de datos:\n\n" +
                    string.Join("\n", erroresIntegridad) +
                    "\n\nContacte al administrador del sistema. La aplicación no puede continuar.",
                    "Error de integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FormLogin());
        }
    }
}
