(function () {
  var workflow = {
    $el: false,
    width: 1200,
    height: 900,

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

    view: function () {

      var g = new dagreD3.graphlib.Graph().setGraph({});
      var render = new dagreD3.render();

      this.makeNode(g, 'start');
      ['task1', 'task2'].forEach((name) => {
        this.makeNode(g, 'task', name);
      });

      this.makeNode(g, 'end');
      this.connect(g, 'start', 'task1');
      this.connect(g, 'task1', 'task2');
      this.connect(g, 'task2', 'end');

      var svg = d3.select("svg"),
        inner = svg.select("g");

      // Set up zoom support
      var zoom = d3.zoom().on("zoom", function () {
        inner.attr("transform", d3.event.transform);
      });
      svg.call(zoom);

      // Run the renderer. This is what draws the final graph.
      render(inner, g);

      // Center the graph
      var initialScale = 0.75;
      svg.call(zoom.transform, d3.zoomIdentity.translate((svg.attr("width") - g.graph().width * initialScale) / 2, 20).scale(initialScale));

      //svg.attr('height', g.graph().height * initialScale + 40);
      svg.attr('height', this.height);
    },

    makeNode: function (g, kind, name) {
      if (kind === 'start') {
        g.setNode(kind, { shape: "circle", style: "fill: #afa" });
      } else if (kind === 'end') {
        g.setNode(kind, { shape: "circle", style: "fill: #d15858" });
      }
      else if (kind === 'condiction') {
        g.setNode(name, { shape: "diamond" });
      } else if (kind === 'task') {
        g.setNode(name, { shape: "rect", rx: 5, ry: 5 });
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

  workflow.view();

})();