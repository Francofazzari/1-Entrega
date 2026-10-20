# -*- coding: utf-8 -*-
"""Exporta diagramas draw.io a PNG con el motor real de draw.io (viewer de diagrams.net) en
Chrome sin ventana. La imagen es exactamente lo que se ve al abrir el .drawio."""
import json
import os
import subprocess
import threading
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

from PIL import Image

CHROME = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
BASE = Path(__file__).resolve().parent.parent
TMP = BASE / 'tmp_render'
ESCALA = 2          # pixeles por punto de diagrama (nitidez para imprimir)
MARGEN = 14         # margen blanco alrededor del diagrama (px de diagrama)

PLANTILLA = r"""<!DOCTYPE html>
<html><head><meta charset="utf-8">
<style>html,body{margin:0;padding:0;background:#ffffff;overflow:hidden;} #g{position:absolute;left:0;top:0;width:%(W)dpx;height:%(H)dpx;overflow:hidden;}</style>
</head><body><div id="g"></div>
<script src="https://viewer.diagrams.net/js/viewer-static.min.js"></script>
<script>
try {
  var xml = %(XML)s;
  var container = document.getElementById('g');
  var graph = new Graph(container);
  graph.foldingEnabled = false;
  graph.setEnabled(false);
  graph.setTooltips(false);
  var doc = mxUtils.parseXml(xml);
  var codec = new mxCodec(doc);
  codec.decode(doc.documentElement, graph.getModel());
  var b = graph.getGraphBounds();
  var t = graph.view.translate;
  graph.view.setTranslate(t.x - b.x + %(M)d, t.y - b.y + %(M)d);
  document.title = 'OK ' + JSON.stringify([b.x, b.y, b.width, b.height]);
} catch (e) { document.title = 'ERROR ' + e; }
</script></body></html>"""

_hilos = threading.local()
_contador = [0]
_candado = threading.Lock()


def _perfil():
    if not hasattr(_hilos, 'perfil'):
        with _candado:
            _contador[0] += 1
            n = _contador[0]
        _hilos.perfil = TMP / ('perfil_%d' % n)
        _hilos.perfil.mkdir(parents=True, exist_ok=True)
    return _hilos.perfil


def recortar(png):
    img = Image.open(png).convert('RGB')
    gris = img.convert('L').point(lambda p: 255 if p < 248 else 0)
    caja = gris.getbbox()
    if not caja:
        raise RuntimeError('imagen vacia: ' + str(png))
    m = MARGEN * ESCALA
    x0, y0, x1, y1 = caja
    x0, y0 = max(0, x0 - m), max(0, y0 - m)
    x1, y1 = min(img.width, x1 + m), min(img.height, y1 + m)
    recorte = img.crop((x0, y0, x1, y1))
    recorte.save(png, optimize=True)
    return recorte.size


def exportar(diagrama, destino):
    """Exporta un Diagrama (drawio.Diagrama) a PNG. Devuelve (ancho, alto) en px de imagen."""
    destino = Path(destino).resolve()
    destino.parent.mkdir(parents=True, exist_ok=True)
    TMP.mkdir(parents=True, exist_ok=True)
    x0, y0, x1, y1 = diagrama.limites()
    W = int(x1 - min(0, x0) + 2 * MARGEN + 220)
    H = int(y1 - min(0, y0) + 2 * MARGEN + 220)
    pagina = TMP / (destino.stem + '.html')
    pagina.write_text(PLANTILLA % {'XML': json.dumps(diagrama.modelo()), 'W': W, 'H': H, 'M': MARGEN},
                      encoding='utf-8')
    cmd = [CHROME, '--headless=new', '--disable-gpu', '--hide-scrollbars', '--no-first-run',
           '--disable-extensions', '--force-device-scale-factor=%d' % ESCALA,
           '--window-size=%d,%d' % (W, H), '--virtual-time-budget=25000',
           '--user-data-dir=' + str(_perfil()), '--screenshot=' + str(destino),
           pagina.resolve().as_uri()]
    for intento in range(3):
        if destino.exists():
            destino.unlink()
        subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=180)
        if destino.exists():
            try:
                return recortar(destino)
            except RuntimeError:
                pass
    raise RuntimeError('No se pudo exportar ' + diagrama.nombre)


def exportar_varios(trabajos, hilos=4):
    """trabajos: lista de (diagrama, destino). Devuelve dict destino -> (ancho, alto)."""
    res = {}
    with ThreadPoolExecutor(max_workers=hilos) as ex:
        futuros = {ex.submit(exportar, d, p): p for d, p in trabajos}
        for f, p in futuros.items():
            res[str(p)] = f.result()
    return res
