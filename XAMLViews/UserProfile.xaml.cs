using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            imgUserProfile.Source = ImageManager.GetSourceImage(selectedImagePath);
        }
    }
}
