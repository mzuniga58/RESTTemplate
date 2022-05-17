using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// Health Response Writer
    /// </summary>
    public static class HealthResponseWriter
    {
        /// <summary>
        /// Format
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="report">The <see cref="HealthReport"/></param>
        public static async Task Format(HttpContext context, HealthReport report)
        {
            var result = JsonSerializer.Serialize(
                new
                {
                    status = report.Status.ToString(),
                    Machine = new
                    {
                        Environment.ProcessorCount,
                        Environment.MachineName,
                        Environment.UserDomainName,
                        Environment.UserName
                    },
                    errors = report.Entries.Select(e => new { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                });
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(result);
        }
    }
}
