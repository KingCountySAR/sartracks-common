using System.Threading.Tasks;

namespace SarData.Common.Apis
{
  public interface ITokenClient
  {
    Task<string> GetToken(string scope);
  }
}
