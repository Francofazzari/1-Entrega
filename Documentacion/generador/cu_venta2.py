# -*- coding: utf-8 -*-
"""PN01 Venta de apuestas: CU-N02 a CU-N06."""
import sd_venta as SD
from clases import DiagramaClases
from cu import DiagramaCU
from actividad import DiagramaFlujo
from n02_comun import diagrama_be, diagrama_der
from comun import SISTEMA_CORTO
from cu_venta import BD


# ================================================================== CU-N02 Vender apuesta de juego de numeros
def n02_cu():
    u = DiagramaCU('uc CU-N02 Vender apuesta de juego de números')
    u.limite(SISTEMA_CORTO, 220, 50, 640, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N02\nVender apuesta de juego de números', 262, 120, w=280)
    u.caso('imp', 'CU-N03\nImprimir ticket', 600, 255, w=210)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.extiende('imp', 'cu', condicion='el operador presiona "Imprimir"', punto='Ticket emitido (paso 12)',
               nota=(575, 95, 265))
    return u.dibujar()


def n02_clases_ui():
    dc = DiagramaClases('class CU-N02 (1) Clases de presentación de la venta', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormPrincipal', None], ['FormJuego', None], ['TicketRenderer', 'FormTicket']])
    dc.paquete('BLL', [['JuegoBLL', None], ['SesionManager', None], ['ApuestaBLL', 'Azar']])
    dc.clase('FormPrincipal', ops=['BtnJuego_Click', 'AbrirHijo'])
    dc.clase('FormJuego', attrs=['juego', 'seleccion', 'plantillas', 'seleccionAutomatica'],
             ops=['FormJuego', 'Formatear', 'CrearTablero', 'CrearColumnas', 'BtnNumero_Click', 'btnAutomatica_Click',
                  'btnLimpiar_Click', 'ActualizarSeleccion', 'btnAgregar_Click', 'btnQuitar_Click', 'ActualizarTotales',
                  'btnConfirmar_Click'])
    dc.clase('FormTicket', attrs=['apuesta', 'esReimpresion'], ops=['FormTicket', 'pnlPapel_Paint', 'btnCopiar_Click'])
    dc.clase('TicketRenderer', attrs=['Ancho'], ops=['Dibujar'], estereotipo='«static»')
    dc.clase('JuegoBLL', ops=['FormatearNumero'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('ApuestaBLL', ops=['Vender', 'GenerarNumerosAutomaticos', 'CodigoCorto', 'CodigoCompleto'])
    dc.clase('Azar', attrs=['generador'], ops=['Entre'], estereotipo='«static»')
    dc.rel('dependencia', 'FormPrincipal', 'FormJuego', nombre='«create»')
    dc.rel('dependencia', 'FormJuego', 'JuegoBLL', nombre='«use»')
    dc.rel('dependencia', 'FormJuego', 'SesionManager', nombre='«use»')
    dc.rel('asociacion', 'FormJuego', 'ApuestaBLL', rol='-bll', mult_d='1')
    dc.rel('dependencia', 'FormJuego', 'FormTicket', nombre='«create»')
    dc.rel('dependencia', 'FormTicket', 'TicketRenderer', nombre='«use»')
    dc.rel('dependencia', 'FormTicket', 'ApuestaBLL', nombre='«use»')
    dc.rel('dependencia', 'ApuestaBLL', 'Azar', nombre='«use»')
    return dc



def n02_clases_datos():
    dc = DiagramaClases('class CU-N02 (2) Clases de negocio y acceso a datos de la venta', horizontal=True)
    dc.paquete('BLL', [['SorteoBLL'], ['ApuestaBLL'], ['BitacoraBLL']])
    dc.paquete('DAL', [['SorteoDAL'], ['ApuestaDAL'], ['BitacoraDAL']])
    dc.clase('ApuestaBLL', attrs=['Vigente'], ops=['Vender', 'ValidarJugada', 'CodigoCorto'])
    dc.clase('SorteoBLL', attrs=['Abierto'], ops=['ObtenerParaVender', 'ObtenerOCrear', 'Buscar', 'Descripcion'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('ApuestaDAL', attrs=['conexion'], ops=['Insertar'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['Buscar', 'Insertar'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('asociacion', 'ApuestaBLL', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('asociacion', 'ApuestaBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'ApuestaBLL', 'ApuestaDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc



CU_N02 = dict(
    codigo='CU-N02',
    objetivo='Registrar la venta de una apuesta de Quiniela Plus, Quini 6, Loto 5 o Telekino —una o más jugadas con '
             'la cantidad de números que fija el juego, elegidos en un tablero o en forma automática— para el sorteo '
             'del día, y emitir el ticket con su código único (GUID).',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=[
        'El usuario inició sesión en el sistema (CU01 Iniciar sesión, T02).',
        'El juego está activo en el catálogo (JUEGOS) y tiene precio cargado.',
    ],
    post=[
        'Queda registrado el ticket (APUESTAS) en estado Vigente, con su código único (GUID), la fecha y hora, el '
        'vendedor y el total (la suma de los precios de las jugadas).',
        'Cada jugada queda registrada (JUGADAS) sobre el sorteo del día del juego, con el precio fijo, la marca de '
        'automática si la eligió el sistema y sus números (JUGADA_NUMERO) con ceros a la izquierda.',
        'Si el sorteo del día no existía, queda creado en estado Abierto, sin lotería ni turno (SORTEOS).',
        'Queda registrado el evento "Venta" en la bitácora.',
    ],
    post_fracaso=[
        'Si una jugada no es válida o el sorteo del día ya no está abierto, la venta no se registra y las jugadas '
        'quedan en pantalla. El ticket, sus jugadas y sus números se guardan en una única transacción.',
    ],
    disparadores=[
        'El apostador pide jugar a Quiniela Plus, Quini 6, Loto 5 o Telekino y el operador presiona el botón del juego '
        'en la sección VENDER del menú principal.',
    ],
    extension=[('Ticket emitido', 12, 'CU-N03', 'El operador presiona «Imprimir» en la pantalla del ticket.')],
    inclusion=[],
    flujo=[
        ('A', 'Presiona el botón del juego (Quiniela Plus, Quini 6, Loto 5 o Telekino) en la sección VENDER.'),
        ('S', 'Muestra la pantalla del juego: el tablero con todos los números del rango (en 5 columnas si son hasta '
              '25 números y en 10 si son más), cuántos números se eligen, el precio por jugada y la fecha del sorteo.'),
        ('A', 'Toca en el tablero los números que eligió el apostador.'),
        ('S', 'Marca cada número y muestra el contador “N de M elegidos”; al completar la cantidad del juego muestra '
              '“Jugada completa”.'),
        ('A', 'Presiona «AGREGAR JUGADA».'),
        ('S', 'Agrega la jugada a la lista "Jugadas del ticket" con su número de orden, los números (con ceros a la '
              'izquierda), la modalidad Manual y el precio; actualiza el total y limpia el tablero.'),
        ('A', 'Cuando terminó de cargar las jugadas (repitiendo los pasos 3 a 6), presiona «CONFIRMAR VENTA».'),
        ('S', 'Valida cada jugada: que el juego tenga precio, que tenga la cantidad de números del juego y que sean '
              'distintos y estén dentro del rango (RN-05).'),
        ('S', 'Obtiene el sorteo del día del juego: si no existe lo crea en estado Abierto, y verifica que esté '
              'abierto (RN-06).'),
        ('S', 'Arma el ticket: genera el código único (GUID), registra la fecha y hora, el vendedor y el estado '
              'Vigente; cada jugada toma el precio del juego y el total es la suma de los precios (RN-07).'),
        ('S', 'Guarda el ticket, sus jugadas y sus números en una única transacción y registra el evento "Venta" en la '
              'bitácora (RN-16).'),
        ('S', 'Muestra el ticket ("Venta registrada").'),
        ('A', 'Si hay que entregar el ticket en papel presiona «Imprimir» (punto de extensión Ticket emitido: se '
              'extiende con CU-N03 Imprimir ticket); después presiona «Cerrar».'),
        ('S', 'Vacía la lista de jugadas y queda listo para otra venta.'),
    ],
    alternativos=[
        ('3.a', 'Jugada automática', [
            'El operador presiona «Automática».',
            'El sistema elige al azar la cantidad de números del juego, distintos y dentro del rango, los marca en el '
            'tablero y muestra “Jugada automática completa”. La jugada se agrega con la modalidad Automática.'],
         'Continúa en el paso 5.'),
        ('3.b', 'Ya eligió todos los números', [
            'Si el operador toca un número cuando ya eligió todos, el sistema muestra “Ya elegiste los N números. '
            'Quitá uno para cambiarlo.” y no lo marca. Tocar un número marcado lo desmarca; «Limpiar» desmarca '
            'todos.'], 'Vuelve al paso 3.'),
        ('5.a', 'Jugada incompleta', [
            'El sistema muestra “Falta elegir 1 número.” o “Faltan elegir N números.” y no agrega la jugada.'],
         'Vuelve al paso 3.'),
        ('6.a', 'Quitar una jugada', [
            'El operador selecciona una jugada y presiona «Quitar jugada». El sistema la quita, renumera la lista y '
            'recalcula el total. Si no hay ninguna seleccionada muestra “Seleccioná en la lista la jugada que querés '
            'quitar.”'], 'Vuelve al paso 3.'),
        ('8.a', 'Sin jugadas o jugada inválida', [
            'El sistema muestra el motivo con el título “No se pudo registrar la venta”, por ejemplo “Cargá al menos '
            'una jugada antes de confirmar la venta.”. La validación de la capa de negocio controla además la '
            'cantidad, el rango y que no haya números repetidos, aunque el tablero ya lo impide.'],
         'Vuelve al paso 3.'),
        ('9.a', 'Sorteo del día cerrado o sorteado', [
            'El sistema muestra “El sorteo … ya no acepta apuestas (está cerrado).” (o “sorteado”) y no registra la '
            'venta.'], 'Fin del caso de uso.'),
        ('11.a', 'Error al guardar', [
            'El sistema revierte la transacción y muestra el error con el título “No se pudo registrar la venta”.'],
         'Vuelve al paso 7.'),
        ('12.a', 'Copiar el código', [
            'El operador presiona «Copiar código»: el sistema copia el código completo al portapapeles y muestra '
            '“Código copiado”.'], 'Continúa en el paso 13.'),
    ],
    reglas=['RN-05', 'RN-06', 'RN-07', 'RN-16'],
    diagrama_cu=n02_cu,
    flujo_alg=None,
    flujo_no_aplica='El armado y registro del ticket lo hace el mismo método que en la Quiniela (ApuestaBLL.Vender), '
                    'cuyo diagrama de flujo está en CU-N01: en este caso de uso se recorre la rama "No" (el juego no se '
                    'sortea por lotería y turno).',
    clases=[('Clases de presentación y de negocio de la venta', n02_clases_ui),
            ('Clases de negocio y de acceso a datos de la venta', n02_clases_datos),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N02 (3) Entidades (BE)',
                                 ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno',
                                  'Bitacora']))],
    secuencias=[
                ('Apertura del juego (pasos 1 y 2)',
                 SD.n02_sd_abrir),
                ('Elección de los números en el tablero (pasos 3 y 4 y flujo 3.b)',
                 SD.n02_sd_elegir),
                ('Jugada automática (flujo 3.a)',
                 SD.n02_sd_automatica),
                ('Agregado de la jugada al ticket (pasos 5 y 6 y flujo 5.a)',
                 SD.n02_sd_agregar),
                ('Quitar una jugada (flujo 6.a)',
                 SD.n02_sd_quitar),
                ('Confirmación de la venta (pasos 7 a 14 y flujos 8.a, 9.a y 11.a)',
                 SD.n02_sd_confirmar),
                ('Pantalla del ticket (pasos 12 y 13 y flujo 12.a)',
                 SD.n02_sd_ticket),
                ('Armado y registro del ticket en la capa de negocio (pasos 8 a 11)',
                 SD.n02_sd_vender),
                ('Obtención del sorteo del día (paso 9 y flujo 9.a)',
                 SD.n02_sd_sorteo),
                ('Guardado del ticket y registro en la bitácora (paso 11 y flujo 11.a)',
                 SD.n02_sd_guardar)],
    der=lambda: diagrama_der('DER CU-N02 Vender apuesta de juego de números',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS',
                              'TURNOS', 'Bitacora']),
    der_nota='USUARIOS no se escribe: el ticket guarda el Id del vendedor en APUESTAS.UsuarioId (clave foránea).',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno', 'Usuario',
                     'Bitacora'],
    prototipos=[],
)


# ================================================================== CU-N03 Imprimir ticket
def n03_cu():
    # el Operador no se asocia con CU-N03: lo usa desde las dos ventas y la reimpresion, que son los casos
    # que extiende (igual que en el modelo de casos de uso del sistema)
    u = DiagramaCU('uc CU-N03 Imprimir ticket')
    u.limite(SISTEMA_CORTO, 220, 50, 640, 400)
    u.actor('op', 'Operador', 70, 170)
    u.actor('adm', 'Administrador', 70, 330)
    u.caso('b1', 'CU-N01\nVender apuesta de Quiniela', 270, 80, w=250)
    u.caso('b2', 'CU-N02\nVender apuesta de juego de números', 270, 205, w=250)
    u.caso('b3', 'CU-N05\nReimprimir ticket', 270, 330, w=250)
    u.caso('cu', 'CU-N03\nImprimir ticket', 610, 205, w=220)
    for b in ('b1', 'b2', 'b3'):
        u.asociacion('op', b, punta=True)
    u.generalizacion('adm', 'op')
    u.extiende('cu', 'b1')
    u.extiende('cu', 'b2')
    u.extiende('cu', 'b3')
    u.notas.append(('extend', 'cu', 'b3',
                    'Condición de las tres extensiones: {el operador presiona «Imprimir» en la pantalla del ticket}\n'
                    'Puntos de extensión: Ticket emitido (CU-N01 y CU-N02) y Reimpresión en pantalla (CU-N05)',
                    (600, 320, 250)))
    return u.dibujar()


def n03_clases():
    dc = DiagramaClases('class CU-N03 (1) Clases de la impresión del ticket', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormTicket'], ['TicketRenderer']])
    dc.paquete('.NET Framework', [['PrintDocument'], ['PrintPreviewDialog']])
    dc.paquete('BLL', [['ApuestaBLL']])
    dc.clase('FormTicket', attrs=['apuesta', 'esReimpresion'], ops=['FormTicket', 'pnlPapel_Paint', 'btnImprimir_Click'])
    dc.clase('TicketRenderer', attrs=['Ancho'],
             ops=['Dibujar', 'Centrado', 'TituloSorteo', 'DibujarJugada', 'CodigoDeBarras', 'Sello'],
             estereotipo='«static»')
    dc.clase('PrintDocument', attrs=['DocumentName', 'PrintPage'], ops=['PrintDocument', 'Print'])
    dc.clase('PrintPreviewDialog', attrs=['Document'], ops=['PrintPreviewDialog', 'ShowDialog'])
    dc.clase('ApuestaBLL', attrs=['Vigente', 'Pagada'], ops=['CodigoCorto', 'CodigoCompleto', 'DescribirRango'])
    dc.rel('dependencia', 'FormTicket', 'PrintDocument', nombre='«create»')
    dc.rel('dependencia', 'FormTicket', 'PrintPreviewDialog', nombre='«create»')
    dc.rel('asociacion', 'PrintPreviewDialog', 'PrintDocument', rol='+Document', mult_d='1')
    dc.rel('dependencia', 'FormTicket', 'TicketRenderer', nombre='«use»')
    dc.rel('dependencia', 'FormTicket', 'ApuestaBLL', nombre='«use»', lado_o='arriba', lado_d='arriba')
    dc.rel('dependencia', 'TicketRenderer', 'ApuestaBLL', nombre='«use»', lado_o='abajo', lado_d='abajo')
    return dc



CU_N03 = dict(
    codigo='CU-N03',
    objetivo='Imprimir el ticket que está en pantalla —recién vendido o reimpreso— con una vista previa, en el mismo '
             'formato con que se muestra: lo que se ve es exactamente lo que se imprime.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=[
        'El ticket está en pantalla en la vista de venta (CU-N01 o CU-N02) o de reimpresión (CU-N05). En la vista de '
        'consulta, que se abre desde los reportes, el botón «Imprimir» no se muestra.',
    ],
    post=[
        'El ticket se envió a la impresora predeterminada (o el operador cerró la vista previa sin imprimir).',
        'El caso de uso no modifica datos: imprime el ticket ya cargado en memoria.',
    ],
    post_fracaso=['Si no hay impresora instalada se informa y no se imprime.'],
    disparadores=['El operador presiona «Imprimir» en la pantalla del ticket.'],
    extension=[],
    extiende=[('CU-N01', 'Ticket emitido', 'El operador presiona «Imprimir».'),
              ('CU-N02', 'Ticket emitido', 'El operador presiona «Imprimir».'),
              ('CU-N05', 'Reimpresión en pantalla', 'El operador presiona «Imprimir».')],
    inclusion=[],
    flujo=[
        ('A', 'Presiona «Imprimir» en la pantalla del ticket.'),
        ('S', 'Prepara el documento de impresión "Ticket XXXXXXXX" (código corto) y dibuja el ticket en la página con el '
              'mismo formato que en pantalla: nombre del juego, fecha y vendedor, jugadas agrupadas por sorteo, total, '
              'código de barras y códigos; en la reimpresión agrega la marca *** REIMPRESIÓN ***.'),
        ('S', 'Muestra la vista previa “Imprimir ticket XXXXXXXX”.'),
        ('A', 'Revisa la vista previa y presiona el botón de imprimir.'),
        ('S', 'Envía el ticket a la impresora predeterminada.'),
        ('A', 'Cierra la vista previa y vuelve a la pantalla del ticket.'),
    ],
    alternativos=[
        ('2.a', 'No hay impresora instalada', [
            'El sistema muestra “No hay ninguna impresora instalada en esta computadora.” y no imprime.'],
         'Fin del caso de uso.'),
        ('4.a', 'No imprime', ['El operador cierra la vista previa sin imprimir.'], 'Fin del caso de uso.'),
    ],
    reglas=[],
    diagrama_cu=n03_cu,
    flujo_alg=None,
    flujo_no_aplica='No corresponde: el caso de uso no tiene un algoritmo propio (el dibujo del ticket lo hace '
                    'TicketRenderer.Dibujar, igual que en pantalla).',
    clases=[('Clases que intervienen en la impresión', n03_clases),
            ('Clases de entidad (BE) que se imprimen',
             lambda: diagrama_be('class CU-N03 (2) Entidades (BE) que se imprimen',
                                 ['Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno']))],
    secuencias=[
                ('Impresión del ticket con vista previa (pasos 1 a 6 y flujos 2.a y 4.a)',
                 SD.n03_sd_imprimir),
                ('Dibujo del ticket en la página (paso 2)',
                 SD.n03_sd_dibujar)],
    der=lambda: diagrama_der('DER CU-N03 Imprimir ticket (tablas de origen de los datos impresos)',
                             ['APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS']),
    der_nota='El caso de uso no accede a la base de datos: imprime el ticket que ya está en memoria, leído al venderlo '
             '(CU-N01, CU-N02) o al buscarlo (CU-N04). Se muestran las tablas de las que provienen esos datos.',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno'],
    prototipos=[],
)


# ================================================================== CU-N04 Buscar ticket
def n04_cu():
    u = DiagramaCU('uc CU-N04 Buscar ticket')
    u.limite(SISTEMA_CORTO, 220, 50, 640, 400)
    u.actor('op', 'Operador', 70, 170)
    u.actor('adm', 'Administrador', 70, 330)
    u.caso('cu', 'CU-N04\nBuscar ticket', 600, 205, w=220)
    u.caso('b1', 'CU-N05\nReimprimir ticket', 280, 80, w=220)
    u.caso('b2', 'CU-N06\nCancelar ticket', 280, 205, w=220)
    u.caso('b3', 'CU-N11\nPagar ticket premiado', 280, 330, w=220)
    u.asociacion('op', 'b1')
    u.asociacion('op', 'b2')
    u.asociacion('op', 'b3')
    u.generalizacion('adm', 'op')
    u.incluye('b1', 'cu')
    u.incluye('b2', 'cu')
    u.incluye('b3', 'cu')
    return u.dibujar()


def n04_flujo():
    f = DiagramaFlujo('Diagrama de flujo CU-N04 - ApuestaBLL.BuscarPorCodigo', [300, 300])
    f.terminal('t0', 'Inicio: BuscarPorCodigo(texto)', 0, 0)
    f.proceso('p0', 'codigoTexto = texto sin espacios', 0, 1)
    f.pregunta('q0', '¿Está vacío?', 0, 2)
    f.proceso('e0', 'Error: "Ingresá el código del ticket."', 1, 2)
    f.pregunta('q1', '¿Es un GUID completo?', 0, 3)
    f.pregunta('q2', '¿Tiene 6 o más caracteres hexadecimales (o guiones)?', 1, 4, w=250)
    f.proceso('e2', 'Error: "El código no es válido. Ingresá el código completo o, al menos, sus primeros 6 caracteres."',
              1, 5, w=260)
    f.proceso('p2', 'ApuestaDAL.BuscarCodigosPorPrefijo(prefijo en mayúsculas): trae hasta 2 códigos', 0, 5)
    f.pregunta('q3', '¿Encontró alguno?', 0, 6)
    f.proceso('e3', 'Error: "No existe ningún ticket con ese código."', 1, 6)
    f.pregunta('q3b', '¿Encontró más de uno?', 0, 7)
    f.proceso('e4', 'Error: "Hay más de un ticket que empieza así. Ingresá el código completo."', 1, 7, w=260)
    f.decision('m', 0, 8)
    f.proceso('p4', 'ApuestaDAL.ObtenerPorCodigo(codigo): cabecera, jugadas con su sorteo y números', 0, 9)
    f.pregunta('q4', '¿Existe?', 0, 10)
    f.proceso('e5', 'Error: "No existe ningún ticket con ese código."', 1, 10)
    f.terminal('t1', 'Fin: devuelve la Apuesta', 0, 11)
    f.terminal('te', 'Fin con error: no se muestra ningún ticket', 1, 11)
    f.flujo('t0', 'p0')
    f.flujo('p0', 'q0')
    f.flujo('q0', 'e0', 'Sí')
    f.flujo('q0', 'q1', 'No')
    f.flujo('q1', 'q2', 'No', lado_a='der', lado_b='arriba')
    f.flujo('q1', 'm', 'Sí', lado_a='izq', lado_b='izq', vuelta='izq')
    f.flujo('q2', 'e2', 'No')
    f.flujo('q2', 'p2', 'Sí', lado_a='izq', lado_b='arriba')
    f.flujo('p2', 'q3')
    f.flujo('q3', 'e3', 'No', lado_a='der', lado_b='izq')
    f.flujo('q3', 'q3b', 'Sí')
    f.flujo('q3b', 'e4', 'Sí', lado_a='der', lado_b='izq')
    f.flujo('q3b', 'm', 'No')
    f.flujo('m', 'p4')
    f.flujo('p4', 'q4')
    f.flujo('q4', 't1', 'Sí')
    f.flujo('q4', 'e5', 'No', lado_a='der', lado_b='izq')
    f.flujo('e5', 'te')
    f.flujo('e0', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e2', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e3', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e4', 'te', vuelta='der', lado_a='der', lado_b='der')
    return f


def n04_clases():
    dc = DiagramaClases('class CU-N04 (1) Clases de la búsqueda del ticket', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormPrincipal'], ['FormOperacionTicket']])
    dc.paquete('BLL', [['ApuestaBLL']])
    dc.paquete('DAL', [['ApuestaDAL']])
    dc.clase('FormPrincipal', ops=['btnPagar_Click', 'btnCancelar_Click', 'btnReimprimir_Click', 'AbrirHijo'])
    dc.clase('FormOperacionTicket', attrs=['modo', 'apuesta'],
             ops=['FormOperacionTicket', 'CrearResumen', 'CrearColumnas', 'CargarTicketsDeHoy', 'dgvHoy_CellClick',
                  'btnBuscar_Click', 'txtCodigo_KeyDown', 'Buscar', 'MostrarTicket'])
    dc.clase('ApuestaBLL', ops=['ListarVentas', 'BuscarPorCodigo', 'CodigoCorto', 'CodigoCompleto'])
    dc.clase('ApuestaDAL', attrs=['conexion'], ops=['ListarVentas', 'BuscarCodigosPorPrefijo', 'ObtenerPorCodigo'])
    dc.rel('dependencia', 'FormPrincipal', 'FormOperacionTicket', nombre='«create»')
    dc.rel('asociacion', 'FormOperacionTicket', 'ApuestaBLL', rol='-apuestaBll', mult_d='1')
    dc.rel('asociacion', 'ApuestaBLL', 'ApuestaDAL', rol='-dal', mult_d='1')
    return dc



CU_N04 = dict(
    codigo='CU-N04',
    objetivo='Localizar un ticket por su código —completo o sus primeros caracteres, tal como figura impreso— o '
             'eligiéndolo de la lista de tickets del día, y mostrar su detalle para operar sobre él.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión y abrió «Pagar», «Cancelar» o «Reimprimir» desde OPERACIONES (este caso de uso '
         'está incluido en CU-N11, CU-N06 y CU-N05).'],
    post=['El ticket queda cargado en pantalla con su juego, fecha y hora, vendedor, código corto, total y estado, y '
          'sus jugadas con el sorteo de cada una y su estado. No se modifica ningún dato.'],
    post_fracaso=['Si el código no identifica a un único ticket, se informa el motivo y no se muestra ningún ticket.'],
    disparadores=['El operador escribe o pega el código del ticket y presiona «Buscar» (o Enter), o hace clic en un '
                  'ticket de la lista "Tickets de hoy".'],
    extension=[],
    inclusion=[],
    incluido_por=['CU-N05', 'CU-N06', 'CU-N11'],
    flujo=[
        ('A', 'Abre «Pagar», «Cancelar» o «Reimprimir» desde OPERACIONES.'),
        ('S', 'Muestra la pantalla de la operación con la búsqueda por código y la lista "Tickets de hoy" (hora, código '
              'corto, juego, total y estado), del más reciente al más antiguo.'),
        ('A', 'Escribe o pega el código del ticket y presiona «Buscar» (o Enter).'),
        ('S', 'Si el texto es un código completo (GUID) busca ese ticket; si no, verifica que tenga al menos 6 '
              'caracteres hexadecimales y busca los tickets cuyo código empieza así (RN-07).'),
        ('S', 'Lee el ticket completo: la cabecera con el vendedor, las jugadas con su sorteo (juego, lotería y turno) y '
              'los números elegidos.'),
        ('S', 'Muestra el detalle del ticket y sus jugadas con el estado de cada sorteo.'),
    ],
    alternativos=[
        ('3.a', 'Elegir el ticket de la lista', [
            'El operador hace clic en un ticket de "Tickets de hoy". El sistema completa el código completo y lo '
            'busca.'], 'Continúa en el paso 4.'),
        ('4.a', 'Código vacío', ['El sistema muestra en rojo “Ingresá el código del ticket.”'], 'Vuelve al paso 3.'),
        ('4.b', 'Código inválido', [
            'El sistema muestra “El código no es válido. Ingresá el código completo o, al menos, sus primeros 6 '
            'caracteres.”'], 'Vuelve al paso 3.'),
        ('4.c', 'No existe', ['El sistema muestra “No existe ningún ticket con ese código.”'], 'Vuelve al paso 3.'),
        ('4.d', 'Código ambiguo', [
            'Hay más de un ticket que empieza con esos caracteres: el sistema muestra “Hay más de un ticket que empieza '
            'así. Ingresá el código completo.”'], 'Vuelve al paso 3.'),
    ],
    reglas=['RN-07'],
    diagrama_cu=n04_cu,
    flujo_alg=('Búsqueda del ticket por código completo o por prefijo (ApuestaBLL.BuscarPorCodigo).', n04_flujo),
    clases=[('Clases que intervienen en la búsqueda del ticket', n04_clases),
            ('Clases de entidad (BE) que se leen',
             lambda: diagrama_be('class CU-N04 (2) Entidades (BE)',
                                 ['Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno']))],
    secuencias=[
                ('Apertura de la operación y lista de tickets de hoy (pasos 1 y 2)',
                 SD.n04_sd_abrir),
                ('Lectura de los tickets vendidos hoy (paso 2)',
                 SD.n04_sd_listar),
                ('Búsqueda del ticket y detalle (pasos 3 a 6 y flujo 3.a)',
                 SD.n04_sd_buscar),
                ('Búsqueda por código completo o por prefijo (pasos 4 y 5 y flujos 4.a a 4.d)',
                 SD.n04_sd_codigo)],
    der=lambda: diagrama_der('DER CU-N04 Buscar ticket',
                             ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS',
                              'TURNOS']),
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno'],
    prototipos=[],
)
