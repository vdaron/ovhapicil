
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class HttpRequestMessage : HttpMessage
    {
        private HttpMethod method;
        private Uri requestUri;

        public HttpRequestMessage(HttpMethod method, Uri requestUri)
        {
            this.method = method;
            this.requestUri = requestUri;
        }

        public HttpMethod Method
        {
            get { return this.method; }
        }

        public Uri RequestUri
        {
            get { return this.requestUri; }
        }
    }
}
