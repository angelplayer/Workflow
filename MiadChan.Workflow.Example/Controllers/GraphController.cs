using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maidchan.Workflow;
using MiadChan.Workflow.Models;

namespace MiadChan.Workflow.Example.Controllers
{
  [ApiController]
  [Route("[Controller]")]
  public class GraphController : ControllerBase
  {
    readonly IGraphConnector connector;

    public GraphController(IGraphConnector connector)
    {
      this.connector = connector;
    }

    [HttpGet("workflow")]
    public IActionResult GetWorkflowList()
    {
      return Ok(connector.GetWorkNameList());
    }

    [HttpGet("workflow/{processName}")]
    public IActionResult Get(string processName)
    {
      // Retrieve workflow defintion from json
      return Ok(connector.GetGraph(processName));
    }

    [HttpPost("workflow")]
    public async Task<IActionResult> Post()
    {
      // Convert json -> workflow json or yml format

      try
      {
        // Load json from file for temporary
        var json = await System.IO.File.ReadAllTextAsync("workflow2.json");
        await connector.SetGraph(json);

        return Ok();
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
    public async Task<IActionResult> Put([FromRoute] string Id, [FromBody] WorkflowDataModel workflowJson)
    {
      await connector.CommitAsync(workflowJson);
      return Ok();
    }

    [HttpGet("workflow/allsteptype")]
    public IActionResult GetAllSteptype()
    {
      return Ok(connector.AllStepType());
    }
  }
}