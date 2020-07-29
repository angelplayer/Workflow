using System;
using Maidchan.Workflow.Storages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Maidchan.Workflow
{
  public class WorkflowOption {
    public string ConnectionString { get; set; }
    public ProviderType Provider { get; set; } = ProviderType.DEFAULT;
    public bool CanCreateDb { get; set; }

    public enum ProviderType {
      DEFAULT = 0,
      SQLITE = 1,
      PGSQL = 2
    }
  }

  public static class MaidWorkflowExtendsion
  {
    public static IServiceCollection AddMaidWorkflow(this IServiceCollection serviceCollection, Action<WorkflowOption> config = null)
    {
      var theOption = new WorkflowOption();
      config?.Invoke(theOption);

      if(theOption.Provider == WorkflowOption.ProviderType.SQLITE) 
      {
        serviceCollection.AddWorkflow(x => x.UseSqlite(theOption.ConnectionString, theOption.CanCreateDb));
      } else if(theOption.Provider == WorkflowOption.ProviderType.PGSQL) 
      {
        // TODO: look for PGSLQ support
        serviceCollection.AddWorkflow();
      } else {
        serviceCollection.AddWorkflow();
      }

      serviceCollection.AddTransient<IGraphStore, GraphStore>();
      
      serviceCollection.AddWorkflowDSL();
      serviceCollection.AddTransient<IWorkflowManager, WorkflowManager>();
      serviceCollection.AddTransient<IGraphConnector, GraphConnector>();

      return serviceCollection;
    }

    public static IApplicationBuilder UseMaidWorkflow(this IApplicationBuilder application)
    {
      var host = application.ApplicationServices.GetService<IWorkflowHost>();
      host.RegisterWorkflow<Workflow.TaskWorkflow, TaskDataModel>();
      host.Start();

      return application;
    }
  }
}