function CreateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

var PanelProps = function (el, network) {
    this.network = network;


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
    this.$el.appendChild(editButton);

    ['Start', 'Task', 'Decision', 'End'].forEach((value) => {
        let button = document.createElement('button');
        button.textContent = value;
        button.addEventListener('click', () => { this.selectAction(value) });
        this.$el.append(button);
    });
}


var Workflow = function (settings) {

    // create an array with nodes
    var nodes = new vis.DataSet([]);

    // create an array with edges
    var edges = new vis.DataSet([]);

    this.createNode = function (name) {
        this.state.select(name);
    }

    this.$panel = {};

    this.data = {
        nodes: nodes,
        edges: edges
    };

    this.options = {
        interaction: {
            hover: true,
            hoverConnectedEdges: false,
        },
        manipulation: {
            initiallyActive: true,
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

    this.$container = settings.el ? document.getElementById(settings.el) : document.getElementById('workflow');

    if (!this.$container) {
        throw "Workflow container is not found";
    };

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

    this.init = function () {
        var network = new vis.Network(this.$container, this.data, this.options);
        this.$panel = new PanelProps('panel', network);
        this.$panel.init();

        network.on('click', (params) => {
            if (this.$panel.getLastAction() == 'selected') {
                let id = CreateUUID();
                let node = this.makeNode(this.$panel.action.name, id, 'New ' + this.$panel.action.name, params.pointer.canvas.x, params.pointer.canvas.y);
                this.addNode(node);
            }
            this.$panel.clearAction();
        });
    }

    this.init();
}
