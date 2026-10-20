# -*- coding: utf-8 -*-
"""Hojas de contacto para revisar muchos diagramas de un vistazo.

Uso: python montaje.py salida.png columnas ancho archivo1.png archivo2.png ...
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw

P = Path(__file__).resolve().parent.parent / 'png_final'


def montaje(archivos, salida, cols=3, ancho=660):
    ims = []
    for a in archivos:
        ruta = Path(a) if Path(a).is_absolute() else P / a
        im = Image.open(ruta).convert('RGB')
        r = ancho / im.width
        im = im.resize((ancho, max(1, int(im.height * r))), Image.LANCZOS)
        ims.append((Path(a).name, im))
    filas = [ims[i:i + cols] for i in range(0, len(ims), cols)]
    alto = sum(max(im.height for _, im in f) + 24 for f in filas)
    c = Image.new('RGB', (cols * (ancho + 10), alto), 'white')
    d = ImageDraw.Draw(c)
    y = 0
    for f in filas:
        x = 0
        for nombre, im in f:
            d.text((x + 4, y + 4), nombre, fill='red')
            c.paste(im, (x, y + 20))
            x += ancho + 10
        y += max(im.height for _, im in f) + 24
    c.save(salida)
    return c.size


if __name__ == '__main__':
    salida, cols, ancho = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
    print(montaje(sys.argv[4:], salida, cols, ancho))
