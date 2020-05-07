function CreateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function SelectionStepType() {
    let div = document.createElement('div');
    div.className = 'popover';
    document.querySelector('body').appendChild(div);

    fetch('/Graph/workflow/allsteptype').then(res => res.json()).then((data) => {
        Array.from(data).forEach((item) => {
            let button = document.createElement('button');
            button.textContent = item.stepType;
            div.append(button);

            button.click = (evt) => { };
        });
    });
}

function createWidget(key, prop, updateCallback) {
    let c = null;
    if (prop.datatype === 'Text') {
        c = document.createElement('input');
        c.type = 'text';
        c.className = 'c-group-control';
        c.value = prop.value;
        c.onchange = (evt) => {
            updateCallback('node', prop, evt.target.value);
        }
    } else if (prop.datatype === 'Boolean') {
        c = document.createElement('input');
        c.type = 'checkbox';
        c.className = 'c-group-control'
        c.checked = prop.value == "true";
        c.onclick = (evt) => {
            updateCallback('node', prop, evt.target.checked ? 'true' : 'false');
        };
     }
    else {
        c = document.createElement('label');
        c.className = 'c-group-control';
        c.textContent = prop;
    }

    let label = document.createElement('label');
    label.textContent = prop.name ?? key;
    label.className = 'c-group-label';

    let span = document.createElement('span');
    span.className = 'sub';
    span.textContent = prop.help ?? '';

    let div = document.createElement('div');
    div.className = 'c-group';
    div.appendChild(label);
    div.appendChild(c);
    div.appendChild(span);

    return div;
}

function makeControl(param) {
    let control;
    let label = document.createElement('label');
    label.textContent = param.key;
    if (param.kind == 'Task') {
        control = document.createElement('input');
        control.type = 'text';
        control.value = param.value;
    }

    const div = document.createElement('div');
    div.appendChild(label)
    div.appendChild(control);
    return div;
}

var PanelProps = function (el, network) {
    this.network = network;

    this.emitChangeNodeEvent = [];
    this.emitChangeEdgeEvent = [];

    this.action = {
        name: '',
        last: null
    };

    this.getLastAction = function () {
        return this.action.last;
    }

    this.clearAction = function () {
        this.action.last = null;
        this.action.name = null;
    }

    this.selectAction = function (name) {
        this.action.last = 'selected'
        this.action.name = name;

        // Disable add edge mode
        if (this.network.manipulation.inMode === 'addEdge') {
            this.network.disableEditMode();
        }
    }

    this.editEdge = function () {
        this.action.last = 'connected';
        this.action.name = '';

        if (this.network.manipulation.inMode === 'addEdge') {
            this.network.disableEditMode();
        } else {
            this.network.addEdgeMode();
        }
    }

    this.$el = document.getElementById(el);
}

PanelProps.prototype.init = function () {

    let editButton = document.createElement('button');
    editButton.textContent = 'Connect';
    editButton.addEventListener('click', () => {
        this.editEdge();
    });
    editButton.className = 'button-node';
    this.$el.appendChild(editButton);

    ['Start', 'Task', 'Decision', 'End'].forEach((value) => {
        let button = document.createElement('button');
        button.textContent = value;
        button.className = 'button-node';
        button.addEventListener('click', () => { this.selectAction(value) });
        this.$el.append(button);
    });
    let props = document.createElement('div');
    props.id = 'props'
    this.$el.append(props);
}

PanelProps.prototype.emitChangeNode = function (id, key, value) {
    this.emitChangeNodeEvent.forEach(cb => cb(id, key, value));
}

PanelProps.prototype.emitChangeEdge = function (id, key, value) {
    this.emitChangeEdgeEvent.forEach(cb => cb(id, key, value));
}

PanelProps.prototype.on = function(eventName, callback) {
    if (eventName === 'emitChangeNode') {
        this.emitChangeNodeEvent.push(callback);
    } else if (eventName === 'emitChangeEdge') {
        this.emitChangeEdgeEvent.push(callback);
    }
}

PanelProps.prototype.draw = function (graph) {
    const element = document.getElementById('props');
    element.innerHTML = '';
    element.className = 'ranka';

    if (!graph) return; // if no graph provide just clear the UI

    let goupHeader = document.createElement('div');
    goupHeader.className = "c-group";
    goupHeader.style = 'border-bottom: 1px solid white';
    let labelHeader = document.createElement('label');
    labelHeader.textContent = "Name";
    labelHeader.className = 'c-group-label';
    let header = document.createElement('input');
    header.className = 'c-group-control';
    header.type = 'text';
    header.value = graph.label ?? '';
    goupHeader.append(labelHeader);
    goupHeader.append(header)

    header.onchange = (evt) => {
        if ((graph.to || graph.from)) {
            this.emitChangeEdge(graph.id, 'label', evt.target.value);
        } else {
            this.emitChangeNode(graph.id, 'label', evt.target.value);
        }
    }

    let labelSelector = document.createElement('label');
    labelSelector.textContent = "Action";
    labelSelector.className = 'c-group-label';
    let taskSelector = document.createElement('label');
    taskSelector.className = 'c-group-control';
    taskSelector.textContent = graph.stepType;
    let groupSelector = document.createElement('div');
    groupSelector.className = 'c-group';

    // let selectorButton = document.createElement('button');
    // selectorButton.textContent = "Selection";
    // selectorButton.onclick = (evt) => {
    //     SelectionStepType();
    // }
    groupSelector.append(labelSelector);
    groupSelector.append(taskSelector);
    // groupSelector.append(selectorButton);
    


    let p = document.createElement('p');
    p.className = 'subtitle';
    p.textContent = graph.description ?? '';

    element.append(goupHeader);
    element.append(groupSelector);
    element.append(p);

    let propsContainer = document.createElement('div');

    let params = graph.Props;
    if (!params && (graph.to || graph.from)) {
        params = { from: graph.from, to: graph.to };
    }

    if (!params) return;

    Object.keys(params).map((k) => {
        propsContainer.appendChild(createWidget(k, params[k], (type, key, val) =>
        {
            if (type == 'node') { 
                params[k].value = val;
                this.emitChangeEdge(graph.id, 'Props', params);
            }
        }));
    });
    element.append(propsContainer);
}


var Workflow = function (settings) {
    this.createNode = function (name) {
        this.state.select(name);
    }

    this.$panel = {};

    this.data = {
        nodes: new vis.DataSet([]),
        edges: new vis.DataSet([])
     };

    this.options = {
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
    };

    this.connectorUlr = settings.connector ?? '';

    this.$container = settings.el ? document.getElementById(settings.el) : document.getElementById('workflow');

    if (!this.$container) {
        throw "Workflow container is not found";
    };

    this.loadData = (callback) => {
        if (this.connect && this.connectorUlr !== '') {
            fetch(this.connectorUlr)
                .then(res => res.json())
                .then((data) => {
                    callback({ nodes: data.nodes, edges: data.edges});
                })
                .catch(err => console.log(err));
        }
    }

    this.addNode = function (node) {
        try {
            this.data.nodes.add(node);
        } catch (err) {
            console.log(err);
        }
    }

    this.connect = function (id, from, to) {
        try {
            this.data.edges.add({ id: id, from: from, to: to });
        } catch (err) {
            console.log(err);
        }
    }

    this.getNode = function (id) {
        try {
            return this.data.nodes.get(id);
        } catch (err) {
            console.log(err);
            return null;
        }
    }

    this.getEdge = function (id) {
        try {
            return this.data.edges.get(id);
        } catch (err) {
            console.log(err);
        }
    }

    this.removeNode = function (id) {
        try {
            this.data.nodes.remove({ id: id });
        } catch (err) {
            console.log(err);
        }
    }

    this.removeEdge = function (id) {
        try {
            this.data.edges.remove({ id: id });
        } catch (err) {
            console.log(err);
        }
    }


    this.makeNode = function (kind, id, label, x, y) {
        let newNode = { id: id, label: label, x: x, y: y };
        switch (kind) {
            case 'Task': newNode.shape = 'box'; break;
            case 'Decision': newNode.shape = 'diamond'; break;
        }

        return newNode;
    }


    this.updateNode = function (id, key, value) {
        try {
            this.data.nodes.update({
                [key]: value,
                id: id
            });
        } catch (err) {
            console.log(err);
        }
    }

    this.updateEdge = function (id, key, value) {
        try {
            this.data.edges.update({
                [key]: value,
                id: id
            });
        } catch (err) {
            console.log(err);
        }
    }

    this.init = function () {
        var network = new vis.Network(this.$container, this.data, this.options);
        this.$panel = new PanelProps('panel', network);
        this.$panel.init();

        this.$panel.on('emitChangeNode', (id, key, value) => {
            this.updateNode(id, key, value);
        });

        this.$panel.on('emitChangeEdge', (id, key, value) => {
            this.updateEdge(id, key, value);
        })

        this.loadData((data) => {
            this.data = {
                nodes: new vis.DataSet(data.nodes),
                edges: new vis.DataSet(data.edges)
            }

            network.setData(this.data);
        });

        network.on('click', (params) => {
            if (this.$panel.getLastAction() == 'selected') {
                let id = CreateUUID();
                let node = this.makeNode(this.$panel.action.name, id, 'New ' + this.$panel.action.name, params.pointer.canvas.x, params.pointer.canvas.y);
                this.addNode(node);
            }
            this.$panel.clearAction();
        });

        network.on('selectNode', (params) => {
            let selected = network.getSelectedNodes();
            let node = this.getNode(selected[0]);
            this.$panel.draw(node);
        });

        network.on('selectEdge', (params) => {
            let selected = network.getSelectedEdges();
            let edge = this.getEdge(selected[0]);
            this.$panel.draw(edge);
        });

        this.$container.addEventListener('keyup', (evt) => {
            if (evt.key === 'Delete') {
                network.deleteSelected();
                this.$panel.draw();
            };
        });
    }

    this.init();
}
