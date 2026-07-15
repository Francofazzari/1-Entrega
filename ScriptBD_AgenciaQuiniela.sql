USE AgenciaQuiniela;
GO

-- Eliminar tablas viejas si existen para no tener conflictos
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