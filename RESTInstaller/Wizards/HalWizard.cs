using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using RESTInstaller.Dialogs;
using RESTInstaller.Models;
using RESTInstaller.Services;

namespace RESTInstaller.Wizards
{
    public class HalWizard : IWizard
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

        /// <summary>
        /// Start generating the entity model
        /// </summary>
        /// <param name="automationObject"></param>
        /// <param name="replacementsDictionary"></param>
        /// <param name="runKind"></param>
        /// <param name="customParams"></param>
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var codeService = ServiceFactory.GetService<ICodeService>();
            var shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            IVsThreadedWaitDialog2 waitDialog = null;

            try
            {
                var configurationFolder = codeService.ConfigurationFolder;
                var installationFolder = codeService.InstallationFolder;

                //  Make sure we are where we're supposed to be
                if (!codeService.IsChildOf(configurationFolder.Folder, installationFolder.Folder))
                {
                    if (!VsShellUtilities.PromptYesNo(
                                $"You are attempting to install a Hal Configuration into {codeService.GetRelativeFolder(installationFolder)}. Typically, Hal configurations reside in {codeService.GetRelativeFolder(configurationFolder)}.\r\n\r\nDo you wish to place the new Hal Configuration in this non-standard location?",
                                "Microsoft Visual Studio",
                                OLEMSGICON.OLEMSGICON_WARNING,
                                shell))
                    {
                        Proceed = false;
                        return;
                    }

                    codeService.SaveProjectMapping();
                }

                var controllerList = codeService.GetListOfControllers();
                var classList = codeService.GetListOfResourceModels();

                //	Construct the form, and fill in all the prerequisite data
                var form = new NewHalConfigurationDialog
                {
                    Controllers = controllerList,
                    ResourceModels = classList,
                    ServiceProvider = ServiceProvider.GlobalProvider
                };

                var isok = form.ShowDialog();

                if (isok.HasValue && isok.Value == true)
                {
                    if (ServiceProvider.GlobalProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) is IVsThreadedWaitDialogFactory dialogFactory)
                    {
                        dialogFactory.CreateInstance(out waitDialog);
                    }

                    if (waitDialog != null && waitDialog.StartWaitDialog("Microsoft Visual Studio",
                                                                 "Building Hal Configuration",
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 null,
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 0,
                                                                 false, true) == VSConstants.S_OK)
                    {
                        //	We will need these when we replace placeholders in the class
                        var className = form.ClassName;
                        var controllerName = form.ControllerName;
                        var resourceClass = codeService.FindClass(className);
                        var controllerClass = codeService.FindClass(controllerName);

                        var resourceModelNamespace = resourceClass.Namespace.Name;
                        var controllersNamespace = controllerClass.Namespace.Name;

                        replacementsDictionary.Add("$resourcemodelsnamespace$", resourceModelNamespace);
                        replacementsDictionary.Add("$controllersnamespace$", controllersNamespace);

                        var emitter = new Emitter();

                        string model = string.Empty;

                        waitDialog.UpdateProgress($"Building Hal Configuration",
                                                  $"Building {replacementsDictionary["$safeitemname$"]}",
                                                  $"Building {replacementsDictionary["$safeitemname$"]}",
                                                  0,
                                                  0,
                                                  true,
                                                  out bool fpCanceled);

                        model = emitter.EmitHalConfiguration(codeService,
                                                             controllerName,
                                                             className,
                                                             replacementsDictionary);

                        replacementsDictionary.Add("$model$", model);
                        Proceed = true;

                        waitDialog.EndWaitDialog(out int usercancel);
                    }
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
