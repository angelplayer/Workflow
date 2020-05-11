using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Maidchan.Workflow.Storages
{
    public class GraphStore : IGraphStore
    {
        public static string storeLocation = "workflow";

        public GraphStore()
        {
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

        public IEnumerable<string> GetWorkflowList() 
        {
            if(Directory.Exists(storeLocation)) 
            {
                // var files = Directory.GetFiles(storeLocation,"*.workflow");
                var files = Directory.GetFiles(storeLocation,"*.workflow");
                foreach(var f in files) 
                {
                    yield return Path.GetFileName(f);
                }
            }
        }
    }
}