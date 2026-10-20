# -*- coding: utf-8 -*-
"""Lista los textos que el sistema le muestra al usuario (para citarlos tal cual en el documento)."""
import re
import sys
from pathlib import Path

REPO = Path(r"C:\Users\franco\source\repos\Entrega1software")
ARCHIVOS = ['BLL/ApuestaBLL.cs', 'BLL/SorteoBLL.cs', 'BLL/PagoBLL.cs', 'BLL/PremioBLL.cs', 'BLL/JuegoBLL.cs',
            'Entrega1software/FormPrincipal.cs', 'Entrega1software/FormSeleccionLoteria.cs',
            'Entrega1software/FormApuesta.cs', 'Entrega1software/FormJuego.cs', 'Entrega1software/FormTicket.cs',
            'Entrega1software/FormOperacionTicket.cs', 'Entrega1software/FormResultado.cs',
            'Entrega1software/FormReporte.cs', 'Entrega1software/FormFelicitaciones.cs']
RE_LIT = re.compile(r'"((?:[^"\\]|\\.)*)"')


def literales(archivo):
    s = (REPO / archivo).read_text(encoding='utf-8-sig')
    res = []
    for linea in s.split('\n'):
        if linea.strip().startswith('//'):
            continue
        for l in RE_LIT.findall(linea):
            if len(l) > 3 and re.search(r'[a-záéíóúñ]{3}', l, re.I):
                res.append(l)
    return res


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    for f in ARCHIVOS:
        print('=====', f)
        vistos = []
        for l in literales(f):
            if l not in vistos:
                vistos.append(l)
                print('   ', l)
