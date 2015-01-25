
namespace OVHApi.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class HttpClient
    {
        private readonly WebHeaderCollection defaultRequestHeaders = new WebHeaderCollection();

        public HttpClient()
        {
        }

        public WebHeaderCollection DefaultRequestHeaders
        {
            get { return this.defaultRequestHeaders; }
        }

        public HttpResponseMessage GetAsync(string uri)
        {
            return this.GetAsync(new Uri(uri));
        }

        private HttpResponseMessage GetAsync(Uri uri)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
        }

        public HttpResponseMessage SendAsync(HttpRequestMessage request)
        {
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(request.RequestUri);
            httpRequest.Method = request.Method.ToString().ToUpperInvariant();

            foreach (string header in this.defaultRequestHeaders)
            {
                httpRequest.Headers.Set(header, this.defaultRequestHeaders[header]);
            }

            foreach (string header in request.Headers)
            {
                httpRequest.Headers.Set(header, request.Headers[header]);
            }

            if (request.Content != null)
            {
                foreach (string header in request.Content.Headers)
                {
                    httpRequest.Headers.Set(header, request.Content.Headers[header]);
                }

                var requestStream = httpRequest.GetRequestStream();
                var bytes = request.Content.GetBytes();
                requestStream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse httpResponse;
            HttpResponseMessage response;
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                response = this.CreateResponse(request, httpResponse);
                return response;
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
                response = this.CreateResponse(request, httpResponse);
                return response;
            }
        }

        private HttpResponseMessage CreateResponse(HttpRequestMessage request, HttpWebResponse httpResponse)
        {
            HttpResponseMessage response;
            response = new HttpResponseMessage(httpResponse.StatusCode);
            response.Request = request;
            response.CopyHeadersFrom(httpResponse.Headers);

            MemoryStream memory;
            var contentLength = response.Headers[HttpResponseHeader.ContentLength];
            int contentIntLength;
            if (int.TryParse(contentLength, out contentIntLength))
            {
                memory = new MemoryStream(contentIntLength);
            }
            else
            {
                memory = new MemoryStream();
            }

            var responseStream = httpResponse.GetResponseStream();
            responseStream.CopyTo(memory);
            memory.Seek(0L, SeekOrigin.Begin);

            response.Content = new HttpContent(memory);
            return response;
        }
    }
}
