using System.Collections.Generic;

namespace RESTInstaller.Models
{
    /// <summary>
    /// Profile Map
    /// </summary>
    public class ProfileMap
    {
        /// <summary>
        /// Resource Class Name
        /// </summary>
        public string ResourceClassName { get; set; }

        /// <summary>
        /// EntityC Class Name
        /// </summary>
        public string EntityClassName { get; set; }

        /// <summary>
        /// Resource Profiles
        /// </summary>
        public List<ResourceProfile> ResourceProfiles { get; set; }

        /// <summary>
        /// Entity Profiles
        /// </summary>
        public List<EntityProfile> EntityProfiles { get; set; }
    }
}
