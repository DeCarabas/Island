namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    
    public abstract class MapObject
    {
        MapPoint location;
        Rectangle? screenRect;

        public Rectangle ScreenRect 
        { 
            get 
            {
                if (this.screenRect == null)
                {
                    Point point = location.ScreenPoint;
                    this.screenRect = new Rectangle(
                        point.X - Frame.HotSpotX, 
                        point.Y - Frame.HotSpotY, 
                        Frame.Width, 
                        Frame.Height);
                }
                return this.screenRect.Value;
            } 
        }
        public Frame Frame { get; set; }
        public Image Image { get; set; }
        public MapPoint Location 
        {
            get 
            { 
                if (this.location == null) 
                { 
                    this.location = new MapPoint();
                    this.location.PropertyChanged += LocationPropertyChanged;
                } 
                return this.location; 
            } 
            set 
            {
                if (value == null) { throw new ArgumentNullException(); }
                if (this.location != null) { this.location.PropertyChanged -= LocationPropertyChanged; }
                this.location = value;
                this.location.PropertyChanged += LocationPropertyChanged;
            }
        }
        public Shape Shape { get { return Frame.Shape; } }

        // Returns: -1 if x < y, 0 if unknown, 1 if x > y
        // This code was swiped from Exult's Game_object::compare, and ported to our model.
        //
        // NOTE: We use properties VERY HEAVILY in this model, and that means that we sometimes end up calculating
        //       things way more often than we ought to. It may turn out to be a performance problem; pay attention!
        //
        static int Compare(MapObject x, MapObject y)
        {
            // See if there's no screen overlap.
            if (!x.ScreenRect.IntersectsWith(y.ScreenRect)) { return 0; }

            // TODO: These computations can be cached in the MapLocation, if needed to make things go faster.
            //
            var inf1 = new OrderingInfo(x);
            var inf2 = new OrderingInfo(y);

            // Comparisons for a given dimension:
            //   -1 if o1<o2, 0 if o1==o2, 1 if o1>o2.
            //
            int xcmp, ycmp, zcmp;
            bool xover, yover, zover;	// True if dim's overlap.

            CompareRanges(inf1.XLeft, inf1.XRight, inf2.XLeft, inf2.XRight, out xcmp, out xover);
            CompareRanges(inf1.YFar, inf1.YNear, inf2.YFar, inf2.YNear, out ycmp, out yover);
            CompareRanges(inf1.ZBottom, inf1.ZTop, inf2.ZBottom, inf2.ZTop, out zcmp, out zover);

            if ((xcmp == 0) && (ycmp == 0) && (zcmp == 0))
            {
                // Same space?
                // Paint biggest area sec. (Fixes plaque at Penumbra's.)
                //
                return (x.ScreenRect.Width < y.ScreenRect.Width && x.ScreenRect.Height < y.ScreenRect.Height) ? -1 :
                       (x.ScreenRect.Width > y.ScreenRect.Width && x.ScreenRect.Height > y.ScreenRect.Height) ? 1 : 0;
            }
            if (xover & yover & zover)	// Complete overlap?
            {
                if (x.Frame.TileSize.Z == 0) // Flat one is always drawn first.
                    return (y.Frame.TileSize.Z == 0) ? 0 : -1;
                else if (y.Frame.TileSize.Z == 0)
                    return 1;
            }
            if (xcmp >= 0 && ycmp >= 0 && zcmp >= 0) return 1;		// GTE in all dimensions.
            if (xcmp <= 0 && ycmp <= 0 && zcmp <= 0) return -1;		// LTE in all dimensions.

            if (yover)			// Y's overlap.
            {
                if (xover)		// X's too?
                    return zcmp;
                else if (zover)		// Y's and Z's?
                    return xcmp;
                // Just Y's overlap.
                else if (zcmp == 0)		// Z's equal?
                    return xcmp;
                else			// See if X and Z dirs. agree.
                    if (xcmp == zcmp)
                        return xcmp;

                    // Experiment:  Fixes Trinsic mayor
                    //   statue-through-roof.
                    //
                    else if (inf1.ZTop / 5 < inf2.ZBottom / 5 && y.Shape.Occludes)
                        return -1;	// A floor above/below.
                    else if (inf2.ZTop / 5 < inf1.ZBottom / 5 && x.Shape.Occludes)
                        return 1;
                    else
                        return 0;
            }
            else if (xover)			// X's overlap.
            {
                if (zover)		// X's and Z's?
                    return ycmp;
                else if (zcmp == 0)		// Z's equal?
                    return ycmp;
                else
                    return ycmp == zcmp ? ycmp : 0;
            }
            // Neither X nor Y overlap.
            else if (xcmp == -1)		// o1 X before o2 X?
            {
                if (ycmp == -1)		// o1 Y before o2 Y?
                    // If Z agrees or overlaps, it's LT.
                    return (zover || zcmp <= 0) ? -1 : 0;
            }
            else if (ycmp == 1)		// o1 Y after o2 Y?
            {
                if (zover || zcmp >= 0)
                    return 1;
                /* So far, this seems to work without causing problems: */
                // Experiment:  Fixes Brit. museum
                //   statue-through-roof.
                else if (inf1.ZTop / 5 < inf2.ZBottom / 5)
                    return -1;	// A floor above.
                else
                    return 0;
            }
            return 0;
        }

        /*
         *	Compare ranges along a given dimension.
         */
        static void CompareRanges
        (
            int from1, int to1,		// First object's range.
            int from2, int to2,		// Second object's range.

            // Returns:
            out int cmp,			// -1 if 1st < 2nd, 1 if 1st > 2nd, 0 if equal.
            out bool overlap	    // true returned if they overlap.
        )
        {
            if (to1 < from2)
            {
                overlap = false;
                cmp = -1;
            }
            else if (to2 < from1)
            {
                overlap = false;
                cmp = 1;
            }
            else				// X's overlap.
            {
                overlap = true;
                if (from1 < from2)
                {
                    cmp = -1;
                }
                else if (from1 > from2)
                {
                    cmp = 1;
                }
                else if (to1 - from1 < to2 - from2)
                {
                    cmp = 1;
                }
                else if (to1 - from1 > to2 - from2)
                {
                    cmp = -1;
                }
                else
                {
                    cmp = 0;
                }
            }
        }

        void LocationPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.screenRect = null;
        }

        static void SortGraph(MapGraphNode node, List<MapObject> sorted, HashSet<MapObject> visited)
        {
            if (visited.Contains(node.Object)) { return; }
            visited.Add(node.Object);
            foreach (MapGraphNode dependency in node.Dependencies)
            {
                SortGraph(dependency, sorted, visited);
            }
            sorted.Add(node.Object);
        }

        public static IList<MapObject> SortObjects(IList<MapObject> objects)
        {
            // I used to have a fast way of doing this for lots of objects, but not any more. (Where's that source
            // code when you need it?)
            //
            var graph = new List<MapGraphNode>();
            foreach (MapObject obj in objects)
            {
                var newNode = new MapGraphNode { Object = obj };
                foreach (MapGraphNode node in graph)
                {
                    int result = Compare(node.Object, obj);
                    if (result < 0)
                    {
                        newNode.Dependencies.Add(node);
                    }
                    else if (result > 0)
                    {
                        node.Dependencies.Add(newNode);
                    }
                }
                graph.Add(newNode);
            }

            HashSet<MapObject> visited = new HashSet<MapObject>();
            List<MapObject> sorted = new List<MapObject>();
            foreach (MapGraphNode node in graph)
            {
                SortGraph(node, sorted, visited);
            }
            return sorted;
        }

        struct OrderingInfo
        {
            int xleft, xright;
            int ynear, yfar;
            int ztop, zbottom;

            public OrderingInfo(MapObject mapObject)
            {
                int tx = mapObject.Location.TileX;
                int ty = mapObject.Location.TileY;
                int tz = mapObject.Location.Z;

                Size3D tileSize = mapObject.Frame.TileSize;

                xleft = tx - tileSize.X + 1;
                xright = tx;
                yfar = ty - tileSize.Y + 1;
                ynear = ty;
                ztop = tz + tileSize.Z - 1;
                zbottom = tz;
                if (tileSize.Z == 0)		// Flat?
                    zbottom--;
            }

            public int XLeft { get { return this.xleft; } }
            public int XRight { get { return this.xright; } }
            public int YFar { get { return this.yfar; } }
            public int YNear { get { return this.ynear; } }
            public int ZBottom { get { return this.zbottom; } }
            public int ZTop { get { return this.ztop; } }
        }

        class MapGraphNode
        {
            List<MapGraphNode> dependencies = new List<MapGraphNode>(1);

            public MapObject Object { get; set; }
            public List<MapGraphNode> Dependencies { get { return this.dependencies; } }
        }
    }
}
