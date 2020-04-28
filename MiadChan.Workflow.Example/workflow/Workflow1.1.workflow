{
    "Id": "Workflow1",
    "Version": "1",
    "Steps": [
        {
            "Id": "Task1",
            "StepType": "Maidchan.Workflow.BeginNode, Maidchan.Workflow",
            "NextStepId": "Task2"
        },
        {
            "Id": "Task2",
            "StepType": "Maidchan.Workflow.TaskNode, Maidchan.Workflow",
            "NextStepId": "Task3"
        },
        {
            "Id": "Task3",
            "StepType": "Maidchan.Workflow.TaskNode2, Maidchan.Workflow",
            "NextStepId": "Task4"
        },
        {
            "Id": "Task4",
            "StepType": "Maidchan.Workflow.TaskNode3, Maidchan.Workflow",
            "NextStepId": "Task5"
        },
        {
            "Id": "Task5",
            "StepType": "Maidchan.Workflow.EndNode, Maidchan.Workflow"
        }
    ]
}
