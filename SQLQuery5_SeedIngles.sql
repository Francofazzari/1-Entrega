USE AgenciaQuiniela;
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
