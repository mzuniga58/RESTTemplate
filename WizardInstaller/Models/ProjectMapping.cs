using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardInstaller.Template.Models
{
    public class ProjectMapping
    {
        public string EntityProject { get; set; }
        public string EntityNamespace { get; set; }
        public string EntityFolder { get; set; }
        public string ResourceProject { get; set; }
        public string ResourceNamespace { get; set; }
        public string ResourceFolder { get; set; }
        public string MappingProject { get; set; }
        public string MappingNamespace { get; set; }
        public string MappingFolder { get; set; }
        public string ExampleProject { get; set; }
        public string ExampleNamespace { get; set; }
        public string ExampleFolder { get; set; }
        public string ControllersProject { get; set; }
        public string ControllersNamespace { get; set; }
        public string ControllersFolder { get; set; }
        public string ExtensionsProject { get; set; }
        public string ExtensionsNamespace { get; set; }
        public string ExtensionsFolder { get; set; }

        public ProjectFolder GetEntityModelsFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = EntityFolder,
                Namespace = EntityNamespace,
                ProjectName = EntityProject
            };

            return pf;
        }

        public ProjectFolder GetResourceModelsFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = ResourceFolder,
                Namespace = ResourceNamespace,
                ProjectName = ResourceProject
            };

            return pf;
        }

        public ProjectFolder GetExamplesFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = ExampleFolder,
                Namespace = ExampleNamespace,
                ProjectName = ExampleProject
            };

            return pf;
        }

        public ProjectFolder GetMappingFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = MappingFolder,
                Namespace = MappingNamespace,
                ProjectName = MappingProject
            };

            return pf;
        }

        public ProjectFolder GetControllersFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = ControllersFolder,
                Namespace = ControllersNamespace,
                ProjectName = ControllersProject
            };

            return pf;
        }

        public ProjectFolder GetExtensionsFolder()
        {
            var pf = new ProjectFolder
            {
                Folder = ExtensionsFolder,
                Namespace = ExtensionsNamespace,
                ProjectName = ExtensionsProject
            };

            return pf;
        }
    }
}
