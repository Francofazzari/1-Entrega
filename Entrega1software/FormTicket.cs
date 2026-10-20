using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    // Desde donde se abre el ticket: recien vendido, reimpreso o consultado desde un reporte.
    public enum VistaTicket { Venta, Reimpresion, Consulta }

    public partial class FormTicket : Form
    {
        private const int MargenPapel = 18;
        private const int MargenMesa = 18;

        private readonly Apuesta apuesta;
        private readonly bool esReimpresion;

        public FormTicket(Apuesta apuesta, VistaTicket vista)
        {
            InitializeComponent();
            this.apuesta = apuesta;
            esReimpresion = vista == VistaTicket.Reimpresion;

            string juego = apuesta.Juego != null ? apuesta.Juego.Nombre : "Ticket";
            string resumen = juego + "  ·  " + apuesta.Jugadas.Count + (apuesta.Jugadas.Count == 1 ? " jugada" : " jugadas") +
                             "  ·  Total " + Estilos.Plata(apuesta.Total);
            switch (vista)
            {
                case VistaTicket.Venta:
                    lblTitulo.Text = "Venta registrada";
                    break;
                case VistaTicket.Reimpresion:
                    pnlEncabezado.BackColor = Estilos.AzulMedio;
                    lblTitulo.Text = "Reimpresión del ticket";
                    break;
                default:
                    pnlEncabezado.BackColor = Estilos.Azul;
                    lblTitulo.Text = "Ticket " + ApuestaBLL.CodigoCorto(apuesta);
                    resumen += "  ·  " + Estilos.TextoEstadoTicket(apuesta.Estado);
                    btnImprimir.Visible = false;
                    break;
            }
            lblSubtitulo.Text = resumen;
            lblSubtitulo.ForeColor = Estilos.Aclarar(pnlEncabezado.BackColor, 0.82f);
            Text = "Ticket " + ApuestaBLL.CodigoCorto(apuesta);

            // Se mide el ticket una vez para darle al "papel" el alto justo.
            int alto;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
                alto = TicketRenderer.Dibujar(g, apuesta, esReimpresion, 0, 0);
            pnlPapel.Size = new Size(TicketRenderer.Ancho + 2 * MargenPapel, alto + 2 * MargenPapel);
            pnlMesa.AutoScrollMinSize = new Size(pnlPapel.Width + 2 * MargenMesa, pnlPapel.Height + 2 * MargenMesa);
            UbicarPapel();
        }

        private void UbicarPapel()
        {
            int x = Math.Max(MargenMesa, (pnlMesa.ClientSize.Width - pnlPapel.Width) / 2);
            pnlPapel.Location = new Point(x + pnlMesa.AutoScrollPosition.X, MargenMesa + pnlMesa.AutoScrollPosition.Y);
            pnlMesa.Invalidate();
        }

        private void pnlMesa_Resize(object sender, EventArgs e)
        {
            UbicarPapel();
        }

        // Sombra suave debajo del papel.
        private void pnlMesa_Paint(object sender, PaintEventArgs e)
        {
            Rectangle sombra = new Rectangle(pnlPapel.Left + 4, pnlPapel.Top + 5, pnlPapel.Width, pnlPapel.Height);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(205, 212, 222)))
                e.Graphics.FillRectangle(b, sombra);
        }

        private void pnlPapel_Paint(object sender, PaintEventArgs e)
        {
            TicketRenderer.Dibujar(e.Graphics, apuesta, esReimpresion, MargenPapel, MargenPapel);
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ApuestaBLL.CodigoCompleto(apuesta));
            lblCopiado.Text = "Código copiado";
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            using (PrintDocument documento = new PrintDocument())
            {
                documento.DocumentName = "Ticket " + ApuestaBLL.CodigoCorto(apuesta);
                documento.PrintPage += (s, ev) =>
                {
                    TicketRenderer.Dibujar(ev.Graphics, apuesta, esReimpresion, ev.MarginBounds.Left, ev.MarginBounds.Top);
                    ev.HasMorePages = false;
                };

                using (PrintPreviewDialog vistaPrevia = new PrintPreviewDialog())
                {
                    vistaPrevia.Document = documento;
                    vistaPrevia.Width = 760;
                    vistaPrevia.Height = 820;
                    vistaPrevia.StartPosition = FormStartPosition.CenterParent;
                    vistaPrevia.Text = "Imprimir ticket " + ApuestaBLL.CodigoCorto(apuesta);
                    try
                    {
                        vistaPrevia.ShowDialog(this);
                    }
                    catch (InvalidPrinterException)
                    {
                        MessageBox.Show("No hay ninguna impresora instalada en esta computadora.", "Imprimir",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }
}
