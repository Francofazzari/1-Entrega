# -*- coding: utf-8 -*-
"""Diagrama de clases único de un caso de uso como archivo propio (además de la página que tiene en el .drawio del
caso de uso). Es el mismo diagrama del documento: lo arma clases_unicos.py con las clases, los miembros y las
relaciones balanceados contra el código.

Uso:  python clases_cu.py [carpeta_png] [CU-N06 ...]
Escribe "<código> <nombre> - Diagrama de clases.drawio" en Documentacion/Diagramas (y su PNG si se indica una
carpeta). Sin códigos, arma el de CU-N06 Cancelar ticket.
"""
import sys

if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    import clases
    from balanceo import especificaciones
    from comun import CASOS
    from modelo_completo import exportar

    args = sys.argv[1:]
    codigos = [a for a in args if a.startswith('CU-N')] or ['CU-N06']
    png = next((a for a in args if not a.startswith('CU-N')), None)
    specs = {s['codigo']: s for s in especificaciones()}
    for c in codigos:
        exportar('%s %s - Diagrama de clases.drawio' % (c, CASOS[c][0]), specs[c]['clases'][0][1](), png)
    print('\n'.join(clases.AVISOS) or 'sin cruces')
