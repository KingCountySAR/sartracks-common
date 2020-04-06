using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace SarData.Server.Apis.TableStore
{
  public class TestTableStore : ITableStore
  {
    public Task<T> GetRow<T>(string table, string key) where T : class, ITableEntity
    {
      throw new NotImplementedException();
    }

    Task<T> ITableStore.InsertRow<T>(string table, T entity)
    {
      throw new NotImplementedException();
    }
  }
}
