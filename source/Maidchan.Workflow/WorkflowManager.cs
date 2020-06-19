using System.Threading.Tasks;
using WorkflowCore.Interface;
using System.Text.Json;
using WorkflowCore.Services.DefinitionStorage;
using WorkflowCore.Exceptions;
using Maidchan.Workflow.Storages;
using Maidchan.Workflow.Exceptions;
using System.Collections.Generic;
using System.Collections.Immutable;
using Maidchan.Workflow.Attributes;
using System.Linq;

namespace Maidchan.Workflow
{
  public class WorkflowManager : IWorkflowManager
  {
    private readonly IWorkflowController controller;
    private readonly IDefinitionLoader definitionLoader;
    private readonly IWorkflowRegistry registry;
    private readonly IPersistenceProvider persistence;
    private readonly IGraphStore graphSore;

    public static IDictionary<string, System.Type> stepClassDict = new SortedDictionary<string, System.Type>();

    public WorkflowManager(IWorkflowController controller, IDefinitionLoader definitionLoader, IWorkflowRegistry registry, IPersistenceProvider persistence, IGraphStore graphSore)
    {
      this.controller = controller;
      this.definitionLoader = definitionLoader;
      this.registry = registry;
      this.persistence = persistence;
      this.graphSore = graphSore;

      if(stepClassDict == null || stepClassDict.Keys.Count == 0) 
      {
        LoadStepClass();
      }
    }

    private void LoadStepClass()
    {
      var allStepClass = typeof(StepTypeAttribute).Assembly.GetTypes();
      lock(stepClassDict) 
      {
        foreach(var stepClass in allStepClass) 
        {
          if(stepClass.GetCustomAttributes(typeof(StepTypeAttribute), true).Length > 0)
          {
            stepClassDict.Add(stepClass.Name, stepClass);
          }
        }
      }
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

    public IEnumerable<string> GetAllStepType()
    {
      var allStepClass = stepClassDict.Values;

      if(allStepClass.Count <= 0) 
        throw new System.Exception("Step classes has not loaded yet!!!");

      foreach(var stepClass in allStepClass) {
        if(stepClass.GetCustomAttributes(typeof(StepTypeAttribute), true).Length > 0)
          yield return $"{stepClass.FullName}, {stepClass.Assembly.GetName().Name}";
      }
    }
    public string[] GetWorkflows()
    {
      var enumerate = graphSore.GetWorkflowList();
      var list = new List<string>(enumerate.Count());
      foreach(var item in enumerate) {
        var path = item.Split('.');
        if(registry.IsRegistered(path[0], int.Parse(path[1])))
        {
          list.Add(path[0]);
        }
      }

      return list.ToArray();
    }

    public IDictionary<string, System.Type> ExportStepType() {
      return stepClassDict.ToImmutableDictionary();
    }
  }
}