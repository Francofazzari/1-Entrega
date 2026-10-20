# -*- coding: utf-8 -*-
"""N01. Especificacion funcional por proceso de negocio: textos y diagramas."""
from comun import CASOS, cu, SISTEMA_CORTO
from cu import DiagramaCU
from actividad import DiagramaActividad
from secuencia import Secuencia
from clases import DiagramaClases

# ------------------------------------------------------------------ modelo conceptual (dominio)
# Atributos de cada concepto (sin identificadores: las relaciones se muestran como asociaciones).
ATRIBUTOS_CONCEPTO = {
    'Juego': ['Nombre', 'Codigo', 'CantidadNumeros', 'NumeroMin', 'NumeroMax', 'CantidadSorteados', 'UsaRango',
              'UsaLoteriaTurno', 'Precio', 'Activo'],
    'Loteria': ['Nombre', 'Codigo', 'Activa'],
    'Turno': ['Nombre', 'Activo'],
    'Sorteo': ['Fecha', 'Estado'],
    'NumeroGanador': ['Numero', 'Posicion'],
    'Apuesta': ['CodigoTicket', 'FechaHora', 'Total', 'Estado'],
    'Jugada': ['Tipo', 'EsAutomatica', 'Monto', 'Orden'],
    'NumeroElegido': ['Numero', 'Orden', 'Rango'],
    'Premio': ['SorteoRealizado', 'Detalle', 'Importe'],
    'Pago': ['FechaHora', 'Monto'],
    'Usuario': ['NroTerminal', 'Nombre', 'Apellido', 'Activo'],
}

GRILLA_CONCEPTO = [
    ['Pago', 'Apuesta', 'Jugada', 'NumeroElegido'],
    ['Usuario', 'Juego', 'Sorteo', 'Premio'],
    [None, 'Loteria', 'NumeroGanador', 'Turno'],
]

# (origen, destino, tipo, mult_origen, mult_destino, nombre)
ASOC_CONCEPTO = [
    # (origen, destino, tipo, mult_origen, mult_destino, nombre, ajustes de posicion de los textos)
    ('Usuario', 'Apuesta', 'asociacion_simple', '1', '0..*', 'vende ▸', dict(lado_rol=-1, lado_mult_o=1)),
    ('Apuesta', 'Jugada', 'composicion_simple', '1', '1..*', 'contiene ▸', {}),
    ('Jugada', 'NumeroElegido', 'composicion_simple', '1', '1..*', 'se juega con ▸', {}),
    ('Jugada', 'Sorteo', 'asociacion_simple', '0..*', '1', 'apuesta a ▾', {}),
    ('Sorteo', 'Juego', 'asociacion_simple', '0..*', '1', '◂ es de', {}),
    ('Apuesta', 'Juego', 'asociacion_simple', '0..*', '1', '/es de ▾', {}),
    ('Sorteo', 'NumeroGanador', 'composicion_simple', '1', '0..*', 'produce ▾', {}),
    ('Sorteo', 'Loteria', 'asociacion_simple', '0..*', '0..1', 'organiza ▸', {}),
    ('Sorteo', 'Turno', 'asociacion_simple', '0..*', '0..1', 'se juega en ▸', {}),
    ('Premio', 'Jugada', 'asociacion_simple', '0..1', '1', 'obtiene ▸', {}),
    ('Pago', 'Apuesta', 'asociacion_simple', '0..1', '1', '◂ se cobra con', {}),
    ('Pago', 'Usuario', 'asociacion_simple', '0..*', '1', 'registra ▴', dict(lado_rol=-1, lado_nombre=-2)),
]

DESCRIPCION_CONCEPTO = {
    'Juego': 'Juego que vende la agencia (Quiniela, Quiniela Plus, Quini 6, Loto 5, Telekino). Define las reglas: '
             'cuántos números se eligen, de qué rango, cuántos salen en el sorteo, si se juega a una posición, si '
             'se sortea por lotería y turno y el precio de la jugada.',
    'Loteria': 'Ente que organiza los sorteos de Quiniela: Provincia, La Ciudad, Córdoba, Santa Fe y Entre Ríos.',
    'Turno': 'Horario de sorteo de la Quiniela: Primera, Matutina, Vespertina y Nocturna.',
    'Sorteo': 'Evento concreto de un juego en una fecha (en Quiniela, además, de una lotería y un turno). Pasa de '
              'Abierto a Cerrado y a Sorteado.',
    'NumeroGanador': 'Número que salió en un sorteo, con el puesto en que salió.',
    'Apuesta': 'El ticket que se lleva el apostador: código único (GUID), fecha y hora, total y estado (Vigente, '
               'Pagada o Anulada). Todas sus jugadas son del mismo juego.',
    'Jugada': 'Apuesta concreta a un sorteo dentro del ticket: tipo (Directa o Redoblona, solo Quiniela), si la '
              'eligió la máquina (automática), monto y orden.',
    'NumeroElegido': 'Número de una jugada; en la Quiniela lleva la posición buscada (rango 1, 5, 10 o 20).',
    'Premio': 'Lo que le corresponde a una jugada al compararla con el resultado de su sorteo. No se guarda: se '
              'calcula al consultar o pagar el ticket.',
    'Pago': 'Cobro de un ticket premiado: fecha y hora, importe y usuario que lo pagó. Un ticket se paga una vez.',
    'Usuario': 'Empleado de la agencia que opera el sistema (perfil Operador o Administrador): vende, paga, '
               'cancela y registra los resultados.',
}


def modelo_conceptual(titulo, clases, nombre=None):
    dc = DiagramaClases(titulo, nombre=nombre, conceptual=True)
    dc.GAP_X = 120
    dc.GAP_FILA = 96
    filas = [[c if c in clases else None for c in fila] for fila in GRILLA_CONCEPTO]
    dc.paquete('Dominio: Agencia de Quiniela', filas)
    for c in clases:
        dc.clase(c, attrs=ATRIBUTOS_CONCEPTO[c])
    for o, d, tipo, mo, md, nombre_rel, ajustes in ASOC_CONCEPTO:
        if o in clases and d in clases:
            kw = dict(lado_nombre=-1)
            kw.update(ajustes)
            dc.rel(tipo, o, d, mult_o=mo, mult_d=md, nombre=nombre_rel, **kw)
    return dc


# ------------------------------------------------------------------ procesos
PROCESOS_N01 = {}

PROCESOS_N01['PN01'] = dict(
    objetivo='Registrar la apuesta que el apostador solicita en el mostrador, cobrar su importe y entregarle el '
             'ticket con código único que la respalda; y atender, sobre ese ticket, los pedidos de reimpresión y '
             'de cancelación.',
    descripcion=[
        'La venta es el proceso central de la agencia. El apostador se acerca al mostrador y le dice al operador '
        'qué quiere jugar. En la Quiniela indica el número (de 1 a 4 cifras), la posición a la que lo juega (a la '
        'primera, a los 5, a los 10 o a los 20), el monto y en qué loterías y horarios del día; también puede '
        'pedir una redoblona: dos números de 2 cifras, cada uno con su posición. En Quiniela Plus, Quini 6, Loto 5 '
        'y Telekino elige la cantidad de números que fija el juego, o pide una jugada automática.',
        'El operador carga la apuesta en el sistema, que valida las reglas del juego y calcula el total. Cuando el '
        'apostador paga, el operador confirma la venta: el sistema asigna cada jugada al sorteo del día que '
        'corresponde (en la Quiniela, uno por cada combinación de lotería y horario), genera el código único del '
        'ticket, guarda todo en una sola operación y lo muestra para imprimirlo y entregarlo.',
        'Con el código del ticket se atienden después los pedidos sobre la venta: si el ticket se perdió o no se '
        'imprimió, se reimprime con la marca REIMPRESIÓN; si el apostador se arrepiente antes de que cierren sus '
        'sorteos, se cancela y se le devuelve el importe.',
    ],
    roles=[
        ('Apostador', 'Actor del negocio (externo)',
         'Persona que concurre a la agencia a jugar. El sistema no registra sus datos.',
         'Solicita la jugada, paga el importe y conserva el ticket. Puede pedir la reimpresión o la cancelación '
         'presentando el ticket.'),
        ('Operador', 'Trabajador del negocio y actor del sistema',
         'Empleado de mostrador (agenciero) que opera la terminal con el perfil Operador.',
         'Atiende al apostador, carga las jugadas, confirma la venta, cobra, imprime y entrega el ticket; '
         'reimprime y cancela tickets a pedido del apostador.'),
        ('Administrador', 'Trabajador del negocio y actor del sistema',
         'Dueño o encargado de la agencia, con el perfil Administrador. Es una especialización del Operador: '
         'puede hacer todo lo que hace el operador.',
         'Puede vender, reimprimir y cancelar igual que el operador.'),
    ],
    entradas=[
        'Pedido del apostador: juego; en Quiniela, número de 1 a 4 cifras, posición y monto (o los dos números de '
        'la redoblona con sus posiciones), loterías y horarios; en los demás juegos, los números elegidos o el '
        'pedido de jugada automática.',
        'Dinero que entrega el apostador.',
        'Catálogo de juegos, loterías y horarios activos, y sorteos del día.',
        'Para la post-venta: el código del ticket (completo o sus primeros caracteres).',
    ],
    comportamiento=[
        'Se valida cada jugada según las reglas del juego (RN-01, RN-02, RN-03 y RN-05).',
        'Se determinan los sorteos del día: en Quiniela, uno por cada lotería y horario elegidos; en los demás '
        'juegos, el sorteo del día del juego. Si un sorteo todavía no existe se crea abierto; si está cerrado o '
        'sorteado no se vende (RN-06).',
        'Se replica cada jugada en cada sorteo y se calcula el total (RN-04).',
        'Se genera el código único del ticket (RN-07), se guarda la apuesta con sus jugadas y números en una sola '
        'transacción, en estado Vigente, y se registra la venta en la bitácora (RN-16).',
        'Se muestra el ticket para imprimirlo y entregarlo.',
        'Reimpresión: se busca el ticket por su código, se registra la reimpresión en la bitácora y se muestra el '
        'ticket con la marca REIMPRESIÓN.',
        'Cancelación: se busca el ticket, se verifica que esté Vigente y que ninguno de sus sorteos haya cerrado '
        '(RN-08), se pasa a estado Anulada, se registra en la bitácora y se devuelve el importe.',
    ],
    salidas=[
        'Ticket (apuesta) en estado Vigente, con su código único, sus jugadas, sus números y el total.',
        'Sorteos del día creados en estado Abierto, si no existían.',
        'Ticket impreso o reimpreso (con la marca REIMPRESIÓN).',
        'Ticket cancelado (estado Anulada) e importe devuelto al apostador.',
        'Registros "Venta", "Reimpresion" y "Cancelacion" en la bitácora.',
    ],
    requisitos=['RF-01', 'RF-02', 'RF-03', 'RF-04', 'RF-05', 'RF-06'],
    reglas=['RN-01', 'RN-02', 'RN-03', 'RN-04', 'RN-05', 'RN-06', 'RN-07', 'RN-08', 'RN-16'],
    conceptos=['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno'],
    casos=['CU-N01', 'CU-N02', 'CU-N03', 'CU-N04', 'CU-N05', 'CU-N06'],
)

PROCESOS_N01['PN02'] = dict(
    objetivo='Cerrar la venta de cada sorteo a su hora, registrar el resultado oficial que informa la lotería (o '
             'simularlo) y publicar los números ganadores, para que los tickets se puedan liquidar.',
    descripcion=[
        'Cada sorteo es un evento concreto: la Quiniela tiene un sorteo por lotería y horario cada día, y los demás '
        'juegos uno por día. Mientras un sorteo está abierto se puede vender sobre él. Al llegar la hora, el '
        'administrador cierra las ventas: el sorteo deja de aceptar apuestas y ya no se pueden cancelar los tickets '
        'que lo incluyen.',
        'El sorteo lo realiza el ente oficial (la lotería), que publica el extracto: en la Quiniela, los 20 números '
        'de 4 cifras con su puesto. El administrador carga ese resultado en el sistema; si deja puestos vacíos, el '
        'sistema los completa al azar. Para practicar o hacer demostraciones también puede pedir que el sistema '
        'sortee al azar todos los sorteos pendientes.',
        'Una vez sorteado, el sorteo queda cerrado para siempre y sus números ganadores se pueden consultar en una '
        'pizarra, de a un sorteo o de varios a la vez (por ejemplo, todas las loterías de un horario).',
    ],
    roles=[
        ('Lotería', 'Actor del negocio (externo)',
         'Ente oficial que organiza el sorteo (Provincia, La Ciudad, Córdoba, Santa Fe, Entre Ríos o el ente del '
         'juego).',
         'Realiza el sorteo y publica el extracto con los números ganadores. No usa el sistema.'),
        ('Administrador', 'Trabajador del negocio y actor del sistema',
         'Dueño o encargado de la agencia, con el perfil Administrador.',
         'Cierra las ventas de los sorteos, carga el resultado publicado por la lotería o sortea al azar.'),
        ('Operador', 'Trabajador del negocio y actor del sistema',
         'Empleado de mostrador con el perfil Operador.',
         'Consulta los números ganadores para informárselos al apostador.'),
    ],
    entradas=[
        'Sorteos del día (juego, lotería, turno y fecha) y su estado.',
        'Extracto oficial publicado por la lotería: los números que salieron, por puesto.',
        'Pedido de cierre de ventas o de sorteo al azar del administrador.',
    ],
    comportamiento=[
        'Cierre de ventas: los sorteos abiertos elegidos pasan a Cerrado y se registra el cierre en la bitácora '
        '(RN-09, RN-16, RN-17).',
        'Carga del resultado: se validan los números cargados (rango del juego, sin repetir) y se completan al azar '
        'los puestos vacíos sin repetir las terminaciones cargadas a mano (RN-10).',
        'Se guardan los números ganadores y el sorteo pasa a Sorteado en una sola transacción; se registra "Sorteo '
        'realizado" en la bitácora, indicando si hubo números cargados a mano (RN-09, RN-16).',
        'Sorteo al azar: se realiza el mismo proceso con todos los puestos vacíos, para cada sorteo pendiente de la '
        'selección.',
        'Consulta: se muestran los números ganadores de uno o varios sorteos en una pizarra, o el estado del sorteo '
        'si todavía no se realizó.',
    ],
    salidas=[
        'Sorteos en estado Cerrado o Sorteado.',
        'Números ganadores de cada sorteo realizado, con su puesto.',
        'Pizarra de números ganadores.',
        'Registros "Cierre de sorteo" y "Sorteo realizado" en la bitácora.',
    ],
    requisitos=['RF-07', 'RF-08', 'RF-09', 'RF-10'],
    reglas=['RN-09', 'RN-10', 'RN-16', 'RN-17'],
    conceptos=['Juego', 'Sorteo', 'Loteria', 'Turno', 'NumeroGanador'],
    casos=['CU-N07', 'CU-N08', 'CU-N09', 'CU-N10'],
)

PROCESOS_N01['PN03'] = dict(
    objetivo='Verificar si un ticket resultó premiado y pagarle el premio al apostador una única vez.',
    descripcion=[
        'Después del sorteo, el apostador vuelve a la agencia con su ticket. El operador lo busca por su código y '
        'el sistema compara cada jugada con el resultado de su sorteo para calcular el premio.',
        'En la Quiniela, una jugada directa gana si alguno de los números ganadores, hasta la posición jugada, '
        'termina con las cifras jugadas; el premio es el monto multiplicado por 7, 70, 600 o 3.500 según la cantidad '
        'de cifras, dividido por la posición. En la redoblona, lo que gana el primer número se le apuesta al '
        'segundo. En los demás juegos el premio depende de la cantidad de aciertos.',
        'El ticket se paga solo si está vigente, si ya se realizaron todos sus sorteos y si tiene premio. Al pagar, '
        'el sistema registra el pago, deja el ticket como pagado y muestra el importe para entregárselo al '
        'apostador.',
    ],
    roles=[
        ('Apostador', 'Actor del negocio (externo)', 'Persona que presenta el ticket para cobrar.',
         'Presenta el ticket y recibe el premio.'),
        ('Operador', 'Trabajador del negocio y actor del sistema', 'Empleado de mostrador con el perfil Operador '
         '(o el Administrador, que lo especializa).',
         'Busca el ticket, verifica el premio, confirma el pago y entrega el dinero.'),
    ],
    entradas=[
        'Código del ticket que presenta el apostador.',
        'Jugadas del ticket y números ganadores de sus sorteos.',
    ],
    comportamiento=[
        'Se busca el ticket por su código (RN-07).',
        'Se calcula el premio de cada jugada cuyo sorteo ya se realizó (RN-11, RN-12, RN-13).',
        'Se verifica que el ticket se pueda pagar: vigente, con todos sus sorteos realizados y con premio (RN-14); '
        'si no, se informa el motivo (por ejemplo, los sorteos que faltan).',
        'Con la confirmación del operador se registra el pago y el ticket pasa a Pagada en una sola transacción; se '
        'registra el pago en la bitácora (RN-16).',
        'Se muestra el cartel de felicitaciones con el importe y las jugadas ganadoras.',
    ],
    salidas=[
        'Pago registrado por el total de los premios del ticket.',
        'Ticket en estado Pagada.',
        'Importe entregado al apostador.',
        'Registro "Pago" en la bitácora.',
    ],
    requisitos=['RF-11', 'RF-12', 'RF-03'],
    reglas=['RN-07', 'RN-11', 'RN-12', 'RN-13', 'RN-14', 'RN-16'],
    conceptos=['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'NumeroGanador', 'Premio', 'Pago', 'Juego'],
    casos=['CU-N11', 'CU-N04'],
)

PROCESOS_N01['PN04'] = dict(
    objetivo='Dar al administrador la información para controlar la operatoria de la agencia: qué se vendió, '
             'qué se pagó y en qué estado están los tickets, en el día, en la semana o en el período que elija.',
    descripcion=[
        'El administrador controla a diario la caja y la actividad de la agencia. Consulta las ventas de un período, '
        'de todos los juegos o de uno, con la cantidad de tickets vendidos, el total vendido y cuántos tickets se '
        'pagaron o se cancelaron; y consulta los pagos realizados con el total pagado.',
        'Desde cualquiera de los listados puede abrir un ticket para verlo completo. Los reportes no guardan '
        'información: se arman leyendo los tickets y los pagos registrados.',
    ],
    roles=[
        ('Administrador', 'Trabajador del negocio y actor del sistema',
         'Dueño o encargado de la agencia, con el perfil Administrador.',
         'Consulta las ventas y los pagos para controlar la caja y tomar decisiones. Tiene además accesos directos '
         '("Ventas en Linea" y "Reporte Semanal").'),
        ('Operador', 'Trabajador del negocio y actor del sistema', 'Empleado de mostrador con el perfil Operador.',
         'Puede consultar los mismos reportes desde el menú Consultas, por ejemplo para controlar su turno.'),
    ],
    entradas=[
        'Período (fecha desde y hasta) y, para las ventas, el juego.',
        'Tickets y pagos registrados.',
    ],
    comportamiento=[
        'Se valida que la fecha desde no sea posterior a la fecha hasta.',
        'Ventas: se listan los tickets del período (y del juego, si se eligió) del más reciente al más antiguo y se '
        'calculan los tickets vendidos y el total vendido sin los cancelados, y la cantidad de pagados y de '
        'cancelados (RN-15).',
        'Pagos: se listan los pagos del período y se calculan la cantidad y el total pagado.',
        'Con doble clic se muestra el ticket completo, con su estado.',
    ],
    salidas=[
        'Reporte de ventas en pantalla, con sus totales.',
        'Reporte de pagos en pantalla, con sus totales.',
        'Vista del ticket elegido.',
    ],
    requisitos=['RF-13', 'RF-14', 'RF-15'],
    reglas=['RN-15'],
    conceptos=['Usuario', 'Apuesta', 'Pago', 'Juego'],
    casos=['CU-N12', 'CU-N13'],
)

# Actividad del diagrama de proceso -> casos de uso del sistema que la soportan
TRAZABILIDAD = {
    'PN01': [
        ('Elegir las loterías y los horarios (Quiniela)', ['CU-N01']),
        ('Cargar las jugadas', ['CU-N01', 'CU-N02']),
        ('Confirmar la venta: validar, asignar sorteos, generar el código y registrar', ['CU-N01', 'CU-N02']),
        ('Imprimir el ticket', ['CU-N03']),
        ('Buscar el ticket por su código', ['CU-N04']),
        ('Reimprimir el ticket', ['CU-N05', 'CU-N03']),
        ('Cancelar el ticket', ['CU-N06']),
    ],
    'PN02': [
        ('Cerrar las ventas del sorteo', ['CU-N08']),
        ('Cargar el resultado del extracto oficial', ['CU-N09']),
        ('Sortear al azar (simulación)', ['CU-N10']),
        ('Consultar los números ganadores', ['CU-N07']),
    ],
    'PN03': [
        ('Buscar el ticket por su código', ['CU-N04']),
        ('Calcular el premio y verificar que se pueda pagar', ['CU-N11']),
        ('Registrar el pago', ['CU-N11']),
    ],
    'PN04': [
        ('Consultar las ventas del período', ['CU-N12']),
        ('Ver un ticket del reporte', ['CU-N12', 'CU-N13']),
        ('Consultar los pagos del período', ['CU-N13']),
    ],
}


# ------------------------------------------------------------------ diagramas del N01
def d_negocio_general():
    """Modelo de casos de uso del negocio: los cuatro procesos con sus actores y trabajadores."""
    u = DiagramaCU('uc Modelo de casos de uso del negocio - Agencia de Quiniela')
    u.limite('Agencia de Quiniela (negocio)', 230, 60, 600, 440)
    u.actor('apo', 'Apostador', 70, 140, '«actor del negocio»')
    u.actor('lot', 'Lotería', 70, 345, '«actor del negocio»')
    u.actor('ope', 'Operador', 900, 190, '«trabajador»')
    u.actor('adm', 'Administrador', 900, 470, '«trabajador»')
    # PN02 y PN04 tienen a los dos trabajadores: PN04 queda entre ellos y PN02, asi las lineas hacia PN02
    # pasan por arriba y por debajo de PN04 sin cruzarse
    u.caso('pn1', 'PN01\nVenta de apuestas', 400, 110, w=240, negocio=True)
    u.caso('pn3', 'PN03\nPago de premios', 400, 220, w=240, negocio=True)
    u.caso('pn2', 'PN02\nGestión de sorteos y resultados', 250, 340, w=270, negocio=True)
    u.caso('pn4', 'PN04\nControl de la gestión comercial', 550, 340, w=270, negocio=True)
    u.asociacion('apo', 'pn1')
    u.asociacion('apo', 'pn3')
    u.asociacion('lot', 'pn2')
    u.asociacion('pn1', 'ope')
    u.asociacion('pn3', 'ope')
    u.asociacion('pn2', 'ope')
    u.asociacion('pn2', 'adm')
    u.asociacion('pn4', 'ope')
    u.asociacion('pn4', 'adm')
    u.generalizacion('adm', 'ope')
    return u.dibujar()
