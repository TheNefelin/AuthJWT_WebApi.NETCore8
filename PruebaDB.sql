CREATE TABLE Usuarios (
    Id NVARCHAR(50) PRIMARY KEY,
    Usuario NVARCHAR(50) NOT NULL,
	Email NVARCHAR(50) NOT NULL,
    AuthHash VARBINARY(64) NOT NULL,
    AuthSalt VARBINARY(16) NOT NULL
)
GO

CREATE PROCEDURE AuthCrearUsuario
	@Id NVARCHAR(50),
    @usuario NVARCHAR(50),
    @clave NVARCHAR(50)
AS
BEGIN
    -- Generar una sal aleatoria
    DECLARE @salt VARBINARY(16);
    SET @salt = CRYPT_GEN_RANDOM(16);

    -- Hash de la contraseña con la sal
    DECLARE @hash VARBINARY(64);
    SET @hash = HASHBYTES('SHA2_256', @clave + CAST(@salt AS NVARCHAR(32)));

	IF NOT EXISTS (SELECT id FROM Usuarios WHERE Email = @usuario)
		BEGIN
		    -- Insertar el usuario en la tabla
			INSERT INTO Usuarios (Id, Usuario, Email, AuthHash, AuthSalt)
			VALUES (@Id, @usuario, @usuario, @hash, @salt);
			
			SELECT 1 AS Estado, 'Usuario Creado Correctamente' AS Msge
			RETURN
		END

	SELECT 0 AS Estado, 'Usuario Ya Existe' AS Msge
END
GO

CREATE PROCEDURE AuthIniciarSesion
    @usuario NVARCHAR(50),
    @clave NVARCHAR(50)
AS
BEGIN
    DECLARE @hash VARBINARY(64);
    DECLARE @salt VARBINARY(16);

    -- Obtener el hash y la sal de la base de datos para el usuario dado
    SELECT @hash = AuthHash, @salt = AuthSalt
    FROM Usuarios
    WHERE Email = @Usuario;

    -- Calcular el hash de la contraseña proporcionada con la sal almacenada
    DECLARE @hashProvidedPassword VARBINARY(64);
    SET @hashProvidedPassword = HASHBYTES('SHA2_256', @clave + CAST(@salt AS NVARCHAR(32)));

    -- Comparar los hashes
    IF @hash = @hashProvidedPassword
		BEGIN
			SELECT 1 AS Estado, 'Inicio de sesión exitoso.' AS Msge
			RETURN
		END

	SELECT 0 AS Estado, 'Inicio de sesión fallido.' AS Msge
END

EXECUTE AuthCrearUsuario 'A123', 'user@example.com', 'string'
EXECUTE AuthIniciarSesion 'user@example.com', 'string'

SELECT * FROM Usuarios
--DROP TABLE Usuarios
