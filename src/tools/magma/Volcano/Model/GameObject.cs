namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;

    /// <summary>
    /// The kind of object that this object is.
    /// </summary>
    /// <remarks>
    /// This work on object kinds was never completed.
    /// </remarks>
    public enum ObjectKind
    {
        /// <summary>
        /// A standard object.
        /// </summary>
        Standard,

        /// <summary>
        /// An extended object.
        /// </summary>
        Extended,

        /// <summary>
        /// An extra object.
        /// </summary>
        Extra,
    }

    /// <summary>
    /// An object on the map that can be interacted with.
    /// </summary>
    public class GameObject : MapObject 
    {
        public ObjectKind ObjectKind { get; set; }
        public byte Quality { get; set; }
    }
}
