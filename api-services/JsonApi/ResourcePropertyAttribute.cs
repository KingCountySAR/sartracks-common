using System;

namespace SarData.JsonApi
{
  [AttributeUsage(AttributeTargets.Property)]
  public class ResourcePropertyAttribute : Attribute
  {
    public ResourcePropertyAttribute(ResourcePropertyType type)
    {
      Type = type;
    }

    public ResourcePropertyType Type { get; private set; }
  }

  public enum ResourcePropertyType
  {
    Metadata,
    Attribute,
    Relationship
  }
}
