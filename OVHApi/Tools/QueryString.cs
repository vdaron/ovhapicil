using System;
using System.Text;
using ServiceStack.Text;
using System.Collections.Generic;

namespace OVHApi.Tools
{
	internal class QueryString : Dictionary<string,object>
	{
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			foreach(string key in Keys) {
				string v = base[key] != null ? GetValueAsString(key).UrlEncode() : null;
				if(!string.IsNullOrEmpty(v)) {
					if(builder.Length > 0) {
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
			return o.ToString();
		}
	}
}

