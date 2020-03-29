using SarData.Common.Apis.Health;

namespace SarData.Server.Apis.Health
{
  public class AppHealthResponse  : HealthResponse
  {
    public AppHealthResponse() { }

    public AppHealthResponse(HealthStatusType status) : base(status) { }

    public object BuildInfo { get; set; }
  }
}
