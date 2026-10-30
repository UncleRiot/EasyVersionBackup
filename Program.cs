using System;
using System.Windows.Forms;

// Semantic Anchor [Program.cs]: Application startup creates one main form and delegates application state and persistence to its existing owners; preserve startup argument handling.
namespace EasyVersionBackup
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool startMinimizedToSystray = false;

            foreach (string argument in args)
            {
                if (string.Equals(argument, "--start-minimized", StringComparison.OrdinalIgnoreCase))
                {
                    startMinimizedToSystray = true;
                    break;
                }
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(startMinimizedToSystray));
        }
    }
}