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
		private class ModelType
		{
			private readonly List<ModelType> _children = new List<ModelType>();

			public ModelType(Model model)
			{
				Model = model;
			}

			public Model Model { get; private set; }
			public ModelType Parent { get; set; }
			public void AddChild(ModelType type)
			{
				_children.Add(type);
				type.Parent = this;
			}
			public IEnumerable<ModelType> Children { get { return _children; } } 
		}

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
			Dictionary<string,ModelType> modelsByNamespace = new Dictionary<string, ModelType>();
			foreach (var ovhApi in apis)
			{
				foreach (var model in ovhApi.Models)
				{
					string fullTypeName = Util.GetType(model.Key);

					if (modelsByNamespace.ContainsKey(fullTypeName))
						continue;

					modelsByNamespace.Add(fullTypeName, new ModelType(model.Value));
				}
			}

			foreach (var st in modelsByNamespace)
			{
				string modelNamespace = Util.GetNamespace(st.Value.Model);

				if (modelsByNamespace.ContainsKey(modelNamespace))
				{
					modelsByNamespace[modelNamespace].AddChild(st.Value);
				}
			}

			foreach (var type in modelsByNamespace.Where(x => x.Value.Parent == null))
			{
				CodeNamespace ns = new CodeNamespace(Util.GetNamespace(type.Value.Model));
				ns.Types.Add(CreateType(type.Value));
				_code.Namespaces.Add(ns);
			}
		}

		private static CodeTypeDeclaration CreateType(ModelType model)
		{
			return model.Model.IsEnum ? CreateEnum(model.Model.Id, model.Model) : CreateClass(model);
		}

		private static CodeTypeDeclaration CreateClass(ModelType modelType)
		{
			string name = modelType.Model.Id;
			Model model = modelType.Model;

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

			foreach (ModelType child in modelType.Children)
			{
				type.Members.Add(CreateType(child));
			}

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
