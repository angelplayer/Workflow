using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maidchan.Workflow;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

namespace MiadChan.Workflow.Example.Controllers
{
  [ApiController]
  [Route("[Controller]")]
  public class GraphController : ControllerBase
  {
    readonly IWorkflowManager workflow;

    public GraphController(IWorkflowManager manager)
    {
      this.workflow = manager;
    }

    [HttpGet("workflow")]
    public IActionResult GetWorkflowList()
    {
      var list = this.workflow.GetWorkflows();

      return Ok(list);
    }

    [HttpGet("workflow/{processName}")]
    public IActionResult Get(string processName)
    {
      // Retrieve workflow defintion from json
      var json = workflow.GetDefinition(processName);

      // Convert it Dagre-3d json data format
      string dagreObject = GraphTransformer.ConvertToDegreD3Object(json.Result);
      return Ok(dagreObject);
    }

    [HttpPost("workflow")]
    public async Task<IActionResult> Post()
    {
      // Convert json -> workflow json or yml format

      try
      {
        // Load json from file for temporary
        var json = await System.IO.File.ReadAllTextAsync("workflow2.json");
        var workflowId = workflow.SaveWorkflow(json);

        return Ok(workflowId.Result);
      }
      catch (FileNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (System.Exception ex)
      {
        return NotFound(ex.Message);
      }
    }

    [HttpPut("workflow/{id}")]
    public async Task<IActionResult> Put([FromRoute] string Id, [FromBody] WorkflowDto workflowJson)
    {
      await Task.Yield();
      return Ok(GraphTransformer.WorkflowFromGraph(workflowJson));
    }

    [HttpGet("workflow/allsteptype")]
    public IActionResult GetAllSteptype()
    {

      var options = new JsonWriterOptions()
      {
        Indented = true
      };

      var output = new MemoryStream();
      using (var writer = new Utf8JsonWriter(output, options))
      {
        writer.WriteStartArray();

        foreach (var step in workflow.GetAllStepType())
        {
          writer.WriteStartObject();
          GraphTransformer.WriteStepTypeParams(writer, step);
          writer.WriteEndObject();
        }

        writer.WriteEndArray();
      }

      return Ok(Encoding.UTF8.GetString(output.GetBuffer()));
    }

    public class Node
    {
      public string Id { get; set; }
      public string Kind { get; set; }
      public string Label { get; set; }
      public string StepType { get; set; }
      public string Description { get; set; }
      public JsonElement Props{ get; set; }
    }

    public class Edge {
      public string From { get; set; }
      public string To { get; set; }
      public string Id { get; set; }
    }

    public class WorkflowDto
    {
      public string Id { get; set; }
      public int Version { get; set; }
      public IEnumerable<Node>  Nodes { get; set; }
      public IEnumerable<Edge> Edges { get; set; }
      public string DataType { get; set; }
    }
  }
}