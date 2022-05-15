using Microsoft.AspNetCore.Mvc.Versioning;
using System.Text.RegularExpressions;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// Header version reader
    /// </summary>
    public class HeaderVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Add Parameters
        /// </summary>
        /// <param name="context"></param>
        public void AddParameters(IApiVersionParameterDescriptionContext context)
        {
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string Read(HttpRequest request)
        {
            var acceptHeader = request.Headers.FirstOrDefault(h => h.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase));

            if (acceptHeader.Key != null)
            {
                foreach (var headerValue in acceptHeader.Value)
                {
                    var match = Regex.Match(headerValue, "application\\/(?<style>[a-z-A-Z0-9]+)(\\.v(?<version>[0-9]+)){0,1}\\+(?<media>.*)", RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        _ = match.Groups["style"].Value;
                        var version = match.Groups["version"].Value;
                        _ = match.Groups["media"].Value;

                        return version;
                    }
                }
            }

            return string.Empty;
        }
    }
}
