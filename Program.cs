using System;
using System.Windows.Forms;

namespace KeyceWordLite
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize(); // .NET 8 helper
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var splash = new SplashForm())
            {
                // Le splash gère quand ouvrir MainForm (bouton ou timer)
                var dialogResult = splash.ShowDialog();
                if (dialogResult == DialogResult.Cancel)
                {
                    // L'utilisateur a fermé le splash sans ouvrir l'app
                    return;
                }
            }

            Application.Run(new MainForm());
        }
    }
}
