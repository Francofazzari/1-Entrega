# -*- coding: utf-8 -*-
"""Diagramas de clases (diseno y conceptual) y DER, con los miembros tomados del codigo real.

Las clases se ubican en una grilla de columnas comun a todos los paquetes (UI, BLL, DAL, BE: los
proyectos de la solucion), asi las clases relacionadas quedan alineadas y sus lineas son rectas.
"""
from drawio import (Diagrama, ancho, h, estilo, mezclar, est_texto, est_nota, marco, OSCURO, MEDIO,
                    CLARO, MUYCLARO, TEXTO, LINEA, GRIS, BLANCO, FUENTE, T, TC)
from ruteo import Rel, asignar_puertos, asignar_canales, rutear, restricciones, largo, cruces
import codigo

CLASES = codigo.leer_clases()
TABLAS = codigo.leer_tablas()


def _externa(nombre, miembros_ext):
    """Clase del .NET Framework que aparece en un diagrama (solo los miembros que usa el sistema)."""
    c = codigo.Clase(nombre, 'NET', '')
    for tipo, nom, dato, params in miembros_ext:
        c.miembros.append(codigo.Miembro(tipo, 'public', nom, dato, params))
    return c


EXTERNAS = {
    'PrintDocument': _externa('PrintDocument', [
        ('constructor', 'PrintDocument', None, []),
        ('propiedad', 'DocumentName', 'string', None),
        ('propiedad', 'PrintPage', 'PrintPageEventHandler', None),
        ('metodo', 'Print', 'void', [])]),
    'PrintPreviewDialog': _externa('PrintPreviewDialog', [
        ('constructor', 'PrintPreviewDialog', None, []),
        ('propiedad', 'Document', 'PrintDocument', None),
        ('metodo', 'ShowDialog', 'DialogResult', [('IWin32Window', 'owner')])]),
}
CLASES.update(EXTERNAS)

ALTO_LINEA = 16
PAD = 7
CAB = 28
CAB_EST = 40
MAX_MIEMBRO = 255
ALTO_FILA_ER = 22
FILA_ALTERNA = '#EEF3F9'
SEPARADOR = '#D3DDE9'
AVISOS = []


def _partir_miembro(texto):
    if ancho(texto, TC) <= MAX_MIEMBRO:
        return [texto]
    partes = texto.split(', ')
    lineas, actual = [], ''
    for k, p in enumerate(partes):
        trozo = p + (',' if k < len(partes) - 1 else '')
        prueba = (actual + ' ' + trozo) if actual else trozo
        if ancho(prueba, TC) <= MAX_MIEMBRO or not actual:
            actual = prueba
        else:
            lineas.append(actual)
            actual = '      ' + trozo
    lineas.append(actual)
    return lineas


def miembros(nombre, attrs=(), ops=(), conceptual=False):
    """(lineas_atributos, lineas_operaciones): listas de (texto, subrayado); valida contra el codigo."""
    c = CLASES[nombre]
    la, lo = [], []
    for a in attrs:
        ms = [m for m in c.miembros if m.nombre == a and m.tipo in ('campo', 'propiedad', 'constante')]
        if not ms:
            raise KeyError('%s no tiene el atributo %s' % (nombre, a))
        m = ms[0]
        texto = ('%s : %s' % (m.nombre, m.tipo_dato)) if conceptual else m.uml()
        for t in _partir_miembro(texto):
            la.append((t, m.estatico and not conceptual))
    for o in ops:
        n = None
        if '/' in o:
            o, n = o.split('/')
            n = int(n)
        ms = [m for m in c.miembros if m.nombre == o and m.tipo in ('metodo', 'constructor')
              and (n is None or len(m.params) == n)]
        if not ms:
            raise KeyError('%s no tiene la operacion %s' % (nombre, o))
        m = ms[0]
        for t in _partir_miembro(m.uml()):
            lo.append((t, m.estatico))
    return la, lo


def _html_lineas(lineas):
    partes = []
    for texto, sub in lineas:
        t = h(texto).replace('  ', '&nbsp;&nbsp;')
        partes.append('<u>%s</u>' % t if sub else t)
    return '<br>'.join(partes)


def medir_clase(titulo, la, lo, estereotipo=None, conceptual=False, ancho_min=150):
    w = max([ancho(titulo, T, True)] + [ancho(t, TC) for t, s in la + lo] +
            ([ancho(estereotipo, TC)] if estereotipo else []))
    w = max(ancho_min, w + 2 * PAD + 10)
    cab = CAB_EST if estereotipo else CAB
    ha = (len(la) * ALTO_LINEA + 8) if la else 12
    ho = 0 if conceptual else ((len(lo) * ALTO_LINEA + 8) if lo else 12)
    return w, cab + ha + ho, cab, ha, ho


def caja_clase(d, x, y, titulo, la, lo, estereotipo=None, conceptual=False, padre='1', cursiva=False,
               ident=None):
    w, alto, cab, ha, ho = medir_clase(titulo, la, lo, estereotipo, conceptual)
    valor = h(titulo)
    if cursiva:
        valor = '<i>%s</i>' % valor
    if estereotipo:
        valor = '<span style="font-weight:normal;font-size:%dpx">%s</span><br>%s' % (TC, h(estereotipo), valor)
    est = estilo('swimlane', shape='swimlane', html=1, whiteSpace='wrap', fontStyle=1, align='center',
                 verticalAlign='middle', startSize=cab, horizontal=1, collapsible=0, container=1,
                 recursiveResize=0, fillColor=OSCURO, swimlaneFillColor=BLANCO, strokeColor=OSCURO,
                 fontColor=BLANCO, fontFamily=FUENTE, fontSize=T, rounded=0, swimlaneLine=1)
    cid = d.v(x, y, w, alto, valor, est, padre=padre, ident=ident)
    est_t = estilo('text', html=1, align='left', verticalAlign='middle', spacingLeft=PAD, spacingRight=4,
                   spacingTop=0, overflow='hidden', rotatable=0, whiteSpace='nowrap', strokeColor='none',
                   fillColor='none', fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO, points='[]',
                   portConstraint='eastwest')
    yy = cab + 4
    for linea in la:
        d.v(0, yy, w, ALTO_LINEA, _html_lineas([linea]), est_t, padre=cid, conectable=False)
        yy += ALTO_LINEA
    if not conceptual:
        d.v(0, cab + ha, w, 1, '', estilo('line', shape='line', strokeWidth=1, fillColor='none',
                                          strokeColor=OSCURO, html=1, points='[]', rotatable=0),
            padre=cid, conectable=False)
        yy = cab + ha + 4
        for linea in lo:
            d.v(0, yy, w, ALTO_LINEA, _html_lineas([linea]), est_t, padre=cid, conectable=False)
            yy += ALTO_LINEA
    return cid, w, alto


def est_relacion(tipo):
    base = estilo(html=1, rounded=0, edgeStyle='orthogonalEdgeStyle', strokeColor=LINEA, fontFamily=FUENTE,
                  fontSize=TC, fontColor=TEXTO, labelBackgroundColor=BLANCO, jumpStyle='arc', jumpSize=8)
    tipos = {
        'asociacion': estilo(endArrow='open', endFill=0, endSize=10, startArrow='none'),
        'asociacion_simple': estilo(endArrow='none', startArrow='none'),
        'composicion': estilo(startArrow='diamondThin', startFill=1, startSize=16, endArrow='open', endFill=0,
                              endSize=10),
        'composicion_simple': estilo(startArrow='diamondThin', startFill=1, startSize=16, endArrow='none'),
        'agregacion': estilo(startArrow='diamondThin', startFill=0, startSize=16, endArrow='open', endFill=0,
                             endSize=10),
        'dependencia': estilo(endArrow='open', endFill=0, endSize=10, dashed=1, dashPattern='8 4'),
        'generalizacion': estilo(endArrow='block', endFill=0, endSize=14, jumpStyle='none'),
        'realizacion': estilo(endArrow='block', endFill=0, endSize=14, dashed=1, dashPattern='8 4'),
    }
    return mezclar(base, tipos[tipo])


def etiqueta_extremo(d, arista, r, texto, extremo, distancia=None, lado=None, color=TEXTO, cursiva=False):
    """Texto junto a un extremo de la arista (rol y multiplicidad).

    En el extremo destino va arriba del ultimo tramo (o a su derecha si es vertical); en el
    origen, abajo (o a la izquierda), para que los dos textos de una relacion corta no se pisen.
    """
    seq = [r.p0] + list(r.puntos) + [r.p1]
    L = largo(r.p0, r.puntos, r.p1)
    if L <= 0:
        return
    if lado is None:
        lado = 1 if extremo == 'd' else -1
    if extremo == 'd':
        a, b = seq[-2], seq[-1]
    else:
        a, b = seq[0], seq[1]
    lineas = texto.split('\n')
    w = max(ancho(l, TC, cursiva=cursiva) for l in lineas)
    alto = 14 * len(lineas)
    vertical = abs(a[0] - b[0]) < 0.5
    tramo = abs(b[0] - a[0]) + abs(b[1] - a[1])
    dd = (alto / 2 + 12) if vertical else (w / 2 + 14)
    dd = min(dd, max(tramo - 4, 6), L * 0.45)
    x = 1 - 2 * dd / L if extremo == 'd' else -1 + 2 * dd / L
    if vertical:
        dx, dy = lado * (w / 2 + 6), 0
    else:
        dx, dy = 0, -lado * (alto / 2 + 3)
    est = estilo('edgeLabel', html=1, resizable=0, align='center', verticalAlign='middle', fontFamily=FUENTE,
                 fontSize=TC, fontColor=color, fontStyle=2 if cursiva else 0, labelBackgroundColor='none')
    d.etiqueta_arista(arista, '<br>'.join(h(l) for l in lineas), x, dx, dy, est)


def etiqueta_centro(d, arista, r, texto, lado=1, color=OSCURO, cursiva=True):
    seq = [r.p0] + list(r.puntos) + [r.p1]
    L = largo(r.p0, r.puntos, r.p1)
    if L <= 0:
        return
    mejor, acum, pos, vertical = -1, 0, 0, False
    for i in range(len(seq) - 1):
        l = abs(seq[i + 1][0] - seq[i][0]) + abs(seq[i + 1][1] - seq[i][1])
        if l > mejor:
            mejor, pos = l, acum + l / 2
            vertical = abs(seq[i + 1][0] - seq[i][0]) < 0.5
        acum += l
    x = -1 + 2 * pos / L
    w = ancho(texto, TC, cursiva=cursiva)
    if vertical:
        dx, dy = (-1 if lado == -2 else 1) * (w / 2 + 8), 0
    else:
        dx, dy = 0, -(1 if lado > 0 else -1) * 11
    est = estilo('edgeLabel', html=1, resizable=0, align='center', verticalAlign='middle', fontFamily=FUENTE,
                 fontSize=TC, fontColor=color, fontStyle=2 if cursiva else 0, labelBackgroundColor='none')
    d.etiqueta_arista(arista, h(texto), x, dx, dy, est)


def notas(d, lista, ids):
    from secuencia import envolver
    for texto, nx, ny, nw, con in lista:
        lineas = []
        for par in texto.split('\n'):
            lineas.extend(envolver(par, nw - 22, TC))
        nid = d.v(nx, ny, nw, 16 + 14 * len(lineas), '<br>'.join(h(l) for l in lineas), est_nota())
        if con:
            d.e(nid, ids[con], '', estilo(endArrow='none', dashed=1, html=1, strokeColor=GRIS, edgeStyle='none'))


# ------------------------------------------------------------------ diagrama de clases
class DiagramaClases:
    GAP_X = 64
    GAP_FILA = 56
    GAP_PAQ = 58
    MARGEN = 26

    def __init__(self, titulo, nombre=None, conceptual=False, horizontal=False):
        self.titulo = titulo
        self.nombre = nombre or titulo
        self.conceptual = conceptual
        self.horizontal = horizontal      # True: los paquetes van uno al lado del otro
        self.paquetes = []
        self.vistas = {}
        self.rels = []
        self.notas = []

    def paquete(self, nombre, filas):
        self.paquetes.append(dict(nombre=nombre, filas=filas))

    def clase(self, nombre, attrs=(), ops=(), estereotipo=None, cursiva=False, titulo=None):
        la, lo = miembros(nombre, attrs, ops, self.conceptual)
        self.vistas[nombre] = dict(la=la, lo=lo, est=estereotipo, cursiva=cursiva, titulo=titulo or nombre,
                                   attrs=list(attrs), ops=[o.split('/')[0] for o in ops])

    def rel(self, tipo, origen, destino, rol=None, mult_d=None, mult_o=None, nombre=None, lado_o=None,
            lado_d=None, canal=None, f_o=None, f_d=None, puntos=None, lado_rol=None, lado_mult_o=None, lado_nombre=1):
        r = Rel(origen, destino, lado_o, lado_d, canal, tipo=tipo, rol=rol, mult_d=mult_d, mult_o=mult_o,
                nombre=nombre, f_o=f_o, f_d=f_d, puntos=puntos, lado_rol=lado_rol, lado_mult_o=lado_mult_o,
                lado_nombre=lado_nombre)
        self.rels.append(r)
        return r

    def nota(self, texto, x, y, w, conectar_a=None):
        self.notas.append((texto, x, y, w, conectar_a))

    def _carpeta(self, d, x, y, w, alto, nombre):
        tab = max(ancho(nombre, T, True) + 26, 70)
        est_p = estilo(shape='folder', tabWidth=tab, tabHeight=22, tabPosition='left', html=1,
                       whiteSpace='wrap', container=1, collapsible=0, recursiveResize=0, fillColor=MUYCLARO,
                       strokeColor=OSCURO, fontFamily=FUENTE, fontSize=T, fontStyle=1, fontColor=OSCURO,
                       align='left', verticalAlign='top', spacingLeft=10, spacingTop=-1)
        return d.v(x, y, w, alto, h(nombre), est_p)

    def _poner(self, d, pid, x0, y0, xr, yr, c, tam):
        v = self.vistas[c]
        cid, w, alto = caja_clase(d, xr, yr, v['titulo'], v['la'], v['lo'], v['est'], self.conceptual,
                                  padre=pid, cursiva=v['cursiva'], ident='cls_' + c)
        self.cajas[c] = (x0 + xr, y0 + yr, w, alto)
        self.ids[c] = cid

    def _ubicar_vertical(self, d, tam):
        """Paquetes uno debajo del otro, con columnas comunes a todos."""
        ncol = max(len(f) for p in self.paquetes for f in p['filas'])
        anchos = [0] * ncol
        for p in self.paquetes:
            for fila in p['filas']:
                for k, c in enumerate(fila):
                    if c:
                        anchos[k] = max(anchos[k], tam[c][0])
        anchos = [a if a else 60 for a in anchos]
        ancho_paq = sum(anchos) + self.GAP_X * (ncol - 1) + 2 * self.MARGEN
        x0, y = 24, 50
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

    def _ubicar_horizontal(self, d, tam):
        """Paquetes uno al lado del otro (UI | BLL | DAL): diagramas mas bajos, para hojas apaisadas."""
        medidas = []
        for p in self.paquetes:
            ncol = max(len(f) for f in p['filas'])
            anchos = [max([tam[f[k]][0] for f in p['filas'] if f[k]] or [60]) for k in range(ncol)]
            altos = [max([tam[c][1] for c in fila if c] or [40]) for fila in p['filas']]
            w = sum(anchos) + self.GAP_X * (ncol - 1) + 2 * self.MARGEN
            alto = 22 + 30 + sum(altos) + self.GAP_FILA * (len(altos) - 1) + 26
            medidas.append((anchos, altos, w, alto))
        alto_max = max(m[3] for m in medidas)
        x, y0 = 24, 50
        self.paq_de = {}
        for i, (p, (anchos, altos, w, alto)) in enumerate(zip(self.paquetes, medidas)):
            pid = self._carpeta(d, x, y0, w, alto_max, p['nombre'])
            # las filas se centran en la altura del paquete
            yf = 22 + 30 + (alto_max - alto) / 2
            for fila, alto_f in zip(p['filas'], altos):
                xc = self.MARGEN
                for k, c in enumerate(fila):
                    if c:
                        self._poner(d, pid, x, y0, xc + (anchos[k] - tam[c][0]) / 2, yf + (alto_f - tam[c][1]) / 2,
                                    c, tam)
                        self.paq_de[c] = i
                    xc += anchos[k] + self.GAP_X
                yf += alto_f + self.GAP_FILA
            x += w + self.GAP_PAQ
        for r in self.rels:
            po, pd = self.paq_de[r.origen], self.paq_de[r.destino]
            if po != pd and (r.lado_o is None or r.lado_d is None):
                r.lado_o, r.lado_d = ('der', 'izq') if po < pd else ('izq', 'der')

    def _compactar(self):
        if self.horizontal:
            for p in self.paquetes:
                ncol = max(len(f) for f in p['filas'])
                for f in p['filas']:
                    f.extend([None] * (ncol - len(f)))
                usadas = [k for k in range(ncol) if any(f[k] for f in p['filas'])]
                p['filas'] = [[f[k] for k in usadas] for f in p['filas'] if any(f)]
            return
        ncol = max(len(f) for p in self.paquetes for f in p['filas'])
        for p in self.paquetes:
            for f in p['filas']:
                f.extend([None] * (ncol - len(f)))
        usadas = [k for k in range(ncol) if any(f[k] for p in self.paquetes for f in p['filas'])]
        for p in self.paquetes:
            p['filas'] = [[f[k] for k in usadas] for f in p['filas'] if any(f)]

    def dibujar(self):
        self._compactar()
        d = Diagrama(self.nombre)
        tam = {}
        for n, v in self.vistas.items():
            w, alto, cab, ha, ho = medir_clase(v['titulo'], v['la'], v['lo'], v['est'], self.conceptual)
            tam[n] = (w, alto)
        self.cajas, self.ids = {}, {}
        if self.horizontal:
            self._ubicar_horizontal(d, tam)
        else:
            self._ubicar_vertical(d, tam)
        asignar_puertos(self.cajas, self.rels)
        asignar_canales(self.cajas, self.rels)
        for r in self.rels:
            rutear(self.cajas, r)
            est = mezclar(est_relacion(r.datos['tipo']), restricciones(r))
            aid = d.e(self.ids[r.origen], self.ids[r.destino], '', est, puntos=r.puntos)
            fin = ' '.join(t for t in (r.datos.get('rol'), r.datos.get('mult_d')) if t)
            if fin:
                etiqueta_extremo(d, aid, r, fin, 'd', lado=r.datos['lado_rol'])
            if r.datos.get('mult_o'):
                etiqueta_extremo(d, aid, r, r.datos['mult_o'], 'o', lado=r.datos['lado_mult_o'])
            if r.datos.get('nombre'):
                etiqueta_centro(d, aid, r, r.datos['nombre'], r.datos['lado_nombre'])
        AVISOS.extend(cruces(self.cajas, self.rels, self.nombre))
        notas(d, self.notas, self.ids)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 24, y1b + 24, self.titulo)
        d.celdas = d.celdas + celdas
        return d


# ------------------------------------------------------------------ DER
def _clave(col, tabla):
    marcas = []
    if col.pk:
        marcas.append('PK')
    if col.fk:
        marcas.append('FK')
    if col.unico or any(col.nombre in cols and len(cols) == 1 for _, cols in tabla.unicos):
        marcas.append('UQ')
    return ', '.join(marcas)


def medir_tabla(nombre):
    t = TABLAS[nombre]
    w_clave = 48
    w_nom = max(ancho(c.nombre, TC, c.pk) for c in t.columnas) + 14
    w_tipo = max(ancho(c.tipo + (' NULL' if c.nulo else ''), TC) for c in t.columnas) + 14
    w = max(w_clave + w_nom + w_tipo + 8, ancho(nombre, T, True) + 30, 190)
    alto = CAB + len(t.columnas) * ALTO_FILA_ER
    return w, alto, w_clave, w_nom


def caja_tabla(d, x, y, nombre, ident=None):
    t = TABLAS[nombre]
    w, alto, w_clave, w_nom = medir_tabla(nombre)
    est = estilo('swimlane', shape='swimlane', html=1, whiteSpace='wrap', fontStyle=1, align='center',
                 verticalAlign='middle', startSize=CAB, horizontal=1, collapsible=0, container=1,
                 recursiveResize=0, fillColor=OSCURO, swimlaneFillColor=BLANCO, strokeColor=OSCURO,
                 fontColor=BLANCO, fontFamily=FUENTE, fontSize=T, rounded=0)
    tid = d.v(x, y, w, alto, h(nombre), est, ident=ident)
    yy = CAB
    n_pk = sum(1 for c in t.columnas if c.pk)
    for i, c in enumerate(t.columnas):
        fondo = BLANCO if i % 2 == 0 else FILA_ALTERNA
        d.v(1, yy, w - 2, ALTO_FILA_ER, '', estilo(html=1, fillColor=fondo, strokeColor=SEPARADOR, rounded=0,
                                                   points='[]'), padre=tid, conectable=False)
        d.v(0, yy, w_clave, ALTO_FILA_ER, h(_clave(c, t)),
            est_texto(10, True, 'left', color=MEDIO, extra='spacingLeft=8;whiteSpace=nowrap;'),
            padre=tid, conectable=False)
        d.v(w_clave, yy, w_nom, ALTO_FILA_ER, h(c.nombre),
            est_texto(TC, c.pk, 'left', extra='whiteSpace=nowrap;' + ('fontStyle=5;' if c.pk else '')),
            padre=tid, conectable=False)
        d.v(w_clave + w_nom, yy, w - w_clave - w_nom - 8, ALTO_FILA_ER, h(c.tipo + (' NULL' if c.nulo else '')),
            est_texto(TC, False, 'right', color='#5B6B7F', extra='whiteSpace=nowrap;'), padre=tid, conectable=False)
        yy += ALTO_FILA_ER
    return tid, w, alto


MARCAS = {'1': 'ERmandOne', '0..1': 'ERzeroToOne', '0..*': 'ERzeroToMany', '1..*': 'ERoneToMany'}


class DiagramaER:
    GAP_X = 74      # lugar justo para las dos marcas de cardinalidad y un tramo de linea
    GAP_Y = 62

    def __init__(self, titulo, nombre=None):
        self.titulo = titulo
        self.nombre = nombre or titulo
        self.filas = []
        self.rels = []
        self.notas = []

    def grilla(self, filas):
        self.filas = filas

    def rel(self, hija, padre, mult_hija='0..*', lado_h=None, lado_p=None, canal=None, f_h=None, f_p=None,
            puntos=None, columna=None):
        t = TABLAS[hija]
        if columna is None:
            cols = [c for c in t.columnas if c.fk == padre]
            if len(cols) != 1:
                raise ValueError('FK ambigua o inexistente %s -> %s' % (hija, padre))
            columna = cols[0].nombre
        col = t.col(columna)
        if col.fk != padre:
            raise ValueError('%s.%s no referencia a %s' % (hija, columna, padre))
        r = Rel(hija, padre, lado_h, lado_p, canal, mult_h=mult_hija, mult_p='0..1' if col.nulo else '1',
                f_o=f_h, f_d=f_p, puntos=puntos, columna=columna)
        self.rels.append(r)
        return r

    def nota(self, texto, x, y, w, conectar_a=None):
        self.notas.append((texto, x, y, w, conectar_a))

    def tablas(self):
        return [t for fila in self.filas for t in fila if t]

    def dibujar(self):
        d = Diagrama(self.nombre)
        ncol = max(len(f) for f in self.filas)
        anchos = [0] * ncol
        altos = []
        for fila in self.filas:
            altos.append(max([medir_tabla(t)[1] for t in fila if t] or [40]))
            for k, t in enumerate(fila):
                if t:
                    anchos[k] = max(anchos[k], medir_tabla(t)[0])
        anchos = [a if a else 40 for a in anchos]
        self.cajas, self.ids = {}, {}
        y = 60
        for fila, alto_f in zip(self.filas, altos):
            x = 36
            for k, t in enumerate(fila):
                if t:
                    w = medir_tabla(t)[0]
                    tid, w, alto = caja_tabla(d, x + (anchos[k] - w) / 2, y, t, ident='tbl_' + t)
                    self.cajas[t] = (x + (anchos[k] - w) / 2, y, w, alto)
                    self.ids[t] = tid
                x += anchos[k] + self.GAP_X
            y += alto_f + self.GAP_Y
        asignar_puertos(self.cajas, self.rels)
        asignar_canales(self.cajas, self.rels)
        for r in self.rels:
            rutear(self.cajas, r)
            est = estilo(html=1, rounded=0, edgeStyle='orthogonalEdgeStyle', strokeColor=LINEA, endFill=0,
                         startFill=0, startArrow=MARCAS[r.datos['mult_h']], endArrow=MARCAS[r.datos['mult_p']],
                         startSize=12, endSize=12, jumpStyle='arc', jumpSize=8)
            d.e(self.ids[r.origen], self.ids[r.destino], '', mezclar(est, restricciones(r)), puntos=r.puntos)
        AVISOS.extend(cruces(self.cajas, self.rels, self.nombre))
        notas(d, self.notas, self.ids)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 28, y1b + 26, self.titulo)
        d.celdas = d.celdas + celdas
        return d
