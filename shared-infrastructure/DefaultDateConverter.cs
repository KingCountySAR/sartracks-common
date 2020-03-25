using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SarData
{
  public class DefaultDateConverter : DateTimeConverterBase
  {
    public override bool CanConvert(Type objectType)
    {
      return base.CanConvert(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.Value == null)
        return DateTime.MinValue;

      return (DateTime)reader.Value;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      DateTime dateTimeValue = (DateTime)value;
      if (dateTimeValue == DateTime.MinValue)
      {
        writer.WriteNull();
        return;
      }

      writer.WriteValue(value);
    }
  }
}
