if (!this.ULTIMA) {
    this.ULTIMA = {};
}

(function() {

   ULTIMA.renderManager = {

// The render manager handles rendering things to the screen.

     renderChunkTerrain : function(context, chunkId, x, y) {

// Render just the flat part of a chunk into the context, at the given
// screen coordinates. TODO: Ignore non-tile shapes.

       var cx, cy;
       var tx, ty;
       var row;
       var shape;

       var template = ULTIMA.cacheManager.loadChunkTemplate(chunkId);
       if (template) {
         cy = y;
         for (ty = 0; ty < template.length; ty++) {
           row = template[ty];
           cx = x;
           for (tx = 0; tx < row.length; tx++) {
             shape = ULTIMA.cacheManager.loadShape(row[tx].s, row[tx].f);
             if (!shape.complete && !shape.seen) {
               shape.seen = true; // next time we get here, just go on.
               shape.onload = renderFunc;
             } else {
               context.drawImage(shape, cx, cy);
             }

             cx = cx + 8;
           }

           cy = cy + 8;
         }
       }
     }

   };
}());