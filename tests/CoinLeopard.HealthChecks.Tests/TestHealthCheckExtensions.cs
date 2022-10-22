using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace CoinLeopard.HealthChecks.Tests;

public class TestHealthCheckExtensions
{
    private const int Port = 9999;

    public TestHealthCheckExtensions() { }

    private static WebApplication _app;

    private void CreateTestServer(string appName, string? basePath = null)
    {
        var builder = WebApplication.CreateBuilder(new string[0]);

        _app = builder.Build();

        _app.UseCLHealthChecks(appName, basePath);

        _app.Run($"http://localhost:{Port}");
    }

    private async Task StopServerASync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task TestSuccessfullHealthCheck_WithBasePath()
    {
        var appName = "app-name";
        var basePath = $"{appName}/api";

        var task = Task.Run(() =>
        {
            CreateTestServer(appName, basePath);
        });

        var client = new HttpClient();

        var requestResult = await client.GetAsync(
            $"http://localhost:{Port}/{basePath}/health-check"
        );

        var result = JsonSerializer.Deserialize<HealthCheckResult>(
            await requestResult.Content.ReadAsStringAsync()
        )!;

        result.Should().NotBeNull();
        result.Uptime.Should().BeLessThan(TimeSpan.FromSeconds(1));
        result.ApplicationName.Should().Be(appName);

        await StopServerASync();
    }

    [Fact]
    public async Task TestSuccessfullHealthCheck_WithoutBasePath()
    {
        var appName = "app-name";

        var task = Task.Run(() =>
        {
            CreateTestServer(appName);
        });

        var client = new HttpClient();

        var requestResult = await client.GetAsync($"http://localhost:{Port}/health-check");

        var result = JsonSerializer.Deserialize<HealthCheckResult>(
            await requestResult.Content.ReadAsStringAsync()
        )!;

        result.Should().NotBeNull();
        result.Uptime.Should().BeLessThan(TimeSpan.FromSeconds(1));
        result.ApplicationName.Should().Be(appName);

        await StopServerASync();
    }

    [Fact]
    public async Task TestWrongPath()
    {
        var appName = "app-name";

        var task = Task.Run(() =>
        {
            CreateTestServer(appName);
        });

        var client = new HttpClient();

        var requestResult = await client.GetAsync($"http://localhost:{Port}/gibberish");

        requestResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await StopServerASync();
    }
}
