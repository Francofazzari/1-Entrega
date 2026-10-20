# -*- coding: utf-8 -*-
"""Control de balanceo: cada diagrama tiene que coincidir con el sistema y con los otros diagramas.

Para cada caso de uso verifica que:
  1. cada mensaje sincronico de un diagrama de secuencia sea un metodo, constructor o propiedad que
     existe en la clase de la linea de vida (con la cantidad de parametros correcta);
  2. cada clase de una linea de vida y cada operacion que recibe esten en un diagrama de clases del
     caso de uso, y que entre las clases que se envian mensajes haya una relacion dibujada;
  3. cada tabla que se lee o escribe en los mensajes a la base de datos este en el DER del caso;
  4. los textos citados entre comillas (mensajes del sistema) existan en el codigo;
  5. las referencias a pasos, flujos alternativos y reglas de negocio sean coherentes.

Uso: python balanceo.py            (todos los casos)
     python balanceo.py CU-N01     (uno solo)
"""
import re
import sys
from pathlib import Path

import clases as CL
from clases import CLASES, TABLAS
import comun

REPO = Path(r"C:\Users\franco\source\repos\Entrega1software")

# ------------------------------------------------------------------ corpus de textos del sistema
_RE_LIT = re.compile(r'"((?:[^"\\]|\\.)*)"')


def _corpus():
    textos = []
    for carpeta in ('Entrega1software', 'BLL', 'DAL'):
        for f in (REPO / carpeta).glob('*.cs'):
            s = f.read_text(encoding='utf-8-sig', errors='replace')
            for lit in _RE_LIT.findall(s):
                textos.append(lit.replace('\\n', ' ').replace('\\"', '"'))
    sql = (REPO / 'ScriptBD_AgenciaQuiniela.sql').read_text(encoding='utf-8-sig', errors='replace')
    textos.extend(re.findall(r"'([^']*)'", sql))
    return textos


CORPUS = _corpus()
CORPUS_UNIDO = ' \u0001 '.join(' '.join(t.split()) for t in CORPUS)
ETIQUETAS_UI = set(' '.join(t.split()) for t in CORPUS)

# etiquetas de controles estandar de Windows o del framework que no estan en el codigo
ETIQUETAS_EXTRA = {'Sí', 'No', 'Aceptar', 'Cancelar'}
NO_ETIQUETAS = {'create', 'use', 'include', 'extend', 'singleton', 'static', 'database', 'dispositivo',
                'actor', 'business worker', 'business actor', 'interface', 'enumeration'}

# marcadores de valores que arma el sistema: …, N, X, $ X, numeros de ejemplo y estados en minuscula
PLACEHOLDER = re.compile(r'…|\.\.\.|XXXXXXXX|\$ ?X\b|\bNN\b|\b[NMLTX]\b|\$?\d[\d.,]*\d|\$?\b\d\b|'
                         r'\b(?:cerrado|sorteado|abierto)\b|[•×·]')


def _en_partes(palabras, maximo=3):
    """True si la secuencia de palabras se puede formar con hasta 'maximo' textos seguidos del codigo
    (el sistema arma algunos mensajes concatenando, por ejemplo singular y plural)."""
    texto = ' '.join(palabras)
    if texto in CORPUS_UNIDO:
        return True
    if maximo == 1:
        return False
    for k in range(len(palabras) - 1, 0, -1):
        if ' '.join(palabras[:k]) in CORPUS_UNIDO and _en_partes(palabras[k:], maximo - 1):
            return True
    return False


def texto_existe(cita):
    """Fragmentos fijos de la cita (sin los marcadores de valores) que no estan en el codigo."""
    faltan = []
    for frag in PLACEHOLDER.split(cita):
        for oracion in re.split(r'(?<=[.?!:])\s+', frag):
            f = ' '.join(oracion.split()).strip(' ,;()«»"“”')
            if len(f) < 6:
                continue
            if not _en_partes(f.split()) and not _en_partes(f.rstrip('.:').split()):
                faltan.append(f)
    return faltan


# ------------------------------------------------------------------ mensajes
_RE_NOMBRE = re.compile(r'^(«create»\s*)?([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\s*(\((.*)\))?$', re.S)


def _partir_args(texto):
    texto = texto.strip()
    if not texto:
        return []
    args, prof, actual = [], 0, ''
    for ch in texto:
        if ch in '<([{':
            prof += 1
        elif ch in '>)]}':
            prof -= 1
        if ch == ',' and prof == 0:
            args.append(actual.strip())
            actual = ''
        else:
            actual += ch
    args.append(actual.strip())
    return args


def operaciones(texto):
    """[(nombre, nargs | None, es_create)] de un mensaje; [] si es un evento descripto en palabras."""
    t = ' '.join(texto.replace('\n', ' ').split())
    t = re.sub(r'\s*\[[^\]]*\]\s*$', '', t)
    if re.match(r'^\d+:', t):
        t = t.split(':', 1)[1].strip()
    partes = [p.strip() for p in re.split(r'\s+/\s+|\s+o\s+', t)]
    res = []
    for p in partes:
        m = _RE_NOMBRE.match(p)
        if not m:
            return []
        nombre = m.group(2).split('.')[-1]
        nargs = None if m.group(3) is None else len(_partir_args(m.group(4)))
        res.append((nombre, nargs, bool(m.group(1))))
    return res


def _existe(clase, nombre, nargs, es_create):
    c = CLASES.get(clase)
    if c is None:
        return 'la clase %s no existe en el código' % clase
    if es_create:
        ctores = c.buscar(clase, 'constructor')
        if not ctores:
            return None if not nargs else '%s no tiene constructor con %d parámetros' % (clase, nargs)
        if not any(len(m.params) - m.opcionales <= (nargs or 0) <= len(m.params) for m in ctores):
            return '%s no tiene constructor con %s parámetros' % (clase, nargs)
        return None
    ms = c.buscar(nombre)
    if not ms:
        return '%s no tiene el miembro %s' % (clase, nombre)
    if nargs is None:
        if not any(m.tipo in ('propiedad', 'campo', 'constante') for m in ms):
            return '%s.%s es un método: falta la lista de parámetros' % (clase, nombre)
        return None
    if not any(m.tipo in ('metodo', 'constructor') and len(m.params) - m.opcionales <= nargs <= len(m.params)
               for m in ms):
        firmas = ', '.join(m.uml() for m in ms)
        return '%s.%s con %d parámetros no coincide con %s' % (clase, nombre, nargs, firmas)
    return None


# ------------------------------------------------------------------ verificacion de un caso de uso
def _tablas_de(sql):
    res = set()
    for t in TABLAS:
        if re.search(r'\b%s\b' % re.escape(t), sql):
            res.add(t)
    return res


def _clases_de_diagramas(spec):
    vistas, rels = {}, set()
    for titulo, fn in spec['clases']:
        dc = fn()
        for nombre, v in dc.vistas.items():
            e = vistas.setdefault(nombre, dict(ops=set(), attrs=set()))
            e['ops'].update(v['ops'])
            e['attrs'].update(v['attrs'])
        for r in dc.rels:
            rels.add((r.origen, r.destino))
    return vistas, rels


def _pasos(texto):
    return [int(x) for x in re.findall(r'\bpasos?\s+(\d+)', texto)] + \
        [int(a) for a, b in re.findall(r'\bpasos\s+(\d+)\s+(?:a|y)\s+(\d+)', texto)] + \
        [int(b) for a, b in re.findall(r'\bpasos\s+(\d+)\s+(?:a|y)\s+(\d+)', texto)]


def verificar(spec, stats=None):
    """Lista de problemas del caso de uso. stats (dict) cuenta lo que se verifico, para el apendice del documento."""
    if stats is None:
        stats = {}
    for k in ('mensajes', 'citas', 'etiquetas', 'tablas'):
        stats.setdefault(k, 0)
    cod = spec['codigo']
    prob = []
    vistas, rels = _clases_de_diagramas(spec)
    der = spec['der']()
    tablas_der = {t for fila in der.filas for t in fila if t}
    n_pasos = len(spec['flujo'])
    alt_refs = {a[0] for a in spec['alternativos']}

    usadas_sql = set()
    for titulo, fn in spec['secuencias']:
        s = fn()
        clase_de = {}
        for l in s.lineas:
            if l['tipo'] == 'objeto':
                clase_de[l['id']] = l['texto'].split(':')[-1].strip()
        for (o, d, texto, tipo) in s.mensajes():
            if d in clase_de and tipo in ('llamada', 'crear'):
                cd = clase_de[d]
                ops = operaciones(texto)
                if not ops and tipo == 'crear':
                    prob.append('[%s] mensaje de creación sin constructor: %r' % (s.titulo, texto))
                for nombre, nargs, es_create in ops:
                    stats['mensajes'] += 1
                    if tipo == 'crear' and not es_create:
                        prob.append('[%s] la creación de %s no dice «create»' % (s.titulo, cd))
                    error = _existe(cd, nombre, nargs, es_create)
                    if error:
                        prob.append('[%s] %s  (mensaje %r)' % (s.titulo, error, texto))
                        continue
                    if cd not in vistas:
                        prob.append('[%s] la clase %s no está en ningún diagrama de clases del caso' % (s.titulo, cd))
                        continue
                    v = vistas[cd]
                    clave = cd if es_create else nombre
                    if nargs is None:
                        if clave not in v['attrs']:
                            prob.append('[%s] el diagrama de clases no muestra %s.%s (atributo)' % (s.titulo, cd, clave))
                    elif clave not in v['ops']:
                        prob.append('[%s] el diagrama de clases no muestra %s.%s()' % (s.titulo, cd, clave))
                co = clase_de.get(o)
                if co and co != cd and ops and (co, cd) not in rels:
                    prob.append('[%s] falta una relación %s -> %s en los diagramas de clases (mensaje %r)'
                                % (s.titulo, co, cd, texto))
            if tipo in ('llamada',) and any(l['id'] == d and l['tipo'] == 'bd' for l in s.lineas):
                usadas_sql |= _tablas_de(texto)
            for cita in re.findall(r'[“"]([^”"]{6,})[”"]', texto):
                stats['citas'] += 1
                for f in texto_existe(cita):
                    prob.append('[%s] texto que no está en el código: %r' % (s.titulo, f))
        for n in _pasos(titulo):
            if n > n_pasos:
                prob.append('[%s] el título nombra el paso %d y el flujo tiene %d pasos' % (titulo, n, n_pasos))
        for ref in re.findall(r'\b(\d+\.[a-z])\b', titulo):
            if ref not in alt_refs:
                prob.append('[%s] el título nombra el flujo %s, que no existe' % (titulo, ref))

    stats['tablas'] += len(usadas_sql)
    faltan = usadas_sql - tablas_der
    if faltan:
        prob.append('tablas usadas en los diagramas de secuencia que no están en el DER: %s' % ', '.join(sorted(faltan)))
    sobran = tablas_der - usadas_sql
    if sobran and not spec.get('der_nota'):
        prob.append('(info) tablas del DER que ningún mensaje lee ni escribe: %s' % ', '.join(sorted(sobran)))

    # textos de la plantilla
    textos = [t for _, t in spec['flujo']]
    for ref, tit, lineas, ret in spec['alternativos']:
        textos += [tit, ret] + list(lineas)
        paso = int(ref.split('.')[0])
        if paso > n_pasos:
            prob.append('flujo alternativo %s: el flujo normal tiene %d pasos' % (ref, n_pasos))
        for n in re.findall(r'paso (\d+)', ret):
            if int(n) > n_pasos:
                prob.append('flujo alternativo %s vuelve al paso %s, que no existe' % (ref, n))
    textos += spec['pre'] + spec['post'] + spec.get('post_fracaso', []) + spec['disparadores'] + [spec['objetivo']]
    for punto, paso, cu, cond in spec.get('extension', []):
        if paso > n_pasos:
            prob.append('punto de extensión "%s": paso %d inexistente' % (punto, paso))
    for item in spec.get('inclusion', []):
        if item[1] > n_pasos:
            prob.append('inclusión de %s en el paso %d inexistente' % (item[0], item[1]))
    for t in textos:
        for cita in re.findall(r'“([^”]{6,})”', t):
            stats['citas'] += 1
            for f in texto_existe(cita):
                prob.append('plantilla: texto que no está en el código: %r' % f)
        for etq in re.findall(r'«([^»]+)»', t):
            e = ' '.join(PLACEHOLDER.sub(' ', etq).split())
            if e in NO_ETIQUETAS or e in ETIQUETAS_EXTRA:
                continue
            stats['etiquetas'] += 1
            if e not in ETIQUETAS_UI:
                prob.append('plantilla: botón o etiqueta «%s» que no está en el código' % e)
    citadas = set(re.findall(r'RN-\d\d', ' '.join(textos)))
    for rn in citadas - set(spec['reglas']):
        prob.append('la regla %s se cita en la plantilla pero no está en la lista de reglas' % rn)
    for rn in set(spec['reglas']) - citadas:
        prob.append('(info) la regla %s está en la lista pero no se cita en ningún paso' % rn)
    return prob


def especificaciones():
    import cu_venta
    import cu_venta2
    import cu_venta3
    import cu_sorteos
    import cu_pago_control
    res = []
    for mod in (cu_venta, cu_venta2, cu_venta3, cu_sorteos, cu_pago_control):
        for k in dir(mod):
            if re.match(r'^CU_N\d\d$', k):
                res.append(getattr(mod, k))
    return sorted(res, key=lambda s: s['codigo'])


def trazabilidad():
    prob = []
    cubiertos = set()
    for rf, desc, proc, casos in comun.REQUISITOS:
        for c in casos:
            if c not in comun.CASOS:
                prob.append('%s nombra el caso %s, que no existe' % (rf, c))
            cubiertos.add(c)
    for c in comun.CASOS:
        if c not in cubiertos:
            prob.append('el caso %s no está asociado a ningún requisito funcional' % c)
    return prob


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    filtro = sys.argv[1] if len(sys.argv) > 1 else None
    total = 0
    for spec in especificaciones():
        if filtro and spec['codigo'] != filtro:
            continue
        p = verificar(spec)
        errores = [x for x in p if not x.startswith('(info)')]
        total += len(errores)
        print('=' * 8, spec['codigo'], comun.CASOS[spec['codigo']][0], '-', len(errores), 'problema(s)')
        for x in p:
            print('   ', x)
    t = trazabilidad()
    total += len(t)
    for x in t:
        print('   ', x)
    print('TOTAL', total)
