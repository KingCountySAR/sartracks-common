using System.Collections.Generic;

namespace SarData.JsonApi
{
  public class JsonApiResource : JsonApiResourceId
  {
    public JsonApiResource(string type, string id) : base(type, id)
    {
    }

    public Dictionary<string, object> Attributes { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public Dictionary<string, object> Relationships { get; set; }

    public override JsonApiResourceId AsId()
    {
      return new JsonApiResourceId(Type, Id);
    }
  }
}
