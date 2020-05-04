(function () {

  var workflow = new WorkflowEditor({ el: 'workflow' });

  workflow.on('load', (wf) => {
    fetch('/Graph/workflow/Workflow2')
      .then(res => res.json())
      .then((data) => {
        workflow.setData(data);
      });
  });

  workflow.init();
})()