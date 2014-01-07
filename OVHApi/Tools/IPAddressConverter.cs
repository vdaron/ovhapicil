using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OVHApi.Tools
{
	class IPAddressConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(IPAddress));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			IPAddress ip = (IPAddress)value;
			writer.WriteValue(ip.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			return IPAddress.Parse(token.Value<string>());
		}
	}
}
