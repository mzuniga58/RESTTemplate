using Microsoft.VisualStudio.PlatformUI;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WizardInstaller.Template.Models;

namespace WizardInstaller.Template.Dialogs
{
	/// <summary>
	/// Interaction logic for AddConnectionDialog.xaml
	/// </summary>
	public partial class AddConnectionDialog : DialogWindow, IDisposable
	{
		private bool disposedValue;
		public DBServer Server { get; set; }
		public DBServer LastServerUsed { get; set; }
		public bool Populating = false;

		public AddConnectionDialog()
		{
			InitializeComponent();
			VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			Populating = true;
			Combobox_ServerType.Items.Clear();
			Combobox_ServerType.Items.Add("My SQL");
			Combobox_ServerType.Items.Add("Postgresql");
			Combobox_ServerType.Items.Add("SQL Server");

			Combobox_Authentication.Items.Clear();
			Combobox_Authentication.Items.Add("Windows Authority");
			Combobox_Authentication.Items.Add("SQL Server Authority");

			if (LastServerUsed == null)
			{
				Combobox_ServerType.SelectedIndex = -1;
				Combobox_Authentication.IsEnabled = false;
				Combobox_Authentication.Visibility = Visibility.Hidden;

				Label_Authentication.Content = "Port Number";
				Textbox_PortNumber.Visibility = Visibility.Visible;
				Textbox_PortNumber.IsEnabled = true;
				Textbox_PortNumber.Text = string.Empty;

				Label_UserName.IsEnabled = true;
				Label_UserName.Visibility = Visibility.Visible;
				Textbox_UserName.Text = string.Empty;
				Textbox_UserName.IsEnabled = true;
				Textbox_UserName.Visibility = Visibility.Visible;

				Label_Password.IsEnabled = true;
				Label_Password.Visibility = Visibility.Visible;
				Textbox_Password.IsEnabled = true;
				Textbox_Password.Visibility = Visibility.Visible;
				Textbox_Password.Password = string.Empty;
				Label_CheckConnection.Content = "Connection is not verified";
			}
			else if (LastServerUsed.DBType == DBServerType.MYSQL)
			{
				Combobox_ServerType.SelectedIndex = 0;
				Combobox_Authentication.IsEnabled = false;
				Combobox_Authentication.Visibility = Visibility.Hidden;
				Label_Authentication.Content = "Port Number";
				Textbox_PortNumber.Visibility = Visibility.Visible;
				Textbox_PortNumber.IsEnabled = true;
				Textbox_PortNumber.Text = "3306";
				Label_UserName.IsEnabled = true;
				Label_UserName.Visibility = Visibility.Visible;
				Textbox_UserName.Text = "root";
				Textbox_UserName.IsEnabled = true;
				Textbox_UserName.Visibility = Visibility.Visible;
				Label_Password.IsEnabled = true;
				Label_Password.Visibility = Visibility.Visible;
				Textbox_Password.IsEnabled = true;
				Textbox_Password.Visibility = Visibility.Visible;
				Label_CheckConnection.Content = "Connection is not verified";
			}
			else if (LastServerUsed.DBType == DBServerType.POSTGRESQL)
			{
				Combobox_ServerType.SelectedIndex = 1;
				Combobox_Authentication.IsEnabled = false;
				Combobox_Authentication.Visibility = Visibility.Hidden;
				Label_Authentication.Content = "Port Number";
				Textbox_PortNumber.Visibility = Visibility.Visible;
				Textbox_PortNumber.IsEnabled = true;
				Textbox_PortNumber.Text = "5432";

				Label_UserName.IsEnabled = true;
				Label_UserName.Visibility = Visibility.Visible;
				Textbox_UserName.IsEnabled = true;
				Textbox_UserName.Visibility = Visibility.Visible;
				Textbox_UserName.Text = "postgres";
				Label_Password.IsEnabled = true;
				Label_Password.Visibility = Visibility.Visible;
				Textbox_Password.IsEnabled = true;
				Textbox_Password.Visibility = Visibility.Visible;
				Label_CheckConnection.Content = "Connection is not verified";
			}
			else
			{
				Combobox_ServerType.SelectedIndex = 2;
				Combobox_Authentication.IsEnabled = true;
				Combobox_Authentication.Visibility = Visibility.Visible;
				Combobox_Authentication.SelectedIndex = 0;

				Label_Authentication.Content = "Authentication";
				Textbox_PortNumber.Visibility = Visibility.Hidden;
				Textbox_PortNumber.IsEnabled = false;

				Label_UserName.IsEnabled = false;
				Label_UserName.Visibility = Visibility.Hidden;
				Textbox_UserName.IsEnabled = false;
				Textbox_UserName.Visibility = Visibility.Hidden;

				Label_Password.IsEnabled = false;
				Label_Password.Visibility = Visibility.Hidden;
				Textbox_Password.IsEnabled = false;
				Textbox_Password.Visibility = Visibility.Hidden;
				Label_CheckConnection.Content = "Connection is not verified";
			}

			Populating = false;
		}

		private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
		{
			RefreshColors();
		}

		private Color ConvertColor(System.Drawing.Color clr)
		{
			return Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
		}

		private void RefreshColors()
		{
			MainGrid.Background = new SolidColorBrush(ConvertColor(VSColorTheme.GetThemedColor(EnvironmentColors.BrandedUIBackgroundColorKey)));
		}

		private void ServerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Populating)
				return;

			try
			{
				if (Combobox_ServerType.SelectedIndex == 0 || Combobox_ServerType.SelectedIndex == 1)
				{
					Label_Authentication.Content = "Port Number";

					Combobox_Authentication.IsEnabled = false;
					Combobox_Authentication.Visibility = Visibility.Hidden;

					Textbox_PortNumber.IsEnabled = true;
					Textbox_PortNumber.Visibility = Visibility.Visible;

					Textbox_UserName.IsEnabled = true;
					Textbox_UserName.Visibility = Visibility.Visible;

					Label_UserName.IsEnabled = true;
					Label_UserName.Visibility = Visibility.Visible;

					Textbox_Password.IsEnabled = true;
					Textbox_Password.Visibility = Visibility.Visible;

					Label_Password.IsEnabled = true;
					Label_Password.Visibility = Visibility.Visible;

					Textbox_UserName.Text = Combobox_ServerType.SelectedIndex == 0 ? "root" : "postgres";
					Textbox_PortNumber.Text = Combobox_ServerType.SelectedIndex == 0 ? "3306" : "5432";
					Label_CheckConnection.Content = "Connection is not verified";
				}
				else
				{
					Label_Authentication.Content = "Authentication";
					Combobox_Authentication.Visibility = Visibility.Visible;
					Combobox_Authentication.IsEnabled = true;
					Combobox_Authentication.SelectedIndex = 0;

					Textbox_PortNumber.IsEnabled = false;
					Textbox_PortNumber.Visibility = Visibility.Hidden;
					Textbox_UserName.Text = string.Empty;

					Label_UserName.IsEnabled = false;
					Label_UserName.Visibility = Visibility.Hidden;

					Textbox_UserName.IsEnabled = false;
					Textbox_UserName.Visibility = Visibility.Hidden;

					Label_Password.IsEnabled = false;
					Label_Password.Visibility = Visibility.Hidden;

					Textbox_Password.IsEnabled = false;
					Textbox_Password.Visibility = Visibility.Hidden;

					Label_CheckConnection.Content = "Connection is not verified";
				}
			}
			catch (Exception error)
			{
				MessageBox.Show(error.Message, "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Authentication_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Populating)
				return;

			if (Combobox_ServerType.SelectedIndex != 2)
				return;

			if (Combobox_Authentication.SelectedIndex == 1)         //	SQL Server Authority
			{
				Label_UserName.IsEnabled = true;
				Label_UserName.Visibility = Visibility.Visible;

				Textbox_UserName.IsEnabled = true;
				Textbox_UserName.Visibility = Visibility.Visible;

				Label_Password.IsEnabled = true;
				Label_Password.Visibility = Visibility.Visible;

				Textbox_Password.IsEnabled = true;
				Textbox_Password.Visibility = Visibility.Visible;

				Label_CheckConnection.Content = "Connection is not verified";
			}
			else
			{
				Label_UserName.IsEnabled = false;
				Label_UserName.Visibility = Visibility.Hidden;

				Textbox_UserName.IsEnabled = false;
				Textbox_UserName.Visibility = Visibility.Hidden;

				Label_Password.IsEnabled = false;
				Label_Password.Visibility = Visibility.Hidden;

				Textbox_Password.IsEnabled = false;
				Textbox_Password.Visibility = Visibility.Hidden;

				Label_CheckConnection.Content = "Connection is not verified";
			}
		}

		private void CheckConnection_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Textbox_ServerName.Text))
			{
				MessageBox.Show("You must provide a server name.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (Combobox_ServerType.SelectedIndex == 0)
			{
				if (string.IsNullOrWhiteSpace(Textbox_UserName.Text))
				{
					MessageBox.Show("You must provide a user name.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
				{
					MessageBox.Show("You must provide a password.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				if (string.IsNullOrWhiteSpace(Textbox_PortNumber.Text))
				{
					MessageBox.Show("You must provide a port.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}
			else if (Combobox_ServerType.SelectedIndex == 1)
			{
				if (string.IsNullOrWhiteSpace(Textbox_UserName.Text))
				{
					MessageBox.Show("You must provide a user name.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
				{
					MessageBox.Show("You must provide a password.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				if (string.IsNullOrWhiteSpace(Textbox_PortNumber.Text))
				{
					MessageBox.Show("You must provide a port.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}
			else
			{
				if (Combobox_Authentication.SelectedIndex == 0)
				{
					if (Combobox_Authentication.SelectedIndex == 1)
					{
						if (string.IsNullOrWhiteSpace(Textbox_UserName.Text))
						{
							MessageBox.Show("You must provide a user name.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
						{
							MessageBox.Show("You must provide a password.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
					}
				}
			}

			CheckConnection();
		}

		private bool CheckConnection()
		{
			string connectionString;

			if (Combobox_ServerType.SelectedIndex == -1)
				return false;

			if (Combobox_ServerType.SelectedIndex == 2)
				if (Combobox_Authentication.SelectedIndex == -1)
					return false;

			var server = new DBServer
			{
				DBType = Combobox_ServerType.SelectedIndex == 0 ? DBServerType.MYSQL : Combobox_ServerType.SelectedIndex == 1 ? DBServerType.POSTGRESQL : DBServerType.SQLSERVER,
				DBAuth = Combobox_Authentication.SelectedIndex == 1 ? DBAuthentication.SQLSERVERAUTH : DBAuthentication.WINDOWSAUTH,
				ServerName = Textbox_ServerName.Text,
				Username = Textbox_UserName.Text,
				Password = Textbox_Password.Password,
			};

			if (string.IsNullOrEmpty(server.ServerName))
				return false;

			if (server.DBType == DBServerType.SQLSERVER)
			{
				try
				{
					if (server.DBAuth == DBAuthentication.SQLSERVERAUTH)
					{
						if (string.IsNullOrWhiteSpace(server.Username))
							return false;
						if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
							return false;
					}

					if (server.DBAuth == DBAuthentication.WINDOWSAUTH)
						connectionString = $"Server={server.ServerName};Database=master;Trusted_Connection=True;";
					else
						connectionString = $"Server={server.ServerName};Database=master;uid={server.Username};pwd={Textbox_Password.Password};";

					using (var connection = new SqlConnection(connectionString))
					{
						connection.Open();
						Label_CheckConnection.Content = "Connection verified";
						Server = server;
						return true;
					}
				}
				catch (Exception)
				{
					Label_CheckConnection.Content = "Connection is not verified";
					Server = null;
				}
			}
			else if (server.DBType == DBServerType.POSTGRESQL)
			{
				if (string.IsNullOrWhiteSpace(server.Username))
					return false;
				if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
					return false;
				if (string.IsNullOrWhiteSpace(Textbox_PortNumber.Text))
					return false;

				if (Int32.TryParse(Textbox_PortNumber.Text, out var portNumber))
					server.PortNumber = portNumber;
				else
					return false;

				try
				{
					connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=postgres;User ID={server.Username};Password={Textbox_Password.Password};";

					using (var connection = new NpgsqlConnection(connectionString))
					{
						connection.Open();
						Label_CheckConnection.Content = "Connection verified";
						Server = server;
						return true;
					}
				}
				catch (Exception)
				{
					Label_CheckConnection.Content = "Connection is not verified";
					Server = null;
				}
			}
			else if (server.DBType == DBServerType.MYSQL)
			{
				if (string.IsNullOrWhiteSpace(server.Username))
					return false;
				if (string.IsNullOrWhiteSpace(Textbox_Password.Password))
					return false;
				if (string.IsNullOrWhiteSpace(Textbox_PortNumber.Text))
					return false;

				if (Int32.TryParse(Textbox_PortNumber.Text, out var portNumber))
					server.PortNumber = portNumber;
				else
					return false;

				try
				{
					connectionString = $"Server={server.ServerName};Port={server.PortNumber};Database=mysql;UID={server.Username};PWD={Textbox_Password.Password};";

					using (var connection = new MySqlConnection(connectionString))
					{
						connection.Open();
						Label_CheckConnection.Content = "Connection verified";
						Server = server;
						return true;
					}
				}
				catch (Exception)
				{
					Label_CheckConnection.Content = "Connection is not verified";
					Server = null;
				}
			}

			return false;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckConnection())
			{
				MessageBox.Show("Could not establish a connection to the server. Check your settings and credentials.", "Microsoft Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			switch (Combobox_ServerType.SelectedIndex)
			{
				case 0:
					Server.DBType = DBServerType.MYSQL;
					break;

				case 1:
					Server.DBType = DBServerType.POSTGRESQL;
					break;

				case 2:
					Server.DBType = DBServerType.SQLSERVER;
					break;

				default:
					Server.DBType = DBServerType.SQLSERVER;
					break;
			}

			DialogResult = true;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs s)
		{
			DialogResult = false;
			Close();

		}

		#region Dispose
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~AddConnectionDialog()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
