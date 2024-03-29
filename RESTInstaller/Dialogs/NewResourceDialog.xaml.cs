﻿using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using RESTInstaller.Models;
using RESTInstaller.Services;

namespace RESTInstaller.Dialogs
{
    /// <summary>
    /// Interaction logic for NewResourceDialog.xaml
    /// </summary>
    public partial class NewResourceDialog : DialogWindow
	{
		#region Variables
		public EntityClass EntityModel { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
		#endregion

		public NewResourceDialog()
		{
			InitializeComponent();
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var codeService = ServiceFactory.GetService<ICodeService>();
			var entityClassList = codeService.GetEntityClassList();

			if (entityClassList.Count == 0)
			{
				VsShellUtilities.ShowMessageBox(ServiceProvider,
												"No entity models were found in the project. Please create a corresponding entity model before attempting to create the resource model.",
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				DialogResult = false;
				Close();
			}

			foreach (var entityClass in entityClassList)
			{
				if (entityClass.ElementType == ElementType.Table ||
					entityClass.ElementType == ElementType.Composite)
				{
					Combobox_EntityClasses.Items.Add(entityClass);
				}
			}

			Combobox_EntityClasses.SelectedIndex = 0;

			Button_Cancel.IsDefault = true;
			Button_OK.IsEnabled = true;
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			if ( Combobox_EntityClasses.SelectedIndex == -1 )
            {
				VsShellUtilities.ShowMessageBox(ServiceProvider,
							"No entity model selected. You must select an entity model to generate a resource model.",
							"Microsoft Visual Studio",
							OLEMSGICON.OLEMSGICON_CRITICAL,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				return;
			}

			EntityModel = (EntityClass)Combobox_EntityClasses.SelectedItem;

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
