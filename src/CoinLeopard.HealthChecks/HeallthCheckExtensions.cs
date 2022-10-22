using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinLeopard.HealthChecks;

public static class HealthChecksExtensions
{
    private const string HealthCheckPath = "/health-check";
    private static DateTime _startDate;

    public static void UseCLHealthChecks(
        this IApplicationBuilder app,
        string applicationName,
        string? basePath = null
    )
    {
        _startDate = DateTime.UtcNow;
        app.Use(
            async (context, next) =>
            {
                var path = new PathString(
                    basePath != null ? $"/{basePath}{HealthCheckPath}" : HealthCheckPath
                );

                if (
                    path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase)
                    && context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                )
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(
                            new HealthCheckResult()
                            {
                                Uptime = DateTime.UtcNow - _startDate,
                                ApplicationName = applicationName
                            }
                        )
                    );
                }
                else
                {
                    await next();
                }
            }
        );
    }
}
