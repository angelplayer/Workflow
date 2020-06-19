using System.Threading.Tasks;
using MiadChan.Workflow.Models;

namespace Maidchan.Workflow
{
  public interface IGraphConnector
  {
    string GetGraph(string graphName);
    ValueTask<string> SetGraph(string json);
    void Commit(WorkflowDataModel model);
    string AllStepType();
    string[] GetWorkNameList();
  }
}