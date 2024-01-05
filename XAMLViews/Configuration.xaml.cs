using System.Windows;
using System.Windows.Controls;
using UIGameClientTourist.GameLogic;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        private int idPlayer;
        public Configuration(int idPlayer)
        {
            InitializeComponent();
            this.idPlayer = idPlayer;
            fullcomboBox();
        }

        private void fullcomboBox()
        {
            cboxLanguages.Items.Add(Properties.Resources.LanguageSpanish_Label);
            cboxLanguages.Items.Add(Properties.Resources.LanguageEnglish_Label);
        }

        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            if (cboxLanguages.SelectedItem != null)
            {
                string selectedLanguage = cboxLanguages.SelectedItem.ToString();

                if (selectedLanguage == Properties.Resources.LanguageSpanish_Label)
                {
                    App.SetAppCulture("es-ES");
                }
                else if (selectedLanguage == Properties.Resources.LanguageEnglish_Label)
                {
                    App.SetAppCulture("en-US");
                }

                Configuration configurationWindow = new Configuration(idPlayer);
                this.Close();
                configurationWindow.Show();
            }
        }

        private void NavigateToMainMenuGame(object sender, RoutedEventArgs e)
        {
            MainMenuGame menuWindow = new MainMenuGame(idPlayer);
            this.Close();
            menuWindow.Show();
        }

        private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volumeValue = volumeSlider.Value;
            MusicService.Instance.SetVolume(volumeValue);
        }

        private void ButPlayMusic(object sender, RoutedEventArgs e)
        {
            MusicService.Instance.PlayMusic();
        }

        private void ButPauseMusic(object sender, RoutedEventArgs e)
        {
            MusicService.Instance.StopMusic();

        }
    }
}
