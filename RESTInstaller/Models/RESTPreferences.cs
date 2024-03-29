﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTInstaller.Models
{
    /// <summary>
    /// REST Preferences (used to pre-populate project form with data not likely to change)
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    internal class RESTPreferences
    {
        /// <summary>
        /// The authors name (most likely the user of this computer)
        /// </summary>
        public string authorName { get; set; }

        /// <summary>
        /// The author's email address
        /// </summary>
        public string emailAddress { get; set; }

        /// <summary>
        /// The author's web site URL
        /// </summary>
        public string webSite { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
