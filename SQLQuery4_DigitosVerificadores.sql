USE AgenciaQuiniela;
GO

-- Digito verificador horizontal: uno por fila de IDIOMAS
IF COL_LENGTH('IDIOMAS', 'DigitoVerificador') IS NULL
    ALTER TABLE IDIOMAS ADD DigitoVerificador INT NULL;
GO

-- Digitos verificadores verticales: uno por columna de IDIOMAS (forma parte del DER)
IF OBJECT_ID('IDIOMAS_DIGITO_VERTICAL', 'U') IS NULL
CREATE TABLE IDIOMAS_DIGITO_VERTICAL (
    Columna VARCHAR(50) PRIMARY KEY,
    Digito  INT NOT NULL
);
GO
