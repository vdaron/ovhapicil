
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class HttpResponseMessage : HttpMessage
    {
        private HttpStatusCode statusCode;

        public HttpResponseMessage(HttpStatusCode statusCode)
        {

        }

        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
            set { this.statusCode = value; }
        }

        public bool IsSuccessStatusCode
        {
            get { return this.statusCode >= HttpStatusCode.OK && this.statusCode <= (HttpStatusCode)299; }
        }

        public HttpRequestMessage Request { get; set; }
    }
}
