using System.Text.Json;

namespace MiadChan.Workflow.Example
{
  public class Node
  {
    public string Id { get; set; }
    public string Kind { get; set; }
    public string Label { get; set; }
    public string StepType { get; set; }
    public string Description { get; set; }
    public JsonElement Props{ get; set; }
  }
}