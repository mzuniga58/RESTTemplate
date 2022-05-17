﻿using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RESTInstaller.Models;
using RESTInstaller.Services;
using Path = System.IO.Path;


namespace RESTInstaller.Dialogs
{
	/// <summary>
	/// Interaction logic for NewEntityDialog.xaml
	/// </summary>
	public partial class NewEntityDialog : DialogWindow
	{
		#region Variables
		private ServerConfig _serverConfig;
		private bool Populating = true;
		public DBTable DatabaseTable { get; set; }
		public List<DBColumn> DatabaseColumns { get; set; }
		public string ConnectionString { get; set; }
		public ProjectFolder EntityModelsFolder { get; set; }
		public string DefaultConnectionString { get; set; }
		public Dictionary<string, string> ReplacementsDictionary { get; set; }
		public List<EntityModel> UndefinedEntityModels { get; set; }
		public DBServerType ServerType { get; set; }
		public ElementType EType { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
		#endregion

		public NewEntityDialog()
		{
			InitializeComponent();
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			Populating = true;
			Combobox_Server.Items.Clear();

			Label_ServerType.Visibility = Visibility.Hidden;
			Label_ServerType_Content.Visibility = Visibility.Hidden;
			Label_ServerType_Content.Content = string.Empty;

			Label_Authentication.Visibility = Visibility.Hidden;
			Label_Authentication_Content.Visibility = Visibility.Hidden;
			Label_Authentication_Content.Content = string.Empty;

			Label_UserName.Visibility = Visibility.Hidden;
			Label_UserName_Content.Visibility = Visibility.Hidden;
			Label_UserName_Content.Content = string.Empty;

			DatabaseColumns = new List<DBColumn>();
			UndefinedEntityModels = new List<EntityModel>();
			ReadServerList();

			Button_Cancel.IsDefault = true;
			Button_OK.IsEnabled = false;
			Populating = false;
		}

		private void Databases_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				Listbox_Tables.SelectedIndex = -1;

				var server = (DBServer)Combobox_Server.SelectedItem;

				if (server == null)
				{
					Listbox_Tables.Items.Clear();
					Listbox_Databases.Items.Clear();
					return;
				}

				var db = (string)Listbox_Databases.SelectedItem;

				if (server.DBType == DBServerType.POSTGRESQL)
				{
					string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database={db};User ID={server.Username};Password={server.Password};";
					Listbox_Tables.Items.Clear();

					ConnectionString = connectionString;
					using (var connection = new NpgsqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
select schemaname, elementname
  from (
SELECT schemaname as schemaName, 
       tablename as elementName
  FROM pg_catalog.pg_tables
 WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema'

union all

select n.nspname as schemaName, 
       t.typname as elementName
  from pg_type as t 
 inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
 WHERE ( t.typrelid = 0
                OR ( SELECT c.relkind = 'c'
                        FROM pg_catalog.pg_class c
                        WHERE c.oid = t.typrelid ) )
            AND NOT EXISTS (
                    SELECT 1
                        FROM pg_catalog.pg_type el
                        WHERE el.oid = t.typelem
                        AND el.typarray = t.oid )
            AND n.nspname <> 'pg_catalog'
            AND n.nspname <> 'information_schema'
            AND n.nspname !~ '^pg_toast'
			and ( t.typcategory = 'C' or t.typcategory = 'E' ) ) as X
order by schemaname, elementname";

						using (var command = new NpgsqlCommand(query, connection))
						{
							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									var dbTable = new DBTable
									{
										Schema = reader.GetString(0),
										Table = reader.GetString(1)
									};

									Listbox_Tables.Items.Add(dbTable);
								}
							}
						}
					}
				}
				else if (server.DBType == DBServerType.MYSQL)
				{
					string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database={db};UID={server.Username};PWD={server.Password};";
					Listbox_Tables.Items.Clear();

					ConnectionString = connectionString;
					using (var connection = new MySqlConnection(connectionString))
					{
						connection.Open();

						var query = @"

SELECT TABLE_SCHEMA, TABLE_NAME FROM information_schema.tables 
 where table_type = 'BASE TABLE'
   and TABLE_SCHEMA = @databaseName;
";

						using (var command = new MySqlCommand(query, connection))
						{
							command.Parameters.AddWithValue("@databaseName", db);

							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									var dbTable = new DBTable
									{
										Schema = reader.GetString(0),
										Table = reader.GetString(1)
									};

									Listbox_Tables.Items.Add(dbTable);
								}
							}
						}
					}
				}
				else
				{
					string connectionString;

					if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
						connectionString = $"Server={server.ServerName};Database={db};Trusted_Connection=True;";
					else
						connectionString = $"Server={server.ServerName};Database={db};uid={server.Username};pwd={server.Password};";

					ConnectionString = connectionString;

					Listbox_Tables.Items.Clear();

					using (var connection = new SqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
select s.name, t.name
  from sys.tables as t with(nolock)
 inner join sys.schemas as s with(nolock) on s.schema_id = t.schema_id
  order by s.name, t.name";

						using (var command = new SqlCommand(query, connection))
						{
							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									var dbTable = new DBTable
									{
										Schema = reader.GetString(0),
										Table = reader.GetString(1)
									};
									Listbox_Tables.Items.Add(dbTable);
								}
							}
						}
					}
				}

				Listbox_Tables.SelectedIndex = -1;
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}
		}

		private void Tables_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var codeService = ServiceFactory.GetService<ICodeService>();

			Button_OK.IsEnabled = false;
			Button_OK.IsDefault = false;
			Button_Cancel.IsDefault = true;

			try
			{
				var server = (DBServer)Combobox_Server.SelectedItem;
				ServerType = server.DBType;
				var db = (string)Listbox_Databases.SelectedItem;
				var table = (DBTable)Listbox_Tables.SelectedItem;
				DatabaseColumns.Clear();

				if (server == null)
					return;

				if (string.IsNullOrWhiteSpace(db))
					return;

				if (table == null)
					return;

				Button_OK.IsEnabled = true;
				Button_OK.IsDefault = true;
				Button_Cancel.IsDefault = false;

				if (server.DBType == DBServerType.POSTGRESQL)
				{
					string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database={db};User ID={server.Username};Password={server.Password};";

					UndefinedEntityModels.Clear();
					EType = DBHelper.GetElementType(mDte, table.Schema, table.Table, connectionString);

					switch (EType)
					{
						case ElementType.Enum:
							break;

						case ElementType.Composite:
							{
								using (var connection = new NpgsqlConnection(connectionString))
								{
									connection.Open();

									var query = @"
select a.attname as columnname,
	   t.typname as datatype,
	   case when t.typname = 'varchar' then a.atttypmod-4
	        when t.typname = 'bpchar' then a.atttypmod-4
			when t.typname = '_varchar' then a.atttypmod-4
			when t.typname = '_bpchar' then a.atttypmod-4
	        when a.atttypmod > -1 then a.atttypmod
	        else a.attlen end as max_len,
	   case atttypid
            when 21 /*int2*/ then 16
            when 23 /*int4*/ then 32
            when 20 /*int8*/ then 64
         	when 1700 /*numeric*/ then
              	case when atttypmod = -1
                     then 0
                     else ((atttypmod - 4) >> 16) & 65535     -- calculate the precision
                     end
         	when 700 /*float4*/ then 24 /*FLT_MANT_DIG*/
         	when 701 /*float8*/ then 53 /*DBL_MANT_DIG*/
         	else 0
  			end as numeric_precision,
  		case when atttypid in (21, 23, 20) then 0
    		 when atttypid in (1700) then            
        		  case when atttypmod = -1 then 0       
            		   else (atttypmod - 4) & 65535            -- calculate the scale  
        			   end
       		else 0
  			end as numeric_scale,		
	   not a.attnotnull as is_nullable,
	   case when ( a.attgenerated = 'a' ) or  ( pg_get_expr(ad.adbin, ad.adrelid) = 'nextval('''
                 || (pg_get_serial_sequence (a.attrelid::regclass::text, a.attname))::regclass
                 || '''::regclass)')
	        then true else false end as is_computed,

	   case when ( a.attidentity = 'a' ) or  ( pg_get_expr(ad.adbin, ad.adrelid) = 'nextval('''
                 || (pg_get_serial_sequence (a.attrelid::regclass::text, a.attname))::regclass
                 || '''::regclass)')
	        then true else false end as is_identity,

	   case when (select indrelid from pg_index as px where px.indisprimary = true and px.indrelid = c.oid and a.attnum = ANY(px.indkey)) = c.oid then true else false end as is_primary,
	   case when (select indrelid from pg_index as ix where ix.indrelid = c.oid and a.attnum = ANY(ix.indkey)) = c.oid then true else false end as is_indexed,
	   case when (select conrelid from pg_constraint as cx where cx.conrelid = c.oid and cx.contype = 'f' and a.attnum = ANY(cx.conkey)) = c.oid then true else false end as is_foreignkey,
       (  select cc.relname from pg_constraint as cx inner join pg_class as cc on cc.oid = cx.confrelid where cx.conrelid = c.oid and cx.contype = 'f' and a.attnum = ANY(cx.conkey)) as foeigntablename
   from pg_class as c
  inner join pg_namespace as ns on ns.oid = c.relnamespace
  inner join pg_attribute as a on a.attrelid = c.oid and not a.attisdropped and attnum > 0
  inner join pg_type as t on t.oid = a.atttypid
  left outer join pg_attrdef as ad on ad.adrelid = a.attrelid and ad.adnum = a.attnum 
  where ns.nspname = @schema
    and c.relname = @tablename
 order by a.attnum
";

									using (var command = new NpgsqlCommand(query, connection))
									{
										command.Parameters.AddWithValue("@schema", table.Schema);
										command.Parameters.AddWithValue("@tablename", table.Table);

										using (var reader = command.ExecuteReader())
										{
											while (reader.Read())
											{
												ConstructPostgresqlColumn(table, reader);
											}
										}
									}
								}

								if (UndefinedEntityModels.Count > 0)
								{
									WarnUndefinedContent(table, connectionString);
								}
							}
							break;

						case ElementType.Table:
							{
								using (var connection = new NpgsqlConnection(connectionString))
								{
									connection.Open();

									var query = @"
select a.attname as columnname,
	   t.typname as datatype,
	   case when t.typname = 'varchar' then a.atttypmod-4
	        when t.typname = 'bpchar' then a.atttypmod-4
			when t.typname = '_varchar' then a.atttypmod-4
			when t.typname = '_bpchar' then a.atttypmod-4
	        when a.atttypmod > -1 then a.atttypmod
	        else a.attlen end as max_len,
	   case atttypid
            when 21 /*int2*/ then 16
            when 23 /*int4*/ then 32
            when 20 /*int8*/ then 64
         	when 1700 /*numeric*/ then
              	case when atttypmod = -1
                     then 0
                     else ((atttypmod - 4) >> 16) & 65535     -- calculate the precision
                     end
         	when 700 /*float4*/ then 24 /*FLT_MANT_DIG*/
         	when 701 /*float8*/ then 53 /*DBL_MANT_DIG*/
         	else 0
  			end as numeric_precision,
  		case when atttypid in (21, 23, 20) then 0
    		 when atttypid in (1700) then            
        		  case when atttypmod = -1 then 0       
            		   else (atttypmod - 4) & 65535            -- calculate the scale  
        			   end
       		else 0
  			end as numeric_scale,		
	   not a.attnotnull as is_nullable,
	   case when ( a.attgenerated = 'a' ) or  ( pg_get_expr(ad.adbin, ad.adrelid) = 'nextval('''
                 || (pg_get_serial_sequence (a.attrelid::regclass::text, a.attname))::regclass
                 || '''::regclass)')
	        then true else false end as is_computed,

	   case when ( a.attidentity = 'a' ) or  ( pg_get_expr(ad.adbin, ad.adrelid) = 'nextval('''
                 || (pg_get_serial_sequence (a.attrelid::regclass::text, a.attname))::regclass
                 || '''::regclass)')
	        then true else false end as is_identity,

	   case when (select indrelid from pg_index as px where px.indisprimary = true and px.indrelid = c.oid and a.attnum = ANY(px.indkey)) = c.oid then true else false end as is_primary,
	   case when (select indrelid from pg_index as ix where ix.indrelid = c.oid and a.attnum = ANY(ix.indkey)) = c.oid then true else false end as is_indexed,
	   case when (select conrelid from pg_constraint as cx where cx.conrelid = c.oid and cx.contype = 'f' and a.attnum = ANY(cx.conkey)) = c.oid then true else false end as is_foreignkey,
       (  select cc.relname from pg_constraint as cx inner join pg_class as cc on cc.oid = cx.confrelid where cx.conrelid = c.oid and cx.contype = 'f' and a.attnum = ANY(cx.conkey)) as foeigntablename
   from pg_class as c
  inner join pg_namespace as ns on ns.oid = c.relnamespace
  inner join pg_attribute as a on a.attrelid = c.oid and not a.attisdropped and attnum > 0
  inner join pg_type as t on t.oid = a.atttypid
  left outer join pg_attrdef as ad on ad.adrelid = a.attrelid and ad.adnum = a.attnum 
  where ns.nspname = @schema
    and c.relname = @tablename
 order by a.attnum
";

									using (var command = new NpgsqlCommand(query, connection))
									{
										command.Parameters.AddWithValue("@schema", table.Schema);
										command.Parameters.AddWithValue("@tablename", table.Table);

										using (var reader = command.ExecuteReader())
										{
											while (reader.Read())
											{
												ConstructPostgresqlColumn(table, reader);
											}
										}
									}

									if (UndefinedEntityModels.Count > 0)
									{
										WarnUndefinedContent(table, connectionString);
									}
								}
							}
							break;
					}
				}
				else if (server.DBType == DBServerType.MYSQL)
				{
					string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database={db};UID={server.Username};PWD={server.Password};";

					using (var connection = new MySqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
SELECT c.COLUMN_NAME as 'columnName',
       c.COLUMN_TYPE as 'datatype',
       case when c.CHARACTER_MAXIMUM_LENGTH is null then -1 else c.CHARACTER_MAXIMUM_LENGTH end as 'max_len',
       case when c.NUMERIC_PRECISION is null then 0 else c.NUMERIC_PRECISION end as 'precision',
       case when c.NUMERIC_SCALE is null then 0 else c.NUMERIC_SCALE end as 'scale',       
	   case when c.GENERATION_EXPRESSION != '' then 1 else 0 end as 'is_computed',
       case when c.EXTRA = 'auto_increment' then 1 else 0 end as 'is_identity',
       case when c.COLUMN_KEY = 'PRI' then 1 else 0 end as 'is_primary',
       case when c.COLUMN_KEY != '' then 1 else 0 end as 'is_indexed',
       case when c.IS_NULLABLE = 'no' then 0 else 1 end as 'is_nullable',
       case when cu.REFERENCED_TABLE_NAME is not null then 1 else 0 end as 'is_foreignkey',
       cu.REFERENCED_TABLE_NAME as 'foreigntablename'
  FROM `INFORMATION_SCHEMA`.`COLUMNS` as c
left outer join information_schema.KEY_COLUMN_USAGE as cu on cu.CONSTRAINT_SCHEMA = c.TABLE_SCHEMA
                                                         and cu.TABLE_NAME = c.TABLE_NAME
														 and cu.COLUMN_NAME = c.COLUMN_NAME
                                                         and cu.REFERENCED_TABLE_NAME is not null
 WHERE c.TABLE_SCHEMA=@schema
  AND c.TABLE_NAME=@tablename
ORDER BY c.ORDINAL_POSITION;
";

						using (var command = new MySqlCommand(query, connection))
						{
							command.Parameters.AddWithValue("@schema", db);
							command.Parameters.AddWithValue("@tablename", table.Table);
							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									var x = reader.GetValue(8);

									var dbColumn = new DBColumn
									{
										ColumnName = codeService.CorrectForReservedNames(codeService.NormalizeClassName(reader.GetString(0))),
										EntityName = reader.GetString(0),
										DBDataType = reader.GetString(1),
										Length = Convert.ToInt64(reader.GetValue(2)),
										NumericPrecision = Convert.ToInt32(reader.GetValue(3)),
										NumericScale = Convert.ToInt32(reader.GetValue(4)),
										IsComputed = Convert.ToBoolean(reader.GetValue(5)),
										IsIdentity = Convert.ToBoolean(reader.GetValue(6)),
										IsPrimaryKey = Convert.ToBoolean(reader.GetValue(7)),
										IsIndexed = Convert.ToBoolean(reader.GetValue(8)),
										IsNullable = Convert.ToBoolean(reader.GetValue(9)),
										IsForeignKey = Convert.ToBoolean(reader.GetValue(10)),
										ForeignTableName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
									};

									dbColumn.ModelDataType = DBHelper.GetMySqlDataType(dbColumn);
									DatabaseColumns.Add(dbColumn);
								}
							}
						}
					}
				}
				else
				{
					string connectionString;

					if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
						connectionString = $"Server={server.ServerName};Database={db};Trusted_Connection=True;";
					else
						connectionString = $"Server={server.ServerName};Database={db};uid={server.Username};pwd={server.Password};";

					using (var connection = new SqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
select c.name as column_name, 
       x.name as datatype, 
	   case when x.name = 'nchar' then c.max_length / 2
	        when x.name = 'nvarchar' then c.max_length / 2
			when x.name = 'text' then -1
			when x.name = 'ntext' then -1
			else c.max_length 
			end as max_length,
       case when c.precision is null then 0 else c.precision end as precision,
       case when c.scale is null then 0 else c.scale end as scale,
	   c.is_nullable, 
	   c.is_computed, 
	   c.is_identity,
	   case when ( select i.is_primary_key from sys.indexes as i inner join sys.index_columns as ic on ic.object_id = i.object_id and ic.index_id = i.index_id and i.is_primary_key = 1 where i.object_id = t.object_id and ic.column_id = c.column_id ) is not null  
	        then 1 
			else 0
			end as is_primary_key,
       case when ( select count(*) from sys.index_columns as ix where ix.object_id = c.object_id and ix.column_id = c.column_id ) > 0 then 1 else 0 end as is_indexed,
	   case when ( select count(*) from sys.foreign_key_columns as f where f.parent_object_id = c.object_id and f.parent_column_id = c.column_id ) > 0 then 1 else 0 end as is_foreignkey,
	   ( select t.name from sys.foreign_key_columns as f inner join sys.tables as t on t.object_id = f.referenced_object_id where f.parent_object_id = c.object_id and f.parent_column_id = c.column_id ) as foreigntablename,
	   d.text as defaultvalue
  from sys.columns as c
 inner join sys.tables as t on t.object_id = c.object_id
 inner join sys.schemas as s on s.schema_id = t.schema_id
 inner join sys.types as x on x.system_type_id = c.system_type_id and x.user_type_id = c.user_type_id
 left outer join sys.syscomments as d on d.id = c.default_object_id
 where t.name = @tablename
   and s.name = @schema
   and x.name != 'sysname'
 order by t.name, c.column_id
";

						using (var command = new SqlCommand(query, connection))
						{
							command.Parameters.AddWithValue("@schema", table.Schema);
							command.Parameters.AddWithValue("@tablename", table.Table);

							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									var dbColumn = new DBColumn
									{
										ColumnName = codeService.CorrectForReservedNames(codeService.NormalizeClassName(reader.GetString(0))),
										EntityName = reader.GetString(0),
										DBDataType = reader.GetString(1),
										Length = Convert.ToInt64(reader.GetValue(2)),
										NumericPrecision = Convert.ToInt32(reader.GetValue(3)),
										NumericScale = Convert.ToInt32(reader.GetValue(4)),
										IsNullable = Convert.ToBoolean(reader.GetValue(5)),
										IsComputed = Convert.ToBoolean(reader.GetValue(6)),
										IsIdentity = Convert.ToBoolean(reader.GetValue(7)),
										IsPrimaryKey = Convert.ToBoolean(reader.GetValue(8)),
										IsIndexed = Convert.ToBoolean(reader.GetValue(9)),
										IsForeignKey = Convert.ToBoolean(reader.GetValue(10)),
										ForeignTableName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
										DefaultValue = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
									};


									if (string.Equals(dbColumn.DBDataType, "geometry", StringComparison.OrdinalIgnoreCase))
									{
										Listbox_Tables.SelectedIndex = -1;
										VsShellUtilities.ShowMessageBox(ServiceProvider,
																		".NET Core does not support the SQL Server geometry data type. You cannot create an entity model from this table.",
																		"Microsoft Visual Studio",
																		OLEMSGICON.OLEMSGICON_CRITICAL,
																		OLEMSGBUTTON.OLEMSGBUTTON_OK,
																		OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
										return;
									}

									if (string.Equals(dbColumn.DBDataType, "geography", StringComparison.OrdinalIgnoreCase))
									{
										Listbox_Tables.SelectedIndex = -1;
										VsShellUtilities.ShowMessageBox(ServiceProvider,
												".NET Core does not support the SQL Server geography data type. You cannot create an entity model from this table.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
										return;
									}

									if (string.Equals(dbColumn.DBDataType, "variant", StringComparison.OrdinalIgnoreCase))
									{
										Listbox_Tables.SelectedIndex = -1;
										VsShellUtilities.ShowMessageBox(ServiceProvider,
												"REST Service does not support the SQL Server sql_variant data type. You cannot create an entity model from this table.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
										return;
									}

									dbColumn.ModelDataType = DBHelper.GetSQLServerDataType(dbColumn);
									DatabaseColumns.Add(dbColumn);
								}
							}
						}
					}
				}
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if (Listbox_Tables.SelectedIndex == -1)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"You must select a database table in order to create an entity model.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				return;
			}

			Save();

			var server = (DBServer)Combobox_Server.SelectedItem;
			DatabaseTable = (DBTable)Listbox_Tables.SelectedItem;

			if (server.DBType == DBServerType.POSTGRESQL)
			{
				UndefinedEntityModels = DBHelper.GenerateEntityClassList(UndefinedEntityModels,
																		 EntityModelsFolder.Folder,
																		 ConnectionString);
			}

			DialogResult = true;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		#region Database Functions
		private void Server_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (!Populating)
				{
					Listbox_Databases.Items.Clear();
					Listbox_Tables.Items.Clear();
					var server = (DBServer)Combobox_Server.SelectedItem;
					ServerType = server.DBType;

					if (server != null)
					{
						if (server.DBType == DBServerType.SQLSERVER)
						{
							Label_ServerType.Visibility = Visibility.Visible;
							Label_ServerType_Content.Visibility = Visibility.Visible;
							Label_ServerType_Content.Content = "SQL Server";

							if (server.DBAuth == DBAuthentication.SQLSERVERAUTH)
							{
								Label_Authentication.Visibility = Visibility.Visible;
								Label_Authentication.Content = "Authentication:";
								Label_Authentication_Content.Visibility = Visibility.Visible;
								Label_Authentication_Content.Content = "SQL Server Auth";

								Label_UserName.Visibility = Visibility.Visible;
								Label_UserName_Content.Visibility = Visibility.Visible;
								Label_UserName_Content.Content = server.Username;
							}
							else
							{
								Label_Authentication.Visibility = Visibility.Visible;
								Label_Authentication.Content = "Authentication:";
								Label_Authentication_Content.Visibility = Visibility.Visible;
								Label_Authentication_Content.Content = "Windows Auth";

								Label_UserName.Visibility = Visibility.Hidden;
								Label_UserName_Content.Visibility = Visibility.Hidden;
							}
						}
						else if (server.DBType == DBServerType.POSTGRESQL)
						{
							Label_ServerType.Visibility = Visibility.Visible;
							Label_ServerType_Content.Visibility = Visibility.Visible;
							Label_ServerType_Content.Content = "Postgresql";

							Label_Authentication.Visibility = Visibility.Visible;
							Label_Authentication.Content = "Port number:";
							Label_Authentication_Content.Content = server.PortNumber.ToString();

							Label_UserName.Visibility = Visibility.Visible;
							Label_UserName_Content.Visibility = Visibility.Visible;
							Label_UserName_Content.Content = server.Username;
						}
						else if (server.DBType == DBServerType.MYSQL)
						{
							Label_ServerType.Visibility = Visibility.Visible;
							Label_ServerType_Content.Visibility = Visibility.Visible;
							Label_ServerType_Content.Content = "MySQL";

							Label_Authentication.Visibility = Visibility.Visible;
							Label_Authentication.Content = "Port number:";
							Label_Authentication_Content.Content = server.PortNumber.ToString();

							Label_UserName.Visibility = Visibility.Visible;
							Label_UserName_Content.Visibility = Visibility.Visible;
							Label_UserName_Content.Content = server.Username;
						}

						if (TestConnection(server))
							PopulateDatabases();
					}
				}
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
											error.Message,
											"Microsoft Visual Studio",
											OLEMSGICON.OLEMSGICON_CRITICAL,
											OLEMSGBUTTON.OLEMSGBUTTON_OK,
											OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}
		}

		private void AddNewServer_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var dialog = new AddConnectionDialog
				{
					LastServerUsed = (DBServer)Combobox_Server.SelectedItem
				};

				var result = dialog.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					Listbox_Databases.Items.Clear();
					Listbox_Tables.Items.Clear();
					_serverConfig.Servers.Add(dialog.Server);
					Save();

					PopulateServers();
				}
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
											error.Message,
											"Microsoft Visual Studio",
											OLEMSGICON.OLEMSGICON_CRITICAL,
											OLEMSGBUTTON.OLEMSGBUTTON_OK,
											OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}
		}

		private void RemoveServer_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var deprecatedServer = (DBServer)Combobox_Server.SelectedItem;
				var newList = new List<DBServer>();

				foreach (var server in _serverConfig.Servers)
				{
					if (!server.ServerName.Equals(deprecatedServer.ServerName, StringComparison.OrdinalIgnoreCase))
					{
						newList.Add(server);
					}
				}

				_serverConfig.Servers = newList;

				if (_serverConfig.LastServerUsed >= _serverConfig.Servers.Count())
				{
					_serverConfig.LastServerUsed = 0;
				}

				Save();

				Populating = true;
				PopulateServers();
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
											error.Message,
											"Microsoft Visual Studio",
											OLEMSGICON.OLEMSGICON_CRITICAL,
											OLEMSGBUTTON.OLEMSGBUTTON_OK,
											OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			}
		}

		private void PopulateServers()
		{
			Populating = true;
			Combobox_Server.Items.Clear();
			Listbox_Databases.Items.Clear();
			Listbox_Tables.Items.Clear();

			if (_serverConfig.Servers.Count() == 0)
			{
				Combobox_Server.IsEnabled = false;
				Combobox_Server.SelectedIndex = -1;

				Label_ServerType.Visibility = Visibility.Hidden;
				Label_ServerType_Content.Visibility = Visibility.Hidden;
				Label_ServerType_Content.Content = string.Empty;

				Label_Authentication.Visibility = Visibility.Hidden;
				Label_Authentication_Content.Visibility = Visibility.Hidden;
				Label_Authentication_Content.Content = string.Empty;

				Label_UserName.Visibility = Visibility.Hidden;
				Label_UserName_Content.Visibility = Visibility.Hidden;
				Label_UserName_Content.Content = string.Empty;
			}
			else
			{
				Combobox_Server.IsEnabled = true;

				foreach (var server in _serverConfig.Servers)
				{
					Combobox_Server.Items.Add(server);
				}

				if (Combobox_Server.Items.Count > 0)
				{
					Populating = false;

					if (Combobox_Server.Items.Count < _serverConfig.LastServerUsed)
						Combobox_Server.SelectedIndex = _serverConfig.LastServerUsed;
					else
						Combobox_Server.SelectedIndex = 0;
				}
				else
				{
					Combobox_Server.SelectedIndex = -1;
				}
			}
		}

		private void Save()
		{
			int index = 0;
			var server = (DBServer)Combobox_Server.SelectedItem;

			if (server != null)
			{
				foreach (var dbServer in _serverConfig.Servers)
				{
					if (string.Equals(dbServer.ServerName, server.ServerName, StringComparison.OrdinalIgnoreCase) &&
						dbServer.DBType == server.DBType)
					{
						_serverConfig.LastServerUsed = index;
						break;
					}

					index++;
				}
			}

			var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var dataFolder = Path.Combine(baseFolder, "rest");

			if (!Directory.Exists(dataFolder))
				Directory.CreateDirectory(dataFolder);

			var filePath = Path.Combine(dataFolder, "Servers");
			File.Delete(filePath);

			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				using (var streamWriter = new StreamWriter(stream))
				{
					using (var writer = new JsonTextWriter(streamWriter))
					{
						var serializer = new JsonSerializer();
						serializer.Serialize(writer, _serverConfig);
					}
				}
			}
		}

		private void PopulateDatabases()
		{
			var server = (DBServer)Combobox_Server.SelectedItem;

			if (server.DBType == DBServerType.POSTGRESQL)
			{
				if (string.IsNullOrWhiteSpace(server.Password))
					return;

				string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=postgres;User ID={server.Username};Password={server.Password};";

				Listbox_Databases.Items.Clear();
				Listbox_Tables.Items.Clear();
				int selectedItem = -1;

				try
				{
					using (var connection = new NpgsqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
SELECT datname 
  FROM pg_database
 WHERE datistemplate = false
   AND datname != 'postgres'
 ORDER BY datname";

						using (var command = new NpgsqlCommand(query, connection))
						{
							using (var reader = command.ExecuteReader())
							{
								int itemindex = 0;

								while (reader.Read())
								{
									var databaseName = reader.GetString(0);

									Listbox_Databases.Items.Add(databaseName);

									string cs = $"Server={server.ServerName};Port={server.PortNumber};Database={databaseName};User ID={server.Username};Password={server.Password};";

									if (DefaultConnectionString.StartsWith(cs, StringComparison.OrdinalIgnoreCase))
										selectedItem = itemindex;

									itemindex++;
								}
							}
						}
					}

					if (Listbox_Databases.Items.Count > 0)
						Listbox_Databases.SelectedIndex = selectedItem;
				}
				catch (Exception error)
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				}
			}
			else if (server.DBType == DBServerType.MYSQL)
			{
				if (string.IsNullOrWhiteSpace(server.Password))
					return;

				string connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=mysql;UID={server.Username};PWD={server.Password};";

				Listbox_Databases.Items.Clear();
				Listbox_Tables.Items.Clear();
				int selectedItem = -1;

				try
				{
					using (var connection = new MySqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
select SCHEMA_NAME from information_schema.SCHEMATA
 where SCHEMA_NAME not in ( 'information_schema', 'performance_schema', 'sys', 'mysql');";

						using (var command = new MySqlCommand(query, connection))
						{
							using (var reader = command.ExecuteReader())
							{
								int itemindex = 0;

								while (reader.Read())
								{
									var databaseName = reader.GetString(0);

									Listbox_Databases.Items.Add(databaseName);

									string cs = $"Server={server.ServerName};Port={server.PortNumber};Database={databaseName};UID={server.Username};PWD={server.Password};";

									if (DefaultConnectionString.StartsWith(cs, StringComparison.OrdinalIgnoreCase))
										selectedItem = itemindex;

									itemindex++;
								}
							}
						}
					}

					if (Listbox_Databases.Items.Count > 0)
						Listbox_Databases.SelectedIndex = selectedItem;
				}
				catch (Exception error)
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				}
			}
			else
			{
				string connectionString;

				if (server.DBAuth == DBAuthentication.SQLSERVERAUTH && string.IsNullOrWhiteSpace(server.Password))
					return;

				if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
					connectionString = $"Server={server.ServerName};Database=master;Trusted_Connection=True;";
				else
					connectionString = $"Server={server.ServerName};Database=master;uid={server.Username};pwd={server.Password};";

				Listbox_Databases.Items.Clear();
				Listbox_Tables.Items.Clear();
				int selectedItem = -1;

				try
				{
					using (var connection = new SqlConnection(connectionString))
					{
						connection.Open();

						var query = @"
select name
  from sys.databases with(nolock)
 where name not in ( 'master', 'model', 'msdb', 'tempdb' )
 order by name";

						using (var command = new SqlCommand(query, connection))
						{
							using (var reader = command.ExecuteReader())
							{
								int itemindex = 0;

								while (reader.Read())
								{
									var databaseName = reader.GetString(0);

									Listbox_Databases.Items.Add(databaseName);
									string cs;

									if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
										cs = $"Server={server.ServerName};Database={databaseName};Trusted_Connection=True;";
									else
										cs = $"Server={server.ServerName};Database={databaseName};uid={server.Username};pwd={server.Password};";

									if (DefaultConnectionString.StartsWith(cs, StringComparison.OrdinalIgnoreCase))
										selectedItem = itemindex;

									itemindex++;
								}
							}
						}
					}

					if (Listbox_Databases.Items.Count > 0)
						Listbox_Databases.SelectedIndex = selectedItem;
				}
				catch (Exception error)
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				}
			}
		}

		/// <summary>
		/// Tests to see if the server credentials are sufficient to establish a connection
		/// to the server
		/// </summary>
		/// <param name="server">The Database Server we are trying to connect to.</param>
		/// <returns></returns>
		private bool TestConnection(DBServer server)
		{
			Listbox_Tables.Items.Clear();
			Listbox_Databases.Items.Clear();

			if (server.DBType == DBServerType.POSTGRESQL)
			{
				string connectionString;
				connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=postgres;User ID={server.Username};Password={server.Password};";

				//	Attempt to connect to the database.
				try
				{
					using (var connection = new NpgsqlConnection(connectionString))
					{
						connection.Open();
					}
				}
				catch (Exception)
				{
					//	We did not succeed. We do not have sufficient information to 
					//	establish the connection.
					return false;
				}
			}
			else if (server.DBType == DBServerType.MYSQL)
			{
				string connectionString;
				connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=mysql;UID={server.Username};PWD={server.Password};";

				//	Attempt to connect to the database.
				try
				{
					using (var connection = new MySqlConnection(connectionString))
					{
						connection.Open();
					}
				}
				catch (Exception)
				{
					//	We did not succeed. We do not have sufficient information to 
					//	establish the connection.
					return false;
				}
			}
			else
			{
				//	Construct the connection string
				string connectionString;

				if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
					connectionString = $"Server={server.ServerName};Database=master;Trusted_Connection=True;";
				else
					connectionString = $"Server={server.ServerName};Database=master;uid={server.Username};pwd={server.Password};";


				//	Attempt to connect to the database.
				try
				{
					using (var connection = new SqlConnection(connectionString))
					{
						connection.Open();
					}
				}
				catch (Exception)
				{
					//	We did not succeed. We do not have sufficient information to 
					//	establish the connection.
					return false;
				}
			}

			//	If we got here, it worked. We were able to establish and close
			//	the connection.
			return true;
		}

		/// <summary>
		/// Reads the list of SQL Servers from the server configuration list
		/// </summary>
		private void ReadServerList()
		{
			//	Indicate that we are merely populating windows at this point. There are certain
			//	actions that occur during the loading of windows that mimic user interaction.
			//	There is no user interaction at this point, so there are certain actions we 
			//	do not want to run while populating.
			Populating = true;
			Combobox_Server.Items.Clear();

			//	Get the location of the server configuration on disk
			var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var dataFolder = Path.Combine(baseFolder, "rest");

			if (!Directory.Exists(dataFolder))
				Directory.CreateDirectory(dataFolder);

			var filePath = Path.Combine(dataFolder, "Servers");

			//	Read the ServerConfig into memory. If one does not exist
			//	create an empty one.
			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				using (var streamReader = new StreamReader(stream))
				{
					using (var reader = new JsonTextReader(streamReader))
					{
						var serializer = new JsonSerializer();

						_serverConfig = serializer.Deserialize<ServerConfig>(reader);

						if (_serverConfig == null)
							_serverConfig = new ServerConfig();
					}
				}
			}

			PopulateServers();

			//	We're done. Turn off the populating flag.
			Populating = false;
		}
		#endregion

		#region Utility Functions
		private void ConstructPostgresqlColumn(DBTable table, NpgsqlDataReader reader)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			var entityName = reader.GetString(0);
			var columnName = codeService.CorrectForReservedNames(codeService.NormalizeClassName(reader.GetString(0)));

			var dbColumn = new DBColumn
			{
				ColumnName = codeService.CorrectForReservedNames(codeService.NormalizeClassName(reader.GetString(0))),
				EntityName = entityName,
				DBDataType = reader.GetString(1),
				Length = Convert.ToInt64(reader.GetValue(2)),
				NumericPrecision = Convert.ToInt32(reader.GetValue(3)),
				NumericScale = Convert.ToInt32(reader.GetValue(4)),
				IsNullable = Convert.ToBoolean(reader.GetValue(5)),
				IsComputed = Convert.ToBoolean(reader.GetValue(6)),
				IsIdentity = Convert.ToBoolean(reader.GetValue(7)),
				IsPrimaryKey = Convert.ToBoolean(reader.GetValue(8)),
				IsIndexed = Convert.ToBoolean(reader.GetValue(9)),
				IsForeignKey = Convert.ToBoolean(reader.GetValue(10)),
				ForeignTableName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
			};

			dbColumn.ModelDataType = DBHelper.GetPostgresDataType(dbColumn);

			if (string.IsNullOrWhiteSpace(dbColumn.ModelDataType))
			{
				var theChildEntityClass = codeService.GetEntityClassBySchema(table.Schema, dbColumn.DBDataType);

				//	See if this column type is already defined...
				if (theChildEntityClass == null)
				{
					//	It's not defined. See if it is already included in the undefined list...
					if (UndefinedEntityModels.FirstOrDefault(ent =>
					   string.Equals(ent.SchemaName, table.Schema, StringComparison.OrdinalIgnoreCase) &&
					   string.Equals(ent.TableName, dbColumn.DBDataType, StringComparison.OrdinalIgnoreCase)) == null)
					{
						//	It's not defined, and it's not in the undefined list, so it is unknown. Let's make it known
						//	by constructing it and including it in the undefined list.
						entityName = dbColumn.DBDataType;
						var className = $"E{codeService.CorrectForReservedNames(codeService.NormalizeClassName(entityName))}";

						var entity = new EntityModel()
						{
							SchemaName = table.Schema,
							ClassName = className,
							TableName = entityName,
							Folder = Path.Combine(EntityModelsFolder.Folder, $"{className}.cs"),
							Namespace = EntityModelsFolder.Namespace,
							ServerType = DBServerType.POSTGRESQL,
							ProjectName = EntityModelsFolder.ProjectName
						};

						UndefinedEntityModels.Add(entity);
					}
				}

				dbColumn.ModelDataType = dbColumn.DBDataType;
			}

			DatabaseColumns.Add(dbColumn);
		}

		private void WarnUndefinedContent(DBTable table, string connectionString)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var message = new StringBuilder();
			message.Append($"The entity model {table.Table} uses ");

			var unknownEnums = new List<string>();
			var unknownComposits = new List<string>();
			var unknownTables = new List<string>();

			foreach (var unknownClass in UndefinedEntityModels)
			{
				unknownClass.ElementType = DBHelper.GetElementType(mDte, unknownClass.SchemaName, unknownClass.TableName, connectionString);

				if (unknownClass.ElementType == ElementType.Enum)
					unknownEnums.Add(unknownClass.TableName);
				else if (unknownClass.ElementType == ElementType.Composite)
					unknownComposits.Add(unknownClass.TableName);
				else if (unknownClass.ElementType == ElementType.Table)
					unknownTables.Add(unknownClass.TableName);
			}

			if (unknownEnums.Count > 0)
			{
				if (unknownEnums.Count > 1)
					message.Append("enum types of ");
				else
					message.Append("an enum type of ");

				for (int index = 0; index < unknownEnums.Count(); index++)
				{
					if (index == unknownEnums.Count() - 1 && unknownEnums.Count > 1)
						message.Append($" and {unknownEnums[index]}");
					else if (index > 0)
						message.Append($", {unknownEnums[index]}");
					else if (index == 0)
						message.Append(unknownEnums[index]);
				}
			}

			if (unknownComposits.Count > 0)
			{
				if (unknownEnums.Count > 0)
					message.Append("and ");

				if (unknownComposits.Count > 1)
					message.Append("composite types of ");
				else
					message.Append("a composite type of ");

				for (int index = 0; index < unknownComposits.Count(); index++)
				{
					if (index == unknownComposits.Count() - 1 && unknownComposits.Count > 1)
						message.Append($" and {unknownComposits[index]}");
					else if (index > 0)
						message.Append($", {unknownComposits[index]}");
					else if (index == 0)
						message.Append(unknownComposits[index]);
				}
			}

			if (unknownTables.Count > 0)
			{
				if (unknownEnums.Count > 0 || unknownComposits.Count > 0)
					message.Append("and ");

				if (unknownTables.Count > 1)
					message.Append("table types of ");
				else
					message.Append("a table type of ");

				for (int index = 0; index < unknownTables.Count(); index++)
				{
					if (index == unknownTables.Count() - 1 && unknownTables.Count > 1)
						message.Append($" and {unknownTables[index]}");
					else if (index > 0)
						message.Append($", {unknownTables[index]}");
					else if (index == 0)
						message.Append(unknownTables[index]);
				}
			}

			message.Append(".\r\n\r\nYou cannot generate this class until all the dependencies have been generated. Would you like to generate the undefined entities as part of generating this class?");
			var shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

			if (!VsShellUtilities.PromptYesNo(
						message.ToString(),
						"Microsoft Visual Studio",
						OLEMSGICON.OLEMSGICON_WARNING,
						shell))
			{
				Button_OK.IsEnabled = false;
				Button_OK.IsDefault = false;
				Button_Cancel.IsDefault = true;
			}
		}

		#endregion
	}
}
