# SistemaVentasApi

Copia independiente de la API del TP5, pensada para ser el backend compartido del TP6 (móvil) y el TP3 (web).
No comparte base de datos, clave JWT ni puertos con el TP5 original.

## Qué cambió respecto al TP5
- Proyecto, solución y namespaces: `TP5ProgramacionMovil` -> `SistemaVentasApi`.
- Base de datos nueva: `SistemaVentasApiDb` (la del TP5 era `SistemaVentasDb`).
- JWT propio: Issuer `SistemaVentasApi`, Audience `SistemaVentasClientes`. Los tokens del TP5 no valen acá.
- La clave JWT de desarrollo está en `appsettings.Development.json` (generada al azar).
  En producción, definirla con la variable de entorno `Jwt__Key` (mínimo 32 caracteres).
  Si queda la clave de ejemplo, la API no arranca.
- Puertos: http 5141 / https 7173 (el TP5 usaba 5041 / 7073), así se pueden correr los dos a la vez.
- No se copiaron las imágenes de prueba de `wwwroot/uploads`.
- Sin cambios en la lógica de los controladores ni en el modelo de datos (mismas migraciones).

## Crear la base de datos
Desde la carpeta `SistemaVentasApi/SistemaVentasApi` (la que tiene el `.csproj`):

    dotnet ef database update

o, en Visual Studio, Consola del Administrador de paquetes: `Update-Database`.
Esto crea `SistemaVentasApiDb` en `localhost\SQLEXPRESS` con las tablas y los datos semilla.
Usuario semilla: `admin` / `123` (solo para desarrollo).

## Pendiente (no incluido todavía)
- Hash de contraseñas y restringir `Auth/Registro`.
- Roles definitivos (Cliente / Cajero / Administrador) y políticas por endpoint.
- Relación Usuario <-> Cliente (requiere migración nueva).
- Serilog, QuestPDF, MailKit.
- Hosting: base SQL Server accesible desde internet y almacenamiento de imágenes que no se borre al redesplegar.
