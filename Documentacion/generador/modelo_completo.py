# -*- coding: utf-8 -*-
"""Diagrama de clases y modelo de datos (DER) de todo el sistema, cada uno completo en una sola página.

Igual que el resto del generador, todo sale del código: cada atributo, operación y relación se valida contra
las clases C# (codigo.leer_clases) y cada tabla, columna y clave foránea contra ScriptBD_AgenciaQuiniela.sql
(codigo.leer_tablas). Si algo no existe en el sistema, el script falla.

Uso:  python modelo_completo.py [carpeta_png]
Escribe en Documentacion/Diagramas:
  - "Modelo de datos (DER) - Sistema completo.drawio"   (una página)
  - "Diagrama de clases - Sistema completo.drawio"      (una página)
y, si se indica una carpeta, el PNG de cada uno.
"""
import re
import sys
from pathlib import Path

import clases
import codigo
from clases import DiagramaClases, DiagramaER, caja_tabla, medir_tabla, MARCAS
from drawio import Diagrama, archivo_drawio, estilo, mezclar, h, est_texto, marco, OSCURO, MEDIO, LINEA, FUENTE, T
from n02_comun import ATRIBUTOS_BE, ASOC_BE
from ruteo import asignar_puertos, rutear, restricciones, cruces, hueco_libre, vertical, _punto

DIAGRAMAS = Path(r'C:\Users\franco\source\repos\Entrega1software\Documentacion\Diagramas')
C = clases.CLASES

# la interfaz del patrón Observer (el lector del código solo toma clases)
_obs = codigo.Clase('IObservadorIdioma', 'BE', 'IObservadorIdioma.cs')
_obs.miembros.append(codigo.Miembro('metodo', 'public', 'ActualizarIdioma', 'void', [('Idioma', 'idioma')]))
C['IObservadorIdioma'] = _obs


# =========================================================================== ruteo
# Las relaciones entre clases de una misma fila que no son vecinas salen y entran por el mismo lado (por
# arriba o por abajo de la fila) y van por un canal propio, por fuera de las cajas que quedan en el medio.
_canales_original = clases.asignar_canales


def _asignar_canales(cajas, rels):
    _canales_original(cajas, rels)
    # El ruteo ordena los escalones pensando en líneas que bajan; en un canal donde todas suben (de una
    # pantalla a la ventana que abre, arriba) el orden tiene que ser el inverso para que no se crucen.
    canales = {}
    for r in rels:
        if (r.canal is None or r.datos.get('puntos') or r.lado_o == r.lado_d
                or not (vertical(r.lado_o) and vertical(r.lado_d))):
            continue
        p0 = _punto(cajas[r.origen], r.lado_o, r.f_o)
        p1 = _punto(cajas[r.destino], r.lado_d, r.f_d)
        canales.setdefault(hueco_libre(cajas, p0[1], p1[1], 'y'), []).append((r, p0[1] > p1[1]))
    for (a, b), lista in canales.items():
        if all(sube for _, sube in lista):
            for r, _ in lista:
                r.canal = a + b - r.canal
    grupos = {}
    for r in rels:
        if r.lado_o == r.lado_d and r.canal is None and not r.datos.get('puntos'):
            co = cajas[r.origen]
            clave = round(co[1]) if r.lado_o in ('arriba', 'abajo') else round(co[0] + co[2] / 2)
            grupos.setdefault((r.lado_o, clave), []).append(r)
    for (lado, _), lista in grupos.items():
        horizontal = lado in ('arriba', 'abajo')

        def tramo(r):
            co, cd = cajas[r.origen], cajas[r.destino]
            if horizontal:
                return abs(co[0] + co[2] / 2 - cd[0] - cd[2] / 2)
            return abs(co[1] + co[3] / 2 - cd[1] - cd[3] / 2)
        lista.sort(key=tramo)
        for k, r in enumerate(lista):
            co, cd = cajas[r.origen], cajas[r.destino]
            if horizontal:
                lo, hi = min(co[0], cd[0]), max(co[0] + co[2], cd[0] + cd[2])
                fila = [c for c in cajas.values()
                        if c[0] < hi and c[0] + c[2] > lo and c[1] < co[1] + co[3] and c[1] + c[3] > co[1]]
                # por arriba pasa más alto: encima de los rótulos de las flechas que llegan desde arriba
                r.canal = (max(c[1] + c[3] for c in fila) + 16 + 11 * k if lado == 'abajo'
                           else min(c[1] for c in fila) - 42 - 11 * k)
            else:
                lo, hi = min(co[1], cd[1]), max(co[1] + co[3], cd[1] + cd[3])
                col = [c for c in cajas.values()
                       if c[1] < hi and c[1] + c[3] > lo and c[0] < co[0] + co[2] and c[0] + c[2] > co[0]]
                r.canal = (max(c[0] + c[2] for c in col) + 16 + 11 * k if lado == 'der'
                           else min(c[0] for c in col) - 16 - 11 * k)


clases.asignar_canales = _asignar_canales


def _lineas_nota(texto, ancho_nota):
    from secuencia import envolver
    lineas = []
    for par in texto.split('\n'):
        lineas.extend(envolver(par, ancho_nota - 44, clases.TC))
    return lineas


def _notas(d, lista, ids):
    """Como clases.notas, pero con más margen: el navegador mide el texto un poco más ancho que el cálculo."""
    from drawio import est_nota
    for texto, nx, ny, nw, con in lista:
        lineas = _lineas_nota(texto, nw)
        nid = d.v(nx, ny, nw, 18 + 14 * len(lineas), '<br>'.join(h(l) for l in lineas), est_nota())
        if con:
            d.e(nid, ids[con], '', estilo(endArrow='none', dashed=1, html=1, strokeColor=clases.GRIS,
                                          edgeStyle='none'))


clases.notas = _notas


def _con_nota(fabrica, texto, ancho_nota, x=40):
    """Arma el diagrama dos veces: la primera para medirlo y la segunda con la nota debajo."""
    x0, y0, x1, y1 = fabrica().dibujar().limites()
    dg = fabrica()
    dg.nota(texto, x, y1 + 18, ancho_nota)
    return dg


# =========================================================================== modelo de datos (DER)
class DERModulos(DiagramaER):
    """DER con las tablas de cada módulo dentro de un recuadro rotulado."""
    PAD = 16        # margen del recuadro alrededor de sus tablas
    TITULO = 30     # alto del rótulo del módulo
    X0, Y0 = 52, 96

    def __init__(self, titulo, grupos):
        super().__init__(titulo)
        self.grupos = grupos            # [(módulo, color de fondo, color de borde, [tablas])]

    def ubicar(self):
        ncol = max(len(f) for f in self.filas)
        anchos = [0] * ncol
        altos = []
        for fila in self.filas:
            altos.append(max([medir_tabla(t)[1] for t in fila if t] or [40]))
            for k, t in enumerate(fila):
                if t:
                    anchos[k] = max(anchos[k], medir_tabla(t)[0])
        anchos = [a if a else 40 for a in anchos]
        cajas, y = {}, self.Y0
        for fila, alto_f in zip(self.filas, altos):
            x = self.X0
            for k, t in enumerate(fila):
                if t:
                    w, alto = medir_tabla(t)[:2]
                    cajas[t] = (x + (anchos[k] - w) / 2, y, w, alto)
                x += anchos[k] + self.GAP_X
            y += alto_f + self.GAP_Y
        return cajas

    def dibujar(self):
        d = Diagrama(self.nombre)
        cajas = self.ubicar()
        for modulo, fondo, borde, tablas in self.grupos:
            x0 = min(cajas[t][0] for t in tablas) - self.PAD
            y0 = min(cajas[t][1] for t in tablas) - self.TITULO
            x1 = max(cajas[t][0] + cajas[t][2] for t in tablas) + self.PAD
            y1 = max(cajas[t][1] + cajas[t][3] for t in tablas) + self.PAD
            d.v(x0, y0, x1 - x0, y1 - y0, h(modulo),
                estilo(rounded=1, arcSize=2, html=1, whiteSpace='wrap', fillColor=fondo, strokeColor=borde,
                       verticalAlign='top', align='left', spacingLeft=12, spacingTop=5, fontFamily=FUENTE,
                       fontSize=T, fontStyle=1, fontColor=OSCURO), conectable=False)
        self.cajas, self.ids = {}, {}
        for t, (x, y, w, alto) in cajas.items():
            tid, w, alto = caja_tabla(d, x, y, t, ident='tbl_' + t)
            self.cajas[t] = (x, y, w, alto)
            self.ids[t] = tid
        asignar_puertos(self.cajas, self.rels)
        clases.asignar_canales(self.cajas, self.rels)
        for r in self.rels:
            rutear(self.cajas, r)
            est = estilo(html=1, rounded=0, edgeStyle='orthogonalEdgeStyle', strokeColor=LINEA, endFill=0,
                         startFill=0, startArrow=MARCAS[r.datos['mult_h']], endArrow=MARCAS[r.datos['mult_p']],
                         startSize=12, endSize=12, jumpStyle='arc', jumpSize=8)
            d.e(self.ids[r.origen], self.ids[r.destino], '', mezclar(est, restricciones(r)), puntos=r.puntos)
        clases.AVISOS.extend(cruces(self.cajas, self.rels, self.nombre))
        clases.notas(d, self.notas, self.ids)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 28, y1b + 26, self.titulo)
        d.celdas = d.celdas + celdas
        return d


NOTAS_DER = [
    'APUESTAS es el ticket: una fila por ticket, con su código, quién lo vendió, la fecha y hora, el total y el '
    'estado (Vigente, Pagada o Anulada).\n'
    'JUGADAS son los renglones del ticket: una fila por jugada, con el sorteo al que va, el tipo, si es automática, '
    'el monto y el orden. Los números de cada jugada están en JUGADA_NUMERO.\n'
    'Van en tablas separadas porque un ticket tiene varias jugadas, cada una de un sorteo distinto: juntas, los '
    'datos del ticket se repetirían en cada jugada.',
    'SORTEOS es el sorteo de un día: juego + lotería + turno + fecha, combinación que no se repite (UQ_SORTEOS). El '
    'sorteo de mañana es otra fila, que el sistema crea la primera vez que se necesita. Lo que no cambia de un día '
    'a otro está en JUEGOS, LOTERIAS y TURNOS.',
    'Ejemplo: el 20/10/2026 se juega el 23 a la cabeza por $500 en Provincia y en La Ciudad, turno Nocturna. Queda '
    '1 fila en APUESTAS (total $1.000) y 2 en JUGADAS, una para cada sorteo del día (Provincia-Nocturna y La '
    'Ciudad-Nocturna), cada una con su número 23 en JUGADA_NUMERO. Si gana, el cobro queda en PAGOS.',
    'Seguridad: PERMISO_PERMISO arma el árbol de familias y patentes de PERMISOS y USUARIO_PERMISO asigna los '
    'permisos a cada usuario. Auditoría: Bitacora y CAMBIOS_USUARIO guardan una copia de los datos del usuario, '
    'sin clave foránea. Multi-idioma: TRADUCCIONES tiene el texto de cada palabra en cada idioma; '
    'IDIOMAS.DigitoVerificador e IDIOMAS_DIGITO_VERTICAL controlan la integridad de IDIOMAS.',
]


def der():
    def fabrica():
        er = DERModulos('DER Modelo de datos completo - Sistema Agencia de Quiniela', [
            ('Seguridad', '#EDF3FB', '#9FB8D8',
             ['PERMISO_PERMISO', 'PERMISOS', 'USUARIO_PERMISO', 'AUDITORIA_LOGIN', 'USUARIOS']),
            ('Auditoría', '#F4F0F8', '#B9AACD', ['CAMBIOS_USUARIO', 'Bitacora']),
            ('Multi-idioma e integridad', '#EDF6F1', '#9FC8B3',
             ['IDIOMAS', 'TRADUCCIONES', 'IDIOMAS_DIGITO_VERTICAL', 'PALABRAS']),
            ('Gestión comercial', '#FBF5EA', '#D8BF92',
             ['JUGADA_NUMERO', 'JUGADAS', 'APUESTAS', 'PAGOS', 'NUMERO_GANADOR', 'SORTEOS', 'JUEGOS', 'LOTERIAS',
              'TURNOS']),
        ])
        er.grilla([
            ['PERMISO_PERMISO', 'PERMISOS', 'USUARIO_PERMISO', 'CAMBIOS_USUARIO', 'IDIOMAS', 'TRADUCCIONES'],
            [None, 'AUDITORIA_LOGIN', 'USUARIOS', 'Bitacora', 'IDIOMAS_DIGITO_VERTICAL', 'PALABRAS'],
            ['JUGADA_NUMERO', 'JUGADAS', 'APUESTAS', 'PAGOS', None, None],
            ['NUMERO_GANADOR', 'SORTEOS', 'JUEGOS', None, None, None],
            [None, 'LOTERIAS', 'TURNOS', None, None, None],
        ])
        er.rel('PERMISO_PERMISO', 'PERMISOS', '0..*', columna='IdPadre')
        er.rel('PERMISO_PERMISO', 'PERMISOS', '0..*', columna='IdHijo')
        er.rel('USUARIO_PERMISO', 'PERMISOS', '0..*')
        er.rel('USUARIO_PERMISO', 'USUARIOS', '0..*')
        er.rel('AUDITORIA_LOGIN', 'USUARIOS', '0..*')
        er.rel('TRADUCCIONES', 'IDIOMAS', '0..*')
        er.rel('TRADUCCIONES', 'PALABRAS', '0..*')
        er.rel('APUESTAS', 'USUARIOS', '0..*')
        er.rel('PAGOS', 'USUARIOS', '0..*')
        er.rel('PAGOS', 'APUESTAS', '0..1')
        er.rel('JUGADAS', 'APUESTAS', '1..*')
        er.rel('JUGADA_NUMERO', 'JUGADAS', '1..*')
        er.rel('JUGADAS', 'SORTEOS', '0..*')
        er.rel('NUMERO_GANADOR', 'SORTEOS', '0..*')
        er.rel('SORTEOS', 'JUEGOS', '0..*')
        er.rel('SORTEOS', 'LOTERIAS', '0..*')
        er.rel('SORTEOS', 'TURNOS', '0..*')
        return er

    # las notas van a la derecha de la gestión comercial, una debajo de la otra
    cajas = fabrica().ubicar()
    x = min(cajas['IDIOMAS'][0], cajas['IDIOMAS_DIGITO_VERTICAL'][0])
    ancho_nota = max(cajas['TRADUCCIONES'][0] + cajas['TRADUCCIONES'][2],
                     cajas['PALABRAS'][0] + cajas['PALABRAS'][2]) - x
    y = cajas['APUESTAS'][1] - DERModulos.TITULO
    er = fabrica()
    for texto in NOTAS_DER:
        er.nota(texto, x, y, ancho_nota)
        y += 18 + 14 * len(_lineas_nota(texto, ancho_nota)) + 16
    return er


# =========================================================================== diagrama de clases
class DiagramaCapas(DiagramaClases):
    """Paquetes de las capas apilados (UI, BLL, DAL, BE) con columnas comunes. Cada módulo ocupa un grupo de
    columnas, rotulado arriba: así cada pantalla queda sobre su clase de negocio y su acceso a datos."""
    GAP_X = 90
    GAP_FILA = 120
    GAP_PAQ = 100
    Y0 = 96

    def __init__(self, titulo, modulos):
        super().__init__(titulo)
        self.modulos = modulos          # [(módulo, primera columna, última columna)]

    def _ubicar_vertical(self, d, tam):
        ncol = max(len(f) for p in self.paquetes for f in p['filas'])
        anchos = [0] * ncol
        for p in self.paquetes:
            for fila in p['filas']:
                for k, c in enumerate(fila):
                    if c:
                        anchos[k] = max(anchos[k], tam[c][0])
        anchos = [a if a else 60 for a in anchos]
        ancho_paq = sum(anchos) + self.GAP_X * (ncol - 1) + 2 * self.MARGEN
        x0, y = 24, self.Y0
        xs, xc = [], x0 + self.MARGEN
        for a in anchos:
            xs.append(xc)
            xc += a + self.GAP_X
        for modulo, k0, k1 in self.modulos:
            xa, xb = xs[k0] - 10, xs[k1] + anchos[k1] + 10
            d.v(xa, y - 44, xb - xa, 24, h(modulo), est_texto(T + 1, True, 'center', color=OSCURO),
                conectable=False)
            d.e(est=estilo(endArrow='none', html=1, strokeColor=MEDIO, strokeWidth=2), desde=(xa, y - 16),
                hasta=(xb, y - 16))
        self.paq_de = {}
        for i, p in enumerate(self.paquetes):
            altos = [max([tam[c][1] for c in fila if c] or [40]) for fila in p['filas']]
            alto_paq = 22 + 30 + sum(altos) + self.GAP_FILA * (len(altos) - 1) + 26
            pid = self._carpeta(d, x0, y, ancho_paq, alto_paq, p['nombre'])
            yf = 22 + 30
            for fila, alto_f in zip(p['filas'], altos):
                xc = self.MARGEN
                for k, c in enumerate(fila):
                    if c:
                        self._poner(d, pid, x0, y, xc + (anchos[k] - tam[c][0]) / 2, yf, c, tam)
                        self.paq_de[c] = i
                    xc += anchos[k] + self.GAP_X
                yf += alto_f + self.GAP_FILA
            y += alto_paq + self.GAP_PAQ


# columnas: 0-1 multi-idioma, 2-5 seguridad, 6-7 auditoría, 8-15 gestión comercial
MODULOS = [('Multi-idioma e integridad', 0, 1), ('Seguridad', 2, 5), ('Auditoría', 6, 7),
           ('Gestión comercial', 8, 15)]
CAPAS = [
    ('UI (Entrega1software)', [
        [None, 'FormAvisoIntegridad', 'Program', 'Estilos', None, None, None, None,
         None, None, None, 'FormTicket', 'TicketRenderer', None, None, 'FormFelicitaciones'],
        ['FormControlIdiomas', None, 'FormLogin', 'FormPrincipal', 'FormPerfiles', 'FormUsuarios',
         'FormControlCambios', 'FormBitacora',
         'FormSeleccionLoteria', 'FormApuesta', 'FormJuego', None, None, 'FormResultado', 'FormReporte',
         'FormOperacionTicket'],
    ]),
    ('BLL', [
        ['IdiomaManager', None, 'UsuarioBLL', 'Encriptador', 'PerfilBLL', 'SesionManager', None, 'BitacoraBLL',
         'LoteriaBLL', 'TurnoBLL', 'JuegoBLL', 'ApuestaBLL', 'Azar', 'SorteoBLL', 'PremioBLL', 'PagoBLL'],
    ]),
    ('DAL', [
        ['IdiomaDAL', None, 'UsuarioDAL', None, 'PerfilDAL', None, None, 'BitacoraDAL',
         'LoteriaDAL', 'TurnoDAL', 'JuegoDAL', 'ApuestaDAL', 'Mapeo', 'SorteoDAL', 'Conexion', 'PagoDAL'],
    ]),
    ('BE', [
        ['Idioma', 'IObservadorIdioma', 'UsuarioHistorial', 'Usuario', 'Permiso', None, None, 'Bitacora',
         None, None, None, 'Apuesta', 'Jugada', 'NumeroElegido', None, None],
        ['Traduccion', None, None, None, 'PermisoSimple', 'PermisoCompleto', None, None,
         None, None, 'Pago', 'Juego', 'Sorteo', 'Premio', None, None],
        [None, None, None, None, None, None, None, None,
         None, None, None, 'NumeroGanador', 'Loteria', 'Turno', None, None],
    ]),
]
# lado por el que salen las relaciones entre clases no vecinas de una misma fila
LADO_FILA = {'UI (Entrega1software)': 'arriba', 'BLL': 'abajo', 'DAL': 'abajo', 'BE': 'abajo'}

# miembros privados que se muestran: los del patrón Singleton y la referencia a BitacoraBLL (que no se dibuja)
PRIVADOS = {
    'SesionManager': ('_instancia', 'SesionManager'),
    'IdiomaManager': ('_instancia', 'IdiomaManager'),
    'ApuestaBLL': ('bitacora',), 'SorteoBLL': ('bitacora',), 'PagoBLL': ('bitacora',),
}
ATRIBUTOS_BE_OTROS = {
    'UsuarioHistorial': ['IdHistorial', 'FechaCambio', 'ResponsableNombre', 'Operacion', 'PermisosIds'],
    'Permiso': ['Id', 'Codigo', 'Nombre', 'Descripcion', 'EsPadre'],
    'Idioma': ['Id', 'Nombre', 'Codigo', 'DigitoVerificador'],
    'Traduccion': ['Clave', 'Valor'],
}
OPS_BE = {'Permiso': ['MostrarArbol'], 'PermisoSimple': ['MostrarArbol'],
          'PermisoCompleto': ['Agregar', 'Eliminar', 'MostrarArbol'], 'IObservadorIdioma': ['ActualizarIdioma']}
ESTEREOTIPOS = {'SesionManager': '«singleton»', 'IdiomaManager': '«singleton»', 'IObservadorIdioma': '«interface»',
                'Permiso': '«abstract»'}
SIN_MIEMBROS = ('Estilos',)       # colores, fuentes y controles comunes: se nombran en la nota

NOTA_CLASES = (
    'Cómo se lee: cada columna es un módulo y cada paquete una capa de la solución. La pantalla (UI) usa su clase '
    'de negocio (BLL), que usa su acceso a datos (DAL); todas trabajan con las entidades (BE). Las flechas sin '
    'rótulo de la UI a la BLL y de la BLL a la DAL son atributos privados de la clase (bll, dal, …), de '
    'multiplicidad 1. Arriba de cada pantalla están las ventanas que abre («create»). Se muestran los miembros '
    'públicos de cada clase.\n'
    'Para que se lea, no se dibujan: el uso de SesionManager (usuario de la sesión) y de Estilos (colores, fuentes '
    'y controles) desde las pantallas; el de BitacoraBLL, que figura como atributo bitacora; el de Conexion y Mapeo '
    'desde la DAL; las pantallas que abre el menú FormPrincipal; y el de las entidades BE desde las demás capas.')


def _miembros(nombre):
    """Atributos y operaciones públicos de la clase (más los privados de PRIVADOS), tal como están en el código."""
    if nombre in SIN_MIEMBROS:
        return [], []
    if nombre in ATRIBUTOS_BE or nombre in ATRIBUTOS_BE_OTROS or nombre in OPS_BE:
        return ATRIBUTOS_BE.get(nombre, ATRIBUTOS_BE_OTROS.get(nombre, [])), OPS_BE.get(nombre, [])
    c = C[nombre]
    privados = PRIVADOS.get(nombre, ())
    attrs, ops, vistos = [], [], set()
    for m in c.miembros:
        if m.vis not in ('public', 'internal') and m.nombre not in privados:
            continue
        if m.tipo in ('campo', 'propiedad', 'constante'):
            attrs.append(m.nombre)
        elif m.tipo in ('metodo', 'constructor'):
            if (m.nombre, len(m.params)) in vistos:
                continue
            vistos.add((m.nombre, len(m.params)))
            iguales = [x for x in c.miembros if x.nombre == m.nombre and x.tipo == m.tipo]
            ops.append('%s/%d' % (m.nombre, len(m.params)) if len(iguales) > 1 else m.nombre)
    return attrs, ops


def _fuente(nombre):
    c = C[nombre]
    ruta = codigo.REPO / codigo.CARPETAS[c.capa] / c.archivo
    return codigo._limpiar(ruta.read_text(encoding='utf-8-sig', errors='replace'))


def _tipo_base(dato):
    m = re.match(r'(?:List|IList|IEnumerable)<(\w+)>', dato or '')
    return (m.group(1), '0..*') if m else (dato, '1')


def _por_atributo(dc, tipo, a, b, atributo, mult_d=None, mult_o=None, rotulo=True, **kw):
    """Asociación (o composición / agregación) que sale de un atributo de la clase a del tipo b."""
    ms = [m for m in C[a].miembros if m.nombre == atributo and m.tipo in ('campo', 'propiedad')]
    if not ms or _tipo_base(ms[0].tipo_dato)[0] != b:
        raise KeyError('%s no tiene el atributo %s : %s' % (a, atributo, b))
    if not rotulo:
        dc.rel(tipo, a, b, **kw)
        return
    rol = codigo.VIS.get(ms[0].vis, '+') + atributo
    dc.rel(tipo, a, b, rol=rol, mult_d=mult_d or _tipo_base(ms[0].tipo_dato)[1], mult_o=mult_o, **kw)


def _uso(dc, a, b, estereotipo):
    """Dependencia «use» o «create» que se ve en el código de a (new b(...) o b.Miembro)."""
    patron = r'\bnew\s+%s\s*\(' % b if estereotipo == '«create»' else r'\bnew\s+%s\s*\(|\b%s\.\w' % (b, b)
    if not re.search(patron, _fuente(a)):
        raise KeyError('%s no usa %s' % (a, b))
    dc.rel('dependencia', a, b, nombre=estereotipo)


def _herencia(dc, a, b):
    if b not in [x.strip() for x in (C[a].base or '').split(',')]:
        raise KeyError('%s no hereda de %s' % (a, b))
    dc.rel('realizacion' if b == 'IObservadorIdioma' else 'generalizacion', a, b)


def _diagrama_clases():
    dc = DiagramaCapas('class Diagrama de clases completo - Sistema Agencia de Quiniela', MODULOS)
    for paquete, filas in CAPAS:
        dc.paquete(paquete, [list(f) for f in filas])
    posicion = {}
    for paquete, filas in CAPAS:
        for i, fila in enumerate(filas):
            for k, c in enumerate(fila):
                if c:
                    posicion[c] = (paquete, i, k)
                    a, o = _miembros(c)
                    est = ESTEREOTIPOS.get(c) or ('«static»' if C[c].estatica else None)
                    dc.clase(c, attrs=a, ops=o, estereotipo=est, cursiva=c == 'Permiso')

    # UI -> BLL: atributos de cada pantalla
    for a, b, atributo in [
            ('FormLogin', 'UsuarioBLL', 'bll'), ('FormPrincipal', 'UsuarioBLL', 'bll'),
            ('FormPerfiles', 'PerfilBLL', 'bll'), ('FormUsuarios', 'UsuarioBLL', 'bll'),
            ('FormControlCambios', 'UsuarioBLL', 'usuarioBLL'), ('FormControlCambios', 'PerfilBLL', 'perfilBLL'),
            ('FormBitacora', 'BitacoraBLL', 'bll'), ('FormBitacora', 'PerfilBLL', 'perfilBll'),
            ('FormSeleccionLoteria', 'LoteriaBLL', 'loteriaBll'), ('FormSeleccionLoteria', 'TurnoBLL', 'turnoBll'),
            ('FormApuesta', 'ApuestaBLL', 'bll'), ('FormJuego', 'ApuestaBLL', 'bll'),
            ('FormResultado', 'SorteoBLL', 'bll'),
            ('FormOperacionTicket', 'ApuestaBLL', 'apuestaBll'), ('FormOperacionTicket', 'PremioBLL', 'premioBll'),
            ('FormOperacionTicket', 'PagoBLL', 'pagoBll'),
            ('FormReporte', 'ApuestaBLL', 'apuestaBll'), ('FormReporte', 'SorteoBLL', 'sorteoBll'),
            ('FormReporte', 'PagoBLL', 'pagoBll')]:
        _por_atributo(dc, 'asociacion', a, b, atributo, rotulo=False)
    # BLL -> DAL y BLL -> BLL
    for a, b in [('IdiomaManager', 'IdiomaDAL'), ('UsuarioBLL', 'UsuarioDAL'), ('PerfilBLL', 'PerfilDAL'),
                 ('BitacoraBLL', 'BitacoraDAL'), ('LoteriaBLL', 'LoteriaDAL'), ('TurnoBLL', 'TurnoDAL'),
                 ('JuegoBLL', 'JuegoDAL'), ('ApuestaBLL', 'ApuestaDAL'), ('SorteoBLL', 'SorteoDAL'),
                 ('PagoBLL', 'PagoDAL')]:
        _por_atributo(dc, 'asociacion', a, b, 'dal', rotulo=False)
    _por_atributo(dc, 'asociacion', 'ApuestaBLL', 'SorteoBLL', 'sorteoBll')
    _por_atributo(dc, 'asociacion', 'PremioBLL', 'SorteoBLL', 'sorteoBll')
    _por_atributo(dc, 'asociacion', 'PagoBLL', 'PremioBLL', 'premioBll')
    # patrón Observer: las pantallas que cambian de idioma implementan IObservadorIdioma
    _por_atributo(dc, 'asociacion', 'IdiomaManager', 'IObservadorIdioma', 'observadores', lado_rol=-1)
    for a in ('FormControlIdiomas', 'FormLogin', 'FormPrincipal'):
        _herencia(dc, a, 'IObservadorIdioma')
    # dependencias que vinculan a las clases auxiliares y a las pantallas secundarias
    _uso(dc, 'FormControlIdiomas', 'IdiomaManager', '«use»')
    _uso(dc, 'UsuarioBLL', 'Encriptador', '«use»')
    _uso(dc, 'PerfilBLL', 'Encriptador', '«use»')
    _uso(dc, 'ApuestaBLL', 'Azar', '«use»')
    _uso(dc, 'SorteoBLL', 'Azar', '«use»')
    _uso(dc, 'FormTicket', 'TicketRenderer', '«use»')
    for a, b in [('Program', 'FormLogin'), ('FormLogin', 'FormAvisoIntegridad'), ('FormLogin', 'FormPrincipal'),
                 ('FormSeleccionLoteria', 'FormApuesta'), ('FormApuesta', 'FormTicket'), ('FormJuego', 'FormTicket'),
                 ('FormOperacionTicket', 'FormTicket'), ('FormOperacionTicket', 'FormFelicitaciones'),
                 ('FormReporte', 'FormTicket'), ('FormReporte', 'FormResultado')]:
        _uso(dc, a, b, '«create»')
    # entidades (BE)
    _herencia(dc, 'UsuarioHistorial', 'Usuario')
    _herencia(dc, 'PermisoSimple', 'Permiso')
    _herencia(dc, 'PermisoCompleto', 'Permiso')
    _por_atributo(dc, 'asociacion', 'Usuario', 'Permiso', 'Permisos')
    _por_atributo(dc, 'agregacion', 'PermisoCompleto', 'Permiso', 'SubPermisos', lado_o='der', lado_d='der')
    dc.rel('dependencia', 'IObservadorIdioma', 'Idioma', nombre='«use»')
    for tipo, o, d, rol, md, mo in ASOC_BE:
        # el rol de Jugada -> Sorteo va a la izquierda: a la derecha pasa la línea de Premio -> Jugada
        extra = dict(lado_rol=-1) if (o, d) == ('Jugada', 'Sorteo') else {}
        _por_atributo(dc, tipo, o, d, rol.lstrip('+'), mult_d=md, mult_o=mo, **extra)

    # clases no vecinas de una misma fila: la relación va por arriba (UI) o por abajo de la fila
    for r in dc.rels:
        po, pd = posicion[r.origen], posicion[r.destino]
        if r.lado_o is None and po[:2] == pd[:2] and abs(po[2] - pd[2]) > 1:
            r.lado_o = r.lado_d = LADO_FILA[po[0]]
    return dc


def diagrama_clases():
    return _con_nota(_diagrama_clases, NOTA_CLASES, 900)


# =========================================================================== exportación
def exportar(nombre, fabrica, carpeta_png=None):
    d = fabrica.dibujar()
    d.ident = 'p00'
    DIAGRAMAS.mkdir(parents=True, exist_ok=True)
    (DIAGRAMAS / nombre).write_text(archivo_drawio([d]), encoding='utf-8')
    print('drawio:', nombre)
    if carpeta_png:
        from render import exportar_varios
        destino = Path(carpeta_png) / (Path(nombre).stem + '.png')
        print('png:', exportar_varios([(d, destino)], hilos=1))
    return d


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    png = sys.argv[1] if len(sys.argv) > 1 else None
    exportar('Modelo de datos (DER) - Sistema completo.drawio', der(), png)
    exportar('Diagrama de clases - Sistema completo.drawio', diagrama_clases(), png)
    print('\n'.join(clases.AVISOS) or 'sin cruces')
