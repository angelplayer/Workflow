using System;
using System.Diagnostics;
using System.IO;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow.TaskType
{
    public class ExecuteCommandLine : StepBody
    {
        public string App { get; set; }
        public string Params { get; set; }
        public bool TerminateOnError { get; set; }

        // output
        public string OutputText { get; set; }

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