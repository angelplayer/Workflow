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

    const vue = new Vue({
        el: '#editor',
        data: () => {
            return {

                text: 'hello',
                steplist: [],
                currentSelection: {
                    index: -1, kind: ''
                },
                connectEventCallback: []
            }
        },

        mounted: function() {
            this.$nextTick(() => {

                fetch('/Graph/workflow/allsteptype').then(res => res.json()).then((data) => {
                    console.log('done loading...');
                    this.$set(this, 'steplist', Array.from(data));
                })

            })
        },

        methods: {
            onchange: function (selectedIndex, actionKind) {
                if (actionKind === 'Connect') {
                    this.resetIndex();

                    // Publish event Connect
                    Array.from(this.connectEventCallback).forEach(cb => cb());
                    return;
                }

                this.$set(this, 'currentSelection', {index: selectedIndex, kind: actionKind});
            },

            onConnectEvent: function (callback) {
                this.connectEventCallback.push(callback);
            },

            resetIndex: function () {
                this.$set(this, 'currentSelection', {index: -1, kind: ''});
            }
        },

        computed: {
            selectAction: function () {
                return this.currentSelection.kind;
            },
            getAction: function () {
                return this.currentSelection.index > -1 ? this.steplist[this.currentSelection.index] : {} ;
                
            }

        }
    });


    const workflow = {
        init: (settings) => {
            let container = document.getElementById(settings.el);
            
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
            let network = new vis.Network(container, data, options);


            // Methods
            const addNode = (newNode) => {
                try {
                    data.nodes.add(newNode);
                } catch (err) {
                    console.log(err);
                }
            }


            // Events
            vue.onConnectEvent(() => {
                if (network.manipulation.inMode === 'addEdge') {
                    network.disableEditMode();
                } else {
                    network.addEdgeMode();
                }
            });

            container.addEventListener('keyup', (evt) => {
                if (evt.key === 'Delete') {
                    network.deleteSelected();
                };
            });

            network.on('selectNode', (mouse) => {

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
                        addNode(node);
                    } else if (kind === 'Decision') {
                        node.shape = 'diamond';
                        node.label = kind;
                        addNode(node);
                    } else {
                        // Ignore other type
                    }
                    
                };

            network.on('')

                vue.resetIndex();
            });
        },
    }

    workflow.init({
        el: 'workflow'
    });
})();