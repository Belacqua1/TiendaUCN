using Resend;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Service responsible for sending emails such as verification codes and welcome messages.
    /// Uses the Resend email client and supports HTML templates with dynamic placeholders.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of <see cref="EmailService"/>.
        /// </summary>
        /// <param name="resend">Resend email client for sending emails.</param>
        /// <param name="configuration">Configuration to read email settings.</param>
        /// <param name="webHostEnvironment">Web host environment to locate templates.</param>
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

        /// <summary>
        /// Generates and sends a verification code email to a user.
        /// Loads the VerificationCode HTML template and replaces placeholders.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="code">Verification code to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendVerificationCodeEmailAsync(string email, string code, string nameHtml)
        {
            Console.WriteLine(
                $"[EMAIL DEBUG] Entrando a SendVerificationCodeEmailAsync para {email}"
            );

            // Prepare template variables
            var variables = new Dictionary<string, string> { { "CODE", code } };

            // Load HTML template and replace placeholders
            var htmlBody = await LoadTemplate(nameHtml, variables);

            // Build the email message
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

            // Send the email
            await _resend.EmailSendAsync(message);

            Console.WriteLine($"[EMAIL DEBUG] EmailSendAsync ejecutado para {email}");
        }

        /// <summary>
        /// Sends a welcome email to a user after successful registration.
        /// Loads the Welcome HTML template and replaces placeholders.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendWelcomeEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(
                    nameof(email),
                    "El correo no puede ser nulo o vacío"
                );

            // Prepare template variables
            var variables = new Dictionary<string, string>
            {
                { "NAME", "Usuario" },
                {
                    "URL_LOGIN",
                    _configuration["EmailConfiguration:LoginUrl"] ?? "https://tienda-ucn.cl/login"
                },
            };

            // Load HTML template
            var htmlBody = await LoadTemplate("Welcome", variables);

            // Build and send the email
            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:WelcomeSubject"]!,
                From = _configuration["EmailConfiguration:From"]!,
                HtmlBody = htmlBody,
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Loads an HTML email template from disk and replaces placeholders with provided variables.
        /// </summary>
        /// <param name="templateName">Name of the template file without extension.</param>
        /// <param name="variables">Optional dictionary of placeholder replacements.</param>
        /// <returns>The HTML content with placeholders replaced.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the template file cannot be found.</exception>
        private async Task<string> LoadTemplate(
            string templateName,
            Dictionary<string, string>? variables = null
        )
        {
            // Build full template path
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

            // Read template content
            var html = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders with provided variables
            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    // Replace placeholders like {{CODE}}, {{NAME}}, {{URL_LOGIN}}
                    html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                }
            }

            return html;
        }
    }
}
