# -*- coding: utf-8 -*-
"""Diagramas del N01: actores, colaboracion entre roles, actividad y modelo conceptual por proceso."""
from comun import NOMBRE_PROCESO, SISTEMA_CORTO
from cu import DiagramaCU
from actividad import DiagramaActividad
from secuencia import Secuencia
from n01 import PROCESOS_N01, modelo_conceptual, ATRIBUTOS_CONCEPTO


def d_actores(pn):
    """Diagrama de actores del proceso: actores y trabajadores del negocio con el caso de uso de negocio."""
    u = DiagramaCU('uc %s %s - roles intervinientes' % (pn, NOMBRE_PROCESO[pn]))
    u.limite('Agencia de Quiniela (negocio)', 250, 50, 380, 300)
    u.caso('pn', '%s\n%s' % (pn, NOMBRE_PROCESO[pn]), 305, 150, w=270, negocio=True)
    if pn in ('PN01', 'PN03'):
        u.actor('apo', 'Apostador', 90, 100, '«actor del negocio»')
        u.asociacion('apo', 'pn')
    if pn == 'PN02':
        u.actor('lot', 'Lotería', 90, 100, '«actor del negocio»')
        u.asociacion('lot', 'pn')
    u.actor('ope', 'Operador', 740, 80, '«trabajador»')
    u.actor('adm', 'Administrador', 740, 250, '«trabajador»')
    if pn == 'PN04':
        u.asociacion('pn', 'adm')
        u.asociacion('pn', 'ope')
    elif pn == 'PN02':
        u.asociacion('pn', 'adm')
        u.asociacion('pn', 'ope')
    else:
        u.asociacion('pn', 'ope')
    u.generalizacion('adm', 'ope')
    return u.dibujar()


def d_colaboracion(pn):
    """Diagrama de secuencia de la colaboracion entre los roles del proceso (nivel de negocio)."""
    s = Secuencia('sd %s %s - colaboración entre roles' % (pn, NOMBRE_PROCESO[pn]))
    s.numerar = True
    if pn == 'PN01':
        s.actor('apo', 'Apostador')
        s.actor('ope', 'Operador')
        s.objeto('sis', ':' + SISTEMA_CORTO)
        s.asincrono('apo', 'ope', 'pide jugar (juego, números, posición,\nmonto, loterías y horarios)')
        s.mensaje('ope', 'sis', 'selecciona el juego y, en Quiniela,\nlas loterías y los horarios')
        s.mensaje('ope', 'sis', 'carga las jugadas', ret='jugadas y total del ticket')
        s.asincrono('ope', 'apo', 'informa el total')
        s.asincrono('apo', 'ope', 'paga el importe')
        with s.llamada('ope', 'sis', 'confirma la venta', ret='ticket con código único'):
            s.mensaje('sis', 'sis', 'valida, asigna los sorteos del día,\ngenera el código y registra')
        with s.fragmento('opt', '[el operador imprime el ticket]'):
            with s.llamada('ope', 'sis', 'imprime el ticket'):
                s.mensaje('sis', 'sis', 'muestra la vista previa y lo envía\na la impresora predeterminada')
        s.asincrono('ope', 'apo', 'entrega el ticket')
    elif pn == 'PN02':
        s.actor('adm', 'Administrador')
        s.objeto('sis', ':' + SISTEMA_CORTO)
        s.actor('lot', 'Lotería')
        s.actor('ope', 'Operador')
        with s.llamada('adm', 'sis', 'cierra las ventas de los sorteos abiertos'):
            s.mensaje('sis', 'sis', 'pasa los sorteos a Cerrado\ny registra el cierre')
        s.asincrono('lot', 'adm', 'publica el extracto oficial')
        with s.alternativa() as a:
            with a.caso('[se carga el extracto]'):
                with s.llamada('adm', 'sis', 'carga los números del extracto', ret='resultado guardado'):
                    s.mensaje('sis', 'sis', 'valida, completa al azar los vacíos\ny pasa el sorteo a Sorteado')
            with a.caso('[simulación]'):
                with s.llamada('adm', 'sis', 'sortea al azar los sorteos pendientes', ret='resultados guardados'):
                    s.mensaje('sis', 'sis', 'genera los números y pasa\nlos sorteos a Sorteado')
        s.mensaje('ope', 'sis', 'consulta los números ganadores', ret='pizarra de números ganadores')
    elif pn == 'PN03':
        s.actor('apo', 'Apostador')
        s.actor('ope', 'Operador')
        s.objeto('sis', ':' + SISTEMA_CORTO)
        s.asincrono('apo', 'ope', 'presenta el ticket para cobrar')
        with s.llamada('ope', 'sis', 'busca el ticket por su código', ret='ticket con el premio de cada jugada'):
            s.mensaje('sis', 'sis', 'calcula los premios y verifica\nsi se puede pagar')
        with s.alternativa() as a:
            with a.caso('[se puede pagar]'):
                with s.llamada('ope', 'sis', 'confirma el pago', ret='cartel con el importe pagado'):
                    s.mensaje('sis', 'sis', 'registra el pago y deja\nel ticket como Pagada')
                s.asincrono('ope', 'apo', 'entrega el premio')
            with a.caso('[no se puede pagar]'):
                s.asincrono('ope', 'apo', 'informa el motivo (sorteos pendientes,\nsin premio, pagado o cancelado)')
    else:
        s.actor('adm', 'Administrador')
        s.objeto('sis', ':' + SISTEMA_CORTO)
        s.mensaje('adm', 'sis', 'pide las ventas del período y del juego',
                  ret='tickets, total vendido, pagados y cancelados')
        with s.fragmento('opt', '[quiere ver un ticket]'):
            s.mensaje('adm', 'sis', 'abre un ticket del listado', ret='ticket completo')
        s.mensaje('adm', 'sis', 'pide los pagos del período', ret='pagos y total pagado')
        s.nota('adm', 'Con esa información controla la caja del día o de la semana y toma decisiones.', 210)
    return s.dibujar()


def d_conceptual(pn):
    p = PROCESOS_N01[pn]
    return modelo_conceptual('class %s %s - modelo conceptual' % (pn, NOMBRE_PROCESO[pn]), p['conceptos']).dibujar()


def d_conceptual_general():
    return modelo_conceptual('class Modelo conceptual del dominio - Agencia de Quiniela',
                             list(ATRIBUTOS_CONCEPTO)).dibujar()


# ------------------------------------------------------------------ diagramas de actividad
def d_actividad_venta():
    a = DiagramaActividad('act PN01 Venta de apuestas', [('Apostador', 250), ('Operador', 300), ('Sistema', 360)],
                          alto_fila=64)
    a.inicio('i', 0, 0)
    a.accion('a1', 'Solicita la jugada: juego, números, posición y monto (en Quiniela, también loterías y horarios)', 0, 1)
    a.accion('a2', 'Elige el juego en el menú VENDER', 1, 1)
    a.decision('d1', 1, 2, '¿Quiniela?')
    a.accion('a3', 'Elige las loterías y los horarios', 1, 3, sistema=True, cu='CU-N01', dx=-78, w=136)
    a.decision('m1', 1, 4)
    a.accion('a4', 'Carga las jugadas', 1, 5, sistema=True, cu='CU-N01 / CU-N02')
    a.accion('a5', 'Valida cada jugada y calcula el total', 2, 5, sistema=True, cu='CU-N01 / CU-N02')
    a.accion('a6', 'Informa el total', 1, 6)
    a.accion('a7', 'Paga el importe', 0, 6)
    a.accion('a8', 'Confirma la venta', 1, 7, sistema=True, cu='CU-N01 / CU-N02')
    a.decision('d2', 2, 8, '¿Sorteos abiertos y\njugadas válidas?', texto_izq=True)
    a.accion('a9', 'Informa el motivo y no registra la venta', 2, 9, sistema=True, dx=80, w=124)
    a.accion('a10', 'Asigna los sorteos del día, genera el código único y registra la apuesta y la bitácora',
             2, 10, sistema=True, cu='CU-N01 / CU-N02', dx=-35)
    a.objeto('o1', ':Ticket [Vigente]', 2, 11, dx=-35)
    a.decision('d3', 1, 12, '¿Imprime?')
    a.accion('a11', 'Imprime el ticket', 1, 13, sistema=True, cu='CU-N03', dx=-78, w=136)
    a.decision('m2', 1, 14)
    a.accion('a12', 'Entrega el ticket', 1, 15)
    a.accion('a13', 'Recibe y conserva el ticket', 0, 15)
    a.fin('f', 0, 16)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'd1')
    a.flujo('d1', 'a3', '[sí]', lado_a='izq', lado_b='arriba')
    a.flujo('d1', 'm1', '[no]', lado_a='abajo', lado_b='arriba')
    a.flujo('a3', 'm1', lado_a='abajo', lado_b='izq')
    a.flujo('m1', 'a4')
    a.flujo('a4', 'a5')
    a.flujo('a5', 'a6', lado_a='abajo', lado_b='der')
    a.flujo('a6', 'a7')
    a.flujo('a7', 'a8', lado_a='abajo', lado_b='izq')
    a.flujo('a8', 'd2', lado_a='der', lado_b='arriba')
    a.flujo('d2', 'a9', '[no]', lado_a='der', lado_b='arriba')
    a.flujo('a9', 'm1', vuelta='der', lado_a='der', lado_b='der')
    a.flujo('d2', 'a10', '[sí]', lado_a='abajo', lado_b='arriba')
    a.flujo('a10', 'o1')
    a.flujo('o1', 'd3', lado_a='abajo', lado_b='der')
    a.flujo('d3', 'a11', '[sí]', lado_a='izq', lado_b='arriba')
    a.flujo('d3', 'm2', '[no]', lado_a='abajo', lado_b='arriba')
    a.flujo('a11', 'm2', lado_a='abajo', lado_b='izq')
    a.flujo('m2', 'a12')
    a.flujo('a12', 'a13')
    a.flujo('a13', 'f')
    a.con_leyenda()
    return a


def d_actividad_reimpresion():
    a = DiagramaActividad('act PN01 Venta de apuestas - reimpresión del ticket',
                          [('Apostador', 240), ('Operador', 280), ('Sistema', 290)], alto_fila=64)
    a.inicio('i', 0, 0)
    a.accion('a1', 'Presenta el ticket y pide reimprimirlo', 0, 1)
    a.accion('a2', 'Busca el ticket por su código o en la lista de tickets de hoy', 1, 1, sistema=True, cu='CU-N04')
    a.accion('a3', 'Muestra el detalle del ticket', 2, 2, sistema=True, cu='CU-N04')
    a.accion('a4', 'Pide la reimpresión', 1, 3, sistema=True, cu='CU-N05')
    a.accion('a5', 'Registra la reimpresión en la bitácora y muestra el ticket marcado REIMPRESIÓN', 2, 4,
             sistema=True, cu='CU-N05')
    a.accion('a6', 'Imprime el ticket', 1, 5, sistema=True, cu='CU-N03')
    a.accion('a7', 'Recibe la reimpresión', 0, 6)
    a.fin('f', 0, 7)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'a3', lado_a='der', lado_b='arriba')
    a.flujo('a3', 'a4', lado_a='abajo', lado_b='der')
    a.flujo('a4', 'a5', lado_a='der', lado_b='arriba')
    a.flujo('a5', 'a6', lado_a='abajo', lado_b='der')
    a.flujo('a6', 'a7', lado_a='abajo', lado_b='der')
    a.flujo('a7', 'f')
    a.con_leyenda()
    return a


def d_actividad_cancelacion():
    a = DiagramaActividad('act PN01 Venta de apuestas - cancelación del ticket',
                          [('Apostador', 240), ('Operador', 280), ('Sistema', 300)], alto_fila=64)
    a.inicio('i', 0, 0)
    a.accion('a1', 'Presenta el ticket y pide cancelarlo', 0, 1)
    a.accion('a2', 'Busca el ticket por su código o en la lista de tickets de hoy', 1, 1, sistema=True, cu='CU-N04')
    a.accion('a3', 'Muestra el detalle y verifica si se puede cancelar', 2, 2, sistema=True, cu='CU-N06')
    a.decision('d1', 2, 3, '¿Vigente y con sus sorteos abiertos?')
    a.accion('a4', 'Informa el motivo', 2, 4, sistema=True, cu='CU-N06', dx=80, w=140)
    a.accion('a5', 'Confirma la cancelación', 1, 4, sistema=True, cu='CU-N06')
    a.accion('a6', 'Deja el ticket como Anulada y registra la cancelación en la bitácora', 2, 5, sistema=True,
             cu='CU-N06', dx=-50)
    a.accion('a7', 'Devuelve el importe', 1, 6)
    a.accion('a8', 'Informa que no se puede cancelar', 1, 7, dx=0)
    a.decision('m1', 0, 8)
    a.fin('f', 0, 9)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'a3', lado_a='der', lado_b='arriba')
    a.flujo('a3', 'd1')
    a.flujo('d1', 'a5', '[sí]', lado_a='izq', lado_b='der')
    a.flujo('d1', 'a4', '[no]', lado_a='der', lado_b='arriba')
    a.flujo('a5', 'a6', lado_a='abajo', lado_b='izq')
    a.flujo('a6', 'a7', lado_a='abajo', lado_b='der')
    a.flujo('a4', 'a8', lado_a='abajo', lado_b='der')
    a.flujo('a7', 'm1', lado_a='izq', lado_b='arriba')
    a.flujo('a8', 'm1', lado_a='izq', lado_b='der')
    a.flujo('m1', 'f')
    a.con_leyenda()
    return a


def d_actividad_sorteos():
    a = DiagramaActividad('act PN02 Gestión de sorteos y resultados',
                          [('Lotería', 210), ('Administrador', 290), ('Sistema', 300), ('Operador', 230)],
                          alto_fila=64)
    a.inicio('i', 1, 0)
    a.accion('a1', 'Llegada la hora del sorteo, cierra las ventas', 1, 1, sistema=True, cu='CU-N08')
    a.accion('a2', 'Pasa los sorteos abiertos a Cerrado y registra el cierre en la bitácora', 2, 1, sistema=True,
             cu='CU-N08')
    a.accion('a3', 'Realiza el sorteo y publica el extracto oficial', 0, 2)
    a.decision('d1', 1, 3, '¿Resultado oficial?')
    a.accion('a4', 'Carga los números del extracto', 1, 4, sistema=True, cu='CU-N09', dx=-60, w=150)
    a.accion('a5', 'Pide sortear al azar (simulación)', 1, 5, sistema=True, cu='CU-N10', dx=60, w=150)
    a.decision('m1', 2, 6)
    a.accion('a6', 'Valida los números y completa al azar los puestos vacíos', 2, 7, sistema=True,
             cu='CU-N09 / CU-N10')
    a.accion('a7', 'Guarda los números ganadores, pasa el sorteo a Sorteado y registra en la bitácora', 2, 8,
             sistema=True, cu='CU-N09 / CU-N10')
    a.objeto('o1', ':Sorteo [Sorteado]', 2, 9)
    a.accion('a8', 'Consulta los números ganadores', 3, 10, sistema=True, cu='CU-N07')
    a.accion('a9', 'Muestra la pizarra con los números ganadores', 2, 11, sistema=True, cu='CU-N07')
    a.fin('f', 2, 12)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'a3', lado_a='abajo', lado_b='der')
    a.flujo('a3', 'd1', lado_a='abajo', lado_b='izq')
    a.flujo('d1', 'a4', '[sí]', lado_a='abajo', lado_b='arriba')
    a.flujo('d1', 'a5', '[no]', lado_a='der', lado_b='arriba')
    a.flujo('a4', 'm1', lado_a='abajo', lado_b='izq')
    a.flujo('a5', 'm1', lado_a='abajo', lado_b='izq')
    a.flujo('m1', 'a6')
    a.flujo('a6', 'a7')
    a.flujo('a7', 'o1')
    a.flujo('o1', 'a8', lado_a='der', lado_b='arriba')
    a.flujo('a8', 'a9', lado_a='abajo', lado_b='der')
    a.flujo('a9', 'f')
    a.con_leyenda()
    return a


def d_actividad_pagos():
    a = DiagramaActividad('act PN03 Pago de premios', [('Apostador', 240), ('Operador', 290), ('Sistema', 310)],
                          alto_fila=64)
    a.inicio('i', 0, 0)
    a.accion('a1', 'Presenta el ticket para cobrar', 0, 1)
    a.accion('a2', 'Busca el ticket por su código o en la lista de tickets de hoy', 1, 1, sistema=True, cu='CU-N04')
    a.accion('a3', 'Muestra el ticket y calcula el premio de cada jugada', 2, 2, sistema=True, cu='CU-N11')
    a.decision('d1', 2, 3, '¿Se puede pagar?')
    a.accion('a4', 'Informa el motivo (cancelado, ya pagado, sorteos pendientes o sin premio)', 2, 4,
             sistema=True, cu='CU-N11', dx=70, w=160)
    a.accion('a5', 'Confirma el pago', 1, 4, sistema=True, cu='CU-N11')
    a.accion('a6', 'Registra el pago, deja el ticket como Pagada y lo registra en la bitácora', 2, 5,
             sistema=True, cu='CU-N11', dx=-60, w=170)
    a.objeto('o1', ':Pago', 2, 6, dx=-60)
    a.accion('a7', 'Muestra el cartel de felicitaciones con el importe', 2, 7, sistema=True, cu='CU-N11', dx=-60, w=170)
    a.accion('a8', 'Entrega el importe', 1, 8)
    a.accion('a9', 'Cobra el premio', 0, 8)
    a.accion('a10', 'Le informa el motivo', 1, 9)
    a.decision('m1', 0, 10)
    a.fin('f', 0, 11)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'a3', lado_a='der', lado_b='arriba')
    a.flujo('a3', 'd1')
    a.flujo('d1', 'a5', '[sí]', lado_a='izq', lado_b='der')
    a.flujo('d1', 'a4', '[no]', lado_a='der', lado_b='arriba')
    a.flujo('a5', 'a6', lado_a='abajo', lado_b='izq')
    a.flujo('a6', 'o1')
    a.flujo('o1', 'a7')
    a.flujo('a7', 'a8', lado_a='abajo', lado_b='der')
    a.flujo('a8', 'a9')
    a.flujo('a4', 'a10', lado_a='abajo', lado_b='der')
    a.flujo('a9', 'm1')
    a.flujo('a10', 'm1', lado_a='izq', lado_b='der')
    a.flujo('m1', 'f')
    a.con_leyenda()
    return a


def d_actividad_control():
    a = DiagramaActividad('act PN04 Control de la gestión comercial', [('Administrador', 320), ('Sistema', 320)],
                          alto_fila=64)
    a.inicio('i', 0, 0)
    a.accion('a1', 'Abre Reportes (o "Ventas en Linea" / "Reporte Semanal")', 0, 1, sistema=True, cu='CU-N12')
    a.accion('a2', 'Elige el período y el juego', 0, 2, sistema=True, cu='CU-N12')
    a.accion('a3', 'Lista los tickets y calcula vendidos, total vendido, pagados y cancelados', 1, 3,
             sistema=True, cu='CU-N12')
    a.decision('d1', 0, 4, '¿Ver un ticket?')
    a.accion('a4', 'Abre el ticket (doble clic)', 0, 5, sistema=True, cu='CU-N12', dx=60, w=150)
    a.accion('a5', 'Muestra el ticket completo', 1, 6, sistema=True, cu='CU-N12')
    a.decision('m1', 0, 7)
    a.accion('a6', 'Consulta los pagos del período', 0, 8, sistema=True, cu='CU-N13')
    a.accion('a7', 'Lista los pagos y calcula la cantidad y el total pagado', 1, 9, sistema=True, cu='CU-N13')
    a.accion('a8', 'Analiza la información y toma decisiones', 0, 10)
    a.fin('f', 0, 11)
    a.flujo('i', 'a1')
    a.flujo('a1', 'a2')
    a.flujo('a2', 'a3', lado_a='der', lado_b='arriba')
    a.flujo('a3', 'd1', lado_a='abajo', lado_b='der', puntos=None)
    a.flujo('d1', 'a4', '[sí]', lado_a='abajo', lado_b='arriba', f_b=0.5)
    a.flujo('d1', 'm1', '[no]', lado_a='izq', lado_b='izq', vuelta='izq')
    a.flujo('a4', 'a5', lado_a='der', lado_b='arriba')
    a.flujo('a5', 'm1', lado_a='abajo', lado_b='der')
    a.flujo('m1', 'a6')
    a.flujo('a6', 'a7', lado_a='der', lado_b='arriba')
    a.flujo('a7', 'a8', lado_a='abajo', lado_b='der')
    a.flujo('a8', 'f')
    a.con_leyenda()
    return a


ACTIVIDADES = {
    'PN01': [('Venta del ticket', d_actividad_venta), ('Reimpresión del ticket', d_actividad_reimpresion),
             ('Cancelación del ticket', d_actividad_cancelacion)],
    'PN02': [('Sorteo y publicación de resultados', d_actividad_sorteos)],
    'PN03': [('Pago de un ticket premiado', d_actividad_pagos)],
    'PN04': [('Consulta de ventas y pagos', d_actividad_control)],
}
