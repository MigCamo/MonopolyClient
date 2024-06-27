using System.Globalization;
using System;
using System.Windows;
using UIGameClientTourist.GameLogic;
using log4net.Config;
using System.IO;
using UIGameClientTourist.Service;

namespace UIGameClientTourist
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public int idPlayer { get; set; }
        private PlayerClient playerClient = new PlayerClient();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetAppCulture("es-MX");
            MusicService.Instance.PlayMusic();
            this.Exit += ExitGame;
        }

        public static void SetAppCulture(string cultureCode)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogsReport.xml");
            XmlConfigurator.Configure(new FileInfo(path));

            try
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cambiando la cultura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ExitGame(object sender, ExitEventArgs e)
        {
            try
            {
                playerClient.LogOut(idPlayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cambiando la cultura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
