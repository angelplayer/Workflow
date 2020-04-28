(function () {
  var workflow = {
    $el: false,
    width: 1200,
    height: 900,
    data: [],
    loaded: false,

    init: function (settings) {
      if (!settings.el) {
        throw "Element is not define";
      }

      this.$el = document.getElementById(settings.el);
      if (!this.$el) {
        throw "Element is not found";
      }

      if (settings.width) {
        this.width = settings.width;
      }

      if (settings.height) {
        this.height = settings.height;
      }
    },

    load: function () {
      fetch('/Graph/workflow/Workflow2')
        .then(res => res.json())
        .then((data) => {
          this.loaded = true;
          this.data = data;
        }).then(() => {
          this.view();
        })
    },

    view: function () {
      if (!this.loaded) return;

      var g = new dagreD3.graphlib.Graph().setGraph({});
      var render = new dagreD3.render();

      // this.makeNode(g, 'start');
      
      // ['task1', 'task2'].forEach((name) => {
      //   this.makeNode(g, 'task', name);
      // });

      // this.data.forEach((name) => {
      //   this.makeNode(g, 'task', name);
      // });

      this.data.Nodes.forEach(node => {
        this.makeNode(g, node.kind, node.Id, node.description, node.Props);
      });

      this.data.edges.forEach((edge) => {
        this.connect(g, edge.from, edge.to);
      });

      // this.makeNode(g, 'end');
      // this.connect(g, 'start', 'task1');
      // this.connect(g, 'task1', 'task2');
      // this.connect(g, 'task2', 'end');

      var svg = d3.select("svg"),
        inner = svg.select("g");

      // Set up zoom support
      var zoom = d3.zoom().on("zoom", function () {
        inner.attr("transform", d3.event.transform);
      });
      svg.call(zoom);

      // Run the renderer. This is what draws the final graph.
      render(inner, g);

      // Add tooltips generate function
      var styleTooltip = function (desc, props) {
        // return "<p>" + desc +"</p>"
        return "<div>" + props.map((val) => {
          return "<div>" + Object.keys(val).map((field) => field + " : " + val[field]) + "</div>";
        }) + "</div>";
      }

      // Add tooltops 
      inner.selectAll("g.node")
        .attr("title", function (v) { return styleTooltip(g.node(v).description, g.node(v).props) })
        .each(function (v) { $(this).tipsy({ gravity: "w", opacity: 1, html: true }); });

      // Center the graph
      var initialScale = 0.75;
      svg.call(zoom.transform, d3.zoomIdentity.translate((svg.attr("width") - g.graph().width * initialScale) / 2, 20).scale(initialScale));

      //svg.attr('height', g.graph().height * initialScale + 40);
      svg.attr('height', this.height);
    },

    makeNode: function (g, kind, name, description, props) {
      if (kind === 'start') {
        g.setNode(kind, { shape: "circle", style: "fill: #afa" });
      } else if (kind === 'end') {
        g.setNode(kind, { shape: "circle", style: "fill: #d15858" });
      }
      else if (kind === 'condiction') {
        g.setNode(name, { shape: "diamond" });
      } else if (kind === 'task') {
        g.setNode(name, { shape: "rect", rx: 5, ry: 5, description: description, props: props});
      }
    },
    connect: function (g, from, to, label) {
      g.setEdge(from, to, {
        //style: ''
        arrowhead: 'vee',
        label: label ?? ''
      });
    }
  };



  workflow.init({
    el: 'workflow'
  });

  let button = document.getElementById("button");
  button.onclick = function () {
    workflow.load();
  }
})();