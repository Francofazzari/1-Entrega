USE AgenciaQuiniela;
GO

-- Se simplifica: el digito vertical ahora es UNO solo por tabla (no uno por columna),
-- calculado directamente sobre la columna DigitoVerificador de IDIOMAS. Se recrea la tabla.
IF OBJECT_ID('IDIOMAS_DIGITO_VERTICAL', 'U') IS NOT NULL
    DROP TABLE IDIOMAS_DIGITO_VERTICAL;
GO

CREATE TABLE IDIOMAS_DIGITO_VERTICAL (
    Digito INT NOT NULL
);
GO
