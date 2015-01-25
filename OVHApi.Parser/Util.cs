using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OVHApi.Parser
{
	public static class Util
	{
		private const string ModelsNamespaces = "OvhApi.Models.";

		public static string GetMethodName(Api api, Operation operation)
		{
			string prefix = GetMethodPrefix(operation);
			string[] tokens = api.Path.Split(new []{'/'},StringSplitOptions.RemoveEmptyEntries);

			StringBuilder result = new StringBuilder(prefix);
			foreach (var token in tokens)
			{
				if (token.StartsWith("{"))
					continue;
				result.Append(FirstUpperCase(token));
			}

			if (operation.ResponseType.EndsWith("[]"))
			{

				if (result[result.Length - 1] == 's' && result[result.Length - 2] != 'm' && result[result.Length - 3] != 'S') // remove existing 's'
				{
					result.Remove(result.Length - 1, 1);
				}

				if (operation.ResponseType == "long[]")
				{
					result.Append("Id");
				}
				else if (operation.ResponseType == "string[]")
				{
					result.Append("Name");
				}
				result.Append("s");
			}

			return result.ToString();
		}

		public static string FirstUpperCase(string value)
		{
			return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLower();
		}

		public static string ToCamelCase(string value)
		{
			return char.ToLowerInvariant(value[0]) + value.Substring(1);
		}

		public static string GetMethodNameAndParameters(Api api, Operation operation)
		{
			StringBuilder result = new StringBuilder("");

			result.Append(GetMethodName(api, operation));

			List<string> required = new List<string>();
			List<string> nulllable = new List<string>();
			foreach (var p in operation.Parameters)
			{
				StringBuilder param = new StringBuilder();
				param.Append(GetType(p.DataType));
				if (IsNullableType(p))
				{
					param.Append('?');
				}
				param.Append(' ');
				param.Append(GetParameterName(p));

				if (!p.Required)
				{
					param.Append(" = null");
					nulllable.Insert(0,param.ToString()); // To have have from date before to date
				}
				else
				{
					required.Add(param.ToString());
				}
			}

			result.Append('(');

			for (int i = 0; i < required.Count; i++)
			{
				if (i > 0)
					result.Append(',');
				result.Append(required[i]);
			}
			for (int i = 0; i < nulllable.Count; i++)
			{
				if (required.Count > 0 || i > 0)
					result.Append(',');
				result.Append(nulllable[i]);
			}
			result.Append(')');
			return result.ToString();
		}

		public static bool IsNullableType(Property prop)
		{
			string type = GetType(prop.Type);

			return prop.CanBeNull && !prop.Type.EndsWith("[]") && type != "string" && !type.Contains(".");
		}

		public static bool IsNullableType(Parameter prop)
		{
			string type = GetType(prop.DataType);
			
			return !prop.Required && !prop.DataType.EndsWith("[]") && type != "string" && (!type.Contains(".") || (type.EndsWith("Enum")));
		}

		public static string FixCase(string name)
		{
			string[] tokens = name.Split('.');

			StringBuilder result = new StringBuilder();

			for (int i = 0; i < tokens.Length; i++)
			{
				if (i > 0)
					result.Append('.');

				string token = tokens[i];
				result.Append(char.ToUpper(token[0]));
				result.Append(token.Substring(1));
			}
			return result.ToString();
		}

		public static string GetType(string type)
		{
			// TODO: Process Recursive Generic types (complexType.UnitAndValues<xdsl.TimestampAndValue>>)
			bool isArray = false;
			string result = type;

			Regex r = new Regex(@"<(.*)>$");
			
			if(r.IsMatch(result))
			{
				Match genericMatch = r.Match(type);
                var genericResult = GetType(genericMatch.Groups[1].Value);

                if (genericResult.StartsWith("complexType"))
                {
                    genericResult = genericResult.Replace("complexType", "OvhApi.Models.ComplexType");
                }

                result = r.Replace(result, "<" + genericResult + ">");
			}

			if (type.Contains("."))
			{
            if (type.StartsWith("complexType"))
            {
                type = type.Replace("complexType", "OvhApi.Models.ComplexType");
            }

				return ModelsNamespaces + FixCase(FixNamespace(result));
			}

            if (type.StartsWith("complexType"))
            {
                type = type.Replace("complexType", "OvhApi.Models.ComplexType");
            }

			if (type.EndsWith("[]"))
			{
				result = result.Substring(0, result.Length - 2);
				isArray = true;
			}

			if (result.EndsWith("Enum"))
			{
				result = result.Substring(0, result.Length - 4);
			}

			switch (result)
			{
				case "boolean":
					result = "bool";
					break;
				case "datetime":
				case "date":
					result = "DateTime";
					break;
				case "time":
					result = "TimeSpan";
					break;
				case "ipv4":
				case "ipv6":
				case "ip":
					result = "System.Net.IPAddress";
					break;
				case "ipBlock":
				case "ipv4Block":
					result = "OVHApi.IPAddressBlock";
					break;
				case "phoneNumber":
				case "text":
				case "password":
					result = "string";
					break;
			}
			return isArray ? result + "[]" : result;
		}

		public static string GetEnumValue(string name)
		{
			string n = FixCase(name);
            n = n.Replace(":", "_").Replace('-', '_').Replace('.', '_').Replace('/', '_').Replace("+", "Plus").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
			if (char.IsDigit(n[0]))
			{
				n = "_" + n;
			}
			return n;
		}

		public static string GetPropertyName(string className, string propertyName)
		{
			string n = FixCase(propertyName);

			if (n == FixCase(className) || (n == "Profile")) // To avoid clash between property name and classname or subclass name
			{
				n = n + "Name";
			}


			return n;
		}

		public static string GetPropertyBackingFieldName(string className, string propertyName)
		{
			string n = ToCamelCase(propertyName);

			if (n == FixCase(className))
			{
				n = n + "Name";
			}

			return '_' + n;
		}

		public static string GetMethodReturnTaskParameter(string type, bool async = true)
		{
			if(type == "void")
				return String.Empty;

            var item = GetType(type);

            if (item.Contains("complexType"))
            {
                item = item.Replace("complexType", "OvhApi.Models.ComplexType");
            }

            if (async)
                return "<" + item + ">";
            return item;
		}

		public static string CreateParameterChecks(Parameter[] parameters, int indent = 3)
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < parameters.Length; i++)
			{
				if (!parameters[i].Required) 
					continue;

				string type = GetType(parameters[i].DataType);

                result.Append(GetIndent(indent));
				switch (type)
				{
					case "string":
						result.AppendFormat("Ensure.NotNullNorEmpty(\"{0}\",{0});\n", GetParameterName(parameters[i]));
						break;
					case "long":
						result.AppendFormat("Ensure.IdIsValid(\"{0}\",{0});\n", GetParameterName(parameters[i]));
						break;
					default:
						result.AppendFormat("Ensure.NotNull(\"{0}\",{0});\n", GetParameterName(parameters[i]));
						break;
				}
			}
			return result.ToString();
		}

        public static string GetIndent(int value)
        {
            var chars = new char[value];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = '\t';
            }

            return new string(chars);
        }

		public static string GetApiPath(Api api, Operation operation)
		{
			string path = api.Path;

			int count = 0;
			List<string> args = new List<string>();
			bool hasQueryString = false;
			foreach (Parameter p in operation.Parameters)
			{
				switch (p.ParamType)
				{
					case ParamType.Path:
						path = path.Replace("{" + p.Name + "}", "{" + count + "}");
					args.Add(GetParameterName(p));
					count++;
						break;
					case ParamType.Query:
						hasQueryString = true;
						break;
				}
			}

			StringBuilder result = new StringBuilder("String.Format(");
			result.Append('"');
			result.Append(path);
			if (hasQueryString)
				result.AppendFormat("{{{0}}}",count);
			result.Append('"');
            
			foreach (var arg in args)
			{
				result.AppendFormat(",System.Uri.EscapeDataString({0}.ToString())", arg);
			}
			if (hasQueryString)
				result.Append(",queryString");
			result.Append(")");

			return result.ToString();
		}

        public static string CreateQueryString(Parameter[] parameters, int indent = 3)
		{
			//var queryString = new QueryString();
			//queryString["fieldType"] = fieldType;
			//queryString["subDomain"] = subDomain;

			StringBuilder result = new StringBuilder();
			foreach (var parameter in parameters)
			{
				if (parameter.ParamType == ParamType.Query)
				{
                    if (result.Length == 0)
					{
                        result.Append(GetIndent(indent));
                        result.AppendLine("var queryString = new QueryString();");
					}

                    result.Append(GetIndent(indent));
                    result.AppendLine(String.Format("queryString.Add(\"{0}\",{1});", parameter.Name, GetParameterName(parameter)));
				}
			}

			return result.ToString();
		}

		public static string CreateBodyRequest(Parameter[] parameters)
		{
			StringBuilder result = new StringBuilder();
			foreach (var parameter in parameters)
			{
				if (parameter.ParamType == ParamType.Body)
				{
					if (String.IsNullOrEmpty(parameter.Name))
						return String.Empty; // The parameter is the body

					if (result.Length == 0)
					{
						result.AppendLine("var requestBody = new Dictionary<string, object>();");
					}
					result.AppendLine(String.Format("requestBody.Add(\"{0}\",{1});", parameter.Name, GetParameterName(parameter)));
				}
			}

			return result.ToString();
		}

        public static string CreateMethodReturn(Api api, Operation operation, int indent = 3, bool async = true)
		{
			StringBuilder result = new StringBuilder();

			string methodReturnType = GetMethodReturnTaskParameter(operation.ResponseType);

            result.Append(GetIndent(indent));

			if (methodReturnType != String.Empty)
			{
				result.Append("return ");
			}

            if (async)
                result.Append("await RawCall");
            else
                result.Append("RawCall");
            
            result.Append(methodReturnType);
			result.AppendFormat("(HttpMethod.{0},", FirstUpperCase(operation.HttpMethod));
			result.Append(GetApiPath(api,operation));
			if (operation.Parameters.Any(parameter => parameter.ParamType == ParamType.Body))
			{
				result.Append(",requestBody");
			}
			result.Append(");");
			return result.ToString();
		}

		public static string GetGenericsParameters(Model model)
		{
			StringBuilder result = new StringBuilder();

			if (model.Generics != null && model.Generics.Length > 0)
			{
				result.Append('<');
				for (int i = 0; i < model.Generics.Length; i++)
				{
					if (i > 0)
					{
						result.Append(',');
					}
					result.Append(model.Generics[i]);
				}
				result.Append('>');
			}

			return result.ToString();
		}

		public static string GetParameterName(Parameter parameter)
		{
			if (String.IsNullOrEmpty(parameter.Name))
				return "requestBody";
			if (parameter.Name == "class")
				return "class_";
			return parameter.Name.Replace('.', '_');
		}

		public static string GetNamespace(Model model)
		{
			return ModelsNamespaces + Util.FixCase(Util.FixNamespace(model.Namespace));
		}

		/// <summary>
		/// Add an '_' before namespaces starting with a number
		/// </summary>
		/// <param name="ns"></param>
		/// <returns></returns>
		private static string FixNamespace(string ns)
		{
			return Regex.Replace(ns, @"\.[0-9]", delegate(Match match)
				{
					string v = match.ToString();
					return "._" + v.Substring(1);
				});
		}

		private static string GetMethodPrefix(Operation operation)
		{
			//if (operation.Parameters.Any(parameter => parameter.ParamType == ParamType.Query))
			//{
			//	return "Retrieve";
			//}

			switch (operation.HttpMethod)
			{
				case "GET":
					return "Get";
				case "POST":
					return "Create";
				case "PUT":
					return "Update";
				case "DELETE":
				default:
					return "Delete";
			}
		}
	}
}
