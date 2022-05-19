using EnvDTE80;
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
    /// Interaction logic for NewHalConfigurationDialog.xaml
    /// </summary>
    public partial class NewHalConfigurationDialog : DialogWindow
    {
		public string ClassName { get; set; }
		public string ControllerName { get; set; }
		public List<string> Controllers { get; set; }
		public List<string> ResourceModels { get; set; }
		public IServiceProvider ServiceProvider { get; set; }


		public NewHalConfigurationDialog()
        {
            InitializeComponent();
        }
		private void OnLoad(object sender, RoutedEventArgs e)
		{
			foreach (var controller in Controllers)
				Combobox_Controller.Items.Add(controller);

			foreach (var model in ResourceModels)
				Combobox_ClassName.Items.Add(model);	

			if ( Combobox_ClassName.Items.Count > 0 )
				Combobox_ClassName.SelectedIndex = 0;	

			if (Combobox_Controller.Items.Count > 0)
				Combobox_Controller.SelectedIndex = 0;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if (Combobox_ClassName.SelectedIndex == -1)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"You must select a Resource Model in order to create a Hal Configuration.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				return;
			}

			if (Combobox_Controller.SelectedIndex == -1)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"You must select a Controller in order to create a Hal Configuration.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				return;
			}

			ClassName = Combobox_ClassName.SelectedItem as string;
			ControllerName = Combobox_Controller.SelectedItem as string;

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
