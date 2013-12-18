using System;
using System.Net;

namespace OVHApi.Commands.Domain
{
	public class DomainServiceNameserver
	{
		/// <summary>
		/// toDelete flag of the name server
		/// </summary>
		/// <value><c>true</c> if to delete; otherwise, <c>false</c>.</value>
		public bool ToDelete{ get; internal set; }

		/// <summary>
		/// Ip of the name server
		/// </summary>
		/// <value>The ip.</value>
		public string Ip{ get; internal set; }

		/// <summary>
		/// isUsed flag of the name server
		/// </summary>
		/// <value><c>true</c> if this instance is used; otherwise, <c>false</c>.</value>
		public bool IsUsed{ get; internal set; }

		/// <summary>
		/// Id of the name server
		/// </summary>
		/// <value>The identifier.</value>
		public long Id{ get; internal set; }

		/// <summary>
		/// Host of the name server
		/// </summary>
		/// <value>The host.</value>
		public string Host{ get; internal set; }
	}
}