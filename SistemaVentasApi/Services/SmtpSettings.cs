namespace SistemaVentasApi.Services
{
    /// <summary>
    /// Configuración del servidor de correo (sección "Smtp" de appsettings).
    /// Usuario y contraseña se cargan con user-secrets o variables de entorno, nunca en el repositorio.
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = "Sistema de Ventas";

        public bool EstaConfigurado =>
            !string.IsNullOrWhiteSpace(Host) &&
            !string.IsNullOrWhiteSpace(User) &&
            !string.IsNullOrWhiteSpace(Password);
    }
}
