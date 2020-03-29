using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace SarData.Common.Apis.Health
{
  public class HealthResponse
  {
    public HealthResponse()
    {
    }

    public HealthResponse(HealthStatus status)
    {
      Status = status;
    }

    public HealthStatus Status { get; set; }
    public IEnumerable<InnerCheck> Checks { get; set; } = new List<InnerCheck>();

    public class InnerCheck
    {
      public string Key { get; set; }
      public HealthStatus Status { get; set; }
    }
  }
}
