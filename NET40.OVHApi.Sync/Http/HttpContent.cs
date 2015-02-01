
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class HttpContent
    {
        ////private readonly Dictionary<string, string> headers = new Dictionary<HttpRequestHeader, string>();
        private readonly WebHeaderCollection headers = new WebHeaderCollection();
        private readonly MemoryStream stream;

        ////public HttpContent()
        ////{
        ////    this.stream new MemoryStream();
        ////}

        public HttpContent(MemoryStream stream)
        {
            this.stream = stream ?? new MemoryStream();
        }

        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        public byte[] GetBytes()
        {
            this.stream.Seek(0L, SeekOrigin.Begin);
            return this.stream.ToArray();
        }
    }
}
