using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maidchan.Workflow;
using System.Text.Json;
using System.Text;

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

        [HttpGet("workflow/allsteptype")]
        public IActionResult GetAllSteptype() {

            var options = new JsonWriterOptions()
            {
                Indented = true
            };

            var output = new MemoryStream();
            using(var writer = new Utf8JsonWriter(output, options)) {
                writer.WriteStartArray();

                foreach(var step in workflow.GetAllStepType()) {
                    writer.WriteStartObject();
                        GraphTransformer.WriteStepTypeParams(writer, step);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            return Ok(Encoding.UTF8.GetString(output.GetBuffer()));
        }
    }
}