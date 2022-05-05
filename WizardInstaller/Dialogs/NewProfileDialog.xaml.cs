using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WizardInstaller.Template.Models;
using WizardInstaller.Template.Services;
using Path = System.IO.Path;

namespace WizardInstaller.Template.Dialogs
{
	/// <summary>
	/// Interaction logic for NewProfileDialog.xaml
	/// </summary>
	public partial class NewProfileDialog : DialogWindow
	{
		#region Variables
		private ServerConfig _serverConfig;
		private bool Populating = true;
		public string ConnectionString { get; set; }
		public string DefaultConnectionString { get; set; }
		public ResourceClass ResourceModel { get; set; }
		public DBServerType ServerType { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
		#endregion

		public NewProfileDialog()
		{
			InitializeComponent();
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();

			if (codeService.ResourceClassList.Count == 0)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"No resource models were found in the project. Please create a corresponding resource model before attempting to create the resource/entity mapping.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				DialogResult = false;
				Close();
			}

			foreach (var resourceClass in codeService.ResourceClassList)
				Combobox_ResourceClasses.Items.Add(resourceClass);

			Button_OK.IsEnabled = false;
			Button_OK.IsDefault = false;
			Button_Cancel.IsDefault = true;

			ReadServerList();
		}

		private void Databases_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				Listbox_Tables.SelectedIndex = -1;

				var server = (DBServer)Combobox_Server.SelectedItem;
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

			Button_Cancel.IsDefault = false;
			Button_OK.IsEnabled = true;
			Button_OK.IsDefault = true;

			try
			{
				var server = (DBServer)Combobox_Server.SelectedItem;

				if (server == null)
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
								"You must select a database server to create a new resource/entity mapping. Please select a database server and try again.",
								"Microsoft Visual Studio",
								OLEMSGICON.OLEMSGICON_WARNING,
								OLEMSGBUTTON.OLEMSGBUTTON_OK,
								OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					Button_OK.IsEnabled = false;
					Button_OK.IsDefault = false;
					Button_Cancel.IsDefault = true;

					return;
				}

				var db = (string)Listbox_Databases.SelectedItem;
				if (string.IsNullOrWhiteSpace(db))
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
								"You must select a database to create a new resource/entity mapping. Please select a database and try again.",
								"Microsoft Visual Studio",
								OLEMSGICON.OLEMSGICON_WARNING,
								OLEMSGBUTTON.OLEMSGBUTTON_OK,
								OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					Button_OK.IsEnabled = false;
					Button_OK.IsDefault = false;
					Button_Cancel.IsDefault = true;

					return;
				}
				var table = (DBTable)Listbox_Tables.SelectedItem;

				if (table == null)
				{
					VsShellUtilities.ShowMessageBox(ServiceProvider,
								"You must select a database table to create a new resource/entity mapping. Please select a database table and try again.",
								"Microsoft Visual Studio",
								OLEMSGICON.OLEMSGICON_WARNING,
								OLEMSGBUTTON.OLEMSGBUTTON_OK,
								OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					Button_OK.IsEnabled = false;
					Button_OK.IsDefault = false;
					Button_Cancel.IsDefault = true;

					return;
				}

				bool foundit = false;

				for (int i = 0; i < Combobox_ResourceClasses.Items.Count; i++)
				{
					var resource = (ResourceClass)Combobox_ResourceClasses.Items[i];

					if (resource.Entity != null &&
						 string.Equals(resource.Entity.TableName, table.Table, StringComparison.OrdinalIgnoreCase) &&
						 string.Equals(resource.Entity.SchemaName, table.Schema, StringComparison.OrdinalIgnoreCase))
					{
						Combobox_ResourceClasses.SelectedIndex = i;
						foundit = true;
						break;
					}
				}

				if (!foundit)
				{
					Combobox_ResourceClasses.SelectedIndex = -1;
					Listbox_Tables.SelectedIndex = -1;

					VsShellUtilities.ShowMessageBox(ServiceProvider,
								"No matching resource class found. You will not be able to create a resource/entity mapping without a matching resource model.",
								"Microsoft Visual Studio",
								OLEMSGICON.OLEMSGICON_WARNING,
								OLEMSGBUTTON.OLEMSGBUTTON_OK,
								OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					Button_OK.IsEnabled = false;
					Button_OK.IsDefault = false;
					Button_Cancel.IsDefault = true;
					return;
				}

				ResourceModel = (ResourceClass)Combobox_ResourceClasses.SelectedItem;
			}
			catch (Exception error)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
							error.Message,
							"Microsoft Visual Studio",
							OLEMSGICON.OLEMSGICON_CRITICAL,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				Button_OK.IsEnabled = false;
				Button_OK.IsDefault = false;
				Button_Cancel.IsDefault = true;
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			Save();

			ResourceModel = (ResourceClass)Combobox_ResourceClasses.SelectedItem;

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
	}
}
