using System;
using System.Net;

namespace OVHApi.Commands.Domain
{
	public class DomainServiceNameserver
	{
		public bool ToDelete{ get; set; }

		public string Ip{ get; set; }

		public bool IsUsed{ get; set; }

		public long Id{ get; set; }

		public string Host{ get; set; }
	}
}