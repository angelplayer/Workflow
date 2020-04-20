using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Maidchan.Workflow
{
    public static class MaidWorkflowExtendsion
    {
        public static IServiceCollection AddMaidWorkflow(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddWorkflow();
            serviceCollection.AddWorkflowDSL();

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