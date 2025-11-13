using System.Security.Principal;
using System.Diagnostics;

namespace ValorantEssentials
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsAdministrator())
            {
                RestartAsAdministrator();
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RestartAsAdministrator()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "This application requires administrator privileges to function properly.",
                    "Administrator Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            Application.Exit();
        }
    }
}