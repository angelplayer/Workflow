using Maidchan.Workflow.Storages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Maidchan.Workflow
{
    public static class MaidWorkflowExtendsion
    {
        public static IServiceCollection AddMaidWorkflow(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IGraphStore, GraphStore>();
            serviceCollection.AddWorkflow(x => x.UseSqlite(@"Data Source=database.db;", true));
            serviceCollection.AddWorkflowDSL();
            serviceCollection.AddTransient<IWorkflowManager, WorkflowManager>();

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