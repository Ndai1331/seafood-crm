using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Core.Exceptions;

namespace Application.Helpers
{
    public class SendingEmail
    {
        private SmtpClient SmtpClient { get; set; } = new SmtpClient();
        public EmailHostConfig Config { get; set; }
        
        public SendingEmail(EmailHostConfig config)
        { 
            Config = config;
           SmtpClient =  GetSmtpConnect(Config);
        }

   
        

        public async Task<bool> SendContent(EmailSendingContent content,bool isCheckConnect = true)
        {
            
            var fromAddress = new MailAddress(Config.From, Config.Sender);
            var message = new MailMessage();
            message.From = fromAddress;
            message.IsBodyHtml = true;
            message.Body = content.Body;
            message.Subject = content.Subject;
            foreach(var email in content.ToEmails)
            {
                message.To.Add(email);
            }
            SmtpClient.Send(message);
            return true;
        }
        
        private SmtpClient GetSmtpConnect(EmailHostConfig  config)
        {
            var smtp =  ConfigEmail(config);
            var fromAddress = new MailAddress(config.From, config.Sender);
            var message = new MailMessage();
            message.From = fromAddress;
            try
            {
                var mailTest = new MailAddress(fromAddress.Address);
                message.Body = "Check-SendingEmail";
                message.To.Add(mailTest);
                smtp.Send(message);
                return smtp;
            }
            catch (Exception e)
            {
                throw new GlobalException("Setting  wrong email config!",HttpStatusCode.BadRequest);
            }
        }
        
        
        private SmtpClient ConfigEmail(EmailHostConfig  config)
        {
            var fromAddress = new MailAddress(config.From, config.Sender);
            var smtp = new SmtpClient
            {
                Host = config.Host,
                Port = config.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, config.Password)
            };
         
            return smtp;
        }
        
         

    }

    public class EmailHostConfig
    {
        public string From { get; set;}
        public string Sender { get; set; }
        public string Host { get; set; }
        public int Port { get; set;}
        public string Password { get; set; }
    }
    
    public class EmailSendingContent {
        public string Body { get; set; }
        public List<string> ToEmails { get; set; } 
        public string Subject { get; set; }
    }

 
}