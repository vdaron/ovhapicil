using System;

namespace OVHApi.Commands.Domain
{
	public enum TransferLockStatus
	{
		Locked,
		Locking,
		Unavailable,
		Unlocked,
		Unlocking
	}

	public enum NameServerType
	{
		External,
		Hosted
	}

	public class DomainServiceInfo
	{
		/// <summary>
		/// Is whois obfuscation supported by this domain name's registry
		/// </summary>
		/// <value><c>true</c> if owo supported; otherwise, <c>false</c>.</value>
		public bool OwoSupported{get;set;}
		public string Domain{get;set;}
		public DateTime LastUpdate{get;set;}
		public TransferLockStatus TransferLockStatus{get;set;}
		public NameServerType NameServerType{get;set;} 
	}
}

