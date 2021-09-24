using LightBuzz.SMTP;
using Models.MotionDetector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage.Streams;

namespace MotionDetector.Utilities
{
    public static class SMTPServices
    {
        public static async Task<SmtpResult> SendAlertEmail(List<IRandomAccessStream> streams, ConfigModel ConfigurationSettings)
        {
            string recipient = ConfigurationSettings.SmtpSettings.Recipient ?? String.Empty;
            using (SmtpClient client = new SmtpClient(ConfigurationSettings.SmtpSettings.SmtpServer,
                                                      ConfigurationSettings.SmtpSettings.SmtpPort,
                                                      ConfigurationSettings.SmtpSettings.UseSSL,
                                                      ConfigurationSettings.SmtpSettings.SmtpUserName,
                                                      ConfigurationSettings.SmtpSettings.SmtpPassword))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.Importance = EmailImportance.High;
                emailMessage.Sender.Address = "UniversalMotionDetector@donotreply.com";
                emailMessage.Sender.Name = "Universal Motion Detector Elite";
                emailMessage.To.Add(new EmailRecipient(recipient));
                emailMessage.Subject = "ALERT | MOTION DETECTED";

                emailMessage.Body = "Alert detected at " + DateTime.Now;

                int imageCount = 1;
                foreach (IRandomAccessStream stream in streams)
                {
                    string fileName = "image_" + imageCount + ".png";
                    RandomAccessStreamReference reference = RandomAccessStreamReference.CreateFromStream(stream);
                    emailMessage.Attachments.Add(new EmailAttachment(fileName, reference));
                    imageCount++;
                }

                streams.Clear();
                SmtpResult result = await client.SendMailAsync(emailMessage);

                return result;
            }
        }

        public static async void RunSMTPTest(ConfigModel ConfigurationSettings, Action callback)
        {
            if (String.IsNullOrEmpty(ConfigurationSettings.SmtpSettings.Recipient))
            {
                callback();
            }
            else
            {
                using (SmtpClient client = new SmtpClient(ConfigurationSettings.SmtpSettings.SmtpServer,
                                          ConfigurationSettings.SmtpSettings.SmtpPort,
                                          ConfigurationSettings.SmtpSettings.UseSSL,
                                          ConfigurationSettings.SmtpSettings.SmtpUserName,
                                          ConfigurationSettings.SmtpSettings.SmtpPassword))
                {
                    EmailMessage emailMessage = new EmailMessage();
                    emailMessage.Subject = "TEST | ALERT | MOTION DETECTED";
                    emailMessage.Importance = EmailImportance.High;
                    emailMessage.Sender.Address = "UniversalMotionDetector@donotreply.com";
                    emailMessage.Sender.Name = "Universal Motion Detector Elite";
                    emailMessage.To.Add(new EmailRecipient(ConfigurationSettings.SmtpSettings.Recipient));
    
                    SmtpResult result = await client.SendMailAsync(emailMessage);
                    callback();
                }
            }
        }
    }
}
