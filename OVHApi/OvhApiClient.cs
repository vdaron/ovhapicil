using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using ServiceStack.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using OVHApi.Commands.Domain;
using OVHApi.Commands;
using OVHApi.Commands.Dedicated.Server;
using OVHApi.Commands.Me;
using OVHApi.Tools;
using OVHApi.Commands.IP;

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

		private long? _timeDelta = null;

		public OvhApiClient(string applicationKey,
		                    string applicationSecret,
		                    OvhInfra infrastructure = OvhInfra.Europe,
		                    string consumerKey = null)
		{
			Ensure.NotNullNotEmpty("applicationKey", applicationKey);
			Ensure.NotNullNotEmpty("applicationSecret", applicationSecret);

			_applicationKey = applicationKey;
			_applicationSecret = applicationSecret;
			ConsumerKey = consumerKey;
			_rootPath = infrastructure == OvhInfra.Europe ? OVH_API_EU : OVH_API_CA;
			_client = new HttpClient();
			_client.DefaultRequestHeaders.Add("X-Ovh-Application", _applicationKey);

			JsConfig.EmitCamelCaseNames = true;
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

			var response = await _client.PostAsync(_rootPath + "/auth/credential", new JsonContent(cmd.ToJson()));

			response.EnsureSuccessStatusCode();

			// by calling .Result you are performing a synchronous call
			var responseContent = response.Content; 

			CredentialsResponse resp = await ParseResponse<CredentialsResponse>(responseContent);

			return resp;
		}

		public async Task<string[]> GetDomainServices()
		{
			return await RawCall<string[]>(HttpMethod.Get,"/domain/",null);
		}
		public async Task<string[]> GetDomainZones()
		{
			return await RawCall<string[]>(HttpMethod.Get,"/domain/zone",null);
		}
		public async Task<DomainZoneDetail> GetDomainZoneDetail(string zoneName)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);

			return await RawCall<DomainZoneDetail>(HttpMethod.Get,"/domain/zone/{0}".Fmt(zoneName));
		}
		public async Task<long[]> GetDomainRecordIds(string zoneName,
		                                             NamedResolutionFieldType? fieldType = null,
		                                             string subDomain = null)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);

			var query = new QueryString();
			query["fieldType"] = fieldType;
			query["subDomain"] = subDomain;
			string queryString = query.ToString();

			return await RawCall<long[]>(HttpMethod.Get, "/domain/zone/{0}/record{1}".Fmt(zoneName,queryString));
		}
		public async Task<DomainRecordDetail> GetDomainRecord(string zoneName,
		                                                      long recordId)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);
			Ensure.IdIsValid("recordId", recordId);

			return await RawCall<DomainRecordDetail>(HttpMethod.Get,"/domain/zone/{0}/record/{1}".Fmt(zoneName,recordId));
		}
		public async Task<DomainRecordDetail> CreateDomainRecord(string zoneName, NamedResolutionFieldType fieldType, string target, string subDomain = null, long? ttl = null)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);
			Ensure.NotNullNotEmpty("target",target);

			TypedDomainRecord newRecord = new TypedDomainRecord();
			newRecord.FieldType = fieldType;
			newRecord.Target = target;
			newRecord.SubDomain = subDomain;
			newRecord.Ttl = ttl;
			return await RawCall<DomainRecordDetail>(HttpMethod.Post,"/domain/zone/{0}/record".Fmt(zoneName),newRecord);
		}
		public async Task<DomainRecordDetail> UpdateDomainRecord(string zoneName, long recordId, string target, string subDomain = null, long? ttl = null)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);
			Ensure.NotNullNotEmpty("target",target);
			Ensure.IdIsValid("recordId",recordId);

			DomainRecord updateRecordValue = new DomainRecord();
			updateRecordValue.Target = target;
			updateRecordValue.SubDomain = subDomain;
			updateRecordValue.Ttl = ttl;
			await RawCall<string>(HttpMethod.Put,"/domain/zone/{0}/record/{1}".Fmt(zoneName, recordId),updateRecordValue);
			return await GetDomainRecord(zoneName, recordId);
		}
		public async Task DeleteDomainRecord(string zoneName, long recordId)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);
			Ensure.IdIsValid("recordId",recordId);

			await RawCall<string>(HttpMethod.Delete,"/domain/zone/{0}/record/{1}".Fmt(zoneName, recordId));
		}
		public async Task<string> ExportDomainZone(string zoneName)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);

			return await RawCall<string>(HttpMethod.Get,"/domain/zone/{0}/export".Fmt(zoneName));
		}
		public async Task<ServiceInfo> GetDomainZoneServiceInfo(string zoneName)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);

			return await RawCall<ServiceInfo>(HttpMethod.Get,"/domain/zone/{0}/serviceInfos".Fmt(zoneName));
		}
		public async Task<DomainZoneSoa> GetDomainZoneSoa(string zoneName)
		{
			Ensure.NotNullNotEmpty("zoneName",zoneName);

			return await RawCall<DomainZoneSoa>(HttpMethod.Get,"/domain/zone/{0}/soa".Fmt(zoneName));
		}
		public async Task<DomainServiceInfo> GetDomainServiceInfo(string serviceName)
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);

			return await RawCall<DomainServiceInfo>(HttpMethod.Get,"/domain/{0}".Fmt(serviceName));
		}
		public async Task<long[]> GetDomainServiceNameServerIds(string serviceName)
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);

			return await RawCall<long[]>(HttpMethod.Get,"/domain/{0}/nameServer".Fmt(serviceName));
		}
		public async Task<DomainServiceNameserver> GetDomainServiceNameServer(string serviceName, long nameserverId)
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);
			Ensure.IdIsValid("nameserverId",nameserverId);

			return await RawCall<DomainServiceNameserver>(HttpMethod.Get,"/domain/{0}/nameServer/{1}".Fmt(serviceName,nameserverId));
		}
		public async Task<WhoisObfuscatorFields[]> GetDomainServiceOwo(string serviceName,WhoisObfuscatorFields? field = null )
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);

			QueryString queryString = new QueryString();
			queryString["field"] = field;
			return await RawCall<WhoisObfuscatorFields[]>(HttpMethod.Get,"/domain/{0}/owo/{1}".Fmt(serviceName,queryString.ToString()));
		}
		public async Task<long[]> GetDomainServiceTasksIds(string serviceName)
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);

			return await RawCall<long[]>(HttpMethod.Get,"/domain/{0}/task".Fmt(serviceName));
		}
		public async Task<OvhTask> GetDomainServiceTask(string serviceName, long taskId)
		{
			Ensure.NotNullNotEmpty("serviceName",serviceName);
			Ensure.IdIsValid("taskId",taskId);

			return await RawCall<OvhTask>(HttpMethod.Get,"/domain/{0}/task/{1}".Fmt(serviceName,taskId));
		}

		public async Task<NickHandle> GetNickhandle()
		{
			return await RawCall<NickHandle>(HttpMethod.Get,"/me");
		}

		public async Task<long[]> GetApiCredentials()
		{
			return await RawCall<long[]>(HttpMethod.Get,"/me/api/credential");
		}
		public async Task<Credential> GetApiCredential(long credentialId)
		{
			Ensure.IdIsValid("credentialId",credentialId);
			return await RawCall<Credential>(HttpMethod.Get, "/me/api/credential/{0}".Fmt(credentialId));
		}
		public async Task DeleteApiCredential(long credentialId)
		{
			Ensure.IdIsValid("credentialId",credentialId);
			await RawCall(HttpMethod.Delete, "/me/api/credential/{0}".Fmt(credentialId));
		}
		public async Task<Application> GetApiCredentialApplication(long credentialId)
		{
			Ensure.IdIsValid("credentialId",credentialId);
			return await RawCall<Application>(HttpMethod.Get, "/me/api/credential/{0}/application".Fmt(credentialId));
		}

		public async Task<string[]> GetBills(DateTime? from = null, DateTime? to = null)
		{
			QueryString qs = new QueryString();
			qs["date.from"] = from;
			qs["date.to"] = to;

			return await RawCall<string[]>(HttpMethod.Get,"/me/bill{0}".Fmt(qs.ToString()));
		}
		public async Task<Bill> GetBill(string billId)
		{
			Ensure.NotNullNotEmpty("billId",billId);

			return await RawCall<Bill>(HttpMethod.Get,"/me/bill/{0}".Fmt(billId));
		}
		public async Task<string[]> GetBillDetails(string billId)
		{
			Ensure.NotNullNotEmpty("billId",billId);
			return await RawCall<string[]>(HttpMethod.Get,"/me/bill/{0}/details".Fmt(billId));
		}
		public async Task<BillDetail> GetBillDetail(string billId, string detailId)
		{
			Ensure.NotNullNotEmpty("billId",billId);
			Ensure.NotNullNotEmpty("detailId",detailId);

			return await RawCall<BillDetail>(HttpMethod.Get,"/me/bill/{0}/details/{1}".Fmt(billId,detailId));
		}
		public async Task<Payment> GetBillPayment(string billId)
		{
			Ensure.NotNullNotEmpty("billId",billId);
			return await RawCall<Payment>(HttpMethod.Get,"/me/bill/{0}/payment".Fmt(billId));
		}

		public async Task<long[]> GetApiApplications()
		{
			return await RawCall<long[]>(HttpMethod.Get,"/me/api/application");
		}
		public async Task<Application> GetApiApplication(long applicationId)
		{
			Ensure.IdIsValid("applicationId",applicationId);
			return await RawCall<Application>(HttpMethod.Get, "/me/api/application/{0}".Fmt(applicationId));
		}
		public async Task DeleteApiApplication(long applicationId)
		{
			Ensure.IdIsValid("applicationId",applicationId);
			await RawCall(HttpMethod.Delete, "/me/api/application/{0}".Fmt(applicationId));
		}

		public async Task<string[]> GetSmsServices()
		{
			return await RawCall<string[]>(HttpMethod.Get,"/sms");
		}

		public async Task<string[]> GetDedicatedServers()
		{
			return await RawCall<string[]>(HttpMethod.Get,"/dedicated/server",null);
		}
		public async Task<ServerInfos> GetDedicatedServerInfos(string serverName)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			return await RawCall<ServerInfos>(HttpMethod.Get, "/dedicated/server/{0}".Fmt(serverName));
		}
		public async Task<ServiceInfo> GetDedicatedServerServiceInfo(string serverName)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			return await RawCall<ServiceInfo>(HttpMethod.Get,"/dedicated/server/{0}/serviceInfos".Fmt(serverName));
		}
		public async Task<string[]> GetDedicatedServerIps(string serverName)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			return await RawCall<string[]>(HttpMethod.Get,"/dedicated/server/{0}/ips".Fmt(serverName));
		}
		public async Task<Mrtg[]> GetDedicatedServerMrtg(string serverName,
		                                                 MrtgPeriod period,
		                                                 MrtgType type)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);

			string t;
			switch(type) {
			case MrtgType.ErrorsDownload:
				t = "errors:download";
				break;
			case MrtgType.ErrorsUpload:
				t = "errors:upload";
				break;
			case MrtgType.PacketsDownload:
				t = "packets:download";
				break;
			case MrtgType.PacketsUpload:
				t = "packets:upload";
				break;
			case MrtgType.TrafficDownload:
				t = "traffic:download";
				break;
			case MrtgType.TrafficUpload:
				t = "traffic:upload";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}

			return await RawCall<Mrtg[]>(
				HttpMethod.Get, 
				"/dedicated/server/{0}/mrtg?period={1}&type={2}".Fmt(
					serverName,
					period.ToString().ToLower(),
					t));
		}
		public async Task<BackupFtp> GetDedicatedServerBackupFtp(string serverName)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			return await RawCall<BackupFtp>(HttpMethod.Get,"/dedicated/server/{0}/features/backupFTP".Fmt(serverName));
		}
		public async Task<long[]> GetDedicatedServerInterventionIds(string serverName)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			return await RawCall<long[]>(HttpMethod.Get,"/dedicated/server/{0}/intervention".Fmt(serverName));
		}
		public async Task<Intervention> GetDedicatedServerIntervention(string serverName,
		                                                               long interventionId)
		{
			Ensure.NotNullNotEmpty("serverName",serverName);
			Ensure.IdIsValid("interventionId",interventionId);
			return await RawCall<Intervention>(HttpMethod.Get,"/dedicated/server/{0}/intervention/{1}".Fmt(serverName, interventionId));
		}
		public async Task<string[]> GetIPs(IPType? type = null)
		{
			QueryString queryString = new QueryString();
			queryString["type"] = type;
			return await RawCall<string[]>(HttpMethod.Get,"/ip{0}".Fmt(queryString.ToString()));
		}

		#region Private methods

		/// <summary>
		/// Get the delta between this computer and the OVH cluster to sign further queries
		/// </summary>
		/// <returns>The time delta.</returns>
		private async Task<long> GetTimeDelta()
		{
			if(_timeDelta == null) {
				var response = _client.GetAsync(_rootPath + "/auth/time");

				if(response.Result.IsSuccessStatusCode) {
					// by calling .Result you are performing a synchronous call
					var responseContent = response.Result.Content; 
					int serverTime = await ParseResponse<int>(responseContent);
					_timeDelta = DateTime.Now.ToUnixTime() - serverTime;
				}
			}
			return _timeDelta.Value;
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
			long now = DateTime.Now.ToUnixTime() - await GetTimeDelta();
			Uri targetUrl = new Uri(_rootPath + path);
			HttpRequestMessage request = new HttpRequestMessage(method, targetUrl);

			string body = content.ToJson();
			if(body != null) {
				request.Content = new JsonContent(body);
			}

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
			request.Headers.Add("X-Ovh-Timestamp", now.ToString());
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

		private static async Task<T> ParseResponse<T>(HttpContent content)
		{
			byte[] responseString = await content.ReadAsByteArrayAsync();
			string strContent = Encoding.UTF8.GetString(responseString);
			return strContent.FromJson<T>();
		}

		#endregion
	}
}

