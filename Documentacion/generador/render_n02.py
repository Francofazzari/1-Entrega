# -*- coding: utf-8 -*-
"""Renderiza los diagramas de las especificaciones de casos de uso (todas o las indicadas) y muestra el tamaño
de letra con que quedan en la página.

Uso: python render_n02.py [CU-N01 CU-N02 ...] [--solo sd|cl|uc|der|flujo]
"""
import sys
import time
from render import exportar_varios
import clases
import pagina
from balanceo import especificaciones


def trabajos_de(spec, solo=None):
    n = spec['codigo'].split('-')[1].lower()
    res = []
    if solo in (None, 'uc'):
        res.append((spec['diagrama_cu'](), r'..\png\%s_uc.png' % n))
    if spec.get('flujo_alg') and solo in (None, 'flujo'):
        f = spec['flujo_alg'][1]()
        d = f.dibujar()
        if f.avisos:
            print('AVISOS flujo', spec['codigo'], f.avisos)
        res.append((d, r'..\png\%s_flujo.png' % n))
    if solo in (None, 'cl'):
        for k, (t, fn) in enumerate(spec['clases']):
            res.append((fn().dibujar(), r'..\png\%s_cl%d.png' % (n, k + 1)))
    if solo in (None, 'sd'):
        for k, (t, fn) in enumerate(spec['secuencias']):
            res.append((fn().dibujar(), r'..\png\%s_sd%d.png' % (n, k + 1)))
    if solo in (None, 'der'):
        res.append((spec['der']().dibujar(), r'..\png\%s_der.png' % n))
    return res


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    args = sys.argv[1:]
    solo = None
    if '--solo' in args:
        i = args.index('--solo')
        solo = args[i + 1]
        args = args[:i] + args[i + 2:]
    t0 = time.time()
    trabajos = []
    for spec in especificaciones():
        if args and spec['codigo'] not in args:
            continue
        trabajos += trabajos_de(spec, solo)
    print('\n'.join(clases.AVISOS) or 'sin cruces')
    res = exportar_varios(trabajos, hilos=6)
    for k in sorted(res):
        v = res[k]
        o, pt = pagina.orientacion(v[0] / 2, v[1] / 2)
        print('%-14s %5.0f x %5.0f  %-9s %4.1f pt %s' % (k.split('\\')[-1], v[0] / 2, v[1] / 2, o, pt,
                                                       '  <-- CHICO' if pt < 6 else ''))
    print('%.0f s' % (time.time() - t0))
