using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using REST.Template.Models;
using RESTInstaller.Extensions;
using RESTInstaller.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RESTInstaller.Services
{
    internal class Emitter
	{

		public string EmitHalConfiguration(ICodeService codeService, string controllerName, string className, Dictionary<string, string> replacementsDictionary)
        {
			ThreadHelper.ThrowIfNotOnUIThread();
			StringBuilder code = new StringBuilder();

			code.AppendLine("\t///\t<summary>");
			code.AppendLine($"\t///\t///Hal Configuration for the {controllerName}.");
			code.AppendLine("\t///\t</summary>");
			code.AppendLine($"\tpublic class {replacementsDictionary["$safeitemname$"]} : BaseConfiguration");
			code.AppendLine("\t{");
			code.AppendLine("\t\t///\t<summary>");
			code.AppendLine($"\t///\t///Configure Hal for the {controllerName}.");
			code.AppendLine("\t\t///\t</summary>");
			code.AppendLine($"\t\tpublic {replacementsDictionary["$safeitemname$"]}()");
			code.AppendLine("\t\t{");

			CodeClass2 resourceClass = codeService.FindClass(className);
			CodeClass2 controllerClass = codeService.FindClass(controllerName);
			CodeClass2 halConfigurationClass = codeService.FindClass("HalConfiguration");

			CodeFunction2 halCode = (CodeFunction2) halConfigurationClass.Children.OfType<CodeFunction>().FirstOrDefault(c => { ThreadHelper.ThrowIfNotOnUIThread(); return c.Name.Equals("HalConfiguration"); });

			if ( halCode != null )
            {
				codeService.AddLine(halCode, $"Links.Register(new {replacementsDictionary["$safeitemname$"]}());");
            }

			var configSpecs = new List<ConfigurationSpec>();

			foreach (CodeFunction2 codeFunction in controllerClass.Children.OfType<CodeFunction2>())
			{
				CodeAttribute2 getAttribute = codeFunction.Attributes.OfType<CodeAttribute2>().FirstOrDefault(c => c.Name.Equals("HttpGet", StringComparison.OrdinalIgnoreCase));

				if (getAttribute != null)
				{
					CodeAttribute2 routeAttribute = codeFunction.Attributes.OfType<CodeAttribute2>().FirstOrDefault(c => c.Name.Equals("Route", StringComparison.OrdinalIgnoreCase));
					CodeAttribute2 swaggerResponseAttribute = codeFunction.Attributes.OfType<CodeAttribute2>().FirstOrDefault(c => c.Name.Equals("SwaggerResponse", StringComparison.OrdinalIgnoreCase));

					if (routeAttribute != null && swaggerResponseAttribute != null)
					{
						var theRoute = routeAttribute.Value;
						var swag = swaggerResponseAttribute.Value;

						var match = Regex.Match(swag, "Type[ ]*=[ ]*typeof\\((?<returntype>[a-zA-Z0-9_\\<\\>]+)\\)");

						if (match.Success)
						{
							var spec = new ConfigurationSpec()
							{
								ResourceType = match.Groups["returntype"].Value,
								Route = theRoute.Substring(1, theRoute.Length - 2),
								FunctionName = codeFunction.Name
							};

							configSpecs.Add(spec);
						}
					}
				}
			}

			//	Generate the single spec
			foreach ( var spec in configSpecs )
            {
				if ( spec.ResourceType.Equals(className, StringComparison.OrdinalIgnoreCase))
                {
					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine("\t\t\t\t\"self\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");

					string nativeRoot = spec.Route;
					StringBuilder translatedRoot = new StringBuilder();
					bool isFinished = false;

					while (!isFinished)
					{
						var match2 = Regex.Match(nativeRoot, "\\{(?<member>[a-zA-Z0-9_]+)\\}");

						if (match2.Success)
						{
							var memberName = resourceClass.Members.OfType<CodeElement2>().FirstOrDefault(m => m.Name.Equals(match2.Groups["member"].Value, StringComparison.OrdinalIgnoreCase));

							translatedRoot.Append(nativeRoot.Substring(0, match2.Groups["member"].Index));
							translatedRoot.Append("c.");
							translatedRoot.Append(memberName.Name);

							nativeRoot = nativeRoot.Substring(match2.Groups["member"].Index + match2.Groups["member"].Length);
						}
						else
						{
							translatedRoot.Append(nativeRoot);
							isFinished = true;
						}
					}

					code.AppendLine($"\t\t\t\tc => $\"/{translatedRoot}\",");
					code.AppendLine("\t\t\t\tx => true);");
					code.AppendLine();
					bool hasExtensions = false;

					foreach ( var childspec in configSpecs )
                    {
						if (!childspec.ResourceType.Equals(className, StringComparison.OrdinalIgnoreCase) &&
							!childspec.ResourceType.Equals($"PagedSet<{className}>", StringComparison.OrdinalIgnoreCase) )
						{
							var extensionPath = childspec.Route.Substring(spec.Route.Length);
							var pathName = extensionPath.Substring(1);

							code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
							code.AppendLine($"\t\t\t\t\"ex:{pathName}\",");
							code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
							code.AppendLine($"\t\t\t\tc => \"{extensionPath}\",");
							code.AppendLine("\t\t\t\tx => true);");
							code.AppendLine();
							hasExtensions = true;
						}
					}

					if ( hasExtensions)
                    {
						var index = translatedRoot.ToString().IndexOf('{');

						if (index != -1)
						{
							var basePath = translatedRoot.ToString().Substring(0, index);
							var templatePath = translatedRoot.ToString().Substring(index);

							code.AppendLine($"\t\t\tLinks.AddLinkTemplate<{spec.ResourceType}>(");
							code.AppendLine($"\t\t\t\t\"curies\",");
							code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
							code.AppendLine("\t\t\t\t\"ex\",");
							code.AppendLine($"\t\t\t\t\"{basePath}\",");
							code.AppendLine($"\t\t\t\t(c, template) => $\"/{{template}}{templatePath}{{{{rel}}}}\",");
							code.AppendLine("\t\t\t\tx => true);");
							code.AppendLine();
						}
					}
				}
				else if ( spec.ResourceType.Equals($"PagedSet<{className}>", StringComparison.OrdinalIgnoreCase))
                {
					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine($"\t\t\t\t\"self\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
					code.AppendLine($"\t\t\t\tc => $\"/{spec.Route}?limit({{c.Start}},{{c.PageSize}})\",");
					code.AppendLine("\t\t\t\tx => true);");
					code.AppendLine();

					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine($"\t\t\t\t\"next\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
					code.AppendLine($"\t\t\t\tc => $\"/{spec.Route}?limit({{c.Start + c.PageSize}},{{c.PageSize}})\",");
					code.AppendLine("\t\t\t\tx => x.Start + x.PageSize <= x.Count);");
					code.AppendLine();

					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine($"\t\t\t\t\"previous\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
					code.AppendLine("\t\t\t\tc => {");
					code.AppendLine("\t\t\t\t\tvar startRecord = c.Start - c.PageSize;");
					code.AppendLine("\t\t\t\t\tif (startRecord < 1)");
					code.AppendLine("\t\t\t\t\t\tstartRecord = 1;");
					code.AppendLine($"\t\t\t\t\treturn $\"/{spec.Route}?limit({{startRecord}},{{c.PageSize}})\";");
					code.AppendLine("\t\t\t\t},");
					code.AppendLine("\t\t\t\tx => x.Start > 1);");
					code.AppendLine();

					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine($"\t\t\t\t\"first\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
					code.AppendLine($"\t\t\t\tc => $\"/{spec.Route}?limit(1,{{c.PageSize}})\",");
					code.AppendLine("\t\t\t\tx => x.Count < x.PageSize);");
					code.AppendLine();

					code.AppendLine($"\t\t\tLinks.AddLink<{spec.ResourceType}>(");
					code.AppendLine($"\t\t\t\t\"last\",");
					code.AppendLine($"\t\t\t\tnameof({controllerName}.{spec.FunctionName}),");
					code.AppendLine("\t\t\t\tc => {");
					code.AppendLine("\t\t\t\t\tvar remainder = c.Count % c.PageSize;");
					code.AppendLine("\t\t\t\t\tif (remainder == 0)");
					code.AppendLine("\t\t\t\t\t\tremainder = c.PageSize;");
					code.AppendLine("\t\t\t\t\tvar startRecord = c.Count - remainder + 1;");
					code.AppendLine($"\t\t\t\t\treturn $\"/{spec.Route}?limit({{startRecord}},{{c.PageSize}})\";");
					code.AppendLine("\t\t\t\t},");
					code.AppendLine("\t\t\t\tx => x.Count < x.PageSize);");
					code.AppendLine();
				}
			}

			code.AppendLine("\t\t}");
			code.AppendLine("\t}");

			return code.ToString();
        }

		public string EmitMappingModel(ResourceClass resourceModel, string mappingClassName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var codeService = ServiceFactory.GetService<ICodeService>();
			var ImageConversionRequired = false;
			var results = new StringBuilder();
			var nn = new NameNormalizer(resourceModel.ClassName);

			results.AppendLine("\t///\t<summary>");
			results.AppendLine($"\t///\t{nn.SingleForm} Profile for AutoMapper");
			results.AppendLine("\t///\t</summary>");

			results.AppendLine($"\tpublic class {mappingClassName} : Profile");
			results.AppendLine("\t{");

			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tInitializes the {nn.SingleForm} Profile");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine($"\t\tpublic {mappingClassName}()");
			results.AppendLine("\t\t{");

			#region Create the Resource to Entity Mapping
			results.AppendLine();
			results.AppendLine($"\t\t\t//\tCreates a mapping to transform a {resourceModel.ClassName} model instance (the source)");
			results.AppendLine($"\t\t\t//\tinto a {resourceModel.Entity.ClassName} model instance (the destination).");
			results.AppendLine($"\t\t\tCreateMap<{resourceModel.ClassName}, {resourceModel.Entity.ClassName}>()");

			bool first = true;

			var profileMap = codeService.GenerateProfileMap(resourceModel);

			foreach (var resourceMap in profileMap.EntityProfiles)
			{
				if (first)
					first = false;
				else
					results.AppendLine("))");

				results.Append($"\t\t\t\t.ForMember(destination => destination.{resourceMap.EntityColumnName}, opts => opts.MapFrom(source =>");
				results.Append(resourceMap.MapFunction);
			}

			results.AppendLine("));");
			results.AppendLine();

			#endregion

			#region Create Entity to Resource Mapping

			results.AppendLine($"\t\t\t//\tCreates a mapping to transform a {resourceModel.Entity.ClassName} model instance (the source)");
			results.AppendLine($"\t\t\t//\tinto a {resourceModel.ClassName} model instance (the destination).");
			results.AppendLine($"\t\t\tCreateMap<{resourceModel.Entity.ClassName}, {resourceModel.ClassName}>()");

			first = true;

			for (int j = 0; j < profileMap.ResourceProfiles.Count(); j++)
			{
				if (first)
				{
					first = false;
					results.Append($"\t\t\t\t.ForMember(destination => destination.{profileMap.ResourceProfiles[j].ResourceColumnName}, opts => opts.MapFrom(source => ");
					results.Append(profileMap.ResourceProfiles[j].MapFunction);
				}
				else
				{
					if (profileMap.ResourceProfiles[j].ResourceColumnName.CountOf('.') > 0)
					{
						j = GenerateChildMappings(results, j, profileMap.ResourceProfiles, profileMap.ResourceProfiles[j].ResourceColumnName.GetBaseColumn(), ref ImageConversionRequired);

						if (j < profileMap.ResourceProfiles.Count())
						{
							results.AppendLine("))");
							results.Append($"\t\t\t\t.ForMember(destination => destination.{profileMap.ResourceProfiles[j].ResourceColumnName}, opts => opts.MapFrom(source => ");
							results.Append(profileMap.ResourceProfiles[j].MapFunction);
						}
					}
					else
					{
						results.AppendLine("))");
						results.Append($"\t\t\t\t.ForMember(destination => destination.{profileMap.ResourceProfiles[j].ResourceColumnName}, opts => opts.MapFrom(source => ");
						results.Append(profileMap.ResourceProfiles[j].MapFunction);
					}
				}
			}

			results.AppendLine("));");
			results.AppendLine();

			#endregion

			results.AppendLine("\t\t}");
			results.AppendLine("\t}");

			return results.ToString();
		}
		
		private int GenerateChildMappings(StringBuilder results, int j, List<ResourceProfile> resourceProfiles, string baseColumn, ref bool ImageConversionRequired)
		{
			var baseCount = baseColumn.CountOf('.');
			var previousCount = resourceProfiles[j].ResourceColumnName.CountOf('.');
			results.AppendLine(" {");

			bool first = true;

			while (j < resourceProfiles.Count() - 1 && resourceProfiles[j].ResourceColumnName.CountOf('.') > baseCount)
			{
				var resourceCount = resourceProfiles[j].ResourceColumnName.CountOf('.');

				if (resourceCount > previousCount)
				{
					j = GenerateChildMappings(results, j, resourceProfiles, resourceProfiles[j].ResourceColumnName.GetBaseColumn(), ref ImageConversionRequired);

					if (j < resourceProfiles.Count() - 1)
					{
						results.AppendLine("))");
						results.Append($"\t\t\t\t.ForMember(destination => destination.{resourceProfiles[j].ResourceColumnName}, opts => opts.MapFrom(source => ");
						results.Append(resourceProfiles[j].MapFunction);
					}
				}
				else
				{
					if (first)
						first = false;
					else
						results.AppendLine(",");

					results.Append("\t\t\t\t\t");

					for (int q = 0; q < resourceCount; q++)
						results.Append("\t");

					var parts = resourceProfiles[j].ResourceColumnName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
					results.Append($"{parts[parts.Count() - 1]} = {resourceProfiles[j].MapFunction}");
				}

				previousCount = resourceCount;
				j++;
			}

			results.AppendLine();
			results.Append("\t\t\t\t\t}");

			return j;
		}

		/// <summary>
		/// Emits an entity data model based upon the fields contained within the database table
		/// </summary>
		/// <param name="serverType">The type of server used to house the table</param>
		/// <param name="table">The name of the database table</param>
		/// <param name="entityClassName">The class name for the model?</param>
		/// <param name="columns">The list of columns contained in the database</param>
		/// <param name="replacementsDictionary">List of replacements key/value pairs for the solution</param>
		/// <param name="connectionString">The connection string to connect to the database, if necessary</param>
		/// <returns>A model of the entity data table</returns>
		public string EmitEntityModel(string entityClassName, string schema, string tablename, DBServerType serverType, DBColumn[] columns, Dictionary<string, string> replacementsDictionary)
		{
			var result = new StringBuilder();
			replacementsDictionary.Add("$image$", "false");
			replacementsDictionary.Add("$net$", "false");
			replacementsDictionary.Add("$netinfo$", "false");
			replacementsDictionary.Add("$barray$", "false");

			result.AppendLine("\t///\t<summary>");
			result.AppendLine($"\t///\t{entityClassName}");
			result.AppendLine("\t///\t</summary>");

			if (string.IsNullOrWhiteSpace(schema))
				result.AppendLine($"\t[Table(\"{tablename}\", DBType = \"{serverType}\")]");
			else
				result.AppendLine($"\t[Table(\"{tablename}\", Schema = \"{schema}\", DBType = \"{serverType}\")]");

			result.AppendLine($"\tpublic class {entityClassName}");
			result.AppendLine("\t{");

			bool firstColumn = true;
			foreach (var column in columns)
			{
				if (firstColumn)
					firstColumn = false;
				else
					result.AppendLine();

				result.AppendLine("\t\t///\t<summary>");
				result.AppendLine($"\t\t///\t{column.ColumnName}");
				result.AppendLine("\t\t///\t</summary>");

				//	Construct the [Member] attribute
				result.Append("\t\t[Member(");
				bool first = true;

				if (column.IsPrimaryKey)
				{
					AppendPrimaryKey(result, ref first);
				}

				if (column.IsIdentity)
				{
					AppendIdentity(result, ref first);
				}

				if (column.IsIndexed || column.IsForeignKey)
				{
					AppendIndexed(result, ref first);
				}

				if (column.IsForeignKey)
				{
					AppendForeignKey(result, ref first);
					result.Append($", ForeignTableName=\"{column.ForeignTableName}\"");
				}

				AppendNullable(result, column.IsNullable, ref first);

				if (serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "NVarChar", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, false, ref first);
				}

				else if (serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "NChar", StringComparison.OrdinalIgnoreCase))
				{
					if (column.Length > 1)
						AppendFixed(result, column.Length, true, ref first);
				}

				else if (serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "NText", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, -1, false, ref first);
				}

				else if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "VarChar", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Varchar", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Name", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "_Varchar", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "VarChar", StringComparison.OrdinalIgnoreCase)))
				{
					if (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Varchar", StringComparison.OrdinalIgnoreCase) && column.Length < 0)
						AppendFixed(result, -1, false, ref first);
					else
						AppendFixed(result, column.Length, false, ref first);
				}

				else if ((serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Bit", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "_Bit", StringComparison.OrdinalIgnoreCase)))
				{
					//	Insert the column definition
					AppendFixed(result, column.Length, true, ref first);
				}

				else if ((serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Varbit", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "_Varbit", StringComparison.OrdinalIgnoreCase)))
				{
					AppendFixed(result, column.Length, false, ref first);
				}

				else if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "Text", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Text", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Citext", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "_Text", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "Text", StringComparison.OrdinalIgnoreCase)))
				{
					AppendFixed(result, -1, false, ref first);
				}

				else if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "Char", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Char", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "_Char", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "String", StringComparison.OrdinalIgnoreCase)))
				{
					//	Insert the column definition
					if (serverType == DBServerType.POSTGRESQL)
					{
						if (string.Equals(column.DBDataType, "bpchar", StringComparison.OrdinalIgnoreCase))
						{
							AppendFixed(result, column.Length, true, ref first);
						}
						else if (string.Equals(column.DBDataType, "_bpchar", StringComparison.OrdinalIgnoreCase))
						{
							AppendFixed(result, column.Length, true, ref first);
						}
					}
					else if (serverType == DBServerType.MYSQL)
					{
						if (column.Length != 1)
							AppendFixed(result, column.Length, true, ref first);
					}
					else
					{
						if (column.Length != 1)
							AppendFixed(result, column.Length, true, ref first);
					}
				}

				else if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "VarBinary", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Bytea", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "VarBinary", StringComparison.OrdinalIgnoreCase)))
				{
					AppendFixed(result, column.Length, false, ref first);
				}

				else if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "Binary", StringComparison.OrdinalIgnoreCase)) ||
						 (serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "Binary", StringComparison.OrdinalIgnoreCase)))
				{
					AppendFixed(result, column.Length, true, ref first);
				}

				else if (serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "Timestamp", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, true, ref first);
					AppendAutofield(result, ref first);
				}

				if ((serverType == DBServerType.SQLSERVER && string.Equals(column.DBDataType, "Decimal", StringComparison.OrdinalIgnoreCase)) ||
					(serverType == DBServerType.MYSQL && string.Equals(column.DBDataType, "Decimal", StringComparison.OrdinalIgnoreCase)) ||
					(serverType == DBServerType.POSTGRESQL && string.Equals(column.DBDataType, "Numeric", StringComparison.OrdinalIgnoreCase)))
				{
					AppendPrecision(result, column.NumericPrecision, column.NumericScale, ref first);
				}

				AppendDatabaseType(result, column, ref first);
				AppendEntityName(result, column, ref first);

				if (serverType == DBServerType.POSTGRESQL)
				{
					if (string.Equals(column.DBDataType, "Inet", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$net$"] = "true";

					if (string.Equals(column.DBDataType, "MacAddr", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$net$"] = "true";

					if (string.Equals(column.DBDataType, "MacAddr8", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$net$"] = "true";

					if (string.Equals(column.DBDataType, "_Boolean", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$barray$"] = "true";

					if (string.Equals(column.DBDataType, "_Bit", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$barray$"] = "true";

					if (string.Equals(column.DBDataType, "Bit", StringComparison.OrdinalIgnoreCase) && column.Length > 1)
						replacementsDictionary["$barray$"] = "true";

					if (string.Equals(column.DBDataType, "Varbit", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$barray$"] = "true";

					if (string.Equals(column.DBDataType, "Point", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "LSeg", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "Circle", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "Box", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "Line", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "Path", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";

					if (string.Equals(column.DBDataType, "Polygon", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$npgsqltypes$"] = "true";
				}
				else if (serverType == DBServerType.SQLSERVER)
				{
					if (string.Equals(column.DBDataType, "Image", StringComparison.OrdinalIgnoreCase))
						replacementsDictionary["$image$"] = "true";
				}

				result.AppendLine(")]");

				//	Insert the column definition
				if (serverType == DBServerType.POSTGRESQL)
				{
					column.ModelDataType = column.ModelDataType;
					result.AppendLine($"\t\tpublic {column.ModelDataType} {column.ColumnName} {{ get; set; }}");
				}
				else if (serverType == DBServerType.MYSQL)
				{
					column.ModelDataType = column.ModelDataType;
					result.AppendLine($"\t\tpublic {column.ModelDataType} {column.ColumnName} {{ get; set; }}");
				}
				else if (serverType == DBServerType.SQLSERVER)
				{
					if (column.ModelDataType.EndsWith("?"))
					{
						result.Append($"\t\tpublic {column.ModelDataType} {column.ColumnName} {{ get; set; }}");
					}
					else
					{
						if (column.IsNullable)
						{
							result.Append($"\t\tpublic {column.ModelDataType}? {column.ColumnName} {{ get; set; }}");
						}
						else
						{
							result.Append($"\t\tpublic {column.ModelDataType} {column.ColumnName} {{ get; set; }}");

							if (column.DefaultValue.Equals("(getutcdate())", StringComparison.OrdinalIgnoreCase))
							{
								result.Append(" = DateTime.UtcNow;");
							}
							else if (column.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase))
							{
								result.Append(" = string.Empty;");

							}
							else if (column.ModelDataType.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
							{
								result.Append(" = DateTime.UtcNow;");

							}
							else if (column.ModelDataType.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
							{
								result.Append(" = DateTimeOffset.UtcNow;");
							}
							else if (column.ModelDataType.Equals("Uri", StringComparison.OrdinalIgnoreCase))
							{
								result.Append(" = new Uri(\"Http:\\localhost\");");
							}
						}
					}

					result.AppendLine();
				}
			}

			result.AppendLine("\t}");

			return result.ToString();
		}

		/// <summary>
		/// Generates a resource model from a given entity model
		/// </summary>
		/// <param name="resourceClass">The <see cref="ResourceClass"/> to generate</param>
		/// <param name="replacementsDictionary">The replacements dictionary</param>
		/// <returns></returns>
		public string EmitResourceModel(string resourceClassName, EntityClass entityModel, bool useRql, Dictionary<string, string> replacementsDictionary)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			List<DBColumn> resourceColumns = new List<DBColumn>();
			var codeService = ServiceFactory.GetService<ICodeService>();
			var entityClasses = codeService.GetEntityClassList();

			replacementsDictionary.Add("$resourceimage$", "false");
			replacementsDictionary.Add("$resourcenet$", "false");
			replacementsDictionary.Add("$resourcenetinfo$", "false");
			replacementsDictionary.Add("$resourcebarray$", "false");
			replacementsDictionary.Add("$usenpgtypes$", "false");
			replacementsDictionary.Add("$annotations$", "false");

			var results = new StringBuilder();

			results.AppendLine("\t///\t<summary>");
			results.AppendLine($"\t///\t{resourceClassName}");
			results.AppendLine("\t///\t</summary>");
			results.AppendLine($"\t[Entity(typeof({entityModel.ClassName}))]");

			results.AppendLine($"\tpublic class {resourceClassName}");
			results.AppendLine("\t{");
			bool firstColumn = true;
			foreach (var member in entityModel.Columns)
			{
				if (firstColumn)
					firstColumn = false;
				else
					results.AppendLine();

				var membername = codeService.CorrectForReservedNames(codeService.NormalizeClassName(member.ColumnName));

				results.AppendLine("\t\t///\t<summary>");
				results.AppendLine($"\t\t///\t{membername}");
				results.AppendLine("\t\t///\t</summary>");

				if (entityModel.ServerType == DBServerType.SQLSERVER)
				{
					if (string.Equals(member.DBDataType, "Image", StringComparison.OrdinalIgnoreCase))
					{
						replacementsDictionary["$resourceimage$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "Date", StringComparison.OrdinalIgnoreCase))
					{
						results.AppendLine("\t\t[JsonFormat(\"yyyy-MM-dd\")]");
						replacementsDictionary["$annotations$"] = "true";
					}
				}
				else if (entityModel.ServerType == DBServerType.POSTGRESQL)
				{
					if (string.Equals(member.DBDataType, "Inet", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(member.DBDataType, "Cidr", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(member.DBDataType, "MacAddr", StringComparison.OrdinalIgnoreCase))
					{
						replacementsDictionary["$resourcenet$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "MacAddr8", StringComparison.OrdinalIgnoreCase))
					{
						replacementsDictionary["$resourcenetinfo$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "_Boolean", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "_Bit", StringComparison.OrdinalIgnoreCase) ||
							 (string.Equals(member.DBDataType, "Bit", StringComparison.OrdinalIgnoreCase) && member.Length > 1) ||
							 string.Equals(member.DBDataType, "VarBit", StringComparison.OrdinalIgnoreCase))
					{
						replacementsDictionary["$resourcebarray$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "Point", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "LSeg", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "Path", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "Circle", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "Polygon", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "Line", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "Box", StringComparison.OrdinalIgnoreCase))
					{
						replacementsDictionary["$usenpgtypes$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "Date", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "_Date", StringComparison.OrdinalIgnoreCase))
					{
						results.AppendLine("\t\t[JsonFormat(\"yyyy-MM-dd\")]");
						replacementsDictionary["$annotations$"] = "true";
					}
					else if (string.Equals(member.DBDataType, "TimeTz", StringComparison.OrdinalIgnoreCase) ||
							 string.Equals(member.DBDataType, "_TimeTz", StringComparison.OrdinalIgnoreCase))
					{
						results.AppendLine("\t\t[JsonFormat(\"HH:mm:ss.fffffffzzz\")]");
						replacementsDictionary["$annotations$"] = "true";
					}
				}
				else if (entityModel.ServerType == DBServerType.MYSQL)
				{
					if (string.Equals(member.DBDataType, "Date", StringComparison.OrdinalIgnoreCase))
					{
						results.AppendLine("\t\t[JsonFormat(\"yyyy-MM-dd\")]");
						replacementsDictionary["$annotations$"] = "true";
					}
				}

				if (member.IsForeignKey)
                {
					EntityClass relatedEntity = null;

					foreach ( var entityClass in entityClasses)
                    {
						if (entityClass.TableName.Equals(member.ForeignTableName, StringComparison.OrdinalIgnoreCase))
							relatedEntity = entityClass;
                    }

					if (relatedEntity != null && relatedEntity.ElementType == ElementType.Enum)
					{
						if (member.IsNullable)
							results.AppendLine($"\t\tpublic {relatedEntity.ClassName}? {membername} {{ get; set; }}");
						else
							results.AppendLine($"\t\tpublic {relatedEntity.ClassName} {membername} {{ get; set; }}");
					}
					else if (member.IsNullable)
					{
						ScriptNullables(results, member, membername);
					}
					else
					{
						ScriptNonNullables(results, member, membername);
					}
				}
				else if (member.IsNullable)
                {
                    ScriptNullables(results, member, membername);
                }
                else
                {
                    ScriptNonNullables(results, member, membername);
                }

                var resourceColumn = new DBColumn()
				{
					ColumnName = membername,
					IsPrimaryKey = member.IsPrimaryKey,
					IsForeignKey = member.IsForeignKey,
					IsComputed = member.IsComputed,
					IsFixed = member.IsFixed,
					IsIdentity = member.IsIdentity,
					IsIndexed = member.IsIndexed,
					IsNullable = member.IsNullable,
					ModelDataType = member.ModelDataType
				};

				resourceColumns.Add(resourceColumn);
			}

			results.AppendLine();
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tChecks the resource to see if it is in a valid state to update.");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine("\t\t///\t<param name=\"orchestrator\">The <see cref=\"IOrchestrator\"/> used to orchestrate operations.</param>");
			if (useRql)
				results.AppendLine("\t\t///\t<param name=\"node\">The <see cref=\"RqlNode\"/> that restricts the update.</param>");
			results.AppendLine("\t\t///\t<param name=\"errors\">The <see cref=\"ModelStateDictionary\"/> that will contain errors from failed validations.</param>");
			results.AppendLine("\t\t///\t<returns><see langword=\"true\"/> if the resource can be updated; <see langword=\"false\"/> otherwise</returns>");
			if (useRql)
				results.AppendLine("\t\tpublic async Task<bool> CanUpdateAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)");
			else
				results.AppendLine("\t\tpublic async Task<bool> CanUpdateAsync(IOrchestrator orchestrator, ModelStateDictionary errors)");
			results.AppendLine("\t\t{");
			results.AppendLine("\t\t\terrors.Clear();");
			results.AppendLine();

			if (useRql)
			{
				results.AppendLine($"\t\t\tvar existingValues = await orchestrator.GetResourceCollectionAsync<{resourceClassName}>(node);");
				results.AppendLine();
				results.AppendLine("\t\t\tif (existingValues.Count == 0)");
				results.AppendLine("\t\t\t{");
				results.AppendLine($"\t\t\t\terrors.AddModelError(\"Search\", \"No matching {resourceClassName} was found.\");");
				results.AppendLine("\t\t\t}");
				results.AppendLine();
				results.AppendLine("\t\t\tvar selectNode = node.ExtractSelectClause();");

				foreach (var member in entityModel.Columns)
				{
					if (member.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase))
					{
						var membername = codeService.CorrectForReservedNames(codeService.NormalizeClassName(member.ColumnName));

						results.AppendLine($"\t\t\tif (selectNode is null || (selectNode is not null && selectNode.SelectContains(nameof({membername}))))");
						results.AppendLine("\t\t\t{");

						if (!member.IsNullable)
						{
							results.AppendLine($"\t\t\t\tif (string.IsNullOrWhiteSpace({membername}))");
							results.AppendLine($"\t\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot be blank or null.\");");
						}

						if (member.Length > 0)
						{
							results.AppendLine($"\t\t\t\tif ({membername} is not null && {membername}.Length > {member.Length})");
							results.AppendLine($"\t\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot exceed {member.Length} characters.\");");
						}

						results.AppendLine("\t\t\t}");
					}
				}
			}
			else
			{
				foreach (var member in entityModel.Columns)
				{
					if (member.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase))
					{
						var membername = codeService.CorrectForReservedNames(codeService.NormalizeClassName(member.ColumnName));

						if (!member.IsNullable)
						{
							results.AppendLine($"\t\t\tif (string.IsNullOrWhiteSpace({membername}))");
							results.AppendLine($"\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot be blank or null.\");");
						}

						if (member.Length > 0)
						{
							results.AppendLine($"\t\t\tif ({membername} is not null && {membername}.Length > {member.Length})");
							results.AppendLine($"\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot exceed {member.Length} characters.\");");
						}
					}
				}
			}

			results.AppendLine("\t\t\treturn errors.IsValid;");
			results.AppendLine("\t\t}");

			results.AppendLine();
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tChecks the resource to see if it is in a valid state to add.");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine("\t\t/// <param name=\"orchestrator\">The <see cref=\"IOrchestrator\"/> used to orchestrate operations.</param>");
			results.AppendLine("\t\t/// <param name=\"errors\">The <see cref=\"ModelStateDictionary\"/> that will contain errors from failed validations.</param>");
			results.AppendLine("\t\t/// <returns><see langword=\"true\"/> if the resource can be updated; <see langword=\"false\"/> otherwise</returns>");
			results.AppendLine("\t\tpublic async Task<bool> CanAddAsync(IOrchestrator orchestrator, ModelStateDictionary errors)");
			results.AppendLine("\t\t{");
			results.AppendLine("\t\t\terrors.Clear();");
			results.AppendLine();

			foreach (var member in entityModel.Columns)
			{
				var membername = codeService.CorrectForReservedNames(codeService.NormalizeClassName(member.ColumnName));

				if (!member.IsNullable && member.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase))
				{
					results.AppendLine($"\t\t\tif (string.IsNullOrWhiteSpace({membername}))");
					results.AppendLine($"\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot be blank or null.\");");
				}

				if (member.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase) && member.Length > 0)
				{
					results.AppendLine($"\t\t\tif ({membername} is not null && {membername}.Length > {member.Length})");
					results.AppendLine($"\t\t\t\terrors.AddModelError(nameof({membername}), \"{membername} cannot exceed {member.Length} characters.\");");
				}
			}

			results.AppendLine();
			results.AppendLine("\t\t\tawait Task.CompletedTask;");
			results.AppendLine();
			results.AppendLine("\t\t\treturn errors.IsValid;");
			results.AppendLine("\t\t}");

			results.AppendLine();
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tChecks the resource to see if it is in a valid state to delete.");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine("\t\t///\t<param name=\"orchestrator\">The <see cref=\"IOrchestrator\"/> used to orchestrate operations.</param>");
			if (useRql)
				results.AppendLine("\t\t///\t<param name=\"node\">The <see cref=\"RqlNode\"/> that restricts the update.</param>");
			results.AppendLine("\t\t///\t<param name=\"errors\">The <see cref=\"ModelStateDictionary\"/> that will contain errors from failed validations.</param>");
			results.AppendLine("\t\t///\t<returns><see langword=\"true\"/> if the resource can be updated; <see langword=\"false\"/> otherwise</returns>");
			if (useRql)
				results.AppendLine("\t\tpublic static async Task<bool> CanDeleteAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)");
			else
				results.AppendLine("\t\tpublic static async Task<bool> CanDeleteAsync(IOrchestrator orchestrator, ModelStateDictionary errors)");
			results.AppendLine("\t\t{");
			results.AppendLine("\t\t\terrors.Clear();");
			results.AppendLine();

			if (useRql)
			{
				results.AppendLine($"\t\t\tvar existingValues = await orchestrator.GetResourceCollectionAsync<{resourceClassName}>(node);");
				results.AppendLine();
				results.AppendLine("\t\t\tif (existingValues.Count == 0)");
				results.AppendLine("\t\t\t{");
				results.AppendLine($"\t\t\t\terrors.AddModelError(\"Search\", \"No matching {resourceClassName} was found.\");");
				results.AppendLine("\t\t\t}");
				results.AppendLine();
			}

			results.AppendLine("\t\t\treturn errors.IsValid;");
			results.AppendLine("\t\t}");
			results.AppendLine("\t}");

			return results.ToString();
		}

        private static void ScriptNonNullables(StringBuilder results, DBColumn member, string membername)
        {
            if (member.ModelDataType.Equals("string", StringComparison.OrdinalIgnoreCase))
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }} = string.Empty;");
            else if (member.ModelDataType.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                results.AppendLine($"\t\tpublic DateTimeOffset {membername} {{ get; set; }} = DateTimeOffset.UtcNow.ToLocalTime();");
            else if (member.ModelDataType.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }} = DateTimeOffset.UtcNow.ToLocalTime();");
            else if (member.ModelDataType.Equals("TimeSpan", StringComparison.OrdinalIgnoreCase))
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }} = TimeSpan.FromSeconds(0);");
            else if (member.ModelDataType.Equals("Guid", StringComparison.OrdinalIgnoreCase))
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }} = Guid.Empty;");
            else
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }}");
        }

        private static void ScriptNullables(StringBuilder results, DBColumn member, string membername)
        {
            if (member.ModelDataType.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
            {
                results.AppendLine($"\t\tpublic DateTimeOffset? {membername} {{ get; set; }}");
            }
            else if (member.ModelDataType.Equals("DateTime?", StringComparison.OrdinalIgnoreCase))
            {
                results.AppendLine($"\t\tpublic DateTimeOffset? {membername} {{ get; set; }}");
            }
            else if (member.ModelDataType.EndsWith("?"))
            {
                results.AppendLine($"\t\tpublic {member.ModelDataType} {membername} {{ get; set; }}");
            }
            else
            {
                results.AppendLine($"\t\tpublic {member.ModelDataType}? {membername} {{ get; set; }}");
            }
        }

        public string EmitEntityEnum(string className, DBServerType serverType, string schema, string tablename, string enumDataType, List<EnumValue> columns)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			var nn = new NameNormalizer(className);
			var builder = new StringBuilder();

			builder.Clear();
			builder.AppendLine("\t///\t<summary>");
			builder.AppendLine($"\t///\tEnumerates a list of {nn.PluralForm}");
			builder.AppendLine("\t///\t</summary>");

			if (string.IsNullOrWhiteSpace(schema))
				builder.AppendLine($"\t[Table(\"{tablename}\", DBType = \"{serverType}\")]");
			else
				builder.AppendLine($"\t[Table(\"{tablename}\", Schema = \"{schema}\", DBType = \"{serverType}\")]");

			builder.AppendLine($"\tpublic enum {className} : {enumDataType}");
			builder.AppendLine("\t{");
			bool firstUse = true;

			foreach (var column in columns)
			{
				if (firstUse)
					firstUse = false;
				else
				{
					builder.AppendLine(",");
					builder.AppendLine();
				}

				builder.AppendLine("\t\t///\t<summary>");
				builder.AppendLine($"\t\t///\t{codeService.NormalizeEnumName(codeService.NormalizeClassName(column.Name))}");
				builder.AppendLine("\t\t///\t</summary>");

				var elementName = codeService.NormalizeEnumName(codeService.NormalizeClassName(column.Name));
				builder.Append($"\t\t{elementName} = {column.Value}");
			}

			builder.AppendLine();
			builder.AppendLine("\t}");

			return builder.ToString();
		}

		#region Helper Functions
		private void AppendComma(StringBuilder result, ref bool first)
		{
			if (first)
				first = false;
			else
				result.Append(", ");
		}
		private void AppendPrimaryKey(StringBuilder result, ref bool first)
		{
			AppendComma(result, ref first);
			result.Append("IsPrimaryKey = true");
		}

		private void AppendIdentity(StringBuilder result, ref bool first)
		{
			AppendComma(result, ref first);
			result.Append("IsIdentity = true, AutoField = true");
		}

		private void AppendIndexed(StringBuilder result, ref bool first)
		{
			AppendComma(result, ref first);
			result.Append("IsIndexed = true");
		}

		private void AppendForeignKey(StringBuilder result, ref bool first)
		{
			AppendComma(result, ref first);
			result.Append("IsForeignKey = true");
		}

		private void AppendNullable(StringBuilder result, bool isNullable, ref bool first)
		{
			AppendComma(result, ref first);

			if (isNullable)
				result.Append("IsNullable = true");
			else
				result.Append("IsNullable = false");
		}

		private void AppendDatabaseType(StringBuilder result, DBColumn column, ref bool first)
		{
			AppendComma(result, ref first);

			result.Append($"NativeDataType=\"{column.DBDataType}\"");
		}

		private void AppendFixed(StringBuilder result, long length, bool isFixed, ref bool first)
		{
			AppendComma(result, ref first);

			if (length == -1)
			{
				if (isFixed)
					result.Append($"IsFixed = true");
				else
					result.Append($"IsFixed = false");
			}
			else
			{
				if (isFixed)
					result.Append($"Length = {length}, IsFixed = true");
				else
					result.Append($"Length = {length}, IsFixed = false");
			}
		}

		private void AppendAutofield(StringBuilder result, ref bool first)
		{
			AppendComma(result, ref first);
			result.Append("AutoField = true");
		}

		private void AppendEntityName(StringBuilder result, DBColumn column, ref bool first)
		{
			if (!string.IsNullOrWhiteSpace(column.EntityName) && !string.Equals(column.ColumnName, column.EntityName, StringComparison.Ordinal))
			{
				if (!string.Equals(column.EntityName, column.DBDataType, StringComparison.Ordinal))
				{
					AppendComma(result, ref first);
					result.Append($"ColumnName = \"{column.EntityName}\"");
				}
			}
		}

		private void AppendPrecision(StringBuilder result, int NumericPrecision, int NumericScale, ref bool first)
		{
			AppendComma(result, ref first);

			result.Append($"Precision={NumericPrecision}, Scale={NumericScale}");
		}
		#endregion
		/// <summary>
		/// Emits the code for a standard controller.
		/// </summary>
		/// <param name="resourceClass">The <see cref="ResourceClassFile"/> associated with the controller.</param>
		/// <param name="controllerClassName">The class name for the controller</param>
		/// <param name="useRql"><see langword="true"/> if this controller supports RQL; <see langword="false"/> otherwise.</param>
		/// <param name="useHal"><see langword="true"/> if this controller supports HAL; <see langword="false"/> otherwise.</param>
		/// <param name="policy">The authentication policy used by the controller</param>
		/// <returns>The controller code</returns>
		public string EmitController(ResourceClass resourceClass, string controllerClassName, bool useRql, bool useHal, string policy)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

			StringBuilder results = new StringBuilder();
			var nn = new NameNormalizer(resourceClass.ClassName);
			var pkcolumns = resourceClass.Entity.Columns.Where(c => c.IsPrimaryKey);

			// --------------------------------------------------------------------------------
			//	Class
			// --------------------------------------------------------------------------------

			results.AppendLine("\t///\t<summary>");
			results.AppendLine($"\t///\t{resourceClass.ClassName} Controller");
			results.AppendLine("\t///\t</summary>");
			results.AppendLine("\t[ApiVersion(\"1.0\")]");
			results.AppendLine("\t[ApiController]");
			results.AppendLine($"\tpublic class {controllerClassName} : ControllerBase");
			results.AppendLine("\t{");
			results.AppendLine("\t\t///\t<value>A generic interface for logging where the category name is derrived from");
			results.AppendLine($"\t\t///\tthe specified <see cref=\"{controllerClassName}\"/> type name.</value>");
			results.AppendLine($"\t\tprivate readonly ILogger<{controllerClassName}> _logger;");
			results.AppendLine();
			results.AppendLine("\t\t///\t<value>The interface to the orchestration layer.</value>");
			results.AppendLine($"\t\tprivate readonly IOrchestrator _orchestrator;");
			results.AppendLine();

			// --------------------------------------------------------------------------------
			//	Constructor
			// --------------------------------------------------------------------------------

			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tInstantiates a {controllerClassName}");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine("\t\t///\t<param name=\"logger\">A generic interface for logging where the category name is derrived from");
			results.AppendLine($"\t\t///\tthe specified <see cref=\"{controllerClassName}\"/> type name. The logger is activated from dependency injection.</param>");
			results.AppendLine("\t\t///\t<param name=\"orchestrator\">The <see cref=\"IOrchestrator\"/> interface for the Orchestration layer. The orchestrator is activated from dependency injection.</param>");
			results.AppendLine($"\t\tpublic {controllerClassName}(ILogger<{controllerClassName}> logger, IOrchestrator orchestrator)");
			results.AppendLine("\t\t{");
			results.AppendLine("\t\t\t_logger = logger;");
			results.AppendLine("\t\t\t_orchestrator = orchestrator;");
			results.AppendLine("\t\t}");
			results.AppendLine();

			// --------------------------------------------------------------------------------
			//	GET Collection Endpoint
			// --------------------------------------------------------------------------------

			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tReturns a collection of {nn.PluralForm}");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine($"\t\t///\t<response code=\"200\">A collection of {nn.PluralForm}</response>");

			if (useRql)
			{
				results.AppendLine($"\t\t///\t<response code=\"400\">The RQL query was malformed.</response>");
			}

			if (!policy.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				results.AppendLine($"\t\t///\t<response code=\"401\">The user is not authorized to acquire this resource.</response>");
				results.AppendLine($"\t\t///\t<response code=\"403\">The user is not allowed to acquire this resource.</response>");
			}

			results.AppendLine("\t\t[HttpGet]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			if (policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[AllowAnonymous]");

			if (useRql)
			{
				results.AppendLine($"\t\t[SupportRQL]");
			}

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedSet<{resourceClass.ClassName}>))]");

			if (useHal)
				results.AppendLine($"\t\t[Produces(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
			else
				results.AppendLine($"\t\t[Produces(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");

			results.AppendLine($"\t\tpublic async Task<IActionResult> Get{nn.PluralForm}Async()");
			results.AppendLine("\t\t{");

			if (useRql)
			{
				results.AppendLine("\t\t\tvar node = RqlNode.Parse(Request.QueryString.Value);");
				results.AppendLine();
			}
			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();

			if (useRql)
			{
				results.AppendLine($"\t\t\tvar resourceCollection = await _orchestrator.GetResourceCollectionAsync<{resourceClass.ClassName}>(node);");
			}
			else
            {
				results.AppendLine($"\t\t\t//\tTo Do: Create an orchestration function that generates a PagedSet<{resourceClass.ClassName}> result.");
				results.AppendLine($"\t\t\tvar resourceCollection = new PagedSet<{resourceClass.ClassName}>();");
			}

			results.AppendLine($"\t\t\treturn Ok(resourceCollection);");
			results.AppendLine("\t\t}");

			results.AppendLine();

			// --------------------------------------------------------------------------------
			//	GET Single Endpoint
			// --------------------------------------------------------------------------------

			if (pkcolumns.Count() > 0)
			{
				results.AppendLine("\t\t///\t<summary>");
				results.AppendLine($"\t\t///\tReturns a {nn.SingleForm}");
				results.AppendLine("\t\t///\t</summary>");
				EmitEndpointExamples(resourceClass.Entity.ServerType, resourceClass.ClassName, results, pkcolumns);
				if (useRql)
				{
					results.AppendLine($"\t\t///\t<response code=\"400\">The RQL query was malformed.</response>");
				}

				if (!policy.Equals("none", StringComparison.OrdinalIgnoreCase))
				{
					results.AppendLine($"\t\t///\t<response code=\"401\">The user is not authorized to acquire this resource.</response>");
					results.AppendLine($"\t\t///\t<response code=\"403\">The user is not allowed to acquire this resource.</response>");
				}
				results.AppendLine($"\t\t///\t<response code=\"404\">The requested resource was not found.</response>");
				results.AppendLine("\t\t[HttpGet]");
				EmitRoute(results, nn.PluralCamelCase, pkcolumns);

				if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

				if (policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[AllowAnonymous]");

				if (useRql)
				{
					results.AppendLine($"\t\t[SupportRQL]");
				}

				results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof({resourceClass.ClassName}))]");
				if (useHal)
					results.AppendLine($"\t\t[Produces(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				else
					results.AppendLine($"\t\t[Produces(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");

				EmitEndpoint(resourceClass.ClassName, "Get", results, pkcolumns);

				results.AppendLine("\t\t{");

				var firstMember = true;
				results.Append("\t\t\tvar node = RqlNode.Parse($\"");
				foreach ( var column in pkcolumns)
                {
					if (column.IsPrimaryKey)
					{
						if (firstMember)
							firstMember = false;
						else
							results.Append("&");

						results.Append($"{column.ColumnName}={{{column.ColumnName.CammelCase()}}}");
					}
                }

				results.AppendLine("\")");
				results.AppendLine($"\t\t\t\t\t\t\t  .Merge(RqlNode.Parse(Request.QueryString.Value));");
				results.AppendLine();

				results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");

				if (useRql)
				{
					results.AppendLine($"\t\t\tvar resource = await _orchestrator.GetSingleResourceAsync<{resourceClass.ClassName}>(node);");
				}
				else
                {
					results.AppendLine($"\t\t\t//\tTo Do: Create an orchestration function that generates a {resourceClass.ClassName} result.");
					results.AppendLine($"\t\t\tvar resource = new {resourceClass.ClassName}();");
				}
				results.AppendLine();
				results.AppendLine("\t\t\tif (resource is null)");
				results.AppendLine("\t\t\t\treturn NotFound();");
				results.AppendLine();
				results.AppendLine("\t\t\treturn Ok(resource);");

				results.AppendLine("\t\t}");
				results.AppendLine();
			}

			// --------------------------------------------------------------------------------
			//	POST Endpoint
			// --------------------------------------------------------------------------------
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tAdds a {nn.SingleForm}");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine($"\t\t///\t<remarks>Add a {nn.SingleForm} to the datastore.</remarks>");
			results.AppendLine($"\t\t///\t<response code=\"201\">The new {nn.SingleForm} was successfully added.</response>");
			results.AppendLine($"\t\t///\t<response code=\"400\">The request failed one or more validations.</response>");
			if (!policy.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				results.AppendLine($"\t\t///\t<response code=\"401\">The user is not authorized to acquire this resource.</response>");
				results.AppendLine($"\t\t///\t<response code=\"403\">The user is not allowed to acquire this resource.</response>");
			}
			results.AppendLine("\t\t[HttpPost]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			if (policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[AllowAnonymous]");

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.Created, Type = typeof({resourceClass.ClassName}))]");
			if (useHal)
			{
				results.AppendLine($"\t\t[Produces(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				results.AppendLine($"\t\t[Consumes(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
			}
			else
			{
				results.AppendLine($"\t\t[Produces(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				results.AppendLine($"\t\t[Consumes(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
			}

			results.AppendLine($"\t\tpublic async Task<IActionResult> Add{resourceClass.ClassName}Async([FromBody] {resourceClass.ClassName} resource)");
			results.AppendLine("\t\t{");

			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();
			results.AppendLine("\t\t\tModelStateDictionary errors = new();");
			results.AppendLine();
			results.AppendLine("\t\t\tif (await resource.CanAddAsync(_orchestrator, errors))");
			results.AppendLine("\t\t\t{");

			if (useRql)
				results.AppendLine($"\t\t\t\tresource = await _orchestrator.AddResourceAsync<{resourceClass.ClassName}>(resource);");
			else
				results.AppendLine($"\t\t\t\t//\tTo Do: create an orchestration function to add the resource.");

			results.Append($"\t\t\t\treturn Created($\"{{Request.Scheme}}://{{Request.Host}}/{nn.PluralCamelCase}"); foreach (var column in pkcolumns)
			{
				if (column.IsPrimaryKey)
				{
					results.Append($"/{{resource.{column.ColumnName}}}");
				}
			}
			results.AppendLine("\", resource);");

			results.AppendLine("\t\t\t}");
			results.AppendLine("\t\t\telse");
			results.AppendLine("\t\t\t\treturn BadRequest(errors);");

			results.AppendLine("\t\t}");
			results.AppendLine();

			// --------------------------------------------------------------------------------
			//	PUT Endpoint
			// --------------------------------------------------------------------------------
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tUpdate a {nn.SingleForm}");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine($"\t\t///\t<remarks>Update a {nn.SingleForm} in the datastore.</remarks>");
			results.AppendLine($"\t\t///\t<response code=\"204\">The {nn.SingleForm} was successfully updated in the datastore.</response>");
			results.AppendLine($"\t\t///\t<response code=\"400\">The request failed one or more validations.</response>");
			if (!policy.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				results.AppendLine($"\t\t///\t<response code=\"401\">The user is not authorized to acquire this resource.</response>");
				results.AppendLine($"\t\t///\t<response code=\"403\">The user is not allowed to acquire this resource.</response>");
			}
			results.AppendLine("\t\t[HttpPut]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			if (policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[AllowAnonymous]");

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.NoContent)]");
			if (useHal)
			{
				results.AppendLine($"\t\t[Produces(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				results.AppendLine($"\t\t[Consumes(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
			}
			else
			{
				results.AppendLine($"\t\t[Produces(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				results.AppendLine($"\t\t[Consumes(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
			}
			results.AppendLine($"\t\tpublic async Task<IActionResult> Update{resourceClass.ClassName}Async([FromBody] {resourceClass.ClassName} resource)");
			results.AppendLine("\t\t{");

			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();

			var firstColumn = true;
			results.Append("\t\t\tvar node = RqlNode.Parse($\"");
			foreach (var column in pkcolumns)
			{
				if (column.IsPrimaryKey)
				{
					if (firstColumn)
						firstColumn = false;
					else
						results.Append("&");

					results.Append($"{column.ColumnName}={{resource.{column.ColumnName}}}");
				}
			}

			results.AppendLine("\")");
			results.AppendLine($"\t\t\t\t\t\t\t  .Merge(RqlNode.Parse(Request.QueryString.Value));");
			results.AppendLine();
			results.AppendLine("\t\t\tModelStateDictionary errors = new();");
			results.AppendLine();
			results.AppendLine("\t\t\tif (await resource.CanUpdateAsync(_orchestrator, node, errors))");
			results.AppendLine("\t\t\t{");

			if ( useRql )
				results.AppendLine($"\t\t\t\tawait _orchestrator.UpdateResourceAsync<{resourceClass.ClassName}>(resource, node);");
			else
				results.AppendLine($"\t\t\t\t//\tTo Do: create an orchestration function to update the resource.");

			results.AppendLine($"\t\t\t\treturn NoContent();");
			results.AppendLine("\t\t\t}");
			results.AppendLine("\t\t\telse");
			results.AppendLine("\t\t\t\treturn BadRequest(errors);");

			results.AppendLine("\t\t}");
			results.AppendLine();

			if (pkcolumns.Count() > 0)
			{
				// --------------------------------------------------------------------------------
				//	DELETE Endpoint
				// --------------------------------------------------------------------------------

				results.AppendLine("\t\t///\t<summary>");
				results.AppendLine($"\t\t///\tDelete a {nn.SingleForm}");
				results.AppendLine("\t\t///\t</summary>");
				EmitEndpointExamples(resourceClass.Entity.ServerType, resourceClass.ClassName, results, pkcolumns);
				results.AppendLine($"\t\t///\t<remarks>Deletes a {nn.SingleForm} in the datastore.</remarks>");
				results.AppendLine($"\t\t///\t<response code=\"204\">The {nn.SingleForm} was successfully deleted.</response>");
				results.AppendLine($"\t\t///\t<response code=\"400\">The request failed one or more validations.</response>");
				if (!policy.Equals("none", StringComparison.OrdinalIgnoreCase))
				{
					results.AppendLine($"\t\t///\t<response code=\"401\">The user is not authorized to acquire this resource.</response>");
					results.AppendLine($"\t\t///\t<response code=\"403\">The user is not allowed to acquire this resource.</response>");
				}
				results.AppendLine("\t\t[HttpDelete]");
				EmitRoute(results, nn.PluralCamelCase, pkcolumns);

				if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

				if (policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[AllowAnonymous]");

				results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.NoContent)]");
				if (useHal)
				{
					results.AppendLine($"\t\t[Produces(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
					results.AppendLine($"\t\t[Consumes(\"application/hal+json\", \"application/hal.v1+json\", MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				}
				else
				{
					results.AppendLine($"\t\t[Produces(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
					results.AppendLine($"\t\t[Consumes(MediaTypeNames.Application.Json, \"application/vnd.v1+json\")]");
				}
				EmitEndpoint(resourceClass.ClassName, "Delete", results, pkcolumns);

				results.AppendLine("\t\t{");
				results.Append("\t\t\tvar node = RqlNode.Parse($\"");
				foreach (var column in pkcolumns)
				{
					if (column.IsPrimaryKey)
					{
						if (firstColumn)
							firstColumn = false;
						else
							results.Append("&");

						results.Append($"{column.ColumnName}={{{column.ColumnName.CammelCase()}}}");
					}
				}

				results.AppendLine("\");");
				results.AppendLine();
				results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
				results.AppendLine();
				results.AppendLine();
				results.AppendLine("\t\t\tModelStateDictionary errors = new();");
				results.AppendLine();
				results.AppendLine($"\t\t\tif (await {resourceClass.ClassName}.CanDeleteAsync(_orchestrator, node, errors))");
				results.AppendLine("\t\t\t{");

				if (useRql)
					results.AppendLine($"\t\t\t\tawait _orchestrator.DeleteResourceAsync<{resourceClass.ClassName}>(node);");
				else
					results.AppendLine($"\t\t\t\t//\tTo Do: construct an orchestration function to delete the resource.");

				results.AppendLine($"\t\t\t\treturn NoContent();");
				results.AppendLine("\t\t\t}");
				results.AppendLine("\t\t\telse");
				results.AppendLine("\t\t\t\treturn BadRequest(errors);");

				results.AppendLine("\t\t}");
				results.AppendLine("\t}");
			}

			return results.ToString();
		}

		private void EmitEndpointExamples(DBServerType serverType, string resourceClassName, StringBuilder results, IEnumerable<DBColumn> pkcolumns)
		{
			bool first = true;

			foreach (var entityColumn in pkcolumns)
			{
				if (first)
					first = false;
				else
					results.Append(", ");

				string exampleValue = "example";

				if (serverType == DBServerType.POSTGRESQL)
					exampleValue = DBHelper.GetPostgresqlExampleValue(entityColumn);
				else if (serverType == DBServerType.MYSQL)
					exampleValue = DBHelper.GetMySqlExampleValue(entityColumn);
				else if (serverType == DBServerType.SQLSERVER)
					exampleValue = DBHelper.GetSqlServerExampleValue(entityColumn);

				results.AppendLine($"\t\t///\t<param name=\"{entityColumn.EntityName}\" example=\"{exampleValue}\">The {entityColumn.EntityName} of the {resourceClassName}.</param>");
			}
		}

		private void EmitEndpoint(string resourceClassName, string action, StringBuilder results, IEnumerable<DBColumn> pkcolumns)
		{
			results.Append($"\t\tpublic async Task<IActionResult> {action}{resourceClassName}Async(");
			bool first = true;

			foreach (var entityColumn in pkcolumns)
			{
				if (first)
					first = false;
				else
					results.Append(", ");

				string dataType = entityColumn.ModelDataType;

				results.Append($"{dataType} {entityColumn.EntityName.CammelCase()}");
			}

			results.AppendLine(")");
		}

		private static void EmitRoute(StringBuilder results, string routeName, IEnumerable<DBColumn> pkcolumns)
		{
			results.Append($"\t\t[Route(\"{routeName}");

			foreach (var entityColumn in pkcolumns)
			{
				results.Append($"/{{{entityColumn.EntityName.CammelCase()}}}");
			}

			results.AppendLine("\")]");
		}

	}
}
