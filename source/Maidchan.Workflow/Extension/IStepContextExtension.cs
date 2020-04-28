using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow
{
    public static class IStepContextExtension
    {
        public static ExecutionResult Next(this IStepExecutionContext context)
        {
            return ExecutionResult.Next();
        }

        public static ExecutionResult Terminate(this IStepExecutionContext context)
        {
            context.Workflow.Status = WorkflowStatus.Terminated;
            return ExecutionResult.Next();
        }

        public static ExecutionResult Completed(this IStepExecutionContext context)
        {
            context.Workflow.Status = WorkflowStatus.Complete;
            return ExecutionResult.Next();
        }

        public static ExecutionResult Suspended(this IStepExecutionContext context)
        {
            context.Workflow.Status = WorkflowStatus.Suspended;
            return ExecutionResult.Next();
        }

        public static ExecutionResult Outcome(this IStepExecutionContext context, object outcome)
        {
            return ExecutionResult.Outcome(outcome);
        }

        public static void LogError(this IStepExecutionContext context, string message)
        {
            System.Console.WriteLine($">>> ERROR >>> {message}");
        }

    }
}