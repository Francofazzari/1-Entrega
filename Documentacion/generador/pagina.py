# -*- coding: utf-8 -*-
"""Medidas de pagina del documento y tamanio de letra con que queda cada diagrama al insertarlo."""

# area util para una figura (cm), descontando margenes, encabezado, pie y epigrafe
VERTICAL = (16.5, 22.5)     # A4 vertical, margenes 2,5 / 2 cm
APAISADA = (26.5, 16.0)     # A4 apaisada, margenes 1,5 cm


def escala(w_px, h_px, area):
    """mm por px de diagrama si la figura ocupa lo mas posible del area (sin agrandar de mas)."""
    return min(area[0] * 10 / w_px, area[1] * 10 / h_px, 0.30)


def puntos(w_px, h_px, area, tam_px=11):
    return tam_px * escala(w_px, h_px, area) / 0.3528


def orientacion(w_px, h_px):
    """Devuelve ('vertical' | 'apaisada', puntos) eligiendo la que deja la letra mas grande."""
    pv = puntos(w_px, h_px, VERTICAL)
    pa = puntos(w_px, h_px, APAISADA)
    # se prefiere vertical si la diferencia es chica (menos cambios de orientacion)
    if pv >= pa - 0.3:
        return 'vertical', pv
    return 'apaisada', pa
