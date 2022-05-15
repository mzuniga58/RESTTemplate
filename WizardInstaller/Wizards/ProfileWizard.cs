using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WizardInstaller.Template.Dialogs;
using WizardInstaller.Template.Services;

namespace WizardInstaller.Template.Wizards
{
	public class ProfileWizard : IWizard
	{
		private bool Proceed = false;

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunFinished()
		{
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var codeService = ServiceFactory.GetService<ICodeService>();
			var shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
			IVsThreadedWaitDialog2 waitDialog = null;

			try
			{
				//  Load the project mapping information
				var projectMapping = codeService.LoadProjectMapping();
				var installationFolder = codeService.InstallationFolder;

				//  Make sure we are where we're supposed to be
				if (!codeService.IsChildOf(projectMapping.MappingFolder, installationFolder.Folder))
				{
					var mappingFolder = projectMapping.GetMappingFolder();

					if (!VsShellUtilities.PromptYesNo(
							$"You are attempting to install a resource/entity mapping model into {codeService.GetRelativeFolder(installationFolder)}. Typically, resource/entity mapping models reside in {codeService.GetRelativeFolder(mappingFolder)}.\r\n\r\nDo you wish to place the new resource/entity mapping model in this non-standard location?",
							"Microsoft Visual Studio",
							OLEMSGICON.OLEMSGICON_WARNING,
							shell))
					{
						Proceed = false;
						return;
					}

					projectMapping.MappingFolder = installationFolder.Folder;
					projectMapping.MappingNamespace = installationFolder.Namespace;
					projectMapping.MappingProject = installationFolder.ProjectName;

					codeService.SaveProjectMapping();
				}

				var form = new NewProfileDialog()
				{
					ServiceProvider = ServiceProvider.GlobalProvider
				};

				var result = form.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					if (ServiceProvider.GlobalProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) is IVsThreadedWaitDialogFactory dialogFactory)
					{
						dialogFactory.CreateInstance(out waitDialog);
					}

					if (waitDialog != null && waitDialog.StartWaitDialog("Microsoft Visual Studio",
																 "Constructing conversion maps",
																 $"Building {replacementsDictionary["$safeitemname$"]}",
																 null,
																 $"Building {replacementsDictionary["$safeitemname$"]}",
																 0,
																 false, true) == VSConstants.S_OK)
					{
						var resourceModel = form.ResourceModel;

						var emitter = new Emitter();
						var model = emitter.EmitMappingModel(resourceModel, replacementsDictionary["$safeitemname$"]);

						replacementsDictionary["$resourcenamespace$"] = resourceModel.Namespace;
						replacementsDictionary["$entitynamespace$"] = resourceModel.Entity.Namespace;
						replacementsDictionary["$model$"] = model;

						waitDialog.EndWaitDialog(out int usercancel);
					}

					Proceed = true;
				}
				else
					Proceed = false;
			}
			catch (Exception error)
			{
                if (waitDialog != null)
					waitDialog.EndWaitDialog(out _);

				VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider,
												error.Message,
												"Microsoft Visual Studio",
												OLEMSGICON.OLEMSGICON_CRITICAL,
												OLEMSGBUTTON.OLEMSGBUTTON_OK,
												OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
				Proceed = false;
			}
		}

		public bool ShouldAddProjectItem(string filePath)
		{
			return Proceed;
		}
	}
}
