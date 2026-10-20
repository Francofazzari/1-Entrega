# -*- coding: utf-8 -*-
"""PN03 Pago de premios (CU-N11) y PN04 Control de la gestion comercial (CU-N12, CU-N13)."""
import sd_pagos as SD
from clases import DiagramaClases
from cu import DiagramaCU
from actividad import DiagramaFlujo
from n02_comun import diagrama_be, diagrama_der
from comun import SISTEMA_CORTO
from cu_venta import BD


# ================================================================== CU-N11 Pagar ticket premiado
def n11_cu():
    u = DiagramaCU('uc CU-N11 Pagar ticket premiado')
    u.limite(SISTEMA_CORTO, 220, 50, 560, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N11\nPagar ticket premiado', 270, 120, w=240)
    u.caso('bus', 'CU-N04\nBuscar ticket', 540, 240, w=210)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.incluye('cu', 'bus')
    return u.dibujar()


def n11_flujo():
    f = DiagramaFlujo('Diagrama de flujo CU-N11 - PremioBLL.Calcular (premio de cada jugada del ticket)', [320, 300])
    f.terminal('t0', 'Inicio: Calcular(a) — por cada jugada j del ticket', 0, 0)
    f.pregunta('q0', '¿El sorteo de j está Sorteado?', 0, 1)
    f.proceso('p0', 'Detalle = "Todavía no se sorteó"; importe 0', 1, 1)
    f.proceso('p1', 'Leer los números ganadores del sorteo (una vez por sorteo)', 0, 2)
    f.pregunta('q1', '¿El juego se juega a una posición (Quiniela)?', 0, 3, w=250)
    f.pregunta('q2', '¿Es redoblona?', 0, 4)
    f.proceso('p2', 'Aciertos = números de j que salieron; importe = monto × tabla de aciertos del juego', 1, 3, w=270)
    f.proceso('p3', 'El número "sale" si un ganador hasta su posición termina con sus cifras', 0, 5)
    f.pregunta('q3', '¿Salió?', 0, 6)
    f.proceso('p4', 'Importe = monto × (7, 70, 600 o 3.500 según las cifras) ÷ posición', 0, 7)
    f.proceso('p5', 'Detalle "El NN no salió …"; importe 0', 1, 6)
    f.pregunta('q4', '¿Salieron los dos números?', 1, 4)
    f.proceso('p6', 'Importe = monto × 70 ÷ posición del 1° × 70 ÷ posición del 2°', 1, 5, w=270)
    f.decision('m', 0, 8)
    f.proceso('p7', 'Redondear el importe a 2 decimales y agregar el Premio', 0, 9)
    f.terminal('t1', 'Fin: lista de premios (uno por jugada)', 0, 10)
    f.flujo('t0', 'q0')
    f.flujo('q0', 'p0', 'No')
    f.flujo('q0', 'p1', 'Sí')
    f.flujo('p1', 'q1')
    f.flujo('q1', 'p2', 'No', lado_a='der', lado_b='izq')
    f.flujo('q1', 'q2', 'Sí')
    f.flujo('q2', 'q4', 'Sí', lado_a='der', lado_b='izq')
    f.flujo('q2', 'p3', 'No')
    f.flujo('p3', 'q3')
    f.flujo('q3', 'p4', 'Sí')
    f.flujo('q3', 'p5', 'No', lado_a='der', lado_b='izq')
    f.flujo('q4', 'p6', 'Sí')
    f.flujo('q4', 'p5', 'No', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('p4', 'm')
    f.flujo('p6', 'm', vuelta='der', lado_a='der', lado_b='der', x_vuelta=None)
    f.flujo('p5', 'm', lado_a='abajo', lado_b='der')
    f.flujo('p2', 'm', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('p0', 'm', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('m', 'p7')
    f.flujo('p7', 't1')
    return f


def n11_clases_ui():
    dc = DiagramaClases('class CU-N11 (1) Clases de presentación y negocio del pago', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormOperacionTicket'], ['FormFelicitaciones']])
    dc.paquete('BLL', [['SesionManager', None], ['ApuestaBLL', None], ['PagoBLL', 'SorteoBLL'], ['PremioBLL', None]])
    dc.clase('FormOperacionTicket', attrs=['modo', 'apuesta', 'premios'],
             ops=['Buscar', 'MostrarTicket', 'btnAccion_Click', 'Confirmar', 'CargarTicketsDeHoy'])
    dc.clase('FormFelicitaciones', attrs=['apuesta', 'total', 'ganadores'],
             ops=['FormFelicitaciones', 'OnShown', 'timer_Tick', 'OnPaint'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('ApuestaBLL', attrs=['Vigente', 'Pagada', 'Anulada'], ops=['BuscarPorCodigo', 'CodigoCorto',
                                                                        'CodigoCompleto'])
    dc.clase('PagoBLL', ops=['MotivoNoPagable', 'Pagar'])
    dc.clase('PremioBLL', attrs=['PagaQuiniela', 'PagaPorAciertos'],
             ops=['Calcular', 'CalcularQuiniela', 'Buscar', 'CalcularPorAciertos'])
    dc.clase('SorteoBLL', attrs=['Sorteado'], ops=['ObtenerNumerosGanadores', 'Descripcion', 'DescripcionCorta'])
    dc.rel('dependencia', 'FormOperacionTicket', 'SesionManager', nombre='«use»')
    dc.rel('asociacion', 'FormOperacionTicket', 'ApuestaBLL', rol='-apuestaBll', mult_d='1')
    dc.rel('asociacion', 'FormOperacionTicket', 'PagoBLL', rol='-pagoBll', mult_d='1')
    dc.rel('asociacion', 'FormOperacionTicket', 'PremioBLL', rol='-premioBll', mult_d='1')
    dc.rel('dependencia', 'FormOperacionTicket', 'FormFelicitaciones', nombre='«create»')
    dc.rel('dependencia', 'FormFelicitaciones', 'ApuestaBLL', nombre='«use»')
    dc.rel('asociacion', 'PagoBLL', 'PremioBLL', rol='-premioBll', mult_d='1')
    dc.rel('dependencia', 'PagoBLL', 'ApuestaBLL', nombre='«use»')
    dc.rel('dependencia', 'PagoBLL', 'SorteoBLL', nombre='«use»')
    dc.rel('asociacion', 'PremioBLL', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    return dc



def n11_clases_datos():
    dc = DiagramaClases('class CU-N11 (2) Clases de negocio y acceso a datos del pago', horizontal=True)
    dc.paquete('BLL', [['PremioBLL', 'SorteoBLL'], ['PagoBLL', None], ['BitacoraBLL', None]])
    dc.paquete('DAL', [['SorteoDAL'], ['PagoDAL'], ['BitacoraDAL']])
    dc.clase('PremioBLL', ops=['Calcular'])
    dc.clase('PagoBLL', ops=['Pagar'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('SorteoBLL', ops=['ObtenerNumerosGanadores'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['ObtenerNumerosGanadores'])
    dc.clase('PagoDAL', attrs=['conexion'], ops=['Registrar'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('asociacion', 'PremioBLL', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('asociacion', 'PagoBLL', 'PremioBLL', rol='-premioBll', mult_d='1')
    dc.rel('asociacion', 'PagoBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'PagoBLL', 'PagoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc


CU_N11 = dict(
    codigo='CU-N11',
    objetivo='Verificar si un ticket resultó premiado —calculando el premio de cada jugada con el resultado de su '
             'sorteo— y pagarle el total al apostador una única vez, dejando el ticket como pagado.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión en el sistema.', 'El ticket existe (se lo encuentra con CU-N04 Buscar ticket).'],
    post=[
        'Queda registrado el pago (PAGOS) por el total de los premios del ticket, con el usuario que lo pagó y la fecha '
        'y hora; el ticket queda en estado Pagada. Las dos operaciones se hacen en una única transacción.',
        'Queda registrado el evento "Pago" en la bitácora con el juego, el código corto y el importe.',
        'Se muestra el cartel de felicitaciones con el importe a entregar.',
    ],
    post_fracaso=['Si el ticket no se puede pagar, o cambió de estado mientras se lo consultaba, no se registra nada.'],
    disparadores=['El apostador presenta el ticket para cobrar y el operador presiona «Pagar» en OPERACIONES.'],
    extension=[],
    inclusion=[('CU-N04', 3, 'Buscar el ticket por su código o en la lista de tickets de hoy.')],
    flujo=[
        ('A', 'Presiona «Pagar» en la sección OPERACIONES.'),
        ('S', 'Muestra la pantalla "Pagar ticket" con la búsqueda y la lista de tickets de hoy.'),
        ('A', 'Busca el ticket: se incluye CU-N04 Buscar ticket.'),
        ('S', 'Calcula el premio de cada jugada: si su sorteo no se realizó, "Todavía no se sorteó"; en la Quiniela '
              'compara el número con los ganadores hasta la posición jugada (RN-11, RN-12); en los demás juegos cuenta '
              'los aciertos (RN-13).'),
        ('S', 'Muestra en cada jugada el resultado y el premio; verifica que el ticket se pueda pagar (RN-14), informa '
              '“Ticket premiado: se le pagan $ X al cliente.” y habilita «PAGAR $ X».'),
        ('A', 'Presiona «PAGAR $ X».'),
        ('S', 'Pide confirmación: “¿Pagar $ X del ticket XXXXXXXX?”'),
        ('A', 'Confirma (Sí).'),
        ('S', 'Vuelve a calcular los premios y a verificar las reglas.'),
        ('S', 'Registra el pago por el total y pasa el ticket de Vigente a Pagada en una única transacción; registra el '
              'evento "Pago" en la bitácora (RN-16).'),
        ('S', 'Muestra el cartel “¡Felicitaciones!” con el código del ticket, el importe ganado y las jugadas '
              'ganadoras.'),
        ('A', 'Entrega el importe al apostador y presiona «ACEPTAR».'),
        ('S', 'Vuelve a leer el ticket y actualiza el detalle (estado Pagado) y la lista de tickets de hoy.'),
    ],
    alternativos=[
        ('3.a', 'El ticket no se encuentra', ['Se aplican los flujos alternativos de CU-N04 Buscar ticket.'],
         'Vuelve al paso 3.'),
        ('5.a', 'El ticket no se puede pagar', [
            'El sistema muestra el motivo en rojo y deja el botón deshabilitado: “El ticket está cancelado: no se puede '
            'pagar.”, “Este ticket ya fue pagado.”, “Todavía no se realizó el sorteo …”, “Todavía faltan N sorteos de '
            'este ticket: …” (nombra hasta cuatro) o “El ticket no tiene premio.”'], 'Fin del caso de uso.'),
        ('8.a', 'No confirma', ['El operador responde No: no se paga.'], 'Vuelve al paso 6.'),
        ('10.a', 'El ticket ya no está vigente', [
            'Otro usuario lo pagó o canceló mientras tanto: la transacción no cambia ninguna fila y se revierte; el '
            'sistema muestra “El ticket ya no está vigente. Buscalo de nuevo.” con el título “No se pudo completar la '
            'operación”.'], 'Fin del caso de uso.'),
    ],
    reglas=['RN-11', 'RN-12', 'RN-13', 'RN-14', 'RN-16'],
    diagrama_cu=n11_cu,
    flujo_alg=('Cálculo del premio de cada jugada (PremioBLL.Calcular).', n11_flujo),
    clases=[('Clases de presentación y de negocio del pago', n11_clases_ui),
            ('Clases de negocio y de acceso a datos del pago', n11_clases_datos),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N11 (3) Entidades (BE)',
                                 ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'NumeroGanador',
                                  'Premio', 'Pago', 'Bitacora'], notas=['Premio', 'Pago']))],
    secuencias=[
                ('Verificación de las reglas para pagar (pasos 3 a 5 y flujo 5.a)',
                 SD.n11_sd_verificar),
                ('Cálculo del premio de cada jugada (paso 4)',
                 SD.n11_sd_calcular),
                ('Lectura de los números ganadores de un sorteo (paso 4)',
                 SD.n11_sd_numeros),
                ('Confirmación del pago (pasos 6 a 13 y flujos 8.a y 10.a)',
                 SD.n11_sd_confirmar),
                ('Cartel de felicitaciones (pasos 11 y 12)',
                 SD.n11_sd_felicitaciones),
                ('Pago del ticket en la capa de negocio (pasos 9 y 10)',
                 SD.n11_sd_pagar),
                ('Registro del pago y de la bitácora (paso 10 y flujo 10.a)',
                 SD.n11_sd_registrar)],
    der=lambda: diagrama_der('DER CU-N11 Pagar ticket premiado',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'PAGOS', 'LOTERIAS', 'SORTEOS',
                              'TURNOS', 'Bitacora', 'JUEGOS', 'NUMERO_GANADOR']),
    der_nota='JUGADAS, JUGADA_NUMERO, SORTEOS, JUEGOS, LOTERIAS, TURNOS y USUARIOS se leen al buscar el ticket (CU-N04 Buscar ticket, incluido en este caso de uso).',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'NumeroGanador', 'Premio', 'Pago',
                     'Usuario', 'Bitacora'],
    prototipos=[],
)


# ================================================================== CU-N12 Consultar reporte de ventas
def _cu_reporte(codigo, nombre):
    u = DiagramaCU('uc %s %s' % (codigo, nombre))
    u.limite(SISTEMA_CORTO, 220, 50, 420, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', '%s\n%s' % (codigo, nombre), 290, 160, w=270)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    return u.dibujar()


def n12_clases():
    dc = DiagramaClases('class CU-N12 (2) Clases del reporte de ventas', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte', None], ['TicketRenderer', 'FormTicket']])
    dc.paquete('BLL', [['ApuestaBLL']])
    dc.paquete('DAL', [['ApuestaDAL']])
    dc.clase('FormReporte', attrs=['dtpVentasDesde', 'dtpVentasHasta', 'cmbVentasJuego', 'dgvVentas'],
             ops=['CargarVentas', 'AbrirTicket'])
    dc.clase('FormTicket', attrs=['apuesta', 'esReimpresion'], ops=['FormTicket', 'pnlPapel_Paint', 'btnCopiar_Click'])
    dc.clase('TicketRenderer', attrs=['Ancho'], ops=['Dibujar'], estereotipo='«static»')
    dc.clase('ApuestaBLL', attrs=['Pagada', 'Anulada'], ops=['ListarVentas', 'BuscarPorCodigo', 'CodigoCorto',
                                                         'CodigoCompleto'])
    dc.clase('ApuestaDAL', attrs=['conexion'], ops=['ListarVentas', 'ObtenerPorCodigo'])
    dc.rel('dependencia', 'FormReporte', 'FormTicket', nombre='«create»')
    dc.rel('asociacion', 'FormReporte', 'ApuestaBLL', rol='-apuestaBll', mult_d='1')
    dc.rel('dependencia', 'FormTicket', 'TicketRenderer', nombre='«use»')
    dc.rel('dependencia', 'FormTicket', 'ApuestaBLL', nombre='«use»')
    dc.rel('asociacion', 'ApuestaBLL', 'ApuestaDAL', rol='-dal', mult_d='1')
    return dc



def n12_clases_apertura():
    dc = DiagramaClases('class CU-N12 (1) Clases de la apertura de los reportes', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormPrincipal'], ['FormReporte']])
    dc.paquete('BLL', [['SesionManager'], ['JuegoBLL'], ['LoteriaBLL'], ['TurnoBLL']])
    dc.clase('FormPrincipal', ops=['btnReportes_Click', 'btnVentas_Click', 'btnReporte_Click', 'AbrirHijo'])
    dc.clase('FormReporte', attrs=['juegos', 'esAdmin'],
             ops=['FormReporte/0', 'FormReporte/2', 'ConstruirVentas', 'ConstruirPagos', 'ConstruirGanadores',
                  'CargarVentas', 'CargarPagos', 'MostrarVista'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('JuegoBLL', ops=['ObtenerActivos'])
    dc.clase('LoteriaBLL', ops=['ObtenerActivas'])
    dc.clase('TurnoBLL', ops=['ObtenerActivos'])
    dc.rel('dependencia', 'FormPrincipal', 'FormReporte', nombre='«create»')
    dc.rel('dependencia', 'FormReporte', 'SesionManager', nombre='«use»')
    dc.rel('dependencia', 'FormReporte', 'JuegoBLL', nombre='«use»')
    dc.rel('dependencia', 'FormReporte', 'LoteriaBLL', nombre='«use»')
    dc.rel('dependencia', 'FormReporte', 'TurnoBLL', nombre='«use»')
    return dc


CU_N12 = dict(
    codigo='CU-N12',
    objetivo='Consultar las ventas de un período —de todos los juegos o de uno— con la cantidad de tickets vendidos, el '
             'total vendido y cuántos se pagaron o se cancelaron, para controlar la caja y la actividad de la agencia.',
    actor='Operador (también el Administrador, que lo especializa y tiene los accesos «Ventas en Linea» y «Reporte '
          'Semanal»)',
    secundarios='—',
    pre=['El usuario inició sesión en el sistema.'],
    post=['Se muestran los tickets del período y sus totales. No se modifica ningún dato.'],
    post_fracaso=['Si el rango de fechas no es válido no se muestra ningún resultado nuevo.'],
    disparadores=['El usuario abre «Reportes» (CONSULTAS); el administrador también puede usar «Ventas en Linea» (ventas '
                  'de hoy) o «Reporte Semanal» (últimos 7 días) de la barra superior.'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Abre «Reportes» en la sección CONSULTAS.'),
        ('S', 'Muestra la pestaña «Ventas en línea» con Desde y Hasta en la fecha de hoy y el juego "Todos los juegos", '
              'y lista las ventas del día.'),
        ('A', 'Cambia las fechas o el juego y presiona «Buscar».'),
        ('S', 'Valida que la fecha Desde no sea posterior a la fecha Hasta.'),
        ('S', 'Lista los tickets vendidos en el período (y del juego elegido), del más reciente al más antiguo, con fecha '
              'y hora, código corto, juego, vendedor, total y estado.'),
        ('S', 'Calcula las tarjetas: Tickets vendidos y Total vendido, sin los cancelados (RN-15), Pagados y '
              'Cancelados.'),
    ],
    alternativos=[
        ('1.a', 'Accesos del administrador', [
            '«Ventas en Linea» abre el reporte con las ventas de hoy; «Reporte Semanal», con Desde = hoy − 6 días.'],
         'Continúa en el paso 2.'),
        ('4.a', 'Rango de fechas inválido', [
            'El sistema muestra “La fecha \'Desde\' no puede ser mayor que la fecha \'Hasta\'.” con el título “Ventas en '
            'línea”.'], 'Vuelve al paso 3.'),
        ('6.a', 'Ver un ticket', [
            'El usuario hace doble clic en un ticket. El sistema lo busca por su código y lo muestra en la vista de '
            'consulta: con su estado y sin el botón «Imprimir».'], 'Vuelve al paso 3.'),
    ],
    reglas=['RN-15'],
    diagrama_cu=lambda: _cu_reporte('CU-N12', 'Consultar reporte de ventas'),
    flujo_alg=None,
    flujo_no_aplica='No corresponde: el reporte filtra los tickets por período y juego y calcula los totales del paso 6.',
    clases=[('Clases que intervienen en la apertura de los reportes', n12_clases_apertura),
            ('Clases que intervienen en el reporte de ventas', n12_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N12 (3) Entidades (BE)', ['Apuesta', 'Juego', 'Jugada', 'Sorteo']))],
    secuencias=[
                ('Apertura de los reportes (pasos 1 y 2 y flujo 1.a)',
                 SD.n12_sd_abrir),
                ('Construcción de la pantalla de reportes (paso 2)',
                 SD.n12_sd_construir),
                ('Listado de las ventas del período (pasos 3 a 6 y flujo 4.a)',
                 SD.n12_sd_listar),
                ('Lectura de las ventas en la base de datos (pasos 4 y 5)',
                 SD.n12_sd_leer),
                ('Vista de un ticket del reporte (flujo 6.a)',
                 SD.n12_sd_ver),
                ('Pantalla del ticket en la vista de consulta (flujo 6.a)',
                 SD.n12_sd_ticket)],
    der=lambda: diagrama_der('DER CU-N12 Consultar reporte de ventas',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'SORTEOS', 'JUEGOS']),
    der_nota='La vista de un ticket (flujo 6.a) lee además JUGADA_NUMERO, LOTERIAS y TURNOS, igual que CU-N04 Buscar '
             'ticket.',
    correspondencia=['Apuesta', 'Jugada', 'Sorteo', 'Juego'],
    prototipos=[],
)


# ================================================================== CU-N13 Consultar reporte de pagos
def n13_clases():
    dc = DiagramaClases('class CU-N13 (1) Clases del reporte de pagos', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte']])
    dc.paquete('BLL', [['PagoBLL']])
    dc.paquete('DAL', [['PagoDAL']])
    dc.clase('FormReporte', attrs=['dtpPagosDesde', 'dtpPagosHasta', 'dgvPagos', 'lblCantidadPagos', 'lblTotalPagado'],
             ops=['btnPagos_Click', 'MostrarVista', 'ConstruirPagos', 'CargarPagos', 'AbrirTicket'])
    dc.clase('PagoBLL', ops=['Listar'])
    dc.clase('PagoDAL', attrs=['conexion'], ops=['Listar'])
    dc.rel('asociacion', 'FormReporte', 'PagoBLL', rol='-pagoBll', mult_d='1')
    dc.rel('asociacion', 'PagoBLL', 'PagoDAL', rol='-dal', mult_d='1')
    return dc


CU_N13 = dict(
    codigo='CU-N13',
    objetivo='Consultar los pagos de premios realizados en un período, con la cantidad de pagos y el total pagado, para '
             'controlar lo que salió de la caja.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión y abrió «Reportes» (CONSULTAS).'],
    post=['Se muestran los pagos del período y sus totales. No se modifica ningún dato.'],
    post_fracaso=['Si el rango de fechas no es válido no se muestra ningún resultado nuevo.'],
    disparadores=['El usuario presiona la pestaña «Pagos en línea» del reporte.'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Presiona la pestaña «Pagos en línea».'),
        ('S', 'Muestra los pagos con Desde y Hasta en la fecha de hoy (o desde la fecha con que se abrió el reporte).'),
        ('A', 'Cambia las fechas y presiona «Buscar».'),
        ('S', 'Valida que la fecha Desde no sea posterior a la fecha Hasta.'),
        ('S', 'Lista los pagos del período, del más reciente al más antiguo, con fecha y hora, código corto del ticket, '
              'juego, usuario que pagó y monto.'),
        ('S', 'Calcula las tarjetas: Pagos realizados y Total pagado.'),
    ],
    alternativos=[
        ('4.a', 'Rango de fechas inválido', [
            'El sistema muestra “La fecha \'Desde\' no puede ser mayor que la fecha \'Hasta\'.” con el título “Pagos en '
            'línea”.'], 'Vuelve al paso 3.'),
        ('6.a', 'Ver el ticket de un pago', [
            'El usuario hace doble clic en un pago y el sistema muestra el ticket en la vista de consulta (como en '
            'CU-N12, flujo 6.a).'], 'Vuelve al paso 3.'),
    ],
    reglas=[],
    diagrama_cu=lambda: _cu_reporte('CU-N13', 'Consultar reporte de pagos'),
    flujo_alg=None,
    flujo_no_aplica='No corresponde: el reporte filtra los pagos por período y suma sus montos.',
    clases=[('Clases que intervienen en el reporte de pagos', n13_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N13 (2) Entidades (BE)', ['Pago'], notas=['Pago']))],
    secuencias=[
                ('Listado de los pagos del período (pasos 1 a 6 y flujos 4.a y 6.a)',
                 SD.n13_sd_listar),
                ('Lectura de los pagos en la base de datos (pasos 4 y 5)',
                 SD.n13_sd_leer)],
    der=lambda: diagrama_der('DER CU-N13 Consultar reporte de pagos',
                             ['USUARIOS', 'APUESTAS', 'PAGOS', 'JUGADAS', 'SORTEOS', 'JUEGOS']),
    der_nota='La vista del ticket de un pago (flujo 6.a) lee además JUGADA_NUMERO, LOTERIAS y TURNOS, igual que CU-N04.',
    correspondencia=['Pago'],
    prototipos=[],
)
