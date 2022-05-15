using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WizardInstaller.Template.Extensions;
using WizardInstaller.Template.Models;

namespace WizardInstaller.Template.Services
{
	internal class Emitter
	{
		public string EmitResourceEnum(ICodeService codeService, string resourceClassName, EntityClass model)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (model.ServerType == DBServerType.MYSQL)
				return EmitResourceMySqlEnum(codeService, resourceClassName, model);
			else if (model.ServerType == DBServerType.POSTGRESQL)
				return EmitResourcePostgresqlEnum(codeService, resourceClassName, model);
			else if (model.ServerType == DBServerType.SQLSERVER)
				return EmitResourceSqlServerEnum(codeService, resourceClassName, model);

			return "Invalid DB Server Type";
		}

		private static string EmitResourceMySqlEnum(ICodeService codeService, string resourceClassName, EntityClass model)
		{
			throw new NotImplementedException("not implemented yet");
		}

		private static string EmitResourcePostgresqlEnum(ICodeService codeService, string resourceClassName, EntityClass model)
		{
			throw new NotImplementedException("not implemented yet");
		}

		private static string EmitResourceSqlServerEnum(ICodeService codeService, string resourceClassName, EntityClass entityModel)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			StringBuilder results = new StringBuilder();

			results.AppendLine("\t///\t<summary>");
			results.AppendLine($"\t///\t{resourceClassName}");
			results.AppendLine("\t///\t</summary>");
			results.AppendLine($"\t[Entity(typeof({entityModel.ClassName}))]");

			var dataType = entityModel.Columns[0].ModelDataType;

			results.AppendLine($"\tpublic enum {resourceClassName} : {dataType}");
			results.AppendLine("\t{");

			bool firstColumn = true;

			string query = "select ";

			foreach (var col in entityModel.Columns)
			{
				if (firstColumn)
					firstColumn = false;
				else
				{
					query += ", ";
				}

				query += col.ColumnName;
			}

			query += " from ";
			query += entityModel.TableName;

			firstColumn = true;
			var connectionString = codeService.GetConnectionStringForEntity(entityModel.ClassName);

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				using (var command = new SqlCommand(query, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (firstColumn)
							{
								firstColumn = false;
							}
							else
							{
								results.AppendLine(",");
								results.AppendLine();
							}

							if (string.Equals(entityModel.Columns[0].DBDataType, "tinyint", StringComparison.OrdinalIgnoreCase))
							{
								var theValue = reader.GetByte(0);
								var name = reader.GetString(1);

								results.AppendLine("\t\t///\t<summary>");
								results.AppendLine($"\t\t///\t{name}");
								results.AppendLine("\t\t///\t</summary>");
								results.Append($"\t\t{name.Replace(" ", "")} = {theValue}");
							}
							else if (string.Equals(entityModel.Columns[0].DBDataType, "smallint", StringComparison.OrdinalIgnoreCase))
							{
								var theValue = reader.GetInt16(0);
								var name = reader.GetString(1);

								results.AppendLine("\t\t///\t<summary>");
								results.AppendLine($"\t\t///\t{name}");
								results.AppendLine("\t\t///\t</summary>");
								results.Append($"\t\t{name.Replace(" ", "")} = {theValue}");
							}
							else if (string.Equals(entityModel.Columns[0].DBDataType, "int", StringComparison.OrdinalIgnoreCase))
							{
								var theValue = reader.GetInt32(0);
								var name = reader.GetString(1);

								results.AppendLine("\t\t///\t<summary>");
								results.AppendLine($"\t\t///\t{name}");
								results.AppendLine("\t\t///\t</summary>");
								results.Append($"\t\t{name.Replace(" ", "")} = {theValue}");
							}
							else if (string.Equals(entityModel.Columns[0].DBDataType, "bigint", StringComparison.OrdinalIgnoreCase))
							{
								var theValue = reader.GetInt64(0);
								var name = reader.GetString(1);

								results.AppendLine("\t\t///\t<summary>");
								results.AppendLine($"\t\t///\t{name}");
								results.AppendLine("\t\t///\t</summary>");
								results.Append($"\t\t{name.Replace(" ", "")} = {theValue}");
							}
						}
					}
				}
			}

			results.AppendLine();

			results.AppendLine("\t}");

			return results.ToString();
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

			if (entityModel.ElementType == ElementType.Enum)
			{
				results.AppendLine($"\tpublic enum {resourceClassName}");
				results.AppendLine("\t{");

				bool firstColumn = true;
				foreach (var member in entityModel.Columns)
				{
					if (firstColumn)
						firstColumn = false;
					else
					{
						results.AppendLine(",");
						results.AppendLine();
					}

					var membername = codeService.CorrectForReservedNames(codeService.NormalizeClassName(member.ColumnName));

					results.AppendLine("\t\t///\t<summary>");
					results.AppendLine($"\t\t///\t{membername}");
					results.AppendLine("\t\t///\t</summary>");
					results.Append($"\t\t{membername}");

					var resourceColumn = new DBColumn()
					{
						ColumnName = membername
					};

					resourceColumns.Add(resourceColumn);

				}
			}
			else
			{
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


					if (member.IsNullable)
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
					else
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
				if ( useRql ) 
					results.AppendLine("\t\t///\t<param name=\"node\">The <see cref=\"RqlNode\"/> that restricts the update.</param>");
				results.AppendLine("\t\t///\t<param name=\"errors\">The <see cref=\"ModelStateDictionary\"/> that will contain errors from failed validations.</param>");
				results.AppendLine("\t\t///\t<returns><see langword=\"true\"/> if the resource can be updated; <see langword=\"false\"/> otherwise</returns>");
				if ( useRql )
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
				if ( useRql ) 
					results.AppendLine("\t\t///\t<param name=\"node\">The <see cref=\"RqlNode\"/> that restricts the update.</param>");
				results.AppendLine("\t\t///\t<param name=\"errors\">The <see cref=\"ModelStateDictionary\"/> that will contain errors from failed validations.</param>");
				results.AppendLine("\t\t///\t<returns><see langword=\"true\"/> if the resource can be updated; <see langword=\"false\"/> otherwise</returns>");
				if ( useRql)
					results.AppendLine("\t\tpublic async Task<bool> CanDeleteAsync(IOrchestrator orchestrator, RqlNode node, ModelStateDictionary errors)");
				else
					results.AppendLine("\t\tpublic async Task<bool> CanDeleteAsync(IOrchestrator orchestrator, ModelStateDictionary errors)");
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
			}

			return results.ToString();
		}

		public string EmitEntityEnum(string className, string schema, string tablename, DBColumn[] columns)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			var nn = new NameNormalizer(className);
			var builder = new StringBuilder();

			builder.Clear();
			builder.AppendLine("\t///\t<summary>");
			builder.AppendLine($"\t///\tEnumerates a list of {nn.PluralForm}");
			builder.AppendLine("\t///\t</summary>");

			if (string.IsNullOrWhiteSpace(schema))
				builder.AppendLine($"\t[PgEnum(\"{tablename}\")]");
			else
				builder.AppendLine($"\t[PgEnum(\"{tablename}\", Schema = \"{schema}\")]");

			builder.AppendLine($"\tpublic enum {className}");
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
				builder.AppendLine($"\t\t///\t{codeService.NormalizeClassName(column.ColumnName)}");
				builder.AppendLine("\t\t///\t</summary>");
				builder.AppendLine($"\t\t[PgName(\"{column.EntityName}\")]");

				var elementName = codeService.NormalizeClassName(column.ColumnName);
				builder.Append($"\t\t{elementName}");
			}

			builder.AppendLine();
			builder.AppendLine("\t}");

			return builder.ToString();
		}

		public string EmitComposite(string className, string schema, string tableName, DBColumn[] columns, Dictionary<string, string> replacementsDictionary)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var codeService = ServiceFactory.GetService<ICodeService>();
			var result = new StringBuilder();

			result.Clear();
			result.AppendLine("\t///\t<summary>");
			result.AppendLine($"\t///\t{className}");
			result.AppendLine("\t///\t</summary>");

			if (string.IsNullOrWhiteSpace(schema))
				result.AppendLine($"\t[PgComposite(\"{tableName}\")]");
			else
				result.AppendLine($"\t[PgComposite(\"{tableName}\", Schema = \"{schema}\")]");

			result.AppendLine($"\tpublic class {className}");
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
				}

				AppendNullable(result, column.IsNullable, ref first);


				if (string.Equals(column.DBDataType, "Varchar", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(column.DBDataType, "Name", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(column.DBDataType, "_Varchar", StringComparison.OrdinalIgnoreCase))
				{
					if (string.Equals(column.DBDataType, "Varchar", StringComparison.OrdinalIgnoreCase) && column.Length < 0)
						AppendFixed(result, -1, false, ref first);
					else
						AppendFixed(result, column.Length, false, ref first);
				}

				else if (string.Equals(column.DBDataType, "Bit", StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(column.DBDataType, "_Bit", StringComparison.OrdinalIgnoreCase))
				{
					//	Insert the column definition
					AppendFixed(result, column.Length, true, ref first);
				}

				else if (string.Equals(column.DBDataType, "Varbit", StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(column.DBDataType, "_Varbit", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, false, ref first);
				}

				else if (string.Equals(column.DBDataType, "Text", StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(column.DBDataType, "Citext", StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(column.DBDataType, "_Text", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, -1, false, ref first);
				}

				//	Insert the column definition
				else if (string.Equals(column.DBDataType, "bpchar", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, true, ref first);
				}
				else if (string.Equals(column.DBDataType, "_bpchar", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, true, ref first);
				}

				else if (string.Equals(column.DBDataType, "bytea", StringComparison.OrdinalIgnoreCase))
				{
					AppendFixed(result, column.Length, false, ref first);
				}

				else if (string.Equals(column.DBDataType, "numeric", StringComparison.OrdinalIgnoreCase))
				{
					AppendPrecision(result, column.NumericPrecision, column.NumericScale, ref first);
				}

				AppendDatabaseType(result, column, ref first);
				AppendEntityName(result, column, ref first);

				if (string.Equals(column.DBDataType, "INet", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$net$"] = "true";

				else if (string.Equals(column.DBDataType, "Cidr", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$net$"] = "true";

				else if (string.Equals(column.DBDataType, "MacAddr", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$netinfo$"] = "true";

				else if (string.Equals(column.DBDataType, "MacAddr8", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$netinfo$"] = "true";

				else if (string.Equals(column.DBDataType, "_Boolean", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$barray$"] = "true";

				else if (string.Equals(column.DBDataType, "_Bit", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$barray$"] = "true";

				else if (string.Equals(column.DBDataType, "Bit", StringComparison.OrdinalIgnoreCase) && column.Length > 1)
					replacementsDictionary["$barray$"] = "true";

				else if (string.Equals(column.DBDataType, "varbit", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$barray$"] = "true";

				else if (string.Equals(column.DBDataType, "Point", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "LSeg", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "Circle", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "Box", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "Line", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "Path", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				else if (string.Equals(column.DBDataType, "Polygon", StringComparison.OrdinalIgnoreCase))
					replacementsDictionary["$npgsqltypes$"] = "true";

				result.AppendLine(")]");

				var memberName = codeService.CorrectForReservedNames(codeService.NormalizeClassName(column.ColumnName));
				result.AppendLine($"\t\t[PgName(\"{column.ColumnName}\")]");

				//	Insert the column definition
				result.AppendLine($"\t\tpublic {DBHelper.GetPostgresDataType(column)} {memberName} {{ get; set; }}");
			}

			result.AppendLine("\t}");

			return result.ToString();
		}

		/// <summary>
		/// Generate undefined elements
		/// </summary>
		/// <param name="dte2"></param>
		/// <param name="undefinedEntityModels">The list of elements to be defined"/></param>
		/// <param name="connectionString">The connection string to the database server</param>
		/// <param name="replacementsDictionary">The replacements dictionary</param>
		public void GenerateComposites(List<EntityModel> undefinedEntityModels, string connectionString, Dictionary<string, string> replacementsDictionary)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var codeService = ServiceFactory.GetService<ICodeService>();

			//	As we generate each model, the model itself may contain undefined types, which in turn need to be added
			//	to the list of undefinedEntityModels so that they too can be generated. Because of this, each successive
			//	generation might (or might not) add to the list of undefinedEntityModels.
			//
			//	This is why the undefinedEntityModels object is being passed as a reference.
			//
			//	This also means that we can't simply iterate through the list with a foreach - because that list is liable to change.
			//
			//	Therefore, we simply pop the top one off the list, treating the list as a todo stack. And we keep doing that until
			//	the stack is empty.

			foreach (var undefinedModel in undefinedEntityModels)
			{
				if (undefinedModel.ElementType == ElementType.Enum)
				{
					//	Has it already been previously defined? We don't want two of these...
					if (codeService.GetEntityClass(undefinedModel.ClassName) == null)
					{
						//	Generate the model
						var result = new StringBuilder();

						result.AppendLine("using Rql;");
						result.AppendLine("using NpgsqlTypes;");
						result.AppendLine();
						result.AppendLine($"namespace {undefinedModel.Namespace}");
						result.AppendLine("{");

						var columns = DBHelper.GenerateColumns(undefinedModel.SchemaName, undefinedModel.TableName, undefinedModel.ServerType, connectionString);
						result.Append(EmitEntityEnum(undefinedModel.ClassName, undefinedModel.SchemaName, undefinedModel.TableName, columns));
						result.AppendLine("}");

						//	Save the model to disk
						if (!Directory.Exists(Path.GetDirectoryName(undefinedModel.Folder)))
							Directory.CreateDirectory(Path.GetDirectoryName(undefinedModel.Folder));

						File.WriteAllText(undefinedModel.Folder, result.ToString());

						//	Add the model to the project
						var parentProject = codeService.GetProjectFromFolder(undefinedModel.Folder);
						ProjectItem entityItem;

						if (parentProject.GetType() == typeof(Project))
							entityItem = ((Project)parentProject).ProjectItems.AddFromFile(undefinedModel.Folder);
						else
							entityItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(undefinedModel.Folder);

						codeService.AddEntity(entityItem);

						//	Register the composite model
						codeService.RegisterComposite(undefinedModel.ClassName,
													  undefinedModel.Namespace,
													  undefinedModel.ElementType,
													  undefinedModel.TableName);
					}
				}
				else if (undefinedModel.ElementType == ElementType.Composite)
				{
					//	Has it already been defined? We don't want two of these...
					if (codeService.GetEntityClass(undefinedModel.ClassName) == null)
					{
						var result = new StringBuilder();

						//	Generate the model (and any child models that might be necessary)

						var columns = DBHelper.GenerateColumns(undefinedModel.SchemaName, undefinedModel.TableName, undefinedModel.ServerType, connectionString);

						var body = EmitComposite(undefinedModel.ClassName,
												 undefinedModel.SchemaName,
												 undefinedModel.TableName,
												 columns,
												 replacementsDictionary);

						result.AppendLine("using Rql;");
						result.AppendLine("using NpgsqlTypes;");

						if (replacementsDictionary.ContainsKey("$net$"))
						{
							if (string.Equals(replacementsDictionary["$net$"], "true", StringComparison.OrdinalIgnoreCase))
								result.AppendLine("using System.Net;");
						}

						if (replacementsDictionary.ContainsKey("$barray$"))
						{
							if (string.Equals(replacementsDictionary["$barray$"], "true", StringComparison.OrdinalIgnoreCase))
								result.AppendLine("using System.Collections;");
						}

						if (replacementsDictionary.ContainsKey("$image$"))
						{
							if (string.Equals(replacementsDictionary["$image$"], "true", StringComparison.OrdinalIgnoreCase))
								result.AppendLine("using System.Drawing;");
						}

						if (replacementsDictionary.ContainsKey("$netinfo$"))
						{
							if (string.Equals(replacementsDictionary["$netinfo$"], "true", StringComparison.OrdinalIgnoreCase))
								result.AppendLine("using System.Net.NetworkInformation;");
						}

						result.AppendLine();
						result.AppendLine($"namespace {undefinedModel.Namespace}");
						result.AppendLine("{");
						result.Append(body);
						result.AppendLine("}");

						//	Save the model to disk
						if (!Directory.Exists(Path.GetDirectoryName(undefinedModel.Folder)))
							Directory.CreateDirectory(Path.GetDirectoryName(undefinedModel.Folder));

						File.WriteAllText(undefinedModel.Folder, result.ToString());

						//	Add the model to the project
						var parentProject = codeService.GetProjectFromFolder(undefinedModel.Folder);
						ProjectItem entityItem;

						if (parentProject.GetType() == typeof(Project))
							entityItem = ((Project)parentProject).ProjectItems.AddFromFile(undefinedModel.Folder);
						else
							entityItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(undefinedModel.Folder);

						codeService.AddEntity(entityItem);

						//	Register the composite model
						codeService.RegisterComposite(undefinedModel.ClassName,
													  undefinedModel.Namespace,
													  undefinedModel.ElementType,
													  undefinedModel.TableName);
					}
				}
			}
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
		/// <param name="moniker">The company monier used in various headers</param>
		/// <param name="controllerClassName">The class name for the controller</param>
		/// <param name="ValidatorInterface">The validiator interface used for validations</param>
		/// <param name="policy">The authentication policy used by the controller</param>
		/// <param name="ValidationNamespace">The validation namespace/param>
		/// <returns></returns>
		public string EmitController(ResourceClass resourceClass, string controllerClassName, string policy)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

			StringBuilder results = new StringBuilder();
			var nn = new NameNormalizer(resourceClass.ClassName);
			var pkcolumns = resourceClass.Entity.Columns.Where(c => c.IsPrimaryKey);

			BuildControllerInterface(resourceClass.ClassName, resourceClass.Namespace);
			BuildControllerOrchestration(resourceClass, resourceClass.Namespace);

			// --------------------------------------------------------------------------------
			//	Class
			// --------------------------------------------------------------------------------

			results.AppendLine("\t///\t<summary>");
			results.AppendLine($"\t///\t{resourceClass.ClassName} Controller");
			results.AppendLine("\t///\t</summary>");
			results.AppendLine("\t[ApiVersion(\"1.0\")]");
			results.AppendLine($"\tpublic class {controllerClassName} : ControllerBase");
			results.AppendLine("\t{");
			results.AppendLine("\t\t///\t<value>A generic interface for logging where the category name is derrived from");
			results.AppendLine($"\t\t///\tthe specified <see cref=\"{controllerClassName}\"/> type name.</value>");
			results.AppendLine($"\t\tprivate readonly ILogger<{controllerClassName}> _logger;");
			results.AppendLine();
			results.AppendLine("\t\t///\t<value>The interface to the orchestration layer.</value>");
			results.AppendLine($"\t\tprotected readonly IOrchestrator _orchestrator;");
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
			results.AppendLine("\t\t///\t<remarks>This call supports RQL. The call will only return up to a maximum of \"QueryLimit\" records, where the value of the query limit is predefined in the service and cannot be changed by the user.</remarks>");
			results.AppendLine($"\t\t///\t<response code=\"200\">Returns a collection of {nn.PluralForm}</response>");
			results.AppendLine("\t\t[HttpGet]");
			results.AppendLine("\t\t[MapToApiVersion(\"1.0\")]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedCollection<{resourceClass.ClassName}>))]");
			results.AppendLine($"\t\t[Produces(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");
			results.AppendLine($"\t\tpublic async Task<IActionResult> Get{nn.PluralForm}Async()");
			results.AppendLine("\t\t{");
			results.AppendLine("\t\t\tvar node = RqlNode.Parse(Request.QueryString.Value);");
			results.AppendLine();
			results.AppendLine("\t\t\tLogContext.PushProperty(\"RqlNode\", node);");
			results.AppendLine("\t\t\tLogContext.PushProperty(\"ClaimsPrincipal\", User.ListClaims());");
			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();

			results.AppendLine($"\t\t\tvar collection = await _orchestrator.Get{resourceClass.ClassName}CollectionAsync(Request, node, User);");
			results.AppendLine($"\t\t\treturn Ok(collection);");
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
				results.AppendLine("\t\t///\t<remarks>This call supports RQL. Use the RQL select clause to limit the members returned.</remarks>");
				results.AppendLine($"\t\t///\t<response code=\"200\">Returns the specified {nn.SingleForm}.</response>");
				results.AppendLine($"\t\t///\t<response code=\"404\">Not Found - returned when the speicifed {nn.SingleForm} does not exist in the datastore.</response>");
				results.AppendLine("\t\t[HttpGet]");
				results.AppendLine("\t\t[MapToApiVersion(\"1.0\")]");
				EmitRoute(results, nn.PluralCamelCase, pkcolumns);

				if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

				results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof({resourceClass.ClassName}))]");
				results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.NotFound)]");
				results.AppendLine($"\t\t[Produces(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");

				EmitEndpoint(resourceClass.ClassName, "Get", results, pkcolumns);

				results.AppendLine("\t\t{");
				results.AppendLine($"\t\t\tvar node = RqlNode.Parse($\"HRef=uri:\r\"/{BuildRoute(nn.PluralCamelCase, pkcolumns)}\\\"\")");
				results.AppendLine($"\t\t\t\t\t\t\t  .Merge(RqlNode.Parse(Request.QueryString.Value));");
				results.AppendLine();

				results.AppendLine("\t\t\tLogContext.PushProperty(\"RqlNode\", node);");
				results.AppendLine("\t\t\tLogContext.PushProperty(\"ClaimsPrincipal\", User.ListClaims());");
				results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
				results.AppendLine($"\t\t\tvar item = await _orchestrator.Get{resourceClass.ClassName}Async(Request, node, User);");
				results.AppendLine();
				results.AppendLine("\t\t\tif (item == null)");
				results.AppendLine("\t\t\t\treturn NotFound();");
				results.AppendLine();
				results.AppendLine("\t\t\treturn Ok(item);");

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
			results.AppendLine($"\t\t///\t<response code=\"201\">Created - returned when the new {nn.SingleForm} was successfully added to the datastore.</response>");
			results.AppendLine("\t\t[HttpPost]");
			results.AppendLine("\t\t[MapToApiVersion(\"1.0\")]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.Created, Type = typeof({resourceClass.ClassName}))]");
			results.AppendLine($"\t\t[Consumes(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");
			results.AppendLine($"\t\t[Produces(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");
			results.AppendLine($"\t\tpublic async Task<IActionResult> Add{resourceClass.ClassName}Async([FromBody] {resourceClass.ClassName} item)");
			results.AppendLine("\t\t{");

			results.AppendLine("\t\t\tLogContext.PushProperty(\"Item\", JsonSerializer.Serialize(item));");
			results.AppendLine("\t\t\tLogContext.PushProperty(\"ClaimsPrincipal\", User.ListClaims());");
			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();
			results.AppendLine($"\t\t\titem = await _orchestrator.Add{resourceClass.ClassName}Async(Request, item, User);");
			results.AppendLine($"\t\t\treturn Created(item.Href.AbsoluteUri, item);");

			results.AppendLine("\t\t}");
			results.AppendLine();

			// --------------------------------------------------------------------------------
			//	PUT Endpoint
			// --------------------------------------------------------------------------------
			results.AppendLine("\t\t///\t<summary>");
			results.AppendLine($"\t\t///\tUpdate a {nn.SingleForm}");
			results.AppendLine("\t\t///\t</summary>");
			results.AppendLine($"\t\t///\t<remarks>Update a {nn.SingleForm} in the datastore.</remarks>");
			results.AppendLine($"\t\t///\t<response code=\"200\">OK - returned when the {nn.SingleForm} was successfully updated in the datastore.</response>");
			results.AppendLine($"\t\t///\t<response code=\"404\">Not Found - returned when the speicifed {nn.SingleForm} does not exist in the datastore.</response>");
			results.AppendLine("\t\t[HttpPut]");
			results.AppendLine("\t\t[MapToApiVersion(\"1.0\")]");
			results.AppendLine($"\t\t[Route(\"{nn.PluralCamelCase}\")]");

			if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
				results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

			results.AppendLine($"\t\t[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof({resourceClass.ClassName}))]");
			results.AppendLine($"\t\t[Consumes(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");
			results.AppendLine($"\t\t[Produces(\"application/vnd.v1+json\", \"application/json\", \"text/json\")]");
			results.AppendLine($"\t\tpublic async Task<IActionResult> Update{resourceClass.ClassName}Async([FromBody] {resourceClass.ClassName} item)");
			results.AppendLine("\t\t{");

			results.AppendLine("\t\t\tLogContext.PushProperty(\"Item\", JsonSerializer.Serialize(item));");
			results.AppendLine("\t\t\tLogContext.PushProperty(\"ClaimsPrincipal\", User.ListClaims());");
			results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
			results.AppendLine();

			results.AppendLine($"\t\t\titem = await _orchestrator.Update{resourceClass.ClassName}Async(Request, item, User);");
			results.AppendLine($"\t\t\treturn Ok(item);");

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
				results.AppendLine($"\t\t///\t<response code=\"204\">No Content - returned when the {nn.SingleForm} was successfully deleted from the datastore.</response>");
				results.AppendLine($"\t\t///\t<response code=\"404\">Not Found - returned when the speicifed {nn.SingleForm} does not exist in the datastore.</response>");
				results.AppendLine("\t\t[HttpDelete]");
				results.AppendLine("\t\t[MapToApiVersion(\"1.0\")]");
				EmitRoute(results, nn.PluralCamelCase, pkcolumns);

				if (!string.IsNullOrWhiteSpace(policy) && !policy.Equals("Anonymous", StringComparison.OrdinalIgnoreCase))
					results.AppendLine($"\t\t[Authorize(\"{policy}\")]");

				EmitEndpoint(resourceClass.ClassName, "Delete", results, pkcolumns);

				results.AppendLine("\t\t{");
				results.AppendLine($"\t\t\tvar node = RqlNode.Parse($\"HRef=uri:\\\"/{BuildRoute(nn.PluralCamelCase, pkcolumns)}\\\"\");");
				results.AppendLine();
				results.AppendLine("\t\t\tLogContext.PushProperty(\"RqlNode\", node);");
				results.AppendLine("\t\t\tLogContext.PushProperty(\"ClaimsPrincipal\", User.ListClaims());");
				results.AppendLine("\t\t\t_logger.LogInformation(\"{s1} {s2}\", Request.Method, Request.Path);");
				results.AppendLine();

				results.AppendLine($"\t\t\tawait _orchestrator.Delete{resourceClass.ClassName}Async(Request, node, User);");
				results.AppendLine($"\t\t\treturn NoContent();");

				results.AppendLine("\t\t}");
				results.AppendLine("\t}");
			}

			return results.ToString();
		}

		private static void BuildControllerInterface(string resourceClassName, string resourceNamespace)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

			ProjectItem orchInterface = mDte.Solution.FindProjectItem("IOrchestrator.cs");
			orchInterface.Open(EnvDTE.Constants.vsViewKindCode);
			FileCodeModel2 fileCodeModel = (FileCodeModel2)orchInterface.FileCodeModel;

			//  Ensure that the interface contains all the required imports
			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Threading.Tasks")) == null)
				fileCodeModel.AddImport("System.Threading.Tasks");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Security.Claims")) == null)
				fileCodeModel.AddImport("System.Security.Claims");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Collections.Generic")) == null)
				fileCodeModel.AddImport("System.Collections.Generic");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql")) == null)
				fileCodeModel.AddImport("Rql");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql.Models")) == null)
				fileCodeModel.AddImport("Rql.Models");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql.Extensions")) == null)
				fileCodeModel.AddImport("Rql.Extensions");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals(resourceNamespace)) == null)
				fileCodeModel.AddImport(resourceNamespace);

			//  Ensure all the required functions are present
			foreach (CodeNamespace orchestrationNamespace in fileCodeModel.CodeElements.OfType<CodeNamespace>())
			{
				CodeInterface2 orchestrationInterface = orchestrationNamespace.Children
																			  .OfType<CodeInterface2>()
																			  .FirstOrDefault(c => c.Name.Equals("IOrchestrator"));

				if (orchestrationInterface != null)
				{
					if (orchestrationInterface.Name.Equals("IOrchestrator", StringComparison.OrdinalIgnoreCase))
					{
						// List<DBColumn> primaryKeyColumns = resourceModel.EntityModel.Columns.Where(c => c.IsPrimaryKey == true).ToList();

						string deleteFunctionName = $"Delete{resourceClassName}Async";
						string updateFunctionName = $"Update{resourceClassName}Async";
						string addFunctionName = $"Add{resourceClassName}Async";
						string getSingleFunctionName = $"Get{resourceClassName}Async";
						string collectionFunctionName = $"Get{resourceClassName}CollectionAsync";

						string article = resourceClassName.ToLower().StartsWith("a") ||
										 resourceClassName.ToLower().StartsWith("e") ||
										 resourceClassName.ToLower().StartsWith("i") ||
										 resourceClassName.ToLower().StartsWith("o") ||
										 resourceClassName.ToLower().StartsWith("u") ? "an" : "a";

						try
						{
							#region Get Single Function
							if (orchestrationInterface.Children.OfType<CodeFunction2>().FirstOrDefault(c => c.Name.Equals(getSingleFunctionName)) == null)
							{
								var theGetSingleFunction = (CodeFunction2)orchestrationInterface.AddFunction(getSingleFunctionName,
														  vsCMFunction.vsCMFunctionFunction,
														  $"Task<{resourceClassName}?>",
														  -1);

								theGetSingleFunction.AddParameter("request", "HttpRequest", -1);
								theGetSingleFunction.AddParameter("node", "RqlNode", -1);
								theGetSingleFunction.AddParameter("User", "ClaimsPrincipal", -1);

								StringBuilder doc = new StringBuilder();
								doc.AppendLine("<doc>");
								doc.AppendLine("<summary>");
								doc.AppendLine($"Asynchronously gets {article} {resourceClassName} resource specified by the <see cref=\"RqlNode\"/>.");
								doc.AppendLine("</summary>");
								doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
								doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection.</param>");
								doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
								doc.AppendLine($"<returns>The specified {resourceClassName} resource.</returns>");
								doc.AppendLine("</doc>");

								theGetSingleFunction.DocComment = doc.ToString();
							}
							#endregion

							#region Get Collection Function
							if (orchestrationInterface.Children.OfType<CodeFunction2>().FirstOrDefault(c => c.Name.Equals(collectionFunctionName)) == null)
							{
								var theCollectionFunction = (CodeFunction2)orchestrationInterface.AddFunction(collectionFunctionName,
														  vsCMFunction.vsCMFunctionFunction,
														  $"Task<PagedCollection<{resourceClassName}>>",
														  -1);

								theCollectionFunction.AddParameter("request", "HttpRequest", -1);
								theCollectionFunction.AddParameter("node", "RqlNode?", -1);
								theCollectionFunction.AddParameter("User", "ClaimsPrincipal", -1);

								StringBuilder doc = new StringBuilder();
								doc.AppendLine("<doc>");
								doc.AppendLine("<summary>");
								doc.AppendLine($"Asynchronously gets a collection of {resourceClassName} resources filtered by the <see cref=\"RqlNode\"/>.");
								doc.AppendLine("</summary>");
								doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
								doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection.</param>");
								doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
								doc.AppendLine($"<returns>The collection of {resourceClassName} resources filtered by the <see cref=\"RqlNode\"/>.</returns>");
								doc.AppendLine("</doc>");

								theCollectionFunction.DocComment = doc.ToString();
							}
							#endregion

							#region Add Function
							if (orchestrationInterface.Children.OfType<CodeFunction2>().FirstOrDefault(c => c.Name.Equals(addFunctionName)) == null)
							{
								var theAddFunction = (CodeFunction2)orchestrationInterface.AddFunction(addFunctionName,
														  vsCMFunction.vsCMFunctionFunction,
														  $"Task<{resourceClassName}>",
														  -1);

								theAddFunction.AddParameter("request", "HttpRequest", -1);
								theAddFunction.AddParameter("item", resourceClassName, -1);
								theAddFunction.AddParameter("User", "ClaimsPrincipal", -1);

								StringBuilder doc = new StringBuilder();
								doc.AppendLine("<doc>");
								doc.AppendLine("<summary>");
								doc.AppendLine($"Asynchronously adds {article} {resourceClassName} resource.");
								doc.AppendLine("</summary>");
								doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
								doc.AppendLine($"<param name=\"item\">The {resourceClassName} resource to add.</param>");
								doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
								doc.AppendLine("<returns>The newly added resource.</returns>");
								doc.AppendLine("</doc>");

								theAddFunction.DocComment = doc.ToString();
							}
							#endregion

							#region Update Function
							if (orchestrationInterface.Children.OfType<CodeFunction2>().FirstOrDefault(c => c.Name.Equals(updateFunctionName)) == null)
							{
								var theUpdateFunction = (CodeFunction2)orchestrationInterface.AddFunction(updateFunctionName,
														  vsCMFunction.vsCMFunctionFunction,
														  $"Task<{resourceClassName}>",
														  -1);

								theUpdateFunction.AddParameter("request", "HttpRequest", -1);
								theUpdateFunction.AddParameter("item", resourceClassName, -1);
								theUpdateFunction.AddParameter("User", "ClaimsPrincipal", -1);

								StringBuilder doc = new StringBuilder();
								doc.AppendLine("<doc>");
								doc.AppendLine("<summary>");
								doc.AppendLine($"Asynchronously updates {article} {resourceClassName} resource.");
								doc.AppendLine("</summary>");
								doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
								doc.AppendLine($"<param name=\"item\">The {resourceClassName} resource to update.</param>");
								doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
								doc.AppendLine("<returns>The updated item.</returns>");
								doc.AppendLine("</doc>");

								theUpdateFunction.DocComment = doc.ToString();
							}
							#endregion

							#region Delete Function
							if (orchestrationInterface.Children.OfType<CodeFunction2>().FirstOrDefault(c => c.Name.Equals(deleteFunctionName)) == null)
							{
								var theDeleteFunction = (CodeFunction2)orchestrationInterface.AddFunction(deleteFunctionName,
														  vsCMFunction.vsCMFunctionFunction,
														  $"Task", -1);

								theDeleteFunction.AddParameter("request", "HttpRequest", -1);
								theDeleteFunction.AddParameter("node", "RqlNode?", -1);
								theDeleteFunction.AddParameter("User", "ClaimsPrincipal", -1);

								StringBuilder doc = new StringBuilder();
								doc.AppendLine("<doc>");
								doc.AppendLine("<summary>");
								doc.AppendLine($"Asynchronously deletes {article} {resourceClassName} resource specified by the <see cref=\"RqlNode\"/>.");
								doc.AppendLine("</summary>");
								doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
								doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection of resources to delete.</param>");
								doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
								doc.AppendLine("</doc>");

								theDeleteFunction.DocComment = doc.ToString();
							}
							#endregion
						}
						catch (Exception)
						{
						}
					}
				}
			}
		}

		private static void BuildControllerOrchestration(ResourceClass resourceClass, string resourceNamespace)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

			ProjectItem orchCode = mDte.Solution.FindProjectItem("Orchestrator.cs");
			orchCode.Open(EnvDTE.Constants.vsViewKindCode);
			var fileCodeModel = (FileCodeModel2)orchCode.FileCodeModel;

			//  Ensure that the interface contains all the required imports
			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Threading.Tasks")) == null)
				fileCodeModel.AddImport("System.Threading.Tasks");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Security.Claims")) == null)
				fileCodeModel.AddImport("System.Security.Claims");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Collections.Generic")) == null)
				fileCodeModel.AddImport("System.Collections.Generic");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("System.Text.Json")) == null)
				fileCodeModel.AddImport("System.Text.Json");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Microsoft.Extensions.DependencyInjection")) == null)
				fileCodeModel.AddImport("Microsoft.Extensions.DependencyInjection");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Microsoft.Extensions.Logging")) == null)
				fileCodeModel.AddImport("Microsoft.Extensions.Logging");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql")) == null)
				fileCodeModel.AddImport("Rql");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql.Models")) == null)
				fileCodeModel.AddImport("Rql.Models");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Rql.Extensions")) == null)
				fileCodeModel.AddImport("Rql.Extensions");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals("Serilog.Context")) == null)
				fileCodeModel.AddImport("Serilog.Context");

			if (fileCodeModel.CodeElements.OfType<CodeImport>().FirstOrDefault(c => c.Namespace.Equals(resourceNamespace)) == null)
				fileCodeModel.AddImport(resourceNamespace);

			//  Add all the functions
			foreach (CodeNamespace orchestrattorNamespace in fileCodeModel.CodeElements.OfType<CodeNamespace>())
			{
				CodeClass2 orchestratorClass = orchestrattorNamespace.Children
																	 .OfType<CodeClass2>()
																	 .FirstOrDefault(c => c.Name.Equals("Orchestrator"));

				if (orchestratorClass != null)
				{
					//var primaryKeyColumns = resourceModel.EntityModel.Columns.Where(c => c.IsPrimaryKey == true).ToList();

					var deleteFunctionName = $"Delete{resourceClass.ClassName}Async";
					var updateFunctionName = $"Update{resourceClass.ClassName}Async";
					var addFunctionName = $"Add{resourceClass.ClassName}Async";
					var getSingleFunctionName = $"Get{resourceClass.ClassName}Async";
					var collectionFunctionName = $"Get{resourceClass.ClassName}CollectionAsync";

					var article = resourceClass.ClassName.ToLower().StartsWith("a") ||
								  resourceClass.ClassName.ToLower().StartsWith("e") ||
								  resourceClass.ClassName.ToLower().StartsWith("i") ||
								  resourceClass.ClassName.ToLower().StartsWith("o") ||
								  resourceClass.ClassName.ToLower().StartsWith("u") ? "an" : "a";

					EditPoint2 editPoint;

					try
					{
						#region Get Single Function
						if (orchestratorClass.Children
											 .OfType<CodeFunction2>()
											 .FirstOrDefault(c => c.Name.Equals(getSingleFunctionName, StringComparison.OrdinalIgnoreCase)) == null)
						{
							var theGetSingleFunction = (CodeFunction2)orchestratorClass.AddFunction(getSingleFunctionName,
													  vsCMFunction.vsCMFunctionFunction,
													  $"Task<{resourceClass.ClassName}?>",
													  -1,
													  vsCMAccess.vsCMAccessPublic);

							theGetSingleFunction.AddParameter("request", "HttpRequest", -1);
							theGetSingleFunction.AddParameter("node", "RqlNode?", -1);
							theGetSingleFunction.AddParameter("User", "ClaimsPrincipal", -1);

							StringBuilder doc = new StringBuilder();
							doc.AppendLine("<doc>");
							doc.AppendLine("<summary>");
							doc.AppendLine($"Asynchronously gets {article} {resourceClass.ClassName} resource specified by the <see cref=\"RqlNode\"/>.");
							doc.AppendLine("</summary>");
							doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
							doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection</param>");
							doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function</param>");
							doc.AppendLine($"<returns>The specified {resourceClass.ClassName} resource.</returns>");
							doc.AppendLine("</doc>");

							theGetSingleFunction.DocComment = doc.ToString();

							editPoint = (EditPoint2)theGetSingleFunction.StartPoint.CreateEditPoint();
							editPoint.ReplaceText(6, "public async", 0);

							editPoint = (EditPoint2)theGetSingleFunction.EndPoint.CreateEditPoint();
							editPoint.LineUp();
							editPoint.StartOfLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"_logger.LogDebug(\"{getSingleFunctionName}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"var resource = await GetSingleAsync<{resourceClass.ClassName}>(node);");
							editPoint.InsertNewLine(2);
							editPoint.Indent(null, 3);
							editPoint.Insert("if (resource != null)");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("{");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 4);
							editPoint.Insert("var rootUrl = new Uri($\"{request.Scheme}://{request.Host}\");");
							editPoint.InsertNewLine();

							foreach (var col in resourceClass.Columns)
							{
								if (col.IsPrimaryKey || col.IsForeignKey)
								{
									editPoint.Indent(null, 4);
									editPoint.Insert($"resource.{col.ColumnName} = new Uri(rootUrl, resource.{col.ColumnName}.AbsolutePath);");
									editPoint.InsertNewLine();
								}
							}

							editPoint.Indent(null, 3);
							editPoint.Insert("}");
							editPoint.InsertNewLine(2);
							editPoint.Indent(null, 3);
							editPoint.Insert("return resource;");
						}
						#endregion

						#region Get Collection Function
						if (orchestratorClass.Children
											 .OfType<CodeFunction2>()
											 .FirstOrDefault(c => c.Name.Equals(collectionFunctionName, StringComparison.OrdinalIgnoreCase)) == null)
						{
							var theCollectionFunction = (CodeFunction2)orchestratorClass.AddFunction(collectionFunctionName,
													  vsCMFunction.vsCMFunctionFunction,
													  $"Task<PagedCollection<{resourceClass.ClassName}>>",
													  -1,
													  vsCMAccess.vsCMAccessPublic);

							theCollectionFunction.AddParameter("request", "HttpRequest", -1);
							theCollectionFunction.AddParameter("node", "RqlNode?", -1);
							theCollectionFunction.AddParameter("User", "ClaimsPrincipal", -1);

							StringBuilder doc = new StringBuilder();
							doc.AppendLine("<doc>");
							doc.AppendLine("<summary>");
							doc.AppendLine($"Asynchronously gets a collection of {resourceClass.ClassName} resources filtered by the <see cref=\"RqlNode\"/>.");
							doc.AppendLine("</summary>");
							doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
							doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection.</param>");
							doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
							doc.AppendLine($"<returns>The collection of {resourceClass.ClassName} resources filtered by the <see cref=\"RqlNode\"/>.</returns>");
							doc.AppendLine("</doc>");

							theCollectionFunction.DocComment = doc.ToString();

							editPoint = (EditPoint2)theCollectionFunction.StartPoint.CreateEditPoint();
							editPoint.ReplaceText(6, "public async", 0);

							editPoint = (EditPoint2)theCollectionFunction.EndPoint.CreateEditPoint();
							editPoint.LineUp();
							editPoint.StartOfLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"_logger.LogDebug(\"{collectionFunctionName}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"var collection = await GetCollectionAsync<{resourceClass.ClassName}>(request, node);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("var rootUrl = new Uri($\"{request.Scheme}://{request.Host}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("collection.Href = new Uri(rootUrl, (string?) collection.Href?.AbsolutePath);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("collection.Next = collection.Next == null ? null : new Uri(rootUrl, (string?) collection.Next?.AbsolutePath);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("collection.Previous = collection.Previous == null ? null : new Uri(rootUrl, (string?) collection.Previous?.AbsolutePath);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("collection.First = collection.First == null ? null : new Uri(rootUrl, (string?) collection.First?.AbsolutePath);");
							editPoint.InsertNewLine(2);
							editPoint.Indent(null, 3);
							editPoint.Insert($"foreach ({resourceClass.ClassName} resource in collection.Items)");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("{");
							editPoint.InsertNewLine();

							foreach (var col in resourceClass.Columns)
							{
								if (col.IsPrimaryKey || col.IsForeignKey)
								{
									editPoint.Indent(null, 4);
									editPoint.Insert($"resource.{col.ColumnName} = new Uri(rootUrl, (string) resource.{col.ColumnName}.AbsolutePath);");
									editPoint.InsertNewLine();
								}
							}
							editPoint.Indent(null, 3);
							editPoint.Insert("}");
							editPoint.InsertNewLine(2);
							editPoint.Indent(null, 3);
							editPoint.Insert("return collection;");
						}
						#endregion

						#region Add Function
						if (orchestratorClass.Children
											 .OfType<CodeFunction2>()
											 .FirstOrDefault(c => c.Name.Equals(addFunctionName, StringComparison.OrdinalIgnoreCase)) == null)
						{
							var theAddFunction = (CodeFunction2)orchestratorClass.AddFunction(addFunctionName,
													  vsCMFunction.vsCMFunctionFunction,
													  $"Task<{resourceClass.ClassName}>",
													  -1,
													  vsCMAccess.vsCMAccessPublic);

							theAddFunction.AddParameter("request", "HttpRequest", -1);
							theAddFunction.AddParameter("item", resourceClass.ClassName, -1);
							theAddFunction.AddParameter("User", "ClaimsPrincipal", -1);

							StringBuilder doc = new StringBuilder();
							doc.AppendLine("<doc>");
							doc.AppendLine("<summary>");
							doc.AppendLine($"Asynchronously adds {article} {resourceClass.ClassName} resource.");
							doc.AppendLine("</summary>");
							doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
							doc.AppendLine($"<param name=\"item\">The {resourceClass.ClassName} resource to add.</param>");
							doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
							doc.AppendLine("<returns>The newly added resource.</returns>");
							doc.AppendLine("</doc>");

							theAddFunction.DocComment = doc.ToString();

							editPoint = (EditPoint2)theAddFunction.StartPoint.CreateEditPoint();
							editPoint.ReplaceText(6, "public async", 0);

							editPoint = (EditPoint2)theAddFunction.EndPoint.CreateEditPoint();
							editPoint.LineUp();
							editPoint.StartOfLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"_logger.LogDebug(\"{addFunctionName}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"var resource = await AddAsync<{resourceClass.ClassName}>(item);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("var rootUrl = new Uri($\"{request.Scheme}://{request.Host}\");");
							editPoint.InsertNewLine();

							foreach (var col in resourceClass.Columns)
							{
								if (col.IsPrimaryKey || col.IsForeignKey)
								{
									editPoint.Indent(null, 3);
									editPoint.Insert($"resource.{col.ColumnName} = new Uri(rootUrl, resource.{col.ColumnName}.AbsolutePath);");
									editPoint.InsertNewLine();
								}
							}

							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("return resource;");
						}
						#endregion

						#region Update Function
						if (orchestratorClass.Children
											 .OfType<CodeFunction2>()
											 .FirstOrDefault(c => c.Name.Equals(updateFunctionName, StringComparison.OrdinalIgnoreCase)) == null)
						{
							var theUpdateFunction = (CodeFunction2)orchestratorClass.AddFunction(updateFunctionName,
													  vsCMFunction.vsCMFunctionFunction,
													  $"Task<{resourceClass.ClassName}>",
													  -1,
													  vsCMAccess.vsCMAccessPublic);

							theUpdateFunction.AddParameter("request", "HttpRequest", -1);
							theUpdateFunction.AddParameter("item", resourceClass.ClassName, -1);
							theUpdateFunction.AddParameter("User", "ClaimsPrincipal", -1);

							StringBuilder doc = new StringBuilder();
							doc.AppendLine("<doc>");
							doc.AppendLine("<summary>");
							doc.AppendLine($"Asynchronously updates {article} {resourceClass.ClassName} resource.");
							doc.AppendLine("</summary>");
							doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
							doc.AppendLine($"<param name=\"item\">The {resourceClass.ClassName} resource to update.</param>");
							doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
							doc.AppendLine("<returns>The updated item.</returns>");
							doc.AppendLine("</doc>");

							theUpdateFunction.DocComment = doc.ToString();

							editPoint = (EditPoint2)theUpdateFunction.StartPoint.CreateEditPoint();
							editPoint.ReplaceText(6, "public async", 0);

							editPoint = (EditPoint2)theUpdateFunction.EndPoint.CreateEditPoint();
							editPoint.LineUp();
							editPoint.StartOfLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"_logger.LogDebug(\"{updateFunctionName}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"var resource = await UpdateAsync<{resourceClass.ClassName}>(item);");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("var rootUrl = new Uri($\"{request.Scheme}://{request.Host}\");");
							editPoint.InsertNewLine();

							foreach (var col in resourceClass.Columns)
							{
								if (col.IsPrimaryKey || col.IsForeignKey)
								{
									editPoint.Indent(null, 3);
									editPoint.Insert($"resource.{col.ColumnName} = new Uri(rootUrl, resource.{col.ColumnName}.AbsolutePath);");
									editPoint.InsertNewLine();
								}
							}

							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert("return resource;");
						}
						#endregion

						#region Delete Function
						if (orchestratorClass.Children
											 .OfType<CodeFunction2>()
											 .FirstOrDefault(c => c.Name.Equals(deleteFunctionName, StringComparison.OrdinalIgnoreCase)) == null)
						{
							var theDeleteFunction = (CodeFunction2)orchestratorClass.AddFunction(deleteFunctionName,
													  vsCMFunction.vsCMFunctionFunction,
													  $"Task",
													  -1,
													  vsCMAccess.vsCMAccessPublic);

							theDeleteFunction.AddParameter("request", "HttpRequest", -1);
							theDeleteFunction.AddParameter("node", "RqlNode?", -1);
							theDeleteFunction.AddParameter("User", "ClaimsPrincipal", -1);

							StringBuilder doc = new StringBuilder();
							doc.AppendLine("<doc>");
							doc.AppendLine("<summary>");
							doc.AppendLine($"Asynchronously deletes {article} {resourceClass.ClassName} resource specified by the <see cref=\"RqlNode\"/>.");
							doc.AppendLine("</summary>");
							doc.AppendLine("<param name=\"request\">The <see cref=\"HttpRequest\"/> associated with this request.</param>");
							doc.AppendLine("<param name=\"node\">The <see cref=\"RqlNode\"/> that further restricts the selection of resources to delete.</param>");
							doc.AppendLine("<param name=\"User\">The <see cref=\"ClaimsPrincipal\"/> of the actor calling the function.</param>");
							doc.AppendLine("</doc>");

							theDeleteFunction.DocComment = doc.ToString();

							editPoint = (EditPoint2)theDeleteFunction.StartPoint.CreateEditPoint();
							editPoint.ReplaceText(6, "public async", 0);

							editPoint = (EditPoint2)theDeleteFunction.EndPoint.CreateEditPoint();
							editPoint.LineUp();
							editPoint.StartOfLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"_logger.LogDebug(\"{deleteFunctionName}\");");
							editPoint.InsertNewLine();
							editPoint.Indent(null, 3);
							editPoint.Insert($"await DeleteAsync<{resourceClass.ClassName}>(node);");
						}
						#endregion
					}
					catch (Exception)
					{
					}
				}
			}
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

				results.Append($"{dataType} {entityColumn.EntityName}");
			}

			if (string.Equals(action, "patch", StringComparison.OrdinalIgnoreCase))
				results.AppendLine(", [FromBody] IEnumerable<PatchCommand> commands)");
			else
				results.AppendLine(")");
		}

		private static void EmitRoute(StringBuilder results, string routeName, IEnumerable<DBColumn> pkcolumns)
		{
			results.Append($"\t\t[Route(\"{routeName}/id");

			foreach (var entityColumn in pkcolumns)
			{
				results.Append($"/{{{entityColumn.EntityName.ToLower()}}}");
			}

			results.AppendLine("\")]");
		}

		private static string BuildRoute(string routeName, IEnumerable<DBColumn> pkcolumns)
		{
			var route = new StringBuilder();

			route.Append(routeName);
			route.Append("/id");

			foreach (var entityColumn in pkcolumns)
			{
				route.Append($"/{{{entityColumn.ColumnName.ToLower()}}}");
			}

			return route.ToString();
		}
	}
}
