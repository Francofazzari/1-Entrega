# -*- coding: utf-8 -*-
"""Diagramas de actividad UML con carriles (procesos de negocio) y diagramas de flujo (algoritmos).

Los nodos se ubican en una grilla (carril, fila); los flujos son ortogonales: recto si los nodos
estan alineados, en 'L' si cambian de carril, y las vueltas atras van por el costado.
"""
from drawio import (Diagrama, ancho, h, estilo, mezclar, est_texto, est_nota, marco, OSCURO, MEDIO, CLARO,
                    MUYCLARO, TEXTO, LINEA, GRIS, BLANCO, FUENTE, T, TC)
from secuencia import envolver
from ruteo import cruces, Rel

CAB_CARRIL = 32


class DiagramaActividad:
    def __init__(self, titulo, carriles, alto_fila=66, nombre=None, con_carriles=True, x0=24, y0=44):
        """carriles: lista de (titulo, ancho)."""
        self.titulo = titulo
        self.nombre = nombre or titulo
        self.carriles = carriles
        self.alto_fila = alto_fila
        self.con_carriles = con_carriles
        self.x0, self.y0 = x0, y0
        self.nodos = {}
        self.flujos = []
        self.notas = []
        self.leyenda = None
        self.max_fila = 0

    # ------------------------------------------------------------ geometria
    def _x_carril(self, k):
        return self.x0 + sum(w for _, w in self.carriles[:k])

    def _filas(self):
        """Centro vertical de cada fila: cada fila mide lo que necesita su nodo mas alto."""
        altos = {}
        for n in self.nodos.values():
            altos[n['fila']] = max(altos.get(n['fila'], 0), n['h'])
        top = self.y0 + (CAB_CARRIL if self.con_carriles else 0) + 10
        centros = {}
        for f in range(self.max_fila + 1):
            alto = max(self.alto_fila, altos.get(f, 0) + 28)
            centros[f] = top + alto / 2
            top += alto
        self.y_fin = top + 8
        return centros

    def _ubicar(self):
        centros = self._filas()
        for n in self.nodos.values():
            cx = self._x_carril(n['carril']) + self.carriles[n['carril']][1] / 2 + n['dx']
            n['x'] = cx - n['w'] / 2
            n['y'] = centros[n['fila']] - n['h'] / 2

    def _nodo(self, ident, tipo, carril, fila, w, alto, texto='', dx=0, **extra):
        self.nodos[ident] = dict(tipo=tipo, x=0, y=0, w=w, h=alto, texto=texto, carril=carril, fila=fila, dx=dx,
                                 **extra)
        self.max_fila = max(self.max_fila, fila)

    # ------------------------------------------------------------ nodos
    def inicio(self, ident, carril, fila, dx=0):
        self._nodo(ident, 'inicio', carril, fila, 26, 26, dx=dx)

    def fin(self, ident, carril, fila, dx=0):
        self._nodo(ident, 'fin', carril, fila, 28, 28, dx=dx)

    def accion(self, ident, texto, carril, fila, sistema=False, cu=None, w=None, dx=0):
        lineas = []
        for par in texto.split('\n'):
            lineas.extend(envolver(par, (w or 170) - 22, TC))
        extra_cu = 1 if cu else 0
        ancho_txt = max(ancho(l, TC) for l in lineas + ([cu] if cu else []))
        w = w or max(130, min(190, ancho_txt + 26))
        alto = max(40, 16 + 14 * (len(lineas) + extra_cu))
        self._nodo(ident, 'accion', carril, fila, w, alto, texto, dx=dx, lineas=lineas, sistema=sistema, cu=cu)

    def decision(self, ident, carril, fila, texto='', dx=0, texto_izq=False):
        self._nodo(ident, 'decision', carril, fila, 36, 36, texto, dx=dx, texto_izq=texto_izq)

    def objeto(self, ident, texto, carril, fila, dx=0, w=None):
        w = w or max(120, ancho(texto, TC) + 24)
        self._nodo(ident, 'objeto', carril, fila, w, 34, texto, dx=dx)

    def barra(self, ident, carril, fila, w=120, dx=0):
        self._nodo(ident, 'barra', carril, fila, w, 6, dx=dx)

    # ------------------------------------------------------------ flujos
    def flujo(self, a, b, guarda=None, lado_a=None, lado_b=None, puntos=None, vuelta=None, x_vuelta=None,
              f_a=0.5, f_b=0.5, lado_guarda=None, objeto=False):
        self.flujos.append(dict(a=a, b=b, guarda=guarda, lado_a=lado_a, lado_b=lado_b, puntos=puntos,
                                vuelta=vuelta, x_vuelta=x_vuelta, f_a=f_a, f_b=f_b, lado_guarda=lado_guarda,
                                objeto=objeto))

    def nota(self, texto, x, y, w, conectar_a=None):
        self.notas.append((texto, x, y, w, conectar_a))

    def con_leyenda(self, x=None, y=None):
        """Referencias de colores; por defecto, debajo de los carriles."""
        self.leyenda = (x, y)

    # ------------------------------------------------------------ dibujo
    def dibujar(self):
        d = Diagrama(self.nombre)
        self._ubicar()
        alto_total = self.y_fin - self.y0
        if self.con_carriles:
            for k, (titulo, w) in enumerate(self.carriles):
                est = estilo('swimlane', shape='swimlane', html=1, whiteSpace='wrap', startSize=CAB_CARRIL,
                             horizontal=1, collapsible=0, container=0, fillColor=OSCURO, swimlaneFillColor=BLANCO,
                             strokeColor=OSCURO, fontColor=BLANCO, fontFamily=FUENTE, fontSize=T, fontStyle=1,
                             swimlaneLine=1)
                d.v(self._x_carril(k), self.y0, w, alto_total, h(titulo), est)
        ids = {}
        for ident, n in self.nodos.items():
            if n['tipo'] == 'inicio':
                est = estilo('ellipse', html=1, fillColor=LINEA, strokeColor=LINEA)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '', est)
            elif n['tipo'] == 'fin':
                est = estilo('ellipse', shape='endState', html=1, fillColor=LINEA, strokeColor=LINEA)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '', est)
            elif n['tipo'] == 'accion':
                partes = [h(l) for l in n['lineas']]
                if n['cu']:
                    partes.append('<span style="font-size:10px;color:%s;font-weight:bold">%s</span>' % (MEDIO, h(n['cu'])))
                est = estilo('rounded', rounded=1, arcSize=24, html=1, whiteSpace='wrap',
                             fillColor=CLARO if n['sistema'] else BLANCO, strokeColor=OSCURO, fontFamily=FUENTE,
                             fontSize=TC, fontColor=TEXTO, spacing=4)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '<br>'.join(partes), est)
            elif n['tipo'] == 'decision':
                est = estilo('rhombus', html=1, whiteSpace='wrap', fillColor=BLANCO, strokeColor=OSCURO,
                             fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '', est)
                if n['texto']:
                    lineas = n['texto'].split('\n')
                    w = max(ancho(l, TC, cursiva=True) for l in lineas) + 8
                    alto = 14 * len(lineas)
                    if n.get('texto_izq'):
                        d.v(n['x'] - w - 4, n['y'] - alto + 2, w, alto, '<br>'.join(h(l) for l in lineas),
                            est_texto(TC, False, 'right', color=TEXTO, cursiva=True))
                    else:
                        d.v(n['x'] + n['w'] + 4, n['y'] - alto + 2, w, alto, '<br>'.join(h(l) for l in lineas),
                            est_texto(TC, False, 'left', color=TEXTO, cursiva=True))
            elif n['tipo'] == 'objeto':
                est = estilo(html=1, whiteSpace='wrap', fillColor=BLANCO, strokeColor=OSCURO, fontFamily=FUENTE,
                             fontSize=TC, fontColor=TEXTO, rounded=0)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], h(n['texto']), est)
            elif n['tipo'] == 'barra':
                est = estilo(html=1, fillColor=LINEA, strokeColor=LINEA)
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '', est)
            elif n['tipo'] in ('terminal', 'proceso', 'pregunta', 'datos'):
                lineas = n.get('lineas') or [n['texto']]
                ids[ident] = d.v(n['x'], n['y'], n['w'], n['h'], '<br>'.join(h(l) for l in lineas),
                                 figura_flujo(n['tipo']))
        rels = []
        for f in self.flujos:
            a, b = self.nodos[f['a']], self.nodos[f['b']]
            pts, la, lb = self._ruta(f, a, b)
            sal = _restr(la, f['f_a'])
            ent = _restr(lb, f['f_b'])
            est = estilo(html=1, edgeStyle='orthogonalEdgeStyle', rounded=0, strokeColor=LINEA, endArrow='open',
                         endFill=0, endSize=9, fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO,
                         labelBackgroundColor=BLANCO, dashed=1 if f['objeto'] else 0)
            est = mezclar(est, 'exitX=%s;exitY=%s;entryX=%s;entryY=%s;exitPerimeter=0;entryPerimeter=0;' %
                          (sal[0], sal[1], ent[0], ent[1]))
            aid = d.e(ids[f['a']], ids[f['b']], '', est, puntos=pts)
            r = Rel(f['a'], f['b'], la, lb)
            r.p0 = _punto(a, la, f['f_a'])
            r.p1 = _punto(b, lb, f['f_b'])
            r.puntos = pts
            rels.append(r)
            if f['guarda']:
                self._guarda(d, aid, r, f['guarda'], f['lado_guarda'])
        cajas = {k: (n['x'], n['y'], n['w'], n['h']) for k, n in self.nodos.items()}
        self.avisos = cruces(cajas, rels, self.nombre)
        for texto, nx, ny, nw, con in self.notas:
            lineas = []
            for par in texto.split('\n'):
                lineas.extend(envolver(par, nw - 22, TC))
            nid = d.v(nx, ny, nw, 16 + 14 * len(lineas), '<br>'.join(h(l) for l in lineas), est_nota())
            if con:
                d.e(nid, ids[con], '', estilo(endArrow='none', dashed=1, html=1, strokeColor=GRIS, edgeStyle='none'))
        if self.leyenda:
            x, y = self.leyenda
            if y is None:
                y = self.y0 + alto_total + 14
            if x is None:
                x = self.x0
            self._dibujar_leyenda(d, x, y)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 22, y1b + 22, self.titulo)
        d.celdas = d.celdas + celdas
        return d

    def _ruta(self, f, a, b):
        if f['puntos'] is not None:
            return f['puntos'], f['lado_a'] or 'abajo', f['lado_b'] or 'arriba'
        cax, cay = a['x'] + a['w'] / 2, a['y'] + a['h'] / 2
        cbx, cby = b['x'] + b['w'] / 2, b['y'] + b['h'] / 2
        if f['vuelta']:
            lado = 'izq' if f['vuelta'] == 'izq' else 'der'
            sig = -1 if lado == 'izq' else 1
            xv = f['x_vuelta']
            if xv is None:
                # canal sobre el borde del carril (o columna) del nodo de origen: todas las vueltas
                # del mismo lado comparten la misma linea vertical
                k = a['carril']
                xv = (self._x_carril(k) + 14) if sig < 0 else (self._x_carril(k) + self.carriles[k][1] - 14)
                xv = min(xv, a['x'] - 14) if sig < 0 else max(xv, a['x'] + a['w'] + 14)
            la, lb = f['lado_a'] or lado, f['lado_b'] or lado
            pa = _punto(a, la, f['f_a'])
            pb = _punto(b, lb, f['f_b'])
            return [(xv, pa[1]), (xv, pb[1])], la, lb
        la, lb = f['lado_a'], f['lado_b']
        if la is None or lb is None:
            if abs(cax - cbx) < 1:
                la, lb = la or ('abajo' if cby > cay else 'arriba'), lb or ('arriba' if cby > cay else 'abajo')
            elif abs(cay - cby) < 1:
                la, lb = la or ('der' if cbx > cax else 'izq'), lb or ('izq' if cbx > cax else 'der')
            elif cby > cay:
                la, lb = la or ('der' if cbx > cax else 'izq'), lb or 'arriba'
            else:
                la, lb = la or ('der' if cbx > cax else 'izq'), lb or 'abajo'
        pa = _punto(a, la, f['f_a'])
        pb = _punto(b, lb, f['f_b'])
        va, vb = la in ('arriba', 'abajo'), lb in ('arriba', 'abajo')
        if va and vb:
            if abs(pa[0] - pb[0]) < 0.5:
                return [], la, lb
            ym = (pa[1] + pb[1]) / 2
            return [(pa[0], ym), (pb[0], ym)], la, lb
        if not va and not vb:
            if abs(pa[1] - pb[1]) < 0.5:
                return [], la, lb
            xm = (pa[0] + pb[0]) / 2
            return [(xm, pa[1]), (xm, pb[1])], la, lb
        if not va:
            return [(pb[0], pa[1])], la, lb
        return [(pa[0], pb[1])], la, lb

    def _guarda(self, d, aid, r, texto, lado):
        seq = [r.p0] + list(r.puntos) + [r.p1]
        a, b = seq[0], seq[1]
        L = sum(abs(seq[i + 1][0] - seq[i][0]) + abs(seq[i + 1][1] - seq[i][1]) for i in range(len(seq) - 1))
        if L <= 0:
            return
        w = ancho(texto, TC)
        vertical = abs(a[0] - b[0]) < 0.5
        tramo = abs(b[0] - a[0]) + abs(b[1] - a[1])
        dd = min((12 if vertical else w / 2 + 10), max(tramo - 4, 4), L * 0.45)
        x = -1 + 2 * dd / L
        if lado is None:
            lado = 1
        if vertical:
            dx, dy = lado * (w / 2 + 6), 0
        else:
            dx, dy = 0, -lado * 10
        est = estilo('edgeLabel', html=1, resizable=0, align='center', verticalAlign='middle', fontFamily=FUENTE,
                     fontSize=TC, fontColor=OSCURO, labelBackgroundColor='none')
        d.etiqueta_arista(aid, h(texto), x, dx, dy, est)

    def _dibujar_leyenda(self, d, x, y):
        d.v(x, y, 300, 66, '', estilo(html=1, fillColor=BLANCO, strokeColor=GRIS, rounded=0))
        d.v(x + 10, y + 8, 120, 16, 'Referencias', est_texto(TC, True, 'left'))
        d.v(x + 12, y + 30, 40, 24, '', estilo('rounded', rounded=1, arcSize=24, html=1, fillColor=BLANCO,
                                               strokeColor=OSCURO))
        d.v(x + 56, y + 30, 90, 24, 'Actividad manual', est_texto(TC, False, 'left'))
        d.v(x + 152, y + 30, 40, 24, '', estilo('rounded', rounded=1, arcSize=24, html=1, fillColor=CLARO,
                                                strokeColor=OSCURO))
        d.v(x + 196, y + 30, 100, 24, 'Apoyada por el sistema', est_texto(TC, False, 'left'))


def _punto(n, lado, f):
    x, y, w, alto = n['x'], n['y'], n['w'], n['h']
    if n['tipo'] in ('decision', 'pregunta'):
        f = 0.5
    return {'arriba': (x + w * f, y), 'abajo': (x + w * f, y + alto), 'izq': (x, y + alto * f),
            'der': (x + w, y + alto * f)}[lado]


def _restr(lado, f):
    return {'arriba': (f, 0), 'abajo': (f, 1), 'izq': (0, f), 'der': (1, f)}[lado]


# ------------------------------------------------------------------ diagrama de flujo (algoritmos)
class DiagramaFlujo(DiagramaActividad):
    """Diagrama de flujo clasico: inicio/fin (terminal), proceso, decision (rombo con texto), E/S."""

    def __init__(self, titulo, columnas, alto_fila=62, nombre=None):
        super().__init__(titulo, [('', w) for w in columnas], alto_fila, nombre, con_carriles=False)

    def terminal(self, ident, texto, col, fila, dx=0):
        w = max(110, ancho(texto, TC, True) + 36)
        self._nodo(ident, 'terminal', col, fila, w, 34, texto, dx=dx)

    def proceso(self, ident, texto, col, fila, w=None, dx=0):
        lineas = []
        for par in texto.split('\n'):
            lineas.extend(envolver(par, (w or 200) - 20, TC))
        w = w or max(150, min(220, max(ancho(l, TC) for l in lineas) + 24))
        self._nodo(ident, 'proceso', col, fila, w, max(38, 14 + 14 * len(lineas)), texto, dx=dx, lineas=lineas)

    def pregunta(self, ident, texto, col, fila, w=None, dx=0):
        lineas = []
        for par in texto.split('\n'):
            lineas.extend(envolver(par, (w or 170) - 50, TC))
        w = w or max(150, min(230, max(ancho(l, TC) for l in lineas) + 64))
        alto = max(50, 26 + 14 * len(lineas))
        self._nodo(ident, 'pregunta', col, fila, w, alto, texto, dx=dx, lineas=lineas)

    def datos(self, ident, texto, col, fila, w=None, dx=0):
        lineas = []
        for par in texto.split('\n'):
            lineas.extend(envolver(par, (w or 200) - 34, TC))
        w = w or max(150, min(230, max(ancho(l, TC) for l in lineas) + 40))
        self._nodo(ident, 'datos', col, fila, w, max(38, 14 + 14 * len(lineas)), texto, dx=dx, lineas=lineas)


def figura_flujo(tipo):
    if tipo == 'terminal':
        return estilo('rounded', rounded=1, arcSize=50, html=1, whiteSpace='wrap', fillColor=OSCURO,
                      strokeColor=OSCURO, fontFamily=FUENTE, fontSize=TC, fontStyle=1, fontColor=BLANCO)
    if tipo == 'pregunta':
        return estilo('rhombus', html=1, whiteSpace='wrap', fillColor=CLARO, strokeColor=OSCURO,
                      fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO, spacing=2)
    if tipo == 'datos':
        return estilo(shape='parallelogram', perimeter='parallelogramPerimeter', fixedSize=1, size=14, html=1,
                      whiteSpace='wrap', fillColor=BLANCO, strokeColor=OSCURO, fontFamily=FUENTE, fontSize=TC,
                      fontColor=TEXTO)
    return estilo(html=1, whiteSpace='wrap', fillColor=BLANCO, strokeColor=OSCURO, fontFamily=FUENTE, fontSize=TC,
                  fontColor=TEXTO, rounded=0)
