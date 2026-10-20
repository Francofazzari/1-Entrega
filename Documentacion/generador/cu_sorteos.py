# -*- coding: utf-8 -*-
"""PN02 Gestion de sorteos y resultados: CU-N07 a CU-N10."""
import sd_sorteos as SD
from clases import DiagramaClases
from cu import DiagramaCU
from actividad import DiagramaFlujo
from n02_comun import diagrama_be, diagrama_der
from comun import SISTEMA_CORTO
from cu_venta import BD


# ================================================================== CU-N07 Consultar numeros ganadores
def n07_cu():
    # CU-N08, CU-N09 y CU-N10 se inician desde la misma pantalla, pero son objetivos propios del Administrador:
    # no extienden la consulta (ver el modelo de casos de uso del sistema)
    u = DiagramaCU('uc CU-N07 Consultar números ganadores')
    u.limite(SISTEMA_CORTO, 220, 50, 420, 330)
    u.actor('op', 'Operador', 70, 90)
    u.actor('adm', 'Administrador', 70, 255)
    u.caso('cu', 'CU-N07\nConsultar números ganadores', 290, 160, w=280)
    u.asociacion('op', 'cu')
    u.generalizacion('adm', 'op')
    u.nota('La consulta no modifica datos. Desde la misma pantalla el Administrador inicia CU-N08, CU-N09 y '
           'CU-N10, que son casos de uso propios de su perfil (RN-17) y no extensiones de esta consulta.',
           680, 150, 260, conectar_a='cu')
    return u.dibujar()


def n07_clases_apertura():
    dc = DiagramaClases('class CU-N07 (1) Clases de la apertura de los reportes', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormPrincipal'], ['FormReporte']])
    dc.paquete('BLL', [['SesionManager'], ['JuegoBLL'], ['LoteriaBLL'], ['TurnoBLL']])
    dc.clase('FormPrincipal', ops=['btnReportes_Click', 'AbrirHijo'])
    dc.clase('FormReporte', attrs=['juegos', 'esAdmin'],
             ops=['FormReporte/0', 'FormReporte/2', 'ConstruirVentas', 'ConstruirPagos', 'ConstruirGanadores',
                  'CargarVentas', 'CargarPagos', 'btnGanadores_Click', 'MostrarVista', 'VerSorteo'])
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



def n07_clases_consulta():
    dc = DiagramaClases('class CU-N07 (2) Clases de la consulta de los sorteos', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte']])
    dc.paquete('BLL', [['SorteoBLL']])
    dc.paquete('DAL', [['SorteoDAL']])
    dc.clase('FormReporte', attrs=['juegoGanadores', 'sorteoActual', 'ganadores', 'celdas'],
             ops=['LoteriasElegidas', 'TurnosElegidos', 'Pendiente', 'VerSorteo', 'Virtual', 'DibujarPizarra',
                  'DibujarVarias', 'DibujarQuiniela', 'DibujarBolillas'])
    dc.clase('SorteoBLL', attrs=['Abierto', 'Sorteado'], ops=['Buscar', 'ObtenerNumerosGanadores', 'Descripcion'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['Buscar', 'ObtenerNumerosGanadores'])
    dc.rel('asociacion', 'FormReporte', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    return dc



CU_N07 = dict(
    codigo='CU-N07',
    objetivo='Consultar el resultado de uno o varios sorteos —por juego, lotería, turno y fecha— en una pizarra con '
             'los números ganadores y el estado de cada sorteo, para informárselo al apostador.',
    actor='Operador (también el Administrador, que lo especializa)',
    secundarios='—',
    pre=['El usuario inició sesión en el sistema.'],
    post=['Se muestra la pizarra de los sorteos elegidos. No se modifica ningún dato.'],
    post_fracaso=['Si falla la lectura se muestra el error con el título "Números ganadores".'],
    disparadores=['El operador abre «Reportes» en la sección CONSULTAS y elige la pestaña «Números ganadores».'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Abre «Reportes» en la sección CONSULTAS y presiona la pestaña «Números ganadores».'),
        ('S', 'Muestra los filtros con el primer juego, la lotería Provincia, el turno Primera y la fecha de hoy.'),
        ('A', 'Elige el juego, la lotería (o «Todas»), el turno (o «Todos») y la fecha. Cada cambio actualiza la '
              'pizarra; también puede presionar «Actualizar».'),
        ('S', 'Por cada combinación de lotería y turno elegida busca el sorteo del juego en esa fecha; si está '
              'sorteado, lee sus números ganadores.'),
        ('S', 'Muestra la pizarra: con un solo sorteo de Quiniela, los 20 números en dos columnas con la cabeza '
              'resaltada; con otro juego, las bolillas en el orden de salida; con varios sorteos, una columna por '
              'sorteo y el resumen “N de M sorteados”.'),
        ('S', 'Muestra el estado del sorteo (Abierto, Cerrado o Sorteado, RN-09) y, solo para el administrador '
              '(RN-17), habilita «Cerrar ventas», «Cargar resultado» y «Sortear al azar» según corresponda; con ellos el '
              'administrador inicia CU-N08, CU-N09 o CU-N10.'),
    ],
    alternativos=[
        ('3.a', 'Juego sin lotería ni turno', [
            'Si el juego no se sortea por lotería y turno, el sistema oculta esos filtros: hay un único sorteo por '
            'fecha.'], 'Continúa en el paso 4.'),
        ('5.a', 'Sorteo sin resultado', [
            'Si el sorteo no existe, el sistema muestra “Todavía no hay apuestas ni resultado para este sorteo.”; si '
            'existe pero no se realizó, “El sorteo todavía no se realizó.”. Con varios sorteos, las columnas '
            'pendientes dicen “Sin sortear”.'], 'Continúa en el paso 6.'),
        ('6.a', 'Usuario Operador', ['Los botones del administrador no se muestran.'], 'Fin del caso de uso.'),
    ],
    reglas=['RN-09', 'RN-17'],
    diagrama_cu=n07_cu,
    flujo_alg=None,
    flujo_no_aplica='No corresponde: la consulta recorre las combinaciones elegidas y lee cada sorteo (pasos 4 y 5).',
    clases=[('Clases que intervienen en la apertura de la pizarra', n07_clases_apertura),
            ('Clases que intervienen en la consulta de los sorteos', n07_clases_consulta),
            ('Clases de entidad (BE) que se leen',
             lambda: diagrama_be('class CU-N07 (3) Entidades (BE)',
                                 ['Usuario', 'Juego', 'Sorteo', 'Loteria', 'Turno', 'NumeroGanador'],
                                 notas=['NumeroGanador']))],
    secuencias=[
                ('Apertura de los reportes y de la pizarra (pasos 1 y 2)',
                 SD.n07_sd_abrir),
                ('Construcción de la pantalla de reportes (paso 2)',
                 SD.n07_sd_construir),
                ('Consulta de los sorteos elegidos (pasos 3 a 6)',
                 SD.n07_sd_ver),
                ('Lectura de un sorteo y de sus números ganadores (paso 4)',
                 SD.n07_sd_leer),
                ('Dibujo de la pizarra (paso 5 y flujo 5.a)',
                 SD.n07_sd_pizarra)],
    der=lambda: diagrama_der('DER CU-N07 Consultar números ganadores',
                             ['SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS', 'NUMERO_GANADOR']),
    correspondencia=['Sorteo', 'Juego', 'Loteria', 'Turno', 'NumeroGanador'],
    prototipos=[],
)


# ================================================================== CU-N08 Cerrar ventas de sorteos
def _cu_admin(codigo, nombre, boton, condicion):
    """Caso de uso propio del Administrador (RN-17): se asocia con él y no extiende CU-N07."""
    u = DiagramaCU('uc %s %s' % (codigo, nombre))
    u.limite(SISTEMA_CORTO, 220, 50, 420, 250)
    u.actor('adm', 'Administrador', 70, 140)
    u.caso('cu', '%s\n%s' % (codigo, nombre), 290, 140, w=280)
    u.asociacion('adm', 'cu')
    u.nota('Solo el perfil Administrador (RN-17). Se inicia con «%s» en la pestaña «Números ganadores» de '
           'Reportes, que se habilita cuando %s.' % (boton, condicion), 680, 130, 260, conectar_a='cu')
    return u.dibujar()


def n08_clases():
    dc = DiagramaClases('class CU-N08 (1) Clases del cierre de ventas', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte']])
    dc.paquete('BLL', [['SesionManager'], ['SorteoBLL'], ['BitacoraBLL']])
    dc.paquete('DAL', [['SorteoDAL'], ['BitacoraDAL']])
    dc.clase('FormReporte', attrs=['esAdmin', 'celdas'], ops=['CerrarVentas', 'VerSorteo'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('SorteoBLL', attrs=['Abierto', 'Cerrado'], ops=['CerrarVentas', 'Descripcion'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['CambiarEstado'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('asociacion', 'FormReporte', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('dependencia', 'FormReporte', 'SesionManager', nombre='«use»')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc


CU_N08 = dict(
    codigo='CU-N08',
    objetivo='Cerrar las ventas de los sorteos elegidos que siguen abiertos, al llegar su horario: desde ese momento '
             'no aceptan apuestas y sus tickets ya no se pueden cancelar.',
    actor='Administrador',
    secundarios='—',
    pre=['El usuario tiene el perfil Administrador (RN-17) y eligió los sorteos en la pestaña «Números '
         'ganadores» de Reportes.',
         'Hay al menos un sorteo abierto entre los elegidos.'],
    post=[
        'Cada sorteo elegido que estaba Abierto queda en estado Cerrado (RN-09).',
        'Por cada sorteo cerrado queda registrado el evento "Cierre de sorteo" en la bitácora.',
        'Los sorteos cerrados no aceptan apuestas (RN-06) y sus tickets no se pueden cancelar (RN-08).',
    ],
    post_fracaso=['Si un cierre falla se informa el error; los sorteos que ya se cerraron quedan cerrados.'],
    disparadores=['El administrador presiona «Cerrar ventas» en la pizarra de números ganadores.'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Con los sorteos elegidos en la pizarra, presiona «Cerrar ventas».'),
        ('S', 'Por cada sorteo elegido que está Abierto, lo pasa a Cerrado (RN-09) y registra el evento "Cierre de '
              'sorteo" en la bitácora (RN-16).'),
        ('S', 'Vuelve a leer los sorteos y actualiza la pizarra: el estado pasa a "Cerrado" y «Cerrar ventas» queda '
              'deshabilitado.'),
    ],
    alternativos=[
        ('2.a', 'Error al cerrar', [
            'El sistema muestra el error con el título “Cerrar ventas”, por ejemplo “Solo se pueden cerrar las ventas '
            'de un sorteo abierto.”'], 'Fin del caso de uso.'),
    ],
    reglas=['RN-06', 'RN-08', 'RN-09', 'RN-16', 'RN-17'],
    diagrama_cu=lambda: _cu_admin('CU-N08', 'Cerrar ventas de sorteos', 'Cerrar ventas',
                                  'hay sorteos abiertos entre los elegidos'),
    flujo_alg=None,
    flujo_no_aplica='No corresponde: el cierre solo cambia el estado de los sorteos abiertos.',
    clases=[('Clases que intervienen en el cierre de ventas', n08_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N08 (2) Entidades (BE)', ['Usuario', 'Juego', 'Sorteo', 'Loteria', 'Turno',
                                                                 'Bitacora']))],
    secuencias=[
                ('Cierre de ventas de los sorteos elegidos (pasos 1 a 3 y flujo 2.a)',
                 SD.n08_sd_cerrar),
                ('Cierre de ventas de un sorteo en la capa de negocio (paso 2)',
                 SD.n08_sd_bll)],
    der=lambda: diagrama_der('DER CU-N08 Cerrar ventas de sorteos',
                             ['SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS', 'Bitacora']),
    der_nota='JUEGOS, LOTERIAS y TURNOS se leen junto con cada sorteo en la pizarra de números ganadores, donde el '
             'administrador elige los sorteos antes de cerrar sus ventas.',
    correspondencia=['Sorteo', 'Juego', 'Loteria', 'Turno', 'Bitacora'],
    prototipos=[],
)


# ================================================================== CU-N09 Cargar resultado de sorteo
def n09_flujo():
    f = DiagramaFlujo('Diagrama de flujo CU-N09 - SorteoBLL.CompletarResultado (validar y completar el resultado)', [320, 300])
    f.terminal('t0', 'Inicio: CompletarResultado(juego, cargados)', 0, 0)
    f.pregunta('q0', '¿Hay un texto por cada número que sale en el sorteo?', 0, 1, w=250)
    f.proceso('e0', 'Error: "En … salen N números por sorteo."', 1, 1)
    f.proceso('p1', 'Por cada puesto con texto: validar que sean solo dígitos, dentro del rango del juego', 0, 2)
    f.pregunta('q1', '¿Algún número no es válido?', 0, 3)
    f.proceso('e1', 'Error: "El número del puesto N (X) no es válido: van del … al …"', 1, 3)
    f.pregunta('q2', '¿Algún número está repetido?', 0, 4)
    f.proceso('e2', 'Error: "El número … está repetido."', 1, 4)
    f.pregunta('q3', '¿El juego se juega a una posición (Quiniela)?', 0, 5, w=250)
    f.proceso('p3', 'terminaciones = últimas 2 cifras de los números cargados', 1, 5)
    f.decision('m', 0, 6)
    f.proceso('p4', 'Por cada puesto vacío: sortear un número al azar (Azar.Entre) hasta que no esté usado ni '
                    'termine en una de las terminaciones', 0, 7, w=280)
    f.proceso('p5', 'Formatear cada número con ceros a la izquierda y asignarle su posición (1, 2, 3...)', 0, 8)
    f.terminal('t1', 'Fin: devuelve los números ganadores', 0, 9)
    f.terminal('te', 'Fin con error: no se guarda nada', 1, 9)
    f.flujo('t0', 'q0')
    f.flujo('q0', 'e0', 'No')
    f.flujo('q0', 'p1', 'Sí')
    f.flujo('p1', 'q1')
    f.flujo('q1', 'e1', 'Sí')
    f.flujo('q1', 'q2', 'No')
    f.flujo('q2', 'e2', 'Sí')
    f.flujo('q2', 'q3', 'No')
    f.flujo('q3', 'p3', 'Sí')
    f.flujo('q3', 'm', 'No')
    f.flujo('p3', 'm', lado_a='abajo', lado_b='der')
    f.flujo('m', 'p4')
    f.flujo('p4', 'p5')
    f.flujo('p5', 't1')
    f.flujo('e0', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e1', 'te', vuelta='der', lado_a='der', lado_b='der')
    f.flujo('e2', 'te', vuelta='der', lado_a='der', lado_b='der')
    return f


def n09_clases():
    dc = DiagramaClases('class CU-N09 (1) Clases de la carga del resultado', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte'], ['FormResultado']])
    dc.paquete('BLL', [['SesionManager', None], ['JuegoBLL', 'Azar'], ['SorteoBLL', None], ['BitacoraBLL', None]])
    dc.paquete('DAL', [['SorteoDAL'], ['BitacoraDAL']])
    dc.clase('FormReporte', attrs=['juegoGanadores', 'celdas'], ops=['CargarResultado', 'VerSorteo'])
    dc.clase('FormResultado', attrs=['sorteo', 'casilleros'],
             ops=['FormResultado', 'ConstruirCasilleros', 'CrearCasillero', 'Casillero_KeyPress', 'Casillero_KeyDown',
                  'Textos', 'btnAzar_Click', 'btnLimpiar_Click', 'btnGuardar_Click'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('JuegoBLL', ops=['FormatearNumero'])
    dc.clase('SorteoBLL', attrs=['Abierto', 'Sorteado'],
             ops=['ObtenerOCrear', 'Buscar', 'CargarResultado', 'CompletarResultado', 'Descripcion'])
    dc.clase('Azar', ops=['Entre'], estereotipo='«static»')
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['Buscar', 'Insertar', 'GuardarResultado'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('dependencia', 'FormReporte', 'FormResultado', nombre='«create»')
    dc.rel('asociacion', 'FormReporte', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('asociacion', 'FormResultado', 'SorteoBLL', rol='-bll', mult_d='1')
    dc.rel('dependencia', 'FormResultado', 'SesionManager', nombre='«use»')
    dc.rel('dependencia', 'FormResultado', 'JuegoBLL', nombre='«use»')
    dc.rel('dependencia', 'SorteoBLL', 'Azar', nombre='«use»')
    dc.rel('dependencia', 'SorteoBLL', 'JuegoBLL', nombre='«use»')
    dc.rel('asociacion', 'SorteoBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc



CU_N09 = dict(
    codigo='CU-N09',
    objetivo='Registrar el resultado de un sorteo con los números que publicó la lotería —completando al azar los '
             'puestos que se dejen vacíos— y dejar el sorteo como sorteado, para que se puedan pagar los premios.',
    actor='Administrador',
    secundarios='—',
    pre=['El usuario tiene el perfil Administrador (RN-17) y eligió los sorteos en la pestaña «Números '
         'ganadores» de Reportes.',
         'Hay un único sorteo elegido, sin sortear y de una fecha que no es futura.'],
    post=[
        'Quedan guardados los números ganadores del sorteo con su puesto (NUMERO_GANADOR) y el sorteo queda en estado '
        'Sorteado, en una única transacción (RN-09, RN-10).',
        'Si el sorteo no existía (no tuvo ventas), queda creado.',
        'Queda registrado el evento "Sorteo realizado" en la bitácora, con la cantidad de números cargados a mano o '
        '"(al azar)".',
    ],
    post_fracaso=['Si un número no es válido o el sorteo ya se realizó, no se guarda nada.'],
    disparadores=['El administrador presiona «Cargar resultado» en la pizarra de números ganadores.'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Con un único sorteo elegido, presiona «Cargar resultado».'),
        ('S', 'Si el sorteo no existe lo crea en estado Abierto. Abre la carga con el sorteo en el encabezado y un '
              'casillero por puesto: en la Quiniela, 20 puestos en dos columnas (1° a 10° y 11° a 20°); en los demás '
              'juegos, tantos como números salen, en filas de 5.'),
        ('A', 'Escribe los números que salieron (solo se aceptan dígitos; Enter pasa al puesto siguiente). Puede dejar '
              'puestos vacíos.'),
        ('A', 'Presiona «GUARDAR RESULTADO».'),
        ('S', 'Pide confirmación indicando qué se completa al azar, por ejemplo: “Los 19 puestos vacíos se completan al '
              'azar. Una vez guardado, el sorteo queda como sorteado y no se puede modificar. ¿Guardar el resultado?”'),
        ('A', 'Confirma (Sí).'),
        ('S', 'Verifica que el sorteo no esté sorteado y que su fecha no sea futura (RN-09).'),
        ('S', 'Valida los números cargados (rango del juego y sin repetir) y completa al azar los puestos vacíos sin '
              'repetir números ni, en la Quiniela, las dos últimas cifras de los cargados a mano (RN-10).'),
        ('S', 'Guarda los números ganadores y pasa el sorteo a Sorteado en una única transacción, y registra el evento '
              '"Sorteo realizado" en la bitácora (RN-16).'),
        ('S', 'Cierra la carga y actualiza la pizarra con el resultado.'),
    ],
    alternativos=[
        ('3.a', 'Completar al azar sin guardar', [
            'El administrador presiona «Completar al azar». El sistema valida lo cargado, completa los puestos vacíos '
            'y los muestra en los casilleros, sin guardar.'], 'Vuelve al paso 3.'),
        ('3.b', 'Limpiar o cancelar', [
            '«Limpiar» vacía todos los casilleros (vuelve al paso 3). «Cancelar» cierra la carga sin guardar.'],
         'Fin del caso de uso, si canceló.'),
        ('6.a', 'No confirma', ['El administrador responde No.'], 'Vuelve al paso 3.'),
        ('7.a', 'Sorteo ya realizado o fecha futura', [
            'El sistema muestra “Ese sorteo ya se realizó.” o “No se puede realizar un sorteo de una fecha futura.” '
            'con el título “Cargar resultado”. También se muestra el primer mensaje si otro usuario lo sorteó '
            'mientras tanto (la transacción no cambia ninguna fila).'], 'Fin del caso de uso.'),
        ('8.a', 'Número inválido o repetido', [
            'El sistema muestra, por ejemplo, “El número del puesto 3 (12345) no es válido: van del 0000 al 9999.” o '
            '“El número 0032 está repetido.” y no guarda nada.'], 'Vuelve al paso 3.'),
    ],
    reglas=['RN-09', 'RN-10', 'RN-16', 'RN-17'],
    diagrama_cu=lambda: _cu_admin('CU-N09', 'Cargar resultado de sorteo', 'Cargar resultado',
                                  'se eligió un único sorteo, sin sortear y de una fecha que no es futura'),
    flujo_alg=('Validación y completado del resultado (SorteoBLL.CompletarResultado). La regla de las terminaciones '
               'garantiza que en la Quiniela un número cargado a mano sea el único que hace ganar a su terminación.',
               n09_flujo),
    clases=[('Clases que intervienen en la carga del resultado', n09_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N09 (2) Entidades (BE)',
                                 ['Usuario', 'Juego', 'Sorteo', 'Loteria', 'Turno', 'NumeroGanador', 'Bitacora'],
                                 notas=['NumeroGanador']))],
    secuencias=[
                ('Apertura de la carga del resultado (pasos 1 y 2)',
                 SD.n09_sd_abrir),
                ('Construcción de la pantalla de carga (paso 2)',
                 SD.n09_sd_construir),
                ('Carga de los números (paso 3 y flujos 3.a y 3.b)',
                 SD.n09_sd_cargar),
                ('Confirmación y guardado (pasos 4 a 6 y 10 y flujo 6.a)',
                 SD.n09_sd_confirmar),
                ('Validación, completado y guardado del resultado (pasos 7 a 9 y flujos 7.a y 8.a)',
                 SD.n09_sd_guardar),
                ('Guardado del resultado y registro en la bitácora (paso 9)',
                 SD.n09_sd_dal),
                ('Validación y completado al azar de los números (paso 8 y flujo 8.a)',
                 SD.n09_sd_completar),
                ('Obtención o creación del sorteo elegido (paso 2)',
                 SD.n09_sd_sorteo)],
    der=lambda: diagrama_der('DER CU-N09 Cargar resultado de sorteo',
                             ['SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS', 'NUMERO_GANADOR', 'Bitacora']),
    correspondencia=['Sorteo', 'Juego', 'Loteria', 'Turno', 'NumeroGanador', 'Bitacora'],
    prototipos=[],
)


# ================================================================== CU-N10 Sortear al azar
def n10_clases():
    dc = DiagramaClases('class CU-N10 (1) Clases del sorteo al azar', horizontal=True)
    dc.paquete('UI (Entrega1software)', [['FormReporte']])
    dc.paquete('BLL', [['SesionManager', 'JuegoBLL'], ['SorteoBLL', None], ['Azar', 'BitacoraBLL']])
    dc.paquete('DAL', [['SorteoDAL'], ['BitacoraDAL']])
    dc.clase('FormReporte', attrs=['juegoGanadores', 'celdas'], ops=['RealizarSorteo', 'Pendiente', 'Virtual',
                                                                    'VerSorteo'])
    dc.clase('SesionManager', attrs=['Instancia', 'UsuarioActual'], estereotipo='«singleton»')
    dc.clase('SorteoBLL', attrs=['Sorteado'], ops=['ObtenerOCrear', 'Buscar', 'RealizarSorteo', 'CargarResultado',
                                                   'CompletarResultado', 'Descripcion', 'DescripcionCorta'])
    dc.clase('Azar', ops=['Entre'], estereotipo='«static»')
    dc.clase('JuegoBLL', ops=['FormatearNumero'])
    dc.clase('BitacoraBLL', ops=['Registrar'])
    dc.clase('SorteoDAL', attrs=['conexion'], ops=['Buscar', 'Insertar', 'GuardarResultado'])
    dc.clase('BitacoraDAL', attrs=['conexion'], ops=['Registrar'])
    dc.rel('asociacion', 'FormReporte', 'SorteoBLL', rol='-sorteoBll', mult_d='1')
    dc.rel('dependencia', 'FormReporte', 'SesionManager', nombre='«use»')
    dc.rel('dependencia', 'SorteoBLL', 'Azar', nombre='«use»')
    dc.rel('dependencia', 'SorteoBLL', 'JuegoBLL', nombre='«use»')
    dc.rel('asociacion', 'SorteoBLL', 'BitacoraBLL', rol='-bitacora', mult_d='1')
    dc.rel('asociacion', 'SorteoBLL', 'SorteoDAL', rol='-dal', mult_d='1')
    dc.rel('asociacion', 'BitacoraBLL', 'BitacoraDAL', rol='-dal', mult_d='1')
    return dc



CU_N10 = dict(
    codigo='CU-N10',
    objetivo='Simular en el sistema el resultado de los sorteos elegidos que todavía no se realizaron, generando al '
             'azar todos sus números ganadores (para practicar o mostrar el funcionamiento sin un extracto oficial).',
    actor='Administrador',
    secundarios='—',
    pre=['El usuario tiene el perfil Administrador (RN-17) y eligió los sorteos en la pestaña «Números '
         'ganadores» de Reportes.',
         'Hay sorteos elegidos sin sortear y la fecha no es futura.'],
    post=[
        'Cada sorteo elegido pendiente queda en estado Sorteado con sus números ganadores generados al azar, sin '
        'repetir (RN-09, RN-10). Los sorteos elegidos que no existían quedan creados.',
        'Por cada sorteo queda registrado el evento "Sorteo realizado" con la indicación "(al azar)".',
        'Los sorteos ya sorteados de la selección no se modifican.',
    ],
    post_fracaso=['Si un sorteo falla se informa el error; los sorteos ya realizados en la misma operación quedan '
                  'sorteados.'],
    disparadores=['El administrador presiona «Sortear al azar» en la pizarra de números ganadores.'],
    extension=[],
    inclusion=[],
    flujo=[
        ('A', 'Con uno o varios sorteos elegidos (por ejemplo, «Todas» las loterías de un turno), presiona «Sortear al '
              'azar».'),
        ('S', 'Pide confirmación nombrando los sorteos pendientes: “¿Sortear al azar los N sorteos que faltan? … Se '
              'generan los números ganadores y dejan de aceptar apuestas.” (con uno solo: “¿Sortear al azar …?”).'),
        ('A', 'Confirma (Sí).'),
        ('S', 'Por cada sorteo pendiente: si no existe lo crea; genera al azar todos sus números, sin repetir; los guarda '
              'y pasa el sorteo a Sorteado en una única transacción, y registra "Sorteo realizado … (al azar)" en la '
              'bitácora (RN-09, RN-10, RN-16).'),
        ('S', 'Vuelve a leer los sorteos y actualiza la pizarra.'),
    ],
    alternativos=[
        ('3.a', 'No confirma', ['El administrador responde No: no se sortea nada.'], 'Fin del caso de uso.'),
        ('4.a', 'Error en un sorteo', [
            'El sistema muestra el error con el título “Sortear al azar” (por ejemplo, “Ese sorteo ya se realizó.” si '
            'otro usuario lo sorteó) y actualiza la pizarra.'], 'Fin del caso de uso.'),
    ],
    reglas=['RN-09', 'RN-10', 'RN-16', 'RN-17'],
    diagrama_cu=lambda: _cu_admin('CU-N10', 'Sortear al azar', 'Sortear al azar',
                                  'hay sorteos elegidos sin sortear y la fecha no es futura'),
    flujo_alg=None,
    flujo_no_aplica='El algoritmo es el de CU-N09 (SorteoBLL.CompletarResultado, figura del diagrama de flujo de CU-N09) '
                    'con todos los puestos vacíos: no hay terminaciones que evitar.',
    clases=[('Clases que intervienen en el sorteo al azar', n10_clases),
            ('Clases de entidad (BE) del caso de uso',
             lambda: diagrama_be('class CU-N10 (2) Entidades (BE)',
                                 ['Usuario', 'Juego', 'Sorteo', 'Loteria', 'Turno', 'NumeroGanador', 'Bitacora'],
                                 notas=['NumeroGanador']))],
    secuencias=[
                ('Sorteo al azar de los sorteos pendientes (pasos 1 a 5 y flujos 3.a y 4.a)',
                 SD.n10_sd_sortear),
                ('Generación y guardado del resultado al azar (paso 4)',
                 SD.n10_sd_resultado),
                ('Guardado del resultado y registro en la bitácora (paso 4)',
                 SD.n10_sd_dal),
                ('Obtención o creación del sorteo pendiente (paso 4)',
                 SD.n10_sd_sorteo)],
    der=lambda: diagrama_der('DER CU-N10 Sortear al azar',
                             ['SORTEOS', 'JUEGOS', 'LOTERIAS', 'TURNOS', 'NUMERO_GANADOR', 'Bitacora']),
    correspondencia=['Sorteo', 'Juego', 'Loteria', 'Turno', 'NumeroGanador', 'Bitacora'],
    prototipos=[],
)
