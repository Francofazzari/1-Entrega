# -*- coding: utf-8 -*-
"""Diagrama de clases único de cada caso de uso.

Junta en un solo diagrama todas las clases que intervienen en el caso de uso —las pantallas, las clases de negocio,
las de acceso a datos y las entidades—, sin recuadros de capas: las clases se ordenan de arriba hacia abajo (primero
las pantallas, después el negocio, los datos y por último las entidades).

Las clases, los miembros y las relaciones salen de los diagramas de cada parte del caso de uso (los mismos que se
balancean contra el código y los diagramas de secuencia); acá se agregan:
  * las asociaciones que unen las pantallas o la sesión con las entidades (un atributo de la clase cuyo tipo es una
    entidad del diagrama: deja de listarse como atributo y se dibuja como asociación con su rol);
  * las asociaciones entre entidades que en el código son un Id (Apuesta y Pago guardan el UsuarioId y el Pago guarda
    el IdApuesta): son las claves foráneas de la base de datos y se dibujan sin flecha.
"""
import re

import clases
import ruteo
from clases import DiagramaClases, medir_clase, caja_clase, est_relacion, etiqueta_extremo, etiqueta_centro
from drawio import Diagrama, marco, mezclar
from ruteo import Rel, asignar_puertos, rutear, restricciones, cruces

# modelo_completo trae el ruteo de los canales que usa el diagrama de clases del sistema (relaciones que salen y
# entran por el mismo lado, canales donde todas las líneas suben) y las notas con más margen. Al importarlo
# reemplaza esas funciones en el módulo clases: se restituyen las originales para no cambiar los demás diagramas.
_notas_original = clases.notas if clases.notas.__module__ == 'clases' else None
import modelo_completo as MC   # noqa: E402

clases.asignar_canales = ruteo.asignar_canales
if _notas_original:
    clases.notas = _notas_original
_canales = MC._asignar_canales
_notas = MC._notas

# asociaciones entre entidades que en el código son un Id (clave foránea en la base de datos):
# (origen, destino, multiplicidad en el origen, multiplicidad en el destino, nombre)
ASOC_ID = [
    ('Usuario', 'Apuesta', '1', '0..*', 'vende'),
    ('Usuario', 'Pago', '1', '0..*', 'registra'),
    ('Pago', 'Apuesta', '0..1', '1', 'paga'),
]

FUERTES = ('composicion', 'agregacion', 'asociacion', 'composicion_simple', 'asociacion_simple')


class DiagramaUnico(DiagramaClases):
    """Clases en una grilla de filas con columnas comunes, sin paquetes. separaciones: {fila: espacio extra arriba}
    para separar un poco más los grupos (pantallas, negocio, datos, entidades)."""
    GAP_X = 76
    GAP_FILA = 66
    X0, Y0 = 32, 64

    def __init__(self, titulo, filas, separaciones=None):
        super().__init__(titulo)
        self.filas = [list(f) for f in filas]
        self.separaciones = dict(separaciones or {})
        self.nota_pie = None

    def _compactar(self):
        ncol = max(len(f) for f in self.filas)
        for f in self.filas:
            f.extend([None] * (ncol - len(f)))
        usadas = [k for k in range(ncol) if any(f[k] for f in self.filas)]
        self.filas = [[f[k] for k in usadas] for f in self.filas]

    def ubicar(self):
        """Lugar de cada clase. Una celda '^' continúa la clase de la fila de arriba (una clase alta, como una
        pantalla con muchos miembros, ocupa varias filas y a su lado se apilan las clases que usa)."""
        self._compactar()
        tam = {}
        for n, v in self.vistas.items():
            w, alto = medir_clase(v['titulo'], v['la'], v['lo'], v['est'])[:2]
            tam[n] = (w, alto)
        ncol = len(self.filas[0])
        anchos = [max([tam[f[k]][0] for f in self.filas if f[k] and f[k] != '^'] or [60]) for k in range(ncol)]
        # alto de cada fila: el de las clases que no siguen abajo; las que ocupan varias filas agrandan la última
        n = len(self.filas)
        tramos = {}
        for k in range(ncol):
            i = 0
            while i < n:
                c = self.filas[i][k]
                if c and c != '^':
                    j = i
                    while j + 1 < n and self.filas[j + 1][k] == '^':
                        j += 1
                    tramos[c] = (i, j)
                    i = j + 1
                else:
                    i += 1
        altos = []
        for i, fila in enumerate(self.filas):
            hs = [tam[c][1] for c in fila if c and c != '^' and tramos[c][0] == tramos[c][1]]
            altos.append(max(hs) if hs else (30 if not any(fila) else 0))
        for c, (i, j) in tramos.items():
            if j > i:
                disponible = sum(altos[i:j + 1]) + self.GAP_FILA * (j - i) + sum(
                    self.separaciones.get(m, 0) for m in range(i + 1, j + 1))
                if disponible < tam[c][1]:
                    altos[j] += tam[c][1] - disponible
        cajas, y = {}, self.Y0
        self.y_filas = []
        for i, fila in enumerate(self.filas):
            y += self.separaciones.get(i, 0)
            self.y_filas.append((y, y + altos[i]))
            x = self.X0
            for k, c in enumerate(fila):
                if c and c != '^':
                    w, alto = tam[c]
                    cajas[c] = (x + (anchos[k] - w) / 2, y, w, alto)
                x += anchos[k] + self.GAP_X
            y += altos[i] + self.GAP_FILA
        faltan = set(self.vistas) - set(cajas)
        if faltan:
            raise ValueError('%s: clases sin lugar en la grilla: %s' % (self.titulo, ', '.join(sorted(faltan))))
        sobran = set(cajas) - set(self.vistas)
        if sobran:
            raise ValueError('%s: la grilla nombra clases que no están: %s' % (self.titulo, ', '.join(sorted(sobran))))
        return cajas

    def dibujar(self):
        d = Diagrama(self.nombre)
        self.cajas, self.ids = {}, {}
        for c, (x, y, w, alto) in self.ubicar().items():
            v = self.vistas[c]
            cid, w, alto = caja_clase(d, x, y, v['titulo'], v['la'], v['lo'], v['est'], cursiva=v['cursiva'],
                                      ident='cls_' + c)
            self.cajas[c] = (x, y, w, alto)
            self.ids[c] = cid
        for r in self.rels:
            canal = r.datos.get('canal_fijo')
            if isinstance(canal, tuple):
                i, f = canal
                arriba = max(b for (t, b) in self.y_filas[:i]) if i else 0
                abajo = self.y_filas[i][0]
                canal = arriba + (abajo - arriba) * f
            r.canal = canal
            r.puntos = []
        asignar_puertos(self.cajas, self.rels)
        _ordenar_puertos(self.cajas, self.rels)
        _canales(self.cajas, self.rels)
        for r in self.rels:
            rutear(self.cajas, r)
        puertos = {}
        for r in self.rels:
            puertos.setdefault((r.origen, r.lado_o), []).append(r.p0[0])
            puertos.setdefault((r.destino, r.lado_d), []).append(r.p1[0])
        for r in self.rels:
            est = mezclar(est_relacion(r.datos['tipo']), restricciones(r))
            aid = d.e(self.ids[r.origen], self.ids[r.destino], '', est, puntos=r.puntos)
            lado_d, lado_o = _lados_rotulos(r, puertos)
            fin = ' '.join(t for t in (r.datos.get('rol'), r.datos.get('mult_d')) if t)
            if fin:
                etiqueta_extremo(d, aid, r, fin, 'd', lado=lado_d)
            if r.datos.get('mult_o'):
                etiqueta_extremo(d, aid, r, r.datos['mult_o'], 'o', lado=lado_o)
            if r.datos.get('nombre'):
                etiqueta_centro(d, aid, r, r.datos['nombre'], r.datos['lado_nombre'])
        clases.AVISOS.extend(cruces(self.cajas, self.rels, self.nombre))
        notas = list(self.notas)
        if self.nota_pie:
            x0b, y0b, x1b, y1b = d.limites()
            notas.append((self.nota_pie, self.X0, y1b + 22, min(x1b - self.X0, 820), None))
        _notas(d, notas, self.ids)
        x0b, y0b, x1b, y1b = d.limites()
        celdas = d.celdas
        d.celdas = []
        marco(d, 0, 0, x1b + 26, y1b + 26, self.titulo)
        d.celdas = d.celdas + celdas
        return d


def _ordenar_puertos(cajas, rels):
    """En un lado de una clase donde llega una línea recta (o una con lugar fijo), las demás líneas se reparten a
    cada lado de ella según dónde está la otra clase: las que van hacia la izquierda, a su izquierda, y las que van
    hacia la derecha, a su derecha. Así no se cruzan al salir de la clase."""
    grupos = {}
    for r in rels:
        grupos.setdefault((r.origen, r.lado_o), []).append((r, 'o'))
        grupos.setdefault((r.destino, r.lado_d), []).append((r, 'd'))
    for (c, lado), lista in grupos.items():
        x, y, w, h = cajas[c]
        vert = lado in ('arriba', 'abajo')
        ini, fin = (x, x + w) if vert else (y, y + h)

        def f_de(it):
            return it[0].f_o if it[1] == 'o' else it[0].f_d

        def es_fijo(it):
            return bool(it[0].recta) or it[0].datos.get('f_o' if it[1] == 'o' else 'f_d') is not None

        fijos = sorted([it for it in lista if es_fijo(it)], key=lambda it: f_de(it))
        libres = [it for it in lista if not es_fijo(it)]
        if not fijos or not libres:
            continue

        def clave(it):
            r, e = it
            otro = cajas[r.destino if e == 'o' else r.origen]
            return otro[0] + otro[2] / 2 if vert else otro[1] + otro[3] / 2

        pos_fijos = [ini + (fin - ini) * f_de(it) for it in fijos]
        tramos = [[] for _ in range(len(fijos) + 1)]
        for it in libres:
            k = sum(1 for pf in pos_fijos if clave(it) > pf)
            tramos[k].append(it)
        bordes = [ini + 6] + pos_fijos + [fin - 6]
        for k, miembros in enumerate(tramos):
            if not miembros:
                continue
            miembros.sort(key=clave)
            a = bordes[k] + (18 if k > 0 else 0)
            b = bordes[k + 1] - (18 if k < len(fijos) else 0)
            for i, (r, e) in enumerate(miembros):
                v = a + (b - a) * (i + 1) / (len(miembros) + 1) if b > a else (a + b) / 2
                f = (v - ini) / (fin - ini)
                if e == 'o':
                    r.f_o = f
                else:
                    r.f_d = f


def _lados_rotulos(r, puertos):
    """Lado de los rótulos de los extremos (rol y multiplicidad) que llegan a una clase por arriba o por abajo.

    - Si la línea dobla cerca de la clase (tramo corto), el rótulo va del lado contrario al escalón.
    - Si no, cuando en ese lado de la clase llegan varias líneas, el rótulo de la de más a la izquierda va a su
      izquierda y el de la de más a la derecha, a su derecha (así los rótulos vecinos no se pisan).
    - Una línea del medio va del lado contrario al escalón; una recta sola, como siempre."""
    seq = [r.p0] + list(r.puntos) + [r.p1]
    res = []
    for extremo, caja, lado, p, q, otro, fijo in (
            ('d', r.destino, r.lado_d, seq[-1], seq[-2], seq[-3] if len(seq) >= 3 else None, r.datos.get('lado_rol')),
            ('o', r.origen, r.lado_o, seq[0], seq[1], seq[2] if len(seq) >= 3 else None, r.datos.get('lado_mult_o'))):
        if fijo is not None or abs(p[0] - q[0]) > 0.5:
            res.append(fijo)
            continue
        escalon = None
        if otro is not None and abs(otro[0] - q[0]) > 0.5:
            escalon = 1 if otro[0] > q[0] else -1          # de qué lado viene (o hacia dónde va) el escalón
        corto = abs(p[1] - q[1]) < 50
        xs = sorted(puertos.get((caja, lado), []))
        if escalon is not None and corto:
            res.append(-escalon)
        elif len(xs) > 1 and abs(p[0] - xs[0]) < 0.5:
            res.append(-1)
        elif len(xs) > 1 and abs(p[0] - xs[-1]) < 0.5:
            res.append(1)
        elif escalon is not None:
            res.append(-escalon)
        else:
            res.append(None)
    return res[0], res[1]


def _tipo_de(clase, atributo):
    ms = [m for m in clases.CLASES[clase].miembros if m.nombre == atributo and m.tipo in ('campo', 'propiedad')]
    if not ms:
        raise KeyError('%s no tiene el atributo %s' % (clase, atributo))
    return ms[0]


def unico(titulo, partes, filas, separaciones=None, puentes=(), usos=(), lados=None, nota=None):
    """Diagrama de clases único armado con las clases y relaciones de los diagramas de cada parte del caso de uso.

    partes: funciones que devuelven los DiagramaClases de cada parte (mismos miembros y relaciones).
    filas: grilla con el lugar de cada clase (de arriba hacia abajo).
    puentes: [(clase, atributo, multiplicidad)] atributos de una pantalla o de la sesión cuyo tipo es una entidad
             del diagrama: se dibujan como asociación y dejan de listarse en la clase.
    usos: [(clase, entidad)] dependencias «use» de una clase de acceso a datos hacia la entidad que lee o guarda (se
          dibujan solo para unir al resto una entidad que quedaría suelta).
    lados: {(origen, destino): dict(lado_o=..., lado_d=..., f_o=..., f_d=..., lado_rol=..., canal=...)} para ubicar
           una línea; canal puede ser (i, f): la altura f (0 a 1) del hueco que está arriba de la fila i.
    nota: texto de la nota al pie del diagrama.
    """
    vistas, orden, rels = {}, [], []
    for fab in partes:
        dc = fab()
        for n, v in dc.vistas.items():
            if n not in vistas:
                vistas[n] = dict(attrs=[], ops=[], est=v['est'], cursiva=v['cursiva'])
                orden.append(n)
            e = vistas[n]
            for a in v['attrs']:
                if a not in e['attrs']:
                    e['attrs'].append(a)
            for o in v['ops_raw']:
                if o not in e['ops']:
                    e['ops'].append(o)
            e['est'] = e['est'] or v['est']
        for r in dc.rels:
            rels.append(dict(tipo=r.datos['tipo'], origen=r.origen, destino=r.destino, rol=r.datos.get('rol'),
                             mult_d=r.datos.get('mult_d'), mult_o=r.datos.get('mult_o'),
                             nombre=r.datos.get('nombre')))
    # una sola relación por par de clases: la asociación (atributo) incluye el uso
    finales, vistos = [], set()
    for r in rels:
        par = (r['origen'], r['destino'])
        if r['tipo'] == 'dependencia' and any(x['tipo'] in FUERTES and (x['origen'], x['destino']) == par
                                              for x in rels):
            continue
        clave = par + (r['tipo'], r['rol'], r['nombre'])
        if clave in vistos:
            continue
        vistos.add(clave)
        finales.append(r)
    # atributos que se dibujan como asociación hacia una entidad
    for clase, atributo, mult in puentes:
        m = _tipo_de(clase, atributo)
        base = re.sub(r'^(?:List|IList|IEnumerable)<(\w+)>$', r'\1', m.tipo_dato)
        if base not in vistas:
            raise KeyError('%s.%s : %s no es una clase del diagrama' % (clase, atributo, m.tipo_dato))
        if clase not in vistas:
            raise KeyError('%s no está en el diagrama' % clase)
        rol = {'public': '+', 'private': '-', 'protected': '#', 'internal': '~'}.get(m.vis, '+') + atributo
        finales.append(dict(tipo='asociacion', origen=clase, destino=base, rol=rol, mult_d=mult, mult_o=None,
                            nombre=None))
        if atributo in vistas[clase]['attrs']:
            vistas[clase]['attrs'].remove(atributo)
    # un atributo que se dibuja como asociación (su rol) no se repite en la lista de atributos de la clase
    for r in finales:
        if r['rol'] and r['tipo'] in FUERTES:
            nombre = r['rol'].lstrip('+-#~/')
            if nombre in vistas[r['origen']]['attrs']:
                vistas[r['origen']]['attrs'].remove(nombre)
    for clase, entidad in usos:
        if clase not in vistas or entidad not in vistas:
            raise KeyError('%s o %s no está en el diagrama' % (clase, entidad))
        if not re.search(r'\b%s\b' % entidad, MC._fuente(clase)):
            raise KeyError('%s no usa %s' % (clase, entidad))
        finales.append(dict(tipo='dependencia', origen=clase, destino=entidad, rol=None, mult_d=None, mult_o=None,
                            nombre='«use»'))
    # asociaciones entre entidades que en el código son un Id (clave foránea)
    for o, d, mo, md, nombre in ASOC_ID:
        if o in vistas and d in vistas:
            finales.append(dict(tipo='asociacion_simple', origen=o, destino=d, rol=None, mult_d=md, mult_o=mo,
                                nombre=nombre))

    dg = DiagramaUnico(titulo, filas, separaciones)
    for n in orden:
        v = vistas[n]
        dg.clase(n, attrs=v['attrs'], ops=v['ops'], estereotipo=v['est'], cursiva=v['cursiva'])
    lados = lados or {}
    for r in finales:
        kw = dict(lados.get((r['origen'], r['destino']), {}))
        canal = kw.pop('canal', None)
        rel = dg.rel(r['tipo'], r['origen'], r['destino'], rol=r['rol'], mult_d=r['mult_d'], mult_o=r['mult_o'],
                     nombre=r['nombre'], **kw)
        rel.datos['canal_fijo'] = canal
    dg.nota_pie = nota
    return dg


# =========================================================================== ubicación de cada caso de uso
# Filas de arriba hacia abajo: pantallas, negocio (BLL), datos (DAL) y entidades (BE). '^' = la clase de arriba
# sigue en esta fila. Las líneas largas pasan por columnas libres.
FP, FSL, FA, FT, TR = 'FormPrincipal', 'FormSeleccionLoteria', 'FormApuesta', 'FormTicket', 'TicketRenderer'
FJ, FOT, FF, FR, FRE = 'FormJuego', 'FormOperacionTicket', 'FormFelicitaciones', 'FormReporte', 'FormResultado'
SM = 'SesionManager'
ARRIBA = dict(lado_o='arriba', lado_d='arriba')

LAYOUTS = {
    'CU-N01': dict(
        filas=[[FP, FSL, FA, FT, TR, None],
               ['TurnoBLL', 'LoteriaBLL', SM, 'ApuestaBLL', 'SorteoBLL', 'BitacoraBLL'],
               ['TurnoDAL', 'LoteriaDAL', None, 'ApuestaDAL', 'SorteoDAL', 'BitacoraDAL'],
               ['NumeroElegido', 'Jugada', 'Apuesta', 'Usuario', None, 'Bitacora'],
               ['Loteria', 'Sorteo', 'Juego', None, None, None],
               [None, 'Turno', None, None, None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')], usos=[('BitacoraDAL', 'Bitacora')],
        lados={('ApuestaBLL', 'BitacoraBLL'): dict(ARRIBA, f_o=0.9), (SM, 'Usuario'): dict(canal=(3, 0.5)),
               (FSL, 'LoteriaBLL'): dict(lado_rol=-1)}),
    'CU-N02': dict(
        filas=[[FP, FJ, None, FT, TR, None],
               [SM, 'JuegoBLL', 'Azar', 'ApuestaBLL', 'SorteoBLL', 'BitacoraBLL'],
               [None, None, None, 'ApuestaDAL', 'SorteoDAL', 'BitacoraDAL'],
               ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', None, 'Bitacora'],
               [None, 'Juego', 'Sorteo', 'Loteria', None, None],
               [None, None, 'Turno', None, None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')], usos=[('BitacoraDAL', 'Bitacora')],
        lados={('ApuestaBLL', 'BitacoraBLL'): dict(ARRIBA, f_o=0.9)}),
    'CU-N03': dict(
        filas=[['PrintPreviewDialog', FT, TR, None],
               ['PrintDocument', None, 'ApuestaBLL', None],
               [None, 'Apuesta', 'Jugada', 'NumeroElegido'],
               [None, 'Juego', 'Sorteo', 'Loteria'],
               [None, None, 'Turno', None]],
        puentes=[(FT, 'apuesta', '1')]),
    'CU-N04': dict(
        filas=[[FP, FOT, None],
               [None, 'ApuestaBLL', None],
               [None, 'ApuestaDAL', None],
               ['Apuesta', 'Jugada', 'NumeroElegido'],
               ['Juego', 'Sorteo', 'Loteria'],
               [None, 'Turno', None]],
        puentes=[(FOT, 'apuesta', '0..1')],
        lados={(FOT, 'Apuesta'): dict(canal=(1, 0.5))}),
    'CU-N05': dict(
        filas=[[FP, None, FOT, FT, TR],
               [SM, None, 'ApuestaBLL', 'BitacoraBLL', None],
               [None, None, 'ApuestaDAL', 'BitacoraDAL', None],
               ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Bitacora'],
               [None, 'Juego', 'Sorteo', 'Loteria', None],
               [None, None, 'Turno', None, None]],
        puentes=[(FOT, 'apuesta', '0..1'), (SM, 'UsuarioActual', '0..1')], usos=[('BitacoraDAL', 'Bitacora')],
        lados={(FOT, 'Apuesta'): dict(canal=(1, 0.72)), (FT, 'ApuestaBLL'): dict(f_d=0.86)}),
    'CU-N06': dict(
        filas=[[FP, None, FOT, None, None],
               [SM, None, 'ApuestaBLL', 'BitacoraBLL', 'SorteoBLL'],
               [None, None, 'ApuestaDAL', 'BitacoraDAL', None],
               ['Usuario', 'Apuesta', 'Jugada', 'NumeroElegido', 'Bitacora'],
               [None, 'Juego', 'Sorteo', None, None],
               [None, None, 'Loteria', 'Turno', None]],
        puentes=[(FOT, 'apuesta', '0..1'), (SM, 'UsuarioActual', '0..1')], usos=[('BitacoraDAL', 'Bitacora')],
        lados={(FOT, 'Apuesta'): dict(canal=(1, 0.72)), ('ApuestaBLL', 'SorteoBLL'): dict(ARRIBA, f_o=0.9)}),
    'CU-N07': dict(
        filas=[[FP, FR, 'LoteriaBLL', 'LoteriaDAL'],
               [SM, '^', 'TurnoBLL', 'TurnoDAL'],
               ['Usuario', '^', 'JuegoBLL', 'JuegoDAL'],
               ['ApuestaBLL', 'PagoBLL', 'SorteoBLL', None],
               ['ApuestaDAL', 'PagoDAL', 'SorteoDAL', None],
               ['Juego', 'Sorteo', 'NumeroGanador', None],
               ['Turno', 'Loteria', None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')], usos=[('SorteoDAL', 'Sorteo'), ('SorteoDAL', 'NumeroGanador')]),
    'CU-N08': dict(
        filas=[[FP, FR, 'LoteriaBLL', 'LoteriaDAL'],
               [SM, '^', 'TurnoBLL', 'TurnoDAL'],
               ['Usuario', '^', 'JuegoBLL', 'JuegoDAL'],
               ['ApuestaBLL', 'PagoBLL', 'SorteoBLL', 'BitacoraBLL'],
               ['ApuestaDAL', 'PagoDAL', 'SorteoDAL', 'BitacoraDAL'],
               ['Juego', 'Sorteo', None, 'Bitacora'],
               ['Turno', 'Loteria', None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')], usos=[('SorteoDAL', 'Sorteo'), ('BitacoraDAL', 'Bitacora')]),
    'CU-N09': dict(
        filas=[[None, FP, FR, 'LoteriaBLL', 'LoteriaDAL'],
               ['Usuario', SM, '^', 'TurnoBLL', 'TurnoDAL'],
               [None, FRE, '^', 'PagoBLL', 'PagoDAL'],
               [None, '^', None, None, None],
               ['JuegoDAL', 'JuegoBLL', 'SorteoBLL', 'ApuestaBLL', 'ApuestaDAL'],
               [None, 'Azar', 'SorteoDAL', 'BitacoraBLL', 'BitacoraDAL'],
               ['Juego', 'Sorteo', 'NumeroGanador', None, 'Bitacora'],
               ['Turno', 'Loteria', None, None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')],
        usos=[('SorteoDAL', 'Sorteo'), ('SorteoDAL', 'NumeroGanador'), ('BitacoraDAL', 'Bitacora')]),
    'CU-N10': dict(
        filas=[[FP, FR, 'LoteriaBLL', 'LoteriaDAL', None],
               [SM, '^', 'TurnoBLL', 'TurnoDAL', None],
               ['Usuario', '^', 'JuegoBLL', 'JuegoDAL', None],
               ['ApuestaBLL', 'PagoBLL', 'SorteoBLL', 'BitacoraBLL', 'Azar'],
               ['ApuestaDAL', 'PagoDAL', 'SorteoDAL', 'BitacoraDAL', None],
               ['Juego', 'Sorteo', 'NumeroGanador', 'Bitacora', None],
               ['Turno', 'Loteria', None, None, None]],
        puentes=[(SM, 'UsuarioActual', '0..1')],
        usos=[('SorteoDAL', 'Sorteo'), ('SorteoDAL', 'NumeroGanador'), ('BitacoraDAL', 'Bitacora')],
        lados={('SorteoBLL', 'Azar'): dict(ARRIBA, f_o=0.9)}),
    'CU-N11': dict(
        filas=[[None, FF, FOT, FP, None],
               ['ApuestaDAL', 'ApuestaBLL', 'PagoBLL', 'PremioBLL', SM],
               [None, 'BitacoraBLL', 'PagoDAL', 'SorteoBLL', None],
               [None, 'BitacoraDAL', None, 'SorteoDAL', None],
               [None, 'Bitacora', None, 'NumeroGanador', 'Usuario'],
               [None, 'NumeroElegido', 'Jugada', 'Apuesta', 'Pago'],
               [None, 'Premio', 'Sorteo', 'Juego', None]],
        puentes=[(SM, 'UsuarioActual', '0..1')],
        usos=[('SorteoDAL', 'NumeroGanador'), ('BitacoraDAL', 'Bitacora')],
        lados={(FOT, 'ApuestaBLL'): dict(lado_rol=1, canal=(1, 0.3)), (FOT, 'PagoBLL'): dict(lado_rol=-1)}),
    'CU-N12': dict(
        filas=[[FP, FR, 'LoteriaBLL', 'LoteriaDAL', None],
               [SM, '^', 'TurnoBLL', 'TurnoDAL', None],
               [TR, FT, 'ApuestaBLL', 'JuegoBLL', 'PagoBLL'],
               [None, None, 'ApuestaDAL', 'JuegoDAL', 'PagoDAL'],
               [None, 'Apuesta', 'Jugada', None, None],
               [None, 'Juego', 'Sorteo', None, None]],
        puentes=[(FT, 'apuesta', '1')], lados={(FR, FT): dict(lado_nombre=-2)}),
    'CU-N13': dict(
        filas=[[FP, FR, 'LoteriaBLL', 'LoteriaDAL'],
               [SM, '^', 'TurnoBLL', 'TurnoDAL'],
               [None, '^', 'JuegoBLL', 'JuegoDAL'],
               [TR, FT, 'ApuestaBLL', 'PagoBLL'],
               [None, None, 'ApuestaDAL', 'PagoDAL'],
               [None, None, None, 'Pago']],
        usos=[('PagoDAL', 'Pago')], lados={(FR, FT): dict(lado_nombre=-2)}),
}


def diagrama(codigo, partes):
    """Diagrama de clases único del caso de uso, con la ubicación de LAYOUTS."""
    from comun import CASOS
    cfg = LAYOUTS[codigo]
    return unico('class %s %s' % (codigo, CASOS[codigo][0]), partes, cfg['filas'], cfg.get('separaciones'),
                 cfg.get('puentes', ()), cfg.get('usos', ()), cfg.get('lados'), cfg.get('nota'))


TITULO = 'todas las clases que intervienen en el caso de uso'


def unir(codigo, partes):
    """Lista 'clases' de la especificación: un único diagrama hecho con las partes [(título, función)]."""
    fabricas = [f for _, f in partes]
    return [(TITULO, lambda: diagrama(codigo, fabricas))]
