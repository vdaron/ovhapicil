
namespace OVHApi
{
    using Newtonsoft.Json;
    using OVHApi.Http;
    using OVHApi.Tools;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    partial class OvhApiClient
    {
        /// <summary>
        /// Request a Consumer Key to the API. That key will need to be validated with the link returned in the answer.
        /// </summary>
        /// <param name="accessRules">list of dictionaries listing the accesses your application will need. Each <see cref="AccessRule"/> is composed of method, of the four HTTP methods, and path, the path you will need access for, with * as a wildcard</param>
        /// <param name="redirectUrl">Url where you want the user to be redirected to after he successfully validates the consumer key</param>
        public CredentialsResponse RequestCredential(IEnumerable<AccessRule> accessRules, string redirectUrl = null)
        {
            Ensure.NotNull("accessRules", accessRules);

            CredentialsRequest cmd = new CredentialsRequest();
            cmd.AccessRules.AddRange(accessRules);
            cmd.Redirection = redirectUrl;

            if (cmd.AccessRules.Count == 0)
                throw new ArgumentException("You must specify at least one accessRule");

            return RawCall<CredentialsResponse>(HttpMethod.Post, "/auth/credential", cmd);
        }

        private void RawCall(HttpMethod method, string path, object content = null)
        {
            var request = BuildHttpRequest(method, path, content);
            var response = _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new OvhException(ParseResponse<Error>(response.Content));
            }
        }

        private T RawCall<T>(HttpMethod method, string path, object content = null)
        {
            var request = BuildHttpRequest(method, path, content);
            var response = _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new OvhException(ParseResponse<Error>(response.Content));
            }

            return ParseResponse<T>(response.Content);
        }

        private HttpRequestMessage BuildHttpRequest(HttpMethod method, string path, object content)
        {
            Uri targetUrl = new Uri(_rootPath + path);
            HttpRequestMessage request = new HttpRequestMessage(method, targetUrl);

            string body = null;
            if (content != null)
            {
                body = JsonConvert.SerializeObject(content, _settings);
                request.Content = new JsonContent(body, Encoding.UTF8);
            }

            if (!String.IsNullOrEmpty(ConsumerKey))
            {
                long now = DateTime.Now.ToUnixTime() - GetTimeDelta();

                //"$1$" + SHA1_HEX(AS+"+"+CK+"+"+METHOD+"+"+QUERY+"+"+BODY+"+"+TSTAMP)
                string hashSource = String.Format(
                    "{0}+{1}+{2}+{3}+{4}+{5}",
                    _applicationSecret,
                    ConsumerKey,
                    method.ToString().ToUpperInvariant(),
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
                var response = _client.GetAsync(_rootPath + "/auth/time");

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

        private T ParseResponse<T>(HttpContent content)
        {
            byte[] responseString = content.GetBytes();
            string strContent = Encoding.UTF8.GetString(responseString);
            return JsonConvert.DeserializeObject<T>(strContent, _settings);
        }
    }
}
