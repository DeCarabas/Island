namespace Volcano.Tectonics.Controllers
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web.Mvc;
    using Volcano.Model;

    [HandleError]
    public class ImageController : Controller
    {
        static UltimaProject project = new UltimaProject { GameDirectory = @"c:\src\island\games\si" };
        static ProjectCache cache = new ProjectCache();

        static ImageController()
        {
            project.Load();
            cache.Project = project;
        }

        public ActionResult Chunk(int chunkNumber)
        {
            throw new NotImplementedException();
        }

        public ActionResult Shape(int shapeNumber, int frameNumber, int paletteNumber)
        {
            if (shapeNumber >= project.Shapes.Count)
                throw new InvalidOperationException(); // Should be 404, help!

            Shape shape = project.Shapes.Contents[shapeNumber];
            if (frameNumber >= shape.Frames.Length)
                throw new InvalidOperationException(); // Likewise!

            if (paletteNumber >= project.Palettes.Count)
                throw new InvalidOperationException(); // And again!
            
            Bitmap bitmap = shape.Frames[frameNumber].GetBitmap(project.Palettes.Contents[paletteNumber]);
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            return File(stream, "image/png");
        }
    }
}
