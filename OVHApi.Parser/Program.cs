using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OVHApi.Parser
{
	public class Program
	{
		public static void Main(string[] args)
		{
			HttpFileLoader loader = new HttpFileLoader();
			var apis = loader.Load();

			ModelGenerator generator = new ModelGenerator();
			generator.CreateModels(apis);
		}
	}
}
