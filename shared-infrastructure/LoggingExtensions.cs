using Microsoft.Extensions.Logging;

namespace SarData.Logging
{
  public static class LoggingExtensions
  {
    public static ILoggingBuilder AddSarDataLogging(this ILoggingBuilder builder)
    {
      return builder
        .AddConsole()
        .AddApplicationInsights()
        .AddAzureWebAppDiagnostics();
    }
  }
}
