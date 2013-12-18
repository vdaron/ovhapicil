using System;

namespace OVHApi.Commands
{
	/// <summary>
	/// A numeric value tagged with its unit
	/// </summary>
	public class UnitAndValue<T>
	{
		public string Unit{ get; set; }

		public T Value{ get; set; }
	}
}

