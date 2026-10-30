using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVentasApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosSemillaPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CategoriasProductos",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Mouse, teclados y accesorios", "Periféricos" },
                    { 2, true, "Discos SSD y dispositivos de almacenamiento", "Almacenamiento" },
                    { 3, true, "Monitores y pantallas", "Monitores" },
                    { 4, true, "Equipos y accesorios de red", "Redes" }
                });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "Id", "Activo", "Cuit", "Direccion", "Email", "RazonSocial", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "30-11111111-1", "Av. Principal 123", "ventas@distribuidoratech.com", "Distribuidora Tech SA", "2954-123456" },
                    { 2, true, "30-22222222-2", "Calle Comercio 456", "contacto@informaticamayorista.com", "Informática Mayorista SRL", "2954-654321" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaProductoId", "Descripcion", "ImagenUrl", "Nombre", "PrecioVenta", "PuntoReposicion", "StockActual", "StockMaximo" },
                values: new object[,]
                {
                    { 1, true, 1, "Mouse óptico USB", null, "Mouse Logitech M90", 15000m, 5, 20, 30 },
                    { 2, true, 1, "Teclado mecánico RGB", null, "Teclado Redragon Kumara", 65000m, 4, 12, 20 },
                    { 3, true, 2, "Disco sólido SATA 480 GB", null, "SSD Kingston 480 GB", 48000m, 3, 8, 15 },
                    { 4, true, 4, "Router Wi-Fi doble banda", null, "Router TP-Link Archer C6", 70000m, 2, 6, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CategoriasProductos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CategoriasProductos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CategoriasProductos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CategoriasProductos",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
