using System.Collections.Generic;

namespace WizardInstaller.Template.Models
{
    public class ResourceProfile
    {
        public string ResourceColumnName { get; set; }
        public string MapFunction { get; set; }
        public string[] EntityColumnNames { get; set; }
        public bool IsDefined { get; set; }

        public List<ResourceProfile> ChildProfiles { get; set; }
    }
}
