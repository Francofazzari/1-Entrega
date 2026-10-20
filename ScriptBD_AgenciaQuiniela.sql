USE AgenciaQuiniela;
GO

-- Eliminar tablas viejas si existen para no tener conflictos
-- Gestion comercial (primero las tablas que dependen de otras)
IF OBJECT_ID('PAGOS', 'U') IS NOT NULL DROP TABLE PAGOS;
IF OBJECT_ID('JUGADA_NUMERO', 'U') IS NOT NULL DROP TABLE JUGADA_NUMERO;
IF OBJECT_ID('JUGADAS', 'U') IS NOT NULL DROP TABLE JUGADAS;
IF OBJECT_ID('NUMERO_GANADOR', 'U') IS NOT NULL DROP TABLE NUMERO_GANADOR;
IF OBJECT_ID('APUESTA_NUMERO', 'U') IS NOT NULL DROP TABLE APUESTA_NUMERO;   -- modelo anterior
IF OBJECT_ID('APUESTA_LOTERIA', 'U') IS NOT NULL DROP TABLE APUESTA_LOTERIA; -- modelo anterior
IF OBJECT_ID('APUESTA_TURNO', 'U') IS NOT NULL DROP TABLE APUESTA_TURNO;     -- modelo anterior
IF OBJECT_ID('APUESTAS', 'U') IS NOT NULL DROP TABLE APUESTAS;
IF OBJECT_ID('SORTEOS', 'U') IS NOT NULL DROP TABLE SORTEOS;
IF OBJECT_ID('JUEGOS', 'U') IS NOT NULL DROP TABLE JUEGOS;
IF OBJECT_ID('LOTERIAS', 'U') IS NOT NULL DROP TABLE LOTERIAS;
IF OBJECT_ID('TURNOS', 'U') IS NOT NULL DROP TABLE TURNOS;
IF OBJECT_ID('AUDITORIA_LOGIN', 'U') IS NOT NULL DROP TABLE AUDITORIA_LOGIN;
IF OBJECT_ID('USUARIO_PERMISO', 'U') IS NOT NULL DROP TABLE USUARIO_PERMISO;
IF OBJECT_ID('PERMISO_PERMISO', 'U') IS NOT NULL DROP TABLE PERMISO_PERMISO;
IF OBJECT_ID('PERFILES_PERMISOS', 'U') IS NOT NULL DROP TABLE PERFILES_PERMISOS;
IF OBJECT_ID('PERFIL_PERMISOS', 'U') IS NOT NULL DROP TABLE PERFIL_PERMISOS;
IF OBJECT_ID('CAMBIOS_USUARIO', 'U') IS NOT NULL DROP TABLE CAMBIOS_USUARIO;
IF OBJECT_ID('USUARIOS', 'U') IS NOT NULL DROP TABLE USUARIOS;
IF OBJECT_ID('PERMISOS', 'U') IS NOT NULL DROP TABLE PERMISOS;
IF OBJECT_ID('PERFILES', 'U') IS NOT NULL DROP TABLE PERFILES;
IF OBJECT_ID('Bitacora', 'U') IS NOT NULL DROP TABLE Bitacora;
GO

CREATE TABLE PERMISOS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(50) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(100),
    EsPadre BIT NOT NULL DEFAULT 0
);
GO

CREATE TABLE PERMISO_PERMISO (
    IdPadre INT NOT NULL FOREIGN KEY REFERENCES PERMISOS(Id),
    IdHijo INT NOT NULL FOREIGN KEY REFERENCES PERMISOS(Id),
    PRIMARY KEY(IdPadre, IdHijo)
);
GO

CREATE TABLE USUARIOS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NroTerminal VARCHAR(20) NOT NULL UNIQUE,
    Clave VARCHAR(256) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE USUARIO_PERMISO (
    IdUsuario INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
    IdPermiso INT NOT NULL FOREIGN KEY REFERENCES PERMISOS(Id),
    PRIMARY KEY(IdUsuario, IdPermiso)
);
GO

CREATE TABLE AUDITORIA_LOGIN (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
    FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
    Accion VARCHAR(10) NOT NULL
);
GO

CREATE TABLE Bitacora (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FechaHora DATETIME DEFAULT GETDATE(),
    UsuarioId INT,
    UsuarioNombre VARCHAR(100),
    Actividad VARCHAR(100),
    Detalle VARCHAR(255)
);
GO

CREATE TABLE CAMBIOS_USUARIO (
    IdHistorial INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    FechaCambio DATETIME NOT NULL DEFAULT GETDATE(),
    ResponsableNombre VARCHAR(100) NOT NULL,
    Operacion VARCHAR(20) NOT NULL,
    NroTerminal VARCHAR(20),
    Clave VARCHAR(256),
    Nombre VARCHAR(100),
    Apellido VARCHAR(100),
    Activo BIT,
    PermisosIds VARCHAR(200)
);
GO

-- Datos iniciales (Familias y Patentes base)
INSERT INTO PERMISOS (Codigo, Nombre, Descripcion, EsPadre) VALUES 
    ('Admin', 'Administrador', 'Rol Admin', 1),
    ('Operador', 'Operador', 'Rol Operador', 1),
    ('GestionEmpleados', 'Gestion de Empleados', 'Patente gestion de empleados', 0),
    ('CargarEmpleado', 'Cargar Empleado', 'Patente cargar empleado', 0),
    ('ModificarEmpleado', 'Modificar Empleado', 'Patente modificar empleado', 0),
    ('EliminarEmpleado', 'Eliminar Empleado', 'Patente eliminar empleado', 0),
    ('VerVentas', 'Ver Ventas en Linea', 'Patente ver ventas en linea', 0),
    ('ReporteSemanal', 'Reporte Semanal', 'Patente reporte semanal', 0),
    ('CargarApuesta', 'Cargar Apuesta', 'Patente cargar apuesta', 0),
    ('CobrarJugada', 'Cobrar Jugada', 'Patente cobrar jugada', 0);

-- Relacionamos al Admin(1) con las patentes
INSERT INTO PERMISO_PERMISO (IdPadre, IdHijo) VALUES (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8);

-- Relacionamos al Operador(2) con patentes operativas
INSERT INTO PERMISO_PERMISO (IdPadre, IdHijo) VALUES (2, 9), (2, 10);

-- Usuario admin (Terminal: t001, clave: 1234)
INSERT INTO USUARIOS (NroTerminal, Clave, Nombre, Apellido, Activo)
VALUES ('t001', 
        '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4',
        'Juan', 'Perez', 1);

-- Le asignamos el rol "Administrador" al admin
INSERT INTO USUARIO_PERMISO (IdUsuario, IdPermiso) VALUES (1, 1);
GO

IF OBJECT_ID('TRADUCCIONES', 'U') IS NOT NULL DROP TABLE TRADUCCIONES;
IF OBJECT_ID('PALABRAS', 'U') IS NOT NULL DROP TABLE PALABRAS;
IF OBJECT_ID('IDIOMAS', 'U') IS NOT NULL DROP TABLE IDIOMAS;
GO

CREATE TABLE IDIOMAS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Codigo VARCHAR(10) NOT NULL
);
GO

CREATE TABLE PALABRAS (
    Tag VARCHAR(50) PRIMARY KEY
);
GO

CREATE TABLE TRADUCCIONES (
    IdiomaId INT NOT NULL FOREIGN KEY REFERENCES IDIOMAS(Id),
    Tag VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES PALABRAS(Tag),
    Traduccion VARCHAR(200) NOT NULL,
    PRIMARY KEY(IdiomaId, Tag)
);
GO

INSERT INTO IDIOMAS (Nombre, Codigo) VALUES ('Espanol', 'es-AR');

INSERT INTO PALABRAS (Tag) VALUES 
('btn_login'), ('btn_logout'), ('lbl_usuario'), ('lbl_clave'),
('btn_usuarios'), ('btn_perfiles'), ('btn_ventas'), ('btn_reporte'),
('btn_apuesta'), ('abm_agregar'), ('abm_eliminar'), ('abm_limpiar'),
('abm_modificar'), ('abm_permisos'), ('abm_titulo'), ('btn_composite'),
('lbl_terminal'), ('btn_ingresar'), ('btn_idiomas');
GO

INSERT INTO TRADUCCIONES (IdiomaId, Tag, Traduccion) VALUES 
(1, 'btn_login', 'Iniciar sesion'),
(1, 'btn_logout', 'Cerrar sesion'),
(1, 'lbl_usuario', 'Usuario:'),
(1, 'lbl_clave', 'Contrasena:'),
(1, 'btn_usuarios', 'Gestion Usuarios'),
(1, 'btn_perfiles', 'Perfiles'),
(1, 'btn_ventas', 'Ventas en Linea'),
(1, 'btn_reporte', 'Reporte Semanal'),
(1, 'btn_apuesta', 'Cargar Apuesta'),
(1, 'abm_agregar', 'Agregar'),
(1, 'abm_eliminar', 'Eliminar'),
(1, 'abm_limpiar', 'Limpiar'),
(1, 'abm_modificar', 'Modificar'),
(1, 'abm_permisos', 'Editar Permisos'),
(1, 'abm_titulo', 'Administrar Usuarios'),
(1, 'btn_composite', 'Administrar Composite'),
(1, 'lbl_terminal', 'Numero de terminal'),
(1, 'btn_ingresar', 'Ingresar al sistema'),
(1, 'btn_idiomas', 'Gestion Idiomas');
GO


-- Digito verificador horizontal: uno por fila de IDIOMAS
IF COL_LENGTH('IDIOMAS', 'DigitoVerificador') IS NULL
    ALTER TABLE IDIOMAS ADD DigitoVerificador INT NULL;
GO

-- Digito verificador vertical: un unico valor para toda la tabla IDIOMAS, calculado sobre
-- la columna DigitoVerificador (forma parte del DER)
IF OBJECT_ID('IDIOMAS_DIGITO_VERTICAL', 'U') IS NULL
CREATE TABLE IDIOMAS_DIGITO_VERTICAL (
    Digito INT NOT NULL
);
GO


-- Agrega el idioma Ingles si todavia no existe
IF NOT EXISTS (SELECT 1 FROM IDIOMAS WHERE Codigo = 'en-US')
    INSERT INTO IDIOMAS (Nombre, Codigo) VALUES ('English', 'en-US');
GO

-- Traducciones al ingles de todas las palabras existentes en PALABRAS.
-- Usa MERGE para poder correr el script varias veces sin duplicar ni fallar.
DECLARE @idiomaIngles INT = (SELECT Id FROM IDIOMAS WHERE Codigo = 'en-US');

MERGE TRADUCCIONES AS destino
USING (VALUES
    ('btn_login',      'Login'),
    ('btn_logout',     'Log out'),
    ('lbl_usuario',    'User:'),
    ('lbl_clave',      'Password:'),
    ('btn_usuarios',   'Manage Users'),
    ('btn_perfiles',   'Profiles'),
    ('btn_ventas',     'Online Sales'),
    ('btn_reporte',    'Weekly Report'),
    ('btn_apuesta',    'Place Bet'),
    ('abm_agregar',    'Add'),
    ('abm_eliminar',   'Delete'),
    ('abm_limpiar',    'Clear'),
    ('abm_modificar',  'Modify'),
    ('abm_permisos',   'Edit Permissions'),
    ('abm_titulo',     'Manage Users'),
    ('btn_composite',  'Manage Composite'),
    ('lbl_terminal',   'Terminal number'),
    ('btn_ingresar',   'Log in'),
    ('btn_idiomas',    'Manage Languages')
) AS origen (Tag, Traduccion)
ON destino.IdiomaId = @idiomaIngles AND destino.Tag = origen.Tag
WHEN MATCHED THEN
    UPDATE SET Traduccion = origen.Traduccion
WHEN NOT MATCHED THEN
    INSERT (IdiomaId, Tag, Traduccion) VALUES (@idiomaIngles, origen.Tag, origen.Traduccion);
GO

-- ============================================================
-- T08. Gestion Comercial: todos los juegos de la agencia
-- ============================================================
-- Modelo: cada JUEGO define sus reglas; cada SORTEO es un evento concreto de un juego
-- (en Quiniela, ademas, de una loteria y un turno). El cliente se lleva un ticket
-- (APUESTAS) con un codigo unico (GUID); el ticket contiene JUGADAS, cada jugada apunta
-- a UN sorteo y se juega con sus numeros (JUGADA_NUMERO). Cuando el sorteo se realiza
-- se guardan sus NUMERO_GANADOR, y si el ticket gano se registra el PAGO.

-- Juegos que vende la agencia y sus reglas.
CREATE TABLE JUEGOS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Codigo VARCHAR(20) NOT NULL UNIQUE,      -- identificador fijo que usa el sistema
    CantidadNumeros INT NOT NULL,            -- cuantos numeros se eligen por jugada
    NumeroMin INT NOT NULL,                  -- numero valido mas chico
    NumeroMax INT NOT NULL,                  -- numero valido mas grande
    CantidadSorteados INT NOT NULL,          -- cuantos numeros salen en cada sorteo
    UsaRango BIT NOT NULL,                   -- se juega a una posicion (solo Quiniela)
    UsaLoteriaTurno BIT NOT NULL,            -- se sortea por loteria y turno (solo Quiniela)
    Precio DECIMAL(10,2) NULL,               -- precio fijo de la jugada (NULL: lo elige el cliente)
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE LOTERIAS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Codigo VARCHAR(20) NOT NULL,
    Activa BIT NOT NULL DEFAULT 1
);
GO

-- Horarios de sorteo de la Quiniela (Primera, Matutina, Vespertina, Nocturna).
CREATE TABLE TURNOS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(30) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- Cada sorteo concreto. Loteria y turno quedan en NULL en los juegos que no los usan.
-- Hay un solo sorteo por juego + loteria + turno + fecha.
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
GO

-- Numeros que salieron en un sorteo, con su posicion (en Quiniela, del 1 al 20).
CREATE TABLE NUMERO_GANADOR (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdSorteo INT NOT NULL FOREIGN KEY REFERENCES SORTEOS(Id),
    Numero VARCHAR(4) NOT NULL,
    Posicion INT NOT NULL
);
GO

-- El ticket. CodigoTicket es un GUID: con el se paga, se cancela o se reimprime.
CREATE TABLE APUESTAS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CodigoTicket UNIQUEIDENTIFIER NOT NULL UNIQUE,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
    FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Vigente'
        CHECK (Estado IN ('Vigente', 'Pagada', 'Anulada'))
);
GO

-- Cada jugada del ticket, siempre sobre UN sorteo. Tipo (Directa o Redoblona) solo se
-- usa en Quiniela; EsAutomatica indica si los numeros los eligio la maquina.
CREATE TABLE JUGADAS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdApuesta INT NOT NULL FOREIGN KEY REFERENCES APUESTAS(Id),
    IdSorteo INT NOT NULL FOREIGN KEY REFERENCES SORTEOS(Id),
    Tipo VARCHAR(20) NULL CHECK (Tipo IS NULL OR Tipo IN ('Directa', 'Redoblona')),
    EsAutomatica BIT NOT NULL DEFAULT 0,
    Monto DECIMAL(10,2) NOT NULL,
    Orden INT NOT NULL
);
GO

-- Numeros elegidos de cada jugada. Rango es la posicion buscada (solo Quiniela):
-- 1 = a la primera, 5, 10 o 20. En la redoblona cada numero tiene la suya.
CREATE TABLE JUGADA_NUMERO (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdJugada INT NOT NULL FOREIGN KEY REFERENCES JUGADAS(Id),
    Numero VARCHAR(4) NOT NULL,
    Orden INT NOT NULL,
    Rango INT NULL CHECK (Rango IS NULL OR Rango IN (1, 5, 10, 20))
);
GO

-- Cobro de un ticket premiado (un ticket se paga una sola vez).
CREATE TABLE PAGOS (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdApuesta INT NOT NULL UNIQUE FOREIGN KEY REFERENCES APUESTAS(Id),
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES USUARIOS(Id),
    FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
    Monto DECIMAL(10,2) NOT NULL
);
GO

INSERT INTO JUEGOS (Nombre, Codigo, CantidadNumeros, NumeroMin, NumeroMax, CantidadSorteados, UsaRango, UsaLoteriaTurno, Precio, Activo) VALUES
    ('Quiniela',      'QUINIELA',  1, 0, 9999, 20, 1, 1, NULL, 1),
    ('Quiniela Plus', 'QPLUS',     8, 0,   99, 20, 0, 0,  500, 1),
    ('Quini 6',       'QUINI6',    6, 0,   45,  6, 0, 0, 1000, 1),
    ('Loto 5',        'LOTO5',     5, 1,   36,  5, 0, 0,  800, 1),
    ('Telekino',      'TELEKINO', 15, 1,   25, 15, 0, 0, 1500, 1);
GO

INSERT INTO LOTERIAS (Nombre, Codigo, Activa) VALUES
    ('Provincia', 'PBA', 1),
    ('La Ciudad', 'CABA', 1),
    ('Cordoba', 'COR', 1),
    ('Santa Fe', 'SFE', 1),
    ('Entre Rios', 'ERI', 1);
GO

INSERT INTO TURNOS (Nombre, Activo) VALUES
    ('Primera', 1),
    ('Matutina', 1),
    ('Vespertina', 1),
    ('Nocturna', 1);
GO
