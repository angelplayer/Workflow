using System.Threading.Tasks;
using WorkflowCore.Interface;
using System.Text.Json;
using WorkflowCore.Services.DefinitionStorage;
using WorkflowCore.Exceptions;
using Maidchan.Workflow.Storages;

namespace Maidchan.Workflow
{
  public class WorkflowManager : IWorkflowManager
  {
    private readonly IWorkflowController controller;
    private readonly IDefinitionLoader definitionLoader;
    private readonly IWorkflowRegistry registry;
    private readonly IPersistenceProvider persistence;
    private readonly IGraphStore graphSore;

    public WorkflowManager(IWorkflowController controller, IDefinitionLoader definitionLoader, IWorkflowRegistry registry, IPersistenceProvider persistence, IGraphStore graphSore)
    {
      this.controller = controller;
      this.definitionLoader = definitionLoader;
      this.registry = registry;
      this.persistence = persistence;
      this.graphSore = graphSore;
    }

    public async Task Execute(string workflowId, object data = null, string reference = null)
    {
      try
      {
        await controller.StartWorkflow(workflowId, data, reference);
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
      var model = registry.GetDefinition(workflowId);
      if (model == null) return "Not registered";

      var definition = await graphSore.Get(model.Id, model.Version);
      return definition;
    }

    public async ValueTask<string> SaveWorkflow(string definition)
    {
      JsonDocument.Parse(definition, new JsonDocumentOptions { AllowTrailingCommas = true });
      var model = definitionLoader.LoadDefinition(definition, Deserializers.Json);
      if (!registry.IsRegistered(model.Id, model.Version))
      {
        registry.RegisterWorkflow(model);
      }

      await graphSore.Save(model.Id, definition, model.Version);

      return model.Id;
    }
  }
}