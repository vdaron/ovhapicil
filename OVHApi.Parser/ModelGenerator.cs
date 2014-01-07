using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace OVHApi.Parser
{
	public class ModelGenerator
	{
		private readonly CodeCompileUnit _code = new CodeCompileUnit();

		public string GetOutput()
		{
			CodeDomProvider p = new CSharpCodeProvider();

			StringBuilder result = new StringBuilder();
			using (System.IO.StringWriter writer = new StringWriter(result))
			{
				var options = new CodeGeneratorOptions();
				options.IndentString = "\t";
				p.GenerateCodeFromCompileUnit(_code,writer,options);
			}

			return result.ToString();
		}

		public ModelGenerator()
		{
			CodeNamespace globalNamespace = new CodeNamespace();
			globalNamespace.Imports.Add(new CodeNamespaceImport("System"));
			globalNamespace.Imports.Add(new CodeNamespaceImport("System.Net.Http"));
			globalNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
			globalNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
			globalNamespace.Imports.Add(new CodeNamespaceImport("Newtonsoft.Json"));
			globalNamespace.Imports.Add(new CodeNamespaceImport("OVHApi.Tools"));
			
			_code.Namespaces.Add(globalNamespace);
		}

		public void CreateModels(IEnumerable<OvhApi> apis)
		{
			Dictionary<string,Model> fullTypeNames = new Dictionary<string, Model>(); 
			Dictionary<string,List<Model>> modelsByNamespaces = new Dictionary<string, List<Model>>();
			Dictionary<string,List<Model>> subModels = new Dictionary<string, List<Model>>();

			//Unify namespace case and remove duplicates
			foreach (var ovhApi in apis)
			{
				foreach (var model in ovhApi.Models)
				{
					string fullTypeName = Util.GetType(model.Key);

					if (fullTypeNames.ContainsKey(fullTypeName))
						continue;

					fullTypeNames.Add(fullTypeName, model.Value);

					string modelNamespace = Util.GetNamespace(model.Value);

					if (!modelsByNamespaces.ContainsKey(modelNamespace))
					{
						modelsByNamespaces.Add(modelNamespace,new List<Model>());
					}
					modelsByNamespaces[modelNamespace].Add(model.Value);
				}
			}

			foreach (string nsName in modelsByNamespaces.Keys)
			{
				if (fullTypeNames.ContainsKey(nsName))
				{
					Model parent = fullTypeNames[nsName];

					subModels.Add(parent.Id, new List<Model>());
					foreach (Model child in modelsByNamespaces[nsName])
					{
						subModels[parent.Id].Add(child);
					}
					modelsByNamespaces[nsName].Clear();
				}
			}


			foreach (string nsName in modelsByNamespaces.Keys)
			{
				if (modelsByNamespaces[nsName].Count == 0)
					continue;

				CodeNamespace ns = new CodeNamespace(nsName);

				foreach (Model model in modelsByNamespaces[nsName])
				{
					var type = CreateType(model);
					if (subModels.ContainsKey(model.Id))
					{
						foreach (Model subModel in subModels[model.Id])
						{
							type.Members.Add(CreateType(subModel));
						}
					}
					ns.Types.Add(type);
				}
				_code.Namespaces.Add(ns);
			}
		}

		private static CodeTypeDeclaration CreateType(Model model)
		{
			return model.IsEnum ? CreateEnum(model.Id, model) : CreateClass(model.Id, model);
		}

		private static CodeTypeDeclaration CreateClass(string name, Model model)
		{
			var type = new CodeTypeDeclaration(Util.FixCase(name));
			type.IsClass = true;
			type.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
			if (model.Generics != null)
			{
				foreach (var gen in model.Generics)
				{
					type.TypeParameters.Add(new CodeTypeParameter(gen));
				}
			}

			AddSummary(type.Comments,model.Description);

			foreach (var prop in model.Properties)
			{
				CodeTypeReference propertyType = GetPropertyType(model, prop.Value);

				//1) Add backing field
				CodeMemberField backingField = new CodeMemberField();
				backingField.Attributes = MemberAttributes.Private;
				backingField.Name = Util.GetPropertyBackingFieldName(type.Name,prop.Key);
				backingField.Type = propertyType;
				AddJsonPropertyAttritbute(backingField.CustomAttributes, prop.Key);
				type.Members.Add(backingField);

				//2) Create property
				var p = new CodeMemberProperty();
				p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
				p.Type = propertyType;
				p.HasGet = true;
				p.Name = Util.GetPropertyName(type.Name, prop.Key);
				p.GetStatements.Add(new CodeMethodReturnStatement(
						new CodeFieldReferenceExpression(
						new CodeThisReferenceExpression(), backingField.Name)));

				p.HasSet = !prop.Value.Readonly;
				if (p.HasSet)
				{
					p.SetStatements.Add(new CodeAssignStatement(
						                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name),
						                    new CodePropertySetValueReferenceExpression()));
				}

				AddSummary(p.Comments,prop.Value.Description);
				type.Members.Add(p);
			}

			return type;
		}

		private static CodeTypeDeclaration CreateEnum(string name, Model model)
		{
			var type = new CodeTypeDeclaration(Util.FixCase(name));
			type.IsEnum = true;
			AddSummary(type.Comments, model.Description);
			foreach (var val in model.Enum)
			{
				var member = new CodeMemberField(type.Name, Util.GetEnumValue(val));
				AddJsonPropertyAttritbute(member.CustomAttributes, val);
				type.Members.Add(member);
			}
			return type;
		}

		private static CodeTypeReference GetPropertyType(Model model, Property prop)
		{
			CodeTypeReference result = null;
			string type = Util.GetType(prop.Type);

			bool isString = false;

			if (type.Contains('.'))
			{
				result = new CodeTypeReference(type);
			}
			else if (model.Generics != null && model.Generics.Contains(type))
			{
				result =  new CodeTypeReference(type, CodeTypeReferenceOptions.GenericTypeParameter);
			}
			else
			{
				switch (type)
				{
					case "phoneNumber":
					case "string":
						result = new CodeTypeReference(typeof(string));
						isString = true;
						break;
					case "long":
						result = new CodeTypeReference(typeof(long));
						break;
					case "int":
						result = new CodeTypeReference(typeof(int));
						break;
					case "DateTime":
						result = new CodeTypeReference(typeof(DateTime));
						break;
					case "bool":
						result = new CodeTypeReference(typeof(bool));
						break;
					case "double":
						result = new CodeTypeReference(typeof(double));
						break;
					case "string[]":
						result = new CodeTypeReference(typeof(string[]));
						break;
					case "long[]":
						result = new CodeTypeReference(typeof(long[]));
						break;
					default:
						result = new CodeTypeReference(typeof(string));
						isString = true;
						break;
				}
			}

			if (prop.CanBeNull && !isString && !type.EndsWith("[]") && !type.Contains("."))
			{
				CodeTypeReference tmp = new CodeTypeReference(typeof (Nullable<>));
				tmp.TypeArguments.Add(result);
				result = tmp;
			}

			return result;
		}

		private static void AddSummary(CodeCommentStatementCollection comments, string summary)
		{
			comments.Add(new CodeCommentStatement("<summary>", true));
			comments.Add(new CodeCommentStatement(summary, true));
			comments.Add(new CodeCommentStatement("</summary>", true));
		}

		private static void AddJsonPropertyAttritbute(CodeAttributeDeclarationCollection attributes, string name)
		{
			attributes.Add(new CodeAttributeDeclaration("JsonProperty", new CodeAttributeArgument(new CodePrimitiveExpression(name))));
		}
	}

	public static class Exts
	{
		public static void AppendLineFormat(this StringBuilder builder, string format, params object[] p)
		{
			builder.AppendFormat(format, p);
			builder.AppendLine();
		}
	}

}
