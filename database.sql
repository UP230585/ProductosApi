IF DB_ID('ProductosDb') IS NULL
    CREATE DATABASE ProductosDb;
GO

USE ProductosDb;
GO

IF OBJECT_ID('dbo.Productos') IS NULL
BEGIN
    CREATE TABLE dbo.Productos (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(100)  NOT NULL,
        Descripcion NVARCHAR(255)  NULL,
        Precio      DECIMAL(10,2)  NOT NULL,
        Stock       INT            NOT NULL DEFAULT 0
    );

    INSERT INTO dbo.Productos (Nombre, Descripcion, Precio, Stock) VALUES
    (N'Teclado mecánico', N'Teclado con switches rojos', 899.00, 15),
    (N'Mouse inalámbrico', N'Mouse óptico 1600 DPI', 349.50, 40),
    (N'Monitor 24"', N'Monitor Full HD IPS', 2799.99, 8);
END
GO
