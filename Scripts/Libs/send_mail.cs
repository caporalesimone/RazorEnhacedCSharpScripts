//C#

// Mail Sender Library
// SimonSoft 2021
//
// This is a library for send an email.
// It uses stored_data.cs library to store into filesystem email username, passwors, addresses
// You can edit all those infos in StoredData.json
// Initialize the Email class the first time so it will add a default entry into StoredData.json then change it with your infos
//
// How to use it:
// Email mail = new Email("BestScriptName", "Mail Subject");
// string message = "";
// message += "Hello I am an HTML body<br>";
// message += "my name is <i> FOO </i><br>\n";
// mail.SendEmail(message);

using System;
using System.Net.Mail;
using System.Net.Mime;
using RazorEnhanced;

//#import <stored_data.cs>

namespace Scripts.Libs
{
    public class Email
    {
        private DateTime antiFlood_LastSent;
        private EmailSettings mailSettings;

        private class EmailSettings
        {
            public string SMTP { get; set; }
            public string CredentialUsername { get; set; }
            public string CredentialPassword { get; set; }

            public string[] MailTOs { get; set; }
            public string[] MailCCs { get; set; }

            public string MailFrom { get; set; }

            public EmailSettings()
            {
                // Suggest to create a hotmail.com account and use it as sender address
                // Just add your credentials and all works as it.
                SMTP = "smtp.live.com"; // "smtp-mail.outlook.com";
                CredentialUsername = "senderMail@hotmail.com";
                CredentialPassword = "mypassword";
                MailTOs = new string[1]{ "destinationMail@gmail.com" };
                MailCCs = new string[1] { "" };
                MailFrom = "senderMail@hotmail.com";
            }
        }
        private class EmailContent
        {
            public bool IsHtml { get; set; }
            public string Content { get; set; }
            public string AttachFileName { get; set; }
        }
        private class EmailSendConfigure
        {
            public string[] TOs { get; set; }
            public string[] CCs { get; set; }
            public string From { get; set; }
            public string FromDisplayName { get; set; }
            public string Subject { get; set; }
            public MailPriority Priority { get; set; }
            public string ClientCredentialUserName { get; set; }
            public string ClientCredentialPassword { get; set; }
            public EmailSendConfigure()
            {
            }
        }
        private class EmailManager
        {
            private readonly string m_HostName; // your email SMTP server  

            public EmailManager(string hostName)
            {
                m_HostName = hostName;
            }

            public void SendMail(EmailSendConfigure emailConfig, EmailContent content)
            {
                MailMessage msg = ConstructEmailMessage(emailConfig, content);
                Send(msg, emailConfig);
            }

            // Put the properties of the email including "to", "cc", "from", "subject" and "email body"  
            private MailMessage ConstructEmailMessage(EmailSendConfigure emailConfig, EmailContent content)
            {
                MailMessage msg = new System.Net.Mail.MailMessage();
                foreach (string to in emailConfig.TOs)
                {
                    if (!string.IsNullOrEmpty(to))
                    {
                        msg.To.Add(to);
                    }
                }

                foreach (string cc in emailConfig.CCs)
                {
                    if (!string.IsNullOrEmpty(cc))
                    {
                        msg.CC.Add(cc);
                    }
                }

                msg.From = new MailAddress(emailConfig.From,
                                           emailConfig.FromDisplayName,
                                           System.Text.Encoding.UTF8);
                msg.IsBodyHtml = content.IsHtml;
                msg.Body = content.Content;
                msg.Priority = emailConfig.Priority;
                msg.Subject = emailConfig.Subject;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;

                if (content.AttachFileName != null)
                {
                    Attachment data = new Attachment(content.AttachFileName,
                                                     MediaTypeNames.Application.Zip);
                    msg.Attachments.Add(data);
                }

                return msg;
            }

            //Send the email using the SMTP server  
            private void Send(MailMessage message, EmailSendConfigure emailConfig)
            {
                SmtpClient client = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(
                                      emailConfig.ClientCredentialUserName,
                                      emailConfig.ClientCredentialPassword),
                    Host = m_HostName,
                    Port = 587, // Try 25 if not works with 587
                    EnableSsl = true  // this is critical
                };

                try
                {
                    client.Send(message);
                }
                catch (Exception e)
                {
                    Misc.SendMessage("Error in Send email: " + e.Message);
                    throw;
                }
                message.Dispose();
            }

        }

        private readonly EmailManager mailMan = null;
        private readonly EmailSendConfigure myConfig = null;
        private readonly EmailContent myContent = null;
        private readonly StoredData storedData = new StoredData();

        public Email(string fromName, string subject)
        {
            mailSettings = storedData.GetData<EmailSettings>("mailSettings", StoredData.StoreType.Global);
            if (mailSettings is null)
            {
                Misc.SendMessage("WARNING!\nEmails credentials are not configured. Fill your mail infos into Data\\StoredData.json file", 33);
                mailSettings = new EmailSettings();
                storedData.StoreData(mailSettings, "mailSettings", StoredData.StoreType.Global);
            }

            mailMan = new EmailManager(mailSettings.SMTP);

            myConfig = new EmailSendConfigure
            {
                ClientCredentialUserName = mailSettings.CredentialUsername,
                ClientCredentialPassword = mailSettings.CredentialPassword,
                TOs = mailSettings.MailTOs,
                CCs = mailSettings.MailCCs,
                From = mailSettings.MailFrom,
                FromDisplayName = fromName,
                Priority = System.Net.Mail.MailPriority.Normal,
                Subject = subject,
            };
            myContent = new EmailContent();

            antiFlood_LastSent = DateTime.Now.AddDays(-1);
        }

        public void SendEmail(string message, string subject = "", string attachFileName = "", bool isHtml = true)
        {
            TimeSpan diff = DateTime.Now.Subtract(antiFlood_LastSent);
            if (diff.TotalSeconds < 30)
            {
                Misc.SendMessage("Time between to mail is too short. Mail Dropped", 33);
                return;
            }
            antiFlood_LastSent = DateTime.Now;

            if (subject != null && subject != "")
            {
                myConfig.Subject = subject;
            }

            if (attachFileName != null && attachFileName != "")
            {
                myContent.AttachFileName = attachFileName;
            }

            myContent.Content = message;
            myContent.IsHtml = isHtml;
            mailMan.SendMail(myConfig, myContent);
        }
    }
}
