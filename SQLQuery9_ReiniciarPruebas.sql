-- ============================================================
-- Reiniciar las pruebas de la gestion comercial
-- ============================================================
-- Borra los movimientos de prueba: los tickets (APUESTAS, JUGADAS y JUGADA_NUMERO), los pagos (PAGOS) y los
-- sorteos con sus numeros ganadores (SORTEOS y NUMERO_GANADOR).
-- NO toca los usuarios, los perfiles y permisos, los idiomas ni el catalogo (JUEGOS, LOTERIAS y TURNOS).
-- Los sorteos se vuelven a crear solos, en estado Abierto, la proxima vez que se vende o se consulta.

USE AgenciaQuiniela;
GO

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    -- primero las tablas que dependen de otras
    DELETE FROM PAGOS;
    DELETE FROM JUGADA_NUMERO;
    DELETE FROM JUGADAS;
    DELETE FROM APUESTAS;
    DELETE FROM NUMERO_GANADOR;
    DELETE FROM SORTEOS;

    COMMIT TRANSACTION;
    PRINT 'Listo: se borraron los tickets, los pagos y los sorteos de prueba.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    PRINT 'No se pudo reiniciar (no se borro nada): ' + ERROR_MESSAGE();
END CATCH;
GO

-- Para borrar tambien los eventos de la bitacora, sacar el comentario de la linea siguiente:
-- DELETE FROM Bitacora;

-- Control: todo tiene que quedar en 0
SELECT (SELECT COUNT(*) FROM APUESTAS) AS Tickets,
       (SELECT COUNT(*) FROM PAGOS)    AS Pagos,
       (SELECT COUNT(*) FROM SORTEOS)  AS Sorteos;
GO
