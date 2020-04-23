
using System.Threading.Tasks;
using Maidchan.Workflow;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;
using WorkflowCore.Services.DefinitionStorage;

namespace MiadChan.Workflow.Example.Controllers
{
    [ApiController()]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        readonly IWorkflowManager workflowManager;

        public WorkflowController(IWorkflowManager manager)
        {
            this.workflowManager = manager;
        }

        [HttpPost("{processName}")]

        public async Task<IActionResult> ExecuteWorkflowAsync(string processName)
        {
            try
            {
                await workflowManager.Execute(processName);
            }
            catch (WorkflowException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Aduit()
        {
            try
            {
                await workflowManager.Execute(nameof(TaskWorkflow), new TaskDataModel() { Id = "business data 1", Message = "init message", Value = 0 }, null);
            }
            catch (WorkflowException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}