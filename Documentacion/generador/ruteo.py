# -*- coding: utf-8 -*-
"""Ruteo ortogonal de relaciones entre cajas.

Reglas, para que no haya flechas raras:
  * si las dos cajas se enfrentan (una arriba de la otra o una al lado de la otra), la linea
    es recta: se elige un punto comun dentro de la zona en que se superponen;
  * si no, la linea hace un unico escalon (tres tramos) por el canal libre entre filas o
    columnas, y las que comparten canal van a alturas distintas, ordenadas para no cruzarse;
  * al final se verifica que ningun tramo atraviese una caja: si pasa, se avisa.
"""

MIN_SEP = 18
MARGEN_LADO = 12


class Rel:
    def __init__(self, origen, destino, lado_o=None, lado_d=None, canal=None, **datos):
        self.origen = origen
        self.destino = destino
        self.lado_o = lado_o
        self.lado_d = lado_d
        self.canal = canal
        self.datos = datos
        self.f_o = 0.5
        self.f_d = 0.5
        self.recta = None
        self.puntos = []
        self.p0 = None
        self.p1 = None


def vertical(lado):
    return lado in ('arriba', 'abajo')


def lados_auto(cajas, r):
    if r.lado_o and r.lado_d:
        return
    xo, yo, wo, ho = cajas[r.origen]
    xd, yd, wd, hd = cajas[r.destino]
    if yo + ho <= yd:
        r.lado_o, r.lado_d = r.lado_o or 'abajo', r.lado_d or 'arriba'
    elif yd + hd <= yo:
        r.lado_o, r.lado_d = r.lado_o or 'arriba', r.lado_d or 'abajo'
    elif xo + wo <= xd:
        r.lado_o, r.lado_d = r.lado_o or 'der', r.lado_d or 'izq'
    else:
        r.lado_o, r.lado_d = r.lado_o or 'izq', r.lado_d or 'der'


def _rango_lado(caja, lado):
    x, y, w, h = caja
    if vertical(lado):
        return x + MARGEN_LADO, x + w - MARGEN_LADO
    return y + MARGEN_LADO, y + h - MARGEN_LADO


def asignar_puertos(cajas, rels):
    for r in rels:
        lados_auto(cajas, r)
        r.recta = None
        if r.datos.get('puntos'):
            continue
        a, b = cajas[r.origen], cajas[r.destino]
        opuestos = {('abajo', 'arriba'), ('arriba', 'abajo'), ('der', 'izq'), ('izq', 'der')}
        if (r.lado_o, r.lado_d) in opuestos:
            lo1, hi1 = _rango_lado(a, r.lado_o)
            lo2, hi2 = _rango_lado(b, r.lado_d)
            lo, hi = max(lo1, lo2), min(hi1, hi2)
            if hi >= lo:
                r.recta = [(lo + hi) / 2, lo, hi]
    # agrupar por lado de caja
    grupos = {}
    for r in rels:
        if r.datos.get('puntos') and r.datos.get('f_o') is None and r.datos.get('f_d') is None:
            pass
        grupos.setdefault((r.origen, r.lado_o), []).append((r, 'o'))
        grupos.setdefault((r.destino, r.lado_d), []).append((r, 'd'))
    # las rectas que comparten lado no pueden quedar pegadas: se separan dentro de su zona comun
    for (caja_id, lado), lista in grupos.items():
        rectas = sorted([r for r, e in lista if r.recta], key=lambda r: r.recta[0])
        for i in range(1, len(rectas)):
            prev, act = rectas[i - 1], rectas[i]
            if act.recta[0] - prev.recta[0] < MIN_SEP:
                nuevo = min(act.recta[2], prev.recta[0] + MIN_SEP)
                if nuevo - prev.recta[0] >= MIN_SEP - 0.5:
                    act.recta[0] = nuevo
                else:
                    act.recta = None
    for (caja_id, lado), lista in grupos.items():
        caja = cajas[caja_id]
        ini, fin = (caja[0], caja[0] + caja[2]) if vertical(lado) else (caja[1], caja[1] + caja[3])

        def clave(item):
            r, extremo = item
            if r.recta:
                return r.recta[0]
            fijo = r.datos.get('f_o' if extremo == 'o' else 'f_d')
            if fijo is not None:
                return ini + (fin - ini) * fijo
            otro = cajas[r.destino if extremo == 'o' else r.origen]
            return otro[0] + otro[2] / 2 if vertical(lado) else otro[1] + otro[3] / 2
        lista.sort(key=clave)
        n = len(lista)
        pos = [ini + (fin - ini) * (k + 1) / (n + 1) for k in range(n)]
        fijos = {}
        for k, (r, extremo) in enumerate(lista):
            if r.recta:
                fijos[k] = r.recta[0]
            else:
                f = r.datos.get('f_o' if extremo == 'o' else 'f_d')
                if f is not None:
                    fijos[k] = ini + (fin - ini) * f
        for k, v in fijos.items():
            pos[k] = v
        # los libres se alejan de los fijos manteniendo el orden
        for k in range(n):
            if k in fijos:
                continue
            for j, v in fijos.items():
                if abs(pos[k] - v) < MIN_SEP:
                    pos[k] = v - MIN_SEP if k < j else v + MIN_SEP
            pos[k] = min(max(pos[k], ini + 8), fin - 8)
        for k, (r, extremo) in enumerate(lista):
            f = (pos[k] - ini) / (fin - ini) if fin > ini else 0.5
            if extremo == 'o':
                r.f_o = f
            else:
                r.f_d = f


def _punto(caja, lado, f):
    x, y, w, h = caja
    return {'arriba': (x + w * f, y), 'abajo': (x + w * f, y + h),
            'izq': (x, y + h * f), 'der': (x + w, y + h * f)}[lado]


def hueco_libre(cajas, a, b, eje, ignorar=()):
    """Intervalo mas grande libre de cajas entre a y b (eje 'y': franja horizontal; 'x': vertical)."""
    lo, hi = min(a, b), max(a, b)
    bordes = {lo, hi}
    for nombre, (x, y, w, h) in cajas.items():
        if nombre in ignorar:
            continue
        s, e = (y, y + h) if eje == 'y' else (x, x + w)
        for v in (s, e):
            if lo < v < hi:
                bordes.add(v)
    bordes = sorted(bordes)
    mejor, mayor = (lo, hi), -1
    for i in range(len(bordes) - 1):
        s0, e0 = bordes[i], bordes[i + 1]
        libre = True
        for nombre, (x, y, w, h) in cajas.items():
            if nombre in ignorar:
                continue
            s, e = (y, y + h) if eje == 'y' else (x, x + w)
            if s < e0 - 0.5 and e > s0 + 0.5:
                libre = False
                break
        if libre and e0 - s0 > mayor:
            mayor = e0 - s0
            mejor = (s0, e0)
    return mejor


def asignar_canales(cajas, rels):
    """Para las relaciones con escalon, elige la altura (o columna) del tramo intermedio."""
    huecos = {}
    for r in rels:
        if r.canal is not None or r.datos.get('puntos'):
            continue
        p0 = _punto(cajas[r.origen], r.lado_o, r.f_o)
        p1 = _punto(cajas[r.destino], r.lado_d, r.f_d)
        if vertical(r.lado_o) and vertical(r.lado_d):
            if abs(p0[0] - p1[0]) < 0.5:
                continue
            if r.lado_o == r.lado_d:      # sale y entra por el mismo lado: canal por fuera
                continue
            clave = ('y',) + hueco_libre(cajas, p0[1], p1[1], 'y')
            huecos.setdefault(clave, []).append((r, p0[0], p1[0]))
        elif not vertical(r.lado_o) and not vertical(r.lado_d):
            if abs(p0[1] - p1[1]) < 0.5:
                continue
            if r.lado_o == r.lado_d:
                continue
            clave = ('x',) + hueco_libre(cajas, p0[0], p1[0], 'x')
            huecos.setdefault(clave, []).append((r, p0[1], p1[1]))
    for (eje, a, b), lista in huecos.items():
        hacia_mas = sorted([t for t in lista if t[2] > t[1]], key=lambda t: -t[1])
        hacia_menos = sorted([t for t in lista if t[2] <= t[1]], key=lambda t: t[1])
        orden = hacia_mas + hacia_menos
        n = len(orden)
        for k, (r, o, d) in enumerate(orden):
            r.canal = a + (b - a) * (k + 1) / (n + 1)


def rutear(cajas, r):
    a = _punto(cajas[r.origen], r.lado_o, r.f_o)
    b = _punto(cajas[r.destino], r.lado_d, r.f_d)
    r.p0, r.p1 = a, b
    if r.datos.get('puntos'):
        r.puntos = list(r.datos['puntos'])
        return r.puntos
    vo, vd = vertical(r.lado_o), vertical(r.lado_d)
    if vo and vd:
        if r.lado_o == r.lado_d:
            # sale y entra por el mismo lado: el tramo horizontal pasa por fuera de todas las cajas
            # que quedan entre los dos extremos
            sig = 1 if r.lado_o == 'abajo' else -1
            x_lo, x_hi = min(a[0], b[0]), max(a[0], b[0])
            entre = [c for c in cajas.values() if c[0] < x_hi and c[0] + c[2] > x_lo]
            if sig > 0:
                yc = max([a[1], b[1]] + [c[1] + c[3] for c in entre]) + 22
            else:
                yc = min([a[1], b[1]] + [c[1] for c in entre]) - 22
            if r.canal is not None:
                yc = r.canal
            pts = [(a[0], yc), (b[0], yc)]
        elif abs(a[0] - b[0]) < 0.5:
            pts = []
        else:
            yc = r.canal if r.canal is not None else (a[1] + b[1]) / 2
            pts = [(a[0], yc), (b[0], yc)]
    elif not vo and not vd:
        if r.lado_o == r.lado_d:
            sig = 1 if r.lado_o == 'der' else -1
            xc = r.canal if r.canal is not None else (max(a[0], b[0]) + 22 if sig > 0 else min(a[0], b[0]) - 22)
            pts = [(xc, a[1]), (xc, b[1])]
        elif abs(a[1] - b[1]) < 0.5:
            pts = []
        else:
            xc = r.canal if r.canal is not None else (a[0] + b[0]) / 2
            pts = [(xc, a[1]), (xc, b[1])]
    elif vo:
        pts = [(a[0], b[1])]
    else:
        pts = [(b[0], a[1])]
    r.puntos = pts
    return pts


def largo(p0, pts, p1):
    seq = [p0] + list(pts) + [p1]
    return sum(abs(seq[i + 1][0] - seq[i][0]) + abs(seq[i + 1][1] - seq[i][1]) for i in range(len(seq) - 1))


def restricciones(r):
    def rx(lado, f):
        return {'arriba': (f, 0), 'abajo': (f, 1), 'izq': (0, f), 'der': (1, f)}[lado]
    ex, ey = rx(r.lado_o, r.f_o)
    nx, ny = rx(r.lado_d, r.f_d)
    return ('exitX=%.4f;exitY=%.4f;exitDx=0;exitDy=0;entryX=%.4f;entryY=%.4f;entryDx=0;entryDy=0;'
            'exitPerimeter=0;entryPerimeter=0;' % (ex, ey, nx, ny))


def cruces(cajas, rels, nombre_diagrama=''):
    """Tramos que atraviesan cajas que no son sus extremos. Devuelve la lista de avisos."""
    avisos = []
    for r in rels:
        seq = [r.p0] + list(r.puntos) + [r.p1]
        for i in range(len(seq) - 1):
            (x0, y0), (x1, y1) = seq[i], seq[i + 1]
            for nombre, (x, y, w, h) in cajas.items():
                if nombre in (r.origen, r.destino):
                    continue
                if (min(x0, x1) < x + w - 1 and max(x0, x1) > x + 1 and
                        min(y0, y1) < y + h - 1 and max(y0, y1) > y + 1):
                    avisos.append('%s: %s -> %s atraviesa %s' % (nombre_diagrama, r.origen, r.destino, nombre))
            # tramo dentro de la propia caja de origen/destino (vuelve sobre si misma)
        for nombre in (r.origen, r.destino):
            x, y, w, h = cajas[nombre]
            for i in range(1, len(seq) - 2):
                (x0, y0), (x1, y1) = seq[i], seq[i + 1]
                if (min(x0, x1) < x + w - 1 and max(x0, x1) > x + 1 and
                        min(y0, y1) < y + h - 1 and max(y0, y1) > y + 1):
                    avisos.append('%s: %s -> %s vuelve sobre %s' % (nombre_diagrama, r.origen, r.destino, nombre))
    return avisos
