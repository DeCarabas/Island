namespace Volcano.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public static class FlexFile
    {
        public static FlexFile<T> Load<T>(FileStream file, FlexDecoder<T> decoder)
        {
            var flex = new FlexFile<T>();

            var reader = new BinaryReader(file);

            byte[] titleBytes = new byte[0x50];
            reader.Read(titleBytes, 0, titleBytes.Length);

            int terminator;
            for (terminator = 0; terminator < titleBytes.Length; terminator++)
            {
                if (titleBytes[terminator] == 0) { break; }
            }

            flex.Title = Encoding.ASCII.GetString(titleBytes, 0, terminator);

            int magic1 = reader.ReadInt32();
            uint count = reader.ReadUInt32();
            int magic2 = reader.ReadInt32();

            file.Seek(9 * 4, SeekOrigin.Current);

            var references = new FlexReference[count];
            for (long i = 0; i < count; i++)
            {
                references[i].Offset = reader.ReadUInt32();
                references[i].Size = reader.ReadUInt32();
            }

            for (long i = 0; i < count; i++)
            {
                if (references[i].Offset == 0)
                {
                    flex.Contents.Add(default(T));
                }
                else
                {
                    file.Seek(references[i].Offset, SeekOrigin.Begin);
                    flex.Contents.Add(decoder(reader, i, references[i].Size));
                }
            }

            return flex;
        }

        struct FlexReference
        {
            public long Offset;
            public long Size;
        }    
    }

    public class FlexFile<T>
    {
        List<T> contents = new List<T>();

        public IList<T> Contents { get { return this.contents; } }
        public int Count { get { return this.contents.Count; } }
        public string Title { get; set; }
    }
}
