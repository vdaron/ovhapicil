using System;

namespace OVHApi.Commands.Domain
{
	public enum NamedResolutionFieldType
	{
		A,
		AAAA,
		CNAME,
		DKIM,
		LOC,
		MX,
		NAPTR,
		NS,
		PTR,
		SPF,
		SRV,
		SSHFP,
		TXT
	}

	public class DomainRecord
	{
		public string SubDomain{ get; set; }

		public string Target{ get; set; }

		public long? Ttl{ get; set; }
	}

	public class TypedDomainRecord : DomainRecord
	{
		public NamedResolutionFieldType FieldType{ get; set; }
	}

	public class DomainRecordDetail : TypedDomainRecord
	{
		public long Id{ get; set; }

		public string Zone{ get; set; }
	}
}

