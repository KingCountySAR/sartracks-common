using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SarData.Common.Apis.Health;

namespace SarData.Server.Apis.Health
{
  public static class HealthExtensions
  {
    public static IApplicationBuilder UseSarHealthChecks<TStartup>(this IApplicationBuilder app)
    {
      return app.UseHealthChecks("/_health", new HealthCheckOptions
      {
        ResponseWriter = async (context, report) =>
        {
          var response = JsonSerializer.Serialize(
             new AppHealthResponse
             {
               Status = Convert(report.Status),
               Checks = report.Entries.Select(e =>
               {
                 HealthStatus innerStatus = e.Value.Status;
                 if (e.Value.Data.TryGetValue("_result", out object statusObj))
                 {
                   innerStatus = (HealthStatus)statusObj;
                 }

                 return new InnerHealthCheck { Key = e.Key, Status = innerStatus.Convert() };
               }),
               BuildInfo = BuildInfo.Get<TStartup>()
             },
             new JsonSerializerOptions().Setup());

          context.Response.ContentType = MediaTypeNames.Application.Json;
          await context.Response.WriteAsync(response);
        }
      });
    }

    public static HealthStatusType Convert(this HealthStatus status)
    {
      return Enum.Parse<HealthStatusType>(status.ToString());
    }
  }
}
