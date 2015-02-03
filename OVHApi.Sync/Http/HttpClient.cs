
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

            this.SetHeaders(this.defaultRequestHeaders, httpRequest);
            this.SetHeaders(request.Headers, httpRequest);

            if (request.Content != null)
            {
                this.SetHeaders(request.Content.Headers, httpRequest);

                var bytes = request.Content.GetBytes();
                if (bytes.Length > 0)
                {
                    var requestStream = httpRequest.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                }
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

        private void SetHeaders(WebHeaderCollection collection, HttpWebRequest httpRequest)
        {
            foreach (string header in collection)
            {
                if (header == "Content-Type")
                {
                    httpRequest.ContentType = collection[header];
                }
                else
                {
                    httpRequest.Headers[header] = collection[header];
                }
            }
        }

        private HttpResponseMessage CreateResponse(HttpRequestMessage request, HttpWebResponse httpResponse)
        {
            HttpResponseMessage response;
            response = new HttpResponseMessage(httpResponse.StatusCode);
            response.Request = request;
            response.CopyHeadersFrom(httpResponse.Headers);
            response.StatusCode = httpResponse.StatusCode;

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
