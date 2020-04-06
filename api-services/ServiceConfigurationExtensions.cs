using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SarData.Common.Apis;
using SarData.Common.Apis.Health;
using SarData.Common.Apis.Messaging;
using SarData.Server.Apis.Health;
using Serilog;

namespace SarData.Server
{
  public static class ServiceConfigurationExtensions
  {
    public static void AddApiHealthCheck<TApi>(this IServiceCollection services, string apiSettingName, IHealthChecksBuilder healthChecks, HealthStatus failStatus) where TApi : class, IHealthDependencyApi
    {
      healthChecks.Add(new HealthCheckRegistration(
          (apiSettingName ?? "unknown") + "-api",
          sp => new ApiHealthCheck<TApi>(sp.GetRequiredService<TApi>()),
          failStatus, new string[0]));
    }

    public static void AddMessagingApi(this IServiceCollection services, IConfiguration config, IHealthChecksBuilder healthChecks)
    {
      string messagingUrl = config["apis:messaging"];
      if (string.IsNullOrWhiteSpace(messagingUrl))
      {
        Log.Logger.Warning("messaging API not configured. Using test implementation");
        services.AddSingleton<IMessagingApi>(new TestMessagingService(config["local_files"] ?? "."));
      }
      else
      {
        Log.Logger.Information("Messaging API configured at " + messagingUrl);
        services.ConfigureApi<IMessagingApi>("messaging", config);
      }
      services.AddApiHealthCheck<IMessagingApi>("messaging", healthChecks, HealthStatus.Degraded);
    }
  }
}
