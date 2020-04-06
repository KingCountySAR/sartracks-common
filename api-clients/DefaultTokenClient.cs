using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

namespace SarData.Common.Apis
{
  public class DefaultTokenClient : ITokenClient
  {
    private readonly string authority;
    private readonly string clientId;
    private readonly string clientSecret;

    public DefaultTokenClient(IConfiguration config)
    {
      authority = config["auth:authority"];
      clientId = config["apis:client_id"];
      clientSecret = config["apis:client_secret"];
    }

    public async Task<string> GetToken(string scope)
    {
      var client = new HttpClient();
      var disco = await client.GetDiscoveryDocumentAsync(authority);
      if (disco.IsError)
      {
        throw new ApplicationException("Error getting OAuth information: " + disco.Error);
      }

      var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
      {
        Address = disco.TokenEndpoint,

        ClientId = clientId,
        ClientSecret = clientSecret,
        Scope = scope
      });

      if (response.IsError)
      {
        throw new ApplicationException("Error getting token " + response.Error);
      }

      return response.AccessToken;
    }
  }
}
