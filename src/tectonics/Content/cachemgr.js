if (!this.ULTIMA) {
    this.ULTIMA = {};
}

(function() {
   var imageCache = [];
   var templateCache = [];

  ULTIMA.cacheManager = {

// The cache manager handles caching various resources we pull from
// the server.

    loadShape : function (shape, frame) {

// loadShape begins to load the given shape and frame into an Image
// object, which is returned, whether or not it is completely
// loaded. The standard way of handling this is to subscribe to
// .onload.

      var shapeFrames = imageCache[shape];
      if (!shapeFrames) {
        shapeFrames = [];
        imageCache[shape] = shapeFrames;
      }

      var frameObj = shapeFrames[frame];
      if (!frameObj) {
        frameObj = new Image();
        frameObj.src = "u7/shapes/" + String(shape) + "/" + String(frame) + ".png";

        shapeFrames[frame] = frameObj;
      }

      return frameObj;
    },

    loadChunkTemplate : function(chunkTemplate, callback) {

// loadChunkTemplate loads a chunk template, which consists of an
// array of array of records with a shape and a frame number. The
// provided callback is called when the template has been loaded,
// either with the loaded template, or with 'undefined'.

      var template = templateCache[chunkTemplate];
      if (!template) {
        var client = new XMLHttpRequest();
        client.onreadystatechange = function() {
          if (this.readyState == 4) {
            if (this.status == 200) {
              template = JSON.parse(this.responseText);
              templateCache[chunkTemplate] = template;
              callback(template);
            } else {
              // TODO: ERRORS?
              callback();
            }
          }
        };
        client.open("GET","u7/map/templates/"+chunkTemplate+".json",true);
        client.send();
      }
      return template;
    }
  };
}());