using System;

namespace OVHApi.Commands.Dedicated.Server
{
	public enum ServerState
	{
		Error,
		Hacked,
		Ok
	}

	public class ServerInfos
	{
		/// <summary>
		/// Dedicated datacenter localisation.
		/// </summary>
		/// <value>The datacenter.</value>
		public string Datacenter{get;set;}

		/// <summary>
		/// Dedicated server ip
		/// </summary>
		/// <value>The ip.</value>
		public string Ip{get;set;}

		/// <summary>
		/// Dedicated server name
		/// </summary>
		/// <value>The name.</value>
		public string Name{get;set;}

		/// <summary>
		/// Dedicater server commercial range
		/// </summary>
		/// <value>The commercial range.</value>
		public string CommercialRange{get;set;}

		/// <summary>
		/// Operating system
		/// </summary>
		/// <value>The os.</value>
		public string Os{ get; set;}

		public ServerState State {get;set;}

		/// <summary>
		/// Dedicated server reverse
		/// </summary>
		/// <value>The reverse.</value>
		public string Reverse{get;set;}

		public long ServerId{get;set;}

		/// <summary>
		/// Icmp monitoring state
		/// </summary>
		/// <value><c>true</c> if monitoring; otherwise, <c>false</c>.</value>
		public bool Monitoring{get;set;}

		public string Rack{get;set;}

		public string RootDevice{get;set;}

		public long LinkSpeed{get;set;}

		public long BootId{get;set;}
	}
}

