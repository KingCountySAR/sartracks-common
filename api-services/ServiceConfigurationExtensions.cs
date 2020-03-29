using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SarData.Common.Apis;
using SarData.Common.Apis.Health;
using SarData.Server.Apis.Health;

namespace SarData.Server
{
  public static class ServiceConfigurationExtensions
  {
    public static void ConfigureApi<TApi>(this IServiceCollection services, string apiSettingName, IConfiguration config, IHealthChecksBuilder healthChecks, HealthStatus failStatus) where TApi : class, IHealthDependencyApi
    {
      services.ConfigureApi<TApi>(apiSettingName, config);
      healthChecks.Add(new HealthCheckRegistration(
          (apiSettingName ?? "unknown") + "-api",
          sp => new ApiHealthCheck<TApi>(sp.GetRequiredService<TApi>()),
          failStatus, new string[0]));
    }
  }
}
