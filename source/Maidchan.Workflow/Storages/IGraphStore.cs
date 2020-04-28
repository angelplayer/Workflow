using System.Threading.Tasks;

namespace Maidchan.Workflow.Storages
{
    public interface IGraphStore
    {
        Task Save(string workflowId, string definition, int version);

        Task Delete(string workflowId, int version);

        Task<string> Get(string workflowId, int version);
    }
}