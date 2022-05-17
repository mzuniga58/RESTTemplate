using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using RESTInstaller.Dialogs;
using RESTInstaller.Services;

namespace RESTInstaller.Wizards
{
    public class ResourceModelWizard : IWizard
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
            var codeService = ServiceFactory.GetService<ICodeService>();
            codeService.AddResource(projectItem);
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var mDte = automationObject as DTE2;
            var codeService = ServiceFactory.GetService<ICodeService>();
            var shell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            IVsThreadedWaitDialog2 waitDialog = null;

            try
            {
                var projectMapping = codeService.LoadProjectMapping();
                var installationFolder = codeService.InstallationFolder;
                var connectionString = codeService.ConnectionString;

                //  Make sure we are where we're supposed to be
                if (!codeService.IsChildOf(projectMapping.ResourceFolder, installationFolder.Folder))
                {
                    mDte.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationBuild);
                    var resourceModelsFolder = projectMapping.GetResourceModelsFolder();

                    if (!VsShellUtilities.PromptYesNo(
                            $"You are attempting to install a resource model into {codeService.GetRelativeFolder(installationFolder)}. Typically, resource models reside in {codeService.GetRelativeFolder(resourceModelsFolder)}.\r\n\r\nDo you wish to place the new resource model in this non-standard location?", "Microsoft Visual Studio",
                            OLEMSGICON.OLEMSGICON_WARNING,
                            shell))
                    {
                        Proceed = false;
                        return;
                    }

                    resourceModelsFolder = installationFolder;

                    projectMapping.ResourceFolder = installationFolder.Folder;
                    projectMapping.ResourceNamespace = installationFolder.Namespace;
                    projectMapping.ResourceProject = installationFolder.ProjectName;

                    codeService.SaveProjectMapping();
                }

                var form = new NewResourceDialog()
                {
                    ServiceProvider = ServiceProvider.GlobalProvider
                };

                var result = form.ShowDialog();

                if (form.DialogResult.HasValue && form.DialogResult.Value == true)
                {
                    if (ServiceProvider.GlobalProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) is IVsThreadedWaitDialogFactory dialogFactory)
                    {
                        dialogFactory.CreateInstance(out waitDialog);
                    }

                    if (waitDialog != null && waitDialog.StartWaitDialog("Microsoft Visual Studio",
                                                                 "Building resource model",
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 null,
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 0,
                                                                 false, true) == VSConstants.S_OK)
                    {
                        var standardEmitter = new Emitter();
                        var entityModel = form.EntityModel;

                        string model;

                        bool useRql = codeService.GetUseRql();

                        waitDialog.UpdateProgress($"Building resource model",
                                                  $"Building {replacementsDictionary["$safeitemname$"]}",
                                                  $"Building {replacementsDictionary["$safeitemname$"]}",
                                                  0,
                                                  0,
                                                  true,
                                                  out bool fpCanceled);

                        if (form.GenerateAsEnum)
                            model = standardEmitter.EmitResourceEnum(codeService,
                                                                     replacementsDictionary["$safeitemname$"],
                                                                     entityModel);
                        else
                            model = standardEmitter.EmitResourceModel(replacementsDictionary["$safeitemname$"],
                                                                      entityModel,
                                                                      useRql,
                                                                      replacementsDictionary);


                        var orchestrationNamespace = codeService.FindOrchestrationNamespace();

                        replacementsDictionary.Add("$model$", model);
                        replacementsDictionary.Add("$entitynamespace$", entityModel.Namespace);
                        replacementsDictionary.Add("$orchestrationnamespace$", orchestrationNamespace);
                        replacementsDictionary.Add("$userql$", useRql.ToString());

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
                {
                    waitDialog.EndWaitDialog(out _);
                }

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
