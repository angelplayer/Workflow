using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maidchan.Workflow
{
    public interface IWorkflowManager
    {
        ///<summary>
        /// Create or update workflow from json defintion
        ///</summary>
        /// <param name="definition">Workflow definiton in json format</param>
        /// <returns>string - workflow id</returns>
        ValueTask<string> SaveWorkflow(string definition);

        ///<summary>
        /// Execute workflow by id
        ///</summary>
        /// <param name="workflowId">Identity of workflow</param>
        Task Execute(string workflowId, object data = null, string reference = null);

        ///<summary>
        /// Get workflow difinition by workflow id in Json format
        ///</summary>
        /// <param name="workflowId">Identity of workflow</param>
        /// <returns>string - workflow id</returns>
        ValueTask<string> GetDefinition(string workflowId);

        ///<summary>
        /// List all step type
        ///</summary>
        IEnumerable<string> GetAllStepType();

        ///<summery>
        ///  List all workflow
        ///</summery>
        string[] GetWorkflows();

        ///<summery>
        /// Export all step type 
        ///</summery>
        IDictionary<string, System.Type> ExportStepType();
  }
}