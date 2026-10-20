# Generador de la documentación N01-N02

Programa que arma los diagramas y el Word de **N01 Especificación funcional por proceso de negocio** y
**N02 Especificaciones de casos de uso** del Sistema Agencia de Quiniela. Lee el código de la solución
(`Entrega1software`, `BLL`, `DAL`, `BE`) y `ScriptBD_AgenciaQuiniela.sql`, así que los diagramas quedan
balanceados con la implementación.

## Pasos

1. `python balanceo.py` – verifica cada caso de uso contra el código y la base de datos (tiene que dar `TOTAL 0`).
2. `python catalogo.py` – arma los 151 diagramas, los exporta a PNG (`../png_final`) y escribe los `.drawio`
   en `Documentacion/Diagramas` (un archivo por grupo, una página por diagrama).
3. `python armar_word.py` – arma `Documentacion/N01_N02_Agencia_de_Quiniela.docx` con todas las figuras,
   las plantillas y las tablas.
4. `powershell -File actualizar_word.ps1 -docx <docx> -pdf <pdf>` – abre el documento en una instancia
   oculta de Word, actualiza el índice, el índice de figuras y las referencias, y exporta el PDF.

## Estructura que esperan los scripts

```
<carpeta de trabajo>/
  ref_entrega.docx        copia de la entrega anterior (encabezado y pie oficiales de la UAI)
  doc/gen/                estos scripts
  doc/recursos/           portada_logo.png
  doc/capturas/           capturas del sistema (las genera harness/CapturasDoc.cs)
  doc/png_final/          PNG de los diagramas (los genera catalogo.py)
```

## Módulos

| Módulo | Contenido |
|---|---|
| `codigo.py` | Lee las clases y miembros del código C# y las tablas del script SQL. |
| `comun.py` | Procesos, casos de uso, requisitos funcionales y reglas de negocio. |
| `n01.py`, `n01_diag.py` | Textos y diagramas del N01. |
| `cu_venta.py`, `cu_venta2.py`, `cu_venta3.py`, `cu_sorteos.py`, `cu_pago_control.py` | Especificaciones CU-N01 a CU-N13. |
| `sd_venta.py`, `sd_sorteos.py`, `sd_pagos.py`, `sd_comun.py` | Diagramas de secuencia. |
| `cu.py`, `clases.py`, `secuencia.py`, `actividad.py`, `ruteo.py`, `drawio.py` | Dibujo de cada tipo de diagrama en draw.io. |
| `render.py` | Exporta a PNG con el visor oficial de draw.io (Chrome sin ventana). |
| `prototipos.py` | Marca las capturas de pantalla y arma las tablas de los prototipos. |
| `balanceo.py` | Verificador del balanceo (Apéndice A del documento). |
| `catalogo.py`, `word.py`, `armar_word.py`, `actualizar_word.ps1` | Armado del documento. |
