using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para MainMenuGame.xaml
    /// </summary>
    public partial class MainMenuGame : Window, IFriendsCallback
    {
        private int idPlayer;
        private TranslateTransform butProfileTranslateTransform;
        private TranslateTransform butLogOutTranslateTransform;

        Service.FriendsClient SesionContext;
        private bool areButtonsVisible = false;

        public MainMenuGame(int idPlayer)
        {
            this.idPlayer = idPlayer;
            InitializeComponent();
            InitializeAnimation();
            InstanceContext context = new InstanceContext(this);
            SesionContext = new Service.FriendsClient(context);
            Service.PlayerClient playerClient = new Service.PlayerClient();
            lblPlayerName.Content = playerClient.GetPlayerName(idPlayer);
            SesionContext.SavePlayerSession(idPlayer);
        }

        private void InitializeAnimation()
        {
            butProfileTranslateTransform = new TranslateTransform();
            butProfile.RenderTransform = butProfileTranslateTransform;
            butLogOutTranslateTransform = new TranslateTransform();
            butLogOut.RenderTransform = butLogOutTranslateTransform;
        }

        private void OpenSlidingMenu(object sender, MouseButtonEventArgs e)
        {
            if (areButtonsVisible)
            {
                HideButtons();
            }
            else
            {
                ShowButtons();
            }
        }

        private void ShowButtons()
        {
            DoubleAnimation moveButProfile = new DoubleAnimation(0, 66, TimeSpan.FromSeconds(0.5));
            butProfileTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButProfile);
            DoubleAnimation moveButLogOut = new DoubleAnimation(0, 115, TimeSpan.FromSeconds(0.5));
            butLogOutTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButLogOut);
            areButtonsVisible = true;
        }

        private void HideButtons()
        {
            DoubleAnimation moveButProfile = new DoubleAnimation(63, 0, TimeSpan.FromSeconds(0.5));
            butProfileTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButProfile);
            DoubleAnimation moveButLogOut = new DoubleAnimation(126, 0, TimeSpan.FromSeconds(0.5));
            butLogOutTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButLogOut);
            areButtonsVisible = false;
        }

        private void GoToLobbyFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Lobby lobbyWindow = new Lobby(0, idPlayer);
            this.Close();
            lobbyWindow.Show();
        }

        private void GoToGamesFromMainMenu(object sender, MouseButtonEventArgs e)
        {
            grdMainMenu.Visibility = Visibility.Collapsed;
            grdGames.Visibility = Visibility.Visible;
        }

        private void GoToFriendsFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Friends friendsWindow = new Friends(idPlayer);
            this.Close();
            friendsWindow.Show();
        }

        private void GoToUserProfileFromMainMenuGame(object sender, RoutedEventArgs e)
        {
            UserProfile userProfileWindow = new UserProfile(idPlayer);
            this.Close();
            userProfileWindow.Show();
        }

        private void GoToConfigurationFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Configuration configurationWindow = new Configuration(idPlayer);
            this.Close();
            configurationWindow.Show();
        }

        private void GoToJoinGameFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            JoinGame joinGameWindow = new JoinGame(idPlayer);
            this.Close();
            joinGameWindow.Show();
        }

        private void LogOut(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }

        private void GoToMainMenuFromGames(object sender, MouseButtonEventArgs e)
        {
            grdMainMenu.Visibility = Visibility.Visible;
            grdGames.Visibility = Visibility.Collapsed;
        }

        private void NavigateToMenuWindow(object sender, MouseButtonEventArgs e)
        {
            grdGames.Visibility = Visibility.Collapsed;
            grdMainMenu.Visibility = Visibility.Visible;
        }

        public void UpdateFriendRequest()
        {
            //throw new NotImplementedException();
        }

        public void UpdateFriendDisplay()
        {
            //throw new NotImplementedException();
        }
    }
}
