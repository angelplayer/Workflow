using System.Threading.Tasks;
using WorkflowCore.Interface;
using System.Text.Json;
using WorkflowCore.Services.DefinitionStorage;
using WorkflowCore.Exceptions;

namespace Maidchan.Workflow
{
    public class WorkflowManager : IWorkflowManager
    {
        private readonly IWorkflowController controller;
        private readonly IDefinitionLoader definitionLoader;
        private readonly IWorkflowRegistry registry;
        private readonly IPersistenceProvider persistence;

        public WorkflowManager(IWorkflowController controller, IDefinitionLoader definitionLoader, IWorkflowRegistry registry, IPersistenceProvider persistence)
        {
            this.controller = controller;
            this.definitionLoader = definitionLoader;
            this.registry = registry;
            this.persistence = persistence;
        }

        public async Task Execute(string workflowId, object data = null, string reference = null)
        {
            try
            {
                await controller.StartWorkflow(workflowId);
            }
            catch (WorkflowNotRegisteredException notRegister)
            {
                throw new WorkflowException(WorkflowException.Warning, notRegister.Message);
            }
            catch (System.Exception ex)
            {
                throw new WorkflowException(WorkflowException.Unknown, ex.Message);
            }
        }

        public async ValueTask<string> GetDefinition(string workflowId)
        {
            // keep wanring always
            await Task.Yield();

            var model = registry.GetDefinition(workflowId);
            if (model == null) return "Not registered";

            // FIXME: Temporary need to convert it json
            return $"workflow - Id: {model.Id}, Version: {model.Version}";
        }

        public async ValueTask<string> SaveWorkflow(string definition)
        {
            JsonDocument.Parse(definition, new JsonDocumentOptions { AllowTrailingCommas = true });
            var model = definitionLoader.LoadDefinition(definition, Deserializers.Json);
            if (!registry.IsRegistered(model.Id, model.Version))
            {
                registry.RegisterWorkflow(model);
            }

            // TODO: Save defintion to external storages

            // keep wanring always
            await Task.Yield();

            return model.Id;
        }
    }
}

// var result = Newtonsoft.Json.JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings
// {
//     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
// });