using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SistemaVentasApi.Services
{
    public interface IEmailService
    {
        bool EstaConfigurado { get; }

        Task EnviarConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            string nombreArchivo,
            byte[] contenido);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public bool EstaConfigurado => _settings.EstaConfigurado;

        public async Task EnviarConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            string nombreArchivo,
            byte[] contenido)
        {
            var remitente = string.IsNullOrWhiteSpace(_settings.From) ? _settings.User : _settings.From;

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_settings.FromName, remitente));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;

            var body = new BodyBuilder { TextBody = cuerpo };
            body.Attachments.Add(nombreArchivo, contenido, new ContentType("application", "pdf"));
            mensaje.Body = body.ToMessageBody();

            // Puerto 465: SSL directo. Otros (587): STARTTLS.
            var seguridad = _settings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(_settings.Host, _settings.Port, seguridad);
            await cliente.AuthenticateAsync(_settings.User, _settings.Password);
            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);
        }
    }
}
