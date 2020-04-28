using System;
using System.Diagnostics;
using System.IO;
using Maidchan.Workflow.Attributes;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow.TaskType
{
    [StepType("Execute commandline application")]
    public class ExecuteCommandLine : StepBody
    {
        [Input(HelpText = "Absolute path to commandline application.")]
        public string App { get; set; }

        [Input(HelpText = "Commandline parameter")]
        public string Params { get; set; }

        [Input(DataKind.Boolean, "Terminate workflow in case of errors")]
        public bool TerminateOnError { get; set; }

        // output
        [Output] public string OutputText { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            try
            {
                using (var process = CreateProcess())
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        OutputText = result;
                    }
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                context.LogError(ex.Message);
                if (TerminateOnError)
                    return context.Terminate();
            }

            return context.Next();
        }

        protected Process CreateProcess()
        {
            return Process.Start(new ProcessStartInfo()
            {
                FileName = App,
                Arguments = Params,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}