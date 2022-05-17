using RESTInstaller.Models;
using RESTInstaller.Services;
using RESTInstaller.Dialogs;
using RESTInstaller.Wizards;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Constants = EnvDTE.Constants;
using Task = System.Threading.Tasks.Task;

namespace RESTInstaller.Extensions
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MenuExtensions
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int EditMappingId = 0x0101;
		public const int AddEntityModelId = 0x102;
		public const int AddResourceModelId = 0x103;
		public const int AddControllerId = 0x104;
		public const int AddProfileId = 0x0106;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("2badb8a1-54a6-4ad8-8f80-4c67668ee954");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

		private static Events2 _events2 = null;
		private static SolutionEvents _solutionEvents = null;
		private static ProjectItemsEvents _projectItemsEvents = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuExtensions"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MenuExtensions(AsyncPackage package, OleMenuCommandService commandService)
        {
			this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var editMappingCommandId = new CommandID(CommandSet, EditMappingId);
            OleMenuCommand EditMappingMenu = new OleMenuCommand(new EventHandler(OnResetMapping), editMappingCommandId);
            EditMappingMenu.BeforeQueryStatus += new EventHandler(OnBeforeResetMapping);
            commandService.AddCommand(EditMappingMenu);

			var addEntityModelCommandId = new CommandID(CommandSet, AddEntityModelId);
			OleMenuCommand AddEntityModelMenu = new OleMenuCommand(new EventHandler(OnAddEntityModel), addEntityModelCommandId);
			AddEntityModelMenu.BeforeQueryStatus += new EventHandler(OnBeforeAddEntityModel);
			commandService.AddCommand(AddEntityModelMenu);

			var addResourceModelCommandId = new CommandID(CommandSet, AddResourceModelId);
			OleMenuCommand AddResourceModelMenu = new OleMenuCommand(new EventHandler(OnAddResourceModel), addResourceModelCommandId);
			AddResourceModelMenu.BeforeQueryStatus += new EventHandler(OnBeforeAddResourceModel);
			commandService.AddCommand(AddResourceModelMenu);

			var addControllerCommandId = new CommandID(CommandSet, AddControllerId);
			OleMenuCommand AddControllerMenu = new OleMenuCommand(new EventHandler(OnAddController), addControllerCommandId);
			AddControllerMenu.BeforeQueryStatus += new EventHandler(OnBeforeAddController);
			commandService.AddCommand(AddControllerMenu);

			var addProfileMapCommandId = new CommandID(CommandSet, AddProfileId);
			OleMenuCommand AddProfileMapMenu = new OleMenuCommand(new EventHandler(OnAddProfileMap), addProfileMapCommandId);
			AddProfileMapMenu.BeforeQueryStatus += new EventHandler(OnBeforeAddProfileMap);
			commandService.AddCommand(AddProfileMapMenu);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static MenuExtensions Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MenuExtensions's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var mDte = (DTE2)Package.GetGlobalService(typeof(SDTE));

			_events2 = (Events2)mDte.Events;
			_solutionEvents = _events2.SolutionEvents;
			_projectItemsEvents = _events2.ProjectItemsEvents;

			OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MenuExtensions(package, commandService);

			if ( mDte.Solution.IsOpen)
            {
				Instance.OnSolutionOpened();
			}

			_solutionEvents.Opened += Instance.OnSolutionOpened;
			_projectItemsEvents.ItemRemoved += Instance.OnProjectItemRemoved;
            _projectItemsEvents.ItemAdded += OnProjectItemAdded;
		}

        #region Project Item action handlers
        private static void OnProjectItemAdded(ProjectItem ProjectItem)
        {
			var codeService = ServiceFactory.GetService<ICodeService>();
			codeService.OnProjectItemAdded(ProjectItem);
		}

		private void OnProjectItemRemoved(ProjectItem ProjectItem)
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			codeService.OnProjectItemRemoved(ProjectItem);
		}

		private void OnSolutionOpened()
		{
			var codeService = ServiceFactory.GetService<ICodeService>();
			codeService.OnSolutionOpened();
		}
        #endregion

        #region Profile Map Operations
        private void OnBeforeAddProfileMap(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;


			if (selectedItems.Length > 1 || selectedItems.Length == 0)
			{
				var myCommand = sender as OleMenuCommand;
				myCommand.Visible = false;
			}
			else
			{
				ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

				if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
					projectItem.Kind == Constants.vsProjectItemKindVirtualFolder)
				{
					var codeService = ServiceFactory.GetService<ICodeService>();
					var projectMap = codeService.LoadProjectMapping();

					var candidateFolder = projectMap.MappingFolder;
					var projectItemPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
					{
						ThreadHelper.ThrowIfNotOnUIThread();
						return p.Name.Equals("FullPath");
					})?.Value.ToString().Trim('\\');

					if (projectItemPath.Equals(candidateFolder))
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = true;
					}
					else
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = false;
					}
				}
				else
				{
					var myCommand = sender as OleMenuCommand;
					myCommand.Visible = false;
				}
			}
		}

		private void OnAddProfileMap(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var codeService = ServiceFactory.GetService<ICodeService>();
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;
			var projectMapping = codeService.LoadProjectMapping();

			ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

			var projectFolderNamespace = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("DefaultNamespace", StringComparison.OrdinalIgnoreCase);
			});

			var projectFolderPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("FullPath", StringComparison.OrdinalIgnoreCase);
			});


			var dialog = new GetClassNameDialog("Mapping Generator", "ResourceProfile.cs");
			var result = dialog.ShowDialog();

			if (result.HasValue && result.Value == true)
			{
				var replacementsDictionary = new Dictionary<string, string>();

				for (int i = 0; i < 10; i++)
				{
					replacementsDictionary.Add($"$guid{i + 1}$", Guid.NewGuid().ToString());
				}

				var className = dialog.ClassName;
				if (className.EndsWith(".cs"))
					className = className.Substring(0, className.Length - 3);

				replacementsDictionary.Add("$time$", DateTime.Now.ToString());
				replacementsDictionary.Add("$year$", DateTime.Now.Year.ToString());
				replacementsDictionary.Add("$username$", Environment.UserName);
				replacementsDictionary.Add("$userdomain$", Environment.UserDomainName);
				replacementsDictionary.Add("$machinename$", Environment.MachineName);
				replacementsDictionary.Add("$clrversion$", GetRunningFrameworkVersion());
				replacementsDictionary.Add("$registeredorganization$", GetOrganization());
				replacementsDictionary.Add("$runsilent$", "True");
				replacementsDictionary.Add("$solutiondirectory$", Path.GetDirectoryName(mDte.Solution.FullName));
				replacementsDictionary.Add("$rootname$", $"{className}.cs");
				replacementsDictionary.Add("$targetframeworkversion$", "6.0");
				replacementsDictionary.Add("$targetframeworkidentifier", ".NETCoreApp");
				replacementsDictionary.Add("$safeitemname$", codeService.NormalizeClassName(codeService.CorrectForReservedNames(className)));
				replacementsDictionary.Add("$rootnamespace$", projectFolderNamespace.Value.ToString());

				var wizard = new ProfileWizard();

				wizard.RunStarted(mDte, replacementsDictionary, Microsoft.VisualStudio.TemplateWizard.WizardRunKind.AsNewItem, null);

				var projectItemPath = Path.Combine(projectFolderPath.Value.ToString(), replacementsDictionary["$rootname$"]);

				if (wizard.ShouldAddProjectItem("Mapping"))
				{
					var theFile = new StringBuilder();

					theFile.AppendLine("using System;");
					theFile.AppendLine("using System.Linq;");
					theFile.AppendLine("using Microsoft.Extensions.Configuration;");

					if (replacementsDictionary.ContainsKey("$barray$"))
						if (replacementsDictionary["$barray$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Collections;");

					theFile.AppendLine("using System.Collections.Generic;");

					if (replacementsDictionary.ContainsKey("$image$"))
						if (replacementsDictionary["$image$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Drawing;");

					if (replacementsDictionary.ContainsKey("$net$"))
						if (replacementsDictionary["$net$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net;");

					if (replacementsDictionary.ContainsKey("$netinfo$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net.NetworkInformation;");

					if (replacementsDictionary.ContainsKey("$annotations$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.ComponentModel.DataAnnotations;");

					if (replacementsDictionary.ContainsKey("$npgsqltypes$"))
						if (replacementsDictionary["$npgsqltypes$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using NpgsqlTypes;");

					theFile.AppendLine($"using {projectMapping.EntityNamespace};");
					theFile.AppendLine($"using {projectMapping.ResourceNamespace};");

					theFile.AppendLine("using AutoMapper;");
					theFile.AppendLine();
					theFile.AppendLine($"namespace {projectMapping.MappingNamespace}");
					theFile.AppendLine("{");

					theFile.Append(replacementsDictionary["$model$"]);
					theFile.AppendLine("}");

					File.WriteAllText(projectItemPath, theFile.ToString());

					var parentProject = codeService.GetProjectFromFolder(projectFolderPath.Value.ToString());
					ProjectItem mappingItem;

					if (parentProject.GetType() == typeof(Project))
						mappingItem = ((Project)parentProject).ProjectItems.AddFromFile(projectItemPath);
					else
						mappingItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(projectItemPath);

					wizard.ProjectItemFinishedGenerating(mappingItem);
					wizard.BeforeOpeningFile(mappingItem);

					var window = mappingItem.Open();
					window.Activate();

					wizard.RunFinished();
				}
			}
		}
        #endregion

        #region Controller Operations
		private void OnBeforeAddController(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;


			if (selectedItems.Length > 1 || selectedItems.Length == 0)
			{
				var myCommand = sender as OleMenuCommand;
				myCommand.Visible = false;
			}
			else
			{
				ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

				if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
					projectItem.Kind == Constants.vsProjectItemKindVirtualFolder)
				{
					var codeService = ServiceFactory.GetService<ICodeService>();
					var projectMap = codeService.LoadProjectMapping();

					var controllersFolder = projectMap.ControllersFolder;
					var projectItemPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
					{
						ThreadHelper.ThrowIfNotOnUIThread();
						return p.Name.Equals("FullPath");
					})?.Value.ToString().Trim('\\');

					if (projectItemPath.Equals(controllersFolder))
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = true;
					}
					else
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = false;
					}
				}
				else
				{
					var myCommand = sender as OleMenuCommand;
					myCommand.Visible = false;
				}
			}
		}

		private void OnAddController(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var codeService = ServiceFactory.GetService<ICodeService>();
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;
			var projectMapping = codeService.LoadProjectMapping();

			ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

			var projectFolderNamespace = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("DefaultNamespace", StringComparison.OrdinalIgnoreCase);
			});

			var projectFolderPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("FullPath", StringComparison.OrdinalIgnoreCase);
			});


			var dialog = new GetClassNameDialog("Controller Generator", "ResourceController.cs");
			var result = dialog.ShowDialog();

			if (result.HasValue && result.Value == true)
			{
				var replacementsDictionary = new Dictionary<string, string>();

				for (int i = 0; i < 10; i++)
				{
					replacementsDictionary.Add($"$guid{i + 1}$", Guid.NewGuid().ToString());
				}

				var className = dialog.ClassName;
				if (className.EndsWith(".cs"))
					className = className.Substring(0, className.Length - 3);

				replacementsDictionary.Add("$time$", DateTime.Now.ToString());
				replacementsDictionary.Add("$year$", DateTime.Now.Year.ToString());
				replacementsDictionary.Add("$username$", Environment.UserName);
				replacementsDictionary.Add("$userdomain$", Environment.UserDomainName);
				replacementsDictionary.Add("$machinename$", Environment.MachineName);
				replacementsDictionary.Add("$clrversion$", GetRunningFrameworkVersion());
				replacementsDictionary.Add("$registeredorganization$", GetOrganization());
				replacementsDictionary.Add("$runsilent$", "True");
				replacementsDictionary.Add("$solutiondirectory$", Path.GetDirectoryName(mDte.Solution.FullName));
				replacementsDictionary.Add("$rootname$", $"{className}.cs");
				replacementsDictionary.Add("$targetframeworkversion$", "6.0");
				replacementsDictionary.Add("$targetframeworkidentifier", ".NETCoreApp");
				replacementsDictionary.Add("$safeitemname$", codeService.NormalizeClassName(codeService.CorrectForReservedNames(className)));
				replacementsDictionary.Add("$rootnamespace$", projectFolderNamespace.Value.ToString());

				var wizard = new ControllerWizard();

				wizard.RunStarted(mDte, replacementsDictionary, Microsoft.VisualStudio.TemplateWizard.WizardRunKind.AsNewItem, null);

				var projectItemPath = Path.Combine(projectFolderPath.Value.ToString(), replacementsDictionary["$rootname$"]);

				if (wizard.ShouldAddProjectItem("Controllers"))
				{
					var theFile = new StringBuilder();

					bool useRql = codeService.GetUseRql();
					theFile.AppendLine("using System;");
					theFile.AppendLine("using System.Collections.Generic;");
					theFile.AppendLine("using System.Linq;");
					theFile.AppendLine("using System.Net;");
					theFile.AppendLine("using System.Net.Mime;");
					theFile.AppendLine("using System.Security.Claims;");
					theFile.AppendLine("using System.Text.Json;");
					theFile.AppendLine("using System.Threading.Tasks;");

					if (replacementsDictionary.ContainsKey("$barray$"))
						if (replacementsDictionary["$barray$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Collections;");

					if (replacementsDictionary.ContainsKey("$image$"))
						if (replacementsDictionary["$image$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Drawing;");

					if (replacementsDictionary.ContainsKey("$net$"))
						if (replacementsDictionary["$net$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net;");

					if (replacementsDictionary.ContainsKey("$netinfo$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net.NetworkInformation;");

					if (replacementsDictionary.ContainsKey("$annotations$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.ComponentModel.DataAnnotations;");

					if (replacementsDictionary.ContainsKey("$npgsqltypes$"))
						if (replacementsDictionary["$npgsqltypes$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using NpgsqlTypes;");

					if (codeService.Policies != null && codeService.Policies.Count > 0)
						theFile.AppendLine("using Microsoft.AspNetCore.Authorization;");

					theFile.AppendLine("using Microsoft.AspNetCore.Mvc;");
					theFile.AppendLine("using Microsoft.AspNetCore.Mvc.ModelBinding;");
					theFile.AppendLine("using Microsoft.Extensions.Logging;");
					theFile.AppendLine("using Microsoft.Extensions.DependencyInjection;");

					var orchestrationNamespace = codeService.FindOrchestrationNamespace();

					theFile.AppendLine($"using {projectMapping.ResourceNamespace};");
					theFile.AppendLine($"using {orchestrationNamespace};");
					theFile.AppendLine("using Tense;");

					if ( useRql )
						theFile.AppendLine("using Tense.Rql;");
					theFile.AppendLine("using Serilog.Context;");
					theFile.AppendLine("using Swashbuckle.AspNetCore.Annotations;");
					theFile.AppendLine();
					theFile.AppendLine($"namespace {projectMapping.ControllersNamespace}");
					theFile.AppendLine("{");

					theFile.Append(replacementsDictionary["$model$"]);
					theFile.AppendLine("}");

					File.WriteAllText(projectItemPath, theFile.ToString());

					var parentProject = codeService.GetProjectFromFolder(projectFolderPath.Value.ToString());
					ProjectItem validationItem;

					if (parentProject.GetType() == typeof(Project))
						validationItem = ((Project)parentProject).ProjectItems.AddFromFile(projectItemPath);
					else
						validationItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(projectItemPath);

					wizard.ProjectItemFinishedGenerating(validationItem);
					wizard.BeforeOpeningFile(validationItem);

					var window = validationItem.Open();
					window.Activate();

					wizard.RunFinished();
				}
			}
		}
        #endregion

        #region Resource Model Operations
        private void OnBeforeAddResourceModel(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;


			if (selectedItems.Length > 1 || selectedItems.Length == 0)
			{
				var myCommand = sender as OleMenuCommand;
				myCommand.Visible = false;
			}
			else
			{
				ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

				if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
					projectItem.Kind == Constants.vsProjectItemKindVirtualFolder)
				{
					var codeService = ServiceFactory.GetService<ICodeService>();
					var projectMap = codeService.LoadProjectMapping();

					var resourceModelsFolder = projectMap.ResourceFolder;
					var projectItemPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
					{
						ThreadHelper.ThrowIfNotOnUIThread();
						return p.Name.Equals("FullPath");
					})?.Value.ToString().Trim('\\');

					if (projectItemPath.Equals(resourceModelsFolder))
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = true;
					}
					else
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = false;
					}
				}
				else
				{
					var myCommand = sender as OleMenuCommand;
					myCommand.Visible = false;
				}
			}
		}

		private void OnAddResourceModel(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var codeService = ServiceFactory.GetService<ICodeService>();
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;
			var projectMapping = codeService.LoadProjectMapping();

			ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

			var projectFolderNamespace = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("DefaultNamespace", StringComparison.OrdinalIgnoreCase);
			});

			var projectFolderPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("FullPath", StringComparison.OrdinalIgnoreCase);
			});


			var dialog = new GetClassNameDialog("Resource Model Generator", "Resource.cs");
			var result = dialog.ShowDialog();

			if (result.HasValue && result.Value == true)
			{
				var replacementsDictionary = new Dictionary<string, string>();

				for (int i = 0; i < 10; i++)
				{
					replacementsDictionary.Add($"$guid{i + 1}$", Guid.NewGuid().ToString());
				}

				var className = dialog.ClassName;
				if (className.EndsWith(".cs"))
					className = className.Substring(0, className.Length - 3);

				var orchestrationNamespace = codeService.FindOrchestrationNamespace();

				replacementsDictionary.Add("$time$", DateTime.Now.ToString());
				replacementsDictionary.Add("$year$", DateTime.Now.Year.ToString());
				replacementsDictionary.Add("$username$", Environment.UserName);
				replacementsDictionary.Add("$userdomain$", Environment.UserDomainName);
				replacementsDictionary.Add("$machinename$", Environment.MachineName);
				replacementsDictionary.Add("$clrversion$", GetRunningFrameworkVersion());
				replacementsDictionary.Add("$registeredorganization$", GetOrganization());
				replacementsDictionary.Add("$runsilent$", "True");
				replacementsDictionary.Add("$solutiondirectory$", Path.GetDirectoryName(mDte.Solution.FullName));
				replacementsDictionary.Add("$rootname$", $"{className}.cs");
				replacementsDictionary.Add("$targetframeworkversion$", "6.0");
				replacementsDictionary.Add("$targetframeworkidentifier", ".NETCoreApp");
				replacementsDictionary.Add("$safeitemname$", codeService.NormalizeClassName(codeService.CorrectForReservedNames(className)));
				replacementsDictionary.Add("$rootnamespace$", projectFolderNamespace.Value.ToString());

				ResourceModelWizard wizard = new ResourceModelWizard();

				wizard.RunStarted(mDte, replacementsDictionary, Microsoft.VisualStudio.TemplateWizard.WizardRunKind.AsNewItem, null);

				var projectItemPath = Path.Combine(projectFolderPath.Value.ToString(), replacementsDictionary["$rootname$"]);

				if (wizard.ShouldAddProjectItem(projectItemPath))
				{
					var theFile = new StringBuilder();

					theFile.AppendLine("using System;");
					theFile.AppendLine("using Tense;");

					if (replacementsDictionary.ContainsKey("$userql$"))
						if (replacementsDictionary["$userql$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using Tense.Rql;");

					theFile.AppendLine("using Microsoft.AspNetCore.Mvc.ModelBinding;");
					theFile.AppendLine($"using {orchestrationNamespace};");

					if (replacementsDictionary.ContainsKey("$resourcebarray$"))
						if (replacementsDictionary["$resourcebarray$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Collections;");

					theFile.AppendLine("using System.Collections.Generic;");
					theFile.AppendLine("using System.ComponentModel.DataAnnotations;");

					if (replacementsDictionary.ContainsKey("$image$"))
						if (replacementsDictionary["$image$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Drawing;");

					if (replacementsDictionary.ContainsKey("$net$"))
						if (replacementsDictionary["$net$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net;");

					if (replacementsDictionary.ContainsKey("$netinfo$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net.NetworkInformation;");

					if (replacementsDictionary.ContainsKey("$npgsqltypes$"))
						if (replacementsDictionary["$npgsqltypes$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using NpgsqlTypes;");
					theFile.AppendLine($"using {projectMapping.EntityNamespace};");


					theFile.AppendLine();
					theFile.AppendLine($"namespace {projectFolderNamespace.Value}");
					theFile.AppendLine("{");
					theFile.Append(replacementsDictionary["$model$"]);
					theFile.AppendLine("}");

					File.WriteAllText(projectItemPath, theFile.ToString());

					var parentProject = codeService.GetProjectFromFolder(projectFolderPath.Value.ToString());
					ProjectItem entityItem;

					if (parentProject.GetType() == typeof(Project))
						entityItem = ((Project)parentProject).ProjectItems.AddFromFile(projectItemPath);
					else
						entityItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(projectItemPath);

					wizard.ProjectItemFinishedGenerating(entityItem);
					wizard.BeforeOpeningFile(entityItem);

					var window = entityItem.Open();
					window.Activate();

					wizard.RunFinished();
				}
			}
		}
        #endregion

        #region Entity Model Operations
        private void OnBeforeAddEntityModel(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;

			if (selectedItems.Length > 1 || selectedItems.Length == 0)
			{
				var myCommand = sender as OleMenuCommand;
				myCommand.Visible = false;
			}
			else
			{
				ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

				if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFolder ||
					projectItem.Kind == Constants.vsProjectItemKindVirtualFolder)
				{
					var codeService = ServiceFactory.GetService<ICodeService>();
					var projectMap = codeService.LoadProjectMapping();

					var entityModelsFolder = projectMap.EntityFolder;
					var projectItemPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        return p.Name.Equals("FullPath");
                    })?.Value.ToString().Trim('\\');

					if (projectItemPath.Equals(entityModelsFolder, StringComparison.OrdinalIgnoreCase))
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = true;
					}
					else
					{
						var myCommand = sender as OleMenuCommand;
						myCommand.Visible = false;
					}
				}
				else
				{
					var myCommand = sender as OleMenuCommand;
					myCommand.Visible = false;
				}
			}
		}

		private void OnAddEntityModel(object sender, EventArgs e)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			var codeService = ServiceFactory.GetService<ICodeService>();
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;

			ProjectItem projectItem = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

			var projectFolderNamespace = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return p.Name.Equals("DefaultNamespace", StringComparison.OrdinalIgnoreCase);
            });

			var projectFolderPath = projectItem.Properties.OfType<Property>().FirstOrDefault(p =>
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return p.Name.Equals("FullPath", StringComparison.OrdinalIgnoreCase);
			});


			var dialog = new GetClassNameDialog("Entity Model Generator", "EResource.cs");
			var result = dialog.ShowDialog();

			if (result.HasValue && result.Value == true)
			{
				var replacementsDictionary = new Dictionary<string, string>();

				for (int i = 0; i < 10; i++)
				{
					replacementsDictionary.Add($"$guid{i + 1}$", Guid.NewGuid().ToString());
				}

				var className = dialog.ClassName;
				if (className.EndsWith(".cs"))
					className = className.Substring(0, className.Length - 3);

				replacementsDictionary.Add("$time$", DateTime.Now.ToString());
				replacementsDictionary.Add("$year$", DateTime.Now.Year.ToString());
				replacementsDictionary.Add("$username$", Environment.UserName);
				replacementsDictionary.Add("$userdomain$", Environment.UserDomainName);
				replacementsDictionary.Add("$machinename$", Environment.MachineName);
				replacementsDictionary.Add("$clrversion$", GetRunningFrameworkVersion());
				replacementsDictionary.Add("$registeredorganization$", GetOrganization());
				replacementsDictionary.Add("$runsilent$", "True");
				replacementsDictionary.Add("$solutiondirectory$", Path.GetDirectoryName(mDte.Solution.FullName));
				replacementsDictionary.Add("$rootname$", $"{className}.cs");
				replacementsDictionary.Add("$targetframeworkversion$", "6.0");
				replacementsDictionary.Add("$targetframeworkidentifier", ".NETCoreApp");
				replacementsDictionary.Add("$safeitemname$", codeService.NormalizeClassName(codeService.CorrectForReservedNames(className)));
				replacementsDictionary.Add("$rootnamespace$", projectFolderNamespace.Value.ToString());

				EntityModelWizard wizard = new EntityModelWizard();

				wizard.RunStarted(mDte, replacementsDictionary, Microsoft.VisualStudio.TemplateWizard.WizardRunKind.AsNewItem, null);

				var projectItemPath = Path.Combine(projectFolderPath.Value.ToString(), replacementsDictionary["$rootname$"]);

				if ( wizard.ShouldAddProjectItem(projectItemPath))
                {
					var theFile = new StringBuilder();

					theFile.AppendLine("using System;");

					if (replacementsDictionary.ContainsKey("$barray$"))
						if (replacementsDictionary["$barray$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Collections;");

					theFile.AppendLine("using System.Collections.Generic;");

					if (replacementsDictionary.ContainsKey("$image$"))
						if (replacementsDictionary["$image$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Drawing;");

					if (replacementsDictionary.ContainsKey("$net$"))
						if (replacementsDictionary["$net$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net;");

					if (replacementsDictionary.ContainsKey("$netinfo$"))
						if (replacementsDictionary["$netinfo$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using System.Net.NetworkInformation;");

					if (replacementsDictionary.ContainsKey("$npgsqltypes$"))
						if (replacementsDictionary["$npgsqltypes$"].Equals("true", StringComparison.OrdinalIgnoreCase))
							theFile.AppendLine("using NpgsqlTypes;");


					theFile.AppendLine("using Tense;");
					theFile.AppendLine();
					theFile.AppendLine($"namespace {projectFolderNamespace.Value}");
					theFile.AppendLine("{");

					theFile.Append(replacementsDictionary["$entityModel$"]);
					theFile.AppendLine("}");

					File.WriteAllText(projectItemPath, theFile.ToString());

					var parentProject = codeService.GetProjectFromFolder(projectFolderPath.Value.ToString());
					ProjectItem entityItem;

					if (parentProject.GetType() == typeof(Project))
						entityItem = ((Project)parentProject).ProjectItems.AddFromFile(projectItemPath);
					else
						entityItem = ((ProjectItem)parentProject).ProjectItems.AddFromFile(projectItemPath);

					wizard.ProjectItemFinishedGenerating(entityItem);
					wizard.BeforeOpeningFile(entityItem);

					var window = entityItem.Open();
					window.Activate();

					wizard.RunFinished();
				}
			}
		}
        #endregion

        #region Miscellaneous Operations
		private void OnBeforeResetMapping(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var myCommand = sender as OleMenuCommand;
			var mDte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
			object[] selectedItems = (object[])mDte.ToolWindows.SolutionExplorer.SelectedItems;

			if (selectedItems.Length > 1)
			{
				myCommand.Visible = false;
			}
			else
			{
				ProjectItem item = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;

				if (item.FileCodeModel != null && item.FileCodeModel.CodeElements != null)
				{
					var theNamespace = item.FileCodeModel.CodeElements.OfType<CodeNamespace>().First();

					if (theNamespace != null)
					{
						var theClass = theNamespace.Children.OfType<CodeClass2>().First();

						if (theClass != null)
						{
							var theBaseClass = theClass.Bases.OfType<CodeClass2>().FirstOrDefault(a => a.Name.Equals("Profile"));

							if (theBaseClass != null)
							{
								myCommand.Enabled = true;
								myCommand.Visible = true;
							}
							else
							{
								myCommand.Enabled = false;
								myCommand.Visible = false;
							}
						}
						else
						{
							myCommand.Enabled = false;
							myCommand.Visible = false;
						}
					}
					else
					{
						myCommand.Enabled = false;
						myCommand.Visible = false;
					}
				}
			}
		}

		private void OnResetMapping(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			IVsThreadedWaitDialog2 waitDialog = null;

			if (ServiceProvider.GlobalProvider.GetService(typeof(SVsThreadedWaitDialogFactory)) is IVsThreadedWaitDialogFactory dialogFactory)
			{
				dialogFactory.CreateInstance(out waitDialog);
			}

			if (waitDialog != null && waitDialog.StartWaitDialog("Microsoft Visual Studio",
														 "Building controller",
														 $"Resetting mapping",
														 null,
														 $"Resetting mapping",
														 0,
														 false, true) == VSConstants.S_OK)
			{
				var codeService = ServiceFactory.GetService<ICodeService>();

				var dte2 = package.GetService<SDTE, DTE2>() as DTE2;
				object[] selectedItems = (object[])dte2.ToolWindows.SolutionExplorer.SelectedItems;

				if (selectedItems.Length == 1)
				{
					ProjectItem item = ((UIHierarchyItem)selectedItems[0]).Object as ProjectItem;
					var theNamespace = item.FileCodeModel.CodeElements.OfType<CodeNamespace>().First();

					if (theNamespace != null)
					{
						var theClass = theNamespace.Children.OfType<CodeClass2>().First();

						if (theClass != null)
						{
							var constructor = theClass.Children
													  .OfType<CodeFunction2>()
													  .First(c => c.FunctionKind == vsCMFunction.vsCMFunctionConstructor);

							if (constructor != null)
							{

								EditPoint2 editPoint = (EditPoint2)constructor.StartPoint.CreateEditPoint();
								bool foundit = editPoint.FindPattern("CreateMap<");
								foundit = foundit && editPoint.LessThan(constructor.EndPoint);

								if (foundit)
								{
									editPoint.StartOfLine();
									EditPoint2 start = (EditPoint2)editPoint.CreateEditPoint();
									editPoint.EndOfLine();
									EditPoint2 end = (EditPoint2)editPoint.CreateEditPoint();
									var text = start.GetText(end);

									var match = Regex.Match(text, "[ \t]*CreateMap\\<(?<resource>[a-zA-Z0-9_]+)[ \t]*\\,[ \t]*(?<entity>[a-zA-Z0-9_]+)\\>[ \t]*\\([ \t]*\\)");


									if (match.Success)
									{
										var resourceModel = codeService.GetResourceClass(match.Groups["resource"].Value);
										var ProfileMap = codeService.GenerateProfileMap(resourceModel);

										editPoint = (EditPoint2)constructor.StartPoint.CreateEditPoint();
										foundit = editPoint.FindPattern($"CreateMap<{resourceModel.ClassName}");
										foundit = foundit && editPoint.LessThan(constructor.EndPoint);

										if (foundit)
										{
											editPoint.LineDown();

											while (!IsLineEmpty(editPoint))
											{
												DeleteLine(editPoint);
											}

											bool first = true;

											foreach (var rmap in ProfileMap.EntityProfiles)
											{
												if (first)
													first = false;
												else
													editPoint.InsertNewLine();

												editPoint.StartOfLine();
												editPoint.Indent(null, 4);
												editPoint.Insert(".ForMember(destination => destination.");
												editPoint.Insert(rmap.EntityColumnName);
												editPoint.Insert(", opts => opts.MapFrom(source => ");
												editPoint.Insert(rmap.MapFunction);
												editPoint.Insert("))");
											}

											editPoint.Insert(";");
											editPoint.InsertNewLine();
										}

										editPoint = (EditPoint2)constructor.StartPoint.CreateEditPoint();
										foundit = editPoint.FindPattern($"CreateMap<{resourceModel.Entity.ClassName}");
										foundit = foundit && editPoint.LessThan(constructor.EndPoint);

										if (foundit)
										{
											editPoint.LineDown();

											while (!IsLineEmpty(editPoint))
											{
												DeleteLine(editPoint);
											}

											bool first = true;

											foreach (var rmap in ProfileMap.ResourceProfiles)
											{
												if (first)
													first = false;
												else
													editPoint.InsertNewLine();

												editPoint.StartOfLine();
												editPoint.Indent(null, 4);
												editPoint.Insert(".ForMember(destination => destination.");
												editPoint.Insert(rmap.ResourceColumnName);
												editPoint.Insert(", opts => opts.MapFrom(source => ");
												editPoint.Insert(rmap.MapFunction);
												editPoint.Insert("))");
											}

											editPoint.Insert(";");
											editPoint.InsertNewLine();
										}
									}
								}
							}
						}
					}
				}

				waitDialog.EndWaitDialog(out int usercancel);
			}
		}
        #endregion

		#region Helper Functions
		public string GetRunningFrameworkVersion()
		{
			string netVer = Environment.Version.ToString();
			Assembly assObj = typeof(Object).GetTypeInfo().Assembly;
			if (assObj != null)
			{
				AssemblyFileVersionAttribute attr;
				attr = (AssemblyFileVersionAttribute)assObj.GetCustomAttribute(typeof(AssemblyFileVersionAttribute));
				if (attr != null)
				{
					netVer = attr.Version;
				}
			}
			return netVer;
		}

		public string GetOrganization()
        {
			string organization;

			var regSoftware = Registry.LocalMachine.OpenSubKey("SOFTWARE");
			var regMicrosoft = regSoftware.OpenSubKey("Microsoft");
			var regWindowsNT = regMicrosoft.OpenSubKey("Windows NT");
			var regCurrentVersion = regWindowsNT.OpenSubKey("CurrentVersion");

			organization = regCurrentVersion.GetValue("RegisteredOrganization").ToString();

			regCurrentVersion.Close();
			regWindowsNT.Close();
			regMicrosoft.Close();
			regSoftware.Close();

			return organization;
		}

		private static bool IsLineEmpty(EditPoint2 editPoint)
		{
			EditPoint2 startOfLine = (EditPoint2)editPoint.CreateEditPoint();
			startOfLine.StartOfLine();
			EditPoint2 eol = (EditPoint2)startOfLine.CreateEditPoint();
			eol.EndOfLine();
			return string.IsNullOrWhiteSpace(editPoint.GetText(eol));
		}

		private static void DeleteLine(EditPoint2 editPoint)
		{
			EditPoint2 startOfLine = (EditPoint2)editPoint.CreateEditPoint();
			startOfLine.StartOfLine();
			EditPoint2 eol = (EditPoint2)startOfLine.CreateEditPoint();
			eol.LineDown();
			eol.StartOfLine();
			startOfLine.Delete(eol);
		}
		#endregion
	}
}
