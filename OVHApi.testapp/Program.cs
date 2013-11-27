
using System;
using OVHApi.Commands;
using OVHApi.Commands.Dedicated.Server;
using OVHApi.Commands.Domain;
using OVHApi.Commands.Me;

namespace OVHApi.testapp
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			OvhApiClient api = new OvhApiClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET", OvhInfra.Europe);

//			CredentialsResponse response = api.RequestCredential(new []{
//				new AccessRule{ Method = "GET", Path = "/*"},
//				new AccessRule{ Method = "PUT", Path = "/*"},
//				new AccessRule{ Method = "POST", Path = "/*"},
//				new AccessRule{ Method = "DELETE", Path = "/*"},
//			}).Result;

			api.ConsumerKey = "YOUR_CONSUMER_KEY";
			try {

				string[] billIds= api.GetBills(DateTime.Now.AddMonths(-5), DateTime.Now.AddMonths(-2)).Result;
				for (int i = 0; i < billIds.Length; i++) {
					Bill b = api.GetBill(billIds[i]).Result;
					Payment p = api.GetBillPayment(billIds[i]).Result;

					string[] billDetailIds = api.GetBillDetails(billIds[i]).Result;
					for (int j = 0; j < billDetailIds.Length; j++) {
						BillDetail bd = api.GetBillDetail(b.BillId,billDetailIds[j]).Result;
					}

				}

				long[] appIds = api.GetApiApplications().Result;
				for (int i = 0; i < appIds.Length; i++) {
					Application app = api.GetApiApplication(appIds[i]).Result;
					Console.WriteLine(app.ApplicationKey);
				}

				long[] credsIds = api.GetApiCredentials().Result;
				for (int i = 0; i < credsIds.Length; i++) {
					Application app = api.GetApiCredentialApplication(credsIds[i]).Result;
					Credential cred = api.GetApiCredential(credsIds[i]).Result;
					if(cred.Expiration < DateTime.Now)
						api.DeleteApiCredential(credsIds[i]).Wait();
				}

				//var rfrf = api.CreateDomainRecord("daron.be",NamedResolutionFieldType.A,"10.0.0.1","test").Result;

//				long[] ids = api.GetDomainRecordIds("daron.be",NamedResolutionFieldType.A).Result;
//
//				foreach(long l in ids)
//				{
//					DomainRecordDetail detail = api.GetDomainRecord("daron.be",l).Result;
//					if(detail.SubDomain == "test")
//					{
//						DomainRecordDetail newRecord = api.UpdateDomainRecord("daron.be",detail.Id ,  "111.111.111.11","test").Result;
//
//						 api.DeleteDomainRecord("daron.be",l).Wait();
//					}
//				}
//
//				foreach(string server in api.GetDedicatedServers().Result) {
//					ServerInfos infos = api.GetDedicatedServerInfos(server).Result;
//					ServiceInfo sInfos = api.GetDedicatedServerServiceInfo(server).Result;
//					string[] ips = api.GetDedicatedServerIps(server).Result;
//					Mrtg[] mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Daily, MrtgType.TrafficUpload).Result;
//					mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Weekly, MrtgType.TrafficDownload).Result;
//					mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Monthly, MrtgType.ErrorsDownload).Result;
//					mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Yearly, MrtgType.ErrorsUpload).Result;
//					mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Hourly, MrtgType.PacketsDownload).Result;
//					mrtg = api.GetDedicatedServerMrtg(server, MrtgPeriod.Monthly, MrtgType.PacketsUpload).Result;
//				}
			} catch(AggregateException ex) {
				Console.WriteLine(ex.Message);
			}



			Console.ReadLine();
		}
	}
}
