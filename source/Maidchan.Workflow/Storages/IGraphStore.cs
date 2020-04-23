using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Maidchan.Workflow.Storages
{
  public interface IGraphStore
  {
    Task Save(string workflowId, string definition, int version);

    Task Delete(string workflowId, int version);

    Task<string> Get(string workflowId, int version);
  }

  public class GraphStore : IGraphStore
  {
    public static string storeLocation = "workflow";

    public GraphStore()
    {
      // if (string.IsNullOrEmpty(storeLocation) || !File.Exists(storeLocation))
      // {
      //     throw new FileNotFoundException("configuration is in graph location is invalid.");
      // }
      // this.storeLocation = storeLocation;
    }

    public async Task Delete(string workflowId, int version)
    {
      var path = Path.Combine(storeLocation, $"{workflowId}.{version}.workflow");
      if (File.Exists(path))
      {
        File.Delete(path);
      }
      await Task.CompletedTask;
    }

    public async Task<string> Get(string workflowId, int version)
    {
      var path = Path.Combine(storeLocation, $"{workflowId}.{version}.workflow");
      if (File.Exists(path))
      {
        var text = File.ReadAllText(path);
        return await Task.FromResult(text);
      }

      return await Task.FromResult(string.Empty);
    }

    public async Task Save(string workflowId, string definition, int version)
    {
      var path = Path.Combine(storeLocation, $"{workflowId}.{version}.workflow");
      if (!File.Exists(path))
      {
        File.WriteAllText(path, definition);
      }
      else
      {
        using (var writer = new StreamWriter(path, false))
        {
          writer.WriteLine(definition);
        }
      }

      await Task.CompletedTask;
    }
  }
}