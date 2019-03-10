namespace Microsoft.Extensions.Configuration
{
  public static class AppConfigExtensions
  {
    public static IConfigurationBuilder AddConfigFiles(this IConfigurationBuilder config, string environment)
    {
      return config.AddJsonFile("appsettings.json", true, true)
                   .AddJsonFile($"appsettings.{environment}.json", true, true)
                   .AddJsonFile("appsettings.local.json", true, true)
                   .AddEnvironmentVariables();
    }
  }
}
