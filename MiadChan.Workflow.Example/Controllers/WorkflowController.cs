
using System.Threading.Tasks;
using Maidchan.Workflow;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;

namespace MiadChan.Workflow.Example.Controllers
{
    [ApiController()]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        readonly IWorkflowController controller;

        public WorkflowController(IWorkflowController controller)
        {
            this.controller = controller;
        }

        [HttpGet]
        public async Task Aduit()
        {
            await controller.StartWorkflow(nameof(TaskWorkflow), new TaskDataModel() { Id = "business data 1", Message = "init message", Value = 0 }, null);
        }
    }
}