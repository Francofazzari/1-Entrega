# -*- coding: utf-8 -*-
"""Diagramas de secuencia UML 2 en draw.io, con disposicion automatica.

Uso:
    s = Secuencia('sd Vender apuesta')
    s.actor('op', 'Operador'); s.objeto('f', ':FormApuesta'); s.bd('bd', 'AgenciaQuiniela')
    with s.llamada('op', 'f', 'btnEnviar_Click(sender, e)'):
        with s.llamada('f', 'bll', 'Vender(...)', ret='a'):
            ...
    d = s.dibujar()

Cada llamada sincronica abre una barra de activacion en el destino y, si devuelve un valor,
dibuja el mensaje de respuesta (linea punteada). Los mensajes se numeran en orden.
"""
import re
from contextlib import contextmanager

from drawio import (Diagrama, ancho, h, estilo, mezclar, est_texto, est_nota, marco, OSCURO, CLARO,
                    TEXTO, LINEA, GRIS, BLANCO, FUENTE, T, TC)

ALTO_CAB = 44          # alto de la cabecera de cada linea de vida
ANCHO_ACT = 12         # ancho de la barra de activacion
FILA = 29              # alto de una fila de mensaje (una linea de texto)
FILA_EXTRA = 13        # por cada linea adicional de la etiqueta
Y0 = 40                # comienzo de las cabeceras dentro del marco


class _Ev:
    def __init__(self, tipo, **kw):
        self.tipo = tipo
        self.hijos = []
        self.num = None
        self.num_ret = None
        self.__dict__.update(kw)


class Secuencia:
    def __init__(self, titulo, nombre=None):
        self.titulo = titulo
        self.nombre = nombre or titulo
        self.lineas = []
        self.raiz = _Ev('raiz')
        self.pila = [self.raiz]
        self.numerar = True

    # ------------------------------------------------------------ participantes
    def actor(self, ident, texto):
        self.lineas.append(dict(id=ident, texto=texto, tipo='actor', creado=False))

    def objeto(self, ident, texto, creado=False):
        self.lineas.append(dict(id=ident, texto=texto, tipo='objeto', creado=creado))

    def bd(self, ident, texto):
        self.lineas.append(dict(id=ident, texto=texto, tipo='bd', creado=False))

    def clases(self):
        """Clases que aparecen como lineas de vida (para el balanceo con el diagrama de clases)."""
        return [l['texto'].split(':')[-1].strip() for l in self.lineas if l['tipo'] == 'objeto']

    # ------------------------------------------------------------ eventos
    def _agregar(self, ev):
        self.pila[-1].hijos.append(ev)
        return ev

    @contextmanager
    def llamada(self, origen, destino, texto, ret=None):
        ev = self._agregar(_Ev('llamada', origen=origen, destino=destino, texto=texto, ret=ret))
        self.pila.append(ev)
        yield ev
        self.pila.pop()

    def mensaje(self, origen, destino, texto, ret=None):
        with self.llamada(origen, destino, texto, ret):
            pass

    @contextmanager
    def crear(self, origen, destino, texto):
        ev = self._agregar(_Ev('crear', origen=origen, destino=destino, texto=texto, ret=None))
        self.pila.append(ev)
        yield ev
        self.pila.pop()

    def asincrono(self, origen, destino, texto):
        self._agregar(_Ev('asinc', origen=origen, destino=destino, texto=texto, ret=None))

    def respuesta(self, origen, destino, texto):
        self._agregar(_Ev('resp', origen=origen, destino=destino, texto=texto, ret=None))

    def nota(self, linea, texto, ancho_nota=230):
        self._agregar(_Ev('nota', linea=linea, texto=texto, ancho=ancho_nota))

    def destruir(self, linea):
        self._agregar(_Ev('destruir', linea=linea))

    @contextmanager
    def fragmento(self, tipo, guarda=''):
        ev = self._agregar(_Ev('frag', clase=tipo, operandos=[]))
        op = _Ev('operando', guarda=guarda)
        ev.operandos.append(op)
        self.pila.append(op)
        yield ev
        self.pila.pop()

    @contextmanager
    def alternativa(self):
        ev = self._agregar(_Ev('frag', clase='alt', operandos=[]))
        yield _Alt(self, ev)

    def referencia(self, texto, lineas):
        self._agregar(_Ev('ref', texto=texto, lineas=lineas))

    # ------------------------------------------------------------ recorrido
    def _recorrer(self, nodo, f):
        for ev in nodo.hijos:
            f(ev)
            if ev.tipo == 'frag':
                for op in ev.operandos:
                    self._recorrer(op, f)
            else:
                self._recorrer(ev, f)

    def mensajes(self):
        """Lista (origen, destino, texto, tipo) de todos los mensajes (para validar contra el codigo)."""
        res = []

        def f(ev):
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                res.append((ev.origen, ev.destino, ev.texto, ev.tipo))
        self._recorrer(self.raiz, f)
        return res

    def _numerar(self):
        n = [0]

        def f(ev):
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                n[0] += 1
                ev.num = n[0]
        # el numero de la respuesta va despues de los mensajes internos: recorrido en orden
        def rec(nodo):
            for ev in nodo.hijos:
                if ev.tipo == 'frag':
                    for op in ev.operandos:
                        rec(op)
                    continue
                if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                    n[0] += 1
                    ev.num = n[0]
                rec(ev)
                if ev.tipo == 'llamada' and ev.ret and ev.origen != ev.destino:
                    n[0] += 1
                    ev.num_ret = n[0]
        rec(self.raiz)

    MAX_ETQ = 250
    MAX_PROPIA = 175

    def _etq(self, texto, numero, propia=False):
        t = '%d: %s' % (numero, texto) if self.numerar and numero else texto
        maximo = self.MAX_PROPIA if propia else self.MAX_ETQ
        # los saltos de renglon escritos a mano se respetan, y cada renglon que sigue siendo largo se parte
        lineas = []
        for linea in t.split('\n'):
            if ancho(linea, TC) <= maximo:
                lineas.append(linea)
            else:
                lineas.extend(partir(linea, maximo, TC))
        return '\n'.join(lineas)

    @staticmethod
    def _lineas_texto(texto):
        return texto.split('\n')

    def _ancho_etq(self, texto):
        return max(ancho(l, TC) for l in self._lineas_texto(texto))

    def _indice(self):
        return {l['id']: i for i, l in enumerate(self.lineas)}

    # ------------------------------------------------------------ posiciones horizontales
    def _calcular_x(self):
        idx = self._indice()
        n = len(self.lineas)
        anchos_cab = []
        for l in self.lineas:
            if l['tipo'] == 'actor':
                anchos_cab.append(max(70, ancho(l['texto'], T, True) + 16))
            else:
                anchos_cab.append(max(92, max(ancho(t, T, True) for t in l['texto'].split('\n')) + 20))
        self.anchos_cab = anchos_cab
        restr = []
        for i in range(n - 1):
            restr.append((i, i + 1, (anchos_cab[i] + anchos_cab[i + 1]) / 2 + 16))
        self.x0_min = 0

        def visitar(ev):
            if ev.tipo == 'frag':
                # el marco no puede tapar con su pestania la barra de activacion de su primera linea de
                # vida, ni invadir las lineas de vida vecinas que no participan
                usadas = set()
                for op in ev.operandos:
                    usadas |= self._lineas_en(op)
                ids = sorted(idx[u] for u in usadas)
                i, j = ids[0], ids[-1]
                tw = self._ancho_tab(ev.clase)
                nivel = self._niveles(ev)
                m_izq = tw + 18 + 10 * nivel
                m_der = 42 + 10 * nivel
                # cada guarda va junto a la linea de vida del primer evento de su operando (UML), a la derecha
                # de sus barras de activacion
                for op in ev.operandos:
                    if op.guarda:
                        L = idx[self._primera_linea(op)]
                        w_g = self.X_GUARDA + ancho(op.guarda, TC, True) + 12
                        if L == j:
                            m_der = max(m_der, w_g)
                        elif L + 1 < n:
                            restr.append((L, L + 1, w_g + 16))
                # los mensajes a si misma y las notas de la ultima linea de vida ensanchan el marco hacia la derecha
                m_der = max(m_der, self._extension_derecha(ev, ids[-1], idx) + 16 + 16 * nivel)
                if j + 1 < n:
                    restr.append((j, j + 1, m_der + 16))
                if i > 0:
                    restr.append((i - 1, i, m_izq + 16))
                else:
                    self.x0_min = max(self.x0_min, m_izq + 12)
                return
            if ev.tipo == 'ref':
                ids = sorted(idx[u] for u in ev.lineas)
                i, j = ids[0], ids[-1]
                w_need = self._ancho_ref(ev)
                if j + 1 < n:
                    restr.append((i, j + 1, w_need - 48 + 20))
                if i == 0:
                    self.x0_min = max(self.x0_min, 60)
                return
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                etq = self._etq(ev.texto, ev.num, ev.origen == ev.destino)
                a, b = idx[ev.origen], idx[ev.destino]
                w = self._ancho_etq(etq)
                if a == b:
                    if a + 1 < n:
                        restr.append((a, a + 1, 48 + w + 16))
                else:
                    i, j = min(a, b), max(a, b)
                    extra = anchos_cab[b] / 2 if ev.tipo == 'crear' else 0
                    restr.append((i, j, w + 36 + extra))
                if ev.tipo == 'llamada' and ev.num_ret:
                    wr = self._ancho_etq(self._etq(ev.ret, ev.num_ret))
                    restr.append((min(a, b), max(a, b), wr + 36))
            elif ev.tipo == 'nota':
                a = idx[ev.linea]
                if a + 1 < n:
                    restr.append((a, a + 1, 24 + ev.ancho + 20))
        self._recorrer(self.raiz, visitar)

        x = [0.0] * n
        x[0] = max(34 + anchos_cab[0] / 2, self.x0_min)
        for i in range(1, n):
            x[i] = x[i - 1] + (anchos_cab[i - 1] + anchos_cab[i]) / 2 + 16
        for _ in range(80):
            cambio = False
            for i, j, dist in restr:
                falta = dist - (x[j] - x[i])
                if falta > 0.5:
                    cambio = True
                    por = falta / (j - i)
                    for k in range(i + 1, n):
                        x[k] += por * min(k - i, j - i)
            if not cambio:
                break
        self.x = x

    # ------------------------------------------------------------ dibujo
    def dibujar(self):
        self._numerar()
        self._calcular_x()
        d = Diagrama(self.nombre)
        self.d = d
        n = len(self.lineas)
        self.y = Y0 + ALTO_CAB + 22
        self.activas = {l['id']: [] for l in self.lineas}
        self.inicio = {}
        self.fin = {}
        self.barras = []
        self.max_x_extra = 0
        self.fondo = []          # marcos de fragmentos (van debajo de los mensajes)
        self._dibujar_nodo(self.raiz, 0)
        y_fin = self.y + 16

        mensajes = d.celdas
        d.celdas = []
        for i, l in enumerate(self.lineas):
            ident = l['id']
            y_ini = self.inicio.get(ident, Y0)
            y_f = self.fin.get(ident, y_fin)
            w = self.anchos_cab[i]
            cx = self.x[i]
            if l['tipo'] == 'actor':
                est = estilo(shape='umlLifeline', participant='umlActor', perimeter='lifelinePerimeter',
                             whiteSpace='wrap', html=1, container=0, collapsible=0, recursiveResize=0,
                             outlineConnect=0, portConstraint='eastwest', size=ALTO_CAB - 6, fillColor=CLARO,
                             strokeColor=OSCURO, fontFamily=FUENTE, fontSize=T, fontColor=TEXTO)
                d.v(cx - 15, y_ini, 30, y_f - y_ini, '', est, ident='ll_' + ident)
                d.v(cx - w / 2, y_ini + ALTO_CAB - 4, w, 16, h(l['texto']),
                    est_texto(T, True, extra='labelBackgroundColor=#FFFFFF;'), ident='lt_' + ident)
            elif l['tipo'] == 'bd':
                est = estilo(shape='cylinder3', whiteSpace='wrap', html=1, boundedLbl=1, backgroundOutline=1,
                             size=7, fillColor=CLARO, strokeColor=OSCURO, fontFamily=FUENTE, fontSize=TC,
                             fontColor=TEXTO, fontStyle=1)
                d.v(cx - w / 2, y_ini - 4, w, ALTO_CAB + 4, '&laquo;database&raquo;<br>' + h(l['texto']), est,
                    ident='ll_' + ident)
                d.e(est=estilo(endArrow='none', dashed=1, html=1, strokeColor=OSCURO, edgeStyle='none'),
                    desde=(cx, y_ini + ALTO_CAB), hasta=(cx, y_f))
            else:
                est = estilo(shape='umlLifeline', perimeter='lifelinePerimeter', whiteSpace='wrap', html=1,
                             container=0, collapsible=0, recursiveResize=0, outlineConnect=0,
                             portConstraint='eastwest', size=ALTO_CAB, fillColor=CLARO, strokeColor=OSCURO,
                             fontFamily=FUENTE, fontSize=T, fontColor=TEXTO, fontStyle=1, verticalAlign='top',
                             spacingTop=6 if '\n' in l['texto'] else 13)
                texto = '<br>'.join(h(t) for t in l['texto'].split('\n'))
                d.v(cx - w / 2, y_ini, w, y_f - y_ini, texto, est, ident='ll_' + ident)
                if ident in self.fin:
                    xs = 9
                    est_x = estilo(endArrow='none', html=1, strokeColor=OSCURO, strokeWidth=2, edgeStyle='none')
                    d.e(est=est_x, desde=(cx - xs, y_f - xs), hasta=(cx + xs, y_f + xs))
                    d.e(est=est_x, desde=(cx - xs, y_f + xs), hasta=(cx + xs, y_f - xs))
        vidas = d.celdas
        d.celdas = []
        for (lid, x0, y0, y1) in self.barras:
            d.v(x0, y0, ANCHO_ACT, max(8, y1 - y0), '',
                estilo(html=1, points='[]', perimeter='orthogonalPerimeter', outlineConnect=0,
                       fillColor=BLANCO, strokeColor=OSCURO))
        barras = d.celdas
        d.celdas = []
        x_der = max(self.x[i] + self.anchos_cab[i] / 2 for i in range(n))
        x_der = max(x_der, self.max_x_extra) + 26
        marco(d, 0, 0, x_der, y_fin + 16, self.titulo)
        d.celdas = d.celdas + vidas + self.fondo + barras + mensajes
        return d

    def _x_act(self, ident, lado):
        i = self._indice()[ident]
        cx = self.x[i]
        pila = self.activas[ident]
        if not pila or self.lineas[i]['tipo'] == 'actor':
            return cx
        nivel = pila[-1][1]
        x0 = cx - ANCHO_ACT / 2 + nivel * 6
        return x0 + ANCHO_ACT if lado > 0 else x0

    def _abrir(self, ident, y):
        nivel = len(self.activas[ident])
        self.activas[ident].append((y, nivel))

    def _cerrar(self, ident, y):
        i = self._indice()[ident]
        y0, nivel = self.activas[ident].pop()
        if self.lineas[i]['tipo'] != 'actor':
            x0 = self.x[i] - ANCHO_ACT / 2 + nivel * 6
            self.barras.append((ident, x0, y0, y))

    def _flecha(self, x0, x1, y, texto, tipo):
        lineas = self._lineas_texto(texto)
        valor = '<br>'.join(h(l) for l in lineas)
        base = estilo(html=1, verticalAlign='bottom', labelBackgroundColor=BLANCO, edgeStyle='none',
                      fontFamily=FUENTE, fontSize=TC, fontColor=TEXTO, strokeColor=LINEA, rounded=0,
                      spacingBottom=1)
        if tipo == 'llamada':
            est = mezclar(base, estilo(endArrow='block', endFill=1, endSize=8))
        elif tipo in ('resp', 'crear'):
            est = mezclar(base, estilo(endArrow='open', endFill=0, endSize=9, dashed=1, dashPattern='6 4'))
        else:
            est = mezclar(base, estilo(endArrow='open', endFill=0, endSize=9))
        self.d.e(valor=valor, est=est, desde=(x0, y), hasta=(x1, y))

    def _propia(self, ident, y, texto):
        x0 = self._x_act(ident, +1)
        lineas = self._lineas_texto(texto)
        valor = '<br>'.join(h(l) for l in lineas)
        est = estilo(html=1, endArrow='block', endFill=1, endSize=7, edgeStyle='none', rounded=0,
                     strokeColor=LINEA)
        xa = x0 + 30
        self.d.e(valor='', est=est, desde=(x0, y), hasta=(x0 + 6, y + 16), puntos=[(xa, y), (xa, y + 16)])
        w = self._ancho_etq(texto)
        alto = 14 * len(lineas)
        self.d.v(xa + 6, y + 8 - alto / 2, w + 4, alto, valor,
                 est_texto(TC, alinear='left', extra='labelBackgroundColor=#FFFFFF;'))
        self.max_x_extra = max(self.max_x_extra, xa + 12 + w)

    def _dibujar_nodo(self, nodo, prof):
        for ev in nodo.hijos:
            getattr(self, '_ev_' + ev.tipo)(ev, prof)

    def _alto_fila(self, texto):
        return FILA + FILA_EXTRA * (len(self._lineas_texto(texto)) - 1)

    def _ev_llamada(self, ev, prof):
        etq = self._etq(ev.texto, ev.num, ev.origen == ev.destino)
        if ev.origen == ev.destino:
            extra = 7 * (len(self._lineas_texto(etq)) - 1)
            y = self.y + 6 + extra
            self._propia(ev.origen, y, etq)
            self.y = y + 16
            self._abrir(ev.destino, self.y)
            self.y += 9
            self._dibujar_nodo(ev, prof)
            self.y += 1
            self._cerrar(ev.destino, self.y)
            self.y += 4 + extra
            return
        self.y += self._alto_fila(etq) - 8
        y = self.y
        i, j = self._indice()[ev.origen], self._indice()[ev.destino]
        lado = 1 if j > i else -1
        x0 = self._x_act(ev.origen, lado)
        self._abrir(ev.destino, y)
        x1 = self._x_act(ev.destino, -lado)
        self._flecha(x0, x1, y, etq, 'llamada')
        self.y = y + 10
        self._dibujar_nodo(ev, prof)
        if ev.num_ret:
            etq_r = self._etq(ev.ret, ev.num_ret)
            self.y += self._alto_fila(etq_r) - 10
            y2 = self.y
            xr0 = self._x_act(ev.destino, -lado)
            self._cerrar(ev.destino, y2)
            xr1 = self._x_act(ev.origen, lado)
            self._flecha(xr0, xr1, y2, etq_r, 'resp')
            self.y = y2 + 8
        else:
            self.y += 5
            self._cerrar(ev.destino, self.y)
            self.y += 4

    def _ev_crear(self, ev, prof):
        etq = self._etq(ev.texto, ev.num)
        i, j = self._indice()[ev.origen], self._indice()[ev.destino]
        lado = 1 if j > i else -1
        self.y += 10
        y_cab = self.y
        self.inicio[ev.destino] = y_cab
        yc = y_cab + ALTO_CAB / 2
        x0 = self._x_act(ev.origen, lado)
        x1 = self.x[j] - lado * self.anchos_cab[j] / 2
        self._flecha(x0, x1, yc, etq, 'crear')
        self.y = y_cab + ALTO_CAB + 4
        if not ev.hijos:
            # constructor sin mensajes propios: no se dibuja una barra de activacion vacia
            self.y += 2
            return
        self._abrir(ev.destino, self.y)
        self.y += 8
        self._dibujar_nodo(ev, prof)
        self.y += 2
        self._cerrar(ev.destino, self.y)
        self.y += 4

    def _ev_asinc(self, ev, prof):
        self._simple(ev, 'asinc')

    def _ev_resp(self, ev, prof):
        self._simple(ev, 'resp')

    def _simple(self, ev, tipo):
        etq = self._etq(ev.texto, ev.num)
        i, j = self._indice()[ev.origen], self._indice()[ev.destino]
        lado = 1 if j > i else -1
        self.y += self._alto_fila(etq) - 8
        x0 = self._x_act(ev.origen, lado)
        x1 = self._x_act(ev.destino, -lado)
        self._flecha(x0, x1, self.y, etq, tipo)
        self.y += 10

    def _ev_nota(self, ev, prof):
        i = self._indice()[ev.linea]
        lineas = []
        for parrafo in ev.texto.split('\n'):
            lineas.extend(envolver(parrafo, ev.ancho - 20, TC))
        alto = 14 + 14 * len(lineas)
        x0 = self.x[i] + 24
        y0 = self.y + 8
        self.d.v(x0, y0, ev.ancho, alto, '<br>'.join(h(l) for l in lineas), est_nota())
        self.max_x_extra = max(self.max_x_extra, x0 + ev.ancho)
        xa = self._x_act(ev.linea, +1)
        self.d.e(est=estilo(endArrow='none', dashed=1, html=1, strokeColor=GRIS, edgeStyle='none'),
                 desde=(xa, y0 + 10), hasta=(x0, y0 + 10))
        self.y = y0 + alto + 4

    def _ev_destruir(self, ev, prof):
        self.y += 12
        self.fin[ev.linea] = self.y
        self.y += 8

    def _lineas_en(self, nodo):
        usadas = set()

        def f(ev):
            for k in ('origen', 'destino', 'linea'):
                if hasattr(ev, k):
                    usadas.add(getattr(ev, k))
            if ev.tipo == 'ref':
                usadas.update(ev.lineas)
        self._recorrer(nodo, f)
        return usadas

    X_GUARDA = 26          # las guardas empiezan a esta distancia de la linea de vida de su primer evento

    def _primera_linea(self, nodo):
        """Linea de vida del primer evento de un operando (donde UML ubica la guarda)."""
        for ev in nodo.hijos:
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                return ev.origen
            if ev.tipo in ('nota', 'destruir'):
                return ev.linea
            if ev.tipo == 'ref':
                return ev.lineas[0]
            if ev.tipo == 'frag' and ev.operandos:
                return self._primera_linea(ev.operandos[0])
        return None

    @staticmethod
    def _ancho_tab(titulo):
        return max(ancho(titulo, TC, True) + 24, 46)

    def _extension_derecha(self, frag, j, idx):
        """Cuanto se extienden a la derecha de la linea de vida j los mensajes a si misma y las notas que estan
        dentro del fragmento (medido desde el centro de la linea de vida)."""
        m = [0]

        def f(ev):
            if ev.tipo == 'llamada' and ev.origen == ev.destino and idx[ev.origen] == j:
                w = self._ancho_etq(self._etq(ev.texto, ev.num, True))
                m[0] = max(m[0], 48 + 12 + w)
            elif ev.tipo == 'nota' and idx[ev.linea] == j:
                m[0] = max(m[0], 24 + ev.ancho)
        for op in frag.operandos:
            self._recorrer(op, f)
        return m[0]

    def _niveles(self, ev):
        """Cantidad de niveles de fragmentos anidados dentro de un fragmento."""
        m = 0
        nodos = ev.operandos if ev.tipo == 'frag' else [ev]
        for nodo in nodos:
            for h_ev in nodo.hijos:
                if h_ev.tipo == 'frag':
                    m = max(m, 1 + self._niveles(h_ev))
                elif h_ev.hijos:
                    m = max(m, self._niveles(h_ev))
        return m

    ANCHO_REF = 300

    def _lineas_ref(self, ev):
        return envolver(ev.texto, self.ANCHO_REF, TC)

    def _ancho_ref(self, ev):
        return max(ancho(l, TC) for l in self._lineas_ref(ev)) + 36

    def _ev_frag(self, ev, prof):
        d = self.d
        idx = self._indice()
        usadas = set()
        for op in ev.operandos:
            usadas |= self._lineas_en(op)
        ids = sorted(idx[u] for u in usadas)
        titulo = ev.clase
        tw = self._ancho_tab(titulo)
        nivel = self._niveles(ev)
        x0 = self.x[ids[0]] - (tw + 18 + 10 * nivel)
        x1 = self.x[ids[-1]] + 42 + 10 * nivel
        extra_antes = self.max_x_extra
        self.max_x_extra = 0
        y0 = self.y + 8
        self.y = y0 + 20
        separadores = []
        guardas = []
        for k, op in enumerate(ev.operandos):
            xg = self.x[idx[self._primera_linea(op)]] + self.X_GUARDA
            if k == 0:
                if op.guarda:
                    guardas.append((xg, y0 + 3, op.guarda))
            else:
                self.y += 10
                ys = self.y
                separadores.append(ys)
                if op.guarda:
                    guardas.append((xg, ys + 3, op.guarda))
                self.y = ys + 18
            self._dibujar_nodo(op, prof + 1)
        self.y += 8
        y1 = self.y
        x1 = max([x1, self.max_x_extra + 16] +
                 [gx + ancho(g, TC, True) + 12 for gx, gy, g in guardas])
        self.max_x_extra = max(extra_antes, x1)
        est = estilo(shape='umlFrame', whiteSpace='wrap', html=1, width=tw, height=20, boundedLbl=1,
                     verticalAlign='middle', align='left', spacingLeft=6, fillColor=CLARO, strokeColor=OSCURO,
                     fontFamily=FUENTE, fontSize=TC, fontStyle=1, fontColor=OSCURO, swimlaneFillColor='none')
        previas = d.celdas
        d.celdas = []
        d.v(x0, y0, x1 - x0, y1 - y0, h(titulo), est)
        for ys in separadores:
            d.e(est=estilo(endArrow='none', dashed=1, dashPattern='8 4', html=1, strokeColor=OSCURO,
                           edgeStyle='none'), desde=(x0, ys), hasta=(x1, ys))
        marcos = d.celdas
        d.celdas = []
        for (gx, gy, guarda) in guardas:
            d.v(gx, gy, ancho(guarda, TC, True) + 6, 15, h(guarda),
                est_texto(TC, True, 'left', color=OSCURO, extra='labelBackgroundColor=#FFFFFF;'))
        self.fondo = marcos + self.fondo
        d.celdas = previas + d.celdas
        self.y += 4

    def _ev_ref(self, ev, prof):
        idx = self._indice()
        ids = sorted(idx[u] for u in ev.lineas)
        lineas = self._lineas_ref(ev)
        x0 = self.x[ids[0]] - 48 + 6 * prof
        x1 = max(self.x[ids[-1]] + 48 - 6 * prof, x0 + self._ancho_ref(ev))
        y0 = self.y + 10
        alto = 24 + 14 * len(lineas) + 8
        # fondo blanco: la referencia tapa las lineas de vida y barras que cubre
        self.d.v(x0, y0, x1 - x0, alto, '', estilo(html=1, fillColor=BLANCO, strokeColor='none', points='[]'))
        est = estilo(shape='umlFrame', whiteSpace='wrap', html=1, width=40, height=20, boundedLbl=1,
                     verticalAlign='middle', align='left', spacingLeft=8, fillColor=CLARO, strokeColor=OSCURO,
                     fontFamily=FUENTE, fontSize=TC, fontStyle=1, fontColor=OSCURO, swimlaneFillColor=BLANCO)
        self.d.v(x0, y0, x1 - x0, alto, 'ref', est)
        self.d.v(x0 + 10, y0 + 22, x1 - x0 - 20, 14 * len(lineas) + 2, '<br>'.join(h(l) for l in lineas),
                 est_texto(TC, False))
        self.max_x_extra = max(self.max_x_extra, x1)
        self.y = y0 + alto + 6


class _Alt:
    def __init__(self, sec, ev):
        self.sec = sec
        self.ev = ev

    @contextmanager
    def caso(self, guarda):
        op = _Ev('operando', guarda=guarda)
        self.ev.operandos.append(op)
        self.sec.pila.append(op)
        yield op
        self.sec.pila.pop()


def partir(texto, ancho_max, tam):
    """Parte una etiqueta larga en renglones, cortando despues de una coma o en un espacio."""
    palabras = texto.split(' ')
    lineas, actual = [], ''
    for p in palabras:
        prueba = (actual + ' ' + p) if actual else p
        if ancho(prueba, tam) <= ancho_max or not actual:
            actual = prueba
        else:
            lineas.append(actual)
            actual = '    ' + p
    if actual:
        lineas.append(actual)
    return lineas


def envolver(texto, ancho_max, tam):
    palabras = texto.split(' ')
    lineas, actual = [], ''
    for p in palabras:
        prueba = (actual + ' ' + p).strip()
        if ancho(prueba, tam) <= ancho_max or not actual:
            actual = prueba
        else:
            lineas.append(actual)
            actual = p
    if actual:
        lineas.append(actual)
    return lineas
