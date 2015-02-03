using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OVHApi.Parser
{
	public interface IFileLoader
	{
		List<OvhApi> Load();
	}
}
