# -*- coding: utf-8 -*-
"""Diagramas de secuencia de PN03 Pago de premios (CU-N11) y PN04 Control de la gestión comercial
(CU-N12 y CU-N13), mensaje por mensaje según el código."""
from secuencia import Secuencia
from sd_comun import (BD, bitacora, lineas_bitacora, sd_abrir_reportes, sd_construir_reporte, sd_listar_ventas,
                      sd_numeros_ganadores, sd_mostrar_ticket)


# ================================================================== CU-N11 Pagar ticket premiado
def n11_sd_verificar():
    s = Secuencia('sd CU-N11 (1) Verificar si el ticket se puede pagar')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('pb', ':PremioBLL')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('sbs', 'SorteoBLL')
    s.referencia('CU-N04 Buscar ticket: el operador busca el ticket y Buscar() lo lee', ['op', 'fo'])
    with s.llamada('fo', 'fo', 'MostrarTicket(a)'):
        with s.llamada('fo', 'pb', 'Calcular(a)', ret='premios'):
            s.referencia('sd CU-N11 (2)', ['pb'])
        with s.llamada('fo', 'pgb', 'MotivoNoPagable(a, premios)', ret='motivo'):
            with s.fragmento('opt', '[hay sorteos sin realizar]'):
                s.mensaje('pgb', 'sbs', 'Descripcion(s) o DescripcionCorta(s)', ret='descripción')
    with s.alternativa() as a:
        with a.caso('[motivo != null (5.a)]'):
            s.asincrono('fo', 'op', 'muestra el motivo en rojo y deja\nel botón deshabilitado')
        with a.caso('[else]'):
            s.asincrono('fo', 'op', '"Ticket premiado: se le pagan $ X al\ncliente." y habilita «PAGAR $ X»')
    s.nota('fo', 'Cada fila de jugadas muestra el resultado (Premio.Detalle) y el premio (Premio.Importe).', 200)
    return s


def n11_sd_calcular():
    s = Secuencia('sd CU-N11 (2) Calcular el premio de cada jugada (PremioBLL.Calcular)')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('pb', ':PremioBLL')
    s.objeto('sb', ':SorteoBLL')
    with s.llamada('fo', 'pb', 'Calcular(a)', ret='premios'):
        with s.fragmento('loop', '[por cada jugada j de a.Jugadas]'):
            with s.alternativa() as a:
                with a.caso('[j.Sorteo.Estado != Sorteado]'):
                    s.nota('pb', 'Detalle = "Todavía no se sorteó"', 170)
                with a.caso('[else]'):
                    with s.fragmento('opt', '[es la primera jugada de ese sorteo]'):
                        with s.llamada('pb', 'sb', 'ObtenerNumerosGanadores(j.Sorteo)', ret='ganadores'):
                            s.referencia('sd CU-N11 (3)', ['sb'])
                    with s.alternativa() as b:
                        with b.caso('[j.Sorteo.Juego.UsaRango]'):
                            with s.llamada('pb', 'pb', 'CalcularQuiniela(j, ganadores, p)'):
                                with s.fragmento('loop', '[por cada número de la jugada]'):
                                    s.mensaje('pb', 'pb', 'Buscar(n, ganadores)')
                        with b.caso('[else]'):
                            s.mensaje('pb', 'pb', 'CalcularPorAciertos(j, ganadores, p)')
    return s


def n11_sd_numeros():
    return sd_numeros_ganadores('sd CU-N11 (3) Leer los números ganadores de un sorteo', 'pb', ':PremioBLL',
                                'j.Sorteo')


def n11_sd_confirmar():
    s = Secuencia('sd CU-N11 (4) Confirmar el pago')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('sm', ':SesionManager')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('pgb', ':PagoBLL')
    with s.llamada('op', 'fo', 'btnAccion_Click(sender, e)'):
        s.mensaje('fo', 'sm', 'Instancia.UsuarioActual', ret='usuario')
        s.mensaje('fo', 'ab', 'CodigoCorto(apuesta)', ret='codigo')
        with s.llamada('fo', 'fo', 'Confirmar(pregunta, titulo)', ret='respuesta'):
            s.asincrono('fo', 'op', '"¿Pagar $ X del ticket XXXXXXXX?"')
            s.asincrono('op', 'fo', 'Sí / No')
        with s.fragmento('opt', '[respondió Sí]'):
            with s.llamada('fo', 'pgb', 'Pagar(apuesta, usuario)', ret='pagado'):
                s.referencia('sd CU-N11 (6)', ['pgb'])
            with s.fragmento('break', '[excepción (10.a)]'):
                s.asincrono('fo', 'op', 'muestra ex.Message con el título\n"No se pudo completar la operación"')
            s.referencia('sd CU-N11 (5) Mostrar las felicitaciones: FormFelicitaciones(apuesta, pagado, premios)',
                         ['op', 'fo'])
            s.mensaje('fo', 'ab', 'BuscarPorCodigo(\nApuestaBLL.CodigoCompleto(apuesta))', ret='a')
            s.mensaje('fo', 'fo', 'MostrarTicket(a)')
            s.mensaje('fo', 'fo', 'CargarTicketsDeHoy()')
    return s


def n11_sd_felicitaciones():
    s = Secuencia('sd CU-N11 (5) Mostrar las felicitaciones')
    s.actor('op', 'Operador')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('ff', ':FormFelicitaciones', creado=True)
    s.objeto('ab', 'ApuestaBLL')
    with s.crear('fo', 'ff', '«create» FormFelicitaciones(\napuesta, pagado, premios)'):
        s.mensaje('ff', 'ab', 'CodigoCorto(apuesta)', ret='código corto')
    s.mensaje('ff', 'ff', 'OnShown(e)')
    with s.fragmento('loop', '[mientras dura la animación]'):
        with s.llamada('ff', 'ff', 'timer_Tick(sender, e)'):
            s.mensaje('ff', 'ff', 'OnPaint(e)')
    s.asincrono('ff', 'op', '"¡Felicitaciones!" con el código, el\nimporte ganado y las jugadas ganadoras')
    s.asincrono('op', 'ff', 'presiona «ACEPTAR»')
    s.destruir('ff')
    return s


def n11_sd_pagar():
    s = Secuencia('sd CU-N11 (6) Pagar el ticket en la capa de negocio')
    s.objeto('fo', ':FormOperacionTicket')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('pb', ':PremioBLL')
    s.objeto('pgd', ':PagoDAL')
    s.objeto('bb', ':BitacoraBLL')
    with s.llamada('fo', 'pgb', 'Pagar(apuesta, usuario)', ret='total'):
        s.mensaje('pgb', 'pb', 'Calcular(a)', ret='premios')
        s.mensaje('pgb', 'pgb', 'MotivoNoPagable(a, premios)')
        with s.fragmento('break', '[motivo != null]'):
            s.respuesta('pgb', 'fo', 'Exception(motivo)')
        s.nota('pgb', 'Arma el Pago: IdApuesta = a.Id, UsuarioId = cajero.Id, FechaHora = DateTime.Now y '
                      'Monto = suma de los premios.', 200)
        with s.llamada('pgb', 'pgd', 'Registrar(pago, Pagada, Vigente)', ret='true / false'):
            s.referencia('sd CU-N11 (7)', ['pgd'])
        with s.fragmento('break', '[Registrar == false (10.a)]'):
            s.respuesta('pgb', 'fo', 'Exception: "El ticket ya no está vigente.\nBuscalo de nuevo."')
        s.mensaje('pgb', 'bb', 'Registrar(cajero.Id, cajero.Nombre,\n"Pago", detalle)')
    return s


def n11_sd_registrar():
    s = Secuencia('sd CU-N11 (7) Registrar el pago y la bitácora')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('pgd', ':PagoDAL')
    lineas_bitacora(s)
    with s.llamada('pgb', 'pgd', 'Registrar(pago, Pagada, Vigente)', ret='true'):
        s.mensaje('pgd', 'bd', 'BEGIN TRANSACTION')
        s.mensaje('pgd', 'bd', "UPDATE APUESTAS SET Estado = 'Pagada'\nWHERE Id = @id AND Estado = 'Vigente'",
                  ret='filas afectadas')
        with s.fragmento('break', '[no cambió ninguna fila]'):
            s.mensaje('pgd', 'bd', 'ROLLBACK')
            s.respuesta('pgd', 'pgb', 'false')
        s.mensaje('pgd', 'bd', 'INSERT INTO PAGOS (IdApuesta,\nUsuarioId, FechaHora, Monto);\nSELECT SCOPE_IDENTITY()',
                  ret='p.Id')
        s.mensaje('pgd', 'bd', 'COMMIT')
    bitacora(s, 'pgb', 'cajero', 'Pago')
    return s


# ================================================================== CU-N12 Consultar reporte de ventas
def _cierre_ventas(s):
    s.asincrono('fr', 'op', 'muestra «Ventas en línea» con las\nventas de hoy y las tarjetas')


NOTA_ACCESOS = ('«Ventas en Linea» (btnVentas_Click) y «Reporte Semanal» (btnReporte_Click), del Administrador, '
                'crean FormReporte(VistaReporte.Ventas, DateTime.Today) y FormReporte(VistaReporte.Ventas, '
                'DateTime.Today.AddDays(-6)) (flujo 1.a).')


def n12_sd_abrir():
    return sd_abrir_reportes('sd CU-N12 (1) Abrir los reportes', 'sd CU-N12 (2) Construir la pantalla de reportes',
                             _cierre_ventas, nota_fp=NOTA_ACCESOS)


def n12_sd_construir():
    return sd_construir_reporte('sd CU-N12 (2) Construir la pantalla de reportes')


def n12_sd_listar():
    s = Secuencia('sd CU-N12 (3) Listar las ventas del período')
    s.actor('op', 'Operador')
    s.objeto('fr', ':FormReporte')
    s.objeto('ab', ':ApuestaBLL')
    with s.fragmento('loop', '[cada vez que presiona «Buscar»]'):
        with s.llamada('op', 'fr', 'presiona «Buscar»'):
            with s.llamada('fr', 'fr', 'CargarVentas()'):
                with s.llamada('fr', 'ab', 'ListarVentas(dtpVentasDesde.Value,\ndtpVentasHasta.Value, juego)',
                               ret='ventas'):
                    s.referencia('sd CU-N12 (4)', ['ab'])
                with s.fragmento('break', '[excepción (4.a)]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el\ntítulo "Ventas en línea"')
                with s.fragmento('loop', '[por cada ticket a de ventas]'):
                    s.mensaje('fr', 'ab', 'CodigoCorto(a)', ret='código corto')
        s.asincrono('fr', 'op', 'muestra la lista y las tarjetas')
    s.nota('fr', 'Tarjetas: Tickets vendidos y Total vendido sin los tickets en estado Anulada (RN-15); Pagados = '
                 'estado Pagada; Cancelados = estado Anulada. juego = null con "Todos los juegos".', 220)
    return s


def n12_sd_leer():
    return sd_listar_ventas('sd CU-N12 (4) Leer las ventas del período', 'fr', ':FormReporte',
                            'dtpVentasDesde.Value,\ndtpVentasHasta.Value, juego')


def n12_sd_ver():
    s = Secuencia('sd CU-N12 (5) Ver un ticket del reporte')
    s.actor('op', 'Operador')
    s.objeto('fr', ':FormReporte')
    s.objeto('ab', ':ApuestaBLL')
    s.objeto('ft', ':FormTicket', creado=True)
    with s.llamada('op', 'fr', 'doble clic en un ticket\n(CellDoubleClick)'):
        with s.llamada('fr', 'fr', 'AbrirTicket(dgvVentas, e.RowIndex)'):
            with s.llamada('fr', 'ab', 'BuscarPorCodigo(\n((Guid)grilla.Rows[fila].Tag).ToString())', ret='a'):
                s.referencia('CU-N04 (4) BuscarPorCodigo', ['ab'])
            with s.fragmento('break', '[se lanzó una excepción]'):
                s.asincrono('fr', 'op', 'muestra ex.Message con el título "Ticket"')
            with s.crear('fr', 'ft', '«create» FormTicket(a,\nVistaTicket.Consulta)'):
                pass
            s.referencia('sd CU-N12 (6) Mostrar el ticket en la vista de consulta', ['op', 'fr', 'ab', 'ft'])
            s.destruir('ft')
    return s


def n12_sd_ticket():
    return sd_mostrar_ticket('sd CU-N12 (6) Mostrar el ticket en la vista de consulta', 'fr', ':FormReporte', 'a',
                             'Consulta')


# ================================================================== CU-N13 Consultar reporte de pagos
def n13_sd_listar():
    s = Secuencia('sd CU-N13 (1) Ver y listar los pagos del período')
    s.actor('op', 'Operador')
    s.objeto('fr', ':FormReporte')
    s.objeto('pgb', ':PagoBLL')
    s.referencia('CU-N12 (1) y (2): al abrir los reportes, el constructor de FormReporte ya llamó a CargarPagos() '
                 'con Desde y Hasta en la fecha de hoy', ['op', 'fr', 'pgb'])
    with s.llamada('op', 'fr', 'btnPagos_Click(sender, e)'):
        s.mensaje('fr', 'fr', 'MostrarVista(VistaReporte.Pagos)')
    s.asincrono('fr', 'op', 'muestra «Pagos en línea»')
    with s.fragmento('loop', '[cada vez que presiona «Buscar»]'):
        with s.llamada('op', 'fr', 'presiona «Buscar»'):
            with s.llamada('fr', 'fr', 'CargarPagos()'):
                with s.llamada('fr', 'pgb', 'Listar(dtpPagosDesde.Value,\ndtpPagosHasta.Value)', ret='pagos'):
                    s.referencia('sd CU-N13 (2)', ['pgb'])
                with s.fragmento('break', '[excepción (4.a)]'):
                    s.asincrono('fr', 'op', 'muestra ex.Message con el\ntítulo "Pagos en línea"')
        s.asincrono('fr', 'op', 'muestra la lista y las tarjetas')
    s.nota('fr', 'Tarjetas: Pagos realizados (cantidad) y Total pagado (suma de los montos). Doble clic en un pago: '
                 'AbrirTicket(dgvPagos, e.RowIndex), como en sd CU-N12 (5) (flujo 6.a).', 220)
    return s


def n13_sd_leer():
    s = Secuencia('sd CU-N13 (2) Leer los pagos del período')
    s.objeto('fr', ':FormReporte')
    s.objeto('pgb', ':PagoBLL')
    s.objeto('pgd', ':PagoDAL')
    s.bd('bd', BD)
    with s.llamada('fr', 'pgb', 'Listar(dtpPagosDesde.Value,\ndtpPagosHasta.Value)', ret='List<Pago>'):
        with s.fragmento('break', '[desde.Date > hasta.Date]'):
            s.respuesta('pgb', 'fr', 'Exception: "La fecha \'Desde\' no puede\nser mayor que la fecha \'Hasta\'."')
        with s.llamada('pgb', 'pgd', 'Listar(desde.Date,\nhasta.Date.AddDays(1))', ret='List<Pago>'):
            s.mensaje('pgd', 'bd', 'SELECT … FROM PAGOS JOIN APUESTAS\nJOIN USUARIOS CROSS APPLY (JUGADAS\n'
                                   'JOIN SORTEOS) JOIN JUEGOS WHERE\nFechaHora en [desde, hasta)', ret='filas')
    return s
