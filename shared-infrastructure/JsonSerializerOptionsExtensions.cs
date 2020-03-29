using System.Text.Json;
using System.Text.Json.Serialization;

namespace SarData
{
  public static class JsonSerializerOptionsExtensions
  {
    public static JsonSerializerOptions Setup(this JsonSerializerOptions options)
    {
      options.IgnoreNullValues = true;
      options.PropertyNameCaseInsensitive = true;
      options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      options.Converters.Add(new JsonStringEnumConverter());
      return options;
    }
  }
}
