# -*- coding: utf-8 -*-
"""Catalogo de figuras del documento N01-N02.

Cada figura tiene su grupo (N01, PN01..PN04, N02, CU-N01..CU-N13), su tipo, el epigrafe con el que se titula en
el Word y el diagrama draw.io que la genera. Desde aca se renderizan todos los PNG y se exportan los .drawio del
repositorio (un archivo por grupo, una pagina por diagrama con el mismo titulo que su marco).

Uso: python catalogo.py [--sin-png] [--sin-drawio]
"""
import re
import sys
import time
from pathlib import Path

from comun import CASOS, NOMBRE_PROCESO, PROCESOS
from drawio import archivo_drawio

BASE = Path(__file__).resolve().parent.parent
PNG = BASE / 'png_final'
REPO = Path(r'C:\Users\franco\source\repos\Entrega1software')
DIAGRAMAS = REPO / 'Documentacion' / 'Diagramas'

# letra (px de diagrama) del texto principal de cada tipo: sirve para calcular con que tamanio queda en la hoja
LETRA = dict(uc=15, uc_negocio=15, uc_sistema=15, actores=15, colab=12, actividad=12, conceptual=12, flujo=12,
             clases=11, sd=11, der=11, proto=None)


class Figura:
    def __init__(self, grupo, tipo, archivo, epigrafe, fabrica=None, ruta=None, k=None, n=None, marco=None):
        self.grupo = grupo          # 'N01', 'PN01', 'N02', 'CU-N01', ...
        self.tipo = tipo            # uc_negocio, conceptual, actores, colab, actividad, uc_sistema, uc, flujo, ...
        self.archivo = archivo      # nombre del PNG
        self.epigrafe = epigrafe    # texto del titulo de la figura (sin "Figura N.")
        self.fabrica = fabrica      # funcion que devuelve el drawio.Diagrama (None para los prototipos)
        self.ruta = ruta or (PNG / archivo)
        self.k, self.n = k, n       # numero de la figura dentro de su serie (diagrama k de n)
        self.marco = marco          # numero que tiene que figurar en el titulo del marco: "(k)"
        self.diagrama = None

    @property
    def letra(self):
        return LETRA.get(self.tipo)


def _de(k, n):
    return ' %d de %d' % (k, n) if n > 1 else ''


def _cu_titulo(codigo):
    return '%s %s' % (codigo, CASOS[codigo][0])


def figuras_n01():
    import n01
    import n01_diag as nd
    res = [
        Figura('N01', 'uc_negocio', 'N01_negocio.png', 'Modelo de casos de uso del negocio de la Agencia de Quiniela',
               n01.d_negocio_general),
        Figura('N01', 'conceptual', 'N01_conceptual.png', 'Modelo conceptual del dominio de la Agencia de Quiniela',
               nd.d_conceptual_general),
    ]
    for pn, nombre in PROCESOS:
        t = '%s %s' % (pn, nombre)
        res.append(Figura(pn, 'actores', 'N01_%s_actores.png' % pn,
                          '%s – Diagrama de actores (roles intervinientes)' % t,
                          lambda pn=pn: nd.d_actores(pn)))
        res.append(Figura(pn, 'colab', 'N01_%s_colaboracion.png' % pn,
                          '%s – Diagrama de secuencia de la colaboración entre los roles' % t,
                          lambda pn=pn: nd.d_colaboracion(pn)))
        acts = nd.ACTIVIDADES[pn]
        for k, (titulo, f) in enumerate(acts, start=1):
            res.append(Figura(pn, 'actividad', 'N01_%s_actividad%d.png' % (pn, k),
                              '%s – Diagrama de actividad%s: %s' % (t, _de(k, len(acts)), titulo),
                              lambda f=f: f().dibujar(), k=k, n=len(acts)))
        res.append(Figura(pn, 'conceptual', 'N01_%s_conceptual.png' % pn,
                          '%s – Modelo conceptual del proceso' % t, lambda pn=pn: nd.d_conceptual(pn)))
    return res


def figuras_n02(con_prototipos=True):
    import n02_comun
    import prototipos
    from balanceo import especificaciones
    res = [Figura('N02', 'uc_sistema', 'N02_casos_de_uso_del_sistema.png',
                  'Modelo de casos de uso del sistema (CU-N01 a CU-N13)', n02_comun.d_cu_sistema)]
    for spec in especificaciones():
        c = spec['codigo']
        t = _cu_titulo(c)
        res.append(Figura(c, 'uc', '%s_casos_de_uso.png' % c, '%s – Diagrama de casos de uso' % t,
                          spec['diagrama_cu']))
        if spec.get('flujo_alg'):
            texto, fab = spec['flujo_alg']
            titulo = texto.split('. ')[0].rstrip('.')
            res.append(Figura(c, 'flujo', '%s_flujo.png' % c, '%s – Diagrama de flujo: %s' % (c, titulo),
                              lambda fab=fab: fab().dibujar()))
        n = len(spec['clases'])
        for k, (titulo, fab) in enumerate(spec['clases'], start=1):
            res.append(Figura(c, 'clases', '%s_clases%d.png' % (c, k),
                              '%s – Diagrama de clases%s: %s' % (c, _de(k, n), titulo),
                              lambda fab=fab: fab().dibujar(), k=k, n=n, marco=k if n > 1 else None))
        n = len(spec['secuencias'])
        for k, (titulo, fab) in enumerate(spec['secuencias'], start=1):
            res.append(Figura(c, 'sd', '%s_secuencia%02d.png' % (c, k),
                              '%s – Diagrama de secuencia%s: %s' % (c, _de(k, n), titulo),
                              lambda fab=fab: fab().dibujar(), k=k, n=n, marco=k if n > 1 else None))
        res.append(Figura(c, 'der', '%s_der.png' % c, '%s – DER: tablas que usa el caso de uso' % c,
                          lambda spec=spec: spec['der']().dibujar()))
        if con_prototipos:
            protos = prototipos.generar(c)
            for k, (titulo, ruta, filas, nota) in enumerate(protos, start=1):
                sub = re.sub(r'^Prototipo CU-N\d+ \(\d+\):\s*', '', titulo)
                f = Figura(c, 'proto', Path(ruta).name,
                           '%s – Prototipo de interfaz%s: %s' % (c, _de(k, len(protos)), sub), ruta=Path(ruta),
                           k=k, n=len(protos))
                f.filas, f.nota = filas, nota
                res.append(f)
    return res


def todas(con_prototipos=True):
    return figuras_n01() + figuras_n02(con_prototipos)


def construir(figs):
    """Arma los diagramas (sin renderizar) y controla que el titulo del marco lleve el numero de la figura."""
    avisos = []
    for i, f in enumerate(figs):
        if f.fabrica is None:
            continue
        d = f.fabrica()
        d.ident = 'p%03d' % i
        f.diagrama = d
        if f.marco is not None and '(%d)' % f.marco not in d.nombre:
            avisos.append('%s: el marco "%s" no lleva el numero (%d)' % (f.archivo, d.nombre, f.marco))
    return avisos


def renderizar(figs, hilos=6):
    from render import exportar_varios
    PNG.mkdir(parents=True, exist_ok=True)
    trabajos = [(f.diagrama, f.ruta) for f in figs if f.diagrama is not None]
    return exportar_varios(trabajos, hilos=hilos)


def nombre_archivo(grupo):
    if grupo in ('N01',) or grupo in NOMBRE_PROCESO:
        return 'N01 Procesos de negocio.drawio'
    if grupo == 'N02':
        return 'N02 Modelo de casos de uso del sistema.drawio'
    return '%s.drawio' % _cu_titulo(grupo)


def exportar_drawio(figs):
    DIAGRAMAS.mkdir(parents=True, exist_ok=True)
    archivos = {}
    for f in figs:
        if f.diagrama is not None:
            archivos.setdefault(nombre_archivo(f.grupo), []).append(f.diagrama)
    for nombre, diagramas in archivos.items():
        (DIAGRAMAS / nombre).write_text(archivo_drawio(diagramas), encoding='utf-8')
    return archivos


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    t0 = time.time()
    figs = todas(con_prototipos=False)
    avisos = construir(figs)
    print('\n'.join(avisos) or 'marcos numerados OK')
    import clases
    print('\n'.join(clases.AVISOS) or 'clases sin cruces')
    print(len(figs), 'diagramas armados en %.0f s' % (time.time() - t0))
    if '--sin-drawio' not in sys.argv:
        for nombre, ds in exportar_drawio(figs).items():
            print('drawio:', nombre, len(ds), 'paginas')
    if '--sin-png' not in sys.argv:
        res = renderizar(figs)
        print(len(res), 'PNG en %.0f s' % (time.time() - t0))
