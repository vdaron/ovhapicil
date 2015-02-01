
namespace OVHApi
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Text;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Newtonsoft.Json.Converters;
    using OVHApi.Tools;
    using OVHApi.Http;
    
    partial class OvhApiClient
    {
		/// <summary>
		/// Request a Consumer Key to the API. That key will need to be validated with the link returned in the answer.
		/// </summary>
		/// <param name="accessRules">list of dictionaries listing the accesses your application will need. Each <see cref="AccessRule"/> is composed of method, of the four HTTP methods, and path, the path you will need access for, with * as a wildcard</param>
		/// <param name="redirectUrl">Url where you want the user to be redirected to after he successfully validates the consumer key</param>
		public async Task<CredentialsResponse> RequestCredential(IEnumerable<AccessRule> accessRules,
		                                                         string redirectUrl = null)
		{
			Ensure.NotNull("accessRules",accessRules);

			CredentialsRequest cmd = new CredentialsRequest();
			cmd.AccessRules.AddRange(accessRules);
			cmd.Redirection = redirectUrl;

			if(cmd.AccessRules.Count == 0)
				throw new ArgumentException("You must specify at least one accessRule");

			return await RawCall<CredentialsResponse>(HttpMethod.Post, "/auth/credential", cmd);
		}

        /// <summary>
        /// Get this object properties
        /// <param name="serviceName">The internal name of your SMS offer</param>
        /// <param name="sender">The sms sender</param>
        /// </summary>
        public async Task<OvhApi.Models.Sms.Sender> GetSmsSendersEx(string serviceName, string sender)
        {
            if (ConsumerKey == null)
                throw new OvhException("This request must be authenticated");

            Ensure.NotNullNorEmpty("serviceName", serviceName);
            Ensure.NotNullNorEmpty("sender", sender);

            return await RawCall<OvhApi.Models.Sms.Sender>(
                HttpMethod.Get,
                String.Format(
                    "/sms/{0}/senders/{1}",
                    System.Uri.EscapeDataString(serviceName.ToString()),
                    System.Uri.EscapeDataString(sender.ToString()).Replace("%2B", "+")));
            // fix here ----------------------------------------------/\
            // the api does not accept a escaped '+' sign.
        }

        /// <summary>
        /// Validate a given sender with an activation code.
        /// <param name="code">The validation code</param>
        /// <param name="serviceName">The internal name of your SMS offer</param>
        /// <param name="sender">The sms sender</param>
        /// </summary>
        public async Task CreateSmsSendersValidateEx(string code, string serviceName, string sender)
        {
            if (ConsumerKey == null)
                throw new OvhException("This request must be authenticated");

            Ensure.NotNullNorEmpty("code", code);
            Ensure.NotNullNorEmpty("serviceName", serviceName);
            Ensure.NotNullNorEmpty("sender", sender);

            var requestBody = new Dictionary<string, object>();
            requestBody.Add("code", code);

            await RawCall(
                HttpMethod.Post,
                String.Format(
                    "/sms/{0}/senders/{1}/validate",
                    Uri.EscapeDataString(serviceName.ToString()),
                    Uri.EscapeDataString(sender.ToString())).Replace("%2B", "+"),
                    requestBody); //                             /\
            // fix here -----------------------------------------/
            // the api does not accept a escaped '+' sign.
        }

		/// <summary>
		/// Get the delta between this computer and the OVH cluster to sign further queries
		/// </summary>
		/// <returns>The time delta.</returns>
		private async Task<long> GetTimeDelta()
		{
			if(_timeDelta == null) {
				var response = await _client.GetAsync(_rootPath + "/auth/time");

				if(response.IsSuccessStatusCode) {
					// by calling .Result you are performing a synchronous call
					var responseContent = response.Content; 
					int serverTime = await ParseResponse<int>(responseContent);
					_timeDelta = DateTime.Now.ToUnixTime() - serverTime;
				}
			}
			return _timeDelta ?? 0; // Just in case we cannot retrieve server time
		}

		private async Task<T> RawCall<T>(HttpMethod method,
		                                 string path,
		                                 object content = null)
		{
			var request = await BuildHttpRequest(method, path, content);
			var r = await _client.SendAsync(request);

			if(!r.IsSuccessStatusCode) {
				throw new OvhException(ParseResponse<Error>(r.Content).Result);
			}
			return await ParseResponse<T>(r.Content);
		}

		private async Task RawCall(HttpMethod method,
		                           string path,
		                           object content = null)
		{
			var request = await BuildHttpRequest(method, path, content);
			var r = await _client.SendAsync(request);
			if(!r.IsSuccessStatusCode) {
				throw new OvhException(ParseResponse<Error>(r.Content).Result);
			}
		}



		private async Task<HttpRequestMessage> BuildHttpRequest(HttpMethod method,
		                                                        string path,
		                                                        object content)
		{
			Uri targetUrl = new Uri(_rootPath + path);
			HttpRequestMessage request = new HttpRequestMessage(method, targetUrl);

			string body = null;
			if(content != null) {
				body = JsonConvert.SerializeObject(content, _settings);
				request.Content = new JsonContent(body);
			}

			if (!String.IsNullOrEmpty(ConsumerKey))
			{
				long now = DateTime.Now.ToUnixTime() - await GetTimeDelta();

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

		private async Task<T> ParseResponse<T>(HttpContent content)
		{
			byte[] responseString = await content.ReadAsByteArrayAsync();
			string strContent = Encoding.UTF8.GetString(responseString);
			return JsonConvert.DeserializeObject<T>(strContent, _settings);
		}
    }
}
