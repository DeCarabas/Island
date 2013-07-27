namespace FlexDump
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Volcano;
    using System.IO;
    using Volcano.Model;

    class Program
    {
        static void Main(string[] args)
        {
            FlexFile<object> file = FlexFile.Load<object>(File.OpenRead(args[0]), (reader, id, length) =>
            {
                Console.WriteLine("ID: {0} Size {1}", id, length);
                return null;
            });
            Console.WriteLine(file.Title);
        }
    }
}
