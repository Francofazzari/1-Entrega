# -*- coding: utf-8 -*-
"""Lee el codigo C# y el script SQL del sistema para que los diagramas usen exactamente los
mismos nombres, tipos y firmas (y para validar que todo lo que se dibuja existe)."""
import re
from pathlib import Path

REPO = Path(r"C:\Users\franco\source\repos\Entrega1software")
CARPETAS = {'BE': 'BE', 'BLL': 'BLL', 'DAL': 'DAL', 'UI': 'Entrega1software'}
VIS = {'public': '+', 'private': '-', 'protected': '#', 'internal': '~'}


def _limpiar(src):
    """Quita comentarios y contenido de strings/chars (deja las comillas) sin cambiar los renglones."""
    out = []
    i, n = 0, len(src)
    while i < n:
        c = src[i]
        if src.startswith('//', i):
            j = src.find('\n', i)
            j = n if j < 0 else j
            i = j
            continue
        if src.startswith('/*', i):
            j = src.find('*/', i + 2)
            j = n if j < 0 else j + 2
            out.append('\n' * src.count('\n', i, j))
            i = j
            continue
        if c == '@' and i + 1 < n and src[i + 1] == '"':
            j = i + 2
            while j < n:
                if src[j] == '"' and j + 1 < n and src[j + 1] == '"':
                    j += 2
                    continue
                if src[j] == '"':
                    break
                j += 1
            out.append('""' + '\n' * src.count('\n', i, j))
            i = j + 1
            continue
        if c == '$' and i + 1 < n and src[i + 1] == '"':
            i += 1
            c = '"'
        if c == '"':
            j = i + 1
            while j < n and src[j] != '"':
                j += 2 if src[j] == '\\' else 1
            out.append('""')
            i = j + 1
            continue
        if c == "'":
            j = i + 1
            while j < n and src[j] != "'":
                j += 2 if src[j] == '\\' else 1
            out.append("''")
            i = j + 1
            continue
        out.append(c)
        i += 1
    return ''.join(out)


def _partir_params(texto):
    params, prof, actual = [], 0, ''
    for ch in texto:
        if ch in '<([':
            prof += 1
        elif ch in '>)]':
            prof -= 1
        if ch == ',' and prof == 0:
            params.append(actual.strip())
            actual = ''
        else:
            actual += ch
    if actual.strip():
        params.append(actual.strip())
    res = []
    for p in params:
        p = p.split('=')[0].strip()
        p = re.sub(r'^(ref|out|params|this)\s+', '', p)
        partes = p.rsplit(' ', 1)
        if len(partes) == 2:
            res.append((partes[0].strip().replace('System.', ''), partes[1].strip()))
    return res


class Miembro:
    def __init__(self, tipo, vis, nombre, tipo_dato, params=None, estatico=False, valor=None, readonly=False):
        self.tipo = tipo            # campo | propiedad | metodo | constructor | constante
        self.vis = vis
        self.nombre = nombre
        self.tipo_dato = tipo_dato
        self.params = params or []
        self.estatico = estatico
        self.valor = valor
        self.readonly = readonly
        self.opcionales = 0         # parametros con valor por defecto

    def uml(self):
        v = VIS.get(self.vis, '+')
        if self.tipo in ('metodo', 'constructor'):
            ps = ', '.join('%s : %s' % (n, t) for t, n in self.params)
            s = '%s %s(%s)' % (v, self.nombre, ps)
            if self.tipo == 'metodo':
                s += ' : ' + self.tipo_dato
            return s
        s = '%s %s : %s' % (v, self.nombre, self.tipo_dato)
        if self.tipo == 'constante' and self.valor is not None:
            s += ' = ' + self.valor
        return s

    def __repr__(self):
        return self.uml()


class Clase:
    def __init__(self, nombre, capa, archivo):
        self.nombre = nombre
        self.capa = capa
        self.archivo = archivo
        self.miembros = []
        self.base = None
        self.estatica = False

    def buscar(self, nombre, tipo=None, nparams=None):
        res = [m for m in self.miembros if m.nombre == nombre and (tipo is None or m.tipo == tipo)]
        if nparams is not None:
            res = [m for m in res if len(m.params) == nparams]
        return res


_RE_CLASE = re.compile(r'^\s*(?:(?:public|internal|private)\s+)?(?:(static|abstract|sealed)\s+)?(?:partial\s+)?class\s+(\w+)(?:\s*:\s*([\w\s,<>]+))?')
_RE_METODO = re.compile(r'^\s*(public|private|protected|internal)\s+((?:(?:static|override|virtual|abstract|new)\s+)*)([\w<>\[\],\.\? ]+?)\s+(\w+)\s*\(([^)]*)\)\s*(?:\{.*|=>.*|;)?\s*$')
_RE_CTOR = re.compile(r'^\s*(public|private|protected|internal)\s+(\w+)\s*\(([^)]*)\)\s*(?::\s*(?:this|base)\(.*\))?\s*(\{.*)?$')
_RE_PROP = re.compile(r'^\s*(public|private|protected|internal)\s+((?:static\s+)?)([\w<>\[\],\.\?]+)\s+(\w+)\s*(\{\s*get|=>)')
_RE_PROP2 = re.compile(r'^\s*(public|private|protected|internal)\s+((?:static\s+)?)([\w<>\[\],\.\?]+)\s+(\w+)\s*$')
_RE_CAMPO_IGUAL = re.compile(r'^\s*(public|private|protected|internal)\s+((?:(?:static|readonly)\s+)*)([\w<>\[\],\.\? ]+?)\s+(\w+)\s*=\s*$')
_RE_CAMPO_INI = re.compile(r'^\s*(public|private|protected|internal)\s+((?:(?:static|readonly)\s+)*)([\w<>\[\],\.\? ]+?)\s+(\w+)\s*=\s*new\b.*$')
_RE_CAMPO = re.compile(r'^\s*(public|private|protected|internal)\s+((?:(?:static|readonly|const)\s+)*)([\w<>\[\],\.\? ]+?)\s+(\w+(?:\s*,\s*\w+)*)\s*(=\s*(.*))?;\s*$')


def leer_clases():
    clases = {}
    for capa, carpeta in CARPETAS.items():
        for f in sorted((REPO / carpeta).glob('*.cs')):
            if f.name.endswith('.Designer.cs') or f.name.startswith('AssemblyInfo'):
                continue
            original = f.read_text(encoding='utf-8-sig', errors='replace')
            src = _limpiar(original)
            lineas = src.split('\n')
            orig_lineas = original.split('\n')
            # firmas de metodos partidas en varios renglones: se unen en el primero
            i = 0
            while i < len(lineas):
                ln = lineas[i]
                if re.match(r'^\s*(public|private|protected|internal)\b', ln) and ln.count('(') > ln.count(')'):
                    j = i + 1
                    while j < len(lineas) and ln.count('(') > ln.count(')'):
                        ln = ln.rstrip() + ' ' + lineas[j].strip()
                        lineas[j] = ''
                        j += 1
                    lineas[i] = ln
                    i = j
                else:
                    i += 1
            prof = 0
            pila = []     # (clase, profundidad del cuerpo)
            for nl, linea in enumerate(lineas):
                m = _RE_CLASE.match(linea)
                if m and not linea.strip().startswith('//'):
                    c = Clase(m.group(2), capa, f.name)
                    c.estatica = m.group(1) == 'static'
                    if m.group(3):
                        c.base = m.group(3).strip()
                    if not pila:   # solo clases de primer nivel
                        clases.setdefault(c.nombre, c)
                        if clases[c.nombre] is not c:
                            c = clases[c.nombre]
                    pila.append([c, prof + 1, not pila, False])
                actual = pila[-1] if pila else None
                if actual and prof == actual[1] and actual[2]:
                    c = actual[0]
                    _miembro(c, linea, orig_lineas[nl] if nl < len(orig_lineas) else linea)
                prof += linea.count('{') - linea.count('}')
                for item in pila:
                    if prof >= item[1]:
                        item[3] = True
                while pila and pila[-1][3] and prof < pila[-1][1]:
                    pila.pop()
    return clases


def _opcionales(texto):
    """Cantidad de parametros que tienen valor por defecto."""
    prof, partes, actual = 0, [], ''
    for ch in texto:
        if ch in '<([':
            prof += 1
        elif ch in '>)]':
            prof -= 1
        if ch == ',' and prof == 0:
            partes.append(actual)
            actual = ''
        else:
            actual += ch
    partes.append(actual)
    return sum(1 for p in partes if '=' in p)


def _miembro(c, linea, original):
    if '(' in linea and not re.search(r'=\s*new\b', linea.split('(')[0]):
        m = _RE_CTOR.match(linea)
        if m and m.group(2) == c.nombre:
            c.miembros.append(Miembro('constructor', m.group(1), m.group(2), None, _partir_params(m.group(3))))
            c.miembros[-1].opcionales = _opcionales(m.group(3))
            return
        m = _RE_METODO.match(linea)
        if m and m.group(4) not in ('if', 'while', 'for', 'foreach', 'switch', 'using', 'return', 'new'):
            mods = m.group(2) or ''
            c.miembros.append(Miembro('metodo', m.group(1), m.group(4), m.group(3).strip(),
                                      _partir_params(m.group(5)), estatico='static' in mods))
            c.miembros[-1].opcionales = _opcionales(m.group(5))
            return
    m = _RE_PROP.match(linea) or _RE_PROP2.match(linea)
    if m:
        c.miembros.append(Miembro('propiedad', m.group(1), m.group(4), m.group(3), estatico=bool(m.group(2).strip())))
        return
    m = _RE_CAMPO.match(linea)
    if not m:
        mi = _RE_CAMPO_INI.match(linea) or _RE_CAMPO_IGUAL.match(linea)
        if mi and not linea.rstrip().endswith(';'):
            c.miembros.append(Miembro('campo', mi.group(1), mi.group(4), mi.group(3).strip(),
                                      estatico='static' in (mi.group(2) or ''), readonly='readonly' in (mi.group(2) or '')))
            return
    if m:
        mods = m.group(2) or ''
        tipo_dato = m.group(3).strip()
        valor = None
        if 'const' in mods:
            mo = re.match(r'.*=\s*(.*);', original.strip())
            valor = mo.group(1).strip() if mo else None
        elif 'static readonly' in mods and m.group(6):
            mo = re.match(r'.*=\s*(.*);', original.strip())
            valor = mo.group(1).strip() if mo else None
        for nombre in [x.strip() for x in m.group(4).split(',')]:
            c.miembros.append(Miembro('constante' if 'const' in mods else 'campo', m.group(1), nombre, tipo_dato,
                                      estatico=('static' in mods or 'const' in mods), valor=valor,
                                      readonly='readonly' in mods))


# ------------------------------------------------------------------ SQL
class Columna:
    def __init__(self, nombre, tipo, nulo, pk=False, fk=None, unico=False, defecto=None, check=None):
        self.nombre = nombre
        self.tipo = tipo
        self.nulo = nulo
        self.pk = pk
        self.fk = fk          # tabla referenciada
        self.unico = unico
        self.defecto = defecto
        self.check = check


class Tabla:
    def __init__(self, nombre):
        self.nombre = nombre
        self.columnas = []
        self.unicos = []      # restricciones UNIQUE compuestas

    def col(self, nombre):
        for c in self.columnas:
            if c.nombre == nombre:
                return c
        raise KeyError(self.nombre + '.' + nombre)


def leer_tablas():
    sql = (REPO / 'ScriptBD_AgenciaQuiniela.sql').read_text(encoding='utf-8-sig')
    sql = re.sub(r'--[^\n]*', '', sql)
    tablas = {}
    for m in re.finditer(r'CREATE TABLE\s+(\w+)\s*\((.*?)\);\s*GO', sql, re.S | re.I):
        t = Tabla(m.group(1))
        cuerpo = m.group(2)
        partes, prof, actual = [], 0, ''
        for ch in cuerpo:
            if ch == '(':
                prof += 1
            elif ch == ')':
                prof -= 1
            if ch == ',' and prof == 0:
                partes.append(actual.strip())
                actual = ''
            else:
                actual += ch
        if actual.strip():
            partes.append(actual.strip())
        pk_compuesta = []
        for p in partes:
            p1 = ' '.join(p.split())
            mu = re.match(r'CONSTRAINT\s+(\w+)\s+UNIQUE\s*\((.*)\)', p1, re.I)
            if mu:
                t.unicos.append((mu.group(1), [x.strip() for x in mu.group(2).split(',')]))
                continue
            mp = re.match(r'PRIMARY KEY\s*\((.*)\)', p1, re.I)
            if mp:
                pk_compuesta = [x.strip() for x in mp.group(1).split(',')]
                continue
            mc = re.match(r'(\w+)\s+(\w+(?:\(\d+(?:,\d+)?\))?)(.*)', p1)
            if not mc:
                continue
            nombre, tipo, resto = mc.group(1), mc.group(2).upper(), mc.group(3)
            if tipo.startswith('INT') and 'IDENTITY' in resto.upper():
                pass
            col = Columna(nombre, tipo, nulo=('NOT NULL' not in resto.upper() and 'PRIMARY KEY' not in resto.upper()))
            col.pk = 'PRIMARY KEY' in resto.upper()
            mf = re.search(r'REFERENCES\s+(\w+)\s*\(', resto, re.I)
            col.fk = mf.group(1) if mf else None
            col.unico = bool(re.search(r'\bUNIQUE\b', resto, re.I))
            md = re.search(r'DEFAULT\s+(\'[^\']*\'|\w+\(\)|\d+)', resto, re.I)
            col.defecto = md.group(1) if md else None
            mk = re.search(r'CHECK\s*\((.*)\)', resto, re.I)
            col.check = mk.group(1) if mk else None
            t.columnas.append(col)
        for nombre in pk_compuesta:
            t.col(nombre).pk = True
        tablas[t.nombre] = t
    # columnas agregadas con ALTER TABLE
    for m in re.finditer(r'ALTER TABLE\s+(\w+)\s+ADD\s+(\w+)\s+(\w+)\s+(NULL|NOT NULL)', sql, re.I):
        if m.group(1) in tablas:
            tablas[m.group(1)].columnas.append(Columna(m.group(2), m.group(3).upper(), m.group(4).upper() == 'NULL'))
    return tablas


if __name__ == '__main__':
    cl = leer_clases()
    for nombre in ['ApuestaBLL', 'SorteoBLL', 'FormApuesta', 'Apuesta', 'ApuestaDAL', 'SesionManager', 'FormReporte', 'Azar']:
        c = cl[nombre]
        print('==', nombre, c.capa, c.base, len(c.miembros))
        for m in c.miembros:
            print('   ', m.tipo, '|', m.uml(), '| static' if m.estatico else '')
    tb = leer_tablas()
    for n, t in tb.items():
        print(n, [(c.nombre, c.tipo, 'NULL' if c.nulo else '', 'PK' if c.pk else '', c.fk or '', 'UQ' if c.unico else '') for c in t.columnas], t.unicos)
