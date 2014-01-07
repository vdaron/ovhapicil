using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography;
using OVHApi.Tools;

namespace OVHApi
{
	public enum OvhInfra
	{
		Europe, Canada
	}

	public partial class OvhApiClient
	{
		class JsonContent: StringContent
		{
			public JsonContent(string str)
				:base(str)
			{
				base.Headers.ContentType.MediaType = "application/json";
			}
		}

		private static readonly Uri OVH_API_EU = new Uri("https://api.ovh.com/1.0");         // Root URL of OVH european API
		private static readonly Uri OVH_API_CA = new Uri("https://ca.api.ovh.com/1.0");      // Root URL of OVH canadian API

		private readonly string _applicationKey;
		private readonly string _applicationSecret;
		private readonly Uri _rootPath;
		private readonly HttpClient _client;
		private readonly SHA1 _sha = SHA1.Create();
		private readonly JsonSerializerSettings _settings = new JsonSerializerSettings();

		private long? _timeDelta = null;

		public OvhApiClient(string applicationKey,
		                    string applicationSecret,
		                    OvhInfra infrastructure = OvhInfra.Europe,
		                    string consumerKey = null)
		{
			Ensure.NotNullNorEmpty("applicationKey", applicationKey);
			Ensure.NotNullNorEmpty("applicationSecret", applicationSecret);

			_settings.Converters.Add(new IPAddressConverter());
			_settings.Formatting = Formatting.Indented;

			_applicationKey = applicationKey;
			_applicationSecret = applicationSecret;
			ConsumerKey = consumerKey;
			_rootPath = infrastructure == OvhInfra.Europe ? OVH_API_EU : OVH_API_CA;
			_client = new HttpClient();
			_client.DefaultRequestHeaders.Add("X-Ovh-Application", _applicationKey);

		}

		public string ConsumerKey { get; set; }

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

		#region Private methods

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

		private byte[] GetHash(string inputString)
		{
			return _sha.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		private string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach(byte b in GetHash(inputString))
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		private async Task<T> ParseResponse<T>(HttpContent content)
		{
			byte[] responseString = await content.ReadAsByteArrayAsync();
			string strContent = Encoding.UTF8.GetString(responseString);
			return JsonConvert.DeserializeObject<T>(strContent, _settings);
		}

		#endregion
	}
}

