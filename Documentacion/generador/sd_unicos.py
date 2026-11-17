# -*- coding: utf-8 -*-
"""Un único diagrama de secuencia por caso de uso (CU-N01 a CU-N13), por capas, mensaje por mensaje según el código.

Cada diagrama recorre el flujo normal completo, de principio a fin, y muestra los flujos alternativos con
fragmentos alt / opt / break / loop en el lugar donde ocurren. Todo lo que hace el caso de uso está en su propio
diagrama: cuando una pantalla se abre y carga datos, la lectura se dibuja completa (formulario -> capa de negocio ->
capa de datos -> base de datos). Solo se usa un marco ref para otro caso de uso (una inclusión o una extensión),
nunca para partir un caso de uso en varios diagramas. Cuando la precondición es tener una pantalla abierta (CU-N08 a
CU-N10 y CU-N13), igual se dibuja cómo se abrió, con todo lo que lee de la base de datos.

Líneas de vida (para que el diagrama entre a lo ancho de una hoja vertical):
  - el actor y cada formulario (pantalla) que interviene, en el orden en que aparecen;
  - "Capa de negocio (BLL)": todas las clases de la BLL (ApuestaBLL, SorteoBLL, SesionManager, Azar, ...);
  - "Capa de datos (DAL)": todas las clases de la DAL (ApuestaDAL, SorteoDAL, ...);
  - la base de datos AgenciaQuiniela.
Cada mensaje a una capa lleva el nombre de la clase que lo recibe: "ApuestaBLL.Vender(...)". Una llamada entre dos
clases de la misma capa (por ejemplo ApuestaBLL -> SorteoBLL) se dibuja como un mensaje a sí misma de esa capa.
Las clases auxiliares de la UI que solo usa un formulario (TicketRenderer) se dibujan sobre ese formulario.

Convenciones (las mismas en todos los diagramas):
  - Cada mensaje sincrónico es un método, constructor o propiedad que existe en la clase nombrada, con sus
    argumentos tal como figuran en el código; las respuestas (línea punteada) se dibujan solo cuando el método
    devuelve un valor.
  - Las excepciones se muestran con un fragmento break y una respuesta "Exception" hacia quien llamó.
  - Los mensajes a la base de datos muestran la sentencia SQL que ejecuta la capa DAL (abreviada con …).
  - El balanceo (balanceo.py) se sigue verificando clase por clase: cada mensaje conserva la clase que lo recibe.
"""
import re
from contextlib import contextmanager

import secuencia
from secuencia import _Ev
from drawio import ancho, T, TC

BD = 'AgenciaQuiniela'      # nombre de la base de datos en las líneas de vida «database»

CAPAS = {'bll': 'Capa de negocio\n(BLL)', 'dal': 'Capa de datos\n(DAL)'}
# clases auxiliares de la UI: sus llamadas se dibujan sobre el formulario que las usa
AYUDANTES_UI = {'TicketRenderer': 'ft'}


def _clase(texto):
    return texto.replace('\n', '').split(':')[-1].strip()


def _capa(clase):
    if clase.endswith('DAL'):
        return 'dal'
    if clase.endswith('BLL') or clase in ('SesionManager', 'Azar'):
        return 'bll'
    return None


class Secuencia(secuencia.Secuencia):
    """Diagrama de secuencia por capas (ver el comentario del módulo)."""
    # letra más grande que en los demás diagramas: el diagrama va a todo el ancho de una hoja vertical, y como las
    # etiquetas se parten a un ancho fijo, agrandar la letra agrega renglones (más alto) pero casi no ensancha
    T = 17
    TC = 16
    LINEA_TXT = 19
    FILA = 38
    FILA_EXTRA = 19
    MAX_ETQ = 165
    MAX_PROPIA = 150
    ANCHO_REF = 260
    MIN_CAB = 90
    ALTO_CAB = 56
    MAX_GUARDA = 140
    MAX_NOTA = 170

    def __init__(self, titulo, nombre=None):
        super().__init__(titulo, nombre)
        self.alias = {}             # línea de vida del código -> línea de vida dibujada
        self.clase_de = {}          # línea de vida del código -> clase
        self.lineas_balanceo = []   # una línea de vida por clase, como en el código (para balanceo.py)

    # ------------------------------------------------------------ participantes
    def actor(self, ident, texto):
        super().actor(ident, texto)
        self.alias[ident] = ident
        self.lineas_balanceo.append(dict(id=ident, texto=texto, tipo='actor', creado=False))

    def bd(self, ident, texto):
        super().bd(ident, texto)
        self.alias[ident] = ident
        self.lineas_balanceo.append(dict(id=ident, texto=texto, tipo='bd', creado=False))

    def objeto(self, ident, texto, creado=False):
        clase = _clase(texto)
        self.clase_de[ident] = clase
        self.lineas_balanceo.append(dict(id=ident, texto=texto, tipo='objeto', creado=creado))
        capa = _capa(clase)
        if clase in AYUDANTES_UI:
            self.alias[ident] = AYUDANTES_UI[clase]
        elif capa:
            if capa not in self.alias.values():
                super().objeto(capa, CAPAS[capa])
            self.alias[ident] = capa
        else:
            super().objeto(ident, _partir_nombre(texto, self.MIN_CAB - 20, self.T), creado)
            self.alias[ident] = ident

    def clases(self):
        return [_clase(l['texto']) for l in self.lineas_balanceo if l['tipo'] == 'objeto']

    # ------------------------------------------------------------ mensajes
    def _mapear(self, origen, destino, texto):
        o, d = self.alias[origen], self.alias[destino]
        cd = self.clase_de.get(destino)
        if cd and (self.alias[destino] != destino) and cd != self.clase_de.get(origen):
            texto = cd + '.' + texto
        return o, d, texto

    @contextmanager
    def llamada(self, origen, destino, texto, ret=None):
        o, d, t = self._mapear(origen, destino, texto)
        with super().llamada(o, d, t, ret) as ev:
            ev.orig = (origen, destino, texto)
            yield ev

    @contextmanager
    def crear(self, origen, destino, texto):
        with super().crear(self.alias[origen], self.alias[destino], texto) as ev:
            ev.orig = (origen, destino, texto)
            yield ev

    def asincrono(self, origen, destino, texto):
        ev = self._agregar(_Ev('asinc', origen=self.alias[origen], destino=self.alias[destino], texto=texto,
                               ret=None))
        ev.orig = (origen, destino, texto)

    def respuesta(self, origen, destino, texto):
        ev = self._agregar(_Ev('resp', origen=self.alias[origen], destino=self.alias[destino], texto=texto, ret=None))
        ev.orig = (origen, destino, texto)

    def nota(self, linea, texto, ancho_nota=230):
        super().nota(self.alias[linea], texto, min(ancho_nota, self.MAX_NOTA))

    def destruir(self, linea):
        super().destruir(self.alias[linea])

    def referencia(self, texto, lineas):
        vistas = []
        for l in lineas:
            if self.alias[l] not in vistas:
                vistas.append(self.alias[l])
        super().referencia(texto, vistas)

    # ------------------------------------------------------------ dibujo
    def _etq(self, texto, numero, propia=False):
        """Como en Secuencia, pero un renglón largo también se puede cortar después de un punto, de un paréntesis
        o de una coma: 'SorteoBLL.ObtenerNumerosGanadores(c.Sorteo)' no tiene espacios."""
        t = '%d: %s' % (numero, texto) if self.numerar and numero else texto
        maximo = self.MAX_PROPIA if propia else self.MAX_ETQ
        # al dibujar, una flecha larga lleva su etiqueta en menos renglones (todo el ancho de la flecha)
        espacio = getattr(self, '_espacio_de', {}).get(numero) if numero and not propia else None
        if espacio:
            maximo = max(maximo, espacio)
        # los saltos de renglón escritos para las etiquetas anchas de los otros diagramas se ignoran: el texto se
        # vuelve a partir parejo al ancho de estos diagramas (si no, quedan palabras sueltas en un renglón)
        return '\n'.join(_partir_codigo(' '.join(t.split()), maximo, self.TC))

    def _calcular_x(self):
        """Ubica las líneas de vida con las etiquetas partidas a MAX_ETQ y después anota, para cada mensaje, el ancho
        real de su flecha: al dibujar, la etiqueta se parte a ese ancho."""
        self._espacio_de = {}
        super()._calcular_x()
        idx = self._indice()
        espacio = {}

        def f(ev):
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp') and ev.origen != ev.destino:
                a, b = idx[ev.origen], idx[ev.destino]
                libre = abs(self.x[a] - self.x[b]) - 44
                if ev.tipo == 'crear':
                    libre -= self.anchos_cab[b] / 2
                espacio[ev.num] = libre
                if ev.num_ret:
                    espacio[ev.num_ret] = libre
        self._recorrer(self.raiz, f)
        self._espacio_de = espacio

    def _ev_llamada(self, ev, prof):
        """Mensaje a sí misma con una etiqueta de varios renglones y mensajes internos: los mensajes internos
        empiezan debajo de la etiqueta (en Secuencia se superponen con su mitad inferior)."""
        if ev.origen != ev.destino or not ev.hijos:
            return super()._ev_llamada(ev, prof)
        etq = self._etq(ev.texto, ev.num, True)
        extra = (self.FILA_EXTRA + 1) // 2 * (len(self._lineas_texto(etq)) - 1)
        y = self.y + 6 + extra
        self._propia(ev.origen, y, etq)
        self.y = y + 16
        self._abrir(ev.destino, self.y)
        self.y += 9 + extra
        self._dibujar_nodo(ev, prof)
        self.y += 1
        self._cerrar(ev.destino, self.y)
        self.y += 4

    def _ev_crear(self, ev, prof):
        # una etiqueta de creación de varios renglones sube por encima de la flecha: se deja lugar para que no tape
        # el mensaje anterior
        self.y += self.FILA_EXTRA * (len(self._etq(ev.texto, ev.num).split('\n')) - 1)
        super()._ev_crear(ev, prof)

    def dibujar(self):
        # el actor y los formularios primero, después las capas y la base de datos
        orden = {'actor': 0, 'objeto': 1, 'bd': 4}
        self.lineas.sort(key=lambda l: 2 if l['id'] == 'bll' else 3 if l['id'] == 'dal' else orden[l['tipo']])
        return super().dibujar()

    def mensajes(self):
        """Mensajes con la clase que los recibe, como en el código (los usa balanceo.py)."""
        res = []

        def f(ev):
            if ev.tipo in ('llamada', 'crear', 'asinc', 'resp'):
                o, d, t = getattr(ev, 'orig', (ev.origen, ev.destino, ev.texto))
                res.append((o, d, t, ev.tipo))
        self._recorrer(self.raiz, f)
        return res


def _partir_codigo(linea, maximo, tam=TC):
    """Parte un renglón de una etiqueta en renglones de 'maximo' px como mucho, cortando después de un espacio, un
    punto, un paréntesis que abre o una coma. Los renglones que siguen van con una pequeña sangría."""
    if ancho(linea, tam) <= maximo:
        return [linea]
    piezas = []
    for p in re.split(r'(?<=[ (.,])', linea):
        if not p:
            continue
        if piezas and len(p.strip()) <= 3:
            piezas[-1] += p              # ')' o 'e)' no quedan solos en un renglón
        elif piezas and re.match(r'^\d+: $', piezas[-1]):
            piezas[-1] += p              # el número del mensaje va con lo que sigue
        else:
            piezas.append(p)
    lineas, actual = [], ''
    for p in piezas:
        if not p:
            continue
        prueba = actual + p
        if ancho(prueba.rstrip(), tam) <= maximo or not actual.strip():
            actual = prueba
        else:
            lineas.append(actual.rstrip())
            actual = '  ' + p.lstrip()
    if actual.strip():
        lineas.append(actual.rstrip())
    return lineas


def _partir_nombre(texto, maximo, tam=T):
    """':FormOperacionTicket' -> ':FormOperacion' + salto + 'Ticket': los nombres largos de las cabeceras van en dos
    renglones, cortados donde empieza una palabra (mayúscula), lo más parejo posible."""
    if '\n' in texto or ancho(texto, tam, True) <= maximo:
        return texto
    cortes = [i for i in range(2, len(texto)) if texto[i].isupper() and texto[i - 1].islower()]
    if not cortes:
        return texto
    i = min(cortes, key=lambda c: max(ancho(texto[:c], tam, True), ancho(texto[c:], tam, True)))
    return texto[:i] + '\n' + texto[i:]


# Sentencias SQL de la capa DAL, abreviadas con … y partidas en renglones cortos.
SQL_LOTERIAS = 'SELECT Id, Nombre,\nCodigo, Activa\nFROM LOTERIAS\nWHERE Activa = 1\nORDER BY Nombre'
SQL_TURNOS = 'SELECT Id, Nombre, Activo\nFROM TURNOS\nWHERE Activo = 1\nORDER BY Id'
SQL_JUEGOS = 'SELECT … FROM JUEGOS j\nWHERE j.Activo = 1\nORDER BY j.Id'
SQL_VENTAS = ('SELECT … FROM APUESTAS\nJOIN USUARIOS CROSS APPLY\n(JUGADAS JOIN SORTEOS)\nJOIN JUEGOS WHERE\n'
              'FechaHora en [desde, hasta)\n[AND juego]')
SQL_PAGOS = ('SELECT … FROM PAGOS\nJOIN APUESTAS JOIN USUARIOS\nCROSS APPLY (JUGADAS\nJOIN SORTEOS) JOIN JUEGOS\n'
             'WHERE FechaHora en\n[desde, hasta)')
SQL_TICKET = 'SELECT … FROM APUESTAS\nJOIN USUARIOS WHERE\nCodigoTicket = @codigo'
SQL_JUGADAS = 'SELECT … FROM JUGADAS\nJOIN SORTEOS JOIN JUEGOS\nLEFT JOIN LOTERIAS, TURNOS\nWHERE IdApuesta = @id'
SQL_NUMEROS_TICKET = 'SELECT … FROM\nJUGADA_NUMERO JOIN JUGADAS\nWHERE IdApuesta = @id'
SQL_PREFIJO = 'SELECT TOP 2 CodigoTicket\nFROM APUESTAS WHERE\nCONVERT(…, CodigoTicket)\nLIKE @prefijo'
SQL_SORTEO = 'SELECT … FROM SORTEOS\nJOIN JUEGOS LEFT JOIN\nLOTERIAS, TURNOS WHERE\njuego, fecha, lotería\ny turno'
SQL_INSERT_SORTEO = "INSERT INTO SORTEOS\n(…, Estado) VALUES\n(…, 'Abierto');\nSELECT SCOPE_IDENTITY()"
SQL_NUMEROS = 'SELECT Numero, Posicion\nFROM NUMERO_GANADOR\nWHERE IdSorteo = @id\nORDER BY Posicion'
SQL_BITACORA = 'INSERT INTO Bitacora\n(FechaHora, UsuarioId,\nUsuarioNombre,\nActividad, Detalle)'


def bitacora(s, origen, usuario, actividad):
    """Registro de un evento en la bitácora: BitacoraBLL -> BitacoraDAL -> base de datos."""
    with s.llamada(origen, 'bb', 'Registrar(%s.Id, %s.Nombre,\n"%s", detalle)' % (usuario, usuario, actividad)):
        with s.llamada('bb', 'bdal', 'Registrar(b)'):
            s.mensaje('bdal', 'bd', SQL_BITACORA)


# ====================================================================== piezas que se repiten
# Cada pieza dibuja mensajes sobre una Secuencia que ya tiene declaradas las líneas de vida que usa.

def _leer_loterias(s, origen, ret='List<Loteria>'):
    with s.llamada(origen, 'lb', 'ObtenerActivas()', ret=ret):
        with s.llamada('lb', 'ld', 'ObtenerActivas()', ret='lista'):
            s.mensaje('ld', 'bd', SQL_LOTERIAS, ret='filas')


def _leer_turnos(s, origen, ret='List<Turno>'):
    with s.llamada(origen, 'tb', 'ObtenerActivos()', ret=ret):
        with s.llamada('tb', 'td', 'ObtenerActivos()', ret='lista'):
            s.mensaje('td', 'bd', SQL_TURNOS, ret='filas')


def _buscar_o_crear(s):
    """Interior de SorteoBLL.ObtenerOCrear (con la línea de vida 'sb' activa)."""
    with s.llamada('sb', 'sb', 'Buscar(juego, loteria, turno, fecha)'):
        with s.llamada('sb', 'sd', 'Buscar(juego.Id, idLoteria,\nidTurno, fecha.Date)', ret='Sorteo o null'):
            s.mensaje('sd', 'bd', SQL_SORTEO, ret='fila o vacío')
    with s.fragmento('opt', '[el sorteo no existe]'):
        with s.llamada('sb', 'sd', 'Insertar(nuevo)', ret='nuevo.Id'):
            s.mensaje('sd', 'bd', SQL_INSERT_SORTEO, ret='Id')


def _listar_ventas(s, origen, argumentos, ret='List<Apuesta>'):
    with s.llamada(origen, 'ab', 'ListarVentas(%s)' % argumentos, ret=ret):
        with s.fragmento('break', '[desde.Date > hasta.Date]'):
            s.respuesta('ab', origen, 'Exception: "La fecha \'Desde\' no puede\nser mayor que la fecha \'Hasta\'."')
        with s.llamada('ab', 'ad', 'ListarVentas(desde.Date,\nhasta.Date.AddDays(1), idJuego)', ret='List<Apuesta>'):
            s.mensaje('ad', 'bd', SQL_VENTAS, ret='filas')


def _obtener_por_codigo(s):
    """Lectura del ticket completo (con 'ab' activa)."""
    with s.llamada('ab', 'ad', 'ObtenerPorCodigo(codigo)', ret='Apuesta o null'):
        s.mensaje('ad', 'bd', SQL_TICKET, ret='cabecera')
        s.mensaje('ad', 'bd', SQL_JUGADAS, ret='jugadas')
        s.mensaje('ad', 'bd', SQL_NUMEROS_TICKET, ret='números')


def _releer_ticket(s, origen):
    """Después de operar, el formulario vuelve a leer el ticket y la lista de tickets de hoy."""
    with s.llamada(origen, 'ab', 'BuscarPorCodigo(\nApuestaBLL.CodigoCompleto(apuesta))', ret='a'):
        _obtener_por_codigo(s)
    s.mensaje(origen, origen, 'MostrarTicket(a)')
    with s.llamada(origen, origen, 'CargarTicketsDeHoy()'):
        _listar_ventas(s, origen, 'DateTime.Today,\nDateTime.Today, null')
        with s.fragmento('loop', '[por cada ticket a de la lista]'):
            s.mensaje(origen, 'ab', 'CodigoCorto(a)', ret='código corto')


def _abrir_operacion(s, boton, cierre):
    """«Pagar», «Cancelar» o «Reimprimir» abre FormOperacionTicket, que lista los tickets vendidos hoy."""
    with s.llamada('op', 'fp', boton):
        with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
            with s.crear('fp', 'fo', '«create» FormOperacionTicket(modo)'):
                s.mensaje('fo', 'fo', 'CrearResumen()')
                s.mensaje('fo', 'fo', 'CrearColumnas()')
                s.mensaje('fo', 'fo', 'MostrarTicket(null)')
                with s.llamada('fo', 'fo', 'CargarTicketsDeHoy()'):
                    _listar_ventas(s, 'fo', 'DateTime.Today,\nDateTime.Today, null')
                    with s.fragmento('break', '[se lanzó una excepción]'):
                        s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudieron cargar los tickets '
                                                'de hoy"')
                    with s.fragmento('loop', '[por cada ticket a de la lista]'):
                        s.mensaje('fo', 'ab', 'CodigoCorto(a)', ret='código corto')
    s.asincrono('fo', 'op', cierre)


VISTAS = {
    'Venta': 'muestra el ticket con el título "Venta registrada"',
    'Reimpresion': 'muestra el ticket con el título "Reimpresión\ndel ticket" y la marca *** REIMPRESIÓN ***',
    'Consulta': 'muestra "Ticket XXXXXXXX" con el estado,\nsin el botón «Imprimir»',
}


def _mostrar_ticket(s, creador, argumento, vista, op='op', abs_='ab'):
    """Pantalla del ticket (FormTicket), la misma para la venta, la reimpresión y la consulta."""
    with s.crear(creador, 'ft', '«create» FormTicket(%s, VistaTicket.%s)' % (argumento, vista)):
        s.mensaje('ft', abs_, 'CodigoCorto(apuesta)', ret='código corto')
        s.mensaje('ft', 'tr', 'Dibujar(g, apuesta, esReimpresion, 0, 0)', ret='alto')
    s.asincrono('ft', op, VISTAS[vista])
    with s.llamada('ft', 'ft', 'pnlPapel_Paint(sender, e)'):
        s.mensaje('ft', 'tr', 'Dibujar(e.Graphics, apuesta, esReimpresion,\nMargenPapel, MargenPapel)', ret='alto')
    with s.fragmento('opt', '[presiona «Copiar código»]'):
        with s.llamada(op, 'ft', 'btnCopiar_Click(sender, e)'):
            s.mensaje('ft', abs_, 'CodigoCompleto(apuesta)', ret='código completo')
        s.asincrono('ft', op, '"Código copiado"')
    if vista != 'Consulta':
        with s.fragmento('opt', '[presiona «Imprimir»]'):
            s.referencia('sd CU-N03 Imprimir ticket (extensión)', [op, 'ft'])
    s.asincrono(op, 'ft', 'presiona «Cerrar»')
    s.destruir('ft')


def _confirmar_respuesta(s, form, actor, pregunta):
    with s.llamada(form, form, 'Confirmar(pregunta, titulo)', ret='respuesta'):
        s.asincrono(form, actor, pregunta)
        s.asincrono(actor, form, 'Sí / No')


def _construir_reporte(s):
    """FormReporte() arma las tres pestañas y carga las ventas y los pagos del día."""
    with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
        with s.crear('fp', 'fr', '«create» FormReporte()'):
            with s.llamada('fr', 'fr', 'FormReporte(VistaReporte.Ventas,\nDateTime.Today)'):
                s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario (esAdmin)')
                with s.llamada('fr', 'jb', 'ObtenerActivos()', ret='juegos'):
                    with s.llamada('jb', 'jd', 'ObtenerActivos()', ret='lista'):
                        s.mensaje('jd', 'bd', SQL_JUEGOS, ret='filas')
                s.mensaje('fr', 'fr', 'ConstruirVentas(desde)')
                s.mensaje('fr', 'fr', 'ConstruirPagos(desde)')
                with s.llamada('fr', 'fr', 'ConstruirGanadores()'):
                    _leer_loterias(s, 'fr', ret='loterias')
                    _leer_turnos(s, 'fr', ret='turnos')
                with s.llamada('fr', 'fr', 'CargarVentas()'):
                    _listar_ventas(s, 'fr', 'dtpVentasDesde.Value,\ndtpVentasHasta.Value, juego', ret='ventas')
                    with s.fragmento('loop', '[por cada ticket a de ventas]'):
                        s.mensaje('fr', 'ab', 'CodigoCorto(a)', ret='código corto')
                with s.llamada('fr', 'fr', 'CargarPagos()'):
                    _listar_pagos(s)
                s.mensaje('fr', 'fr', 'MostrarVista(vista)')


def _listar_pagos(s):
    with s.llamada('fr', 'pgb', 'Listar(dtpPagosDesde.Value,\ndtpPagosHasta.Value)', ret='pagos'):
        with s.fragmento('break', '[desde.Date > hasta.Date]'):
            s.respuesta('pgb', 'fr', 'Exception: "La fecha \'Desde\' no puede\nser mayor que la fecha \'Hasta\'."')
        with s.llamada('pgb', 'pgd', 'Listar(desde.Date,\nhasta.Date.AddDays(1))', ret='List<Pago>'):
            s.mensaje('pgd', 'bd', SQL_PAGOS, ret='filas')


def _ver_sorteo(s, actor):
    """FormReporte.VerSorteo(): busca cada sorteo elegido y, si está sorteado, lee sus números ganadores."""
    with s.llamada('fr', 'fr', 'VerSorteo()'):
        with s.alternativa() as a:
            with a.caso('[la lectura no falla]'):
                s.mensaje('fr', 'fr', 'LoteriasElegidas()')
                s.mensaje('fr', 'fr', 'TurnosElegidos()')
                with s.fragmento('loop', '[por cada lotería l y turno t]'):
                    with s.llamada('fr', 'sb', 'Buscar(juegoGanadores, l, t, fecha)', ret='c.Sorteo'):
                        with s.llamada('sb', 'sd', 'Buscar(juego.Id, idLoteria,\nidTurno, fecha.Date)',
                                       ret='Sorteo o null'):
                            s.mensaje('sd', 'bd', SQL_SORTEO, ret='fila o vacío')
                    with s.fragmento('opt', '[c.Sorteo != null y está Sorteado]'):
                        with s.llamada('fr', 'sb', 'ObtenerNumerosGanadores(c.Sorteo)', ret='c.Numeros'):
                            with s.llamada('sb', 'sd', 'ObtenerNumerosGanadores(s.Id)', ret='List<NumeroGanador>'):
                                s.mensaje('sd', 'bd', SQL_NUMEROS, ret='filas')
                with s.alternativa() as b:
                    with b.caso('[un solo sorteo]'):
                        with s.fragmento('opt', '[el sorteo todavía no existe]'):
                            s.mensaje('fr', 'fr', 'Virtual(celdas[0], fecha)')
                        s.mensaje('fr', 'sb', 'Descripcion(descripto)', ret='texto del estado')
                    with b.caso('[varios sorteos]'):
                        with s.fragmento('loop', '[por cada sorteo elegido c]'):
                            s.mensaje('fr', 'fr', 'Pendiente(c)')
            with a.caso('[se lanzó una excepción]'):
                s.asincrono('fr', actor, 'muestra ex.Message con el título\n"Números ganadores"')
        s.mensaje('fr', 'fr', 'DibujarPizarra(sender, e)\n[pnlPizarra.Invalidate()]')


def _guardar_resultado(s, form):
    """Interior de SorteoBLL.CargarResultado (con 'sb' activa): valida, completa y guarda el resultado."""
    with s.fragmento('break', '[ya Sorteado o fecha futura (13.a)]' if form == 'fres' else
                     '[ya Sorteado o fecha futura (10.a)]'):
        s.respuesta('sb', form, 'Exception: "Ese sorteo ya se realizó." o\n"No se puede realizar un sorteo de\nuna '
                                'fecha futura."')
    _completar_resultado(s, form)
    with s.llamada('sb', 'sd', 'GuardarResultado(s.Id, numeros, Sorteado)', ret='true / false'):
        s.mensaje('sd', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('sd', 'bd', "UPDATE SORTEOS SET\nEstado = 'Sorteado'\nWHERE Id = @id AND\nEstado <> 'Sorteado'",
                  ret='filas afectadas')
        with s.fragmento('break', '[no cambió ninguna fila]'):
            s.mensaje('sd', 'bd', 'ROLLBACK')
            s.respuesta('sd', 'sb', 'false')
        with s.fragmento('loop', '[por cada número ganador n]'):
            s.mensaje('sd', 'bd', 'INSERT INTO\nNUMERO_GANADOR (IdSorteo,\nNumero, Posicion)')
        s.mensaje('sd', 'bd', 'COMMIT')
    with s.fragmento('break', '[GuardarResultado == false]'):
        s.respuesta('sb', form, 'Exception: "Ese sorteo ya se realizó."')
    s.mensaje('sb', 'sb', 'Descripcion(s)')
    bitacora(s, 'sb', 'usuario', 'Sorteo realizado')


def _completar_resultado(s, form):
    """SorteoBLL.CompletarResultado: valida los números cargados y completa al azar los puestos vacíos."""
    with s.llamada('sb', 'sb', 'CompletarResultado(s.Juego, cargados)'):
        with s.fragmento('break', '[cargados.Count != juego.CantidadSorteados]'):
            s.respuesta('sb', form, 'Exception: "En … salen N números por sorteo."')
        if form == 'fres':
            with s.fragmento('break', '[número inválido o repetido (14.a)]'):
                s.respuesta('sb', form, 'Exception: "El número del puesto N (X) no es\nválido: van del … al …" o "El '
                                        'número …\nestá repetido."')
        with s.fragmento('loop', '[por cada puesto vacío]'):
            with s.fragmento('loop', '[mientras n esté usado o repita terminación]'):
                s.mensaje('sb', 'az', 'Entre(juego.NumeroMin,\njuego.NumeroMax)', ret='n')
        with s.fragmento('loop', '[por cada puesto i, del 1 al último]'):
            s.mensaje('sb', 'jbs', 'FormatearNumero(juego, valores[i].Value)', ret='número con ceros')


# ====================================================================== CU-N01 Vender apuesta de Quiniela
def n01_sd():
    s = Secuencia('sd CU-N01 Vender apuesta de Quiniela')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fsl', ':FormSeleccion\nLoteria', creado=True)
    s.objeto('fa', ':FormApuesta', creado=True)
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('tb', ':TurnoBLL')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('ld', ':LoteriaDAL')
    s.objeto('td', ':TurnoDAL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)

    # pasos 1 y 2: abrir la selección de loterías y horarios (carga las loterías y los turnos activos)
    with s.llamada('op', 'fp', 'BtnJuego_Click(sender, e)'):
        with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
            with s.crear('fp', 'fsl', '«create» FormSeleccionLoteria(juego)'):
                with s.llamada('fsl', 'fsl', 'CargarLoterias()'):
                    _leer_loterias(s, 'fsl', ret='todasLasLoterias')
                with s.llamada('fsl', 'fsl', 'CargarTurnos()'):
                    _leer_turnos(s, 'fsl', ret='todosLosTurnos')
                s.mensaje('fsl', 'fsl', 'ActualizarResumen()')
    s.asincrono('fsl', 'op', 'muestra un botón por lotería y por horario,\n«Todas», «Todos» y el resumen')

    # pasos 3 y 4: elegir las loterías y los horarios
    with s.fragmento('loop', '[hasta elegir las loterías y los horarios]'):
        with s.alternativa() as a:
            with a.caso('[una lotería o un horario]'):
                with s.llamada('op', 'fsl', 'BtnLoteria_Click(sender, e)\no BtnTurno_Click(sender, e)'):
                    s.mensaje('fsl', 'fsl', 'ToggleSeleccion(btn, idsSeleccionados)')
                    s.mensaje('fsl', 'fsl', 'ActualizarBotonTodos(btnTodos,\nidsSeleccionados, totalDisponibles)')
                    s.mensaje('fsl', 'fsl', 'ActualizarResumen()')
            with a.caso('[«Todas» o «Todos» (3.a)]'):
                with s.llamada('op', 'fsl', 'BtnTodasLoterias_Click(sender, e)\no BtnTodosTurnos_Click(sender, e)'):
                    with s.llamada('fsl', 'fsl', 'ToggleTodos(panel, btnTodos,\nidsSeleccionados, totalDisponibles)'):
                        s.mensaje('fsl', 'fsl', 'ActualizarBotonTodos(btnTodos,\nidsSeleccionados, totalDisponibles)')
                    s.mensaje('fsl', 'fsl', 'ActualizarResumen()')
        s.asincrono('fsl', 'op', '"Vas a jugar en N sorteos:\nL loterías × T horarios."')

    # pasos 5 y 6: continuar a la carga de jugadas
    with s.llamada('op', 'fsl', 'btnContinuar_Click(sender, e)'):
        with s.fragmento('break', '[falta lotería u horario (6.a)]'):
            s.asincrono('fsl', 'op', '"Elegí al menos una lotería y un horario."')
        with s.crear('fsl', 'fa', '«create» FormApuesta(juego,\nloteriasElegidas, turnosElegidos)'):
            with s.llamada('fa', 'fa', 'DescribirSorteos()'):
                with s.fragmento('opt', '[loterias.Count > 1]'):
                    _leer_loterias(s, 'fa')
            s.mensaje('fa', 'fa', 'CrearColumnas()')
            s.mensaje('fa', 'fa', 'CrearTeclado()')
            s.mensaje('fa', 'fa', 'CrearBotonesRango()')
            s.mensaje('fa', 'fa', 'AplicarTipo(ApuestaBLL.Directa)')
            s.mensaje('fa', 'fa', 'ActualizarTotales()')
    s.destruir('fsl')
    s.asincrono('fa', 'op', 'muestra la carga de jugadas:\nDirecta, a la primera, $ 100')

    # pasos 7 y 8 (se repiten por cada jugada): número, posiciones y agregar al ticket
    with s.fragmento('loop', '[por cada jugada que pide el apostador]'):
        with s.fragmento('opt', '[pide una redoblona (7.a)]'):
            with s.llamada('op', 'fa', 'btnRedoblona_Click(sender, e)'):
                with s.llamada('fa', 'fa', 'AplicarTipo(ApuestaBLL.Redoblona)'):
                    s.mensaje('fa', 'fa', 'ActualizarRangos()')
                    s.mensaje('fa', 'fa', 'ActualizarNumero()')
        with s.fragmento('loop', '[por cada cifra del número]'):
            with s.llamada('op', 'fa', 'Tecla(tecla)'):
                s.mensaje('fa', 'fa', 'ActualizarNumero()')
        with s.fragmento('loop', '[por cada posición que elige]'):
            with s.llamada('op', 'fa', 'BtnRango_Click(sender, e)'):
                s.mensaje('fa', 'fa', 'ActualizarRangos()')
        with s.llamada('op', 'fa', 'btnAgregar_Click(sender, e)'):
            with s.llamada('fa', 'fa', 'Agregar()'):
                with s.fragmento('break', '[numeroActual vacío (7.b)]'):
                    s.mensaje('fa', 'fa', 'MostrarAviso("Ingresá un\nnúmero con el teclado.")')
                with s.fragmento('break', '[ninguna posición elegida (7.b)]'):
                    s.mensaje('fa', 'fa', 'MostrarAviso("Elegí al\nmenos una posición.")')
                with s.alternativa() as a:
                    with a.caso('[tipo == Directa]'):
                        with s.fragmento('loop', '[por cada posición elegida]'):
                            with s.llamada('fa', 'fa', 'AgregarPlantilla(j)'):
                                s.mensaje('fa', 'fa', 'ActualizarTotales()')
                        s.mensaje('fa', 'fa', 'ActualizarNumero()')
                    with a.caso('[redoblona sin 2 cifras (7.b)]'):
                        s.mensaje('fa', 'fa', 'MostrarAviso("En la redoblona\ncada número es de 2 cifras.")')
                    with a.caso('[redoblona, primer número (7.a)]'):
                        s.mensaje('fa', 'fa', 'ActualizarNumero()')
                        s.asincrono('fa', 'op', '"1° número: NN a los X.\nAhora cargá el 2°."')
                    with a.caso('[redoblona, segundo número (7.a)]'):
                        with s.llamada('fa', 'fa', 'AgregarPlantilla(j)'):
                            s.mensaje('fa', 'fa', 'ActualizarTotales()')
                        s.mensaje('fa', 'fa', 'ActualizarNumero()')
        s.asincrono('fa', 'op', 'muestra la jugada en la lista\ny el total del ticket')
        with s.fragmento('opt', '[quita una jugada (8.a)]'):
            with s.llamada('op', 'fa', 'btnQuitar_Click(sender, e)'):
                with s.fragmento('break', '[ninguna jugada seleccionada]'):
                    s.asincrono('fa', 'op', '"Seleccioná en la lista la\njugada que querés quitar."')
                s.mensaje('fa', 'fa', 'ActualizarTotales()')
    s.nota('fa', 'Las teclas del equipo también llaman a Tecla(tecla) (ProcessCmdKey); Enter llama a Agregar(), igual '
                 'que «AGREGAR AL TICKET».', 220)

    # pasos 9 a 16: confirmar la venta, armar y guardar el ticket y mostrarlo
    with s.llamada('op', 'fa', 'btnEnviar_Click(sender, e)'):
        with s.fragmento('break', '[primerNumeroRedoblona != null (9.a)]'):
            s.mensaje('fa', 'fa', 'MostrarAviso("Falta cargar el\n2° número de la redoblona.")')
        s.mensaje('fa', 'sm', 'Instancia.UsuarioActual', ret='vendedor : Usuario')
        with s.llamada('fa', 'ab', 'Vender(juego, loterias, turnos,\nplantillas, vendedor)', ret='a : Apuesta'):
            with s.fragmento('break', '[plantillas vacía (10.a)]'):
                s.respuesta('ab', 'fa', 'Exception: "Cargá al menos una jugada\nantes de confirmar la venta."')
            with s.fragmento('loop', '[por cada plantilla p]'):
                s.mensaje('ab', 'ab', 'ValidarJugada(juego, p)')
            with s.fragmento('break', '[jugada inválida (10.a)]'):
                s.respuesta('ab', 'fa', 'Exception con el motivo (monto, cifras,\nposición, cantidad o rango)')
            with s.fragmento('break', '[sin loterías o sin turnos]'):
                s.respuesta('ab', 'fa', 'Exception: "Elegí al menos una lotería."\no "Elegí al menos un horario."')
            with s.fragmento('loop', '[por cada lotería l y cada turno t]'):
                with s.llamada('ab', 'sb', 'ObtenerParaVender(juego, l, t)', ret='sorteo'):
                    with s.llamada('sb', 'sb', 'ObtenerOCrear(juego, loteria,\nturno, DateTime.Today)'):
                        _buscar_o_crear(s)
                    with s.fragmento('break', '[s.Estado != Abierto (11.a)]'):
                        s.mensaje('sb', 'sb', 'Descripcion(s)')
                        s.respuesta('sb', 'ab', 'Exception: "El sorteo … ya no acepta\napuestas (está …)."')
            s.nota('ab', 'Arma la Apuesta (Guid.NewGuid(), vendedor, DateTime.Now, Vigente) con una Jugada por sorteo '
                         'y plantilla; Total = suma de los montos.', 210)
            _insertar_ticket(s)
            s.mensaje('ab', 'ab', 'CodigoCorto(a)')
            bitacora(s, 'ab', 'vendedor', 'Venta')
        with s.fragmento('break', '[se lanzó una excepción]'):
            s.asincrono('fa', 'op', 'muestra ex.Message con el título\n"No se pudo registrar la venta"')
        _mostrar_ticket(s, 'fa', 'a', 'Venta')
        s.mensaje('fa', 'fa', 'ActualizarTotales()')
    s.asincrono('fa', 'op', 'vacía la lista: listo para otra venta')
    return s


def _insertar_ticket(s):
    """ApuestaDAL guarda el ticket en una transacción (con 'ab' activa)."""
    with s.llamada('ab', 'ad', 'Insertar(a)'):
        s.mensaje('ad', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('ad', 'bd', 'INSERT INTO APUESTAS\n(CodigoTicket, UsuarioId,\nFechaHora, Total, Estado);\n'
                              'SELECT SCOPE_IDENTITY()', ret='a.Id')
        with s.fragmento('loop', '[por cada jugada j de a.Jugadas]'):
            s.mensaje('ad', 'bd', 'INSERT INTO JUGADAS\n(IdApuesta, IdSorteo, Tipo,\nEsAutomatica, Monto, Orden);\n'
                                  'SELECT SCOPE_IDENTITY()', ret='j.Id')
            with s.fragmento('loop', '[por cada número n de j.Numeros]'):
                s.mensaje('ad', 'bd', 'INSERT INTO\nJUGADA_NUMERO (IdJugada,\nNumero, Orden, Rango)')
        s.mensaje('ad', 'bd', 'COMMIT')
    s.nota('ad', 'Si un INSERT falla no se llega al COMMIT: la transacción se revierte y no queda guardado ni el '
                 'ticket ni sus jugadas (13.a).', 210)


# ====================================================================== CU-N02 Vender apuesta de juego de números
def n02_sd():
    s = Secuencia('sd CU-N02 Vender apuesta de juego de números')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fj', ':FormJuego', creado=True)
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('jbs', 'JuegoBLL')
    s.objeto('az', 'Azar')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)

    # pasos 1 y 2: abrir el juego (el tablero se arma con los datos del juego elegido en el menú)
    with s.llamada('op', 'fp', 'BtnJuego_Click(sender, e)'):
        with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
            with s.crear('fp', 'fj', '«create» FormJuego(juego)'):
                s.mensaje('fj', 'fj', 'Formatear(juego.NumeroMin)')
                s.mensaje('fj', 'fj', 'Formatear(juego.NumeroMax)')
                with s.llamada('fj', 'fj', 'CrearTablero()'):
                    with s.fragmento('loop', '[por cada número del rango del juego]'):
                        with s.llamada('fj', 'fj', 'Formatear(numero)'):
                            s.mensaje('fj', 'jbs', 'FormatearNumero(juego, numero)', ret='número con ceros')
                s.mensaje('fj', 'fj', 'CrearColumnas()')
                s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
                s.mensaje('fj', 'fj', 'ActualizarTotales()')
    s.nota('fp', 'El juego del botón lo leyó FormPrincipal al iniciar la sesión (JuegoBLL.ObtenerActivos, '
                 'tabla JUEGOS): FormJuego no vuelve a la base de datos.', 210)
    s.asincrono('fj', 'op', 'muestra el tablero, "Elegí N números\ndel … al …", el precio y el sorteo del día')

    # pasos 3 a 6 (se repiten por cada jugada): elegir los números y agregar la jugada
    with s.fragmento('loop', '[por cada jugada que pide el apostador]'):
        with s.alternativa() as a:
            with a.caso('[elige los números en el tablero]'):
                with s.fragmento('loop', '[por cada número que toca]'):
                    with s.llamada('op', 'fj', 'BtnNumero_Click(sender, e)'):
                        with s.alternativa() as b:
                            with b.caso('[ya eligió todos (3.b)]'):
                                s.mensaje('fj', 'fj', 'ActualizarSeleccion("Ya elegiste los "\n+ juego.CantidadNumeros '
                                                      '+\n" números. Quitá uno\npara cambiarlo.")')
                            with b.caso('[else]'):
                                s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
                    s.asincrono('fj', 'op', '"N de M elegidos" o "Jugada completa"')
            with a.caso('[pide una jugada automática (3.a)]'):
                with s.llamada('op', 'fj', 'btnAutomatica_Click(sender, e)'):
                    with s.llamada('fj', 'ab', 'GenerarNumerosAutomaticos(juego)', ret='List<int>'):
                        with s.fragmento('loop', '[hasta CantidadNumeros distintos]'):
                            s.mensaje('ab', 'az', 'Entre(juego.NumeroMin,\njuego.NumeroMax)', ret='número')
                    s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
                s.asincrono('fj', 'op', 'marca los números elegidos al azar:\n"Jugada automática completa"')
        with s.fragmento('opt', '[presiona «Limpiar» (3.b)]'):
            with s.llamada('op', 'fj', 'btnLimpiar_Click(sender, e)'):
                s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
        with s.llamada('op', 'fj', 'btnAgregar_Click(sender, e)'):
            with s.fragmento('break', '[faltan números (5.a)]'):
                s.mensaje('fj', 'fj', 'ActualizarSeleccion(faltan == 1\n? "Falta elegir 1 número."\n: "Faltan elegir " '
                                      '+ faltan\n+ " números.")')
            with s.fragmento('loop', '[por cada número elegido n]'):
                s.mensaje('fj', 'fj', 'Formatear(n)')
            s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
            s.mensaje('fj', 'fj', 'ActualizarTotales()')
        s.asincrono('fj', 'op', 'muestra la jugada en la lista\n(Manual o Automática) y el total')
        with s.fragmento('opt', '[quita una jugada (6.a)]'):
            with s.llamada('op', 'fj', 'btnQuitar_Click(sender, e)'):
                with s.fragmento('break', '[ninguna jugada seleccionada]'):
                    s.asincrono('fj', 'op', '"Seleccioná en la lista la\njugada que querés quitar."')
                s.mensaje('fj', 'fj', 'ActualizarTotales()')

    # pasos 7 a 14: confirmar la venta, armar y guardar el ticket y mostrarlo
    with s.llamada('op', 'fj', 'btnConfirmar_Click(sender, e)'):
        s.mensaje('fj', 'sm', 'Instancia.UsuarioActual', ret='vendedor : Usuario')
        with s.llamada('fj', 'ab', 'Vender(juego, null, null,\nplantillas, vendedor)', ret='a : Apuesta'):
            with s.fragmento('break', '[plantillas vacía (8.a)]'):
                s.respuesta('ab', 'fj', 'Exception: "Cargá al menos una jugada\nantes de confirmar la venta."')
            with s.fragmento('loop', '[por cada plantilla p]'):
                s.mensaje('ab', 'ab', 'ValidarJugada(juego, p)')
            with s.fragmento('break', '[jugada inválida (8.a)]'):
                s.respuesta('ab', 'fj', 'Exception con el motivo (monto, cifras,\nposición, cantidad o rango)')
            with s.llamada('ab', 'sb', 'ObtenerParaVender(juego, null, null)', ret='sorteo'):
                with s.llamada('sb', 'sb', 'ObtenerOCrear(juego, loteria,\nturno, DateTime.Today)'):
                    _buscar_o_crear(s)
                with s.fragmento('break', '[s.Estado != Abierto (9.a)]'):
                    s.mensaje('sb', 'sb', 'Descripcion(s)')
                    s.respuesta('sb', 'ab', 'Exception: "El sorteo … ya no acepta\napuestas (está …)."')
            s.nota('ab', 'Arma la Apuesta (Guid.NewGuid(), vendedor, DateTime.Now, Vigente) con una Jugada por '
                         'plantilla; cada jugada toma el precio del juego.', 210)
            _insertar_ticket_n02(s)
            s.mensaje('ab', 'ab', 'CodigoCorto(a)')
            bitacora(s, 'ab', 'vendedor', 'Venta')
        with s.fragmento('break', '[se lanzó una excepción]'):
            s.asincrono('fj', 'op', 'muestra ex.Message con el título\n"No se pudo registrar la venta"')
        _mostrar_ticket(s, 'fj', 'a', 'Venta')
        s.mensaje('fj', 'fj', 'ActualizarTotales()')
    s.asincrono('fj', 'op', 'vacía la lista: listo para otra venta')
    return s


def _insertar_ticket_n02(s):
    with s.llamada('ab', 'ad', 'Insertar(a)'):
        s.mensaje('ad', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('ad', 'bd', 'INSERT INTO APUESTAS\n(CodigoTicket, UsuarioId,\nFechaHora, Total, Estado);\n'
                              'SELECT SCOPE_IDENTITY()', ret='a.Id')
        with s.fragmento('loop', '[por cada jugada j de a.Jugadas]'):
            s.mensaje('ad', 'bd', 'INSERT INTO JUGADAS\n(IdApuesta, IdSorteo, Tipo,\nEsAutomatica, Monto, Orden);\n'
                                  'SELECT SCOPE_IDENTITY()', ret='j.Id')
            with s.fragmento('loop', '[por cada número n de j.Numeros]'):
                s.mensaje('ad', 'bd', 'INSERT INTO\nJUGADA_NUMERO (IdJugada,\nNumero, Orden, Rango)')
        s.mensaje('ad', 'bd', 'COMMIT')
    s.nota('ad', 'Si un INSERT falla no se llega al COMMIT: la transacción se revierte y no queda guardado ni el '
                 'ticket ni sus jugadas (11.a).', 210)


# ====================================================================== CU-N03 Imprimir ticket
def n03_sd():
    s = Secuencia('sd CU-N03 Imprimir ticket')
    s.actor('op', 'Operador')
    s.objeto('ft', ':FormTicket')
    s.objeto('pd', ':PrintDocument', creado=True)
    s.objeto('pp', ':PrintPreviewDialog', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.objeto('ab', 'ApuestaBLL')
    with s.llamada('op', 'ft', 'btnImprimir_Click(sender, e)'):
        with s.crear('ft', 'pd', '«create» PrintDocument()'):
            pass
        with s.crear('ft', 'pp', '«create» PrintPreviewDialog()'):
            pass
        with s.llamada('ft', 'pp', 'ShowDialog(this)'):
            with s.llamada('pp', 'pd', 'Print() [vista previa]'):
                with s.llamada('pd', 'ft', 'evento PrintPage(s, ev)'):
                    with s.llamada('ft', 'tr', 'Dibujar(ev.Graphics, apuesta,\nesReimpresion, ev.MarginBounds.Left,\n'
                                               'ev.MarginBounds.Top)', ret='alto'):
                        with s.fragmento('opt', '[reimpresion]'):
                            s.mensaje('tr', 'tr', 'Centrado(g, "***  REIMPRESIÓN  ***",\nfNegrita, tinta, x, cy)')
                        s.mensaje('tr', 'ab', 'CodigoCorto(a)', ret='código corto')
                        with s.fragmento('loop', '[por cada sorteo de las jugadas]'):
                            s.mensaje('tr', 'tr', 'TituloSorteo(s)')
                            with s.fragmento('loop', '[por cada jugada j de ese sorteo]'):
                                with s.llamada('tr', 'tr', 'DibujarJugada(g, j, fNormal, fNumeros,\nfChica, tinta, '
                                                           'suave, x, cy)'):
                                    with s.fragmento('opt', '[j.Sorteo.Juego.UsaRango]'):
                                        s.mensaje('tr', 'ab', 'DescribirRango(n.Rango)',
                                                  ret='"a la primera" o "a los N"')
                        s.mensaje('tr', 'tr', 'CodigoDeBarras(g, a, tinta, x, cy)')
                        s.mensaje('tr', 'ab', 'CodigoCompleto(a)', ret='código completo')
                        s.mensaje('tr', 'ab', 'CodigoCorto(a)', ret='código corto')
                        with s.fragmento('opt', '[a.Estado != Vigente]'):
                            s.mensaje('tr', 'tr', 'Sello(g, a.Estado == Pagada\n? "PAGADO" : "CANCELADO",\ncolor, x, '
                                                  'inicioJugadas,\nfinJugadas)')
            s.asincrono('pp', 'op', 'muestra la vista previa\n"Imprimir ticket XXXXXXXX"')
            with s.fragmento('opt', '[imprime desde la vista previa (si no imprime: 4.a)]'):
                with s.llamada('op', 'pp', 'presiona el botón de imprimir'):
                    with s.llamada('pp', 'pd', 'Print()'):
                        s.mensaje('pd', 'ft', 'evento PrintPage(s, ev)')
            s.asincrono('op', 'pp', 'cierra la vista previa')
        with s.fragmento('break', '[InvalidPrinterException (2.a)]'):
            s.asincrono('ft', 'op', '"No hay ninguna impresora\ninstalada en esta computadora."')
        s.destruir('pp')
        s.destruir('pd')
    s.nota('pd', 'Al imprimir, PrintPage vuelve a dibujar la página igual que en la vista previa. PrintDocument.Print() '
                 'envía la página a la impresora predeterminada de Windows: la impresora no es un actor del sistema, '
                 'es el medio de salida que usa .NET.', 240)
    return s


# ====================================================================== CU-N04 Buscar ticket
def n04_sd():
    s = Secuencia('sd CU-N04 Buscar ticket')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fo', ':FormOperacionTicket', creado=True)
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.bd('bd', BD)

    # pasos 1 y 2: abrir la operación (lista los tickets vendidos hoy)
    _abrir_operacion(s, 'btnPagar_Click(sender, e) /\nbtnCancelar_Click(sender, e) /\nbtnReimprimir_Click(sender, e)',
                     'muestra "Pagar ticket", "Cancelar ticket" o\n"Reimprimir ticket", la búsqueda y "Tickets de hoy"')

    # pasos 3 a 6: buscar el ticket por su código y mostrar su detalle
    with s.llamada('op', 'fo', 'btnBuscar_Click(sender, e) o\ntxtCodigo_KeyDown(sender, e) o\n'
                               'dgvHoy_CellClick(sender, e)'):
        with s.fragmento('opt', '[hizo clic en un ticket de la lista (3.a)]'):
            s.mensaje('fo', 'ab', 'CodigoCompleto(elegido)', ret='txtCodigo.Text')
        with s.llamada('fo', 'fo', 'Buscar()'):
            with s.llamada('fo', 'ab', 'BuscarPorCodigo(txtCodigo.Text)', ret='a : Apuesta'):
                with s.fragmento('break', '[el texto está vacío (4.a)]'):
                    s.respuesta('ab', 'fo', 'Exception: "Ingresá el código\ndel ticket."')
                with s.fragmento('opt', '[no es un GUID completo]'):
                    with s.fragmento('break', '[código inválido (4.b)]'):
                        s.respuesta('ab', 'fo', 'Exception: "El código no es válido. …"')
                    with s.llamada('ab', 'ad', 'BuscarCodigosPorPrefijo(\ncodigoTexto.ToUpperInvariant())',
                                   ret='encontrados'):
                        s.mensaje('ad', 'bd', SQL_PREFIJO, ret='códigos')
                    with s.fragmento('break', '[ninguno (4.c) o varios (4.d)]'):
                        s.respuesta('ab', 'fo', 'Exception: "No existe ningún ticket…"\no "Hay más de un ticket que…"')
                _obtener_por_codigo(s)
                with s.fragmento('break', '[a == null (4.c)]'):
                    s.respuesta('ab', 'fo', 'Exception: "No existe ningún ticket…"')
            with s.fragmento('break', '[excepción (4.a a 4.d)]'):
                s.mensaje('fo', 'fo', 'MostrarTicket(null)')
                s.asincrono('fo', 'op', 'muestra ex.Message en rojo')
            with s.llamada('fo', 'fo', 'MostrarTicket(a)'):
                s.mensaje('fo', 'ab', 'CodigoCorto(a)', ret='código corto')
    s.nota('fo', 'Según el modo, MostrarTicket(a) también calcula los premios y el motivo para pagar (CU-N11) o para '
                 'cancelar (CU-N06).', 220)
    s.asincrono('fo', 'op', 'muestra el detalle del ticket y sus\njugadas con el estado de cada sorteo')
    return s


# ====================================================================== CU-N05 Reimprimir ticket
def n05_sd():
    s = Secuencia('sd CU-N05 Reimprimir ticket')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fo', ':FormOperacionTicket', creado=True)
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)

    # pasos 1 y 2
    _abrir_operacion(s, 'btnReimprimir_Click(sender, e)',
                     'muestra "Reimprimir ticket", la búsqueda\ny "Tickets de hoy"')
    # paso 3 (inclusión) y paso 4
    s.referencia('sd CU-N04 Buscar ticket (inclusión): el operador busca el ticket y Buscar() lo lee y lo muestra con '
                 'MostrarTicket(a) (3.a: no se encuentra)', ['op', 'fo', 'ab', 'ad', 'bd'])
    s.asincrono('fo', 'op', '"La reimpresión sale marcada como\nREIMPRESIÓN." (o "El ticket está …:\nla reimpresión '
                            'sale con ese sello.")')
    # pasos 5 a 9
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        with s.llamada('fo', 'ab', 'RegistrarReimpresion(apuesta, usuario)'):
            s.mensaje('ab', 'ab', 'NombreJuego(a)')
            s.mensaje('ab', 'ab', 'CodigoCorto(a)')
            bitacora(s, 'ab', 'usuario', 'Reimpresion')
        with s.fragmento('break', '[excepción (6.a)]'):
            s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
        _mostrar_ticket(s, 'fo', 'apuesta', 'Reimpresion')
        _releer_ticket(s, 'fo')
    s.asincrono('fo', 'op', 'muestra el detalle actualizado y\nla lista de tickets de hoy')
    return s


# ====================================================================== CU-N06 Cancelar ticket
def n06_sd():
    s = Secuencia('sd CU-N06 Cancelar ticket')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fo', ':FormOperacionTicket', creado=True)
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('sbs', 'SorteoBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)

    # pasos 1 y 2
    _abrir_operacion(s, 'btnCancelar_Click(sender, e)', 'muestra "Cancelar ticket", la búsqueda\ny "Tickets de hoy"')
    # paso 3 (inclusión) y paso 4
    s.referencia('sd CU-N04 Buscar ticket (inclusión): el operador busca el ticket y Buscar() lo lee '
                 '(3.a: no se encuentra)', ['op', 'fo', 'ab', 'ad', 'bd'])
    with s.llamada('fo', 'fo', 'MostrarTicket(a)'):
        s.mensaje('fo', 'ab', 'CodigoCorto(a)', ret='código corto')
        with s.llamada('fo', 'ab', 'MotivoNoAnulable(a)', ret='motivo'):
            with s.fragmento('opt', '[un sorteo no está Abierto]'):
                s.mensaje('ab', 'sbs', 'Descripcion(cerrada.Sorteo)', ret='descripción')
    with s.alternativa() as a:
        with a.caso('[motivo != null (4.a)]'):
            s.asincrono('fo', 'op', 'muestra el motivo en rojo y deja\n«CANCELAR TICKET» deshabilitado')
        with a.caso('[else]'):
            s.asincrono('fo', 'op', '"El ticket se puede cancelar: se le\ndevuelven $ X al cliente."')

    # pasos 5 a 11
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        _confirmar_respuesta(s, 'fo', 'op', '"¿Cancelar el ticket XXXXXXXX por $ X?\nEsta operación no se puede '
                                            'deshacer."')
        with s.fragmento('opt', '[respondió Sí (si no: 7.a)]'):
            with s.llamada('fo', 'ab', 'Anular(apuesta, usuario)'):
                s.mensaje('ab', 'ab', 'MotivoNoAnulable(a)')
                with s.fragmento('break', '[motivo != null]'):
                    s.respuesta('ab', 'fo', 'Exception(motivo)')
                with s.llamada('ab', 'ad', 'CambiarEstado(a.Id, Anulada, Vigente)', ret='true / false'):
                    s.mensaje('ad', 'bd', "UPDATE APUESTAS SET\nEstado = 'Anulada' WHERE\nId = @id AND\nEstado = 'Vigente'", ret='filas afectadas')
                with s.fragmento('break', '[CambiarEstado == false (8.a)]'):
                    s.respuesta('ab', 'fo', 'Exception: "El ticket cambió de estado mientras\nlo consultabas. Buscalo '
                                            'de nuevo."')
                s.mensaje('ab', 'ab', 'NombreJuego(a)')
                s.mensaje('ab', 'ab', 'CodigoCorto(a)')
                bitacora(s, 'ab', 'usuario', 'Cancelacion')
            with s.fragmento('break', '[excepción (8.a)]'):
                s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
            s.asincrono('fo', 'op', '"El ticket XXXXXXXX quedó cancelado.\nDevolvé $ X al cliente."')
            _releer_ticket(s, 'fo')
            # paso 11: solo si se canceló (con No o con un error la pantalla queda como estaba)
            s.asincrono('fo', 'op', 'muestra el ticket en estado Cancelado y\nla lista de tickets de hoy')
    return s


# ====================================================================== CU-N07 Consultar números ganadores
def n07_sd():
    s = Secuencia('sd CU-N07 Consultar números ganadores')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fr', ':FormReporte', creado=True)
    s.objeto('sm', ':SesionManager')
    s.objeto('jb', ':JuegoBLL')
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('tb', ':TurnoBLL')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('jd', ':JuegoDAL')
    s.objeto('ld', ':LoteriaDAL')
    s.objeto('td', ':TurnoDAL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('pgd', ':PagoDAL')
    s.objeto('sd', ':SorteoDAL')
    s.bd('bd', BD)

    # pasos 1 y 2: abrir los reportes (el constructor carga juegos, loterías, turnos, ventas y pagos del día)
    with s.llamada('op', 'fp', 'btnReportes_Click(sender, e)'):
        _construir_reporte(s)
    s.asincrono('fr', 'op', 'muestra «Ventas en línea»')
    # pasos 3 y 4: pestaña «Números ganadores»
    with s.llamada('op', 'fr', 'btnGanadores_Click(sender, e)'):
        with s.llamada('fr', 'fr', 'MostrarVista(VistaReporte.Ganadores)'):
            _ver_sorteo(s, 'op')
    s.asincrono('fr', 'op', 'muestra los filtros (primer juego,\nProvincia, Primera, hoy), el estado\ny la pizarra')
    # pasos 5 a 8: cada cambio de filtro vuelve a leer los sorteos
    with s.fragmento('loop', '[cada vez que cambia un filtro o presiona «Actualizar»]'):
        with s.llamada('op', 'fr', 'cambia el juego, la lotería, el turno o\nla fecha, o presiona «Actualizar»'):
            _ver_sorteo(s, 'op')
        with s.llamada('fr', 'fr', 'DibujarPizarra(sender, e)'):
            with s.alternativa() as a:
                with a.caso('[celdas.Count > 1]'):
                    with s.llamada('fr', 'fr', 'DibujarVarias(g, area, color)'):
                        with s.fragmento('loop', '[por cada sorteo elegido]'):
                            s.mensaje('fr', 'fr', 'Pendiente(celda)')
                with a.caso('[ganadores.Count == 0 (7.a)]'):
                    s.asincrono('fr', 'op', '"Todavía no hay apuestas ni resultado\npara este sorteo." o "El sorteo '
                                            'todavía\nno se realizó."')
                with a.caso('[juegoGanadores.UsaRango]'):
                    s.mensaje('fr', 'fr', 'DibujarQuiniela(g, area, color)')
                with a.caso('[else]'):
                    s.mensaje('fr', 'fr', 'DibujarBolillas(g, area, color)')
        s.asincrono('fr', 'op', 'muestra el estado y la pizarra: una columna por\nsorteo, los 20 números de la '
                                'Quiniela o las bolillas')
    s.nota('fr', 'Solo para el administrador (RN-17; 8.a) habilita «Sortear al azar» (fecha no futura y algún sorteo '
                 'sin sortear), «Cargar resultado» (además, un solo sorteo) y «Cerrar ventas» (algún sorteo Abierto).',
           230)
    return s


# ====================================================================== apertura de los reportes
# CU-N08, CU-N09, CU-N10 y CU-N13 empiezan con la pantalla de reportes ya abierta (precondición). Igual se dibuja
# cómo se abrió, con todo lo que lee de la base de datos, para que cada diagrama muestre la carga de su pantalla.
def _lineas_reportes(s, actor, nombre_actor):
    """Líneas de vida que intervienen al abrir «Reportes»: el constructor de FormReporte lee los juegos, las
    loterías, los turnos, las ventas y los pagos del día."""
    s.actor(actor, nombre_actor)
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fr', ':FormReporte', creado=True)
    s.objeto('sm', ':SesionManager')
    for ident, texto in (('jb', ':JuegoBLL'), ('lb', ':LoteriaBLL'), ('tb', ':TurnoBLL'), ('ab', ':ApuestaBLL'),
                         ('pgb', ':PagoBLL'), ('jd', ':JuegoDAL'), ('ld', ':LoteriaDAL'), ('td', ':TurnoDAL'),
                         ('ad', ':ApuestaDAL'), ('pgd', ':PagoDAL')):
        s.objeto(ident, texto)


def _abrir_reportes(s, actor):
    with s.llamada(actor, 'fp', 'btnReportes_Click(sender, e)'):
        _construir_reporte(s)
    s.asincrono('fr', actor, 'muestra «Ventas en línea»')


def _abrir_pizarra(s, actor, eleccion):
    """El usuario abre «Reportes» y la pestaña «Números ganadores» y elige los sorteos en la pizarra."""
    _abrir_reportes(s, actor)
    with s.llamada(actor, 'fr', 'btnGanadores_Click(sender, e)'):
        with s.llamada('fr', 'fr', 'MostrarVista(VistaReporte.Ganadores)'):
            _ver_sorteo(s, actor)
    s.asincrono('fr', actor, 'muestra los filtros, el estado\ny la pizarra')
    with s.fragmento('loop', '[hasta %s]' % eleccion):
        with s.llamada(actor, 'fr', 'cambia el juego, la lotería, el turno o\nla fecha, o presiona «Actualizar»'):
            s.mensaje('fr', 'fr', 'VerSorteo()')
        s.asincrono('fr', actor, 'muestra el estado y la pizarra')
    s.nota('fr', 'Cada cambio de filtro vuelve a leer los sorteos elegidos y sus números ganadores con VerSorteo(), '
                 'igual que al abrir la pestaña.', 170)


# ====================================================================== CU-N08 Cerrar ventas de sorteos
def n08_sd():
    s = Secuencia('sd CU-N08 Cerrar ventas de sorteos')
    _lineas_reportes(s, 'adm', 'Administrador')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)
    # pasos 1 a 6: abre los reportes, la pestaña «Números ganadores» y elige los sorteos
    _abrir_pizarra(s, 'adm', 'elegir los sorteos')
    with s.llamada('adm', 'fr', 'presiona «Cerrar ventas»'):
        with s.llamada('fr', 'fr', 'CerrarVentas()'):
            with s.fragmento('loop', '[por cada sorteo s Abierto]'):
                s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario')
                with s.llamada('fr', 'sb', 'CerrarVentas(s, usuario)'):
                    with s.fragmento('break', '[s.Estado != Abierto]'):
                        s.respuesta('sb', 'fr', 'Exception: "Solo se pueden cerrar las\nventas de un sorteo abierto."')
                    with s.llamada('sb', 'sd', 'CambiarEstado(s.Id, Cerrado)'):
                        s.mensaje('sd', 'bd', "UPDATE SORTEOS SET\nEstado = 'Cerrado'\nWHERE Id = @id")
                    s.mensaje('sb', 'sb', 'Descripcion(s)')
                    bitacora(s, 'sb', 'usuario', 'Cierre de sorteo')
            with s.fragmento('break', '[excepción (8.a)]'):
                s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Cerrar ventas"')
            _ver_sorteo(s, 'adm')
    s.asincrono('fr', 'adm', 'muestra el estado "Cerrado" y\n«Cerrar ventas» deshabilitado')
    return s


# ====================================================================== CU-N09 Cargar resultado de sorteo
def n09_sd():
    s = Secuencia('sd CU-N09 Cargar resultado de sorteo')
    _lineas_reportes(s, 'adm', 'Administrador')
    s.objeto('fres', ':FormResultado', creado=True)
    s.objeto('sb', ':SorteoBLL')
    s.objeto('jbs', 'JuegoBLL')
    s.objeto('az', 'Azar')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)
    # pasos 1 a 6: abre los reportes, la pestaña «Números ganadores» y elige un único sorteo
    _abrir_pizarra(s, 'adm', 'elegir un único sorteo')

    with s.llamada('adm', 'fr', 'presiona «Cargar resultado»'):
        with s.llamada('fr', 'fr', 'CargarResultado()'):
            # pasos 7 y 8: si el sorteo no existe se crea; se abre la carga
            with s.fragmento('opt', '[el sorteo elegido todavía no existe]'):
                with s.llamada('fr', 'sb', 'ObtenerOCrear(juegoGanadores, c.Loteria,\nc.Turno, dtpGanFecha.Value.Date)',
                               ret='s : Sorteo'):
                    _buscar_o_crear(s)
            with s.fragmento('break', '[se lanzó una excepción]'):
                s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
            with s.crear('fr', 'fres', '«create» FormResultado(s)'):
                s.mensaje('fres', 'sb', 'Descripcion(sorteo)', ret='encabezado')
                with s.fragmento('opt', '[el juego no es la Quiniela]'):
                    s.mensaje('fres', 'jbs', 'FormatearNumero(juego, juego.NumeroMin)', ret='desde')
                    s.mensaje('fres', 'jbs', 'FormatearNumero(juego, juego.NumeroMax)', ret='hasta')
                with s.llamada('fres', 'fres', 'ConstruirCasilleros()'):
                    with s.fragmento('loop', '[por cada puesto i]'):
                        s.mensaje('fres', 'fres', 'CrearCasillero(i + 1, cifras)')
            s.asincrono('fres', 'adm', 'muestra un casillero por puesto (Quiniela: 20\nen dos columnas; los demás '
                                       'juegos: filas de 5)')

            # pasos 9 a 15: la carga queda abierta hasta que se guarda con éxito o se cancela (con No o con un error
            # se vuelve al paso 9)
            with s.fragmento('loop', '[hasta guardar con éxito o presionar «Cancelar»]'):
                # paso 9: cargar los números
                with s.fragmento('loop', '[por cada puesto que completa]'):
                    s.mensaje('adm', 'fres', 'Casillero_KeyPress(sender, e) /\nCasillero_KeyDown(sender, e)')
                s.nota('fres', 'KeyPress solo deja escribir dígitos; con Enter, KeyDown pasa al puesto siguiente.', 190)
                with s.fragmento('opt', '[presiona «Completar al azar» (9.a)]'):
                    with s.llamada('adm', 'fres', 'btnAzar_Click(sender, e)'):
                        s.mensaje('fres', 'sb', 'CompletarResultado(sorteo.Juego, Textos())', ret='completo')
                        with s.fragmento('break', '[número inválido o repetido (14.a)]'):
                            s.asincrono('fres', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
                    s.asincrono('fres', 'adm', 'completa los casilleros vacíos (sin guardar)')
                s.nota('sb', 'CompletarResultado valida y completa los números como se detalla en el paso 14.', 180)
                with s.fragmento('opt', '[presiona «Limpiar» (9.b)]'):
                    s.mensaje('adm', 'fres', 'btnLimpiar_Click(sender, e)')

                with s.alternativa() as a:
                    # pasos 10 a 15: confirmar y guardar el resultado
                    with a.caso('[presiona «GUARDAR RESULTADO»]'):
                        with s.llamada('adm', 'fres', 'btnGuardar_Click(sender, e)'):
                            s.mensaje('fres', 'fres', 'Textos()')
                            s.asincrono('fres', 'adm', '"Los N puestos vacíos se completan al azar.\nUna vez guardado, '
                                                       'el sorteo queda como\nsorteado y no se puede modificar.\n'
                                                       '¿Guardar el resultado?"')
                            s.asincrono('adm', 'fres', 'Sí / No')
                            with s.fragmento('opt', '[respondió Sí (si no: 12.a, vuelve al paso 9)]'):
                                s.mensaje('fres', 'sm', 'Instancia.UsuarioActual', ret='usuario')
                                with s.llamada('fres', 'sb', 'CargarResultado(sorteo, textos, usuario)',
                                               ret='List<NumeroGanador>'):
                                    _guardar_resultado(s, 'fres')
                                with s.fragmento('break', '[excepción (13.a, 14.a)]'):
                                    s.asincrono('fres', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
                                    s.nota('fres', 'La carga sigue abierta con los números escritos: vuelve al '
                                                   'paso 9.', 190)
                                s.nota('fres', 'DialogResult = OK y Close(): la carga se cierra (fin del loop).', 190)
                    # 9.b: cancelar
                    with a.caso('[presiona «Cancelar» o Esc (9.b)]'):
                        s.asincrono('adm', 'fres', 'presiona «Cancelar»')
                        s.nota('fres', 'DialogResult = Cancel: la carga se cierra sin guardar (fin del loop).', 190)
            s.destruir('fres')
            # paso 16 (y después de cancelar): actualizar la pizarra
            _ver_sorteo(s, 'adm')
            s.asincrono('fr', 'adm', 'muestra la pizarra actualizada (con el\nresultado y el estado "Sorteado" si se '
                                     'guardó)')
    return s


# ====================================================================== CU-N10 Sortear al azar
def n10_sd():
    s = Secuencia('sd CU-N10 Sortear al azar')
    _lineas_reportes(s, 'adm', 'Administrador')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('jbs', 'JuegoBLL')
    s.objeto('az', 'Azar')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)
    # pasos 1 a 6: abre los reportes, la pestaña «Números ganadores» y elige los sorteos
    _abrir_pizarra(s, 'adm', 'elegir los sorteos')

    with s.llamada('adm', 'fr', 'presiona «Sortear al azar»'):
        with s.llamada('fr', 'fr', 'RealizarSorteo()'):
            # pasos 7 y 8: pedir confirmación nombrando los sorteos pendientes
            with s.fragmento('loop', '[por cada sorteo elegido c]'):
                s.mensaje('fr', 'fr', 'Pendiente(c)')
            with s.fragmento('loop', '[por cada sorteo pendiente c]'):
                s.mensaje('fr', 'sb', 'Descripcion(…) o DescripcionCorta(…)', ret='descripción')
            s.asincrono('fr', 'adm', '"¿Sortear al azar los N sorteos que faltan?\n• … Se generan los números '
                                     'ganadores\ny dejan de aceptar apuestas."')
            s.asincrono('adm', 'fr', 'Sí / No')
            # pasos 9 y 10
            with s.fragmento('opt', '[respondió Sí (si no: 9.a)]'):
                with s.fragmento('loop', '[por cada sorteo pendiente c]'):
                    with s.fragmento('opt', '[c.Sorteo == null]'):
                        with s.llamada('fr', 'sb', 'ObtenerOCrear(juegoGanadores,\nc.Loteria, c.Turno, fecha)',
                                       ret='s'):
                            _buscar_o_crear(s)
                    s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario')
                    with s.llamada('fr', 'sb', 'RealizarSorteo(s, usuario)', ret='List<NumeroGanador>'):
                        with s.llamada('sb', 'sb', 'CargarResultado(s, new string[\ns.Juego.CantidadSorteados], '
                                                   'usuario)'):
                            _guardar_resultado(s, 'fr')
                with s.fragmento('break', '[excepción (10.a)]'):
                    s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Sortear al azar"')
                    s.mensaje('fr', 'fr', 'VerSorteo()')
                # paso 11: solo si confirmó (con No no se actualiza nada)
                _ver_sorteo(s, 'adm')
                s.asincrono('fr', 'adm', 'muestra la pizarra con los números\nganadores y el estado "Sorteado"')
    return s


# ====================================================================== CU-N11 Pagar ticket premiado
def n11_sd():
    s = Secuencia('sd CU-N11 Pagar ticket premiado')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fo', ':FormOperacionTicket', creado=True)
    s.objeto('ff', ':FormFelicitaciones', creado=True)
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('pb', ':PremioBLL')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('bb', ':BitacoraBLL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('sd', ':SorteoDAL')
    s.objeto('pgd', ':PagoDAL')
    s.objeto('bdal', ':BitacoraDAL')
    s.bd('bd', BD)

    # pasos 1 y 2
    _abrir_operacion(s, 'btnPagar_Click(sender, e)', 'muestra "Pagar ticket", la búsqueda\ny "Tickets de hoy"')
    # paso 3 (inclusión)
    s.referencia('sd CU-N04 Buscar ticket (inclusión): el operador busca el ticket y Buscar() lo lee '
                 '(3.a: no se encuentra)', ['op', 'fo', 'ab', 'ad', 'bd'])
    # pasos 4 y 5: calcular los premios y verificar si se puede pagar
    with s.llamada('fo', 'fo', 'MostrarTicket(a)'):
        with s.llamada('fo', 'pb', 'Calcular(a)', ret='premios'):
            with s.fragmento('loop', '[por cada jugada j de a.Jugadas]'):
                with s.alternativa() as a:
                    with a.caso('[j.Sorteo.Estado != Sorteado]'):
                        s.nota('pb', 'Detalle = "Todavía no se sorteó"', 170)
                    with a.caso('[else]'):
                        with s.fragmento('opt', '[es la primera jugada de ese sorteo]'):
                            with s.llamada('pb', 'sb', 'ObtenerNumerosGanadores(j.Sorteo)', ret='ganadores'):
                                with s.llamada('sb', 'sd', 'ObtenerNumerosGanadores(s.Id)',
                                               ret='List<NumeroGanador>'):
                                    s.mensaje('sd', 'bd', SQL_NUMEROS, ret='filas')
                        with s.alternativa() as b:
                            with b.caso('[j.Sorteo.Juego.UsaRango]'):
                                with s.llamada('pb', 'pb', 'CalcularQuiniela(j, ganadores, p)'):
                                    with s.fragmento('loop', '[por cada número de la jugada]'):
                                        s.mensaje('pb', 'pb', 'Buscar(n, ganadores)')
                            with b.caso('[else]'):
                                s.mensaje('pb', 'pb', 'CalcularPorAciertos(j, ganadores, p)')
        with s.llamada('fo', 'pgb', 'MotivoNoPagable(a, premios)', ret='motivo'):
            with s.fragmento('opt', '[hay sorteos sin realizar]'):
                s.mensaje('pgb', 'sb', 'Descripcion(s) o DescripcionCorta(s)', ret='descripción')
    s.nota('fo', 'Cada fila de jugadas muestra el resultado (Premio.Detalle) y el premio (Premio.Importe).', 200)
    with s.alternativa() as a:
        with a.caso('[motivo != null (5.a)]'):
            s.asincrono('fo', 'op', 'muestra el motivo en rojo y deja\nel botón deshabilitado')
        with a.caso('[else]'):
            s.asincrono('fo', 'op', '"Ticket premiado: se le pagan $ X al\ncliente." y habilita «PAGAR $ X»')

    # pasos 6 a 13
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        _confirmar_respuesta(s, 'fo', 'op', '"¿Pagar $ X del ticket XXXXXXXX?"')
        with s.fragmento('opt', '[respondió Sí (si no: 8.a)]'):
            with s.llamada('fo', 'pgb', 'Pagar(apuesta, usuario)', ret='pagado'):
                s.mensaje('pgb', 'pb', 'Calcular(a)', ret='premios')
                s.nota('pb', 'Mismo cálculo que en el paso 4.', 150)
                s.mensaje('pgb', 'pgb', 'MotivoNoPagable(a, premios)')
                with s.fragmento('break', '[motivo != null]'):
                    s.respuesta('pgb', 'fo', 'Exception(motivo)')
                s.nota('pgb', 'Arma el Pago: IdApuesta = a.Id, UsuarioId = cajero.Id, FechaHora = DateTime.Now y '
                              'Monto = suma de los premios.', 200)
                with s.llamada('pgb', 'pgd', 'Registrar(pago, Pagada, Vigente)', ret='true / false'):
                    s.mensaje('pgd', 'bd', 'BEGIN TRANSACTION')
                    s.mensaje('pgd', 'bd', "UPDATE APUESTAS SET\nEstado = 'Pagada' WHERE\nId = @id AND\nEstado = 'Vigente'", ret='filas afectadas')
                    with s.fragmento('break', '[no cambió ninguna fila]'):
                        s.mensaje('pgd', 'bd', 'ROLLBACK')
                        s.respuesta('pgd', 'pgb', 'false')
                    s.mensaje('pgd', 'bd', 'INSERT INTO PAGOS\n(IdApuesta, UsuarioId,\nFechaHora, Monto);\n'
                                           'SELECT SCOPE_IDENTITY()', ret='p.Id')
                    s.mensaje('pgd', 'bd', 'COMMIT')
                with s.fragmento('break', '[Registrar == false (10.a)]'):
                    s.respuesta('pgb', 'fo', 'Exception: "El ticket ya no está vigente.\nBuscalo de nuevo."')
                bitacora(s, 'pgb', 'cajero', 'Pago')
            with s.fragmento('break', '[excepción (10.a)]'):
                s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
            # pasos 11 y 12: cartel de felicitaciones
            with s.crear('fo', 'ff', '«create» FormFelicitaciones(\napuesta, pagado, premios)'):
                s.mensaje('ff', 'ab', 'CodigoCorto(apuesta)', ret='código corto')
            s.mensaje('ff', 'ff', 'OnShown(e)')
            with s.fragmento('loop', '[mientras dura la animación]'):
                with s.llamada('ff', 'ff', 'timer_Tick(sender, e)'):
                    s.mensaje('ff', 'ff', 'OnPaint(e)')
            s.asincrono('ff', 'op', '"¡Felicitaciones!" con el código, el\nimporte ganado y las jugadas ganadoras')
            s.asincrono('op', 'ff', 'presiona «ACEPTAR»')
            s.destruir('ff')
            # paso 13: solo si se pagó (con No o con un error la pantalla queda como estaba)
            _releer_ticket(s, 'fo')
            s.asincrono('fo', 'op', 'muestra el ticket en estado Pagado y\nla lista de tickets de hoy')
    return s


# ====================================================================== CU-N12 Consultar reporte de ventas
def n12_sd():
    s = Secuencia('sd CU-N12 Consultar reporte de ventas')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fr', ':FormReporte', creado=True)
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.objeto('sm', ':SesionManager')
    s.objeto('jb', ':JuegoBLL')
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('tb', ':TurnoBLL')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('jd', ':JuegoDAL')
    s.objeto('ld', ':LoteriaDAL')
    s.objeto('td', ':TurnoDAL')
    s.objeto('ad', ':ApuestaDAL')
    s.objeto('pgd', ':PagoDAL')
    s.bd('bd', BD)

    # pasos 1 y 2: abrir los reportes (el constructor lista las ventas de hoy)
    with s.llamada('op', 'fp', 'btnReportes_Click(sender, e)'):
        _construir_reporte(s)
    s.nota('fp', '«Ventas en Linea» (btnVentas_Click) y «Reporte Semanal» (btnReporte_Click), del Administrador, '
                 'crean FormReporte(VistaReporte.Ventas, DateTime.Today) y FormReporte(VistaReporte.Ventas, '
                 'DateTime.Today.AddDays(-6)) (flujo 1.a).', 230)
    s.asincrono('fr', 'op', 'muestra «Ventas en línea» con las\nventas de hoy y las tarjetas')

    # pasos 3 a 6: listar las ventas del período
    with s.fragmento('loop', '[cada vez que presiona «Buscar»]'):
        with s.llamada('op', 'fr', 'presiona «Buscar»'):
            with s.llamada('fr', 'fr', 'CargarVentas()'):
                _listar_ventas(s, 'fr', 'dtpVentasDesde.Value,\ndtpVentasHasta.Value, juego', ret='ventas')
                with s.fragmento('break', '[excepción (4.a)]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el\ntítulo "Ventas en línea"')
                with s.fragmento('loop', '[por cada ticket a de ventas]'):
                    s.mensaje('fr', 'ab', 'CodigoCorto(a)', ret='código corto')
        s.asincrono('fr', 'op', 'muestra la lista y las tarjetas')
    s.nota('fr', 'Tarjetas: Tickets vendidos y Total vendido sin los tickets en estado Anulada (RN-15); Pagados = '
                 'estado Pagada; Cancelados = estado Anulada. juego = null con "Todos los juegos".', 220)

    # flujo 6.a: ver un ticket del reporte
    with s.fragmento('opt', '[doble clic en un ticket (6.a)]'):
        with s.llamada('op', 'fr', 'doble clic en un ticket\n(CellDoubleClick)'):
            with s.llamada('fr', 'fr', 'AbrirTicket(dgvVentas, e.RowIndex)'):
                with s.llamada('fr', 'ab', 'BuscarPorCodigo(\n((Guid)grilla.Rows[fila].Tag).ToString())', ret='a'):
                    _obtener_por_codigo(s)
                with s.fragmento('break', '[se lanzó una excepción]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el título "Ticket"')
                _mostrar_ticket(s, 'fr', 'a', 'Consulta')
    return s


# ====================================================================== CU-N13 Consultar reporte de pagos
def n13_sd():
    s = Secuencia('sd CU-N13 Consultar reporte de pagos')
    _lineas_reportes(s, 'op', 'Operador')
    s.objeto('ft', ':FormTicket', creado=True)
    s.objeto('tr', 'TicketRenderer')
    s.bd('bd', BD)
    # pasos 1 y 2: abre los reportes; el constructor ya lista los pagos del día con CargarPagos()
    _abrir_reportes(s, 'op')

    # pasos 3 y 4
    with s.llamada('op', 'fr', 'btnPagos_Click(sender, e)'):
        s.mensaje('fr', 'fr', 'MostrarVista(VistaReporte.Pagos)')
    s.asincrono('fr', 'op', 'muestra «Pagos en línea» con los pagos de hoy')
    # pasos 5 a 8
    with s.fragmento('loop', '[cada vez que presiona «Buscar»]'):
        with s.llamada('op', 'fr', 'presiona «Buscar»'):
            with s.llamada('fr', 'fr', 'CargarPagos()'):
                _listar_pagos(s)
                with s.fragmento('break', '[excepción (6.a)]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el\ntítulo "Pagos en línea"')
        s.asincrono('fr', 'op', 'muestra la lista y las tarjetas')
    s.nota('fr', 'Tarjetas: Pagos realizados (cantidad) y Total pagado (suma de los montos).', 200)

    # flujo 8.a: ver el ticket de un pago
    with s.fragmento('opt', '[doble clic en un pago (8.a)]'):
        with s.llamada('op', 'fr', 'doble clic en un pago\n(CellDoubleClick)'):
            with s.llamada('fr', 'fr', 'AbrirTicket(dgvPagos, e.RowIndex)'):
                with s.llamada('fr', 'ab', 'BuscarPorCodigo(\n((Guid)grilla.Rows[fila].Tag).ToString())', ret='a'):
                    _obtener_por_codigo(s)
                with s.fragmento('break', '[se lanzó una excepción]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el título "Ticket"')
                _mostrar_ticket(s, 'fr', 'a', 'Consulta')
    return s


TODOS = dict(n01=n01_sd, n02=n02_sd, n03=n03_sd, n04=n04_sd, n05=n05_sd, n06=n06_sd, n07=n07_sd, n08=n08_sd,
             n09=n09_sd, n10=n10_sd, n11=n11_sd, n12=n12_sd, n13=n13_sd)
