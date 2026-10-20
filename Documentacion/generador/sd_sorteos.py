# -*- coding: utf-8 -*-
"""Diagramas de secuencia de PN02 Gestión de sorteos y resultados (CU-N07 a CU-N10), según el código."""
from secuencia import Secuencia
from sd_comun import (BD, bitacora, lineas_bitacora, sd_abrir_reportes, sd_construir_reporte, sd_leer_sorteo,
                      sd_resultado_bll, sd_resultado_dal, sd_obtener_o_crear)


def _ver_sorteo(s, origen='fr'):
    with s.llamada(origen, origen, 'VerSorteo()'):
        s.referencia('CU-N07: vuelve a leer los sorteos y redibuja la pizarra', [origen])


# ================================================================== CU-N07 Consultar numeros ganadores
def _cierre_ganadores(s):
    with s.llamada('op', 'fr', 'btnGanadores_Click(sender, e)'):
        with s.llamada('fr', 'fr', 'MostrarVista(VistaReporte.Ganadores)'):
            with s.llamada('fr', 'fr', 'VerSorteo()'):
                s.referencia('sd CU-N07 (3)', ['fr'])
    s.asincrono('fr', 'op', 'muestra los filtros (primer juego,\nProvincia, Primera, hoy), el estado\ny la pizarra')


def n07_sd_abrir():
    return sd_abrir_reportes('sd CU-N07 (1) Abrir los reportes y la pestaña «Números ganadores»',
                             'sd CU-N07 (2) Construir la pantalla de reportes', _cierre_ganadores)


def n07_sd_construir():
    return sd_construir_reporte('sd CU-N07 (2) Construir la pantalla de reportes')


def n07_sd_ver():
    s = Secuencia('sd CU-N07 (3) Ver los sorteos elegidos')
    s.actor('op', 'Operador')
    s.objeto('fr', ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    with s.llamada('op', 'fr', 'cambia el juego, la lotería, el turno o\nla fecha, o presiona «Actualizar»'):
        with s.llamada('fr', 'fr', 'VerSorteo()'):
            with s.alternativa() as a:
                with a.caso('[la lectura no falla]'):
                    s.mensaje('fr', 'fr', 'LoteriasElegidas()')
                    s.mensaje('fr', 'fr', 'TurnosElegidos()')
                    with s.fragmento('loop', '[por cada lotería l y turno t]'):
                        s.mensaje('fr', 'sb', 'Buscar(juegoGanadores, l, t, fecha)', ret='c.Sorteo')
                        with s.fragmento('opt', '[c.Sorteo != null y está Sorteado]'):
                            s.mensaje('fr', 'sb', 'ObtenerNumerosGanadores(c.Sorteo)', ret='c.Numeros')
                    with s.alternativa() as b:
                        with b.caso('[un solo sorteo]'):
                            with s.fragmento('opt', '[el sorteo todavía no existe]'):
                                s.mensaje('fr', 'fr', 'Virtual(celdas[0], fecha)')
                            s.mensaje('fr', 'sb', 'Descripcion(descripto)', ret='texto del estado')
                        with b.caso('[varios sorteos]'):
                            with s.fragmento('loop', '[por cada sorteo elegido c]'):
                                s.mensaje('fr', 'fr', 'Pendiente(c)')
                with a.caso('[se lanzó una excepción]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el título\n"Números ganadores"')
            s.mensaje('fr', 'fr', 'DibujarPizarra(sender, e)\n[pnlPizarra.Invalidate()]')
    s.nota('fr', 'Para el administrador habilita «Sortear al azar» (fecha no futura y algún sorteo sin sortear), '
                 '«Cargar resultado» (además, un solo sorteo) y «Cerrar ventas» (algún sorteo Abierto).', 220)
    s.asincrono('fr', 'op', 'muestra el estado y la pizarra')
    return s


def n07_sd_leer():
    return sd_leer_sorteo('sd CU-N07 (4) Leer un sorteo y sus números ganadores')


def n07_sd_pizarra():
    s = Secuencia('sd CU-N07 (5) Dibujar la pizarra')
    s.actor('op', 'Operador')
    s.objeto('fr', ':FormReporte')
    with s.llamada('fr', 'fr', 'DibujarPizarra(sender, e)'):
        with s.alternativa() as a:
            with a.caso('[celdas.Count > 1]'):
                with s.llamada('fr', 'fr', 'DibujarVarias(g, area, color)'):
                    with s.fragmento('loop', '[por cada sorteo elegido]'):
                        s.mensaje('fr', 'fr', 'Pendiente(celda)')
            with a.caso('[ganadores.Count == 0 (5.a)]'):
                s.asincrono('fr', 'op', '"Todavía no hay apuestas ni resultado\npara este sorteo." o "El sorteo '
                                        'todavía\nno se realizó."')
            with a.caso('[juegoGanadores.UsaRango]'):
                s.mensaje('fr', 'fr', 'DibujarQuiniela(g, area, color)')
            with a.caso('[else]'):
                s.mensaje('fr', 'fr', 'DibujarBolillas(g, area, color)')
    s.asincrono('fr', 'op', 'muestra la pizarra: una columna por sorteo,\nlos 20 números de la Quiniela o las bolillas')
    return s


# ================================================================== CU-N08 Cerrar ventas de sorteos
def n08_sd_cerrar():
    s = Secuencia('sd CU-N08 (1) Cerrar las ventas de los sorteos elegidos')
    s.actor('adm', 'Administrador')
    s.objeto('fr', ':FormReporte')
    s.objeto('sm', ':SesionManager')
    s.objeto('sb', ':SorteoBLL')
    with s.llamada('adm', 'fr', 'presiona «Cerrar ventas»'):
        with s.llamada('fr', 'fr', 'CerrarVentas()'):
            with s.fragmento('loop', '[por cada sorteo s Abierto]'):
                s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario')
                with s.llamada('fr', 'sb', 'CerrarVentas(s, usuario)'):
                    s.referencia('sd CU-N08 (2)', ['sb'])
            with s.fragmento('break', '[excepción (2.a)]'):
                s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Cerrar ventas"')
            _ver_sorteo(s)
    s.asincrono('fr', 'adm', 'muestra el estado "Cerrado" y\n«Cerrar ventas» deshabilitado')
    return s


def n08_sd_bll():
    s = Secuencia('sd CU-N08 (2) Cerrar las ventas de un sorteo')
    s.objeto('fr', ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('sd', ':SorteoDAL')
    lineas_bitacora(s)
    with s.llamada('fr', 'sb', 'CerrarVentas(s, usuario)'):
        with s.fragmento('break', '[s.Estado != Abierto]'):
            s.respuesta('sb', 'fr', 'Exception: "Solo se pueden cerrar las\nventas de un sorteo abierto."')
        with s.llamada('sb', 'sd', 'CambiarEstado(s.Id, Cerrado)'):
            s.mensaje('sd', 'bd', "UPDATE SORTEOS SET\nEstado = 'Cerrado'\nWHERE Id = @id")
        s.mensaje('sb', 'sb', 'Descripcion(s)')
        bitacora(s, 'sb', 'usuario', 'Cierre de sorteo')
    return s


# ================================================================== CU-N09 Cargar resultado de sorteo
def n09_sd_abrir():
    s = Secuencia('sd CU-N09 (1) Abrir la carga del resultado')
    s.actor('adm', 'Administrador')
    s.objeto('fr', ':FormReporte')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('fres', ':FormResultado', creado=True)
    with s.llamada('adm', 'fr', 'presiona «Cargar resultado»'):
        with s.llamada('fr', 'fr', 'CargarResultado()'):
            with s.fragmento('opt', '[el sorteo elegido todavía no existe]'):
                with s.llamada('fr', 'sb', 'ObtenerOCrear(juegoGanadores, c.Loteria,\nc.Turno, dtpGanFecha.Value.Date)',
                               ret='s : Sorteo'):
                    s.referencia('sd CU-N09 (8)', ['sb'])
            with s.fragmento('break', '[se lanzó una excepción]'):
                s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
            with s.crear('fr', 'fres', '«create» FormResultado(s)'):
                s.referencia('sd CU-N09 (2)', ['fres'])
            s.asincrono('fres', 'adm', 'muestra un casillero por puesto')
            s.referencia('sd CU-N09 (3) y (4): cargar los números y guardar el resultado', ['adm', 'fr', 'sb', 'fres'])
            s.destruir('fres')
            _ver_sorteo(s)
    return s


def n09_sd_construir():
    s = Secuencia('sd CU-N09 (2) Construir la pantalla de carga')
    s.objeto('fr', ':FormReporte')
    s.objeto('fres', ':FormResultado', creado=True)
    s.objeto('sbs', 'SorteoBLL')
    s.objeto('jb', 'JuegoBLL')
    with s.crear('fr', 'fres', '«create» FormResultado(s)'):
        s.mensaje('fres', 'sbs', 'Descripcion(sorteo)', ret='encabezado')
        with s.fragmento('opt', '[el juego no es la Quiniela]'):
            s.mensaje('fres', 'jb', 'FormatearNumero(juego, juego.NumeroMin)', ret='desde')
            s.mensaje('fres', 'jb', 'FormatearNumero(juego, juego.NumeroMax)', ret='hasta')
        with s.llamada('fres', 'fres', 'ConstruirCasilleros()'):
            with s.fragmento('loop', '[por cada puesto i]'):
                s.mensaje('fres', 'fres', 'CrearCasillero(i + 1, cifras)')
    s.nota('fres', 'Quiniela: 20 puestos en dos columnas (1° a 10° y 11° a 20°); los demás juegos: filas de 5.', 200)
    return s


def n09_sd_cargar():
    s = Secuencia('sd CU-N09 (3) Cargar los números')
    s.actor('adm', 'Administrador')
    s.objeto('fres', ':FormResultado')
    s.objeto('sb', ':SorteoBLL')
    with s.fragmento('loop', '[por cada puesto que completa]'):
        s.mensaje('adm', 'fres', 'Casillero_KeyPress(sender, e) /\nCasillero_KeyDown(sender, e)')
    s.nota('fres', 'KeyPress solo deja escribir dígitos; con Enter, KeyDown pasa al puesto siguiente.', 190)
    with s.fragmento('opt', '[presiona «Completar al azar» (3.a)]'):
        with s.llamada('adm', 'fres', 'btnAzar_Click(sender, e)'):
            with s.llamada('fres', 'sb', 'CompletarResultado(sorteo.Juego, Textos())', ret='completo'):
                s.referencia('sd CU-N09 (7)', ['sb'])
            with s.fragmento('break', '[número inválido o repetido (8.a)]'):
                s.asincrono('fres', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
            s.asincrono('fres', 'adm', 'completa los casilleros vacíos (sin guardar)')
    with s.fragmento('opt', '[presiona «Limpiar» (3.b)]'):
        s.mensaje('adm', 'fres', 'btnLimpiar_Click(sender, e)')
    return s


def n09_sd_confirmar():
    s = Secuencia('sd CU-N09 (4) Confirmar y guardar el resultado')
    s.actor('adm', 'Administrador')
    s.objeto('fres', ':FormResultado')
    s.objeto('sm', ':SesionManager')
    s.objeto('sb', ':SorteoBLL')
    with s.llamada('adm', 'fres', 'btnGuardar_Click(sender, e)'):
        s.mensaje('fres', 'fres', 'Textos()')
        s.asincrono('fres', 'adm', '"Los N puestos vacíos se completan al azar.\nUna vez guardado, el sorteo queda como\n'
                                   'sorteado y no se puede modificar.\n¿Guardar el resultado?"')
        s.asincrono('adm', 'fres', 'Sí / No')
        with s.fragmento('opt', '[respondió Sí]'):
            s.mensaje('fres', 'sm', 'Instancia.UsuarioActual', ret='usuario')
            with s.llamada('fres', 'sb', 'CargarResultado(sorteo, textos, usuario)', ret='List<NumeroGanador>'):
                s.referencia('sd CU-N09 (5)', ['sb'])
            with s.fragmento('break', '[excepción (7.a, 8.a)]'):
                s.asincrono('fres', 'adm', 'muestra ex.Message con el título\n"Cargar resultado"')
            s.nota('fres', 'DialogResult = OK y Close(): la carga se cierra y vuelve a la pizarra.', 190)
    return s


def n09_sd_guardar():
    return sd_resultado_bll('sd CU-N09 (5) Validar, completar y guardar el resultado', 'carga', 'sd CU-N09 (6)')


def n09_sd_dal():
    return sd_resultado_dal('sd CU-N09 (6) Guardar el resultado y registrarlo en la bitácora')


def n09_sd_completar():
    s = Secuencia('sd CU-N09 (7) Validar y completar los números (CompletarResultado)')
    s.objeto('fres', ':FormResultado')
    s.objeto('sb', ':SorteoBLL')
    s.objeto('az', 'Azar')
    s.objeto('jb', 'JuegoBLL')
    with s.llamada('fres', 'sb', 'CompletarResultado(sorteo.Juego, Textos())', ret='List<NumeroGanador>'):
        with s.fragmento('break', '[cargados.Count != juego.CantidadSorteados]'):
            s.respuesta('sb', 'fres', 'Exception: "En … salen N números por sorteo."')
        with s.fragmento('break', '[número inválido o repetido (8.a)]'):
            s.respuesta('sb', 'fres', 'Exception: "El número del puesto N (X) no es\nválido: van del … al …" o "El '
                                      'número …\nestá repetido."')
        s.nota('sb', 'En la Quiniela guarda las dos últimas cifras de los números cargados a mano (terminaciones).',
               200)
        with s.fragmento('loop', '[por cada puesto vacío]'):
            with s.fragmento('loop', '[mientras n esté usado o repita terminación]'):
                s.mensaje('sb', 'az', 'Entre(juego.NumeroMin,\njuego.NumeroMax)', ret='n')
        with s.fragmento('loop', '[por cada puesto i, del 1 al último]'):
            s.mensaje('sb', 'jb', 'FormatearNumero(juego, valores[i].Value)', ret='número con ceros')
    s.nota('sb', 'CargarResultado (sd CU-N09 (5)) llama al mismo método antes de guardar.', 190)
    return s


def n09_sd_sorteo():
    return sd_obtener_o_crear('sd CU-N09 (8) Obtener o crear el sorteo elegido',
                              'ObtenerOCrear(juegoGanadores, c.Loteria,\nc.Turno, dtpGanFecha.Value.Date)')


# ================================================================== CU-N10 Sortear al azar
def n10_sd_sortear():
    s = Secuencia('sd CU-N10 (1) Sortear al azar los sorteos pendientes')
    s.actor('adm', 'Administrador')
    s.objeto('fr', ':FormReporte')
    s.objeto('sm', ':SesionManager')
    s.objeto('sb', ':SorteoBLL')
    with s.llamada('adm', 'fr', 'presiona «Sortear al azar»'):
        with s.llamada('fr', 'fr', 'RealizarSorteo()'):
            with s.fragmento('loop', '[por cada sorteo elegido c]'):
                s.mensaje('fr', 'fr', 'Pendiente(c)')
            with s.fragmento('loop', '[por cada sorteo pendiente c]'):
                s.mensaje('fr', 'sb', 'Descripcion(…) o DescripcionCorta(…)', ret='descripción')
            s.asincrono('fr', 'adm', '"¿Sortear al azar los N sorteos que faltan?\n• … Se generan los números ganadores\n'
                                     'y dejan de aceptar apuestas."')
            s.asincrono('adm', 'fr', 'Sí / No')
            with s.fragmento('opt', '[respondió Sí]'):
                with s.fragmento('loop', '[por cada sorteo pendiente c]'):
                    with s.fragmento('opt', '[c.Sorteo == null]'):
                        s.mensaje('fr', 'sb', 'ObtenerOCrear(juegoGanadores,\nc.Loteria, c.Turno, fecha)', ret='s')
                    s.mensaje('fr', 'sm', 'Instancia.UsuarioActual', ret='usuario')
                    with s.llamada('fr', 'sb', 'RealizarSorteo(s, usuario)', ret='List<NumeroGanador>'):
                        s.referencia('sd CU-N10 (2)', ['sb'])
                with s.fragmento('break', '[excepción (4.a)]'):
                    s.asincrono('fr', 'adm', 'muestra ex.Message con el título\n"Sortear al azar"')
                    s.mensaje('fr', 'fr', 'VerSorteo()')
                _ver_sorteo(s)
    return s


def n10_sd_resultado():
    return sd_resultado_bll('sd CU-N10 (2) Generar y guardar el resultado al azar', 'azar', 'sd CU-N10 (3)')


def n10_sd_dal():
    return sd_resultado_dal('sd CU-N10 (3) Guardar el resultado y registrarlo en la bitácora')


def n10_sd_sorteo():
    return sd_obtener_o_crear('sd CU-N10 (4) Obtener o crear el sorteo pendiente',
                              'ObtenerOCrear(juegoGanadores,\nc.Loteria, c.Turno, fecha)')
