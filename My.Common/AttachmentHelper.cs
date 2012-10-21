#region usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;


#endregion



namespace My.Common
{
    public static class AttachmentHelper
    {
        public static Attachment CreateAttachment(Stream stream, string displayName)
        {
            const TransferEncoding transferEncoding = TransferEncoding.Base64;
            Attachment attachment = new Attachment(stream, displayName, MediaTypeNames.Application.Octet);
            attachment.TransferEncoding = transferEncoding;

            string tranferEncodingMarker;
            string encodingMarker;
            int maxChunkLength;

            switch (transferEncoding)
            {
                // ReSharper disable CSharpWarnings::CS0162
                case TransferEncoding.Base64:
                    tranferEncodingMarker = "B";
                    encodingMarker = "UTF-8";
                    maxChunkLength = 30;
                    break;
                case TransferEncoding.QuotedPrintable:
                    tranferEncodingMarker = "Q";
                    encodingMarker = "ISO-8859-1";
                    maxChunkLength = 76;
                    break;
                default:
                    throw (new ArgumentException(string.Format("The specified TransferEncoding is not supported: {0}", transferEncoding, "transferEncoding")));
                    // ReSharper restore CSharpWarnings::CS0162
            }

            attachment.NameEncoding = Encoding.GetEncoding(encodingMarker);

            string encodingtoken = string.Format("=?{0}?{1}?", encodingMarker, tranferEncodingMarker);
            const string softbreak = "?=";
            string encodedAttachmentName;

            if (attachment.TransferEncoding == TransferEncoding.QuotedPrintable)
                encodedAttachmentName = HttpUtility.UrlEncode(displayName, Encoding.Default).Replace("+", " ").Replace("%", "=");
            else
                encodedAttachmentName = Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName));

            encodedAttachmentName = SplitEncodedAttachmentName(encodingtoken, softbreak, maxChunkLength, encodedAttachmentName);
            attachment.Name = encodedAttachmentName;

            return attachment;
        }


        static string SplitEncodedAttachmentName(string encodingtoken, string softbreak, int maxChunkLength, string encoded)
        {
            int splitLength = maxChunkLength - encodingtoken.Length - (softbreak.Length*2);
            IEnumerable<string> parts = SplitByLength(encoded, splitLength);

            string encodedAttachmentName = encodingtoken;

            foreach (string part in parts)
                encodedAttachmentName += part + softbreak + encodingtoken;

            encodedAttachmentName = encodedAttachmentName.Remove(encodedAttachmentName.Length - encodingtoken.Length, encodingtoken.Length);
            return encodedAttachmentName;
        }


        static IEnumerable<string> SplitByLength(string stringToSplit, int length)
        {
            while (stringToSplit.Length > length)
            {
                yield return stringToSplit.Substring(0, length);
                stringToSplit = stringToSplit.Substring(length);
            }

            if (stringToSplit.Length > 0) yield return stringToSplit;
        }
    }
}