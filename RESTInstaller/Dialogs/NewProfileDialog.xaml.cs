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
using RESTInstaller.Models;
using RESTInstaller.Services;
using Path = System.IO.Path;

namespace RESTInstaller.Dialogs
{
	/// <summary>
	/// Interaction logic for NewProfileDialog.xaml
	/// </summary>
	public partial class NewProfileDialog : DialogWindow
	{
		#region Variables
		public ResourceClass ResourceModel { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
		#endregion

		public NewProfileDialog()
		{
			InitializeComponent();
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			var resourceClassList = codeService.GetResourceClassList();


			if (resourceClassList.Count == 0)
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

			foreach (var resourceClass in resourceClassList)
				Combobox_ResourceClasses.Items.Add(resourceClass);

			Combobox_ResourceClasses.SelectedIndex = 0;

			Button_OK.IsEnabled = true;
			Button_OK.IsDefault = true;
			Button_Cancel.IsDefault = true;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if (Combobox_ResourceClasses.SelectedIndex == -1)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"No resource model selected. Please select a resource model to generate the resource/entity mapping.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				return;
			}

			ResourceModel = (ResourceClass)Combobox_ResourceClasses.SelectedItem;

			DialogResult = true;
			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
