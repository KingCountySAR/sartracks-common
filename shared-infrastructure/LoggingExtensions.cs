using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace SarData.Logging
{
  public static class LoggingExtensions
  {
    public static ILoggingBuilder AddSarDataLogging(this ILoggingBuilder builder, string contentRoot)
    {
      Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.RollingFile(Path.Combine(contentRoot, "log-{Date}.txt"), restrictedToMinimumLevel: LogEventLevel.Information)
        .CreateLogger();

      return builder
        .AddSerilog()
        .AddApplicationInsights()
        .AddAzureWebAppDiagnostics();
    }
  }
}
