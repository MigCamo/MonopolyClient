using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lógica de interacción para UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        private int idPlayer;
        public UserProfile(int idPlayer)
        {
            InitializeComponent();
            this.idPlayer = idPlayer;
        }
        private void NavigateToMainMenuGame(object sender, RoutedEventArgs e)
        {
            MainMenuGame menuWindow = new MainMenuGame(idPlayer);
            this.Close();
            menuWindow.Show();
        }

        private void ProfileImage_Click(object sender, MouseButtonEventArgs e)
        {
            Border selectedBorder = (Border)sender;
            selectedBorder.BorderBrush = Brushes.Green;
            Image selectedImage = (Image)selectedBorder.Child;
            string selectedImagePath = selectedImage.Source.ToString();
            imgUserProfile.Source = ImageManager.GetSourceImage(selectedImagePath) ;
        }
    }
}
