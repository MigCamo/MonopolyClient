using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volumeValue = volumeSlider.Value;
            MusicService.Instance.SetVolume(volumeValue);
        }

        private void rbutPlayMusic_Checked_1(object sender, RoutedEventArgs e)
        {
            MusicService.Instance.PlayMusic();
        }

        private void rbutPauseMusic_Checked(object sender, RoutedEventArgs e)
        {
            MusicService.Instance.StopMusic();

        }
    }
}
