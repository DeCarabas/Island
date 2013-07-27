namespace extractmap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Volcano.Model;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // TODO: Multiple palettes.
                //
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: extractmap <game-dir> <out-dir>");
                    return;
                }

                var project = new UltimaProject { GameDirectory = args[0] };
                project.Load();

                string templatePath = Path.Combine(args[1], "map", "templates");
                if (!Directory.Exists(templatePath)) { Directory.CreateDirectory(templatePath); }

                // TODO: Does this need to be more efficient? (Chunked?)
                for (int i = 0; i < project.Map.ChunkTemplates.Count; i++)
                {
                    using (var writer = File.CreateText(Path.Combine(templatePath, i.ToString() + ".json")))
                    {
                        writer.Write("[");
                        ChunkTemplate template = project.Map.ChunkTemplates[i];
                        for (int y = 0; y < template.Height; y++)
                        {
                            if (y != 0) { writer.Write(","); }
                            writer.Write("[");
                            for (int x = 0; x < template.Width; x++)
                            {
                                if (x != 0) { writer.Write(","); }
                                writer.Write(@"{{""s"":{0},""f"":{1}}}",
                                    template[x, y].Shape.Id,
                                    template[x, y].FrameNumber);
                            }
                            writer.Write("]");
                        }
                        writer.Write("]");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SAD: {0}", e);
            }
        }
    }

}
