using System.IO;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Maidchan.Workflow.Attributes;

namespace Maidchan.Workflow.TaskType
{
    [StepType]
    public class WriteToFile : StepBody
    {
        [Input(HelpText = "Text value")]
        public string Text { get; set; }

        [Input(HelpText = "Absolute text file path")]
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