IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoriasProductos] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(255) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_CategoriasProductos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Documento] nvarchar(20) NULL,
        [Telefono] nvarchar(30) NULL,
        [Email] nvarchar(100) NULL,
        [Direccion] nvarchar(200) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [Id] int NOT NULL IDENTITY,
        [RazonSocial] nvarchar(100) NOT NULL,
        [Cuit] nvarchar(20) NULL,
        [Telefono] nvarchar(30) NULL,
        [Email] nvarchar(100) NULL,
        [Direccion] nvarchar(200) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Username] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Rol] nvarchar(50) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(255) NULL,
        [PrecioVenta] decimal(18,2) NOT NULL,
        [StockActual] int NOT NULL,
        [PuntoReposicion] int NOT NULL,
        [StockMaximo] int NOT NULL,
        [ImagenUrl] nvarchar(255) NULL,
        [Activo] bit NOT NULL,
        [CategoriaProductoId] int NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Productos_CategoriasProductos_CategoriaProductoId] FOREIGN KEY ([CategoriaProductoId]) REFERENCES [CategoriasProductos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Compras] (
        [Id] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [ProveedorId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        CONSTRAINT [PK_Compras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Compras_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Compras_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [Ventas] (
        [Id] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [ClienteId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        CONSTRAINT [PK_Ventas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ventas_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Ventas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesCompras] (
        [Id] int NOT NULL IDENTITY,
        [CompraId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetallesCompras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesCompras_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesCompras_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE TABLE [DetallesVentas] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [ProductoId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetallesVentas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesVentas_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesVentas_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Ventas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Compras_ProveedorId] ON [Compras] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Compras_UsuarioId] ON [Compras] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesCompras_CompraId] ON [DetallesCompras] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesCompras_ProductoId] ON [DetallesCompras] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesVentas_ProductoId] ON [DetallesVentas] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetallesVentas_VentaId] ON [DetallesVentas] ([VentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaProductoId] ON [Productos] ([CategoriaProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Ventas_ClienteId] ON [Ventas] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Ventas_UsuarioId] ON [Ventas] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911022349_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260911022349_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914000852_AgregarDatosSemillaPrueba'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[CategoriasProductos]'))
        SET IDENTITY_INSERT [CategoriasProductos] ON;
    EXEC(N'INSERT INTO [CategoriasProductos] ([Id], [Activo], [Descripcion], [Nombre])
    VALUES (1, CAST(1 AS bit), N''Mouse, teclados y accesorios'', N''Periféricos''),
    (2, CAST(1 AS bit), N''Discos SSD y dispositivos de almacenamiento'', N''Almacenamiento''),
    (3, CAST(1 AS bit), N''Monitores y pantallas'', N''Monitores''),
    (4, CAST(1 AS bit), N''Equipos y accesorios de red'', N''Redes'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[CategoriasProductos]'))
        SET IDENTITY_INSERT [CategoriasProductos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914000852_AgregarDatosSemillaPrueba'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Cuit', N'Direccion', N'Email', N'RazonSocial', N'Telefono') AND [object_id] = OBJECT_ID(N'[Proveedores]'))
        SET IDENTITY_INSERT [Proveedores] ON;
    EXEC(N'INSERT INTO [Proveedores] ([Id], [Activo], [Cuit], [Direccion], [Email], [RazonSocial], [Telefono])
    VALUES (1, CAST(1 AS bit), N''30-11111111-1'', N''Av. Principal 123'', N''ventas@distribuidoratech.com'', N''Distribuidora Tech SA'', N''2954-123456''),
    (2, CAST(1 AS bit), N''30-22222222-2'', N''Calle Comercio 456'', N''contacto@informaticamayorista.com'', N''Informática Mayorista SRL'', N''2954-654321'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Cuit', N'Direccion', N'Email', N'RazonSocial', N'Telefono') AND [object_id] = OBJECT_ID(N'[Proveedores]'))
        SET IDENTITY_INSERT [Proveedores] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914000852_AgregarDatosSemillaPrueba'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'CategoriaProductoId', N'Descripcion', N'ImagenUrl', N'Nombre', N'PrecioVenta', N'PuntoReposicion', N'StockActual', N'StockMaximo') AND [object_id] = OBJECT_ID(N'[Productos]'))
        SET IDENTITY_INSERT [Productos] ON;
    EXEC(N'INSERT INTO [Productos] ([Id], [Activo], [CategoriaProductoId], [Descripcion], [ImagenUrl], [Nombre], [PrecioVenta], [PuntoReposicion], [StockActual], [StockMaximo])
    VALUES (1, CAST(1 AS bit), 1, N''Mouse óptico USB'', NULL, N''Mouse Logitech M90'', 15000.0, 5, 20, 30),
    (2, CAST(1 AS bit), 1, N''Teclado mecánico RGB'', NULL, N''Teclado Redragon Kumara'', 65000.0, 4, 12, 20),
    (3, CAST(1 AS bit), 2, N''Disco sólido SATA 480 GB'', NULL, N''SSD Kingston 480 GB'', 48000.0, 3, 8, 15),
    (4, CAST(1 AS bit), 4, N''Router Wi-Fi doble banda'', NULL, N''Router TP-Link Archer C6'', 70000.0, 2, 6, 10)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'CategoriaProductoId', N'Descripcion', N'ImagenUrl', N'Nombre', N'PrecioVenta', N'PuntoReposicion', N'StockActual', N'StockMaximo') AND [object_id] = OBJECT_ID(N'[Productos]'))
        SET IDENTITY_INSERT [Productos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914000852_AgregarDatosSemillaPrueba'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914000852_AgregarDatosSemillaPrueba', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915003206_AgregarImagenesProducto'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'ImagenUrl');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Productos] DROP COLUMN [ImagenUrl];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915003206_AgregarImagenesProducto'
)
BEGIN
    CREATE TABLE [Imagenes] (
        [Id] int NOT NULL IDENTITY,
        [NombreOriginal] nvarchar(max) NOT NULL,
        [NombreArchivo] nvarchar(max) NOT NULL,
        [RutaRelativa] nvarchar(max) NOT NULL,
        [TipoContenido] nvarchar(max) NOT NULL,
        [TamanoBytes] bigint NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [ProductoId] int NOT NULL,
        CONSTRAINT [PK_Imagenes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Imagenes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915003206_AgregarImagenesProducto'
)
BEGIN
    CREATE INDEX [IX_Imagenes_ProductoId] ON [Imagenes] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915003206_AgregarImagenesProducto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915003206_AgregarImagenesProducto', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915230252_AgregaDatosSemillaVentas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Direccion', N'Documento', N'Email', N'Nombre', N'Telefono') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] ON;
    EXEC(N'INSERT INTO [Clientes] ([Id], [Activo], [Direccion], [Documento], [Email], [Nombre], [Telefono])
    VALUES (1, CAST(1 AS bit), N''S/D'', N''00000000'', N''cf@sistema.com'', N''Consumidor Final'', N''S/N''),
    (2, CAST(1 AS bit), N''Av. San Martín 123'', N''12345678'', N''thiago@correo.com'', N''Thiago Rivadineira'', N''2954-112233'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Direccion', N'Documento', N'Email', N'Nombre', N'Telefono') AND [object_id] = OBJECT_ID(N'[Clientes]'))
        SET IDENTITY_INSERT [Clientes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915230252_AgregaDatosSemillaVentas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre', N'PasswordHash', N'Rol', N'Username') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [Activo], [Nombre], [PasswordHash], [Rol], [Username])
    VALUES (1, CAST(1 AS bit), N''Administrador del Sistema'', N''123'', N''Admin'', N''admin'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre', N'PasswordHash', N'Rol', N'Username') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915230252_AgregaDatosSemillaVentas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915230252_AgregaDatosSemillaVentas', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [ClienteId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    CREATE TABLE [RegistrosAuditoria] (
        [Id] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [UsuarioId] int NULL,
        [Accion] nvarchar(50) NOT NULL,
        [Entidad] nvarchar(50) NULL,
        [EntidadId] int NULL,
        [Detalle] nvarchar(500) NULL,
        [DireccionIp] nvarchar(45) NULL,
        CONSTRAINT [PK_RegistrosAuditoria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RegistrosAuditoria_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    EXEC(N'UPDATE [Usuarios] SET [ClienteId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Usuarios_ClienteId] ON [Usuarios] ([ClienteId]) WHERE [ClienteId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Username] ON [Usuarios] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    CREATE INDEX [IX_RegistrosAuditoria_Fecha] ON [RegistrosAuditoria] ([Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    CREATE INDEX [IX_RegistrosAuditoria_UsuarioId] ON [RegistrosAuditoria] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    ALTER TABLE [Usuarios] ADD CONSTRAINT [FK_Usuarios_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029000155_VincularUsuarioClienteYAuditoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029000155_VincularUsuarioClienteYAuditoria', N'10.0.12');
END;

COMMIT;
GO

