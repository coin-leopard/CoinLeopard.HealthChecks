namespace CoinLeopard.HealthChecks;

public class HealthCheckResult
{
    public string ApplicationName { get; set; } = null!;
    public TimeSpan Uptime { get; set; }
}
