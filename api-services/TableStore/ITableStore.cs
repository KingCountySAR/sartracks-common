using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace SarData.Server.Apis.TableStore
{
  public interface ITableStore
  {
    Task<T> InsertRow<T>(string table, T entity) where T : class, ITableEntity;
    Task<T> GetRow<T>(string table, string key) where T : class, ITableEntity;
  }
}
