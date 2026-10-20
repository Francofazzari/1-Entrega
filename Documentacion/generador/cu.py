# -*- coding: utf-8 -*-
"""Diagramas de casos de uso UML (sistema y negocio)."""
from drawio import (Diagrama, ancho, h, estilo, mezclar, est_texto, est_nota, marco, OSCURO, MEDIO, CLARO,
                    MUYCLARO, TEXTO, LINEA, GRIS, NOTA, BLANCO, FUENTE, T, TC)
from secuencia import envolver

# los diagramas de casos de uso tienen poco texto y mucho aire: se dibujan con letra mas grande
TU = 15         # casos de uso, actores y limite del sistema
TUC = 13        # estereotipos, etiquetas de relaciones y notas
LINEA_NOTA = 17


class DiagramaCU:
    def __init__(self, titulo, nombre=None):
        self.titulo = titulo
        self.nombre = nombre or titulo
        self.d = Diagrama(self.nombre)
        self.limites = []
        self.elementos = {}      # id -> (x, y, w, h, tipo)
        self.aristas = []
        self.notas = []
        self.puntas = set()      # asociaciones que llegan a la punta (izquierda o derecha) del caso de uso
        self.rutas = {}          # (hijo, padre) -> puntos de quiebre de la generalizacion
        self.d.celdas_fondo = []

    def limite(self, texto, x, y, w, alto):
        self.limites.append((texto, x, y, w, alto))

    def actor(self, ident, texto, x, y, estereotipo=None):
        """Actor: figura de 34x64 con el nombre debajo. (x, y) es la esquina superior izquierda."""
        self.elementos[ident] = dict(x=x, y=y, w=34, h=64, tipo='actor', texto=texto, est=estereotipo)

    def caso(self, ident, texto, x, y, w=None, alto=None, estereotipo=None, negocio=False):
        lineas = texto.split('\n')
        if w is None:
            w = max(ancho(l, TU, i == 0 and len(lineas) > 1) for i, l in enumerate(lineas)) + 70
            w = max(w, 170)
        if alto is None:
            alto = max(70, 28 + 19 * len(lineas) + (17 if estereotipo else 0))
        self.elementos[ident] = dict(x=x, y=y, w=w, h=alto, tipo='caso', texto=texto, est=estereotipo,
                                     negocio=negocio)
        return w, alto

    def caja(self, ident):
        e = self.elementos[ident]
        return e['x'], e['y'], e['w'], e['h']

    def centro(self, ident):
        x, y, w, alto = self.caja(ident)
        return x + w / 2, y + alto / 2

    def asociacion(self, a, b, navegable=False, punta=False):
        """punta: la linea llega a la punta del caso de uso (asi un abanico de lineas no cruza otros casos)."""
        self.aristas.append(('asoc', a, b, None, navegable))
        if punta:
            self.puntas.add((a, b))

    def incluye(self, base, incluido):
        self.aristas.append(('include', base, incluido, '«include»', None))

    def extiende(self, extensor, base, condicion=None, punto=None, nota=None):
        """nota: (x, y, w) de la nota con la condicion y el punto de extension."""
        self.aristas.append(('extend', extensor, base, '«extend»', None))
        if nota and (condicion or punto):
            texto = []
            if condicion:
                texto.append('Condición: {%s}' % condicion)
            if punto:
                texto.append('Punto de extensión: %s' % punto)
            self.notas.append(('extend', extensor, base, '\n'.join(texto), nota))

    def generalizacion(self, hijo, padre, puntos=None):
        """puntos: quiebres de la flecha cuando el hijo no esta debajo del padre (sale debajo del nombre del hijo)."""
        self.aristas.append(('gen', hijo, padre, None, None))
        if puntos:
            self.rutas[(hijo, padre)] = puntos

    def nota(self, texto, x, y, w, conectar_a=None):
        self.notas.append(('libre', conectar_a, None, texto, (x, y, w)))

    def dibujar(self):
        d = self.d
        for texto, x, y, w, alto in self.limites:
            d.v(x, y, w, alto, h(texto), estilo(html=1, whiteSpace='wrap', fillColor=MUYCLARO, strokeColor=OSCURO,
                                                verticalAlign='top', align='center', fontFamily=FUENTE, fontSize=TU,
                                                fontStyle=1, fontColor=OSCURO, spacingTop=8, rounded=0,
                                                container=0))
        ids = {}
        for ident, e in self.elementos.items():
            if e['tipo'] == 'actor':
                est = estilo(shape='umlActor', verticalLabelPosition='bottom', verticalAlign='top', html=1,
                             outlineConnect=0, fillColor=CLARO, strokeColor=OSCURO, fontFamily=FUENTE, fontSize=TU,
                             fontStyle=1, fontColor=TEXTO, labelBackgroundColor='none')
                valor = h(e['texto'])
                if e['est']:
                    valor = '<span style="font-weight:normal;font-size:%dpx">%s</span><br>%s' % (TUC, h(e['est']), valor)
                ids[ident] = d.v(e['x'], e['y'], e['w'], e['h'], valor, est, ident='act_' + ident)
                # el nombre va debajo de la figura y es mas ancho que ella: cuenta para el tamanio del marco
                wl = max(ancho(e['texto'], TU, True), ancho(e['est'] or '', TUC))
                cx = e['x'] + e['w'] / 2
                d.puntos.append((cx - wl / 2 - 4, e['y']))
                d.puntos.append((cx + wl / 2 + 4, e['y'] + e['h'] + (40 if e['est'] else 22)))
            else:
                lineas = e['texto'].split('\n')
                partes = []
                if e['est']:
                    partes.append('<span style="font-size:%dpx">%s</span>' % (TUC, h(e['est'])))
                for i, l in enumerate(lineas):
                    if i == 0 and len(lineas) > 1:
                        partes.append('<b>%s</b>' % h(l))
                    else:
                        partes.append(h(l))
                est = estilo('ellipse', html=1, whiteSpace='wrap', fillColor=CLARO, strokeColor=OSCURO,
                             fontFamily=FUENTE, fontSize=TU, fontColor=TEXTO, perimeter='ellipsePerimeter',
                             spacingLeft=10, spacingRight=10)
                ids[ident] = d.v(e['x'], e['y'], e['w'], e['h'], '<br>'.join(partes), est, ident='uc_' + ident)
                if e.get('negocio'):
                    # caso de uso de negocio: barra diagonal (notacion RUP)
                    x, y, w, alto = e['x'], e['y'], e['w'], e['h']
                    d.e(est=estilo(endArrow='none', html=1, strokeColor=OSCURO, strokeWidth=1.5, edgeStyle='none'),
                        desde=(x + w * 0.80, y + alto * 0.10), hasta=(x + w * 0.93, y + alto * 0.30))
        self.ids = ids
        medios = {}
        for tipo, a, b, etq, nav in self.aristas:
            base = estilo(html=1, edgeStyle='none', rounded=0, strokeColor=LINEA, fontFamily=FUENTE, fontSize=TUC,
                          fontColor=OSCURO, labelBackgroundColor=BLANCO, fontStyle=2)
            if tipo == 'asoc':
                est = mezclar(base, estilo(endArrow='open' if nav else 'none', endFill=0, endSize=10))
            elif tipo in ('include', 'extend'):
                est = mezclar(base, estilo(endArrow='open', endFill=0, endSize=10, dashed=1, dashPattern='8 4'))
            else:
                est = mezclar(base, estilo(endArrow='block', endFill=0, endSize=14))
                if self.elementos[a]['tipo'] == 'actor' and self.elementos[b]['tipo'] == 'actor':
                    # el hijo esta debajo del padre: la flecha llega debajo del nombre del padre
                    lineas = 2 if self.elementos[b].get('est') else 1
                    if (a, b) in self.rutas:
                        # el hijo esta a un costado: la flecha sale debajo de su nombre y rodea el limite del sistema
                        lineas_a = 2 if self.elementos[a].get('est') else 1
                        est = mezclar(est, 'exitX=0.5;exitY=1;exitDy=%d;exitPerimeter=0;' % (10 + 20 * lineas_a))
                    else:
                        est = mezclar(est, 'exitX=0.5;exitY=0;exitPerimeter=0;')
                    est = mezclar(est, 'entryX=0.5;entryY=1;entryDy=%d;entryPerimeter=0;' % (10 + 20 * lineas))
            if tipo == 'asoc' and self.elementos[a]['tipo'] == 'actor':
                xa = self.elementos[a]['x']
                lado = 1 if self.centro(b)[0] > xa else 0
                est = mezclar(est, 'exitX=%d;exitY=0.5;exitPerimeter=0;' % lado)
                if (a, b) in self.puntas:
                    est = mezclar(est, 'entryX=%d;entryY=0.5;entryPerimeter=0;' % (1 - lado))
            if tipo == 'asoc' and self.elementos[b]['tipo'] == 'actor':
                eb, ea = self.elementos[b], self.elementos[a]
                if eb['y'] > ea['y'] + ea['h'] and ea['x'] <= eb['x'] + eb['w'] / 2 <= ea['x'] + ea['w']:
                    # actor justo debajo del caso de uso: la linea llega a la cabeza
                    est = mezclar(est, 'entryX=0.5;entryY=0;entryPerimeter=0;')
                else:
                    lado = 1 if self.centro(a)[0] > eb['x'] else 0
                    est = mezclar(est, 'entryX=%d;entryY=0.5;entryPerimeter=0;' % lado)
            d.e(ids[a], ids[b], h(etq) if etq else '', est, puntos=self.rutas.get((a, b)) if tipo == 'gen' else None)
            ca, cb = self.centro(a), self.centro(b)
            medios[(a, b)] = ((ca[0] + cb[0]) / 2, (ca[1] + cb[1]) / 2)
        for tipo, a, b, texto, (x, y, w) in self.notas:
            lineas = []
            for par in texto.split('\n'):
                lineas.extend(envolver(par, w - 22, TUC))
            alto = 18 + LINEA_NOTA * len(lineas)
            d.v(x, y, w, alto, '<br>'.join(h(l) for l in lineas), est_nota('fontSize=%d;' % TUC))
            if tipo == 'extend':
                mx, my = medios[(a, b)]
                # punto de la nota mas cercano al medio de la relacion
                px = min(max(mx, x), x + w)
                py = y + alto if my > y + alto else (y if my < y else my)
                if x <= mx <= x + w:
                    px = mx
                else:
                    px = x if mx < x else x + w
                    py = min(max(my, y), y + alto)
                d.e(est=estilo(endArrow='none', dashed=1, html=1, strokeColor=GRIS, edgeStyle='none'),
                    desde=(px, py), hasta=(mx, my))
            elif a:
                # nota a la derecha del elemento: la linea sale de su borde izquierdo (no cruza el texto)
                ea = self.elementos[a]
                desde = (x, y + alto / 2) if x > ea['x'] + ea['w'] else (x + w / 2, y + alto)
                d.e(None, ids[a], '', estilo(endArrow='none', dashed=1, html=1, strokeColor=GRIS, edgeStyle='none'),
                    desde=desde)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 26, y1b + 26, self.titulo)
        d.celdas = d.celdas + celdas
        return d
