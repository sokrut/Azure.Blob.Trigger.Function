using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace FunctionApp1
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run([BlobTrigger("uploadedfiles/{name}", Connection = "")] BlockBlobClient blob, string name)
        {
            var properties = await blob.GetPropertiesAsync();
            var metadata = properties.Value.Metadata;
            metadata.TryGetValue("email", out var email);

            var sasToken = blob.GenerateSasUri(BlobSasPermissions.All, DateTimeOffset.UtcNow.AddHours(1));

            using (MailMessage mail = new MailMessage("berladtar@gmail.com", email))
            {

                mail.Subject = "UploadFile";
                mail.Body = sasToken.AbsoluteUri;
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential("berladtar@gmail.com", "gtyqewxpdcaxvacg");
                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                }
            }
        }
    }
}
