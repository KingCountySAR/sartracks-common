using System.Collections.Generic;

namespace SarData.Common.Apis.Health
{
  public class HealthResponse
  {
    public HealthResponse() { }

    public HealthResponse(HealthStatusType status)
    {
      Status = status;
    }

    public HealthStatusType Status { get; set; }
    public IEnumerable<InnerHealthCheck> Checks { get; set; }
  }
}
