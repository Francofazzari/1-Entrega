# -*- coding: utf-8 -*-
"""Construccion de diagramas draw.io (mxGraphModel sin comprimir) con una paleta unica.

Todos los diagramas del documento salen de aca: mismos colores, mismas fuentes y mismos
grosores de linea. Las medidas de texto se toman con la fuente Arial (la que usa draw.io en
Windows para Helvetica) para dimensionar las cajas sin que el texto se corte.
"""
import html
import re
from xml.sax.saxutils import quoteattr

from PIL import ImageFont

# ---------------------------------------------------------------- paleta (una sola para todo)
OSCURO = '#1F4E79'      # encabezados, bordes de figuras (Estilos.Azul del sistema)
MEDIO = '#185FA5'       # acentos (Estilos.AzulMedio)
CLARO = '#EAF1F8'       # relleno de figuras
MUYCLARO = '#F6F9FC'    # relleno de marcos y limites del sistema
TEXTO = '#1F2937'       # texto general
LINEA = '#2D3E50'       # lineas de relaciones y mensajes
GRIS = '#8A99AD'        # notas y lineas secundarias
NOTA = '#F4F6F9'        # relleno de notas
BLANCO = '#FFFFFF'

FUENTE = 'Helvetica'
T = 12                  # tamanio base de texto (px)
TC = 11                 # texto chico (miembros de clases, columnas)

_fuentes = {}


def _f(tam, negrita=False, cursiva=False):
    clave = (tam, negrita, cursiva)
    if clave not in _fuentes:
        nombre = {(False, False): 'arial.ttf', (True, False): 'arialbd.ttf',
                  (False, True): 'ariali.ttf', (True, True): 'arialbi.ttf'}[(negrita, cursiva)]
        _fuentes[clave] = ImageFont.truetype(nombre, tam)
    return _fuentes[clave]


def ancho(texto, tam=T, negrita=False, cursiva=False):
    """Ancho en px de una linea de texto (sin etiquetas html)."""
    if not texto:
        return 0
    return _f(tam, negrita, cursiva).getlength(texto)


def ancho_lineas(lineas, tam=T, negrita=False):
    return max([ancho(l, tam, negrita) for l in lineas] or [0])


def h(texto):
    """Texto plano -> html seguro para el value de una celda con html=1."""
    return html.escape(texto, quote=False)


def estilo(*nombres, **kw):
    partes = list(nombres)
    for k, v in kw.items():
        if v is None:
            continue
        if v is True:
            v = 1
        if v is False:
            v = 0
        partes.append('%s=%s' % (k, v))
    return ';'.join(partes) + ';'


def mezclar(base, extra):
    """Combina dos estilos 'a=1;b=2;' (extra pisa a base)."""
    d = {}
    orden = []
    for s in (base, extra):
        for p in s.split(';'):
            if not p:
                continue
            if '=' in p:
                k, v = p.split('=', 1)
            else:
                k, v = p, None
            if k not in d:
                orden.append(k)
            d[k] = v
    return ''.join((k if d[k] is None else '%s=%s' % (k, d[k])) + ';' for k in orden)


class Diagrama:
    def __init__(self, nombre, ident=None):
        self.nombre = nombre
        self.ident = ident or re.sub(r'[^A-Za-z0-9]+', '_', nombre)[:40]
        self.celdas = []
        self._n = 0
        self.vertices = {}   # id -> (x, y, w, h, parent)
        self.puntos = []     # puntos de aristas (para calcular el tamanio)

    def nid(self, prefijo='c'):
        self._n += 1
        return '%s%d' % (prefijo, self._n)

    # ------------------------------------------------------------ vertices
    def v(self, x, y, w, alto, valor='', est='', padre='1', ident=None, conectable=True):
        ident = ident or self.nid('v')
        x, y, w, alto = round(x, 1), round(y, 1), round(w, 1), round(alto, 1)
        extra = '' if conectable else ' connectable="0"'
        self.celdas.append(
            '<mxCell id=%s value=%s style=%s vertex="1" parent=%s%s><mxGeometry x="%s" y="%s" width="%s" height="%s" as="geometry"/></mxCell>'
            % (quoteattr(ident), quoteattr(valor), quoteattr(est), quoteattr(padre), extra, x, y, w, alto))
        ax, ay = x, y
        if padre != '1' and padre in self.vertices:
            px, py = self.absoluto(padre)
            ax, ay = px + x, py + y
        self.vertices[ident] = (x, y, w, alto, padre, ax, ay)
        return ident

    def absoluto(self, ident):
        x, y, w, alto, padre, ax, ay = self.vertices[ident]
        return ax, ay

    def rect(self, ident):
        x, y, w, alto, padre, ax, ay = self.vertices[ident]
        return ax, ay, w, alto

    # ------------------------------------------------------------ aristas
    def e(self, origen=None, destino=None, valor='', est='', puntos=None, desde=None, hasta=None,
          padre='1', ident=None):
        """Arista. origen/destino: ids de celdas; desde/hasta: puntos sueltos (x, y)."""
        ident = ident or self.nid('e')
        attrs = ''
        if origen:
            attrs += ' source=%s' % quoteattr(origen)
        if destino:
            attrs += ' target=%s' % quoteattr(destino)
        geo = '<mxGeometry relative="1" as="geometry">'
        if desde:
            geo += '<mxPoint x="%s" y="%s" as="sourcePoint"/>' % (round(desde[0], 1), round(desde[1], 1))
            self.puntos.append(desde)
        if hasta:
            geo += '<mxPoint x="%s" y="%s" as="targetPoint"/>' % (round(hasta[0], 1), round(hasta[1], 1))
            self.puntos.append(hasta)
        if puntos:
            geo += '<Array as="points">' + ''.join(
                '<mxPoint x="%s" y="%s"/>' % (round(px, 1), round(py, 1)) for px, py in puntos) + '</Array>'
            self.puntos.extend(puntos)
        geo += '</mxGeometry>'
        self.celdas.append('<mxCell id=%s value=%s style=%s edge="1" parent=%s%s>%s</mxCell>'
                           % (quoteattr(ident), quoteattr(valor), quoteattr(est), quoteattr(padre), attrs, geo))
        return ident

    def etiqueta_arista(self, arista, valor, x, dx=0, dy=0, est=None):
        """Etiqueta extra pegada a una arista. x en [-1, 1] (origen -> destino)."""
        ident = self.nid('l')
        est = est or estilo('edgeLabel', html=1, resizable=0, align='center', verticalAlign='middle',
                            fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO, labelBackgroundColor=BLANCO)
        self.celdas.append(
            '<mxCell id=%s value=%s style=%s vertex="1" connectable="0" parent=%s>'
            '<mxGeometry x="%s" relative="1" as="geometry"><mxPoint x="%s" y="%s" as="offset"/></mxGeometry></mxCell>'
            % (quoteattr(ident), quoteattr(valor), quoteattr(est), quoteattr(arista), round(x, 4), round(dx, 1), round(dy, 1)))
        return ident

    # ------------------------------------------------------------ salida
    def limites(self):
        xs, ys, xs2, ys2 = [], [], [], []
        for ident, (x, y, w, alto, padre, ax, ay) in self.vertices.items():
            xs.append(ax)
            ys.append(ay)
            xs2.append(ax + w)
            ys2.append(ay + alto)
        for px, py in self.puntos:
            xs.append(px)
            ys.append(py)
            xs2.append(px)
            ys2.append(py)
        if not xs:
            return 0, 0, 0, 0
        return min(xs), min(ys), max(xs2), max(ys2)

    def modelo(self):
        x0, y0, x1, y1 = self.limites()
        return ('<mxGraphModel dx="0" dy="0" grid="0" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" '
                'fold="0" page="0" pageScale="1" pageWidth="%d" pageHeight="%d" math="0" shadow="0">'
                '<root><mxCell id="0"/><mxCell id="1" parent="0"/>%s</root></mxGraphModel>'
                % (int(x1 + 40), int(y1 + 40), ''.join(self.celdas)))


def archivo_drawio(diagramas):
    """Contenido de un .drawio con una pagina por diagrama."""
    paginas = []
    for d in diagramas:
        paginas.append('<diagram id=%s name=%s>%s</diagram>' % (quoteattr(d.ident), quoteattr(d.nombre), d.modelo()))
    return ('<mxfile host="app.diagrams.net" agent="Documentacion Agencia de Quiniela" version="24.7.17">'
            + ''.join(paginas) + '</mxfile>')


# ---------------------------------------------------------------- estilos comunes
def est_texto(tam=T, negrita=False, alinear='center', valinear='middle', color=TEXTO, cursiva=False, extra=''):
    fs = (1 if negrita else 0) + (2 if cursiva else 0)
    return mezclar(estilo('text', html=1, whiteSpace='wrap', strokeColor='none', fillColor='none',
                          align=alinear, verticalAlign=valinear, fontFamily=FUENTE, fontSize=tam,
                          fontColor=color, fontStyle=fs, spacing=0, overflow='visible'), extra)


def est_caja(relleno=CLARO, borde=OSCURO, tam=T, negrita=False, redondeado=False, extra=''):
    return mezclar(estilo(rounded=1 if redondeado else 0, whiteSpace='wrap', html=1, fillColor=relleno,
                          strokeColor=borde, fontFamily=FUENTE, fontSize=tam, fontColor=TEXTO,
                          fontStyle=1 if negrita else 0, arcSize=12 if redondeado else None), extra)


def est_nota(extra=''):
    return mezclar(estilo(shape='note', whiteSpace='wrap', html=1, size=12, fillColor=NOTA, strokeColor=GRIS,
                          fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO, align='left', verticalAlign='top',
                          spacingLeft=8, spacingRight=8, spacingTop=6), extra)


def est_linea(extra=''):
    return mezclar(estilo(html=1, endArrow='none', strokeColor=LINEA, strokeWidth=1, fontFamily=FUENTE,
                          fontSize=TC, fontColor=TEXTO, labelBackgroundColor=BLANCO, rounded=0,
                          edgeStyle='none'), extra)


def est_marco(titulo_ancho, extra=''):
    """Marco UML (frame) con el pentagono del titulo arriba a la izquierda."""
    return mezclar(estilo(shape='umlFrame', whiteSpace='wrap', html=1, width=titulo_ancho, height=24,
                          boundedLbl=1, verticalAlign='middle', align='left', spacingLeft=8,
                          fillColor='none', swimlaneFillColor=BLANCO, strokeColor=OSCURO,
                          fontFamily=FUENTE, fontSize=T, fontStyle=1, fontColor=OSCURO), extra)


def marco(d, x, y, w, alto, titulo, ident=None):
    """Marco del diagrama (frame UML 2: 'sd ...', 'uc ...', 'class ...', 'act ...')."""
    tw = ancho(titulo, T, True) + 26
    # el texto del pentagono va en el propio marco
    return d.v(x, y, w, alto, h(titulo), est_marco(tw), ident=ident)
