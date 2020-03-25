using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SarData.Server.JsonApi
{
  public abstract class JsonApiController : Controller
  {
    protected readonly NamingStrategy naming;
    private readonly Dictionary<string, LambdaExpression> queryProjections;
    private readonly Dictionary<string, LambdaExpression> modelProjections;
    private readonly IConfiguration config;

    public JsonApiController(
      Dictionary<string, LambdaExpression> queryProjections,
      Dictionary<string, LambdaExpression> modelProjections,
      JsonSerializerSettings serializerSettings, IConfiguration config)
    {
      naming = serializerSettings.ContractResolver as NamingStrategy ?? new CamelCaseNamingStrategy();
      this.queryProjections = queryProjections;
      this.modelProjections = modelProjections;
      this.config = config;
    }

    protected virtual JsonResult SingleResource<T>(T model, Dictionary<string, string[]> fields, string[] include)
    {
      var mapper = new ResourceMapper(naming, include, fields);
      return Json(mapper.Map(model));
    }

    protected JsonResult ListResource<T>(IEnumerable<T> model, Dictionary<string, string[]> fields, string[] include)
    {
      var mapper = new ResourceMapper(naming, include, fields);
      return Json(mapper.MapList(model));
    }

    protected Expression<Func<F, I>> GetQueryProjection<F, I>(string[] include, Dictionary<string, string[]> fields)
    {
      ProjectionLibrary library = new ProjectionLibrary(queryProjections, new Dictionary<string, object> { { "utc", DateTime.UtcNow } }, naming);
      return library.BuildProjection<F, I>(include, fields);
    }

    protected Expression<Func<I, T>> GetModelProjection<I, T>(string[] include, Dictionary<string, string[]> fields)
    {
      ProjectionLibrary library = new ProjectionLibrary(modelProjections, new Dictionary<string, object> { { "PhotoPath", config["paths:photos"] } }, naming);

      return library.BuildProjection<I, T>(include, fields);
    }
  }

  public abstract class JsonApiController<F,I,T> : JsonApiController
  {
    public JsonApiController(
      Dictionary<string, LambdaExpression> queryProjections,
      Dictionary<string, LambdaExpression> modelProjections,
      JsonSerializerSettings serializerSettings, IConfiguration config) : base(queryProjections, modelProjections, serializerSettings, config)
    { }

    override protected JsonResult SingleResource<M>(M model, Dictionary<string, string[]> fields, string[] include)
    {
      return SingleResource<M>(model, fields, include);
    }

    protected JsonResult ListResource(IEnumerable<T> model, Dictionary<string, string[]> fields, string[] include)
    {
      return ListResource<T>(model, fields, include);
    }

    protected Expression<Func<F, I>> GetQueryProjection(string[] include, Dictionary<string, string[]> fields)
    {
      return GetQueryProjection<F, I>(include, fields);
    }

    protected Expression<Func<I, T>> GetModelProjection(string[] include, Dictionary<string, string[]> fields)
    {
      return GetModelProjection<I, T>(include, fields);
    }
  }
}
