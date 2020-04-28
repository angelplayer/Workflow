
using System.Threading.Tasks;
using Maidchan.Workflow;
using Maidchan.Workflow.Exceptions;
using Maidchan.Workflow.TaskType;
using Microsoft.AspNetCore.Mvc;

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
                await workflowManager.Execute(processName, new SampleModel() { App = "dotnet", Args = "--version", LogFile = "workflow/cli.logs" });
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