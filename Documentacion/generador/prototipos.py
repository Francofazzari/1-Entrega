# -*- coding: utf-8 -*-
"""Prototipos de interfaz de cada caso de uso (N02, item n).

El sistema ya está construido: el prototipo es la pantalla real (ejecutable), capturada con datos de prueba,
con los controles que intervienen en el caso de uso numerados. Cada número se explica en una tabla con el tipo
de control, lo que hace y el paso de la descripción analítica al que corresponde.
Las capturas y la posición de cada control las genera harness/CapturasDoc.cs (capturas/capturas.json).
"""
import json
import os
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

BASE = Path(__file__).resolve().parent.parent
CAPTURAS = BASE / 'capturas'
SALIDA = BASE / 'prototipos'
ACENTO = '#E8590C'
FUENTE_NUM = ImageFont.truetype(r'C:\Windows\Fonts\segoeuib.ttf', 17)

with open(CAPTURAS / 'capturas.json', encoding='utf-8') as f:
    ZONAS = json.load(f)


def anotar(captura, marcas, salida, recorte=None):
    """marcas: [(zona, ubicacion del numero)]; ubicacion: esq, izq, der, arriba, abajo. Devuelve la ruta."""
    img = Image.open(CAPTURAS / (captura + '.png')).convert('RGB')
    ox = oy = 0
    if recorte:
        x, y, w, h = ZONAS[captura]['zonas'][recorte]
        img = img.crop((x, y, x + w, y + h))
        ox, oy = x, y
    d = ImageDraw.Draw(img)
    r = 15
    for i, (zona, ubic) in enumerate(marcas, start=1):
        x, y, w, h = ZONAS[captura]['zonas'][zona]
        x, y = x - ox, y - oy
        d.rounded_rectangle((x - 3, y - 3, x + w + 3, y + h + 3), radius=6, outline=ACENTO, width=3)
        if ubic == 'izq':
            cx, cy = x - r - 8, y + h / 2
        elif ubic == 'der':
            cx, cy = x + w + r + 8, y + h / 2
        elif ubic == 'arriba':
            cx, cy = x + w / 2, y - r - 8
        elif ubic == 'abajo':
            cx, cy = x + w / 2, y + h + r + 8
        else:
            cx, cy = x + 2, y + 2
        cx = min(max(cx, r + 2), img.width - r - 3)
        cy = min(max(cy, r + 2), img.height - r - 3)
        d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=ACENTO, outline='white', width=2)
        d.text((cx, cy), str(i), font=FUENTE_NUM, fill='white', anchor='mm')
    SALIDA.mkdir(exist_ok=True)
    destino = SALIDA / (salida + '.png')
    img.save(destino)
    return str(destino)


def P(captura, titulo, filas, recorte=None, nota=None):
    """Un prototipo: captura, titulo y filas (zona, ubicacion, elemento, tipo, descripcion, paso)."""
    return dict(captura=captura, titulo=titulo, filas=filas, recorte=recorte, nota=nota)


MENU_VENDER = P('principal', 'Menú principal: sección VENDER', [
    ('quiniela', 'der', '«Quiniela»', 'Botón del menú', 'Abre la selección de loterías y horarios de la Quiniela.', '1'),
    ('apuesta', 'abajo', '«Cargar Apuesta»', 'Botón de la barra superior (Administrador)',
     'Abre la misma selección de loterías y horarios.', 'Disparador'),
])
TICKET_VENTA = lambda pasos: P('ticket', 'Pantalla del ticket (vista de venta)', [
    ('titulo', 'izq', 'Título "Venta registrada"', 'Encabezado', 'Confirma que la venta quedó registrada.', pasos[0]),
    ('papel', 'izq', 'Ticket', 'Comprobante dibujado (TicketRenderer)',
     'Juego, fecha y hora, vendedor, código corto, jugadas agrupadas por sorteo, total, código de barras y código '
     'completo.', pasos[0]),
    ('copiar', 'arriba', '«Copiar código»', 'Botón', 'Copia el código completo al portapapeles y muestra "Código '
     'copiado".', pasos[1]),
    ('imprimir', 'arriba', '«Imprimir»', 'Botón', 'Abre la vista previa de impresión: inicia CU-N03 Imprimir ticket '
     '(extensión).', 'Punto de extensión'),
    ('cerrar', 'arriba', '«Cerrar»', 'Botón (también Enter y Esc)', 'Cierra el ticket y deja la carga lista para otra '
     'venta.', pasos[2]),
])

PROTOTIPOS = {
    'CU-N01': [
        MENU_VENDER,
        P('seleccion', 'Selección de loterías y horarios', [
            ('loterias', 'esq', 'Botones de lotería', 'Botones de selección (se marcan y desmarcan)',
             'Una lotería activa por botón.', '3, 4'),
            ('todas', 'der', '«Todas»', 'Botón de selección', 'Marca todas las loterías; si ya estaban, las desmarca.',
             '3.a'),
            ('turnos', 'esq', 'Botones de horario', 'Botones de selección', 'Un turno activo por botón.', '3, 4'),
            ('todos', 'der', '«Todos»', 'Botón de selección', 'Marca todos los horarios; si ya estaban, los desmarca.',
             '3.a'),
            ('resumen', 'esq', 'Resumen', 'Etiqueta', '"Vas a jugar en N sorteos: L loterías × T horarios." o "Elegí '
             'al menos una lotería y un horario."', '4'),
            ('continuar', 'esq', '«CONTINUAR»', 'Botón', 'Verifica la elección y abre la carga de jugadas.', '5, 6, 6.a'),
        ]),
        P('apuesta', 'Carga de jugadas', [
            ('encabezado', 'esq', 'Encabezado', 'Etiquetas', 'Juego, loterías, horarios, cantidad de sorteos y fecha.',
             '6'),
            ('tipo', 'izq', '«Directa» / «Redoblona»', 'Botones de selección', 'Tipo de jugada.', '7, 7.a'),
            ('numero', 'izq', 'Número y teclado', 'Etiqueta y botones 0 a 9, ← y C',
             'Número de 1 a 4 cifras (2 en la redoblona); también se escribe con el teclado del equipo.', '7'),
            ('posicion', 'izq', 'Posiciones', 'Botones de selección', '«A la 1ra», «A los 5», «A los 10», «A los 20» y '
             '«Todas».', '7'),
            ('monto', 'izq', 'Monto por jugada', 'Campo numérico', 'De $100 a $5.000 (RN-01).', '7'),
            ('agregar', 'izq', '«AGREGAR AL TICKET»', 'Botón (también Enter)', 'Agrega una jugada por cada posición '
             'elegida.', '8, 7.b'),
            ('lista', 'esq', 'Jugadas del ticket', 'Grilla', 'Tipo, números, posición, monto y subtotal de cada jugada.',
             '8'),
            ('quitar', 'esq', '«Quitar jugada»', 'Botón', 'Quita la jugada seleccionada de la lista.', '8.a'),
            ('total', 'esq', 'Total del ticket', 'Etiqueta', 'Suma de las jugadas por la cantidad de sorteos.', '8'),
            ('confirmar', 'esq', '«CONFIRMAR VENTA»', 'Botón', 'Registra la venta y muestra el ticket.', '9 a 14'),
        ]),
        P('redoblona', 'Redoblona: primer número cargado', [
            ('tipo', 'izq', '«Redoblona»', 'Botón de selección (marcado)', 'Limita el número a 2 cifras y a una '
             'posición.', '7.a'),
            ('estado', 'izq', 'Indicación de la redoblona', 'Etiqueta', '"1° número: 23 a los 5. Ahora cargá el 2°."',
             '7.a'),
            ('posicion', 'izq', 'Posiciones', 'Botones de selección', 'Una sola posición; «Todas» queda deshabilitado.',
             '7.a'),
        ], recorte='panel'),
        TICKET_VENTA(('14', '14.a', '15, 16')),
    ],
    'CU-N02': [
        P('principal', 'Menú principal: sección VENDER', [
            ('juegos_numeros', 'der', '«Quiniela Plus», «Quini 6», «Loto 5», «Telekino»', 'Botones del menú',
             'Un botón por cada juego activo de la tabla JUEGOS; abre la pantalla del juego.', '1'),
        ]),
        P('quini6', 'Pantalla del juego (Quini 6)', [
            ('encabezado', 'esq', 'Encabezado', 'Etiquetas', 'Cuántos números se eligen, el rango, el precio por jugada y '
             'el sorteo del día.', '2'),
            ('tablero', 'esq', 'Tablero', 'Botones de selección', 'Un botón por número del rango del juego.', '3, 3.b'),
            ('contador', 'izq', 'Contador', 'Etiqueta', '"N de M elegidos", "Jugada completa" o el aviso.', '4, 3.b, 5.a'),
            ('automatica', 'esq', '«Automática»', 'Botón', 'Elige los números al azar.', '3.a'),
            ('limpiar', 'esq', '«Limpiar»', 'Botón', 'Desmarca todos los números.', '3.b'),
            ('agregar', 'esq', '«AGREGAR JUGADA»', 'Botón', 'Agrega la jugada a la lista.', '5, 5.a'),
            ('lista', 'esq', 'Jugadas del ticket', 'Grilla', 'N°, números, modalidad (Manual o Automática) y precio.',
             '6'),
            ('quitar', 'esq', '«Quitar jugada»', 'Botón', 'Quita la jugada seleccionada y renumera la lista.', '6.a'),
            ('confirmar', 'esq', '«CONFIRMAR VENTA»', 'Botón', 'Registra la venta y muestra el ticket.', '7 a 12'),
        ]),
        TICKET_VENTA(('12', '12.a', '13, 14')),
    ],
    'CU-N03': [
        P('ticket', 'Pantalla del ticket: botón «Imprimir»', [
            ('papel', 'izq', 'Ticket', 'Comprobante dibujado (TicketRenderer)', 'Es exactamente lo que se imprime.', '2'),
            ('imprimir', 'arriba', '«Imprimir»', 'Botón', 'Abre la vista previa "Imprimir ticket XXXXXXXX" del sistema '
             'operativo, desde la que se envía a la impresora.', '1, 3 a 5'),
        ], nota='La vista previa es el diálogo estándar de Windows (PrintPreviewDialog). Si no hay impresora instalada '
                'se informa "No hay ninguna impresora instalada en esta computadora." (flujo 2.a).'),
        P('ticket_reimpresion', 'Reimpresión: el mismo botón «Imprimir»', [
            ('papel', 'izq', 'Ticket con la marca *** REIMPRESIÓN ***', 'Comprobante dibujado', 'La reimpresión se '
             'imprime con su marca (y el sello si está pagado o cancelado).', '2'),
            ('imprimir', 'arriba', '«Imprimir»', 'Botón', 'Abre la vista previa de impresión.', '1'),
        ]),
    ],
    'CU-N04': [
        P('principal', 'Menú principal: sección OPERACIONES', [
            ('pagar', 'der', '«Pagar»', 'Botón del menú', 'Abre la pantalla "Pagar ticket" (CU-N11).', '1'),
            ('cancelar', 'der', '«Cancelar»', 'Botón del menú', 'Abre la pantalla "Cancelar ticket" (CU-N06).', '1'),
            ('reimprimir', 'der', '«Reimprimir»', 'Botón del menú', 'Abre la pantalla "Reimprimir ticket" (CU-N05).', '1'),
        ]),
        P('reimprimir', 'Búsqueda del ticket (pantalla de una operación)', [
            ('encabezado', 'esq', 'Encabezado', 'Etiquetas', 'Título y ayuda de la operación elegida.', '2'),
            ('codigo', 'esq', 'Código del ticket', 'Campo de texto', 'Código completo o, al menos, sus primeros 6 '
             'caracteres.', '3'),
            ('buscar', 'arriba', '«Buscar»', 'Botón (también Enter)', 'Busca el ticket.', '3 a 5'),
            ('hoy', 'esq', 'Tickets de hoy', 'Grilla', 'Hora, código corto, juego, total y estado; un clic busca ese '
             'ticket.', '2, 3.a'),
            ('resumen', 'esq', 'Detalle del ticket', 'Etiquetas', 'Juego, fecha y hora, vendedor, código, total y '
             'estado.', '6'),
            ('jugadas', 'esq', 'Jugadas', 'Grilla', 'Sorteo, jugada, monto y estado del sorteo.', '6'),
        ], nota='Si el código no identifica a un único ticket, en el lugar del detalle se muestra el motivo en rojo '
                '(flujos 4.a a 4.d).'),
    ],
    'CU-N05': [
        P('reimprimir', 'Reimprimir ticket', [
            ('motivo', 'esq', 'Mensaje', 'Etiqueta', '"La reimpresión sale marcada como REIMPRESIÓN."', '4'),
            ('accion', 'esq', '«REIMPRIMIR TICKET»', 'Botón', 'Registra la reimpresión y muestra el ticket.', '5 a 7'),
        ], recorte='detalle'),
        P('ticket_reimpresion', 'Pantalla de la reimpresión', [
            ('titulo', 'izq', 'Título "Reimpresión del ticket"', 'Encabezado', 'Vista de reimpresión.', '7'),
            ('papel', 'izq', 'Ticket con la marca *** REIMPRESIÓN ***', 'Comprobante dibujado', 'Mismo código, jugadas y '
             'estado que el original.', '7'),
            ('imprimir', 'arriba', '«Imprimir»', 'Botón', 'Inicia CU-N03 Imprimir ticket (extensión).',
             'Punto de extensión'),
            ('cerrar', 'arriba', '«Cerrar»', 'Botón', 'Vuelve a la operación y actualiza el detalle.', '8, 9'),
        ]),
    ],
    'CU-N06': [
        P('cancelar', 'Cancelar ticket: el ticket se puede cancelar', [
            ('motivo', 'esq', 'Mensaje', 'Etiqueta (verde)', '"El ticket se puede cancelar: se le devuelven $ X al '
             'cliente."', '4'),
            ('accion', 'esq', '«CANCELAR TICKET»', 'Botón', 'Pide confirmación y cancela el ticket.', '5 a 9'),
        ], recorte='detalle'),
        P('cancelar_no', 'Cancelar ticket: el ticket no se puede cancelar', [
            ('motivo', 'esq', 'Motivo', 'Etiqueta (roja)', 'Por ejemplo: "El sorteo … ya cerró: el ticket no se puede '
             'cancelar."', '4.a'),
            ('accion', 'esq', '«CANCELAR TICKET»', 'Botón deshabilitado', 'No se puede usar.', '4.a'),
        ], recorte='detalle'),
    ],
    'CU-N07': [
        P('principal', 'Menú principal: sección CONSULTAS', [
            ('reportes', 'der', '«Reportes»', 'Botón del menú', 'Abre los reportes de la agencia.', '1'),
        ]),
        P('reporte_ganadores', 'Pestaña «Números ganadores» (un sorteo de Quiniela)', [
            ('pestanias', 'esq', 'Pestañas', 'Botones de selección', '«Ventas en línea», «Pagos en línea» y «Números '
             'ganadores».', '1'),
            ('filtros', 'esq', 'Filtros', 'Listas y selector de fecha', 'Juego, lotería (o «Todas»), turno (o «Todos») y '
             'fecha.', '2, 3, 3.a'),
            ('actualizar', 'abajo', '«Actualizar»', 'Botón', 'Vuelve a leer los sorteos elegidos.', '3'),
            ('estado', 'esq', 'Estado', 'Etiqueta', 'Descripción del sorteo y su estado.', '6'),
            ('cerrar', 'abajo', '«Cerrar ventas»', 'Botón (solo Administrador)', 'Inicia CU-N08 (extensión).', '6'),
            ('cargar', 'abajo', '«Cargar resultado»', 'Botón (solo Administrador)', 'Inicia CU-N09 (extensión).', '6'),
            ('sortear', 'abajo', '«Sortear al azar»', 'Botón (solo Administrador)', 'Inicia CU-N10 (extensión).', '6'),
            ('pizarra', 'esq', 'Pizarra', 'Panel dibujado', 'Los 20 números en dos columnas, con la cabeza resaltada.',
             '5'),
        ]),
        P('ganadores_todas', 'Varios sorteos: «Todas» las loterías de un turno', [
            ('filtros', 'esq', 'Filtros', 'Listas', 'Lotería «Todas».', '3'),
            ('estado', 'esq', 'Resumen', 'Etiqueta', '"N de M sorteados".', '5'),
            ('pizarra', 'esq', 'Pizarra', 'Panel dibujado', 'Una columna por sorteo; los pendientes dicen "Sin sortear".',
             '5, 5.a'),
        ]),
        P('reporte_quini6', 'Otro juego: bolillas en el orden de salida', [
            ('filtros', 'esq', 'Filtros', 'Listas', 'Juego Quini 6: sin lotería ni turno.', '3.a'),
            ('pizarra', 'esq', 'Pizarra', 'Panel dibujado', 'Bolillas de menor a mayor y el orden de salida.', '5'),
        ]),
    ],
    'CU-N08': [
        P('reporte_ganadores', 'Pizarra de números ganadores: «Cerrar ventas»', [
            ('cerrar', 'abajo', '«Cerrar ventas»', 'Botón (solo Administrador)', 'Cierra las ventas de los sorteos '
             'elegidos que están abiertos.', '1, 2'),
            ('estado', 'esq', 'Estado', 'Etiqueta', 'Pasa a "Cerrado".', '3'),
        ]),
    ],
    'CU-N09': [
        P('reporte_ganadores', 'Pizarra de números ganadores: «Cargar resultado»', [
            ('cargar', 'abajo', '«Cargar resultado»', 'Botón (solo Administrador)', 'Abre la carga del resultado del '
             'sorteo elegido.', '1, 2'),
        ]),
        P('resultado', 'Carga del resultado', [
            ('puestos', 'esq', 'Puestos', 'Casilleros de texto', 'Uno por número que sale: en la Quiniela, 20 en dos '
             'columnas.', '2, 3'),
            ('puesto8', 'izq', 'Puesto 8 cargado a mano', 'Casillero de texto', 'Ejemplo: el 32, que se guarda como '
             '0032.', '3'),
            ('azar', 'arriba', '«Completar al azar»', 'Botón', 'Completa los vacíos sin guardar.', '3.a'),
            ('limpiar', 'arriba', '«Limpiar»', 'Botón', 'Vacía todos los casilleros.', '3.b'),
            ('cancelar', 'arriba', '«Cancelar»', 'Botón', 'Cierra la carga sin guardar.', '3.b'),
            ('guardar', 'arriba', '«GUARDAR RESULTADO»', 'Botón', 'Pide confirmación y guarda el resultado.', '4 a 10'),
        ]),
    ],
    'CU-N10': [
        P('reporte_ganadores', 'Pizarra de números ganadores: «Sortear al azar»', [
            ('sortear', 'abajo', '«Sortear al azar»', 'Botón (solo Administrador)', 'Pide confirmación y sortea los '
             'sorteos elegidos pendientes.', '1 a 4'),
            ('pizarra', 'esq', 'Pizarra', 'Panel dibujado', 'Se actualiza con los números generados.', '5'),
        ]),
        P('ganadores_todas', 'Resultado con varias loterías', [
            ('estado', 'esq', 'Resumen', 'Etiqueta', '"N de M sorteados".', '5'),
            ('pizarra', 'esq', 'Pizarra', 'Panel dibujado', 'Una columna por sorteo.', '5'),
        ]),
    ],
    'CU-N11': [
        P('principal', 'Menú principal: «Pagar»', [
            ('pagar', 'der', '«Pagar»', 'Botón del menú', 'Abre la pantalla "Pagar ticket".', '1'),
        ]),
        P('pagar', 'Pagar ticket: ticket premiado', [
            ('codigo', 'esq', 'Código del ticket', 'Campo de texto', 'Se busca con CU-N04.', '3'),
            ('hoy', 'esq', 'Tickets de hoy', 'Grilla', 'Un clic busca ese ticket.', '3'),
            ('resumen', 'esq', 'Detalle del ticket', 'Etiquetas', 'Juego, fecha y hora, vendedor, código, total y estado.',
             '5'),
            ('jugadas', 'esq', 'Jugadas', 'Grilla', 'Resultado (detalle) y premio de cada jugada.', '4, 5'),
            ('motivo', 'izq', 'Mensaje', 'Etiqueta', '"Ticket premiado: se le pagan $ X al cliente." o el motivo en rojo.',
             '5, 5.a'),
            ('accion', 'izq', '«PAGAR $ X»', 'Botón', 'Pide confirmación y paga el ticket.', '6 a 10'),
        ]),
        P('felicitaciones', 'Cartel de felicitaciones', [
            ('aceptar', 'arriba', '«ACEPTAR»', 'Botón', 'Cierra el cartel; el ticket queda como Pagado.', '12, 13'),
        ], nota='El cartel muestra el código del ticket, el importe ganado (que sube animado hasta el total) y las '
                'jugadas ganadoras (paso 11).'),
    ],
    'CU-N12': [
        P('principal', 'Menú principal: accesos al reporte de ventas', [
            ('reportes', 'der', '«Reportes»', 'Botón del menú', 'Abre los reportes en «Ventas en línea».', '1'),
            ('ventas', 'abajo', '«Ventas en Linea»', 'Botón de la barra superior (Administrador)', 'Ventas de hoy.',
             '1.a'),
            ('reporte', 'abajo', '«Reporte Semanal»', 'Botón de la barra superior (Administrador)', 'Ventas de los '
             'últimos 7 días.', '1.a'),
        ]),
        P('reporte_ventas', 'Pestaña «Ventas en línea»', [
            ('pestanias', 'esq', 'Pestañas', 'Botones de selección', '«Ventas en línea» marcada.', '2'),
            ('filtros', 'esq', 'Filtros', 'Selectores de fecha y lista', 'Desde, Hasta y Juego.', '3'),
            ('buscar', 'abajo', '«Buscar»', 'Botón', 'Lista las ventas del período.', '3 a 5, 4.a'),
            ('tarjetas', 'esq', 'Tarjetas', 'Etiquetas', 'Tickets vendidos, Total vendido (sin cancelados), Pagados y '
             'Cancelados.', '6'),
            ('grilla', 'esq', 'Ventas', 'Grilla', 'Fecha y hora, código, juego, vendedor, total y estado; doble clic abre '
             'el ticket.', '5, 6.a'),
        ]),
        P('ticket_pagado', 'Ticket abierto desde el reporte (vista de consulta)', [
            ('titulo', 'izq', 'Título "Ticket XXXXXXXX"', 'Encabezado', 'Con el estado del ticket.', '6.a'),
            ('papel', 'izq', 'Ticket', 'Comprobante dibujado', 'Con el sello PAGADO o CANCELADO si corresponde; no hay '
             'botón «Imprimir».', '6.a'),
            ('copiar', 'arriba', '«Copiar código»', 'Botón', 'Copia el código completo.', '6.a'),
            ('cerrar', 'arriba', '«Cerrar»', 'Botón', 'Vuelve al reporte.', '6.a'),
        ]),
    ],
    'CU-N13': [
        P('reporte_pagos', 'Pestaña «Pagos en línea»', [
            ('pestanias', 'esq', 'Pestañas', 'Botones de selección', '«Pagos en línea» marcada.', '1'),
            ('filtros', 'esq', 'Filtros', 'Selectores de fecha', 'Desde y Hasta.', '3'),
            ('buscar', 'abajo', '«Buscar»', 'Botón', 'Lista los pagos del período.', '3 a 5, 4.a'),
            ('tarjetas', 'esq', 'Tarjetas', 'Etiquetas', 'Pagos realizados y Total pagado.', '6'),
            ('grilla', 'esq', 'Pagos', 'Grilla', 'Fecha y hora, código, juego, quién pagó y monto; doble clic abre el '
             'ticket.', '5, 6.a'),
        ]),
    ],
}


def generar(codigo):
    """Anota las capturas del caso de uso y devuelve [(titulo, ruta_png, filas, nota)]."""
    res = []
    n = codigo.split('-')[1].lower()
    for k, p in enumerate(PROTOTIPOS.get(codigo, []), start=1):
        ruta = anotar(p['captura'], [(f[0], f[1]) for f in p['filas']], '%s_proto%d' % (n, k), p['recorte'])
        filas = [(str(i), f[2], f[3], f[4], f[5]) for i, f in enumerate(p['filas'], start=1)]
        res.append(('Prototipo %s (%d): %s' % (codigo, k, p['titulo']), ruta, filas, p['nota']))
    return res


if __name__ == '__main__':
    import sys
    sys.stdout.reconfigure(encoding='utf-8')
    for c in sorted(PROTOTIPOS):
        for t, ruta, filas, nota in generar(c):
            print(t, '->', os.path.basename(ruta), len(filas))
