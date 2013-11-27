using System;
using System.Collections.Generic;

namespace OVHApi.Commands
{
	public enum CredentialStatus
	{
		Expired,
		PendingValidation,
		Refused,
		Validated
	}

	public class AccessRule
	{
		public string Method{get;set;}
		public string Path{get;set;}
	}
	public class CredentialsRequest
	{
		public CredentialsRequest()
		{
			AccessRules = new List<AccessRule>();
		}
		public List<AccessRule> AccessRules{ get; private set;}
		public string Redirection{get;set;}
	}

	public class CredentialsResponse
	{
		public string ValidationUrl{ get; set;}
		public string ConsumerKey{ get; set;}
		public CredentialStatus State{ get; set;}
	}

	public class Credential
	{
		public CredentialStatus Status{ get; set;}
		public string ApplicationId{get;set;}
		public DateTime Expiration{get;set;}
		public DateTime LastUse{get;set;}
		public long CredentialId{ get; set; }
		public DateTime Creation{get;set;}
		public AccessRule[] Rules{get;set;}
	}
}

