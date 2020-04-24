using System;
using System.Diagnostics;
using System.IO;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow.TaskType
{

    public class SampleModel
    {
        public string App { get; set; }
        public string LogFile { get; set; }
        public string Args { get; set; }

        public string Answer { get; set; }
    }

    public class WriteToFile : StepBody
    {
        public string Text { get; set; }
        public string Filepath { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (!(string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(Filepath)))
            {
                File.AppendAllText(Filepath, Text);
                return ExecutionResult.Next();
            }
            return ExecutionResult.Outcome(string.Empty);
        }
    }

    public class ExecuteCommandLine : StepBody
    {
        public string App { get; set; }
        public string Params { get; set; }

        // output
        public string OutputText { get; set; }

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

        protected void LogError(string message)
        {
            // TODO: Change to propery logger
            System.Console.WriteLine($">>> ERROR >>> {message}");
        }

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
                    return ExecutionResult.Next();
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return ExecutionResult.Outcome("Fail");
        }
    }
}