namespace SarData.JsonApi
{
  public class JsonApiResourceId
  {
    public JsonApiResourceId(string type, string id)
    {
      Type = type;
      Id = id;
    }

    public string Type { get; }
    public string Id { get; }

    public virtual JsonApiResourceId AsId()
    {
      return this;
    }
  }
}
