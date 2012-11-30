#region usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

using JetBrains.Annotations;

using NLog;


#endregion

#pragma warning disable 1573



namespace My.Common
{
    public class MailSender
    {
        static readonly Logger Logger = LogManager.GetLogger("MailSender");
        const int EmailSizeLimit = 15000000;


        /// <param name="attachments"> collection of ( (file name, file extension), body ) </param>
        /// <param name="messageBody"> html </param>
        /// <param name="sendIfBigFiles"> If files total size more then allowed limit, so send mail but not all files - only several from them </param>
        /// <exception cref="SmtpFailedRecipientsException"></exception>
        /// <exception cref="SmtpException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns> returns error desc </returns>
        public string SendMessage([NotNull] string mailFrom, [NotNull] string mailTo, [NotNull] string messageBody, [NotNull] string messageTitle,
                                  List<KeyValuePair<Tuple<string, string>, byte[]>> attachments = null, bool sendIfBigFiles = true)
        {
            int emailId = GetEmailId(mailFrom, mailTo, messageBody, messageTitle);

            MailMessage objEmail = new MailMessage();

            if (!string.IsNullOrEmpty(mailTo))
            {
                string[] emails = mailTo.Split(new[] {";", ",", " "}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string email in emails)
                    try
                    {
                        objEmail.To.Add(new MailAddress(email.Trim()));
                    }
                    catch (FormatException ex)
                    {
                        Logger.Error("Email [{0}] is wrong. Whole email=[{2}]. messageTitle=[{1}]", email, messageTitle, mailTo);
                        return SetStatusException(mailTo, email, emailId, ex);
                    }
            }

            if (objEmail.To.Count > 0)
            {
                objEmail.From = new MailAddress(mailFrom);
                objEmail.IsBodyHtml = true;
                objEmail.Subject = messageTitle;
                objEmail.Body = messageBody;
                objEmail.Priority = MailPriority.High;
                objEmail.SubjectEncoding = Encoding.UTF8;

                SmtpClient smtp = new SmtpClient();

                if (attachments != null)
                {
                    int totalSize = 0;
                    KeyValuePair<Tuple<string, string>, byte[]>[] ats = attachments
                            .Where(x => x.Key != null && x.Value != null && x.Value.Length > 0)
                            .TakeWhile(a => (totalSize += a.Value.Length) < EmailSizeLimit - objEmail.Body.Length*2 - objEmail.Subject.Length*2)
                            .ToArray();
                    if (ats.Length < attachments.Count && !sendIfBigFiles)
                        throw new Exception("files size too big");

                    Logger.Info("{0} attachmets, total size {1}; was {2}, total size {3}", ats.Count(), ats.Sum(a => a.Value.Length),
                                attachments.Count, attachments.Sum(a => a.Value.Length));
                    foreach (KeyValuePair<Tuple<string, string>, byte[]> a in ats)
                        objEmail.Attachments.Add(AttachmentHelper.CreateAttachment(new MemoryStream(a.Value),
                                                                                   a.Key.Item1.EndsWith(a.Key.Item2) ? a.Key.Item1
                                                                                           : (a.Key.Item1 + (a.Key.Item2.StartsWith(".") ? "" : ".")
                                                                                              + a.Key.Item2)));
                }
                try
                {
                    smtp.Send(objEmail);
                    Logger.Trace("sent ok");
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("smtp.Send", ex);
                    return SetStatusException(mailTo, null, emailId, ex);
                }
            }
            SetStatusOk(emailId);

            return null;
        }


        protected virtual void SetStatusOk(int emailId) {}


        protected virtual string SetStatusException(string mailTo, string email, int emailId, Exception ex)
        {
            return null;
        }


        protected virtual int GetEmailId(string mailFrom, string mailTo, string messageBody, string messageTitle)
        {
            return 0;
        }
    }
}