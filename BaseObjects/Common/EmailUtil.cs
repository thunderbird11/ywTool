using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects.Common
{
    public class EmailUtil
    {
        public static bool SendEmail(List<string> to, string subject, string body,
            string smtp, int port, string username, string password, string from ,
            List<string> bcc = null, string format = "HTML", List<string> attachments = null, List<Attachment> inlineattachments =null )
        {
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();
            client.Port = port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            client.Host = smtp;
            client.EnableSsl = true;
            mail.Subject = subject;
            foreach (var t in to)
                mail.To.Add(new MailAddress(t));
            if (bcc != null)
                foreach (var t in bcc)
                    mail.Bcc.Add(new MailAddress(t));

            mail.From = new MailAddress(from);
            mail.Body = body;
            mail.IsBodyHtml = format.ToLower() == "html";
            if (attachments != null)
            {
                foreach (var t in attachments)
                    mail.Attachments.Add(new Attachment(t));
            }
            if (inlineattachments != null)
            {
                foreach (var t in inlineattachments)
                    mail.Attachments.Add(t);
            }
            client.Send(mail);
            return true;
        }
    }
}
