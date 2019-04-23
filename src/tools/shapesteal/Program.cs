using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Model;

namespace shapesteal
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var project = new UltimaProject { GameDirectory = args[0] };
                Console.WriteLine("Loading project...");
                project.Load();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StealForm(project));

                Console.WriteLine("OK");
            }
            catch(Exception e)
            {
                Console.WriteLine("SAD: {0}", e);
            }
        }
    }
}
