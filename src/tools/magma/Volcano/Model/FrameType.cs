namespace Volcano.Model
{
    /// <summary>
    /// Indicates whether the specified frame is a 'shape' or a 'tile'. 'Tile' frames are the base frames that make up 
    /// the ground in the world; everything else is a 'shape'.
    /// </summary>
    public enum FrameType
    {
        /// <summary>
        /// Indicates that the frame is a "Shape", and therefore is some sort of larger object in the world.
        /// </summary>
        Shape,

        /// <summary>
        /// Indicates that the frame is a lowly "Tile", and is only suitable for being the ground.
        /// </summary>
        Tile
    }
}
