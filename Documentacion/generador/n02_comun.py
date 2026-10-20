# -*- coding: utf-8 -*-
"""Piezas comunes de las especificaciones de casos de uso: diagramas de entidades (BE), DER,
correspondencia clase-tabla y diagrama de casos de uso de cada especificacion."""
from comun import CASOS, SISTEMA_CORTO
from clases import DiagramaClases, DiagramaER
from cu import DiagramaCU

# ------------------------------------------------------------------ entidades (BE)
ATRIBUTOS_BE = {
    'Apuesta': ['Id', 'CodigoTicket', 'UsuarioId', 'UsuarioNombre', 'FechaHora', 'Total', 'Estado'],
    'Jugada': ['Id', 'Tipo', 'EsAutomatica', 'Monto', 'Orden'],
    'NumeroElegido': ['Numero', 'Orden', 'Rango'],
    'Sorteo': ['Id', 'Fecha', 'Estado'],
    'Juego': ['Id', 'Nombre', 'Codigo', 'CantidadNumeros', 'NumeroMin', 'NumeroMax', 'CantidadSorteados', 'UsaRango',
              'UsaLoteriaTurno', 'Precio', 'Activo'],
    'Loteria': ['Id', 'Nombre', 'Codigo', 'Activa'],
    'Turno': ['Id', 'Nombre', 'Activo'],
    'NumeroGanador': ['Numero', 'Posicion'],
    'Premio': ['SorteoRealizado', 'Detalle', 'Importe'],
    'Pago': ['Id', 'IdApuesta', 'CodigoTicket', 'JuegoNombre', 'UsuarioId', 'UsuarioNombre', 'FechaHora', 'Monto'],
    'Usuario': ['Id', 'NroTerminal', 'Clave', 'Nombre', 'Apellido', 'Activo'],
    'Bitacora': ['Id', 'FechaHora', 'UsuarioId', 'UsuarioNombre', 'Actividad', 'Detalle'],
}

GRILLA_BE = [
    ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido'],
    ['Pago', 'Juego', 'Sorteo', 'Premio'],
    ['Bitacora', 'NumeroGanador', 'Loteria', 'Turno'],
]

# asociaciones navegables tal como estan en el codigo (propiedades que referencian otra clase)
ASOC_BE = [
    ('composicion', 'Apuesta', 'Jugada', '+Jugadas', '1..*', '1'),
    ('asociacion', 'Apuesta', 'Juego', '+Juego', '1', '0..*'),
    ('composicion', 'Jugada', 'NumeroElegido', '+Numeros', '1..*', '1'),
    ('asociacion', 'Jugada', 'Sorteo', '+Sorteo', '1', '0..*'),
    ('asociacion', 'Sorteo', 'Juego', '+Juego', '1', '0..*'),
    ('asociacion', 'Sorteo', 'Loteria', '+Loteria', '0..1', '0..*'),
    ('asociacion', 'Sorteo', 'Turno', '+Turno', '0..1', '0..*'),
    ('asociacion', 'Premio', 'Jugada', '+Jugada', '1', '0..1'),
]

NOTAS_BE = {
    'Usuario': 'Apuesta y Pago guardan el Id del usuario (UsuarioId) y su nombre para mostrarlo; la relación con '
               'USUARIOS está en la base de datos (clave foránea).',
    'Pago': 'IdApuesta identifica al ticket pagado (APUESTAS.Id); CodigoTicket y JuegoNombre se completan al listar '
            'los pagos.',
    'Bitacora': 'Registro de auditoría: guarda el usuario y el detalle del evento como texto, sin relaciones.',
    'NumeroGanador': 'Los números ganadores se obtienen por sorteo (SorteoBLL.ObtenerNumerosGanadores); la clase no '
                     'guarda referencia al sorteo.',
    'Premio': 'No se guarda: PremioBLL lo calcula para cada jugada al consultar o pagar el ticket.',
}


def diagrama_be(titulo, clases, notas=()):
    dc = DiagramaClases(titulo)
    dc.GAP_X = 96
    dc.GAP_FILA = 70
    filas = [[c if c in clases else None for c in fila] for fila in GRILLA_BE]
    dc.paquete('BE', filas)
    for c in clases:
        dc.clase(c, attrs=ATRIBUTOS_BE[c])
    for tipo, o, d, rol, md, mo in ASOC_BE:
        if o in clases and d in clases:
            if o == 'Apuesta' and d == 'Juego':
                dc.rel(tipo, o, d, rol='+/Juego', mult_d=md, mult_o=mo)
            else:
                dc.rel(tipo, o, d, rol=rol, mult_d=md, mult_o=mo)
    dc.notas_be = [c for c in notas if c in clases]
    return dc


# ------------------------------------------------------------------ DER
GRILLA_DER = [
    ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO'],
    ['PAGOS', 'LOTERIAS', 'SORTEOS', 'TURNOS'],
    ['Bitacora', None, 'JUEGOS', 'NUMERO_GANADOR'],
]

# (hija, padre, cantidad de filas hijas por cada fila padre)
FK_DER = [
    ('APUESTAS', 'USUARIOS', '0..*'),
    ('PAGOS', 'APUESTAS', '0..1'),
    ('PAGOS', 'USUARIOS', '0..*'),
    ('JUGADAS', 'APUESTAS', '1..*'),
    ('JUGADA_NUMERO', 'JUGADAS', '1..*'),
    ('JUGADAS', 'SORTEOS', '0..*'),
    ('NUMERO_GANADOR', 'SORTEOS', '0..*'),
    ('SORTEOS', 'LOTERIAS', '0..*'),
    ('SORTEOS', 'TURNOS', '0..*'),
    ('SORTEOS', 'JUEGOS', '0..*'),
]


def diagrama_der(titulo, tablas):
    er = DiagramaER(titulo)
    filas = [[t if t in tablas else None for t in fila] for fila in GRILLA_DER]
    filas = [f for f in filas if any(f)]
    ncol = max(len(f) for f in filas)
    usadas = [k for k in range(ncol) if any(f[k] for f in filas)]
    filas = [[f[k] for k in usadas] for f in filas]
    er.grilla(filas)
    for hija, padre, mult in FK_DER:
        if hija in tablas and padre in tablas:
            er.rel(hija, padre, mult)
    if 'Bitacora' in tablas:
        er.nota_bitacora = True
    return er


# correspondencia entre clases de entidad y tablas (para la pagina del DER)
CORRESPONDENCIA = {
    'Apuesta': ('APUESTAS', 'Jugadas: JUGADAS.IdApuesta. Juego no es columna: se deduce del sorteo de sus jugadas.'),
    'Jugada': ('JUGADAS', 'Sorteo: IdSorteo. Numeros: JUGADA_NUMERO.IdJugada.'),
    'NumeroElegido': ('JUGADA_NUMERO', 'El Id y el IdJugada de la tabla no se cargan en la clase: el número pertenece '
                                       'a su jugada.'),
    'Sorteo': ('SORTEOS', 'Juego, Loteria y Turno: IdJuego, IdLoteria e IdTurno (estos dos, NULL fuera de Quiniela).'),
    'Juego': ('JUEGOS', 'Mismas columnas.'),
    'Loteria': ('LOTERIAS', 'Mismas columnas.'),
    'Turno': ('TURNOS', 'Mismas columnas.'),
    'NumeroGanador': ('NUMERO_GANADOR', 'El Id y el IdSorteo de la tabla no se cargan en la clase.'),
    'Pago': ('PAGOS', 'CodigoTicket, JuegoNombre y UsuarioNombre se leen de APUESTAS, JUEGOS y USUARIOS al listar.'),
    'Usuario': ('USUARIOS', 'Permisos se carga desde USUARIO_PERMISO (ver T04).'),
    'Bitacora': ('Bitacora', 'Mismas columnas.'),
    'Premio': ('—', 'No se guarda: se calcula.'),
}


# ------------------------------------------------------------------ diagrama de casos de uso de cada especificacion
def diagrama_cu(codigo, extiende=(), extendido_por=(), incluye=(), incluido_por=(), secundarios=()):
    """Diagrama del caso de uso con su actor, el Administrador (que especializa al Operador) y sus relaciones.

    extiende: [(base, punto, condicion)] casos que este caso extiende.
    extendido_por: [(extensor, punto, condicion)] casos que extienden a este.
    incluye / incluido_por: codigos de casos de uso.
    secundarios: [(id, nombre, estereotipo)] actores secundarios (a la derecha).
    """
    nombre, proceso, actor = CASOS[codigo]
    u = DiagramaCU('uc %s %s' % (codigo, nombre))
    otros = list(extiende) + list(extendido_por) + [(c, None, None) for c in incluye] + \
        [(c, None, None) for c in incluido_por]
    n_otros = len(otros)
    alto = max(300, 120 + 120 * max(1, (n_otros + 1) // 2) + 40)
    ancho_lim = 640 if n_otros else 420
    x_lim = 210
    u.limite(SISTEMA_CORTO, x_lim, 50, ancho_lim, alto)
    w_main, h_main = u.caso('cu', '%s\n%s' % (codigo, nombre), x_lim + 40, 50 + alto / 2 - 40, w=250)
    u.actor('act', actor, 70, 50 + alto / 2 - 60)
    u.asociacion('act', 'cu')
    if actor == 'Operador':
        u.actor('adm', 'Administrador', 70, 50 + alto / 2 + 110)
        u.generalizacion('adm', 'act')
    # casos relacionados a la derecha, uno debajo del otro
    y = 50 + 70
    paso = (alto - 100) / max(1, n_otros)
    xr = x_lim + ancho_lim - 260
    notas_x = x_lim + ancho_lim + 30
    k = 0
    for base, punto, cond in extiende:
        u.caso('o%d' % k, '%s\n%s' % (base, CASOS[base][0]), xr, y + k * paso, w=230)
        u.extiende('cu', 'o%d' % k, condicion=cond, punto=punto, nota=(notas_x, y + k * paso - 6, 250))
        k += 1
    for ext, punto, cond in extendido_por:
        u.caso('o%d' % k, '%s\n%s' % (ext, CASOS[ext][0]), xr, y + k * paso, w=230)
        u.extiende('o%d' % k, 'cu', condicion=cond, punto=punto, nota=(notas_x, y + k * paso - 6, 250))
        if CASOS[ext][2] == 'Administrador' and actor != 'Administrador':
            u.asociacion('adm', 'o%d' % k)
        k += 1
    for inc in incluye:
        u.caso('o%d' % k, '%s\n%s' % (inc, CASOS[inc][0]), xr, y + k * paso, w=230)
        u.incluye('cu', 'o%d' % k)
        k += 1
    for base in incluido_por:
        u.caso('o%d' % k, '%s\n%s' % (base, CASOS[base][0]), xr, y + k * paso, w=230)
        u.incluye('o%d' % k, 'cu')
        u.asociacion('act', 'o%d' % k)
        k += 1
    xs = x_lim + ancho_lim + (300 if (extiende or extendido_por) else 60)
    for i, (ident, nom, est) in enumerate(secundarios):
        u.actor(ident, nom, xs, 50 + alto / 2 - 50 + i * 150, est)
    return u


# ------------------------------------------------------------------ modelo de casos de uso del sistema
def d_cu_sistema():
    """Los 13 casos de uso del sistema con sus actores y sus relaciones «include» y «extend».

    CU-N08, CU-N09 y CU-N10 son objetivos propios del Administrador: se asocian con el y no extienden CU-N07,
    aunque sus botones esten en la misma pantalla. La impresora no es actor (impresora predeterminada de Windows).
    """
    u = DiagramaCU('uc Modelo de casos de uso del sistema - ' + SISTEMA_CORTO)
    u.limite(SISTEMA_CORTO, 190, 50, 760, 850)
    # el Operador queda a la altura del medio de su columna; el Administrador, junto a los casos que solo
    # el puede usar (RN-17). La generalizacion rodea el limite del sistema por debajo para no cruzar nada.
    u.actor('op', 'Operador', 40, 443)
    u.actor('adm', 'Administrador', 1020, 593)
    col1 = [('CU-N01', 90), ('CU-N02', 190), ('CU-N05', 290), ('CU-N06', 390), ('CU-N11', 490), ('CU-N07', 590),
            ('CU-N12', 690), ('CU-N13', 790)]
    col2 = [('CU-N03', 190), ('CU-N04', 390), ('CU-N08', 490), ('CU-N09', 590), ('CU-N10', 690)]
    for codigo, y in col1:
        u.caso(codigo, '%s\n%s' % (codigo, CASOS[codigo][0]), 250, y, w=290)
        u.asociacion('op', codigo, punta=True)
    for codigo, y in col2:
        u.caso(codigo, '%s\n%s' % (codigo, CASOS[codigo][0]), 620, y, w=290)
    for codigo in ('CU-N08', 'CU-N09', 'CU-N10'):
        u.asociacion('adm', codigo, punta=True)
    u.generalizacion('adm', 'op', puntos=[(1037, 935), (57, 935)])
    for base in ('CU-N01', 'CU-N02', 'CU-N05'):
        u.extiende('CU-N03', base)
    # una sola nota para las tres extensiones de CU-N03: tienen la misma condicion
    u.notas.append(('extend', 'CU-N03', 'CU-N02',
                    'Condición: {el operador presiona «Imprimir» en la pantalla del ticket}\n'
                    'Puntos de extensión: Ticket emitido (CU-N01 y CU-N02) y Reimpresión en pantalla (CU-N05)',
                    (625, 272, 285)))
    for base in ('CU-N05', 'CU-N06', 'CU-N11'):
        u.incluye(base, 'CU-N04')
    return u.dibujar()
