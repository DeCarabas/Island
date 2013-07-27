window.onload = function() {
  var canvas = document.getElementById("surface");
  var context = canvas.getContext("2d");

  var renderMan = ULTIMA.renderManager;

  var chunkId = 17; // HACKS



  var render = function renderFunc() {
    context.clearRect(0, 0, canvas.width, canvas.height);
    renderMan.renderChunkTerrain(context, chunkId, 0, 0);
  };

  render();
};
