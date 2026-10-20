# -*- coding: utf-8 -*-
"""Vistas del PDF para revisarlo: hojas de contacto de varias paginas o una pagina a buena resolucion.

Uso: python ver_pdf.py archivo.pdf salida.png desde hasta [columnas] [escala]
"""
import sys

import pypdfium2 as pdfium
from PIL import Image, ImageDraw


def hoja(pdf, salida, desde, hasta, cols=4, escala=0.45):
    doc = pdfium.PdfDocument(pdf)
    hasta = min(hasta, len(doc))
    ims = []
    for i in range(desde - 1, hasta):
        im = doc[i].render(scale=escala).to_pil().convert('RGB')
        d = ImageDraw.Draw(im)
        d.rectangle((0, 0, im.width - 1, im.height - 1), outline='gray')
        d.text((6, 4), str(i + 1), fill='red')
        ims.append(im)
    if not ims:
        return None
    w, h = ims[0].size
    filas = (len(ims) + cols - 1) // cols
    c = Image.new('RGB', (cols * (w + 6), filas * (h + 6)), 'white')
    for k, im in enumerate(ims):
        c.paste(im, ((k % cols) * (w + 6), (k // cols) * (h + 6)))
    c.save(salida)
    return len(doc), c.size


if __name__ == '__main__':
    a = sys.argv
    cols = int(a[5]) if len(a) > 5 else 4
    esc = float(a[6]) if len(a) > 6 else 0.45
    print(hoja(a[1], a[2], int(a[3]), int(a[4]), cols, esc))
