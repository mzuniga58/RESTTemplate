using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using REST.Template.Dialogs;
using WizardInstaller.Template.Models;
using Microsoft.Win32;

namespace WizardInstaller.Template.Wizards
{
	public class ProjectWizard : IWizard
	{
		private bool Proceed;
		private NewProjectDialog inputForm;
		private string framework;
		private string databaseTechnology;
		private string logPath;
		private string projectMapPath;

		// This method is called before opening any item that
		// has the OpenInEditor attribute.
		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
		}

		// This method is only called for item templates,
		// not for project templates.
		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		// This method is called after the project is created.
		public void RunFinished()
		{
		}

		public void RunStarted(object automationObject,
			Dictionary<string, string> replacementsDictionary,
			WizardRunKind runKind, object[] customParams)
		{
			string solutionDirectory = replacementsDictionary["$solutiondirectory$"];
			string destinationDirectory = replacementsDictionary["$destinationdirectory$"];

			try
			{
				Proceed = true;
				Random randomNumberGenerator = new Random(Convert.ToInt32(0x0000ffffL & DateTime.Now.ToFileTimeUtc()));

				string preferencesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REST");
				string preferencesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REST\\Preferences.json");

				if (!Directory.Exists(preferencesFolder))
					Directory.CreateDirectory(preferencesFolder);

				// Display a form to the user. The form collects
				// input for the custom message.
				using (inputForm = new NewProjectDialog())
				{
					if (File.Exists(preferencesFile))
					{
						var jsonText = File.ReadAllText(preferencesFile);
						var preferences = JsonSerializer.Deserialize<RESTPreferences>(jsonText);

						if (preferences != null)
						{
							inputForm.VendorTag = string.IsNullOrWhiteSpace(preferences.vendorMoniker) ? "" : preferences.vendorMoniker;
							inputForm.TeamName = string.IsNullOrWhiteSpace(preferences.authorName) ? "" : preferences.authorName;
							inputForm.TeamEmail = string.IsNullOrWhiteSpace(preferences.emailAddress) ? "" : preferences.emailAddress;
							inputForm.TeamUrl = string.IsNullOrWhiteSpace(preferences.webSite) ? "" : preferences.webSite;
						}
						else
                        {
							inputForm.TeamName = (string) Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion", "RegisteredOrganization", "YourName");
                        }
					}

					//	Show the form
					var result = inputForm.ShowModal();

					if (result.HasValue && result.Value == true)
					{
						//	Read data from the form
						framework = inputForm.Framework;
						databaseTechnology = inputForm.DatabaseTechnology;

						//	Save the preferences
						var preferences = new RESTPreferences
						{
							vendorMoniker = inputForm.VendorTag,
							authorName = inputForm.TeamName,
							emailAddress = inputForm.TeamEmail,
							webSite = inputForm.TeamUrl
						};

						if (string.IsNullOrWhiteSpace(preferences.emailAddress))
							preferences.emailAddress = "none";

						if (string.IsNullOrWhiteSpace(preferences.webSite))
							preferences.webSite = "none";

						replacementsDictionary.Add("$authorname$", preferences.authorName);
						replacementsDictionary.Add("$emailaddress$", preferences.emailAddress);
						replacementsDictionary.Add("$website$", preferences.webSite);
						replacementsDictionary.Add("$companymoniker$", preferences.vendorMoniker);

						var preferenceData = JsonSerializer.Serialize(preferences, new JsonSerializerOptions()
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
							WriteIndented = true,
							DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
						});

						using (var stream = new FileStream(preferencesFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
						{
							using (var writer = new StreamWriter(stream))
							{
								writer.Write(preferenceData);
								writer.Flush();
							}
						}

						//	Setup the project path file

						logPath = Path.Combine(replacementsDictionary["$destinationdirectory$"], "App_Data\\log-{Date}.json").Replace("\\", "\\\\");

						if (string.IsNullOrWhiteSpace(replacementsDictionary["$specifiedsolutionname$"]))
							projectMapPath = Path.Combine(replacementsDictionary["$destinationdirectory$"], ".rest");
						else
							projectMapPath = Path.Combine(replacementsDictionary["$solutiondirectory$"], ".rest");

						if (!Directory.Exists(projectMapPath))
						{
							DirectoryInfo dir = Directory.CreateDirectory(projectMapPath);
							dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
						}

						var projectMapping = new ProjectMapping
						{
							EntityProject = replacementsDictionary["$safeprojectname$"],
							EntityFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Models\\EntityModels"),
							EntityNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Models.EntityModels",

							ResourceProject = replacementsDictionary["$safeprojectname$"],
							ResourceFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Models\\ResourceModels"),
							ResourceNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Models.ResourceModels",

							MappingProject = replacementsDictionary["$safeprojectname$"],
							MappingFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Mapping"),
							MappingNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Mapping",

							ExampleProject = replacementsDictionary["$safeprojectname$"],
							ExampleFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Examples"),
							ExampleNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Examples",

							ControllersProject = replacementsDictionary["$safeprojectname$"],
							ControllersFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Controllers"),
							ControllersNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Controllers",

							ExtensionsProject = replacementsDictionary["$safeprojectname$"],
							ExtensionsFolder = Path.Combine(replacementsDictionary["$destinationdirectory$"], "Extensions"),
							ExtensionsNamespace = $"{replacementsDictionary["$safeprojectname$"]}.Extensions",
						};

						var projectMappingData = JsonSerializer.Serialize<ProjectMapping>(projectMapping, new JsonSerializerOptions()
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
							WriteIndented = true,
							DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
						});

						var projectMappingPath = Path.Combine(projectMapPath, "ProjectMap.json");

						using (var stream = new FileStream(projectMappingPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
						{
							using (var writer = new StreamWriter(stream))
							{
								writer.Write(projectMappingData);
								writer.Flush();
							}
						}

						var portNumber = randomNumberGenerator.Next(1024, 65534);
						var sslPortNumber = randomNumberGenerator.Next(1024, 65534);
						var useAuth = inputForm.AuthCheckbox.IsChecked ?? false;
						var useRql = inputForm.RQLCheckbox.IsChecked ?? false;

						// Add custom parameters.
						replacementsDictionary.Add("$framework$", framework);
						replacementsDictionary.Add("$useauth$", useAuth.ToString());
						replacementsDictionary.Add("$userql$", useRql.ToString());
						replacementsDictionary.Add("$databasetechnology$", databaseTechnology);
						replacementsDictionary.Add("$logPath$", logPath);
						replacementsDictionary.Add("$portNumber$", portNumber.ToString());
						replacementsDictionary.Add("$sslportNumber$", sslPortNumber.ToString());
						replacementsDictionary.Add("$repository$", GenerateRepository(replacementsDictionary));
					}
					else
					{
						Proceed = false;
						throw new Exception("User canceled the operation. Aborting project creation.");
					}
				}
			}
			catch (Exception)
			{
				if (Directory.Exists(destinationDirectory))
				{
					Directory.Delete(destinationDirectory, true);
				}

				if (Directory.Exists(solutionDirectory))
				{
					Directory.Delete(solutionDirectory, true);
				}

				Proceed = false;
				throw;
			}
		}

		// This method is only called for item templates,
		// not for project templates.
		public bool ShouldAddProjectItem(string filePath)
		{
			if ( databaseTechnology.Equals("None", StringComparison.OrdinalIgnoreCase) )
            {
				if ( filePath.Equals("IRepository.cs", StringComparison.OrdinalIgnoreCase) ||
					 filePath.Equals("Repository.cs", StringComparison.OrdinalIgnoreCase) )
                {
					return false;
                }
            }
			return Proceed;
		}

		public string GenerateRepository(Dictionary<string, string> replacementsDictionary)
		{
			var code = new StringBuilder();

			if (replacementsDictionary["$userql$"].Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				code.AppendLine("using Rql.Services;");
				code.AppendLine("using Rql.Models;");
				if (replacementsDictionary["$databasetechnology$"].Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
				{
					code.AppendLine("using Rql.SqlServer;");
				}
				else if (replacementsDictionary["$databasetechnology$"].Equals("postgresql", StringComparison.OrdinalIgnoreCase))
				{
					code.AppendLine("using Rql.Postgresql;");
					code.AppendLine("using Npgsql;");
				}
				else if (replacementsDictionary["$databasetechnology$"].Equals("mysql", StringComparison.OrdinalIgnoreCase))
				{
					code.AppendLine("using Rql.Mysql;");
					code.AppendLine("using MySql.Data;");
				}
			}

			code.AppendLine();
			code.AppendLine($"namespace {replacementsDictionary["$safeprojectname$"]}.Repositories");
			code.AppendLine("{");
			code.AppendLine("\t///\t<summary>");
			code.AppendLine("\t///\tThe repository");
			code.AppendLine("\t///\t</summary>");
			code.Append("\tpublic class Repository : ");

			if (replacementsDictionary["$userql$"].Equals("true", StringComparison.OrdinalIgnoreCase))
				code.Append("SqlServerRepository, ");

			code.AppendLine("IRepository");
			code.AppendLine("\t{");
			code.AppendLine("\t\tprivate readonly ILogger<Repository> _logger;");
			code.AppendLine();
			code.AppendLine("\t\t///\t<summary>");
			code.AppendLine("\t\t///\tInstantiates the Repository");
			code.AppendLine("\t\t///\t</summary>");
			code.AppendLine("\t\t///\t<param name=\"logger\">A generic interface for logging where the category name is derrived from the <see cref=\"Repository\"/> name.");
			code.AppendLine("\t\t///\tGenerally used to enable activation of a named <see cref=\"ILogger\"/> from dependency injection.</param>");

			if (replacementsDictionary["$userql$"].Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				code.AppendLine("\t\t///\t<param name=\"options\">Represents a set of key/value application configuration properties.</param>");
				code.AppendLine("\t\tpublic Repository(ILogger<Repository> logger, IRepositoryOptions options)");
				code.AppendLine("\t\t\t: base(logger, options)");
			}
			else
            {
				code.AppendLine("\t\tpublic Repository(ILogger<Repository> logger)");
			}

			code.AppendLine("\t\t{");
			code.AppendLine("\t\t\t_logger = logger;");
			code.AppendLine("\t\t}");
			code.AppendLine("\t}");
			code.AppendLine("}");

			return code.ToString();
		}
	}
}
