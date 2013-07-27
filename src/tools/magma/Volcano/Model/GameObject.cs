namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;

    public enum ObjectKind
    {
        Standard,
        Extended,
        Extra,
    }

    public class GameObject : MapObject 
    {
        public ObjectKind ObjectKind { get; set; }
        public byte Quality { get; set; }
    }
}
