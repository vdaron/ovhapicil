using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OVHApi.Parser
{
	public class HttpFileLoader : IFileLoader
	{
		private readonly string[] _urls = new[]
			                                 {
				                                 //"https://api.ovh.com/1.0/domains.json", // fully deprecated
				                                 "https://api.ovh.com/1.0/cdn/dedicated.json",
				                                 "https://api.ovh.com/1.0/cdn/website.json",
				                                 "https://api.ovh.com/1.0/cdn/webstorage.json",
				                                 "https://api.ovh.com/1.0/cloud.json",
				                                 "https://api.ovh.com/1.0/dedicated/installationTemplate.json",
				                                 "https://api.ovh.com/1.0/dedicated/nas.json",
				                                 "https://api.ovh.com/1.0/dedicated/nasha.json",
				                                 "https://api.ovh.com/1.0/dedicated/server.json",
				                                 "https://api.ovh.com/1.0/dedicatedCloud.json",
				                                 "https://api.ovh.com/1.0/domain.json",
				                                 "https://api.ovh.com/1.0/email/exchange.json",
				                                 "https://api.ovh.com/1.0/ip.json",
				                                 "https://api.ovh.com/1.0/license/cpanel.json",
				                                 "https://api.ovh.com/1.0/license/directadmin.json",
				                                 "https://api.ovh.com/1.0/license/plesk.json",
				                                 "https://api.ovh.com/1.0/license/virtuozzo.json",
				                                 "https://api.ovh.com/1.0/license/windows.json",
				                                 "https://api.ovh.com/1.0/me.json",
				                                 "https://api.ovh.com/1.0/newAccount.json",
				                                 "https://api.ovh.com/1.0/order.json",
				                                 "https://api.ovh.com/1.0/price.json",
				                                 "https://api.ovh.com/1.0/sms.json",
				                                 "https://api.ovh.com/1.0/telephony.json",
				                                 "https://api.ovh.com/1.0/vps.json",
				                                 "https://api.ovh.com/1.0/vrack.json",
				                                 "https://api.ovh.com/1.0/xdsl.json",
			                                 };

		public List<OvhApi> Load()
		{
			List<OvhApi> apis = new List<OvhApi>();
			WebClient client = new WebClient();

			foreach (var url in _urls)
			{
				string json = client.DownloadString(url);
				apis.Add(JsonConvert.DeserializeObject<OvhApi>(json));
			}

			return apis;
		}
	}
}
