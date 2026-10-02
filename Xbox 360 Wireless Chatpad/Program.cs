using System;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Si se invoca con --elevated-action ejecuta solo eso y sale
            if (args != null && args.Length > 0)
            {
                const string prefix = "--elevated-action=";
                foreach (var a in args)
                {
                    if (a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        MainApplication.RunElevatedAction(a.Substring(prefix.Length));
                        return;
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Window_Main());
        }
    }
}