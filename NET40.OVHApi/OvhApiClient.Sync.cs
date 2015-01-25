
namespace OVHApi
{
    using Newtonsoft.Json;
    using OVHApi.Tools;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;

    partial class OvhApiClient
    {
        private void RawCall(HttpMethod method, string path, object content = null)
        {
            var request = BuildHttpRequest(method, path, content);
            var r = await _client.SendAsync(request);
            if (!r.IsSuccessStatusCode)
            {
                throw new OvhException(ParseResponse<Error>(r.Content).Result);
            }
        }

        private HttpRequestMessage BuildHttpRequest(HttpMethod method, string path, object content)
        {
            Uri targetUrl = new Uri(_rootPath + path);
            HttpRequestMessage request = new HttpRequestMessage(method, targetUrl);

            string body = null;
            if (content != null)
            {
                body = JsonConvert.SerializeObject(content, _settings);
                request.Content = new JsonContent(body);
            }

            if (!String.IsNullOrEmpty(ConsumerKey))
            {
                long now = DateTime.Now.ToUnixTime() - GetTimeDelta();

                //"$1$" + SHA1_HEX(AS+"+"+CK+"+"+METHOD+"+"+QUERY+"+"+BODY+"+"+TSTAMP)
                string hashSource = String.Format("{0}+{1}+{2}+{3}+{4}+{5}",
                                                  _applicationSecret,
                                                  ConsumerKey,
                                                  method.Method.ToUpper(),
                                                  targetUrl,
                                                  body,
                                                  now);

                request.Headers.Add("X-Ovh-Signature", "$1$" + GetHashString(hashSource));
                request.Headers.Add("X-Ovh-Consumer", ConsumerKey);
                request.Headers.Add("X-Ovh-Timestamp", now.ToString(CultureInfo.InvariantCulture));
            }

            return request;
        }

        /// <summary>
        /// Get the delta between this computer and the OVH cluster to sign further queries
        /// </summary>
        /// <returns>The time delta.</returns>
        private long GetTimeDelta()
        {
            if (_timeDelta == null)
            {
                var response = _client.get(_rootPath + "/auth/time");

                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    var responseContent = response.Content;
                    int serverTime = ParseResponse<int>(responseContent);
                    _timeDelta = DateTime.Now.ToUnixTime() - serverTime;
                }
            }

            return _timeDelta ?? 0; // Just in case we cannot retrieve server time
        }
    }
}
