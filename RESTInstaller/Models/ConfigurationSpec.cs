using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST.Template.Models
{
    public class ConfigurationSpec
    {
        public string ResourceType { get; set; }
        public string Route { get; set; }
        public string FunctionName { get; set; }
    }
}
