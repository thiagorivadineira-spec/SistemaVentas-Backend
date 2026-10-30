using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVentasApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregaDatosSemillaVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Activo", "Direccion", "Documento", "Email", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "S/D", "00000000", "cf@sistema.com", "Consumidor Final", "S/N" },
                    { 2, true, "Av. San Martín 123", "12345678", "thiago@correo.com", "Thiago Rivadineira", "2954-112233" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Nombre", "PasswordHash", "Rol", "Username" },
                values: new object[] { 1, true, "Administrador del Sistema", "123", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
