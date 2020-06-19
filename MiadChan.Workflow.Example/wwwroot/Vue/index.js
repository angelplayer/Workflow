(function () {
Vue.config.devtools = true;
Vue.component('ActionItem', {
  props: ['action', 'index'],
  template: `
    <li class="action-item">
        <button @click="buttonClick">{{action}}</button>
    </li>
  `,
  methods: {
    buttonClick: function (evt) {
      this.$emit('selected', this.index);
    }
  }
});

Vue.component('ActionList', {
  props: ['items'],
  data: function () {
    return {
      defaultAction: [
        { action: 'Connect' },
        { action: 'Decision'}
      ]
    }
  },
  template: `
    <ul class="action-list">
      <action-item v-for="(item, index) in defaultAction"
        v-bind:key="item+index"
        v-bind:action="item.action"
        v-bind:index="index"
        v-on:selected="onOtherSelected"
      />
      <action-item 
        v-for="(item, index) in items" 
        v-bind:key="item.stepType" 
        v-bind:action="item.stepType"
        v-bind:index="index"
        v-on:selected="onSelected"
      />
    </ul>
  `,
  methods: {
    onSelected: function (selectedIndex) {
      this.$emit('change', selectedIndex, "stepType");
    },
    onOtherSelected: function (selectedIndex) {
      this.$emit('change', selectedIndex, this.defaultAction[selectedIndex].action);
    }
  }
});


Vue.component('ActionParam', {
  props: ['param', 'value'],
  template: `
    <div>
        <label>{{param.name}}</label>
        <input v-if="inputType === 'textbox'" v-bind:type="inputType" v-on:change="updateState" v-bind:value="value"/>
        <input v-else-if="inputType === 'checkbox'" v-bind:type="inputType" v-on:click="checkboxupdateState" v-bind:checked="getCheckboxChecked"/>
        <div>
            <span>{{param.help}}</span>
        </div>
    </div>
  `,
  methods: {
    updateState: function (evt) {
        this.$emit('update', "Props."+this.param.name, evt);
    },
    checkboxupdateState: function (evt) {
        this.$emit('update', "Props." + this.param.name, { target: { value: evt.target.checked ? 'true': 'false' } });
    }
  },
  computed: {
    inputType: function () {
        if (this.param.datatype == 'Text') return 'textbox';
        else if (this.param.datatype == 'Boolean') return 'checkbox';
        else return "";
    },getCheckboxChecked: function() {
        return this.value === 'true'; 
    }
  }
});

Vue.component('ActionProps', {
  props: ['graph', 'params'],
  data: function () {
    return {
    }
  },
  data: function () {
    return {
        headerEdit: false
    }
  },
  template: `
    <div>
        <h1 v-if="headerEdit"><input v-bind:value="graph.label" v-on:focusout="labelFocusOut" /></h1>
        <h1 v-else v-on:dblclick="labelDoubleClick" >{{graph.label}}</h1>
        <p>{{params.description}}</p>
        <div>
            <ActionParam v-for="(item, index) in getParamProps" 
                v-bind:key="index+item" 
                v-bind:param="item"
                v-bind:value="getParamVal[item.name]"
                v-on:update="emitUpdate"/>
        </div>
    </div>
  `,
  methods: {
    labelDoubleClick: function (evt) {
      this.$set(this, 'headerEdit', true);
    }, labelFocusOut: function (evt) {
      this.$set(this, 'headerEdit', false);
      this.emitUpdate('label', evt);
    },
    emitUpdate: function (key, evt) {
      this.$emit('update',this.graph.id, key, evt.target.value);
    }
  },
  computed: {
    getParamProps: function () {
        return this.params.Props;
    },
    getParamVal: function () {
        return this.graph.Props ? this.graph.Props: []; 
    }
}
});


const vue = new Vue({
  el: '#editor',
  data: () => {
    return {
      text: 'hello',
      steplist: [],
      currentSelection: {
          index: -1, kind: ''
      },
      connectEventCallback: [],
      graphUpdateEventCallback: [],
      graphLoadedEventCallback: [],
      commitGraphCallback: [],
      graph: {},
      currentViewWorkflow: {},
      workflowNames: [],
    }
  },
  mounted: function() {
    this.$nextTick(() => {
      fetch('/Graph/workflow/allsteptype').then(res => res.json()).then((data) => {
          this.$set(this, 'steplist', Array.from(data));
      });
      // fetch('/Graph/workflow').then(res => res.json()).then((data) => {
      //     this.$set(this, 'workflowNames', data);
      // });
    })
  },

  methods: {
      onchange: function (selectedIndex, actionKind) {
        if (actionKind === 'Connect') {
            this.resetIndex();
            Array.from(this.connectEventCallback).forEach(cb => cb());
            return;
        }
        this.$set(this, 'currentSelection', {index: selectedIndex, kind: actionKind});
      },
      onUpdateGraphEvent: function (callback) {
          this.graphUpdateEventCallback.push(callback);
      },
      updateGraphState: function (id, key, value) {
        Array.from(this.graphUpdateEventCallback).forEach(cb => cb(id, key, value));
      },
      onConnectEvent: function (callback) {
        this.connectEventCallback.push(callback);
      },
      onGraphLoadedEvent: function (callback) {
        this.graphLoadedEventCallback.push(callback);
      },
      graphLoaded: function () {
        Array.from(this.graphLoadedEventCallback).forEach(cb => cb());
      },
      onCommitEvent: function (callback) {
        this.commitGraphCallback.push(callback);
      },
      resetIndex: function () {
        this.$set(this, 'currentSelection', {index: -1, kind: ''});
      },
      selectGraph: function (graph) {
        this.$set(this, 'graph', graph);
      },
      chooseWorkflow: function (workflowName) {
        if (!workflowName || workflowName === '') return;

        fetch(`/Graph/workflow/${workflowName}`).then(res => res.json()).then((data) => {
            this.$set(this, 'currentViewWorkflow', data);
            this.graphLoaded();
        });
    },
    newWorkflow: function (workflowName) {
      if (!workflowName || workflowName === '') return;
      this.$set(this, 'currentViewWorkflow', {
        id: workflowName,
        nodes: [],
        edges: [],
        version: 1
      });
      this.graphLoaded();
    },
    commitGraphToServer: function (jsonData) {
      jsonData.id = this.currentViewWorkflow.id;
      jsonData.version = this.currentViewWorkflow.version;

      fetch(`/Graph/workflow/${this.currentViewWorkflow.id}`, {
        method: 'PUT',
        body: JSON.stringify(jsonData),
        headers: { "content-type": "Application/json" }
      }).then(res => res.json())
        .then((data) => this.$set(this, "defintions", data))
        .catch(e => console.log(e));
    },
    commitGraph: function () {
      if (this.currentViewWorkflow) {
        Array.from(this.commitGraphCallback).forEach(cb => cb());
      }
    }
  },
  computed: {
    selectAction: function () {
      return this.currentSelection.kind;
    },
    getAction: function () {
      return this.currentSelection.index > -1 ? this.steplist[this.currentSelection.index] : {} ;
    },
    stepParams: function () {
      if (!this.graph.stepType || this.graph.stepType == 'Dicision') return {};       
      return Array.from(this.steplist).filter(x => x.stepType === this.graph.stepType)[0];
    },
    getCurrentWorkflow: function () {
      return this.currentViewWorkflow;
    }
  }
});

const workflow = {
  init: (settings) => {
    let options = {
      interaction: {
        hover: true,
        hoverConnectedEdges: false,
        selectConnectedEdges: false,
      },
      manipulation: {
        initiallyActive: false,
        enabled: false,
        addNode: false,
        addEdge: true,
        deleteNode: false,
        deleteEdge: false,
      },
      physics: {
        enabled: false
      },
      edges: {
        arrows: {
          to: { type: 'triangle', enabled: true }
        },
        color: '#242020',
        smooth: {
          type: 'horizontal',
          roundness: 0
        },
      },
      nodes: {
          shape: 'box',
          margin: {
            top: 15, bottom: 15, right: 10, left: 10
          },
          borderWidth: 0.3,
          borderWidthSelected: 1,
          color: {
            background: '#5bb8ab',
            hover: {
              background: "#95c7c0",
          }
        }
      }
    }

  const data = {
      nodes: new vis.DataSet([]),
      edges: new vis.DataSet([])
  }
    
  let container = document.getElementById(settings.el);
  let network = new vis.Network(container, data, options);
    
  // Methods
  const addNode = (newNode) => {
    try {
      data.nodes.add(newNode);
    } catch (err) {
      console.log(err);
    }
  }
  const getNode = (id) => {
    try {
      return data.nodes.get(id);
    } catch (err) {
      console.log(err);
      return null;
    }
  }

  const updateNode = (id, key, value) => {
    try {
      data.nodes.update({
        [key]: value,
        id: id
      });
    } catch (err) {
      console.log(err);
    }
  }

  const addEdge = (edge) => {
    try {
      data.edges.add(edge);
    } catch (err) {
      console.log(err);
    }
  }
    
  // Events
  container.addEventListener('keyup', (evt) => {
    if (evt.key === 'Delete') {
      network.deleteSelected();
    };
  });

  vue.onConnectEvent(() => {
    if (network.manipulation.inMode === 'addEdge') {
      network.disableEditMode();
    } else {
      network.addEdgeMode();
    }
  });

  vue.onUpdateGraphEvent((id, key, value) => {
    if (key.startsWith('Props')) {
      const subkey = key.substring(6);
      const oldVal = getNode(id).Props;
      value = {
        ...oldVal,
        [subkey]: value,
      }
      key = "Props";
      updateNode(id, key, value)
    } else updateNode(id, key, value);
    vue.selectGraph(getNode(id));
});

  vue.onGraphLoadedEvent((json) => {
    if (vue.getCurrentWorkflow) {
      Array.from(vue.getCurrentWorkflow.nodes).forEach((n) => {
        let node = { id: n.id, ...n };
        if (n.kind === 'stepType') {
            node.shape = 'box';
        } else if (n.kind === 'Decision') {
            node.shape = 'diamond';
        }
        addNode(node);
      });
      Array.from(vue.getCurrentWorkflow.edges).forEach((e) => {
          // TODO: should add label to edge here
          addEdge(e);
      });
    }
  });

  vue.onCommitEvent(() => {
    vue.commitGraphToServer({
      nodes: data.nodes.map(x => x),
      edges: data.edges.map(x => x)
    });
  });
  
  network.on('selectNode', (mouse) => {
    let selected = network.getSelectedNodes();
    let node = getNode(selected[0]);
    vue.selectGraph(node);
  });

  network.on('selectEdge', (mouse) => {
      
  });

  network.on('click', (mouse) => {
    if (vue.selectAction !== '') {
      let node = { x: mouse.pointer.canvas.x, y: mouse.pointer.canvas.y }
      let kind = vue.selectAction;
      if (kind === 'stepType') {
          node.shape = 'box';
          node.label = vue.getAction.stepType;
          node.stepType = vue.getAction.stepType;
          addNode(node);
      } else if (kind === 'Decision') {
          node.shape = 'diamond';
          node.label = kind;
          addNode(node);
      } else {
          // Ignore other type
      }   
    };
    vue.resetIndex();
  });
}}

  workflow.init({
      el: 'workflow'
  });
    
  /// Choose workflow 
  function EditWorkflow(workflowName) {
    vue.chooseWorkflow(workflowName);
  }

  /// create new workflow
  function AddNewWorkflow(workflowName) {
    vue.newWorkflow(workflowName);
  }
})();