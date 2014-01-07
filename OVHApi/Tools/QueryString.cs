using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OVHApi.Tools
{
	internal class QueryString : Dictionary<string,object>
	{
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			foreach (string key in Keys)
			{
				string v = base[key] != null ? System.Net.WebUtility.UrlEncode(GetValueAsString(key)) : null;
				if (!string.IsNullOrEmpty(v))
				{
					if (builder.Length > 0)
					{
						builder.Append('&');
					}

					builder.AppendFormat("{0}={1}", key, v);
				}
			}
			return builder.Length > 0 ? "?" + builder : String.Empty;
		}

		private string GetValueAsString(string key)
		{
			object o = base[key];

			if(o is DateTime) {
				return ((DateTime)o).ToString("yyyyMMdd");
			}
			else if(o is Enum)
			{
				return GetEnumValue(o);
			}
			return o.ToString();
		}

		private string GetEnumValue(object v)
		{
			var type = v.GetType();
			var memInfo = type.GetMember(v.ToString());
			var attributes = memInfo[0].GetCustomAttributes(typeof(JsonPropertyAttribute),false);
			return ((JsonPropertyAttribute)attributes[0]).PropertyName;
		}
	}
}

