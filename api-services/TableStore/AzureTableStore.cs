using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace SarData.Server.Apis.TableStore
{
  public class AzureTableStore : ITableStore
  {
    public static readonly string CONNECTION_STRING_KEY = "store:tableString";
    private readonly string connString;

    public AzureTableStore(IConfiguration config)
    {
      connString = config[CONNECTION_STRING_KEY];
    }

    public async Task<T> InsertRow<T>(string table, T entity) where T : class, ITableEntity
    {
      entity.PartitionKey = "default";

      var op = TableOperation.Insert(entity);
      var result = await GetTable(table).ExecuteAsync(op);
      var row = result.Result as T;
      return row;
    }

    public async Task<T> GetRow<T>(string table, string key) where T : class, ITableEntity
    {
      var op = TableOperation.Retrieve<T>("default", key);
      var result = await GetTable(table).ExecuteAsync(op);
      var row = result.Result as T;
      return row;
    }

    private CloudTable GetTable(string name)
    {
      CloudStorageAccount account = CloudStorageAccount.Parse(connString);
      CloudTableClient client = account.CreateCloudTableClient(new TableClientConfiguration());
      CloudTable table = client.GetTableReference(name);

      return table;
    }
  }
}
