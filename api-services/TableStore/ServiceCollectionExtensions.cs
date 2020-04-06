using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SarData.Server.Apis.TableStore;
using Serilog;

namespace SarData.Server.Apis
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddTableStorage(this IServiceCollection services, IConfiguration config)
    {
      if (string.IsNullOrWhiteSpace(config[AzureTableStore.CONNECTION_STRING_KEY]))
      {
        Log.Logger.Warning("Using test table storage");
        services.AddSingleton<ITableStore, TestTableStore>();
      }
      else
      {
        Log.Logger.Information("Using Azure Table Storage");
        services.AddSingleton<ITableStore, AzureTableStore>();
      }
      return services;
    }
  }
}
