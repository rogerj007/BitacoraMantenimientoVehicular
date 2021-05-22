using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace BitacoraMantenimientoVehicular.Bot.Helpers
{
    public static class MailExtension
    {
        public static bool SendMail(this string html,string to, string subject )
        {
            try
            {

                var mmMensaje = new MailMessage();


                //Email destino
                mmMensaje.To.Add(to);
                mmMensaje.From = new MailAddress("sysmanespejoutn@gmail.com", subject, Encoding.UTF8);
                //Asunto
                mmMensaje.Subject = subject;
                //Establecer codificación UTF8
                mmMensaje.SubjectEncoding = Encoding.UTF8;
                //Establecer el cuerpo del email
                mmMensaje.Body = html;
                //Establecer la codificación UTF8 del cuerpo del email
                mmMensaje.BodyEncoding = Encoding.UTF8;
                //Habilitar soporte HTML en el correo que se envia
                mmMensaje.IsBodyHtml = true;


                var view = AlternateView.CreateAlternateViewFromString(mmMensaje.Body, Encoding.UTF8, MediaTypeNames.Text.Html);

                mmMensaje.AlternateViews.Add(view);
                mmMensaje.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;


                var scCliente = new SmtpClient
                {
                    Credentials = new NetworkCredential("sysmanespejoutn@gmail.com", "0401696570"),
                    Port = 587,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = int.MaxValue
                };

                scCliente.Send(mmMensaje);
                Console.WriteLine($"Envio correcto: {to}");
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error en envio de correo {exception.Message}");
                return false;
            }

        }

    }
}