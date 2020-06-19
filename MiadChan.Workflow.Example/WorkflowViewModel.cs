using System.Collections.Generic;

namespace MiadChan.Workflow.Example
{
  public class WorkflowDataModel
  {
    public string Id { get; set; }
    public int Version { get; set; }
    public IEnumerable<Node>  Nodes { get; set; }
    public IEnumerable<Edge> Edges { get; set; }
    public string DataType { get; set; }
  }
}