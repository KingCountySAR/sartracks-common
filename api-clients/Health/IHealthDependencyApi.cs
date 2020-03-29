using Refit;
using System.Threading.Tasks;

namespace SarData.Common.Apis.Health
{
  public interface IHealthDependencyApi
  {
    [Get("/_health")]
    Task<HealthResponse> CheckHealth();

    [Get("/_health/auth")]
    Task<HealthResponse> CheckAuth();
  }
}
