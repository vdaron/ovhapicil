using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace OVHApi.Parser
{
	public class Property
	{
		[JsonProperty]
		public bool CanBeNull { get; internal set; }
		[JsonProperty]
		public string Type { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
		[JsonProperty]
		public bool Readonly { get; internal set; }
	}

	public class Model
	{
		public Model()
		{
			Properties = new Dictionary<string, Property>();
		}
		[JsonProperty]
		public string Namespace { get; internal set; }
		[JsonProperty]
		public string Id { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
		[JsonProperty]
		public string[] Enum { get; internal set; }
		[JsonProperty]
		public string EnumType { get; internal set; }
		[JsonProperty]
		public string[] Generics { get; internal set; }
		[JsonProperty]
		public Dictionary<string, Property> Properties { get; internal set; }

		[JsonIgnore]
		public bool IsEnum
		{
			get { return Id.EndsWith("Enum"); }
		}
	}

	public enum ApiState
	{
		Production,
		Beta,
		Deprecated
	}

	public class ApiStatus
	{
		[JsonProperty]
		public ApiState Value { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
		[JsonProperty]
		public DateTime DeprecatedDate { get; internal set; }
		[JsonProperty]
		public DateTime DeletionDate { get; internal set; }
		[JsonProperty]
		public string Replacement { get; internal set; }
	}

	public enum ParamType
	{
		Path,
		Query,
		Body
	}

	public class Parameter
	{
		[JsonProperty]
		public bool Required { get; internal set; }
		[JsonProperty]
		public string DataType { get; internal set; }
		[JsonProperty]
		public ParamType ParamType { get; internal set; }
		[JsonProperty]
		public string Name { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
	}

	public class Operation
	{
		[JsonProperty]
		public Parameter[] Parameters { get; internal set; }
		[JsonProperty]
		public string HttpMethod { get; internal set; }
		[JsonProperty]
		public ApiStatus ApiStatus { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
		[JsonProperty]
		public string ResponseType { get; internal set; }
		[JsonProperty]
		public bool NoAuthentication { get; internal set; }
	}

	public class Api
	{
		[JsonProperty]
		public Operation[] Operations { get; internal set; }
		[JsonProperty]
		public string Path { get; internal set; }
		[JsonProperty]
		public string Description { get; internal set; }
	}

	public class OvhApi
	{
		[JsonProperty]
		public string ApiVersion { get; internal set; }
		[JsonProperty]
		public string BasePath { get; internal set; }
		[JsonProperty]
		public string ResourcePath { get; internal set; }
		[JsonProperty]
		public Dictionary<string, Model> Models { get; internal set; }
		[JsonProperty]
		public Api[] Apis { get; internal set; }
	}
}
