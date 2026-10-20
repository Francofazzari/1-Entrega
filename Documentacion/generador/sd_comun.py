# -*- coding: utf-8 -*-
"""Diagramas de secuencia que se repiten en varios casos de uso porque ejecutan el mismo código.

Convenciones (las mismas en todos los diagramas):
  - Cada mensaje sincrónico es un método, constructor o propiedad que existe en la clase de la línea de
    vida, con sus argumentos tal como figuran en el código; las respuestas (línea punteada) se dibujan
    solo cuando el método devuelve un valor.
  - Una línea de vida sin «:» representa a la clase: recibe llamadas a miembros estáticos.
  - Las excepciones se muestran con un fragmento break y una respuesta "Exception" hacia quien llamó.
  - Los mensajes a la base de datos muestran la sentencia SQL que ejecuta la capa DAL (abreviada con …).
  - Un marco ref remite a otro diagrama de secuencia donde se detalla esa parte.
"""
from secuencia import Secuencia

BD = 'AgenciaQuiniela'

SQL_SORTEO = 'SELECT … FROM SORTEOS JOIN JUEGOS\nLEFT JOIN LOTERIAS, TURNOS WHERE\njuego, fecha, lotería y turno'
SQL_INSERT_SORTEO = "INSERT INTO SORTEOS (…, Estado)\nVALUES (…, 'Abierto');\nSELECT SCOPE_IDENTITY()"
SQL_NUMEROS = 'SELECT Numero, Posicion\nFROM NUMERO_GANADOR WHERE\nIdSorteo = @id ORDER BY Posicion'
SQL_BITACORA = 'INSERT INTO Bitacora (FechaHora,\nUsuarioId, UsuarioNombre,\nActividad, Detalle)'


def bitacora(s, origen, usuario, actividad):
    """Registro de un evento en la bitácora: BitacoraBLL -> BitacoraDAL -> base de datos."""
    with s.llamada(origen, 'bb', 'Registrar(%s.Id, %s.Nombre,\n"%s", detalle)' % (usuario, usuario, actividad)):
        with s.llamada('bb', 'bdal', 'Registrar(b)'):
            s.mensaje('bdal', 'bd', SQL_BITACORA)


def lineas_bitacora(s):
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)


# ------------------------------------------------------------------ sorteos
def _buscar_o_crear(s):
    """Interior de SorteoBLL.ObtenerOCrear (con la línea de vida 'sb' activa)."""
    with s.llamada('sb', 'sb', 'Buscar(juego, loteria, turno, fecha)'):
        with s.llamada('sb', 'sd', 'Buscar(juego.Id, idLoteria,\nidTurno, fecha.Date)', ret='Sorteo o null'):
            s.mensaje('sd', 'bd', SQL_SORTEO, ret='fila o vacío')
    with s.fragmento('opt', '[el sorteo no existe]'):
        with s.llamada('sb', 'sd', 'Insertar(nuevo)', ret='nuevo.Id'):
            s.mensaje('sd', 'bd', SQL_INSERT_SORTEO, ret='Id')


def sd_obtener_sorteo(titulo):
    """ApuestaBLL pide el sorteo del día sobre el que vende (CU-N01 y CU-N02)."""
    s = Secuencia(titulo)
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    s.bd('bd', BD)
    with s.llamada('ab', 'sb', 'ObtenerParaVender(juego, loteria, turno)', ret='s : Sorteo'):
        with s.llamada('sb', 'sb', 'ObtenerOCrear(juego, loteria,\nturno, DateTime.Today)'):
            _buscar_o_crear(s)
        with s.fragmento('break', '[s.Estado != Abierto]'):
            s.mensaje('sb', 'sb', 'Descripcion(s)')
            s.respuesta('sb', 'ab', 'Exception: "El sorteo … ya no acepta\napuestas (está …)."')
    return s


def sd_obtener_o_crear(titulo, mensaje):
    """FormReporte pide el sorteo elegido y, si no existe, se crea (CU-N09 y CU-N10)."""
    s = Secuencia(titulo)
    s.objeto('fr', ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    s.bd('bd', BD)
    with s.llamada('fr', 'sb', mensaje, ret='s : Sorteo'):
        _buscar_o_crear(s)
    return s


def sd_leer_sorteo(titulo):
    """Lectura de un sorteo y de sus números ganadores para la pizarra (CU-N07)."""
    s = Secuencia(titulo)
    s.objeto('fr', ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    s.bd('bd', BD)
    with s.llamada('fr', 'sb', 'Buscar(juegoGanadores, l, t, fecha)', ret='c.Sorteo'):
        with s.llamada('sb', 'sd', 'Buscar(juego.Id, idLoteria,\nidTurno, fecha.Date)', ret='Sorteo o null'):
            s.mensaje('sd', 'bd', SQL_SORTEO, ret='fila o vacío')
    with s.fragmento('opt', '[c.Sorteo != null y está Sorteado]'):
        with s.llamada('fr', 'sb', 'ObtenerNumerosGanadores(c.Sorteo)', ret='c.Numeros'):
            with s.llamada('sb', 'sd', 'ObtenerNumerosGanadores(s.Id)', ret='List<NumeroGanador>'):
                s.mensaje('sd', 'bd', SQL_NUMEROS, ret='filas')
    return s


def sd_numeros_ganadores(titulo, origen, texto_origen, argumento):
    s = Secuencia(titulo)
    s.objeto(origen, texto_origen)
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    s.bd('bd', BD)
    with s.llamada(origen, 'sb', 'ObtenerNumerosGanadores(%s)' % argumento, ret='ganadores'):
        with s.llamada('sb', 'sd', 'ObtenerNumerosGanadores(s.Id)', ret='List<NumeroGanador>'):
            s.mensaje('sd', 'bd', SQL_NUMEROS, ret='filas')
    return s


def sd_resultado_bll(titulo, origen, ref_dal):
    """Validar, completar y guardar el resultado de un sorteo en la capa de negocio (SorteoBLL.CargarResultado).

    origen = 'carga' (CU-N09, desde FormResultado) o 'azar' (CU-N10, RealizarSorteo desde FormReporte).
    """
    s = Secuencia(titulo)
    s.objeto('f', ':FormResultado' if origen == 'carga' else ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('bb', ':BitacoraBLL')

    def interior():
        with s.fragmento('break', '[ya Sorteado o fecha futura]'):
            s.respuesta('sb', 'f', 'Exception: "Ese sorteo ya se realizó." o\n"No se puede realizar un sorteo de\nuna fecha '
                                   'futura."')
        s.mensaje('sb', 'sb', 'CompletarResultado(s.Juego, cargados)')
        with s.llamada('sb', 'sd', 'GuardarResultado(s.Id, numeros, Sorteado)', ret='true / false'):
            s.referencia(ref_dal, ['sd'])
        with s.fragmento('break', '[GuardarResultado == false]'):
            s.respuesta('sb', 'f', 'Exception: "Ese sorteo ya se realizó."')
        s.mensaje('sb', 'sb', 'Descripcion(s)')
        s.mensaje('sb', 'bb', 'Registrar(usuario.Id, usuario.Nombre,\n"Sorteo realizado", detalle)')

    if origen == 'carga':
        with s.llamada('f', 'sb', 'CargarResultado(sorteo, textos, usuario)', ret='List<NumeroGanador>'):
            interior()
    else:
        with s.llamada('f', 'sb', 'RealizarSorteo(s, usuario)', ret='List<NumeroGanador>'):
            with s.llamada('sb', 'sb', 'CargarResultado(s, new string[\ns.Juego.CantidadSorteados], usuario)'):
                interior()
    return s


def sd_resultado_dal(titulo):
    """Guardado del resultado en una transacción y registro en la bitácora (CU-N09 y CU-N10)."""
    s = Secuencia(titulo)
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    lineas_bitacora(s)
    with s.llamada('sb', 'sd', 'GuardarResultado(s.Id, numeros, Sorteado)', ret='true'):
        s.mensaje('sd', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('sd', 'bd', "UPDATE SORTEOS SET Estado = 'Sorteado'\nWHERE Id = @id AND Estado <> 'Sorteado'",
                  ret='filas afectadas')
        with s.fragmento('break', '[no cambió ninguna fila]'):
            s.mensaje('sd', 'bd', 'ROLLBACK')
            s.respuesta('sd', 'sb', 'false')
        with s.fragmento('loop', '[por cada número ganador n]'):
            s.mensaje('sd', 'bd', 'INSERT INTO NUMERO_GANADOR\n(IdSorteo, Numero, Posicion)')
        s.mensaje('sd', 'bd', 'COMMIT')
    bitacora(s, 'sb', 'usuario', 'Sorteo realizado')
    return s


# ------------------------------------------------------------------ tickets
def sd_vender(titulo, origen, texto_origen, argumentos):
    """Armado y registro del ticket en la capa de negocio: ApuestaBLL.Vender (CU-N01 y CU-N02). El detalle de
    ObtenerParaVender y de Insertar está en los dos diagramas siguientes de cada caso de uso."""
    s = Secuencia(titulo)
    s.objeto(origen, texto_origen)
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('bb', ':BitacoraBLL')
    with s.llamada(origen, 'ab', 'Vender(%s)' % argumentos, ret='a : Apuesta'):
        with s.fragmento('break', '[plantillas vacía]'):
            s.respuesta('ab', origen, 'Exception: "Cargá al menos una jugada\nantes de confirmar la venta."')
        with s.fragmento('loop', '[por cada plantilla p]'):
            s.mensaje('ab', 'ab', 'ValidarJugada(juego, p)')
        with s.fragmento('break', '[jugada inválida]'):
            s.respuesta('ab', origen, 'Exception con el motivo (monto, cifras,\nposición, cantidad o rango)')
        with s.alternativa() as a:
            with a.caso('[juego.UsaLoteriaTurno]'):
                with s.fragmento('break', '[sin loterías o sin turnos]'):
                    s.respuesta('ab', origen, 'Exception: "Elegí al menos una lotería."\no "Elegí al menos un horario."')
                with s.fragmento('loop', '[por cada lotería l y cada turno t]'):
                    s.mensaje('ab', 'sb', 'ObtenerParaVender(juego, l, t)', ret='sorteo')
            with a.caso('[else]'):
                s.mensaje('ab', 'sb', 'ObtenerParaVender(juego, null, null)', ret='sorteo')
        s.nota('ab', 'Arma la Apuesta (Guid.NewGuid(), vendedor, DateTime.Now, Vigente) con una Jugada por sorteo y '
                     'plantilla; Total = suma de los montos.', 210)
        s.mensaje('ab', 'ad', 'Insertar(a)')
        s.mensaje('ab', 'ab', 'CodigoCorto(a)')
        s.mensaje('ab', 'bb', 'Registrar(vendedor.Id, vendedor.Nombre,\n"Venta", detalle)')
    return s


def sd_guardar_ticket(titulo):
    """ApuestaDAL guarda el ticket en una transacción y la venta queda en la bitácora (CU-N01 y CU-N02)."""
    s = Secuencia(titulo)
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ad', ':ApuestaDAL')
    lineas_bitacora(s)
    with s.llamada('ab', 'ad', 'Insertar(a)'):
        s.mensaje('ad', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('ad', 'bd', 'INSERT INTO APUESTAS (CodigoTicket,\nUsuarioId, FechaHora, Total, Estado);\n'
                              'SELECT SCOPE_IDENTITY()', ret='a.Id')
        with s.fragmento('loop', '[por cada jugada j de a.Jugadas]'):
            s.mensaje('ad', 'bd', 'INSERT INTO JUGADAS (IdApuesta, IdSorteo,\nTipo, EsAutomatica, Monto, Orden);\n'
                                  'SELECT SCOPE_IDENTITY()', ret='j.Id')
            with s.fragmento('loop', '[por cada número n de j.Numeros]'):
                s.mensaje('ad', 'bd', 'INSERT INTO JUGADA_NUMERO\n(IdJugada, Numero, Orden, Rango)')
        s.mensaje('ad', 'bd', 'COMMIT')
    s.nota('ad', 'Si un INSERT falla no se llega al COMMIT: la transacción se revierte y no queda guardado ni el '
                 'ticket ni sus jugadas.', 210)
    bitacora(s, 'ab', 'vendedor', 'Venta')
    return s


def sd_listar_ventas(titulo, origen, texto_origen, argumentos):
    """Tickets vendidos en un período (CU-N04: los de hoy; CU-N12: el reporte de ventas)."""
    s = Secuencia(titulo)
    s.objeto(origen, texto_origen)
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.bd('bd', BD)
    with s.llamada(origen, 'ab', 'ListarVentas(%s)' % argumentos, ret='List<Apuesta>'):
        with s.fragmento('break', '[desde.Date > hasta.Date]'):
            s.respuesta('ab', origen, 'Exception: "La fecha \'Desde\' no puede\nser mayor que la fecha \'Hasta\'."')
        with s.llamada('ab', 'ad', 'ListarVentas(desde.Date,\nhasta.Date.AddDays(1), idJuego)', ret='List<Apuesta>'):
            s.mensaje('ad', 'bd', 'SELECT … FROM APUESTAS JOIN USUARIOS\nCROSS APPLY (JUGADAS JOIN SORTEOS)\n'
                                  'JOIN JUEGOS WHERE FechaHora en\n[desde, hasta) [AND juego]', ret='filas')
    return s


def sd_buscar_por_codigo(titulo):
    """Búsqueda del ticket por su código completo o por sus primeros caracteres (CU-N04)."""
    s = Secuencia(titulo)
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.bd('bd', BD)
    with s.llamada('fo', 'ab', 'BuscarPorCodigo(txtCodigo.Text)', ret='a : Apuesta'):
        with s.fragmento('break', '[el texto está vacío (4.a)]'):
            s.respuesta('ab', 'fo', 'Exception: "Ingresá el código\ndel ticket."')
        with s.fragmento('opt', '[no es un GUID completo]'):
            with s.fragmento('break', '[código inválido (4.b)]'):
                s.respuesta('ab', 'fo', 'Exception: "El código no es válido. …"')
            with s.llamada('ab', 'ad', 'BuscarCodigosPorPrefijo(\ncodigoTexto.ToUpperInvariant())', ret='encontrados'):
                s.mensaje('ad', 'bd', 'SELECT TOP 2 CodigoTicket FROM\nAPUESTAS WHERE CONVERT(…,\nCodigoTicket) LIKE @prefijo',
                          ret='códigos')
            with s.fragmento('break', '[ninguno (4.c) o varios (4.d)]'):
                s.respuesta('ab', 'fo', 'Exception: "No existe ningún ticket…"\no "Hay más de un ticket que…"')
        with s.llamada('ab', 'ad', 'ObtenerPorCodigo(codigo)', ret='Apuesta o null'):
            s.mensaje('ad', 'bd', 'SELECT … FROM APUESTAS JOIN USUARIOS\nWHERE CodigoTicket = @codigo',
                      ret='cabecera')
            s.mensaje('ad', 'bd', 'SELECT … FROM JUGADAS JOIN SORTEOS\nJOIN JUEGOS LEFT JOIN LOTERIAS, TURNOS\n'
                                  'WHERE IdApuesta = @id', ret='jugadas')
            s.mensaje('ad', 'bd', 'SELECT … FROM JUGADA_NUMERO\nJOIN JUGADAS WHERE IdApuesta = @id', ret='números')
        with s.fragmento('break', '[a == null (4.c)]'):
            s.respuesta('ab', 'fo', 'Exception: "No existe ningún ticket…"')
    return s


VISTAS = {
    'Venta': 'muestra el ticket con el título "Venta registrada"',
    'Reimpresion': 'muestra el ticket con el título "Reimpresión\ndel ticket" y la marca *** REIMPRESIÓN ***',
    'Consulta': 'muestra "Ticket XXXXXXXX" con el estado,\nsin el botón «Imprimir»',
}


def sd_mostrar_ticket(titulo, creador, texto_creador, argumento, vista):
    """Pantalla del ticket (FormTicket): la misma para la venta, la reimpresión y la consulta desde los reportes."""
    s = Secuencia(titulo)
    s.actor('op', 'Operador')
    s.objeto(creador, texto_creador)
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('ab', 'ApuestaBLL')
    s.objeto('tr', 'TicketRenderer')
    with s.crear(creador, 'ft', '«create» FormTicket(%s, VistaTicket.%s)' % (argumento, vista)):
        s.mensaje('ft', 'ab', 'CodigoCorto(apuesta)', ret='código corto')
        s.mensaje('ft', 'tr', 'Dibujar(g, apuesta, esReimpresion, 0, 0)', ret='alto')
    s.asincrono('ft', 'op', VISTAS[vista])
    with s.llamada('ft', 'ft', 'pnlPapel_Paint(sender, e)'):
        s.mensaje('ft', 'tr', 'Dibujar(e.Graphics, apuesta, esReimpresion,\nMargenPapel, MargenPapel)', ret='alto')
    with s.fragmento('opt', '[presiona «Copiar código»]'):
        with s.llamada('op', 'ft', 'btnCopiar_Click(sender, e)'):
            s.mensaje('ft', 'ab', 'CodigoCompleto(apuesta)', ret='código completo')
        s.asincrono('ft', 'op', '"Código copiado"')
    if vista != 'Consulta':
        with s.fragmento('opt', '[presiona «Imprimir»]'):
            s.referencia('CU-N03 Imprimir ticket (extensión)', ['op', 'ft'])
    s.asincrono('op', 'ft', 'presiona «Cerrar»')
    s.destruir('ft')
    return s


# ------------------------------------------------------------------ reportes
def sd_abrir_reportes(titulo, ref_construir, cierre, nota_fp=None):
    """Apertura de FormReporte desde «Reportes» (CU-N07 y CU-N12)."""
    s = Secuencia(titulo)
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fr', ':FormReporte', creado=True)
    with s.llamada('op', 'fp', 'btnReportes_Click(sender, e)'):
        with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
            with s.crear('fp', 'fr', '«create» FormReporte()'):
                s.referencia(ref_construir, ['fr'])
    if nota_fp:
        s.nota('fp', nota_fp, 230)
    cierre(s)
    return s


def sd_construir_reporte(titulo):
    """El constructor de FormReporte arma las tres pestañas y carga las ventas y los pagos del día."""
    s = Secuencia(titulo)
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fr', ':FormReporte', creado=True)
    s.objeto('sm', ':SesionManager')
    s.objeto('jb', ':JuegoBLL')
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('tb', ':TurnoBLL')
    with s.crear('fp', 'fr', '«create» FormReporte()'):
        with s.llamada('fr', 'fr', 'FormReporte(VistaReporte.Ventas,\nDateTime.Today)'):
            s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario (esAdmin)')
            s.mensaje('fr', 'jb', 'ObtenerActivos()', ret='juegos')
            s.mensaje('fr', 'fr', 'ConstruirVentas(desde)')
            s.mensaje('fr', 'fr', 'ConstruirPagos(desde)')
            with s.llamada('fr', 'fr', 'ConstruirGanadores()'):
                s.mensaje('fr', 'lb', 'ObtenerActivas()', ret='loterias')
                s.mensaje('fr', 'tb', 'ObtenerActivos()', ret='turnos')
            s.mensaje('fr', 'fr', 'CargarVentas()')
            s.mensaje('fr', 'fr', 'CargarPagos()')
            s.mensaje('fr', 'fr', 'MostrarVista(vista)')
    s.nota('fr', 'CargarVentas() y CargarPagos() listan lo del día (CU-N12 y CU-N13). JuegoBLL, LoteriaBLL y '
                 'TurnoBLL leen JUEGOS, LOTERIAS y TURNOS con su DAL.', 220)
    return s
