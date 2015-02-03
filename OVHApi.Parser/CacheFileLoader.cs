using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace OVHApi.Parser
{
	public class CacheFileLoader : IFileLoader
	{
		public List<OvhApi> Load()
		{
			List<OvhApi> apis = new List<OvhApi>();

			Assembly assembly = Assembly.GetExecutingAssembly();
			foreach (string url in assembly.GetManifestResourceNames().Where(x => x.EndsWith(".json")))
			{
				using(Stream stream = assembly.GetManifestResourceStream(url))
				using(TextReader reader = new StreamReader(stream))
				{
					string json = reader.ReadToEnd();
					apis.Add(JsonConvert.DeserializeObject<OvhApi>(json));
				}
			}

			return apis;
		}
	}
}
