# -*- coding: utf-8 -*-
"""PN01 Venta de apuestas: especificaciones CU-N01 a CU-N06 con sus diagramas."""
import sd_venta as SD
from clases import DiagramaClases
from cu import DiagramaCU
from actividad import DiagramaFlujo
from n02_comun import diagrama_be, diagrama_der
from comun import SISTEMA_CORTO

BD = 'AgenciaQuiniela'


# ================================================================== CU-N01 Vender apuesta de Quiniela
def n01_cu():
    u = DiagramaCU('uc CU-N01 Vender apuesta de Quiniela')
    u.limite(SISTEMA_CORTO, 220, 50, 640, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N01\nVender apuesta de Quiniela', 270, 120, w=260)
    u.caso('imp', 'CU-N03\nImprimir ticket', 600, 255, w=210)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.extiende('imp', 'cu', condicion='el operador presiona "Imprimir"', punto='Ticket emitido (paso 14)',
               nota=(560, 95, 270))
    return u.dibujar()


def n01_flujo():
    f = DiagramaFlujo('Diagrama de flujo CU-N01 - ApuestaBLL.Vender (armado y registro del ticket)', [300, 300])
    f.terminal('t0', 'Inicio: Vender(juego, loterias, turnos, plantillas, vendedor)', 0, 0)
    f.pregunta('q0', '¿Hay al menos una jugada?', 0, 1)
    f.proceso('e0', 'Error: "Cargá al menos una jugada antes de confirmar la venta."', 1, 1)
    f.proceso('p1', 'Validar cada jugada con las reglas del juego (ValidarJugada)', 0, 2)
    f.pregunta('q1', '¿Todas las jugadas son válidas?', 0, 3)
    f.proceso('e1', 'Error con el motivo (monto, cifras, posición, cantidad o rango de números)', 1, 3)
    f.pregunta('q2', '¿El juego se sortea por lotería y turno?', 0, 4)
    f.proceso('p2', 'Por cada lotería × turno elegidos: ObtenerParaVender (crea el sorteo del día si no existe)', 0, 5)
    f.proceso('p2b', 'Un único sorteo: el del día del juego, sin lotería ni turno', 1, 5)
    f.decision('m', 0, 6)
    f.pregunta('q3', '¿Todos los sorteos están abiertos?', 0, 7)
    f.proceso('e3', 'Error: "El sorteo … ya no acepta apuestas (está …)."', 1, 7)
    f.proceso('p3', 'Crear la Apuesta: CodigoTicket = Guid.NewGuid(), FechaHora = ahora, Estado = Vigente', 0, 8)
    f.proceso('p4', 'Por cada sorteo y cada plantilla agregar una Jugada (Quiniela: con tipo y posición; demás juegos: '
                    'con el precio del juego y los números con ceros a la izquierda)', 0, 9, w=260)
    f.proceso('p5', 'Total = suma de los montos de las jugadas', 0, 10)
    f.proceso('p6', 'ApuestaDAL.Insertar(a): ticket, jugadas y números en una transacción', 0, 11)
    f.proceso('p7', 'BitacoraBLL.Registrar(..., "Venta", ...)', 0, 12)
    f.terminal('t1', 'Fin: devuelve la Apuesta', 0, 13)
    f.terminal('te', 'Fin con error: no se registra la venta', 1, 13)
    f.flujo('t0', 'q0')
    f.flujo('q0', 'e0', 'No')
    f.flujo('q0', 'p1', 'Sí')
    f.flujo('p1', 'q1')
    f.flujo('q1', 'e1', 'No')
    f.flujo('q1', 'q2', 'Sí')
    f.flujo('q2', 'p2', 'Sí')
    f.flujo('q2', 'p2b', 'No', lado_a='der', lado_b='arriba')
    f.flujo('p2', 'm')
    f.flujo('p2b', 'm', lado_a='abajo', lado_b='der')
    f.flujo('m', 'q3')
    f.flujo('q3', 'e3', 'No', lado_a='der', lado_b='izq')
    f.flujo('q3', 'p3', 'Sí')
    f.flujo('p3', 'p4')
    f.flujo('p4', 'p5')
    f.flujo('p5', 'p6')
    f.flujo('p6', 'p7')
    f.flujo('p7', 't1')
    f.flujo('e0', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e1', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e3', 'te', vuelta='der', lado_a='der', lado_b='der')
    return f


def n01_clases_seleccion():
    dc = DiagramaClases('class CU-N01 (1) Clases de la selección de loterías y horarios', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormPrincipal'], ['FormSeleccionLoteria'], ['FormApuesta']])
    dc.paquete('BLL', [['LoteriaBLL'], ['TurnoBLL']])
    dc.paquete('DAL', [['LoteriaDAL'], ['TurnoDAL']])
    dc.clase('FormPrincipal', ops=['BtnJuego_Click', 'AbrirHijo'])
    dc.clase('FormSeleccionLoteria', attrs=['juego', 'todasLasLoterias', 'idsLoteriasSeleccionadas', 'todosLosTurnos',
                                           'idsTurnosSeleccionados'],
             ops=['FormSeleccionLoteria', 'CargarLoterias', 'CargarTurnos', 'BtnLoteria_Click', 'BtnTodasLoterias_Click',
                  'BtnTurno_Click', 'BtnTodosTurnos_Click', 'ToggleSeleccion', 'ToggleTodos', 'ActualizarBotonTodos',
                  'ActualizarResumen', 'btnContinuar_Click'])
    dc.clase('FormApuesta', attrs=['juego', 'loterias', 'turnos', 'cantidadSorteos'], ops=['FormApuesta', 'DescribirSorteos'])
    dc.clase('LoteriaBLL', ops=['ObtenerActivas'])
    dc.clase('TurnoBLL', ops=['ObtenerActivos'])
    dc.clase('LoteriaDAL', attrs=['conexion'], ops=['ObtenerActivas'])
    dc.clase('TurnoDAL', attrs=['conexion'], ops=['ObtenerActivos'])
    dc.rel('dependencia', 'FormPrincipal', 'FormSeleccionLoteria', nombre='«create»')
    dc.rel('dependencia', 'FormSeleccionLoteria', 'FormApuesta', nombre='«create»')
    dc.rel('asociacion', 'FormSeleccionLoteria', 'LoteriaBLL', rol='-loteriaBll', mult_d='1')
    dc.rel('asociacion', 'FormSeleccionLoteria', 'TurnoBLL', rol='-turnoBll', mult_d='1')
    dc.rel('dependencia', 'FormApuesta', 'LoteriaBLL', nombre='«use»')
    dc.rel('asociacion', 'LoteriaBLL', 'LoteriaDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'TurnoBLL', 'TurnoDAL', rol='-dal', mult_d='1')
    return dc



def n01_clases_venta():
    dc = DiagramaClases('class CU-N01 (2) Clases de la carga de jugadas y del ticket', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['TicketRenderer', 'FormTicket'], [None, 'FormApuesta']])
    dc.paquete('BLL', [['SesionManager'], ['ApuestaBLL']])
    dc.clase('FormApuesta', attrs=['juego', 'loterias', 'turnos', 'cantidadSorteos', 'plantillas', 'rangosSeleccionados',
                                  'tipo', 'numeroActual', 'primerNumeroRedoblona'],
             ops=['CrearColumnas', 'CrearTeclado', 'CrearBotonesRango', 'btnRedoblona_Click', 'AplicarTipo',
                  'BtnRango_Click', 'ActualizarRangos', 'Tecla', 'ActualizarNumero', 'ProcessCmdKey', 'btnAgregar_Click',
                  'Agregar', 'AgregarPlantilla', 'btnQuitar_Click', 'ActualizarTotales', 'MostrarAviso',
                  'btnEnviar_Click'])
    dc.clase('FormTicket', attrs=['apuesta', 'esReimpresion'], ops=['FormTicket', 'pnlPapel_Paint', 'btnCopiar_Click'])
    dc.clase('TicketRenderer', attrs=['Ancho'], ops=['Dibujar'], estereotipo='«static»')
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('ApuestaBLL', attrs=['MontoMinimo', 'MontoMaximo', 'Directa', 'Redoblona', 'RangosValidos'],
             ops=['Vender', 'CodigoCorto', 'CodigoCompleto'])
    dc.rel('asociacion', 'FormApuesta', 'ApuestaBLL', rol='-bll', mult_d='1')
    dc.rel('dependencia', 'FormApuesta', 'SesionManager', nombre='«use»')
    dc.rel('dependencia', 'FormApuesta', 'FormTicket', nombre='«create»')
    dc.rel('dependencia', 'FormTicket', 'TicketRenderer', nombre='«use»')
    dc.rel('dependencia', 'FormTicket', 'ApuestaBLL', nombre='«use»')
    return dc



def n01_clases_datos():
    dc = DiagramaClases('class CU-N01 (3) Clases de negocio y acceso a datos de la venta', horizontal=True)
    dc.paquete('BLL', [['SorteoBLL'], ['ApuestaBLL'], ['BitacoraBLL']])
    dc.paquete('DAL', [['SorteoDAL'], ['ApuestaDAL'], ['BitacoraDAL']])
    dc.clase('ApuestaBLL', attrs=['Vigente'], ops=['Vender', 'ValidarJugada', 'ValidarNumeroQuiniela', 'CodigoCorto'])
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



def n01_be():
    return diagrama_be('class CU-N01 (4) Entidades (BE)',
                       ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Juego', 'Sorteo', 'Loteria', 'Turno',
                        'Bitacora'], notas=['Usuario', 'Bitacora'])


def n01_der():
    return diagrama_der('DER CU-N01 Vender apuesta de Quiniela',
                        ['USUARIOS', 'APUESTAS', 'JUGADAS', 'JUGADA_NUMERO', 'SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS',
                         'Bitacora'])


CU_N01 = dict(
    codigo='CU-N01',
    objetivo='Registrar la venta de una apuesta de Quiniela que el apostador pide en el mostrador —una o más jugadas '
             'directas o redoblonas, para una o varias loterías y horarios del día— y emitir el ticket con el código '
             'único (GUID) que la respalda.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=[
        'El usuario inició sesión en el sistema (CU01 Iniciar sesión, T02).',
        'La Quiniela está cargada y activa en el catálogo de juegos (JUEGOS.Codigo = "QUINIELA").',
        'Hay al menos una lotería y un turno activos.',
    ],
    post=[
        'Queda registrado el ticket (APUESTAS) en estado Vigente, con su código único (GUID), la fecha y hora, el '
        'vendedor y el total.',
        'Por cada jugada cargada y cada combinación de lotería y horario elegida queda una jugada (JUGADAS) asociada al '
        'sorteo de Quiniela del día correspondiente, con su número o números y su posición (JUGADA_NUMERO).',
        'Los sorteos del día que todavía no existían quedan creados en estado Abierto (SORTEOS).',
        'Queda registrado el evento "Venta" en la bitácora con el juego, el código corto del ticket, la cantidad de '
        'jugadas y el total.',
    ],
    post_fracaso=[
        'Si una jugada no es válida o alguno de los sorteos ya no está abierto, la venta no se registra y las jugadas '
        'cargadas quedan en pantalla para corregirlas.',
        'El ticket, sus jugadas y sus números se guardan en una única transacción: si el guardado falla no queda nada '
        'registrado (los sorteos que se hayan creado quedan abiertos para las próximas ventas).',
    ],
    disparadores=[
        'El apostador pide jugar a la Quiniela en el mostrador y el operador presiona «Quiniela» en la sección VENDER '
        'del menú principal (o «Cargar Apuesta» en la barra superior).',
    ],
    extension=[('Ticket emitido', 14, 'CU-N03', 'El operador presiona «Imprimir» en la pantalla del ticket.')],
    inclusion=[],
    flujo=[
        ('A', 'Presiona «Quiniela» en la sección VENDER del menú principal.'),
        ('S', 'Obtiene las loterías y los turnos activos y muestra la pantalla de selección, con un botón por cada '
              'lotería y por cada horario y los botones «Todas» y «Todos».'),
        ('A', 'Selecciona una o más loterías y uno o más horarios.'),
        ('S', 'Marca cada selección y actualiza el resumen: “Vas a jugar en N sorteos: L loterías × T horarios.”'),
        ('A', 'Presiona «CONTINUAR».'),
        ('S', 'Verifica que haya al menos una lotería y un horario, cierra la selección y abre la carga de jugadas, '
              'con las loterías, los horarios, la cantidad de sorteos y la fecha en el encabezado. La jugada empieza '
              'como Directa, a la primera y por el monto mínimo ($100).'),
        ('A', 'Ingresa el número (de 1 a 4 cifras) con el teclado de la pantalla o el del equipo, elige una o más '
              'posiciones (A la 1ra, A los 5, A los 10, A los 20 o Todas) y el monto, y presiona «AGREGAR AL TICKET» '
              '(o Enter).'),
        ('S', 'Agrega a la lista "Jugadas del ticket" una jugada directa por cada posición elegida, con su tipo, número, '
              'posición, monto y subtotal (monto × cantidad de sorteos), y actualiza el total del ticket.'),
        ('A', 'Cuando terminó de cargar las jugadas que pide el apostador (repitiendo los pasos 7 y 8), presiona '
              '«CONFIRMAR VENTA».'),
        ('S', 'Valida cada jugada: monto entre $100 y $5.000, directa con un número de 1 a 4 cifras, redoblona con dos '
              'números de 2 cifras y posición a la primera, a los 5, a los 10 o a los 20 (RN-01, RN-02, RN-03).'),
        ('S', 'Por cada lotería y horario elegidos obtiene el sorteo de Quiniela del día: si no existe lo crea en estado '
              'Abierto, y verifica que esté abierto (RN-06).'),
        ('S', 'Arma el ticket: genera el código único (GUID), registra la fecha y hora, el vendedor y el estado '
              'Vigente, repite cada jugada en cada sorteo y calcula el total (RN-04, RN-07).'),
        ('S', 'Guarda el ticket, sus jugadas y sus números en una única transacción y registra el evento "Venta" en la '
              'bitácora (RN-16).'),
        ('S', 'Muestra el ticket ("Venta registrada"): juego, jugadas agrupadas por sorteo, total, código de barras, '
              'código corto y código completo.'),
        ('A', 'Si hay que entregar el ticket en papel presiona «Imprimir» (punto de extensión Ticket emitido: se '
              'extiende con CU-N03 Imprimir ticket); después presiona «Cerrar».'),
        ('S', 'Vacía la lista de jugadas y queda listo para otra venta sobre los mismos sorteos.'),
    ],
    alternativos=[
        ('3.a', 'Selección con «Todas» o «Todos»', [
            'El operador presiona «Todas» (o «Todos»).',
            'El sistema marca todas las loterías (o todos los horarios) activos; si ya estaban todos marcados, los '
            'desmarca.'], 'Continúa en el paso 4.'),
        ('6.a', 'No eligió lotería u horario', [
            'El sistema muestra “Elegí al menos una lotería y un horario.” y no avanza.'], 'Vuelve al paso 3.'),
        ('7.a', 'Redoblona', [
            'El operador presiona «Redoblona». El sistema limita el número a 2 cifras, deja elegir una sola posición y '
            'deshabilita «Todas».',
            'El operador carga el primer número con su posición y presiona «AGREGAR AL TICKET». El sistema lo guarda e '
            'indica “1° número: NN a los X. Ahora cargá el 2°.”',
            'El operador carga el segundo número con su posición y presiona «AGREGAR AL TICKET». El sistema agrega una '
            'única jugada Redoblona con los dos números.'], 'Continúa en el paso 9.'),
        ('7.b', 'Falta el número o la posición', [
            'El sistema muestra “Ingresá un número con el teclado.” o “Elegí al menos una posición.” y no agrega la '
            'jugada. En la redoblona, si el número no tiene 2 cifras, muestra “En la redoblona cada número es de 2 '
            'cifras.”'], 'Vuelve al paso 7.'),
        ('8.a', 'Quitar una jugada', [
            'El operador selecciona una jugada de la lista y presiona «Quitar jugada».',
            'El sistema la quita y recalcula el total. Si no hay ninguna seleccionada muestra “Seleccioná en la lista '
            'la jugada que querés quitar.”'], 'Vuelve al paso 7.'),
        ('9.a', 'Redoblona incompleta', [
            'Si al presionar «CONFIRMAR VENTA» hay un primer número de redoblona sin el segundo, el sistema muestra '
            '“Falta cargar el 2° número de la redoblona.” y no registra la venta.'], 'Vuelve al paso 7.'),
        ('10.a', 'Sin jugadas o jugada inválida', [
            'El sistema muestra, con el título “No se pudo registrar la venta”, el motivo: por ejemplo “Cargá al menos '
            'una jugada antes de confirmar la venta.” o “El monto de cada jugada debe estar entre $100 y $5.000.”',
            'No registra la venta y deja las jugadas en pantalla.'], 'Vuelve al paso 7.'),
        ('11.a', 'Sorteo cerrado o sorteado', [
            'El sistema muestra “El sorteo … ya no acepta apuestas (está cerrado).” (o “sorteado”) y no registra la '
            'venta.'], 'Fin del caso de uso: el operador puede iniciar otra venta eligiendo sorteos abiertos.'),
        ('13.a', 'Error al guardar', [
            'La base de datos rechaza la operación: el sistema revierte la transacción y muestra el error con el título '
            '“No se pudo registrar la venta”. Las jugadas quedan en pantalla.'], 'Vuelve al paso 9.'),
        ('14.a', 'Copiar el código', [
            'El operador presiona «Copiar código». El sistema copia el código completo del ticket al portapapeles y '
            'muestra “Código copiado”.'], 'Continúa en el paso 15.'),
    ],
    reglas=['RN-01', 'RN-02', 'RN-03', 'RN-04', 'RN-06', 'RN-07', 'RN-16'],
    diagrama_cu=n01_cu,
    flujo_alg=('Armado y registro del ticket (ApuestaBLL.Vender). El mismo método atiende a todos los juegos: la rama '
               '"No" de la lotería y el turno es la de CU-N02.', n01_flujo),
    clases=[('Clases que intervienen en la selección de loterías y horarios', n01_clases_seleccion),
            ('Clases de presentación y de negocio de la carga y la confirmación de la venta', n01_clases_venta),
            ('Clases de negocio y de acceso a datos de la venta', n01_clases_datos),
            ('Clases de entidad (BE) del caso de uso', n01_be)],
    secuencias=[
                ('Apertura de la venta de Quiniela (pasos 1 y 2)',
                 SD.n01_sd_abrir),
                ('Lectura de las loterías y los turnos activos (paso 2)',
                 SD.n01_sd_leer),
                ('Elección de las loterías y los horarios (pasos 3 y 4 y flujo 3.a)',
                 SD.n01_sd_elegir),
                ('Paso a la carga de jugadas (pasos 5 y 6 y flujo 6.a)',
                 SD.n01_sd_continuar),
                ('Ingreso del número y de las posiciones (paso 7 y flujo 7.a)',
                 SD.n01_sd_numero),
                ('Agregado de la jugada al ticket (pasos 7 y 8 y flujos 7.a y 7.b)',
                 SD.n01_sd_agregar),
                ('Quitar una jugada (flujo 8.a)',
                 SD.n01_sd_quitar),
                ('Confirmación de la venta (pasos 9 a 16 y flujos 9.a, 10.a, 11.a y 13.a)',
                 SD.n01_sd_confirmar),
                ('Pantalla del ticket (pasos 14 y 15 y flujo 14.a)',
                 SD.n01_sd_ticket),
                ('Armado y registro del ticket en la capa de negocio (pasos 10 a 13)',
                 SD.n01_sd_vender),
                ('Obtención del sorteo del día (paso 11 y flujo 11.a)',
                 SD.n01_sd_sorteo),
                ('Guardado del ticket y registro en la bitácora (paso 13 y flujo 13.a)',
                 SD.n01_sd_guardar)],
    der=n01_der,
    der_nota='USUARIOS no se escribe: el ticket guarda el Id del vendedor en APUESTAS.UsuarioId (clave foránea).',
    correspondencia=['Apuesta', 'Jugada', 'NumeroElegido', 'Sorteo', 'Juego', 'Loteria', 'Turno', 'Usuario',
                     'Bitacora'],
    prototipos=[],
)
