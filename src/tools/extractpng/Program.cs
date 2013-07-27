namespace extractpng
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Volcano.Model;
    using System.IO;
    using System.Drawing;
    using System.Drawing.Imaging;

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
                    Console.WriteLine("Usage: extractpng <game-dir> <out-dir>");
                    return;
                }

                var project = new UltimaProject { GameDirectory = args[0] };
                project.Load();
                
                for (int i = 0; i < project.Shapes.Count; i++)
                {
                    double percent = (((double)i)/((double)project.Shapes.Count)) * 100.0;
                    Console.Write("\rShape {0}/{1} ({2}%)                    ", i, project.Shapes.Count, percent);

                    string shapePath = Path.Combine(args[1], "shapes", i.ToString());
                    if (!Directory.Exists(shapePath)) { Directory.CreateDirectory(shapePath); }
                    
                    Shape shape = project.Shapes.Contents[i];
                    Frame[] frames = shape.Frames;
                    for (int j = 0; j < frames.Length; j++)
                    {
                        string framePath = Path.Combine(shapePath, j.ToString() + ".png");
                        Bitmap bitmap = frames[j].GetBitmap(project.Palettes.Contents[0]);
                        bitmap.Save(framePath, ImageFormat.Png);
                    }
                }
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("SAD: {0}", e);
            }
        }
    }
}
