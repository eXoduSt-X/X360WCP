using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    static class MainApplication
    {
        // Lógica mínima para ejecutar acciones que requieren privilegios.
        // Rellena los casos con el código que necesita privilegios.
        internal static void RunElevatedAction(string action)
        {
            try
            {
                // Comprobar si realmente estamos elevados (seguridad)
                if (!IsRunAsAdmin())
                {
                    // Por seguridad, si no estamos elevados, no hacemos nada.
                    return;
                }

                switch (action?.ToLowerInvariant())
                {
                    case "instalardriver":
                        // DriverInstaller.Install(); // ejemplo
                        break;
                    case "configurarservicio":
                        // ServiceHelper.Configure();
                        break;
                    default:
                        // Acción desconocida: log o nada.
                        break;
                }
            }
            catch (Exception)
            {
                // Manejo de errores/registro según necesites.
            }
        }

        // Solicita elevación para ejecutar una acción concreta (lanza nuevo proceso con UAC).
        public static bool RequestElevationForAction(string actionName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = $"--elevated-action={actionName}"
                };
                Process.Start(psi);
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // El usuario canceló el UAC o no se permitió la elevación.
                return false;
            }
        }

        // Método para relanzar la aplicación elevada (sin argumentos).
        public static bool RequestRunElevated()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // El usuario canceló el UAC o no se permitió la elevación.
                return false;
            }
        }

        static bool IsRunAsAdmin()
        {
            using (var id = WindowsIdentity.GetCurrent())
            {
                var p = new WindowsPrincipal(id);
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}