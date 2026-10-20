# -*- coding: utf-8 -*-
"""PN01 Venta de apuestas: CU-N05 Reimprimir ticket y CU-N06 Cancelar ticket."""
import sd_venta as SD
from clases import DiagramaClases
from cu import DiagramaCU
from n02_comun import diagrama_be, diagrama_der
from comun import SISTEMA_CORTO
from cu_venta import BD


# ================================================================== CU-N05 Reimprimir ticket
def n05_cu():
    u = DiagramaCU('uc CU-N05 Reimprimir ticket')
    u.limite(SISTEMA_CORTO, 220, 50, 640, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N05\nReimprimir ticket', 280, 120, w=230)
    u.caso('bus', 'CU-N04\nBuscar ticket', 600, 95, w=210)
    u.caso('imp', 'CU-N03\nImprimir ticket', 600, 255, w=210)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.incluye('cu', 'bus')
    u.extiende('imp', 'cu', condicion='el operador presiona "Imprimir"', punto='Reimpresión en pantalla (paso 7)',
               nota=(300, 250, 230))
    return u.dibujar()


def n05_clases():
    dc = DiagramaClases('class CU-N05 (1) Clases de la reimpresión del ticket', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormOperacionTicket', None], ['TicketRenderer', 'FormTicket']])
    dc.paquete('BLL', [['SesionManager'], ['ApuestaBLL'], ['BitacoraBLL']])
    dc.paquete('DAL', [['BitacoraDAL']])
    dc.clase('FormOperacionTicket', attrs=['modo', 'apuesta'],
             ops=['Buscar', 'MostrarTicket', 'btnAccion_Click', 'CargarTicketsDeHoy'])
    dc.clase('FormTicket', attrs=['apuesta', 'esReimpresion'], ops=['FormTicket', 'pnlPapel_Paint', 'btnCopiar_Click'])
    dc.clase('TicketRenderer', attrs=['Ancho'], ops=['Dibujar'], estereotipo='«static»')
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('ApuestaBLL', attrs=['Vigente'], ops=['RegistrarReimpresion', 'BuscarPorCodigo', 'CodigoCorto',
                                                   'CodigoCompleto', 'NombreJuego'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('dependencia', 'FormOperacionTicket', 'SesionManager', nombre='«use»')
    dc.rel('asociacion', 'FormOperacionTicket', 'ApuestaBLL', rol='-apuestaBll', mult_d='1')
    dc.rel('dependencia', 'FormOperacionTicket', 'FormTicket', nombre='«create»')
    dc.rel('dependencia', 'FormTicket', 'TicketRenderer', nombre='«use»')
    dc.rel('dependencia', 'FormTicket', 'ApuestaBLL', nombre='«use»')
    dc.rel('asociacion', 'ApuestaBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc



CU_N05 = dict(
    codigo='CU-N05',
    objetivo='Volver a emitir un ticket ya vendido —por ejemplo, si falló la impresora o el apostador lo perdió— '
             'marcado como REIMPRESIÓN, dejando constancia en la bitácora.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión en el sistema.', 'El ticket existe (se lo encuentra con CU-N04 Buscar ticket).'],
    post=[
        'Queda registrado el evento "Reimpresion" en la bitácora con el juego y el código corto del ticket.',
        'El ticket se muestra con la marca *** REIMPRESIÓN *** (y con el sello PAGADO o CANCELADO si corresponde), '
        'listo para imprimirse. El ticket no cambia: mismo código, mismas jugadas, mismo estado.',
    ],
    post_fracaso=['Si no se encuentra el ticket no se registra nada.'],
    disparadores=['El apostador pide una copia de su ticket y el operador presiona «Reimprimir» en OPERACIONES.'],
    extension=[('Reimpresión en pantalla', 7, 'CU-N03', 'El operador presiona «Imprimir» en la pantalla del ticket.')],
    inclusion=[('CU-N04', 3, 'Buscar el ticket por su código o en la lista de tickets de hoy.')],
    flujo=[
        ('A', 'Presiona «Reimprimir» en la sección OPERACIONES.'),
        ('S', 'Muestra la pantalla "Reimprimir ticket" con la búsqueda y la lista de tickets de hoy.'),
        ('A', 'Busca el ticket: se incluye CU-N04 Buscar ticket.'),
        ('S', 'Muestra el detalle del ticket, informa “La reimpresión sale marcada como REIMPRESIÓN.” (si el ticket '
              'está pagado o cancelado, que sale con ese sello) y habilita «REIMPRIMIR TICKET».'),
        ('A', 'Presiona «REIMPRIMIR TICKET».'),
        ('S', 'Registra el evento "Reimpresion" en la bitácora (RN-16).'),
        ('S', 'Muestra el ticket ("Reimpresión del ticket") con la marca *** REIMPRESIÓN ***.'),
        ('A', 'Si hay que entregar la copia en papel presiona «Imprimir» (punto de extensión Reimpresión en '
              'pantalla: se extiende con CU-N03 Imprimir ticket); después presiona «Cerrar».'),
        ('S', 'Vuelve a leer el ticket y actualiza el detalle y la lista de tickets de hoy.'),
    ],
    alternativos=[
        ('3.a', 'El ticket no se encuentra', ['Se aplican los flujos alternativos de CU-N04 Buscar ticket.'],
         'Vuelve al paso 3.'),
        ('6.a', 'Error al registrar', [
            'El sistema muestra el error con el título “No se pudo completar la operación” y no muestra el ticket.'],
         'Fin del caso de uso.'),
    ],
    reglas=['RN-16'],
    diagrama_cu=n05_cu,
    flujo_alg=None,
    flujo_no_aplica='No corresponde: el caso de uso no tiene un algoritmo complejo.',
    clases=[('Clases que intervienen en la reimpresión', n05_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N05 (2) Entidades (BE)',
                                 ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno',
                                  'Bitacora']))],
    secuencias=[
                ('Reimpresión del ticket (pasos 3 a 9 y flujo 6.a)',
                 SD.n05_sd_reimprimir),
                ('Pantalla de la reimpresión (pasos 7 y 8)',
                 SD.n05_sd_ticket),
                ('Registro de la reimpresión en la bitácora (paso 6)',
                 SD.n05_sd_registrar)],
    der=lambda: diagrama_der('DER CU-N05 Reimprimir ticket',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS',
                              'TURNOS', 'Bitacora']),
    der_nota='JUGADAS, JUGADA_NUMERO, SORTEOS, JUEGOS, LOTERIAS, TURNOS y USUARIOS se leen al buscar el ticket (CU-N04 Buscar ticket, incluido en este caso de uso).',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno', 'Usuario',
                     'Bitacora'],
    prototipos=[],
)


# ================================================================== CU-N06 Cancelar ticket
def n06_cu():
    u = DiagramaCU('uc CU-N06 Cancelar ticket')
    u.limite(SISTEMA_CORTO, 220, 50, 560, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N06\nCancelar ticket', 280, 120, w=220)
    u.caso('bus', 'CU-N04\nBuscar ticket', 540, 240, w=210)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.incluye('cu', 'bus')
    return u.dibujar()


def n06_clases():
    dc = DiagramaClases('class CU-N06 (1) Clases de la cancelación del ticket', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormOperacionTicket']])
    dc.paquete('BLL', [['SesionManager'], ['ApuestaBLL'], ['BitacoraBLL'], ['SorteoBLL']])
    dc.paquete('DAL', [['ApuestaDAL'], ['BitacoraDAL']])
    dc.clase('FormOperacionTicket', attrs=['modo', 'apuesta'],
             ops=['Buscar', 'MostrarTicket', 'btnAccion_Click', 'Confirmar', 'CargarTicketsDeHoy'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('ApuestaBLL', attrs=['Vigente', 'Anulada'],
             ops=['MotivoNoAnulable', 'Anular', 'BuscarPorCodigo', 'CodigoCorto', 'CodigoCompleto', 'NombreJuego'])
    dc.clase('SorteoBLL', attrs=['Abierto'], ops=['Descripcion'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('ApuestaDAL', attrs=['conexion'], ops=['CambiarEstado'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('asociacion', 'FormOperacionTicket', 'ApuestaBLL', rol='-apuestaBll', mult_d='1')
    dc.rel('dependencia', 'FormOperacionTicket', 'SesionManager', nombre='«use»')
    dc.rel('dependencia', 'ApuestaBLL', 'SorteoBLL', nombre='«use»', lado_o='izq', lado_d='izq')
    dc.rel('asociacion', 'ApuestaBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'ApuestaBLL', 'ApuestaDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc



CU_N06 = dict(
    codigo='CU-N06',
    objetivo='Anular un ticket vigente cuando el apostador se arrepiente antes de que cierren sus sorteos, dejarlo en '
             'estado Anulada (se muestra como Cancelado) y devolverle el importe.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión en el sistema.', 'El ticket existe (se lo encuentra con CU-N04 Buscar ticket).'],
    post=[
        'El ticket queda en estado Anulada (se muestra como "Cancelado"): ya no se puede pagar ni volver a cancelar.',
        'Queda registrado el evento "Cancelacion" en la bitácora con el juego, el código corto y el importe.',
        'El operador le devuelve el importe al apostador (fuera del sistema).',
    ],
    post_fracaso=['Si el ticket no se puede cancelar, o cambió de estado mientras se lo consultaba, no se modifica.'],
    disparadores=['El apostador pide anular su ticket y el operador presiona «Cancelar» en OPERACIONES.'],
    extension=[],
    inclusion=[('CU-N04', 3, 'Buscar el ticket por su código o en la lista de tickets de hoy.')],
    flujo=[
        ('A', 'Presiona «Cancelar» en la sección OPERACIONES.'),
        ('S', 'Muestra la pantalla "Cancelar ticket" con la búsqueda y la lista de tickets de hoy.'),
        ('A', 'Busca el ticket: se incluye CU-N04 Buscar ticket.'),
        ('S', 'Verifica que el ticket se pueda cancelar: que esté Vigente y que todos sus sorteos sigan abiertos '
              '(RN-08). Informa “El ticket se puede cancelar: se le devuelven $ X al cliente.” y habilita «CANCELAR '
              'TICKET».'),
        ('A', 'Presiona «CANCELAR TICKET».'),
        ('S', 'Pide confirmación: “¿Cancelar el ticket XXXXXXXX por $ X? Esta operación no se puede deshacer.”'),
        ('A', 'Confirma (Sí).'),
        ('S', 'Vuelve a verificar las reglas y pasa el ticket a Anulada solo si sigue Vigente; registra el evento '
              '"Cancelacion" en la bitácora (RN-16).'),
        ('S', 'Informa “El ticket XXXXXXXX quedó cancelado. Devolvé $ X al cliente.”'),
        ('A', 'Devuelve el importe al apostador y cierra el mensaje.'),
        ('S', 'Vuelve a leer el ticket y actualiza el detalle (estado Cancelado) y la lista de tickets de hoy.'),
    ],
    alternativos=[
        ('3.a', 'El ticket no se encuentra', ['Se aplican los flujos alternativos de CU-N04 Buscar ticket.'],
         'Vuelve al paso 3.'),
        ('4.a', 'El ticket no se puede cancelar', [
            'El sistema muestra el motivo en rojo y deja «CANCELAR TICKET» deshabilitado: “Este ticket ya fue '
            'cancelado.”, “Este ticket ya fue pagado: no se puede cancelar.” o “El sorteo … ya cerró: el ticket no se '
            'puede cancelar.”'], 'Fin del caso de uso.'),
        ('7.a', 'No confirma', ['El operador responde No: no se cancela el ticket.'], 'Vuelve al paso 5.'),
        ('8.a', 'El ticket cambió de estado', [
            'Otro usuario lo pagó o canceló mientras tanto: el sistema muestra “El ticket cambió de estado mientras lo '
            'consultabas. Buscalo de nuevo.” con el título “No se pudo completar la operación”.'],
         'Fin del caso de uso.'),
    ],
    reglas=['RN-08', 'RN-16'],
    diagrama_cu=n06_cu,
    flujo_alg=None,
    flujo_no_aplica='No corresponde: las reglas para cancelar (estado del ticket y de sus sorteos) se enumeran en el '
                    'paso 4 y en el flujo 4.a.',
    clases=[('Clases que intervienen en la cancelación', n06_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N06 (2) Entidades (BE)',
                                 ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno',
                                  'Bitacora']))],
    secuencias=[
                ('Verificación de las reglas para cancelar (pasos 3 y 4 y flujo 4.a)',
                 SD.n06_sd_verificar),
                ('Confirmación de la cancelación (pasos 5 a 11 y flujos 7.a y 8.a)',
                 SD.n06_sd_confirmar),
                ('Anulación del ticket y registro en la bitácora (paso 8 y flujo 8.a)',
                 SD.n06_sd_anular)],
    der=lambda: diagrama_der('DER CU-N06 Cancelar ticket',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS',
                              'TURNOS', 'Bitacora']),
    der_nota='JUGADAS, JUGADA_NUMERO, SORTEOS, JUEGOS, LOTERIAS, TURNOS y USUARIOS se leen al buscar el ticket (CU-N04 Buscar ticket, incluido en este caso de uso).',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno', 'Usuario',
                     'Bitacora'],
    prototipos=[],
)
