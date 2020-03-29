using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SarData.Common.Apis.Health;

namespace SarData.Server.Apis.Health
{
  class ApiHealthCheck<TApi> : IHealthCheck where TApi : IHealthDependencyApi
  {
    private readonly TApi api;

    public ApiHealthCheck(TApi api)
    {
      this.api = api;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
      Dictionary<string, object> data = new Dictionary<string, object>();

      try
      {
        HealthResponse authStatus = await api.CheckAuth();
        return new HealthCheckResult(HealthStatus.Healthy);
      }
      catch (Exception ex)
      {
        data.Add("_result", HealthStatus.Unhealthy);
        return new HealthCheckResult(
            context.Registration.FailureStatus,
            description: "Failed to check status of API.",
            exception: ex,
            data: data);
      }
    }
  }
}
