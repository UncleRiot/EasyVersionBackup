using System;
using System.Windows.Forms;

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