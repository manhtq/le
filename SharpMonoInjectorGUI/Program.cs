using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;

namespace SharpMonoInjectorGUI
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [STAThread]
        static void Main()
        {
            // Check if running as administrator
            if (!IsRunningAsAdministrator())
            {
                var result = MessageBox.Show(
                    "This application requires administrator privileges to inject DLLs.\n" +
                    "Would you like to restart with administrator privileges?",
                    "Administrator Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    RestartAsAdministrator();
                    return;
                }
            }

            // Enable visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show console for debugging (optional)
            #if DEBUG
            AllocConsole();
            Console.WriteLine("SharpMono Injector GUI - Debug Console");
            Console.WriteLine("========================================");
            #endif

            // Run the main form
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private static void RestartAsAdministrator()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to restart with administrator privileges:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}