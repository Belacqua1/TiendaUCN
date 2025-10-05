using Resend;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(
            IResend resend,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _resend = resend;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendVerificationCodeEmailAsync(string email, string code)
        {
            Console.WriteLine(
                $"[EMAIL DEBUG] Entrando a SendVerificationCodeEmailAsync para {email}"
            );

            // Cargar la plantilla y reemplazar {{CODE}}
            var variables = new Dictionary<string, string> { { "CODE", code } };
            var htmlBody = await LoadTemplate("VerificationCode", variables);

            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:VerificationSubject"],
                From = _configuration["EmailConfiguration:From"],
                HtmlBody = htmlBody,
            };

            Console.WriteLine(
                $"[EMAIL DEBUG] Construido mensaje: {message.Subject} desde {message.From} a {message.To}"
            );

            await _resend.EmailSendAsync(message);

            Console.WriteLine($"[EMAIL DEBUG] EmailSendAsync ejecutado para {email}");
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(
                    nameof(email),
                    "El correo no puede ser nulo o vacío"
                );

            var variables = new Dictionary<string, string>
            {
                { "NAME", "Usuario" },
                {
                    "URL_LOGIN",
                    _configuration["EmailConfiguration:LoginUrl"] ?? "https://tienda-ucn.cl/login"
                },
            };

            var htmlBody = await LoadTemplate("Welcome", variables);

            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:WelcomeSubject"]!,
                From = _configuration["EmailConfiguration:From"]!,
                HtmlBody = htmlBody,
            };

            await _resend.EmailSendAsync(message);
        }

        private async Task<string> LoadTemplate(
            string templateName,
            Dictionary<string, string>? variables = null
        )
        {
            var templatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "src",
                "Application",
                "Resources",
                "Templates",
                "Email",
                $"{templateName}.html"
            );

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"No se encontró la plantilla: {templatePath}");

            var html = await File.ReadAllTextAsync(templatePath);

            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    // Reemplaza todos los placeholders tipo {{CODE}}, {{NAME}}, {{URL_LOGIN}}, etc.
                    html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                }
            }

            return html;
        }
    }
}
