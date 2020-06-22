using System.Threading.Tasks;
using MiadChan.Workflow.Models;

namespace Maidchan.Workflow
{
  public interface IGraphConnector
  {
    string GetGraph(string graphName);
    Task SetGraph(string json);
    Task CommitAsync(WorkflowDataModel model);
    string AllStepType();
    string[] GetWorkNameList();
  }
}