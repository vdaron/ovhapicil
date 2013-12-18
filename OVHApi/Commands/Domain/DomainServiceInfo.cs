using System;

namespace OVHApi.Commands.Domain
{
	/// <summary>
	/// Domain lock status
	/// </summary>
	public enum TransferLockStatus
	{
		Locked,
		Locking,
		Unavailable,
		Unlocked,
		Unlocking
	}

	/// <summary>
	/// DomainNS Type
	/// </summary>
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
		public bool OwoSupported{ get; internal set; }

		/// <summary>
		/// Domain name
		/// </summary>
		/// <value>The domain.</value>
		public string Domain{ get; internal set; }

		/// <summary>
		/// Last update date
		/// </summary>
		/// <value>The last update.</value>
		public DateTime LastUpdate{ get; internal set; }

		/// <summary>
		/// Transfer lock status
		/// </summary>
		/// <value>The transfer lock status.</value>
		public TransferLockStatus TransferLockStatus{ get; set; }

		/// <summary>
		/// Name servers type
		/// </summary>
		/// <value>The type of the name server.</value>
		public NameServerType NameServerType{ get; set; }
	}
}

