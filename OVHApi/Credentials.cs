using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OVHApi
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
		[JsonProperty("method")]
		public string Method { get; set; }
		[JsonProperty("path")]
		public string Path{get;set;}
	}

	public class CredentialsRequest
	{
		public CredentialsRequest()
		{
			AccessRules = new List<AccessRule>();
		}

		[JsonProperty("accessRules")]
		public List<AccessRule> AccessRules{ get; private set;}

		[JsonProperty("redirection")]
		public string Redirection{get;set;}
	}

	public class CredentialsResponse
	{
		[JsonProperty("validationUrl")]
		public string ValidationUrl{ get; set;}
		[JsonProperty("consumerKey")]
		public string ConsumerKey{ get; set;}
		[JsonProperty("state")]
		public CredentialStatus State{ get; set;}
	}

	//public class Credential
	//{
	//	public CredentialStatus Status{ get; set;}
	//	public string ApplicationId{get;set;}
	//	public DateTime Expiration{get;set;}
	//	public DateTime LastUse{get;set;}
	//	public long CredentialId{ get; set; }
	//	public DateTime Creation{get;set;}
	//	public AccessRule[] Rules{get;set;}
	//}
}

