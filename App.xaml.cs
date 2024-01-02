using System.Globalization;
using System;
using System.Windows;
using UIGameClientTourist.GameLogic;

namespace UIGameClientTourist
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetAppCulture("en-US");
            //MusicService.Instance.PlayMusic();
        }

        public static void SetAppCulture(string cultureCode)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cambiando la cultura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
