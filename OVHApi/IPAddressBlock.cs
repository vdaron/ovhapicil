using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVHApi
{
	public class IPAddressBlock
	{
		public string Value { get; set; }

		public static implicit operator IPAddressBlock(string address)
		{
			return new IPAddressBlock() { Value = address };
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
