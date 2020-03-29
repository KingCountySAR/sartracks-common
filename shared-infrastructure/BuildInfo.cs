using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SarData
{
  public static class BuildInfo
  {
    public static JsonElement Get<T>()
    {
      var assembly = typeof(T).Assembly;
      var resourceStream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(f => f.EndsWith(".build_info.json")));
      using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
      {
        return JsonSerializer.Deserialize<JsonElement>(reader.ReadToEnd(), new JsonSerializerOptions().Setup());
      }
    }
  }
}
