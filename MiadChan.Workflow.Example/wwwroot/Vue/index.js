(function () {
    
    Vue.config.devtools = true;

    Vue.component('ActionItem', {
        props: ['action', 'index'],
        template: `
            <li class="action-item">
                <button @click="buttonClick">{{action.stepType}}</button>
            </li>
        `,

        methods: {
            buttonClick: function (evt) {
                this.$emit('selected', this.index);
            }
        }
    });

    Vue.component('ActionList',  {
        props: ['items'],
        template: `
            <ul class="action-list">
                <action-item 
                    v-for="(item, index) in items" 
                    v-bind:key="item.stepType" 
                    v-bind:action="item"
                    v-bind:index="index"
                    v-on:selected="onSelected"
                />
            </ul>
        `,
        methods: {
            onSelected: function (selectedIndex) {
                this.$emit('change', selectedIndex);
            }
        }
    });


    const vue = new Vue({
        el: '#editor',
        data: () => {
            return {

                text: 'hello',
                steplist: [],
                currentSelectedIndex: -1
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
            onchange: function(selectedIndex) {
                this.$set(this, 'currentSelectedIndex', selectedIndex);
            },

            resetIndex: function () {
                this.$set(this, 'currentSelectedIndex', -1);
            }
        },

        computed: {
            selectAction: function () {
                return this.currentSelectedIndex > -1 ? this.steplist[this.currentSelectedIndex] : '';
            }
        }
    });

    let container = document.getElementById('workflow');
    let options = {}
    const data = {
        nodes: new vis.DataSet([]),
        edges: new vis.DataSet([])
    }
    let network = new vis.Network(container, data, options);

    network.on('click', () => {
        if (vue.selectAction !== '') {
            try {
                let newNode = {shape: 'box', id: 1, label: vue.selectAction.stepType};
                data.nodes.add(newNode);
            } catch (err) {
                console.log(err);
            }
        } 

        vue.resetIndex();
    });

})();