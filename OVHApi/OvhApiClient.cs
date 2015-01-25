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
			_settings.Converters.Add(new StringEnumConverter());

			_settings.Formatting = Formatting.Indented;

			_applicationKey = applicationKey;
			_applicationSecret = applicationSecret;
			ConsumerKey = consumerKey;
			_rootPath = infrastructure == OvhInfra.Europe ? OVH_API_EU : OVH_API_CA;
			_client = new HttpClient();
			_client.DefaultRequestHeaders.Add("X-Ovh-Application", _applicationKey);

		}

		public string ConsumerKey { get; set; }

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
	}
}

