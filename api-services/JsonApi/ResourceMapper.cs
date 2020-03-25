using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using SarData.JsonApi;
using SarData.Server.Apis;

namespace SarData.Server.JsonApi
{
  class ResourceMapper
  {
    Dictionary<string, JsonApiResource> processed = new Dictionary<string, JsonApiResource>();
    Dictionary<string, List<JsonApiResource>> byPath = new Dictionary<string, List<JsonApiResource>>();
    private readonly NamingStrategy naming;
    private readonly string[] include;
    private readonly Dictionary<string, string[]> sparseFields;

    public ResourceMapper(NamingStrategy naming, string[] include, Dictionary<string, string[]> fields)
    {
      this.naming = naming;
      this.include = include;
      this.sparseFields = fields;
    }

    public object Map(object model)
    {
      processed.Clear();
      byPath.Clear();

      List<JsonApiResource> included = null;
      JsonApiResource resource = MapResource(model, "");

      if (include.Length > 0)
      {
        included = new List<JsonApiResource>();
        Dictionary<string, JsonApiResource> forInclude = new Dictionary<string, JsonApiResource>();
        foreach (var pair in byPath)
        {
          bool shouldInclude = pair.Key.Length > 1 && include.Any(f => 
          (f + ".").StartsWith(pair.Key + ".") || f.Replace(".","!").Replace("-", ".") == pair.Key
          );

          if (shouldInclude) included.AddRange(pair.Value);          
        }
      }

      return new
      {
        Data = resource,
        Included = included?.Distinct(new ResourceIdComparer())
      };
    }

    public object MapList<T>(IEnumerable<T> model)
    {
      processed.Clear();
      byPath.Clear();

      List<JsonApiResource> included = null;
      List<JsonApiResourceId> resources = MapResourceList(model, "");

      if (include.Length > 0)
      {
        included = new List<JsonApiResource>();
        Dictionary<string, JsonApiResource> forInclude = new Dictionary<string, JsonApiResource>();
        foreach (var pair in byPath)
        {
          bool shouldInclude = pair.Key.Length > 1 && include.Any(f =>
          (f + ".").StartsWith(pair.Key + ".") || f.Replace(".", "!").Replace("-", ".") == pair.Key
          );

          if (shouldInclude) included.AddRange(pair.Value);
        }
      }

      return new
      {
        Data = resources,
        Included = included?.Distinct(new ResourceIdComparer())
      };
    }

    private JsonApiResource MapResource(object model, string path)
    {
      if (model == null) return null;

      var idProperty = model.GetType().GetProperties().Where(f => f.GetCustomAttribute<KeyAttribute>() != null || f.Name == "Id").ToArray();
      if (idProperty.Length != 1) throw new InvalidOperationException($"Can't identity key property for {model.GetType().FullName}. Must have one property named Id or decorated with [Key] attribute");

      string id = idProperty[0].GetValue(model)?.ToString();
      if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException($"Invalid {model.GetType().FullName} has empty {idProperty[0].Name} value.");

      string resourceType = naming.GetPropertyName(model.GetType().Name, false);

      string processedKey = $"{resourceType} {id}";
      if (processed.ContainsKey(processedKey))
      {
        return processed[processedKey];
      }

      JsonApiResource resource = new JsonApiResource(resourceType, id);
      processed.Add(processedKey, resource);

      string byPathKey = path.Trim('.');
      if (!byPath.TryGetValue(byPathKey, out List<JsonApiResource> peers))
      {
        peers = new List<JsonApiResource>();
        byPath.Add(byPathKey, peers);
      }
      peers.Add(resource);

      var fields = model.GetType().GetProperties().Select(p => new { p, a = p.GetCustomAttribute<ResourcePropertyAttribute>(true) }).Where(f => f.a != null);

      foreach (var field in fields)
      {
        var resourcePropertyType = field.a.Type;

        object result = field.p.GetValue(model);
        string name = naming.GetPropertyName(field.p.Name, false);

        bool shouldMap = true;
        if (sparseFields.TryGetValue(resourceType, out string[] sparseMembers))
        {
          shouldMap = sparseMembers.Contains(name);
        }

        switch (field.a.Type)
        {
          case ResourcePropertyType.Metadata:
            if (!shouldMap) continue;
            resource.Metadata = resource.Metadata ?? new Dictionary<string, object>();
            resource.Metadata.Add(name, result);
            break;

          case ResourcePropertyType.Attribute:
            if (!shouldMap) continue;
            resource.Attributes = resource.Attributes ?? new Dictionary<string, object>();
            resource.Attributes.Add(name, result);
            break;

          case ResourcePropertyType.Relationship:
            if (result == null) continue;

            Type relationshipType = result.GetType();
            Type elementType = GetElementType(relationshipType);
            string childType = naming.GetPropertyName(elementType.Name, false);
            object data;
            if (relationshipType != elementType)
            {
              data = new { Data = MapResourceList((IEnumerable)result, $"{path}.{name}").Select(f => f.AsId()) };
            }
            else
            {
              data = MapResource(result, $"{path}.{name}").AsId();
            }
            if (!shouldMap) continue;
            resource.Relationships = resource.Relationships ?? new Dictionary<string, object>();
            resource.Relationships.Add(name, data);
            break;
        }
      }

      return resource;
    }

    private List<JsonApiResourceId> MapResourceList(IEnumerable model, string path)
    {
      if (model == null) return null;

      List<JsonApiResourceId> result = new List<JsonApiResourceId>();
      foreach (var src in model)
      {
        JsonApiResource resource = MapResource(src, path);

        result.Add(string.IsNullOrWhiteSpace(path) ? resource : resource.AsId());
      }
      return result;
    }

    private Type GetElementType(Type propertyType)
    {
      Type t = propertyType;
      if (propertyType.IsGenericType && propertyType != typeof(string) && typeof(IEnumerable<>).MakeGenericType(propertyType.GetGenericArguments()).IsAssignableFrom(propertyType))
      {
        t = propertyType.GetGenericArguments()[0];
      }
      return t;
    }
  }

  class ResourceIdComparer : IEqualityComparer<JsonApiResourceId>
  {
    public bool Equals(JsonApiResourceId x, JsonApiResourceId y)
    {
      return GetHashCode(x).Equals(GetHashCode(y));
    }

    public int GetHashCode(JsonApiResourceId obj)
    {
      return $"{obj.Type} {obj.Id}".GetHashCode();
    }
  }
}