using System;

namespace OVHApi.Commands.Domain
{
	public class DomainZoneSoa
	{
		public string Email{get;set;}
		public long NxDomainTtl{ get; set;}
		public long Refresh{get;set;}
		public long Ttl{ get; set;}
		public long Serial{get;set;}
		public string Server{get;set;}
		public long Expore{get;set;}
	}
}

