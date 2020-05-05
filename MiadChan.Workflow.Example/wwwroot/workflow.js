
function WorkflowEditor(settings) {
  this.settings = settings;
  this.width = 1200;
  this.height = 900;
  this.data = {};
  this.loaded = false;
  this.$el = undefined;

  this.selectedNodeName = null;
  this.propsContainer = null;

  this.onload = [];
}

WorkflowEditor.prototype.init = function () {
  if (!this.settings.el) {
    throw "Elements is not defined";
  }

  this.$el = document.getElementById(this.settings.el);
  if (!this.$el) {
    throw "Element is not found";
  }

  if (this.settings.width) {
    this.width = this.settings.width;
  }

  if (this.settings.height) {
    this.height = this.settings.height;
  }

  let div = document.createElement('div');
  div.className = "sidebar";
  div.style.width = "300px";
  div.style.height = "100%";

  let p = document.createElement('p');
  p.textContent = "Properties";
  div.appendChild(p);

  let propsDiv = document.createElement('div');
  propsDiv.className = "props-container";
  propsDiv.id = "propsDiv";
  div.appendChild(propsDiv);

  this.$el.appendChild(div);
  this.propsContainer = propsDiv;


  this.dispatch('load');
}


WorkflowEditor.prototype.draw = function () {
  // if (!this.loaded) return;

  var g = new dagreD3.graphlib.Graph().setGraph({});
  var render = new dagreD3.render();

  var svg = d3.select("svg"),
    inner = svg.select("g");


  this.data.Nodes.forEach(node => {
    this.makeNode(g, node.kind, node.Id, node.description, node.Props);
  });

  this.data.edges.forEach((edge) => {
    this.connect(g, edge.from, edge.to);
  });


  // Set up zoom support
  var zoom = d3.zoom().on("zoom", function () {
    inner.attr("transform", d3.event.transform);
  });
  svg.call(zoom);

  // Run the renderer. This is what draws the final graph.
  render(inner, g);

  inner.selectAll('g.node').on('click', (nodeName) => this.view(g.node(nodeName)));
  // Center the graph
  var initialScale = 0.75;
  svg.call(zoom.transform, d3.zoomIdentity.translate((svg.attr("width") - g.graph().width * initialScale) / 2, 20).scale(initialScale));


  svg.attr('height', this.height);
}

WorkflowEditor.prototype.makeNode = function (g, kind, name, description, props) {
  if (kind === 'start') {
    g.setNode(kind, { shape: "circle", style: "fill: #afa" });
  } else if (kind === 'end') {
    g.setNode(kind, { shape: "circle", style: "fill: #d15858" });
  }
  else if (kind === 'condiction') {
    g.setNode(name, { shape: "diamond" });
  } else if (kind === 'task') {
    g.setNode(name, { shape: "rect", rx: 5, ry: 5, description: description, props: props });
  }
}

WorkflowEditor.prototype.connect = function (g, from, to, label) {
  g.setEdge(from, to, {
    //style: ''
    arrowhead: 'vee',
    label: label ?? ''
  });
}

WorkflowEditor.prototype.on = function (eventName, callback) {
  if (typeof callback !== 'function') return;

  if (eventName === 'load') {
    this.onload.push(callback);
  }
}

WorkflowEditor.prototype.dispatch = function (eventName) {
  if (eventName === 'load') {
    this.onload.forEach((cb) => cb(this));
  } else if (eventName === 'refresh') {
    this.draw();
  }
}

WorkflowEditor.prototype.setData = function (data) {
  this.data = data;
  this.dispatch('refresh');
}

function createWidget(prop) {
  let c = null;
  if (prop.datatype === 'Text') {
    c = document.createElement('input');
    c.type = 'text';
  }

  let label = document.createElement('label');
  label.textContent = prop.name;

  let span = document.createElement('span');
  span.textContent = prop.help;

  let div = document.createElement('div');
  div.appendChild(c);
  div.appendChild(label);
  div.appendChild(span);

  return div;
}

WorkflowEditor.prototype.view = function (node) {
  this.propsContainer.innerHTML = '';
  var props = node.props;
  Object.keys(props).map((k) => {
    this.propsContainer.appendChild(createWidget(props[k]));
  })
}