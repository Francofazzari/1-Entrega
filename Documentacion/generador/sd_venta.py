# -*- coding: utf-8 -*-
"""Diagramas de secuencia de PN01 Venta de apuestas (CU-N01 a CU-N06), mensaje por mensaje según el código."""
from secuencia import Secuencia
from sd_comun import (BD, bitacora, lineas_bitacora, sd_obtener_sorteo, sd_vender, sd_guardar_ticket,
                      sd_listar_ventas, sd_buscar_por_codigo, sd_mostrar_ticket)


def _ticket(s, form, crea, ref):
    """Creación de la pantalla del ticket; su detalle está en el diagrama 'Mostrar el ticket' del caso de uso."""
    with s.crear(form, 'ft', crea):
        pass
    s.referencia(ref, ['op', form, 'ft'])
    s.destruir('ft')


# ================================================================== CU-N01 Vender apuesta de Quiniela
def n01_sd_abrir():
    s = Secuencia('sd CU-N01 (1) Abrir la venta de Quiniela')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fsl', ':FormSeleccionLoteria', creado=True)
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('tb', ':TurnoBLL')
    with s.llamada('op', 'fp', 'BtnJuego_Click(sender, e)'):
        with s.alternativa() as a:
            with a.caso('[juego.UsaLoteriaTurno]'):
                with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
                    with s.crear('fp', 'fsl', '«create» FormSeleccionLoteria(juego)'):
                        with s.llamada('fsl', 'fsl', 'CargarLoterias()'):
                            s.mensaje('fsl', 'lb', 'ObtenerActivas()', ret='todasLasLoterias')
                        with s.llamada('fsl', 'fsl', 'CargarTurnos()'):
                            s.mensaje('fsl', 'tb', 'ObtenerActivos()', ret='todosLosTurnos')
                        s.mensaje('fsl', 'fsl', 'ActualizarResumen()')
            with a.caso('[else]'):
                s.referencia('CU-N02: AbrirHijo con un FormJuego', ['fp'])
    s.asincrono('fsl', 'op', 'muestra un botón por lotería y por horario,\n«Todas», «Todos» y el resumen')
    return s


def n01_sd_leer():
    s = Secuencia('sd CU-N01 (2) Leer las loterías y los turnos activos')
    s.objeto('fsl', ':FormSeleccionLoteria')
    s.objeto('lb', ':LoteriaBLL')
    s.objeto('ld', ':LoteriaDAL')
    s.objeto('tb', ':TurnoBLL')
    s.objeto('td', ':TurnoDAL')
    s.bd('bd', BD)
    with s.llamada('fsl', 'lb', 'ObtenerActivas()', ret='List<Loteria>'):
        with s.llamada('lb', 'ld', 'ObtenerActivas()', ret='lista'):
            s.mensaje('ld', 'bd', 'SELECT Id, Nombre, Codigo, Activa\nFROM LOTERIAS WHERE Activa = 1\nORDER BY Nombre',
                      ret='filas')
    with s.llamada('fsl', 'tb', 'ObtenerActivos()', ret='List<Turno>'):
        with s.llamada('tb', 'td', 'ObtenerActivos()', ret='lista'):
            s.mensaje('td', 'bd', 'SELECT Id, Nombre, Activo\nFROM TURNOS WHERE Activo = 1\nORDER BY Id', ret='filas')
    return s


def n01_sd_elegir():
    s = Secuencia('sd CU-N01 (3) Elegir las loterías y los horarios')
    s.actor('op', 'Operador')
    s.objeto('fsl', ':FormSeleccionLoteria')
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
    return s


def n01_sd_continuar():
    s = Secuencia('sd CU-N01 (4) Continuar a la carga de jugadas')
    s.actor('op', 'Operador')
    s.objeto('fsl', ':FormSeleccionLoteria')
    s.objeto('fa', ':FormApuesta', creado=True)
    s.objeto('lb', ':LoteriaBLL')
    with s.llamada('op', 'fsl', 'btnContinuar_Click(sender, e)'):
        with s.fragmento('break', '[falta lotería u horario (6.a)]'):
            s.asincrono('fsl', 'op', '"Elegí al menos una lotería y un horario."')
        with s.crear('fsl', 'fa', '«create» FormApuesta(juego,\nloteriasElegidas, turnosElegidos)'):
            with s.llamada('fa', 'fa', 'DescribirSorteos()'):
                with s.fragmento('opt', '[loterias.Count > 1]'):
                    s.mensaje('fa', 'lb', 'ObtenerActivas()', ret='List<Loteria>')
            s.mensaje('fa', 'fa', 'CrearColumnas()')
            s.mensaje('fa', 'fa', 'CrearTeclado()')
            s.mensaje('fa', 'fa', 'CrearBotonesRango()')
            s.mensaje('fa', 'fa', 'AplicarTipo(ApuestaBLL.Directa)')
            s.mensaje('fa', 'fa', 'ActualizarTotales()')
    s.destruir('fsl')
    s.asincrono('fa', 'op', 'muestra la carga de jugadas:\nDirecta, a la primera, $ 100')
    return s


def n01_sd_numero():
    s = Secuencia('sd CU-N01 (5) Ingresar el número y las posiciones')
    s.actor('op', 'Operador')
    s.objeto('fa', ':FormApuesta')
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
    s.nota('fa', 'Las teclas del equipo también llaman a Tecla(tecla) (ProcessCmdKey); Enter llama a Agregar(), igual '
                 'que «AGREGAR AL TICKET».', 220)
    return s


def n01_sd_agregar():
    s = Secuencia('sd CU-N01 (6) Agregar la jugada al ticket')
    s.actor('op', 'Operador')
    s.objeto('fa', ':FormApuesta')
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
    return s


def n01_sd_quitar():
    s = Secuencia('sd CU-N01 (7) Quitar una jugada')
    s.actor('op', 'Operador')
    s.objeto('fa', ':FormApuesta')
    with s.llamada('op', 'fa', 'btnQuitar_Click(sender, e)'):
        with s.fragmento('break', '[ninguna jugada seleccionada]'):
            s.asincrono('fa', 'op', '"Seleccioná en la lista la\njugada que querés quitar."')
        s.mensaje('fa', 'fa', 'ActualizarTotales()')
    s.asincrono('fa', 'op', 'muestra la lista sin la jugada\ny el total actualizado')
    return s


def _confirmar(s, form, boton, vender, ref_vender, ref_ticket, guardia_aviso=None):
    """Confirmación de la venta en FormApuesta (CU-N01) o FormJuego (CU-N02)."""
    with s.llamada('op', form, boton):
        if guardia_aviso:
            with s.fragmento('break', guardia_aviso):
                s.mensaje(form, form, 'MostrarAviso("Falta cargar el\n2° número de la redoblona.")')
        s.mensaje(form, 'sm', 'Instancia.UsuarioActual', ret='vendedor : Usuario')
        with s.llamada(form, 'ab', vender, ret='a : Apuesta'):
            s.referencia(ref_vender, ['ab'])
        with s.fragmento('break', '[se lanzó una excepción]'):
            s.asincrono(form, 'op', 'muestra ex.Message con el título\n"No se pudo registrar la venta"')
        _ticket(s, form, '«create» FormTicket(a, VistaTicket.Venta)', ref_ticket)
        s.mensaje(form, form, 'ActualizarTotales()')
    s.asincrono(form, 'op', 'vacía la lista: listo para otra venta')


def n01_sd_confirmar():
    s = Secuencia('sd CU-N01 (8) Confirmar la venta')
    s.actor('op', 'Operador')
    s.objeto('fa', ':FormApuesta')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ft', ':FormTicket', creado=True)
    _confirmar(s, 'fa', 'btnEnviar_Click(sender, e)', 'Vender(juego, loterias, turnos,\nplantillas, vendedor)',
               'sd CU-N01 (10) Vender', 'sd CU-N01 (9) Mostrar el ticket',
               guardia_aviso='[primerNumeroRedoblona != null (9.a)]')
    return s


def n01_sd_ticket():
    return sd_mostrar_ticket('sd CU-N01 (9) Mostrar el ticket de la venta', 'fa', ':FormApuesta', 'a', 'Venta')


def n01_sd_vender():
    return sd_vender('sd CU-N01 (10) Armar y registrar el ticket (ApuestaBLL.Vender)', 'fa', ':FormApuesta',
                     'juego, loterias, turnos,\nplantillas, vendedor')


def n01_sd_sorteo():
    return sd_obtener_sorteo('sd CU-N01 (11) Obtener el sorteo del día para vender')


def n01_sd_guardar():
    return sd_guardar_ticket('sd CU-N01 (12) Guardar el ticket y registrar la venta')


# ================================================================== CU-N02 Vender apuesta de juego de numeros
def n02_sd_abrir():
    s = Secuencia('sd CU-N02 (1) Abrir el juego')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fj', ':FormJuego', creado=True)
    s.objeto('jb', 'JuegoBLL')
    with s.llamada('op', 'fp', 'BtnJuego_Click(sender, e)'):
        with s.alternativa() as a:
            with a.caso('[juego.UsaLoteriaTurno]'):
                s.referencia('CU-N01: AbrirHijo con un FormSeleccionLoteria', ['fp'])
            with a.caso('[else]'):
                with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
                    with s.crear('fp', 'fj', '«create» FormJuego(juego)'):
                        s.mensaje('fj', 'fj', 'Formatear(juego.NumeroMin)')
                        s.mensaje('fj', 'fj', 'Formatear(juego.NumeroMax)')
                        with s.llamada('fj', 'fj', 'CrearTablero()'):
                            with s.fragmento('loop', '[por cada número del rango del juego]'):
                                with s.llamada('fj', 'fj', 'Formatear(numero)'):
                                    s.mensaje('fj', 'jb', 'FormatearNumero(juego, numero)', ret='número con ceros')
                        s.mensaje('fj', 'fj', 'CrearColumnas()')
                        s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
                        s.mensaje('fj', 'fj', 'ActualizarTotales()')
    s.asincrono('fj', 'op', 'muestra el tablero, "Elegí N números\ndel … al …", el precio y el sorteo del día')
    return s


def n02_sd_elegir():
    s = Secuencia('sd CU-N02 (2) Elegir los números en el tablero')
    s.actor('op', 'Operador')
    s.objeto('fj', ':FormJuego')
    with s.fragmento('loop', '[por cada número que toca]'):
        with s.llamada('op', 'fj', 'BtnNumero_Click(sender, e)'):
            with s.alternativa() as b:
                with b.caso('[ya eligió todos (3.b)]'):
                    s.mensaje('fj', 'fj', 'ActualizarSeleccion("Ya elegiste los "\n+ juego.CantidadNumeros +\n'
                                          '" números. Quitá uno\npara cambiarlo.")')
                with b.caso('[else]'):
                    s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
        s.asincrono('fj', 'op', '"N de M elegidos" o "Jugada completa"')
    with s.fragmento('opt', '[presiona «Limpiar» (3.b)]'):
        with s.llamada('op', 'fj', 'btnLimpiar_Click(sender, e)'):
            s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
    return s


def n02_sd_automatica():
    s = Secuencia('sd CU-N02 (3) Pedir una jugada automática')
    s.actor('op', 'Operador')
    s.objeto('fj', ':FormJuego')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('az', 'Azar')
    with s.llamada('op', 'fj', 'btnAutomatica_Click(sender, e)'):
        with s.llamada('fj', 'ab', 'GenerarNumerosAutomaticos(juego)', ret='List<int>'):
            with s.fragmento('loop', '[hasta CantidadNumeros distintos]'):
                s.mensaje('ab', 'az', 'Entre(juego.NumeroMin,\njuego.NumeroMax)', ret='número')
        s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
    s.asincrono('fj', 'op', 'marca los números elegidos al azar:\n"Jugada automática completa"')
    return s


def n02_sd_agregar():
    s = Secuencia('sd CU-N02 (4) Agregar la jugada al ticket')
    s.actor('op', 'Operador')
    s.objeto('fj', ':FormJuego')
    with s.llamada('op', 'fj', 'btnAgregar_Click(sender, e)'):
        with s.fragmento('break', '[faltan números (5.a)]'):
            s.mensaje('fj', 'fj', 'ActualizarSeleccion(faltan == 1\n? "Falta elegir 1 número."\n: "Faltan elegir " + '
                                  'faltan\n+ " números.")')
        with s.fragmento('loop', '[por cada número elegido n]'):
            s.mensaje('fj', 'fj', 'Formatear(n)')
        s.mensaje('fj', 'fj', 'ActualizarSeleccion()')
        s.mensaje('fj', 'fj', 'ActualizarTotales()')
    s.asincrono('fj', 'op', 'muestra la jugada en la lista\n(Manual o Automática) y el total')
    return s


def n02_sd_quitar():
    s = Secuencia('sd CU-N02 (5) Quitar una jugada')
    s.actor('op', 'Operador')
    s.objeto('fj', ':FormJuego')
    with s.llamada('op', 'fj', 'btnQuitar_Click(sender, e)'):
        with s.fragmento('break', '[ninguna jugada seleccionada]'):
            s.asincrono('fj', 'op', '"Seleccioná en la lista la\njugada que querés quitar."')
        s.mensaje('fj', 'fj', 'ActualizarTotales()')
    s.asincrono('fj', 'op', 'muestra la lista renumerada\ny el total actualizado')
    return s


def n02_sd_confirmar():
    s = Secuencia('sd CU-N02 (6) Confirmar la venta')
    s.actor('op', 'Operador')
    s.objeto('fj', ':FormJuego')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ft', ':FormTicket', creado=True)
    _confirmar(s, 'fj', 'btnConfirmar_Click(sender, e)', 'Vender(juego, null, null,\nplantillas, vendedor)',
               'sd CU-N02 (8) Vender', 'sd CU-N02 (7) Mostrar el ticket')
    return s


def n02_sd_ticket():
    return sd_mostrar_ticket('sd CU-N02 (7) Mostrar el ticket de la venta', 'fj', ':FormJuego', 'a', 'Venta')


def n02_sd_vender():
    return sd_vender('sd CU-N02 (8) Armar y registrar el ticket (ApuestaBLL.Vender)', 'fj', ':FormJuego',
                     'juego, null, null,\nplantillas, vendedor')


def n02_sd_sorteo():
    return sd_obtener_sorteo('sd CU-N02 (9) Obtener el sorteo del día para vender')


def n02_sd_guardar():
    return sd_guardar_ticket('sd CU-N02 (10) Guardar el ticket y registrar la venta')


# ================================================================== CU-N03 Imprimir ticket
def n03_sd_imprimir():
    s = Secuencia('sd CU-N03 (1) Imprimir el ticket con vista previa')
    s.actor('op', 'Operador')
    s.objeto('ft', ':FormTicket')
    s.objeto('pd', ':PrintDocument', creado=True)
    s.objeto('pp', ':PrintPreviewDialog', creado=True)
    with s.llamada('op', 'ft', 'btnImprimir_Click(sender, e)'):
        with s.crear('ft', 'pd', '«create» PrintDocument()'):
            pass
        with s.crear('ft', 'pp', '«create» PrintPreviewDialog()'):
            pass
        with s.llamada('ft', 'pp', 'ShowDialog(this)'):
            with s.llamada('pp', 'pd', 'Print() [vista previa]'):
                with s.llamada('pd', 'ft', 'evento PrintPage(s, ev)'):
                    s.referencia('sd CU-N03 (2)', ['ft'])
            s.asincrono('pp', 'op', 'muestra la vista previa\n"Imprimir ticket XXXXXXXX"')
            with s.fragmento('opt', '[imprime desde la vista previa]'):
                with s.llamada('op', 'pp', 'presiona el botón de imprimir'):
                    with s.llamada('pp', 'pd', 'Print()'):
                        s.mensaje('pd', 'ft', 'evento PrintPage(s, ev)')
            s.asincrono('op', 'pp', 'cierra la vista previa')
        with s.fragmento('break', '[InvalidPrinterException (2.a)]'):
            s.asincrono('ft', 'op', '"No hay ninguna impresora\ninstalada en esta computadora."')
        s.destruir('pp')
        s.destruir('pd')
    s.nota('pd', 'PrintDocument.Print() envía la página a la impresora predeterminada de Windows: la impresora no es '
                 'un actor del sistema, es el medio de salida que usa .NET.', 230)
    return s


def n03_sd_dibujar():
    s = Secuencia('sd CU-N03 (2) Dibujar el ticket en la página')
    s.objeto('pd', ':PrintDocument')
    s.objeto('ft', ':FormTicket')
    s.objeto('tr', 'TicketRenderer')
    s.objeto('ab', 'ApuestaBLL')
    with s.llamada('pd', 'ft', 'evento PrintPage(s, ev)'):
        with s.llamada('ft', 'tr', 'Dibujar(ev.Graphics, apuesta,\nesReimpresion, ev.MarginBounds.Left,\n'
                                   'ev.MarginBounds.Top)', ret='alto'):
            with s.fragmento('opt', '[reimpresion]'):
                s.mensaje('tr', 'tr', 'Centrado(g, "***  REIMPRESIÓN  ***",\nfNegrita, tinta, x, cy)')
            s.mensaje('tr', 'ab', 'CodigoCorto(a)', ret='código corto')
            with s.fragmento('loop', '[por cada sorteo de las jugadas]'):
                s.mensaje('tr', 'tr', 'TituloSorteo(s)')
                with s.fragmento('loop', '[por cada jugada j de ese sorteo]'):
                    with s.llamada('tr', 'tr', 'DibujarJugada(g, j, fNormal, fNumeros,\nfChica, tinta, suave, x, cy)'):
                        with s.fragmento('opt', '[j.Sorteo.Juego.UsaRango]'):
                            s.mensaje('tr', 'ab', 'DescribirRango(n.Rango)', ret='"a la primera" o "a los N"')
            s.mensaje('tr', 'tr', 'CodigoDeBarras(g, a, tinta, x, cy)')
            s.mensaje('tr', 'ab', 'CodigoCompleto(a)', ret='código completo')
            s.mensaje('tr', 'ab', 'CodigoCorto(a)', ret='código corto')
            with s.fragmento('opt', '[a.Estado != Vigente]'):
                s.mensaje('tr', 'tr', 'Sello(g, a.Estado == Pagada\n? "PAGADO" : "CANCELADO",\ncolor, x, '
                                      'inicioJugadas,\nfinJugadas)')
    return s


# ================================================================== CU-N04 Buscar ticket
def n04_sd_abrir():
    s = Secuencia('sd CU-N04 (1) Abrir la operación y listar los tickets de hoy')
    s.actor('op', 'Operador')
    s.objeto('fp', ':FormPrincipal')
    s.objeto('fo', ':FormOperacionTicket', creado=True)
    s.objeto('ab', ':ApuestaBLL')
    with s.llamada('op', 'fp', 'btnPagar_Click(sender, e) /\nbtnCancelar_Click(sender, e) /\n'
                               'btnReimprimir_Click(sender, e)'):
        with s.llamada('fp', 'fp', 'AbrirHijo(crear)'):
            with s.crear('fp', 'fo', '«create» FormOperacionTicket(modo)'):
                s.mensaje('fo', 'fo', 'CrearResumen()')
                s.mensaje('fo', 'fo', 'CrearColumnas()')
                s.mensaje('fo', 'fo', 'MostrarTicket(null)')
                with s.llamada('fo', 'fo', 'CargarTicketsDeHoy()'):
                    s.mensaje('fo', 'ab', 'ListarVentas(DateTime.Today,\nDateTime.Today, null)', ret='List<Apuesta>')
                    with s.fragmento('break', '[se lanzó una excepción]'):
                        s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudieron cargar los tickets '
                                                'de hoy"')
                    with s.fragmento('loop', '[por cada ticket a de la lista]'):
                        s.mensaje('fo', 'ab', 'CodigoCorto(a)', ret='código corto')
    s.asincrono('fo', 'op', 'muestra "Pagar ticket", "Cancelar ticket" o\n"Reimprimir ticket", la búsqueda y '
                            '"Tickets de hoy"')
    return s


def n04_sd_listar():
    return sd_listar_ventas('sd CU-N04 (2) Leer los tickets vendidos hoy', 'fo', ':FormOperacionTicket',
                            'DateTime.Today,\nDateTime.Today, null')


def n04_sd_buscar():
    s = Secuencia('sd CU-N04 (3) Buscar el ticket y mostrar su detalle')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ab', ':ApuestaBLL')
    with s.alternativa() as a:
        with a.caso('[presiona «Buscar» o Enter]'):
            with s.llamada('op', 'fo', 'btnBuscar_Click(sender, e)\no txtCodigo_KeyDown(sender, e)'):
                with s.llamada('fo', 'fo', 'Buscar()'):
                    with s.llamada('fo', 'ab', 'BuscarPorCodigo(txtCodigo.Text)', ret='a : Apuesta'):
                        s.referencia('sd CU-N04 (4)', ['ab'])
                    with s.fragmento('break', '[excepción (4.a a 4.d)]'):
                        s.mensaje('fo', 'fo', 'MostrarTicket(null)')
                        s.asincrono('fo', 'op', 'muestra ex.Message en rojo')
                    with s.llamada('fo', 'fo', 'MostrarTicket(a)'):
                        s.mensaje('fo', 'ab', 'CodigoCorto(a)', ret='código corto')
        with a.caso('[hace clic en un ticket de la lista (3.a)]'):
            with s.llamada('op', 'fo', 'dgvHoy_CellClick(sender, e)'):
                s.mensaje('fo', 'ab', 'CodigoCompleto(elegido)', ret='txtCodigo.Text')
                s.mensaje('fo', 'fo', 'Buscar()')
    s.nota('fo', 'Buscar() hace lo mismo en los dos casos. Según el modo, MostrarTicket(a) también calcula los '
                 'premios y el motivo para pagar (CU-N11) o para cancelar (CU-N06).', 230)
    s.asincrono('fo', 'op', 'muestra el detalle del ticket y sus\njugadas con el estado de cada sorteo')
    return s


def n04_sd_codigo():
    return sd_buscar_por_codigo('sd CU-N04 (4) Buscar el ticket por su código (BuscarPorCodigo)')


# ================================================================== CU-N05 Reimprimir ticket
def n05_sd_reimprimir():
    s = Secuencia('sd CU-N05 (1) Reimprimir el ticket')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ft', ':FormTicket', creado=True)
    s.referencia('CU-N04 Buscar ticket: el operador busca el ticket y Buscar() lo muestra con MostrarTicket(a)',
                 ['op', 'fo', 'ab'])
    s.asincrono('fo', 'op', '"La reimpresión sale marcada como\nREIMPRESIÓN." (o "El ticket está …:\nla reimpresión '
                            'sale con ese sello.")')
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        with s.llamada('fo', 'ab', 'RegistrarReimpresion(apuesta, usuario)'):
            s.referencia('sd CU-N05 (3)', ['ab'])
        with s.fragmento('break', '[excepción (6.a)]'):
            s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
        _ticket(s, 'fo', '«create» FormTicket(apuesta,\nVistaTicket.Reimpresion)', 'sd CU-N05 (2) Mostrar la reimpresión')
        s.mensaje('fo', 'ab', 'BuscarPorCodigo(\nApuestaBLL.CodigoCompleto(apuesta))', ret='a')
        s.mensaje('fo', 'fo', 'MostrarTicket(a)')
        s.mensaje('fo', 'fo', 'CargarTicketsDeHoy()')
    return s


def n05_sd_ticket():
    return sd_mostrar_ticket('sd CU-N05 (2) Mostrar la reimpresión del ticket', 'fo', ':FormOperacionTicket',
                             'apuesta', 'Reimpresion')


def n05_sd_registrar():
    s = Secuencia('sd CU-N05 (3) Registrar la reimpresión en la bitácora')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ab', ':ApuestaBLL')
    lineas_bitacora(s)
    with s.llamada('fo', 'ab', 'RegistrarReimpresion(apuesta, usuario)'):
        s.mensaje('ab', 'ab', 'NombreJuego(a)')
        s.mensaje('ab', 'ab', 'CodigoCorto(a)')
        bitacora(s, 'ab', 'usuario', 'Reimpresion')
    return s


# ================================================================== CU-N06 Cancelar ticket
def n06_sd_verificar():
    s = Secuencia('sd CU-N06 (1) Verificar si el ticket se puede cancelar')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('sbs', 'SorteoBLL')
    s.referencia('CU-N04 Buscar ticket: el operador busca el ticket y Buscar() lo lee', ['op', 'fo', 'ab'])
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
    return s


def n06_sd_confirmar():
    s = Secuencia('sd CU-N06 (2) Confirmar la cancelación')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        with s.llamada('fo', 'fo', 'Confirmar(pregunta, titulo)', ret='respuesta'):
            s.asincrono('fo', 'op', '"¿Cancelar el ticket XXXXXXXX por $ X?\nEsta operación no se puede deshacer."')
            s.asincrono('op', 'fo', 'Sí / No')
        with s.fragmento('opt', '[respondió Sí]'):
            with s.llamada('fo', 'ab', 'Anular(apuesta, usuario)'):
                s.referencia('sd CU-N06 (3)', ['ab'])
            with s.fragmento('break', '[excepción (8.a)]'):
                s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
            s.asincrono('fo', 'op', '"El ticket XXXXXXXX quedó cancelado.\nDevolvé $ X al cliente."')
            s.mensaje('fo', 'ab', 'BuscarPorCodigo(\nApuestaBLL.CodigoCompleto(apuesta))', ret='a')
            s.mensaje('fo', 'fo', 'MostrarTicket(a)')
            s.mensaje('fo', 'fo', 'CargarTicketsDeHoy()')
    return s


def n06_sd_anular():
    s = Secuencia('sd CU-N06 (3) Anular el ticket y registrar la cancelación')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ad', ':ApuestaDAL')
    lineas_bitacora(s)
    with s.llamada('fo', 'ab', 'Anular(apuesta, usuario)'):
        s.mensaje('ab', 'ab', 'MotivoNoAnulable(a)')
        with s.fragmento('break', '[motivo != null]'):
            s.respuesta('ab', 'fo', 'Exception(motivo)')
        with s.llamada('ab', 'ad', 'CambiarEstado(a.Id, Anulada, Vigente)', ret='true / false'):
            s.mensaje('ad', 'bd', "UPDATE APUESTAS SET Estado = 'Anulada'\nWHERE Id = @id AND Estado = 'Vigente'",
                      ret='filas afectadas')
        with s.fragmento('break', '[CambiarEstado == false (8.a)]'):
            s.respuesta('ab', 'fo', 'Exception: "El ticket cambió de estado mientras\nlo consultabas. Buscalo de nuevo."')
        s.mensaje('ab', 'ab', 'NombreJuego(a)')
        s.mensaje('ab', 'ab', 'CodigoCorto(a)')
        bitacora(s, 'ab', 'usuario', 'Cancelacion')
    return s
