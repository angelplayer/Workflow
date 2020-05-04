using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Maidchan.Workflow
{

  public class TaskDataModel
  {
    public string Id { get; set; }
    public int Value { get; set; }
    public string Message { get; set; }
  }

  public class TaskWorkflow : IWorkflow<TaskDataModel>
  {
    public string Id => nameof(TaskWorkflow);

    public int Version => 1;

    public void Build(IWorkflowBuilder<TaskDataModel> builder)
    {
      var branchNode2 = builder.CreateBranch().StartWith<TaskNode2>().Input(step => step.Data, data => data);
      var branchNode3 = builder.CreateBranch().StartWith<TaskNode3>().Input(step => step.Data, data => data);

      builder.StartWith<BeginNode>()
          .Then<TaskNode>().Input(step => step.Data, data => data)
          .Decide(data => data.Value)
              .Branch((data, outcome) => data.Value <= 1, branchNode2)
              .Branch((data, outcome) => data.Value > 1, branchNode3)
          .Then<EndNode>();
    }
  }

  public class BeginNode : StepBody
  {
    public override ExecutionResult Run(IStepExecutionContext context)
    {
      System.Console.WriteLine("===>Begin Node");
      return ExecutionResult.Next();
    }
  }

  public class TaskNode : StepBody
  {
    public TaskDataModel Data { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
      Data.Message += "- Changed by tasknode";
      Data.Value = 1;

      System.Console.WriteLine($"===> Execute task node: {Data.Message}");
      return ExecutionResult.Next();
    }
  }

  public class TaskNode2 : StepBody
  {
    public TaskDataModel Data { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
      Data.Message += "change byte tasknode 2";
      System.Console.WriteLine($"===> Execute task node 2: {Data.Message}");
      return ExecutionResult.Next();
    }
  }

  public class TaskNode3 : StepBody
  {
    public TaskDataModel Data { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
      Data.Message += "change byte tasknode 3";
      System.Console.WriteLine($"===> Execute task node 3: {Data.Message}");
      return ExecutionResult.Next();
    }
  }

  public class EndNode : StepBody
  {
    public override ExecutionResult Run(IStepExecutionContext context)
    {
      System.Console.WriteLine("===> End node");
      return ExecutionResult.Next();
    }
  }
}