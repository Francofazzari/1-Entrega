# -*- coding: utf-8 -*-
"""Definiciones comunes del documento: procesos, casos de uso, requisitos y reglas de negocio.

Es la unica fuente de nombres y codigos: el N01, el N02 y todos los diagramas los toman de aca.
"""

SISTEMA = 'Sistema de gestión de la Agencia de Quiniela'
SISTEMA_CORTO = 'Sistema Agencia de Quiniela'
AUTOR = 'Franco Tomas Fazzari'      # como figura en el encabezado oficial de la carpeta
FECHA = '20/10/2026'
VERSION = '1.0'

PROCESOS = [
    ('PN01', 'Venta de apuestas'),
    ('PN02', 'Gestión de sorteos y resultados'),
    ('PN03', 'Pago de premios'),
    ('PN04', 'Control de la gestión comercial'),
]
NOMBRE_PROCESO = dict(PROCESOS)

# codigo -> (nombre, proceso, actor principal, tipo)
CASOS = {
    'CU-N01': ('Vender apuesta de Quiniela', 'PN01', 'Operador'),
    'CU-N02': ('Vender apuesta de juego de números', 'PN01', 'Operador'),
    'CU-N03': ('Imprimir ticket', 'PN01', 'Operador'),
    'CU-N04': ('Buscar ticket', 'PN01', 'Operador'),
    'CU-N05': ('Reimprimir ticket', 'PN01', 'Operador'),
    'CU-N06': ('Cancelar ticket', 'PN01', 'Operador'),
    'CU-N07': ('Consultar números ganadores', 'PN02', 'Operador'),
    'CU-N08': ('Cerrar ventas de sorteos', 'PN02', 'Administrador'),
    'CU-N09': ('Cargar resultado de sorteo', 'PN02', 'Administrador'),
    'CU-N10': ('Sortear al azar', 'PN02', 'Administrador'),
    'CU-N11': ('Pagar ticket premiado', 'PN03', 'Operador'),
    'CU-N12': ('Consultar reporte de ventas', 'PN04', 'Operador'),
    'CU-N13': ('Consultar reporte de pagos', 'PN04', 'Operador'),
}


def cu(codigo):
    """'CU-N01 Vender apuesta de Quiniela'."""
    return '%s %s' % (codigo, CASOS[codigo][0])


def esp(codigo):
    return 'ECU-' + codigo.split('-')[1]


REQUISITOS = [
    # codigo, descripcion, proceso, casos de uso
    ('RF-01', 'Registrar apuestas de Quiniela, directas y redoblonas, para una o más loterías y uno o más horarios '
              'del día, con número de 1 a 4 cifras, posición y monto.', 'PN01', ['CU-N01']),
    ('RF-02', 'Registrar apuestas de Quiniela Plus, Quini 6, Loto 5 y Telekino eligiendo los números en un '
              'tablero o en forma automática.', 'PN01', ['CU-N02']),
    ('RF-03', 'Identificar cada ticket con un código único (GUID) que se imprime en el ticket y permite '
              'buscarlo por el código completo o por sus primeros caracteres.', 'PN01',
     ['CU-N01', 'CU-N02', 'CU-N04']),
    ('RF-04', 'Mostrar e imprimir el ticket de una venta.', 'PN01', ['CU-N01', 'CU-N02', 'CU-N03']),
    ('RF-05', 'Reimprimir un ticket ya vendido, marcado como reimpresión, dejando constancia en la bitácora.',
     'PN01', ['CU-N05', 'CU-N03']),
    ('RF-06', 'Cancelar un ticket vigente mientras todos sus sorteos sigan abiertos.', 'PN01', ['CU-N06']),
    ('RF-07', 'Consultar los números ganadores de uno o varios sorteos por juego, lotería, turno y fecha.',
     'PN02', ['CU-N07']),
    ('RF-08', 'Cerrar las ventas de los sorteos abiertos.', 'PN02', ['CU-N08']),
    ('RF-09', 'Registrar el resultado de un sorteo cargando los números informados por la lotería y '
              'completando al azar los puestos vacíos.', 'PN02', ['CU-N09']),
    ('RF-10', 'Simular al azar los sorteos pendientes.', 'PN02', ['CU-N10']),
    ('RF-11', 'Calcular el premio de cada jugada de un ticket comparándola con el resultado de su sorteo.',
     'PN03', ['CU-N11']),
    ('RF-12', 'Pagar una única vez un ticket premiado cuyos sorteos ya se realizaron.', 'PN03', ['CU-N11']),
    ('RF-13', 'Consultar las ventas de un período, total o por juego, con la cantidad de tickets, el total '
              'vendido y los tickets pagados y cancelados.', 'PN04', ['CU-N12']),
    ('RF-14', 'Consultar los pagos de un período con la cantidad y el total pagado.', 'PN04', ['CU-N13']),
    ('RF-15', 'Ver el ticket completo desde los reportes.', 'PN04', ['CU-N12', 'CU-N13']),
]

REGLAS = [
    ('RN-01', 'En la Quiniela el monto de cada jugada va de $100 a $5.000.'),
    ('RN-02', 'El número de una jugada directa de Quiniela tiene de 1 a 4 cifras; la redoblona lleva dos números '
              'de 2 cifras.'),
    ('RN-03', 'Las posiciones válidas son a la primera (1), a los 5, a los 10 y a los 20. En la redoblona cada '
              'número tiene su propia posición.'),
    ('RN-04', 'Cada jugada de Quiniela se repite en cada combinación de lotería y horario elegida (un sorteo por '
              'combinación). El total del ticket es la suma de los montos de todas sus jugadas.'),
    ('RN-05', 'En Quiniela Plus, Quini 6, Loto 5 y Telekino cada jugada tiene una cantidad fija de números '
              'distintos dentro del rango del juego y un precio fijo: Quiniela Plus 8 números del 00 al 99 ($500), '
              'Quini 6 6 números del 00 al 45 ($1.000), Loto 5 5 números del 01 al 36 ($800) y Telekino 15 números '
              'del 01 al 25 ($1.500).'),
    ('RN-06', 'Solo se vende sobre los sorteos del día que están abiertos. Un sorteo se crea, en estado Abierto, '
              'la primera vez que se lo necesita.'),
    ('RN-07', 'Cada ticket tiene un código único (GUID). El código corto son sus primeros 8 caracteres; para '
              'buscar un ticket hacen falta el código completo o, al menos, sus primeros 6 caracteres.'),
    ('RN-08', 'Un ticket se puede cancelar solo si está Vigente y ninguno de sus sorteos cerró. Al cancelarlo se '
              'le devuelve el importe al apostador.'),
    ('RN-09', 'Un sorteo pasa de Abierto a Cerrado (cierre de ventas) y de Abierto o Cerrado a Sorteado (resultado '
              'cargado). Un sorteo sorteado no se modifica y no se puede sortear una fecha futura.'),
    ('RN-10', 'En la Quiniela salen 20 números de 4 cifras (0000 a 9999), del 1° al 20° puesto; en los demás juegos '
              'salen tantos números como indica el juego, dentro de su rango. Los números no se repiten, los '
              'puestos vacíos se completan al azar y, en la Quiniela, los números al azar no terminan en las mismas '
              'dos cifras que los cargados a mano.'),
    ('RN-11', 'Premio de una jugada directa de Quiniela: monto × 7, 70, 600 o 3.500 (según el número tenga 1, 2, 3 '
              'o 4 cifras) ÷ posición jugada, si alguno de los números ganadores hasta esa posición termina con las '
              'cifras jugadas.'),
    ('RN-12', 'Premio de una redoblona: monto × 70 ÷ posición del primer número × 70 ÷ posición del segundo, si '
              'salen los dos números.'),
    ('RN-13', 'Premio en los juegos de números: precio de la jugada × un multiplicador según la cantidad de '
              'aciertos (tablas de demostración del sistema).'),
    ('RN-14', 'Un ticket se paga una sola vez: tiene que estar Vigente, todos sus sorteos tienen que estar '
              'sorteados y tiene que tener premio.'),
    ('RN-15', 'En el reporte de ventas, la cantidad de tickets vendidos y el total vendido no incluyen los '
              'tickets cancelados.'),
    ('RN-16', 'Cada venta, reimpresión, cancelación, cierre de ventas, sorteo y pago queda registrado en la '
              'bitácora con el usuario, la fecha y la hora.'),
    ('RN-17', 'Cerrar ventas, cargar resultados y sortear al azar son operaciones exclusivas del perfil '
              'Administrador.'),
]
TEXTO_REGLA = dict(REGLAS)
