-- ============================================================
-- Actualiza una base AgenciaQuiniela YA EXISTENTE al modelo con todos los juegos
-- (Quiniela, Quiniela Plus, Quini 6, Loto 5 y Telekino): sorteos, codigo de ticket
-- con GUID, numeros ganadores y pagos.
--
-- Para una base nueva no hace falta: alcanza con ScriptBD_AgenciaQuiniela.sql.
--
-- ATENCION: borra las apuestas cargadas con el modelo anterior de Quiniela (tablas
-- APUESTAS, APUESTA_LOTERIA, APUESTA_TURNO y APUESTA_NUMERO). Usuarios, permisos,
-- bitacora, idiomas, loterias y turnos no se tocan.
-- Se puede correr mas de una vez sin problemas.
-- ============================================================

USE AgenciaQuiniela;
GO

-- 1) Tablas del modelo anterior de Quiniela
IF OBJECT_ID('APUESTA_NUMERO', 'U') IS NOT NULL DROP TABLE APUESTA_NUMERO;
IF OBJECT_ID('APUESTA_LOTERIA', 'U') IS NOT NULL DROP TABLE APUESTA_LOTERIA;
IF OBJECT_ID('APUESTA_TURNO', 'U') IS NOT NULL DROP TABLE APUESTA_TURNO;
IF OBJECT_ID('APUESTAS', 'U') IS NOT NULL AND COL_LENGTH('APUESTAS', 'CodigoTicket') IS NULL
    DROP TABLE APUESTAS;
GO

-- 2) Catalogo: juegos, loterias y turnos
IF OBJECT_ID('JUEGOS', 'U') IS NULL
BEGIN
    CREATE TABLE JUEGOS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL,
        Codigo VARCHAR(20) NOT NULL UNIQUE,
        CantidadNumeros INT NOT NULL,
        NumeroMin INT NOT NULL,
        NumeroMax INT NOT NULL,
        CantidadSorteados INT NOT NULL,
        UsaRango BIT NOT NULL,
        UsaLoteriaTurno BIT NOT NULL,
        Precio DECIMAL(10,2) NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM JUEGOS)
    INSERT INTO JUEGOS (Nombre, Codigo, CantidadNumeros, NumeroMin, NumeroMax, CantidadSorteados, UsaRango, UsaLoteriaTurno, Precio, Activo) VALUES
        ('Quiniela',      'QUINIELA',  1, 0, 9999, 20, 1, 1, NULL, 1),
        ('Quiniela Plus', 'QPLUS',     8, 0,   99, 20, 0, 0,  500, 1),
        ('Quini 6',       'QUINI6',    6, 0,   45,  6, 0, 0, 1000, 1),
        ('Loto 5',        'LOTO5',     5, 1,   36,  5, 0, 0,  800, 1),
        ('Telekino',      'TELEKINO', 15, 1,   25, 15, 0, 0, 1500, 1);
GO

IF OBJECT_ID('LOTERIAS', 'U') IS NULL
BEGIN
    CREATE TABLE LOTERIAS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL,
        Codigo VARCHAR(20) NOT NULL,
        Activa BIT NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM LOTERIAS)
    INSERT INTO LOTERIAS (Nombre, Codigo, Activa) VALUES
        ('Provincia', 'PBA', 1),
        ('La Ciudad', 'CABA', 1),
        ('Cordoba', 'COR', 1),
        ('Santa Fe', 'SFE', 1),
        ('Entre Rios', 'ERI', 1);
GO

IF OBJECT_ID('TURNOS', 'U') IS NULL
BEGIN
    CREATE TABLE TURNOS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(30) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM TURNOS)
    INSERT INTO TURNOS (Nombre, Activo) VALUES
        ('Primera', 1),
        ('Matutina', 1),
        ('Vespertina', 1),
        ('Nocturna', 1);
GO

-- 3) Sorteos y sus numeros ganadores
IF OBJECT_ID('SORTEOS', 'U') IS NULL
BEGIN
    CREATE TABLE SORTEOS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdJuego INT NOT NULL FOREIGN KEY REFERENCES JUEGOS(Id),
        IdLoteria INT NULL FOREIGN KEY REFERENCES LOTERIAS(Id),
        IdTurno INT NULL FOREIGN KEY REFERENCES TURNOS(Id),
        Fecha DATE NOT NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Abierto'
            CHECK (Estado IN ('Abierto', 'Cerrado', 'Sorteado')),
        CONSTRAINT UQ_SORTEOS UNIQUE (IdJuego, IdLoteria, IdTurno, Fecha)
    );
END
GO

IF OBJECT_ID('NUMERO_GANADOR', 'U') IS NULL
BEGIN
    CREATE TABLE NUMERO_GANADOR (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdSorteo INT NOT NULL FOREIGN KEY REFERENCES SORTEOS(Id),
        Numero VARCHAR(4) NOT NULL,
        Posicion INT NOT NULL
    );
END
GO

-- 4) Venta: ticket, jugadas y numeros elegidos
IF OBJECT_ID('APUESTAS', 'U') IS NULL
BEGIN
    CREATE TABLE APUESTAS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CodigoTicket UNIQUEIDENTIFIER NOT NULL UNIQUE,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
        FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        Total DECIMAL(10,2) NOT NULL,
        Estado VARCHAR(20) NOT NULL DEFAULT 'Vigente'
            CHECK (Estado IN ('Vigente', 'Pagada', 'Anulada'))
    );
END
GO

IF OBJECT_ID('JUGADAS', 'U') IS NULL
BEGIN
    CREATE TABLE JUGADAS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdApuesta INT NOT NULL FOREIGN KEY REFERENCES APUESTAS(Id),
        IdSorteo INT NOT NULL FOREIGN KEY REFERENCES SORTEOS(Id),
        Tipo VARCHAR(20) NULL CHECK (Tipo IS NULL OR Tipo IN ('Directa', 'Redoblona')),
        EsAutomatica BIT NOT NULL DEFAULT 0,
        Monto DECIMAL(10,2) NOT NULL,
        Orden INT NOT NULL
    );
END
GO

IF OBJECT_ID('JUGADA_NUMERO', 'U') IS NULL
BEGIN
    CREATE TABLE JUGADA_NUMERO (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdJugada INT NOT NULL FOREIGN KEY REFERENCES JUGADAS(Id),
        Numero VARCHAR(4) NOT NULL,
        Orden INT NOT NULL,
        Rango INT NULL CHECK (Rango IS NULL OR Rango IN (1, 5, 10, 20))
    );
END
GO

-- 5) Cobro
IF OBJECT_ID('PAGOS', 'U') IS NULL
BEGIN
    CREATE TABLE PAGOS (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdApuesta INT NOT NULL UNIQUE FOREIGN KEY REFERENCES APUESTAS(Id),
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
        FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        Monto DECIMAL(10,2) NOT NULL
    );
END
GO
