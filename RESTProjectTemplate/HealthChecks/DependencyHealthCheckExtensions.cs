using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Text.Json;

namespace $safeprojectname$.Healthchecks
{
    /// <summary>
    /// Dependency Health Check Extensions
    /// </summary>
    public static class DependencyHealthCheckExtensions
    {
        private const string DependencyTag = "dependency";

        /// <summary>
        /// AddDependencyHealthChecks
        /// </summary>
        /// <param name="services">the <see cref="IServiceCollection"/> being extended.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> interface.</param>
        public static void AddDependencyHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            // Chain as many dependency health checks as you need using the services collection following this convention:
            services.AddHealthChecks()
                .AddUrlGroup(options => options
                        .AddUri(new Uri(configuration["$safeprojectname$:HealthCheckUrl"])) /*REPLACE MyFirstDependency with your real dependency configuration name*/
                        .ExpectHttpCode(200)
                        .UseGet()
                    , failureStatus: HealthStatus.Unhealthy
                    , name: "$safeprojectname$" /*REPLACE YourDependencyNameHere*/
                    , tags: new[] { DependencyTag }
                );
        }

        /// <summary>
        /// MapDependencyHealthChecks
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> being extended.</param>
        /// <param name="pattern">A string representing the pattern</param>
        public static void MapDependencyHealthChecks(this IEndpointRouteBuilder endpoints, string pattern)
        {
            endpoints.MapHealthChecks(pattern, new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains(DependencyTag),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        health = report.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
                    },
                        new JsonSerializerOptions { WriteIndented = true });
                    await context.Response.WriteAsync(result);
                }
            });
        }
    }
}
