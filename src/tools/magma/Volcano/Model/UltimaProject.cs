namespace Volcano.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections;
    using System.Drawing;
    using System.ComponentModel;

    public delegate T FlexDecoder<T>(BinaryReader reader, long objectId, long objectSize);

    /// <summary>
    /// Represents the game data for a Ultima 7 based game.
    /// </summary>
    public class UltimaProject : INotifyPropertyChanged
    {
        // These were taken from the exult source code; they are colors with alphas that are to be used 
        // in place of the "translucent" colors in the palettes.
        //
        static Color[] Blends = new Color[] 
        {
            Color.FromArgb(192,208,216,224),    
            Color.FromArgb(198,136, 44,148),    
            Color.FromArgb(211,248,252, 80),
            Color.FromArgb(247,144,148,252),
            Color.FromArgb(201, 64,216, 64),    
            Color.FromArgb(140,204, 60, 84),		        
            Color.FromArgb(128,144, 40,192),     
            Color.FromArgb(128, 96, 40, 16),    
            Color.FromArgb(192,100,108,116), 
            Color.FromArgb(128, 68,132, 28),    
            Color.FromArgb( 64,255,208, 48),    
            Color.FromArgb(128, 28, 52,255),
            Color.FromArgb(128,  8, 68,  0),    
            Color.FromArgb(118,255,  8,  8),    
            Color.FromArgb(128,255,244,248), 
            Color.FromArgb(128, 56, 40, 32),    
            Color.FromArgb( 82,228,224,214)
        };

        string gameDirectory;
        UltimaMap map;
        FlexFile<Color[]> palettes;
        FlexFile<Shape> shapes;
        FlexFile<string> text;

        public string GameDirectory
        {
            get { return this.gameDirectory; }
            set
            {
                this.gameDirectory = value;
                Notify("GameDirectory");
                Notify("GamedatDirectory");
                Notify("StaticDirectory");
            }
        }
        public string GamedatDirectory { get { return Path.Combine(GameDirectory, "gamedat"); } }
        public string StaticDirectory { get { return Path.Combine(GameDirectory, "static"); } }

        public UltimaMap Map
        {
            get { return this.map; }
            set
            {
                this.map = value;
                Notify("Map");
            }
        }
        public FlexFile<Color[]> Palettes
        {
            get { return this.palettes; }
            set
            {
                this.palettes = value;
                Notify("Palettes");
            }
        }
        public FlexFile<Shape> Shapes
        {
            get { return this.shapes; }
            set
            {
                this.shapes = value;
                Notify("Shapes");
            }
        }
        public FlexFile<string> Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                Notify("Text");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        Frame GetFrame(int shape, int frame)
        {
            Shape shapeObject = Shapes.Contents[shape];
            return shapeObject.Frames[frame % shapeObject.Frames.Length];
        }

        public void Load()
        {
            Palettes = LoadPalettes();
            Shapes = LoadShapes();
            Map = LoadMap();
            Text = LoadText();
        }

        FlexFile<string> LoadText()
        {
            return OpenStaticFlex<string>("TEXT.FLX", (reader, id, size) =>
            {
                byte[] bytes = reader.ReadBytes((int)size);
                int zeroIndex = bytes.Length;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] == 0) { zeroIndex = i; break; }
                }
                return Encoding.ASCII.GetString(bytes, 0, zeroIndex);
            });
        }

        void LoadFixedObjects(UltimaMap map)
        {
            for (int y = 0; y < MapUnits.RegionsPerMap; y++)
            {
                int absoluteRegionTileY = y * MapUnits.ChunksPerRegion * MapUnits.TilesPerChunk;
                for (int x = 0; x < MapUnits.RegionsPerMap; x++)
                {
                    int absoluteRegionTileX = x * MapUnits.ChunksPerRegion * MapUnits.TilesPerChunk;

                    FlexFile<List<MapObject>> staticItems;
                    int fileCoord = (y * MapUnits.RegionsPerMap) + x;
                    staticItems = OpenStaticFlex(String.Format("U7IFIX{0:X02}", fileCoord), (reader, id, size) =>
                    {
                        int chunkX = (int)(id % MapUnits.ChunksPerRegion);
                        int chunkY = (int)(id / MapUnits.ChunksPerRegion);

                        int absoluteChunkTileX = chunkX * MapUnits.TilesPerChunk;
                        int absoluteChunkTileY = chunkY * MapUnits.TilesPerChunk;

                        List<MapObject> objects = new List<MapObject>((int)size / 4);
                        for (long i = 0; i < size; i += 4)
                        {
                            MapObject obj = new StaticMapObject();

                            byte coord = reader.ReadByte();
                            int tileX = ((coord & 0xF0) >> 4);
                            int tileY = ((coord & 0x0F) >> 0);

                            int globalTileX = tileX + absoluteChunkTileX + absoluteRegionTileX;
                            int globalTileY = tileY + absoluteChunkTileY + absoluteRegionTileY;



                            obj.Location.X = globalTileX * MapUnits.PixelsPerTile;
                            obj.Location.Y = globalTileY * MapUnits.PixelsPerTile;
                            obj.Location.Z = reader.ReadByte() & 0x0F;

                            ushort data = reader.ReadUInt16();

                            int shape = (data & 0x03FF);
                            int frame = (data & 0x7C00) >> 10;

                            obj.Frame = GetFrame(shape, frame);

                            objects.Add(obj);
                        }

                        return objects;
                    });


                    int iChunk = 0;
                    for (int cy = 0; cy < MapUnits.ChunksPerRegion; cy++)
                    {
                        for (int cx = 0; cx < MapUnits.ChunksPerRegion; cx++)
                        {
                            List<MapObject> chunkObjects = staticItems.Contents[iChunk];
                            if (chunkObjects != null)
                            {
                                map[x, y][cx, cy].Objects.AddRange(chunkObjects);
                            }
                            iChunk++;
                        }
                    }
                }
            }
        }

        void LoadGameObjects(UltimaMap map)
        {
            for (int y = 0; y < MapUnits.RegionsPerMap; y++)
            {
                int absoluteRegionTileY = y * MapUnits.ChunksPerRegion * MapUnits.TilesPerChunk;
                for (int x = 0; x < MapUnits.RegionsPerMap; x++)
                {
                    int absoluteRegionTileX = x * MapUnits.ChunksPerRegion * MapUnits.TilesPerChunk;

                    int fileCoord = (y * MapUnits.RegionsPerMap) + x;
                    using (var stream = OpenGameFile(String.Format("U7IREG{0:X02}", fileCoord)))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            while (reader.BaseStream.Position < reader.BaseStream.Length)
                            {
                                List<MapObject> chunkObjects = LoadObjectList(reader, (xy) =>
                                {
                                    int lx = (xy & 0x00FF);
                                    int ly = (xy & 0xFF00) >> 8;

                                    return new MapPoint
                                    {
                                        X = (MapUnits.PixelsPerRegion * x) + (lx * MapUnits.PixelsPerTile),
                                        Y = (MapUnits.PixelsPerRegion * y) + (ly * MapUnits.PixelsPerTile)
                                    };
                                });

                                foreach (MapObject mo in chunkObjects)
                                {
                                    MapRegion region = map[mo.Location.WorldX, mo.Location.WorldY];
                                    MapChunk chunk = region[mo.Location.RegionX, mo.Location.RegionY];

                                    chunk.Objects.Add(mo);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<MapObject> LoadObjectList(BinaryReader reader, Func<ushort, MapPoint> decodePosition)
        {
            List<MapObject> objects = new List<MapObject>();
            
            byte objectKind = reader.ReadByte();
            while (objectKind != 0 && objectKind != 1)
            {
                UInt16 xy = reader.ReadUInt16();

                var gameObject = new GameObject { Location = decodePosition(xy) };

                ushort shapeId = reader.ReadUInt16();
                int shapeNumber = shapeId & 0x03FF;
                int frameNumber = (shapeId & 0x7C00) >> 10;
                gameObject.Frame = Shapes.Contents[shapeNumber].Frames[frameNumber];

                switch (objectKind)
                {
                case 0x06:
                    gameObject.ObjectKind = ObjectKind.Standard;
                    ReadStandardObject(gameObject, reader);
                    break;
                case 0x0C:
                    gameObject.ObjectKind = ObjectKind.Extended;
                    ReadExtendedObject(gameObject, reader);
                    break;
                case 0x12:
                    gameObject.ObjectKind = ObjectKind.Extra;
                    ReadExtraObject(gameObject, reader);
                    break;
                default:
                    throw new NotSupportedException("Unknown object type");
                }

                objects.Add(gameObject);

                if (reader.BaseStream.Length == reader.BaseStream.Position) { break; }
                objectKind = reader.ReadByte();
            }

            return objects;
        }

        void Notify(string property)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        }        

        static void SetObjectLift(GameObject gameObject, byte lift)
        {
            gameObject.Location.Z = (lift & 0xF0) >> 4;
        }

        static void ReadStandardObject(GameObject gameObject, BinaryReader reader)
        {
            byte lift = reader.ReadByte();
            byte quality = reader.ReadByte();

            SetObjectLift(gameObject, lift);
            gameObject.Quality = quality;
        }

        void ReadExtendedObject(GameObject gameObject, BinaryReader reader)
        {
            // TODO: Interpret this; it's different per object type. (Virtue stones, barges, spellbooks, &c.)
            UInt16 type = reader.ReadUInt16();
            byte proba = reader.ReadByte();
            UInt16 data1 = reader.ReadUInt16();
            byte lift = reader.ReadByte();
            UInt16 data2 = reader.ReadUInt16();

            if (gameObject.Shape.Class == ShapeClass.Container)
            {
                List<MapObject> contents = LoadObjectList(reader, xy => new MapPoint());
            }

            SetObjectLift(gameObject, lift);
        }

        static void ReadExtraObject(GameObject gameObject, BinaryReader reader)
        {
            // TODO: Interpret this; it's really just spellbook information.
            byte[] circlesFirst = reader.ReadBytes(5);
            byte lift = reader.ReadByte();
            byte[] circlesSecond = reader.ReadBytes(4);
            int flags = reader.ReadInt32();

            SetObjectLift(gameObject, lift);
        }

        UltimaMap LoadMap()
        {
            var map = new UltimaMap();
            using (FileStream chunksFile = OpenStaticFile("U7CHUNKS"))
            {
                int id = 0;
                var reader = new BinaryReader(chunksFile);
                while (chunksFile.Position != chunksFile.Length)
                {
                    var chunk = new ChunkTemplate { Id = id++ };
                    for (int y = 0; y < chunk.Height; y++)
                    {
                        for (int x = 0; x < chunk.Width; x++)
                        {
                            ushort data = reader.ReadUInt16();

                            int shape = (data & 0x03FF);
                            int frame = (data & 0x7C00) >> 10;

                            chunk[x, y] = GetFrame(shape, frame);
                        }
                    }

                    map.ChunkTemplates.Add(chunk);
                }
            }

            using (FileStream mapFile = OpenStaticFile("U7MAP"))
            {
                var reader = new BinaryReader(mapFile);

                for (int regionY = 0; regionY < map.Height; regionY++)
                {
                    for (int regionX = 0; regionX < map.Width; regionX++)
                    {
                        var region = new MapRegion();

                        for (int y = 0; y < region.Height; y++)
                        {
                            for (int x = 0; x < region.Width; x++)
                            {
                                ushort chunkId = reader.ReadUInt16();
                                region[x, y] = new MapChunk
                                {
                                    Location = new MapPoint
                                    {
                                        X = (regionX * MapUnits.PixelsPerRegion) + (x * MapUnits.PixelsPerChunk),
                                        Y = (regionY * MapUnits.PixelsPerRegion) + (y * MapUnits.PixelsPerChunk),
                                    },
                                    Template = map.ChunkTemplates[chunkId]
                                };
                            }
                        }

                        map[regionX, regionY] = region;
                    }
                }
            }

            LoadFixedObjects(map); // TODO: GULP
            LoadGameObjects(map);
            return map;
        }

        FlexFile<Color[]> LoadPalettes()
        {
            FlexFile<byte[]> xforms = OpenStaticFlex("XFORM.TBL", (reader, id, length) =>
            {
                return reader.ReadBytes(256);
            });

            return OpenStaticFlex("PALETTES.FLX", (reader, id, length) =>
            {
                int transformStart = 0xFF - Blends.Length;

                var palette = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    // These palettes are 6 bits per channel; we want 8 bits per channel otherwise the color range is
                    // all messed up. So expand, please.
                    //
                    byte red = (byte)(((double)reader.ReadByte()) / ((double)0x3F) * 255);
                    byte green = (byte)(((double)reader.ReadByte()) / ((double)0x3F) * 255);
                    byte blue = (byte)(((double)reader.ReadByte()) / ((double)0x3F) * 255);

                    if (i >= transformStart && i <= 0xFE)
                    {
                        palette[i] = Blends[i - transformStart];
                    }
                    else
                    {
                        palette[i] = Color.FromArgb(red, green, blue);
                    }
                }

                return palette;
            });
        }

        FlexFile<Shape> LoadShapes()
        {
            // Load the raw shape data...
            //
            FlexFile<Shape> shapes = OpenStaticFlex("SHAPES.VGA", (reader, id, length) =>
            {
                byte[] block = reader.ReadBytes((int)length);
                Shape shape = Shape.Load(new MemoryStream(block));
                shape.Id = id;
                return shape;
            });

            // Load extra shape data (occludes, tfa, &c.)
            //
            using (FileStream tfa = OpenStaticFile("TFA.DAT"))
            {
                byte[] tfaData = new byte[3];
                for (int i = 0; i < shapes.Count; i++)
                {
                    tfa.Read(tfaData, 0, 3);

                    Shape shape = shapes.Contents[i];
                    if (shape != null)
                    {
                        shape.Size = new Size3D
                        {
                            X = 1 + (tfaData[2] & 0x07),
                            Y = 1 + ((tfaData[2] >> 3) & 0x07),
                            Z = tfaData[0] >> 5,
                        };
                        shape.Class = (ShapeClass)(tfaData[1] & 0x0F);
                    }
                }
            }

            using (FileStream occludes = OpenStaticFile("OCCLUDE.DAT"))
            {
                var data = new byte[occludes.Length];
                occludes.Read(data, 0, data.Length);

                var occludeBits = new BitArray(data);
                for (int i = 0; i < occludeBits.Count; i++)
                {
                    Shape shape = shapes.Contents[i];
                    if (shape != null)
                    {
                        shape.Occludes = occludeBits[i];
                    }
                }
            }

            return shapes;
        }

        public FileStream OpenStaticFile(string fileName)
        {
            return File.OpenRead(Path.Combine(StaticDirectory, fileName));
        }

        public FlexFile<T> OpenStaticFlex<T>(string fileName, FlexDecoder<T> decoder)
        {
            using (FileStream file = OpenStaticFile(fileName))
            {
                return FlexFile.Load(file, decoder);
            }
        }

        public FileStream OpenGameFile(string fileName)
        {
            return File.OpenRead(Path.Combine(GamedatDirectory, fileName));
        }

        public FlexFile<T> OpenGameFlex<T>(string fileName, FlexDecoder<T> decoder)
        {
            using (FileStream file = OpenGameFile(fileName))
            {
                return FlexFile.Load(file, decoder);
            }
        }
    }
}
