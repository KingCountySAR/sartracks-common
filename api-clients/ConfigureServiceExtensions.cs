using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace SarData.Common.Apis
{
  public static class ConfigureServiceExtensions
  {
    public static void ConfigureApi<TApi>(this IServiceCollection services, string apiSettingName, IConfiguration config) where TApi : class
    {
      Uri apiUrl = new Uri(config["apis:" + apiSettingName]);
      string scope = config["apis:scope"];
      var client = new HttpClient();

      services.AddRefitClient<TApi>()
        .ConfigureHttpClient(
        (svcs, c) =>
        {
          var tokenClient = svcs.GetRequiredService<ITokenClient>();
          c.BaseAddress = apiUrl;
          Task.Run(() =>
          {
            c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenClient.GetToken(scope).Result);
          }).Wait();
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(2));
    }
  }
}
