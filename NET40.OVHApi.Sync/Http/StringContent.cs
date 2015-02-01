
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class StringContent : ByteArrayContent
    {
        public StringContent(string content, Encoding encoding)
            : base(StringContent.GetContentByteArray(content, encoding))
        {
            this.Headers[HttpRequestHeader.ContentType] = "text/plain; charset=" + encoding.WebName;
        }

        public StringContent(string content, Encoding encoding, string contentType)
            : base(StringContent.GetContentByteArray(content, encoding))
        {
            this.Headers[HttpRequestHeader.ContentType] = contentType ?? "text/plain; charset=" + encoding.WebName;
        }

        private static byte[] GetContentByteArray(string content, Encoding encoding)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return encoding.GetBytes(content);
        }
    }
}
