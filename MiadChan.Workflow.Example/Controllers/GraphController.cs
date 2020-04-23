using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;
using System.Text.Json;
using WorkflowCore.Services.DefinitionStorage;
using WorkflowCore.Models;
using Newtonsoft.Json;
using Maidchan.Workflow;

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

        [HttpGet("workflow/{processName}")]
        public IActionResult Get(string processName)
        {
            // Retrieve workflow defintion from json
            var json = workflow.GetDefinition(processName);

            // Convert it Dagre-3d json data format
            return Ok(json.Result);
        }

        [HttpPost("workflow")]
        public async Task<IActionResult> Post()
        {
            // Convert json -> workflow json or yml format

            try
            {
                // Load json from file for temporary
                var json = await System.IO.File.ReadAllTextAsync("workflow.json");
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
    }
}