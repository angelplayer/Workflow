using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiadChan.Workflow.Models;
using MiadChan.Workflow.Transformer;

namespace Maidchan.Workflow
{
  public class GraphConnector : IGraphConnector
  {
    private IWorkflowManager workflowManager;

    public GraphConnector(IWorkflowManager manager) => this.workflowManager = manager;

    public string[] GetWorkNameList()
    {
      return this.workflowManager.GetWorkflows();
    }

    public string GetGraph(string graphName) 
    {
      var json = workflowManager.GetDefinition(graphName);
      return GraphTransformer.ConvertToDegreD3Object(json.Result);
    }

    public async Task SetGraph(string json) 
    {
      await workflowManager.SaveWorkflow(json);
    }

    public async Task CommitAsync(WorkflowDataModel workflowJson) 
    {
      var resutl = GraphTransformer.WorkflowFromGraph(workflowJson, workflowManager.ExportStepType());
      await SetGraph(resutl);
    }

    public string AllStepType() 
    {
       var options = new JsonWriterOptions()
      {
        Indented = true
      };

      var output = new MemoryStream();
      using (var writer = new Utf8JsonWriter(output, options))
      {
        writer.WriteStartArray();
        foreach (var step in workflowManager.GetAllStepType())
        {
          writer.WriteStartObject();
          GraphTransformer.WriteStepTypeParams(writer, step);
          writer.WriteEndObject();
        }
        writer.WriteEndArray();
      }

      return Encoding.UTF8.GetString(output.GetBuffer());
    }
  }
}