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
    public class EntityModelWizard : IWizard
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
                if (!codeService.IsChildOf(projectMapping.EntityFolder, installationFolder.Folder))
                {
                    if (!VsShellUtilities.PromptYesNo(
                                $"You are attempting to install an entity model into {codeService.GetRelativeFolder(installationFolder)}. Typically, entity models reside in {codeService.GetRelativeFolder(projectMapping.GetEntityModelsFolder())}.\r\n\r\nDo you wish to place the new entity model in this non-standard location?",
                                "Microsoft Visual Studio",
                                OLEMSGICON.OLEMSGICON_WARNING,
                                shell))
                    {
                        Proceed = false;
                        return;
                    }

                    projectMapping.EntityFolder = installationFolder.Folder;
                    projectMapping.EntityNamespace = installationFolder.Namespace;
                    projectMapping.EntityProject = installationFolder.ProjectName;

                    codeService.SaveProjectMapping();
                }

                //	Construct the form, and fill in all the prerequisite data
                var form = new NewEntityDialog
                {
                    ReplacementsDictionary = replacementsDictionary,
                    EntityModelsFolder = projectMapping.GetEntityModelsFolder(),
                    DefaultConnectionString = connectionString,
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
                                                                 "Building entity model",
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 null,
                                                                 $"Building {replacementsDictionary["$safeitemname$"]}",
                                                                 0,
                                                                 false, true) == VSConstants.S_OK)
                    {
                        bool fpCanceled = false;

                        //	Replace the default connection string in the appSettings.Local.json, so that the 
                        //	user doesn't have to do it. Note: this function only replaces the connection string
                        //	if the appSettings.Local.json contains the original placeholder connection string.
                        codeService.ConnectionString = $"{form.ConnectionString}Application Name={mDte.Solution.FullName}";

                        //	We will need these when we replace placeholders in the class
                        var className = replacementsDictionary["$safeitemname$"];
                        replacementsDictionary["$entityClass$"] = className;

                        var emitter = new Emitter();
                        string model = string.Empty;

                        if (form.EType == ElementType.Enum)
                        {
                            waitDialog.UpdateProgress($"Building entity model",
                                                     $"Building {replacementsDictionary["$safeitemname$"]}",
                                                     $"Building {replacementsDictionary["$safeitemname$"]}",
                                                     0,
                                                     0,
                                                     true,
                                                     out fpCanceled);

                            var enumDataType = form.DatabaseColumns[0].ModelDataType;

                            var columns = DBHelper.GenerateEnumColumns(form.ServerType, 
                                                                       form.DatabaseTable.Schema,
                                                                       form.DatabaseTable.Table,
                                                                       form.ConnectionString,
                                                                       form.DatabaseColumns);

                            model = emitter.EmitEntityEnum(replacementsDictionary["$safeitemname$"],
                                                           form.ServerType,
                                                           form.DatabaseTable.Schema,
                                                           form.DatabaseTable.Table,
                                                           enumDataType,
                                                           columns);

                        }
                        else
                        {
                            waitDialog.UpdateProgress($"Building entity model",
                                                    $"Building {replacementsDictionary["$safeitemname$"]}",
                                                    $"Building {replacementsDictionary["$safeitemname$"]}",
                                                    0,
                                                    0,
                                                    true,
                                                    out fpCanceled);

                            model = emitter.EmitEntityModel(replacementsDictionary["$safeitemname$"],
                                                                    form.DatabaseTable.Schema,
                                                                    form.DatabaseTable.Table,
                                                                    form.ServerType,
                                                                    form.DatabaseColumns.ToArray(),
                                                                    replacementsDictionary);
                        }

                        replacementsDictionary.Add("$entityModel$", model);
                        Proceed = true;

                        var entityMap = new EntityDBMap()
                        {
                            EntityClassName = replacementsDictionary["$safeitemname$"],
                            DBServerType = form.ServerType.ToString(),
                            EntitySchema = form.DatabaseTable.Schema,
                            EntityTable = form.DatabaseTable.Table,
                            ConnectionString = form.ConnectionString
                        };

                        codeService.AddEntityMap(entityMap);

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
