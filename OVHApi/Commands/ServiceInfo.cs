using System;

namespace OVHApi.Commands
{
	public enum DomainStatus
	{
		Expired,
		InCreation,
		Ok,
		UnPaid
	}
	public class ServiceInfo
	{
		public DomainStatus Status{get;set;}
		public DateTime EngagedUpTo{get;set;}
		public string ContactBilling{get;set;}
		public string Domain{get;set;}
		public string ContactTech{get;set;}
		public DateTime Expiration{get;set;}
		public string ContactAdmin{get;set;}
		public DateTime Creation{get;set;}
	}
}

