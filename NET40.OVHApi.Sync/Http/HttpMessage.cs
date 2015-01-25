
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class HttpMessage
    {
        private WebHeaderCollection headers;
        private HttpContent content;

        public HttpMessage()
        {
        }

        public WebHeaderCollection Headers
        {
            get { return this.headers ?? (this.headers = new WebHeaderCollection()); }
        }

        public HttpContent Content
        {
            get { return this.content; }
            set { this.content = value; }
        }

        internal void CopyHeadersFrom(WebHeaderCollection collection)
        {
            var headers = this.Headers;
            foreach (string header in collection)
            {
                headers.Set(header, collection[header]);
            }
        }
    }
}
