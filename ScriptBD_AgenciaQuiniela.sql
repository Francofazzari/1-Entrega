USE AgenciaQuiniela;
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