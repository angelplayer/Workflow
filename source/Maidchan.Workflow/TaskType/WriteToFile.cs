using System.IO;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow.TaskType
{
    public class WriteToFile : StepBody
    {
        public string Text { get; set; }
        public string Filepath { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (!(string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(Filepath)))
            {
                File.AppendAllText(Filepath, Text);
            }
            return context.Next();
        }
    }
}