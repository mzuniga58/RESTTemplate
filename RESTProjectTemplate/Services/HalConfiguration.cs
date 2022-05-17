using Tense;
using Tense.Hal;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// The HAL Configuration
    /// </summary>
    public class HalConfiguration : IHalConfiguration
    {
        /// <summary>
        /// Configured links
        /// </summary>
        public TypeLinks Links { get; } = new TypeLinks();
    }
}
