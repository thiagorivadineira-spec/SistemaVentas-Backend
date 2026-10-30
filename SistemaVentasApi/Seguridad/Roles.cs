namespace SistemaVentasApi.Seguridad
{
    /// <summary>
    /// Roles del sistema. El valor guardado en la base es el texto de cada constante
    /// (Admin = Administrador).
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Cajero = "Cajero";
        public const string Cliente = "Cliente";

        public const string AdminOCajero = Admin + "," + Cajero;
        public const string TodosLosRoles = Admin + "," + Cajero + "," + Cliente;
    }
}
