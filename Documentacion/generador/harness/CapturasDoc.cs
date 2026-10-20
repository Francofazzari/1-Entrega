using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using BE;
using BLL;
using Entrega1software;

// Capturas para los prototipos de interfaz de la documentacion (N02, item n): cada pantalla recortada al area
// de la ventana, mas la posicion de los controles que se numeran en cada prototipo (capturas.json).
// Corre contra la base descartable AgenciaQuiniela_Prueba.
static class CapturasDoc
{
    static string dir;
    static readonly List<string> errores = new List<string>();
    static readonly StringBuilder json = new StringBuilder();

    [STAThread]
    static void Main(string[] args)
    {
        dir = args[0];
        Directory.CreateDirectory(dir);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        SesionManager.Instancia.IniciarSesion(new Usuario
        {
            Id = 1, Nombre = "Juan", Apellido = "Perez", NroTerminal = "t001",
            Permisos = { new PermisoCompleto { Id = 1, Codigo = "Admin", Nombre = "Administrador", EsPadre = true } }
        });
        Usuario u = SesionManager.Instancia.UsuarioActual;
        List<Juego> juegos = new JuegoBLL().ObtenerActivos();
        Func<string, Juego> J = c => juegos.First(j => j.Codigo == c);
        List<Loteria> lots = new LoteriaBLL().ObtenerActivas();
        List<Turno> turnos = new TurnoBLL().ObtenerActivos();
        Func<string, Loteria> L = n => lots.First(l => l.Nombre == n);
        Func<string, Turno> T = n => turnos.First(t => t.Nombre == n);
        ApuestaBLL apuestas = new ApuestaBLL();

        // Ticket de ejemplo: Quiniela Entre Rios Nocturna, 3 jugadas.
        Apuesta ejemplo = apuestas.Vender(J("QUINIELA"), new List<Loteria> { L("Entre Rios") }, new List<Turno> { T("Nocturna") },
            new List<Jugada>
            {
                Directa("232", 10, 400), Directa("15", 1, 100), Redoblona("23", 5, "45", 20, 200)
            }, u);
        ejemplo = apuestas.BuscarPorCodigo(ApuestaBLL.CodigoCompleto(ejemplo));

        // Ticket premiado para Pagar: Quiniela Santa Fe Matutina con resultado controlado.
        Apuesta premiado = apuestas.Vender(J("QUINIELA"), new List<Loteria> { L("Santa Fe") }, new List<Turno> { T("Matutina") },
            new List<Jugada> { Directa("48", 5, 500), Directa("1234", 1, 200) }, u);
        int idSorteo = premiado.Jugadas[0].Sorteo.Id;
        string[] salieron = { "7012", "3348", "0915", "5521", "8890", "1166", "2047", "6633", "4409", "9958",
                              "3170", "2284", "7745", "0391", "5602", "8817", "1439", "6926", "4053", "9571" };
        Sql("UPDATE SORTEOS SET Estado = 'Sorteado' WHERE Id = " + idSorteo + "; " +
            string.Join(" ", salieron.Select((n, i) => "INSERT INTO NUMERO_GANADOR (IdSorteo, Numero, Posicion) VALUES (" + idSorteo + ", '" + n + "', " + (i + 1) + ");")));

        List<Apuesta> ventas = apuestas.ListarVentas(DateTime.Today, DateTime.Today, null);
        Func<string, Apuesta> Primera = codigo => apuestas.BuscarPorCodigo(ApuestaBLL.CodigoCompleto(ventas.First(v => v.Juego.Codigo == codigo)));
        Apuesta pagado = apuestas.BuscarPorCodigo(ApuestaBLL.CodigoCompleto(ventas.First(v => v.Estado == "Pagada")));
        Apuesta qplus = Primera("QPLUS");
        Apuesta telekino = Primera("TELEKINO");
        Apuesta quini6 = Primera("QUINI6");

        json.Append("{");

        Capturar("principal", new FormPrincipal(), 1400, 800, null, f => new Dictionary<string, Rectangle>
        {
            { "usuario", R(f, Nombre(f, "flpUsuario")) },
            { "admin", R(f, Nombre(f, "flpAdmin")) },
            { "ventas", R(f, Nombre(f, "btnVentas")) },
            { "reporte", R(f, Nombre(f, "btnReporte")) },
            { "apuesta", R(f, Nombre(f, "btnApuesta")) },
            { "ayuda", R(f, Nombre(f, "btnAyuda")) },
            { "salir", R(f, Nombre(f, "btnLogout")) },
            { "vender", Union(R(f, Nombre(f, "lblVender")), R(f, Nombre(f, "flpJuegos"))) },
            { "quiniela", R(f, Boton(f, "Quiniela")) },
            { "juegos_numeros", Union(R(f, Boton(f, "Quiniela Plus")), R(f, Boton(f, "Telekino"))) },
            { "pagar", R(f, Nombre(f, "btnPagar")) },
            { "cancelar", R(f, Nombre(f, "btnCancelar")) },
            { "reimprimir", R(f, Nombre(f, "btnReimprimir")) },
            { "reportes", R(f, Nombre(f, "btnReportes")) }
        });

        Capturar("seleccion", new FormSeleccionLoteria(J("QUINIELA")), 740, 560, f =>
        {
            Click(f, "Provincia"); Click(f, "La Ciudad"); Click(f, "Primera"); Click(f, "Nocturna");
        }, f => new Dictionary<string, Rectangle>
        {
            { "loterias", R(f, Nombre(f, "pnlLoterias")) },
            { "todas", R(f, Boton(f, "Todas")) },
            { "turnos", R(f, Nombre(f, "pnlTurnos")) },
            { "todos", R(f, Boton(f, "Todos")) },
            { "resumen", R(f, Nombre(f, "lblResumen")) },
            { "continuar", R(f, Nombre(f, "btnContinuar")) }
        });

        List<Loteria> dosLoterias = new List<Loteria> { L("Provincia"), L("La Ciudad") };
        List<Turno> dosTurnos = new List<Turno> { T("Primera"), T("Nocturna") };
        Capturar("apuesta", new FormApuesta(J("QUINIELA"), dosLoterias, dosTurnos), 1100, 800, f =>
        {
            Monto(f, 400); Click(f, "2"); Click(f, "3"); Click(f, "2"); Click(f, "A los 10"); Click(f, "AGREGAR AL TICKET");
            Monto(f, 100); Click(f, "1"); Click(f, "5"); Click(f, "Todas"); Click(f, "AGREGAR AL TICKET");
            Click(f, "Redoblona"); Monto(f, 200);
            Click(f, "2"); Click(f, "3"); Click(f, "A los 5"); Click(f, "AGREGAR AL TICKET");
            Click(f, "4"); Click(f, "5"); Click(f, "A los 20"); Click(f, "AGREGAR AL TICKET");
            Click(f, "Directa"); Monto(f, 300); Click(f, "4"); Click(f, "7"); Click(f, "A los 5");
        }, f => new Dictionary<string, Rectangle>
        {
            { "encabezado", R(f, Campo(f, "pnlEncabezado")) },
            { "tipo", R(f, Nombre(f, "tlpTipo")) },
            { "numero", Union(R(f, Nombre(f, "lblNumero")), R(f, Nombre(f, "tlpTeclado"))) },
            { "posicion", R(f, Nombre(f, "tlpRango")) },
            { "monto", R(f, Nombre(f, "nudMonto")) },
            { "agregar", R(f, Nombre(f, "btnAgregar")) },
            { "lista", R(f, Nombre(f, "dgvJugadas")) },
            { "total", R(f, Nombre(f, "tlpTotal")) },
            { "quitar", R(f, Nombre(f, "btnQuitar")) },
            { "confirmar", R(f, Nombre(f, "btnEnviar")) }
        });

        Capturar("redoblona", new FormApuesta(J("QUINIELA"), new List<Loteria> { L("Provincia") }, new List<Turno> { T("Nocturna") }), 1100, 800, f =>
        {
            Click(f, "Redoblona"); Monto(f, 200);
            Click(f, "2"); Click(f, "3"); Click(f, "A los 5"); Click(f, "AGREGAR AL TICKET");
            Click(f, "4"); Click(f, "5"); Click(f, "A los 20");
        }, f => new Dictionary<string, Rectangle>
        {
            { "panel", R(f, Nombre(f, "pnlDerecha")) },
            { "tipo", R(f, Nombre(f, "tlpTipo")) },
            { "estado", R(f, Nombre(f, "lblRedoblona")) },
            { "posicion", R(f, Nombre(f, "tlpRango")) }
        });

        Capturar("quini6", new FormJuego(J("QUINI6")), 1100, 720, f =>
        {
            foreach (string n in new[] { "03", "11", "18", "27", "35", "41" }) Click(f, n);
            Click(f, "AGREGAR JUGADA");
            Click(f, "Automática"); Click(f, "AGREGAR JUGADA");
            Click(f, "05"); Click(f, "08"); Click(f, "44");
        }, f => new Dictionary<string, Rectangle>
        {
            { "encabezado", R(f, Campo(f, "pnlEncabezado")) },
            { "tablero", R(f, Nombre(f, "tlpNumeros")) },
            { "contador", R(f, Nombre(f, "lblContador")) },
            { "automatica", R(f, Nombre(f, "btnAutomatica")) },
            { "limpiar", R(f, Nombre(f, "btnLimpiar")) },
            { "agregar", R(f, Nombre(f, "btnAgregar")) },
            { "lista", R(f, Nombre(f, "dgvJugadas")) },
            { "quitar", R(f, Nombre(f, "btnQuitar")) },
            { "confirmar", R(f, Nombre(f, "btnConfirmar")) }
        });

        Capturar("ticket", new FormTicket(ejemplo, VistaTicket.Venta), 520, 860, null, f => new Dictionary<string, Rectangle>
        {
            { "titulo", R(f, Campo(f, "pnlEncabezado")) },
            { "papel", R(f, Nombre(f, "pnlPapel")) },
            { "copiar", R(f, Nombre(f, "btnCopiar")) },
            { "imprimir", R(f, Nombre(f, "btnImprimir")) },
            { "cerrar", R(f, Nombre(f, "btnCerrar")) }
        });

        Capturar("ticket_pagado", new FormTicket(pagado, VistaTicket.Consulta), 520, 760, null, f => new Dictionary<string, Rectangle>
        {
            { "titulo", R(f, Campo(f, "pnlEncabezado")) },
            { "papel", R(f, Nombre(f, "pnlPapel")) },
            { "copiar", R(f, Nombre(f, "btnCopiar")) },
            { "cerrar", R(f, Nombre(f, "btnCerrar")) }
        });

        Capturar("pagar", new FormOperacionTicket(ModoOperacion.Pagar), 1240, 720, f =>
        {
            Nombre<TextBox>(f, "txtCodigo").Text = ApuestaBLL.CodigoCorto(premiado);
            Click(f, "Buscar");
        }, f => ZonasOperacion(f));

        Capturar("cancelar", new FormOperacionTicket(ModoOperacion.Cancelar), 1240, 720, f =>
        {
            Nombre<TextBox>(f, "txtCodigo").Text = ApuestaBLL.CodigoCorto(qplus);
            Click(f, "Buscar");
        }, f => ZonasOperacion(f));

        Capturar("cancelar_no", new FormOperacionTicket(ModoOperacion.Cancelar), 1240, 720, f =>
        {
            Nombre<TextBox>(f, "txtCodigo").Text = ApuestaBLL.CodigoCorto(telekino);
            Click(f, "Buscar");
        }, f => ZonasOperacion(f));

        Capturar("reimprimir", new FormOperacionTicket(ModoOperacion.Reimprimir), 1240, 720, f =>
        {
            Nombre<TextBox>(f, "txtCodigo").Text = ApuestaBLL.CodigoCorto(quini6);
            Click(f, "Buscar");
        }, f => ZonasOperacion(f));

        Capturar("ticket_reimpresion", new FormTicket(quini6, VistaTicket.Reimpresion), 520, 760, null, f => new Dictionary<string, Rectangle>
        {
            { "titulo", R(f, Campo(f, "pnlEncabezado")) },
            { "papel", R(f, Nombre(f, "pnlPapel")) },
            { "copiar", R(f, Nombre(f, "btnCopiar")) },
            { "imprimir", R(f, Nombre(f, "btnImprimir")) },
            { "cerrar", R(f, Nombre(f, "btnCerrar")) }
        });

        Capturar("reporte_ventas", new FormReporte(), 1180, 760, null, f => new Dictionary<string, Rectangle>
        {
            { "pestanias", R(f, Nombre(f, "flpPestanias")) },
            { "filtros", R(f, Visibles<FlowLayoutPanel>(f).First(p => p.Controls.OfType<DateTimePicker>().Any())) },
            { "buscar", R(f, Visibles<Button>(f).First(b => b.Text == "Buscar")) },
            { "tarjetas", R(f, Visibles<TableLayoutPanel>(f).First(t => t.ColumnCount == 4 && t.RowCount == 1 && t.Controls.Count == 4)) },
            { "grilla", R(f, Visibles<DataGridView>(f).First()) }
        });

        Capturar("reporte_pagos", new FormReporte(VistaReporte.Pagos, DateTime.Today), 1180, 760, null, f => new Dictionary<string, Rectangle>
        {
            { "pestanias", R(f, Nombre(f, "flpPestanias")) },
            { "filtros", R(f, Visibles<FlowLayoutPanel>(f).First(p => p.Controls.OfType<DateTimePicker>().Any())) },
            { "buscar", R(f, Visibles<Button>(f).First(b => b.Text == "Buscar")) },
            { "tarjetas", R(f, Visibles<TableLayoutPanel>(f).First(t => t.ColumnCount == 4 && t.RowCount == 1 && t.Controls.Count == 2)) },
            { "grilla", R(f, Visibles<DataGridView>(f).First()) }
        });

        Capturar("reporte_ganadores", new FormReporte(VistaReporte.Ganadores, DateTime.Today), 1180, 760, null, f => new Dictionary<string, Rectangle>
        {
            { "pestanias", R(f, Nombre(f, "flpPestanias")) },
            { "filtros", R(f, Visibles<FlowLayoutPanel>(f).First(p => p.Controls.OfType<ComboBox>().Any())) },
            { "actualizar", R(f, Visibles<Button>(f).First(b => b.Text == "Actualizar")) },
            { "estado", R(f, Visibles<Label>(f).First(l => l.Text.Contains("Sorteado") || l.Text.Contains("Abierto"))) },
            { "cerrar", R(f, Visibles<Button>(f).First(b => b.Text == "Cerrar ventas")) },
            { "cargar", R(f, Visibles<Button>(f).First(b => b.Text == "Cargar resultado")) },
            { "sortear", R(f, Visibles<Button>(f).First(b => b.Text == "Sortear al azar")) },
            { "pizarra", R(f, Visibles<Panel>(f).First(p => p.GetType().Name == "PanelDibujo")) }
        });

        // Carga del resultado: el 32 escrito a mano en el puesto 8 (el resto se completaria al azar).
        Sorteo cordobaNocturna = new SorteoBLL().ObtenerOCrear(J("QUINIELA"), L("Cordoba"), T("Nocturna"), DateTime.Today);
        Capturar("resultado", new FormResultado(cordobaNocturna), 0, 0, f =>
        {
            TextBox puesto8 = Todos(f).OfType<TextBox>().ElementAt(7);
            puesto8.Text = "32";
        }, f => new Dictionary<string, Rectangle>
        {
            { "puestos", R(f, Nombre(f, "tlpNumeros")) },
            { "puesto8", R(f, Todos(f).OfType<TextBox>().ElementAt(7).Parent) },
            { "azar", R(f, Nombre(f, "btnAzar")) },
            { "limpiar", R(f, Nombre(f, "btnLimpiar")) },
            { "cancelar", R(f, Nombre(f, "btnCancelar")) },
            { "guardar", R(f, Nombre(f, "btnGuardar")) }
        });

        // Cartel de felicitaciones al pagar el ticket premiado (se espera a que termine de subir el importe).
        Apuesta premiadoLeido = apuestas.BuscarPorCodigo(ApuestaBLL.CodigoCompleto(premiado));
        List<Premio> premiosPremiado = new PremioBLL().Calcular(premiadoLeido);
        Capturar("felicitaciones", new FormFelicitaciones(premiadoLeido, premiosPremiado.Sum(p => p.Importe), premiosPremiado), 0, 0, f =>
        {
            DateTime hasta = DateTime.Now.AddSeconds(1.6);
            while (DateTime.Now < hasta) { Application.DoEvents(); System.Threading.Thread.Sleep(10); }
        }, f => new Dictionary<string, Rectangle>
        {
            { "aceptar", R(f, Nombre(f, "btnAceptar")) }
        });

        // Pizarra con todas las loterias de un turno (se sortea antes La Ciudad para que haya dos columnas con numeros).
        new SorteoBLL().RealizarSorteo(new SorteoBLL().ObtenerOCrear(J("QUINIELA"), L("La Ciudad"), T("Primera"), DateTime.Today), u);
        Capturar("ganadores_todas", new FormReporte(VistaReporte.Ganadores, DateTime.Today), 1180, 760, f =>
        {
            Visibles<ComboBox>(f).First(c => c.Items.Count > 0 && c.Items[0] is Loteria).SelectedIndex = 0;
        }, f => new Dictionary<string, Rectangle>
        {
            { "filtros", R(f, Visibles<FlowLayoutPanel>(f).First(p => p.Controls.OfType<ComboBox>().Any())) },
            { "estado", R(f, Visibles<Label>(f).First(l => l.Text.Contains("sorteados"))) },
            { "pizarra", R(f, Visibles<Panel>(f).First(p => p.GetType().Name == "PanelDibujo")) }
        });

        Capturar("reporte_quini6", new FormReporte(VistaReporte.Ganadores, DateTime.Today), 1180, 760, f =>
        {
            ComboBox cmb = Visibles<ComboBox>(f).First(c => c.Items.Count == juegos.Count && c.Items[0] is Juego);
            cmb.SelectedIndex = juegos.FindIndex(j => j.Codigo == "QUINI6");
        }, f => new Dictionary<string, Rectangle>
        {
            { "filtros", R(f, Visibles<FlowLayoutPanel>(f).First(p => p.Controls.OfType<ComboBox>().Any())) },
            { "pizarra", R(f, Visibles<Panel>(f).First(p => p.GetType().Name == "PanelDibujo")) }
        });

        json.Append("\n}\n");
        File.WriteAllText(Path.Combine(dir, "capturas.json"), json.ToString(), new UTF8Encoding(false));
        Console.WriteLine(errores.Count == 0 ? "capturas de la documentacion OK" : string.Join(Environment.NewLine, errores));
    }

    static Dictionary<string, Rectangle> ZonasOperacion(Form f)
    {
        return new Dictionary<string, Rectangle>
        {
            { "encabezado", R(f, Campo(f, "pnlEncabezado")) },
            { "codigo", R(f, Nombre(f, "tlpCodigo")) },
            { "buscar", R(f, Nombre(f, "btnBuscar")) },
            { "hoy", R(f, Nombre(f, "dgvHoy")) },
            { "detalle", R(f, Nombre(f, "pnlDetalle")) },
            { "resumen", R(f, Nombre(f, "tlpResumen")) },
            { "jugadas", R(f, Nombre(f, "dgvJugadas")) },
            { "motivo", R(f, Nombre(f, "lblMotivo")) },
            { "accion", R(f, Nombre(f, "btnAccion")) }
        };
    }

    static Jugada Directa(string n, int rango, decimal monto)
    {
        return new Jugada { Tipo = ApuestaBLL.Directa, Monto = monto, Numeros = { new NumeroElegido { Numero = n, Orden = 1, Rango = rango } } };
    }

    static Jugada Redoblona(string a, int ra, string b, int rb, decimal monto)
    {
        return new Jugada
        {
            Tipo = ApuestaBLL.Redoblona, Monto = monto,
            Numeros = { new NumeroElegido { Numero = a, Orden = 1, Rango = ra }, new NumeroElegido { Numero = b, Orden = 2, Rango = rb } }
        };
    }

    // ancho/alto 0 = tamanio original de la pantalla
    static void Capturar(string nombre, Form f, int ancho, int alto, Action<Form> preparar, Func<Form, Dictionary<string, Rectangle>> zonas)
    {
        try
        {
            f.StartPosition = FormStartPosition.Manual;
            f.WindowState = FormWindowState.Normal;
            f.ShowInTaskbar = false;
            f.Location = new Point(-6000, -6000);
            if (ancho > 0) f.Size = new Size(ancho, alto);
            f.Show();
            Application.DoEvents();
            if (preparar != null) preparar(f);
            Application.DoEvents();
            f.Refresh();

            Rectangle cliente = f.RectangleToScreen(f.ClientRectangle);
            using (Bitmap completo = new Bitmap(f.Width, f.Height))
            {
                f.DrawToBitmap(completo, new Rectangle(0, 0, f.Width, f.Height));
                Rectangle recorte = new Rectangle(cliente.Left - f.Left, cliente.Top - f.Top, f.ClientSize.Width, f.ClientSize.Height);
                using (Bitmap img = completo.Clone(recorte, completo.PixelFormat))
                    img.Save(Path.Combine(dir, nombre + ".png"), ImageFormat.Png);
            }

            Dictionary<string, Rectangle> z = zonas(f);
            if (json.Length > 1) json.Append(",");
            json.Append("\n  \"" + nombre + "\": {\"ancho\": " + f.ClientSize.Width + ", \"alto\": " + f.ClientSize.Height + ", \"zonas\": {");
            json.Append(string.Join(", ", z.Select(kv => "\"" + kv.Key + "\": [" + kv.Value.X + ", " + kv.Value.Y + ", " + kv.Value.Width + ", " + kv.Value.Height + "]")));
            json.Append("}}");
            Console.WriteLine("  " + nombre + "  " + f.ClientSize.Width + "x" + f.ClientSize.Height);
        }
        catch (Exception ex)
        {
            errores.Add(nombre + ": " + ex);
        }
        finally
        {
            f.Close();
            f.Dispose();
        }
    }

    // Rectangulo de un control relativo al area de la ventana (la misma que la captura recortada).
    static Rectangle R(Form f, Control c)
    {
        return f.RectangleToClient(c.RectangleToScreen(c.ClientRectangle));
    }

    static Rectangle Union(Rectangle a, Rectangle b)
    {
        return Rectangle.Union(a, b);
    }

    static IEnumerable<Control> Todos(Control c)
    {
        foreach (Control h in c.Controls)
        {
            yield return h;
            foreach (Control n in Todos(h)) yield return n;
        }
    }

    static IEnumerable<T> Visibles<T>(Form f) where T : Control
    {
        return Todos(f).OfType<T>().Where(c => c.Visible);
    }

    static Control Nombre(Form f, string nombre)
    {
        Control c = Todos(f).FirstOrDefault(x => x.Name == nombre);
        if (c == null) throw new Exception("No hay control '" + nombre + "' en " + f.Name);
        return c;
    }

    // Controles creados por codigo sin Name (por ejemplo el encabezado de Estilos.Encabezado): se toman del campo.
    static Control Campo(Form f, string campo)
    {
        FieldInfo fi = f.GetType().GetField(campo, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi == null) throw new Exception("No hay campo '" + campo + "' en " + f.Name);
        return (Control)fi.GetValue(f);
    }

    static T Nombre<T>(Form f, string nombre) where T : Control
    {
        return (T)Nombre(f, nombre);
    }

    static Control Boton(Form f, string texto)
    {
        Button b = Todos(f).OfType<Button>().FirstOrDefault(x => x.Text == texto && x.Visible);
        if (b == null) throw new Exception("No hay boton '" + texto + "' en " + f.Name);
        return b;
    }

    static void Monto(Form f, decimal valor)
    {
        Nombre<NumericUpDown>(f, "nudMonto").Value = valor;
    }

    static void Click(Form f, string texto)
    {
        Button b = Todos(f).OfType<Button>().FirstOrDefault(x => x.Text == texto && x.Visible);
        if (b == null) throw new Exception("No hay boton '" + texto + "'");
        b.PerformClick();
        Application.DoEvents();
    }

    static void Sql(string q)
    {
        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["AgenciaQuiniela"].ConnectionString))
        {
            con.Open();
            new SqlCommand(q, con).ExecuteNonQuery();
        }
    }
}
