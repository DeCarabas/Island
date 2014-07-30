namespace Volcano.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides mechanisms for reading data from Flex files.
    /// </summary>
    /// <remarks>
    /// Data for U7-engine games is stored in a particular format of file called a "flex" file. "Flex" files are just
    /// packed files, that contain a table-of-contents at the front. Each entry in the table of contents has an offset
    /// and a size in it; this allows the data to be referred to by index somewhere else, and loaded from the file on
    /// demand. Alas, we load everything eagerly, because we no longer have to worry about running in 4MB of EMS. 
    /// (Although maybe we should...)
    /// </remarks>
    public static class FlexFile
    {
        /// <summary>
        /// Loads a flex file from the specified stream, using the specified decoder to decode the elements.
        /// </summary>
        /// <typeparam name="T">The type of element contained in the file.</typeparam>
        /// <param name="file">The file stream to read.</param>
        /// <param name="decoder">The decoder to use to decode the entries.</param>
        /// <returns>A flex file, with all of the elements decoded.</returns>
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

    /// <summary>Represents a loaded Flex file.</summary>
    /// <typeparam name="T">The type of elements in this file.</typeparam>
    public class FlexFile<T>
    {
        List<T> contents = new List<T>();

        /// <summary>Gets a list of the contents of the flex file.</summary>
        public IList<T> Contents { get { return this.contents; } }

        /// <summary>Gets the count of elements in the flex file.</summary>
        public int Count { get { return this.contents.Count; } }

        /// <summary>Gets or sets the title of the file.</summary>
        public string Title { get; set; }
    }
}
