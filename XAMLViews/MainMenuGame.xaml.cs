using log4net;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UIGameClientTourist.GameLogic;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para MainMenuGame.xaml
    /// </summary>
    public partial class MainMenuGame : Window, IFriendsCallback
    {
        private readonly int _currentPlayerID;
        private TranslateTransform _butProfileTranslateTransform;
        private TranslateTransform _butLogOutTranslateTransform;
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(MainMenuGame));
        private readonly FriendsClient _SesionContext;
        private bool _areButtonsVisible = false;
        private readonly PlayerClient _playerClient = new PlayerClient();

        public MainMenuGame(int idPlayer)
        {
            this._currentPlayerID = idPlayer;
            InitializeComponent();
            InitializeAnimation();
            InstanceContext context = new InstanceContext(this);

            try
            {
                _SesionContext = new FriendsClient(context);
                lblPlayerName.Content = _playerClient.GetPlayerName(idPlayer);
                _SesionContext.SavePlayerSession(idPlayer);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }

            ((App)Application.Current).idPlayer = idPlayer;
        }

        private void InitializeAnimation()
        {
            _butProfileTranslateTransform = new TranslateTransform();
            butProfile.RenderTransform = _butProfileTranslateTransform;
            _butLogOutTranslateTransform = new TranslateTransform();
            butLogOut.RenderTransform = _butLogOutTranslateTransform;
        }

        private void MouseClickOpenSlidingMenu(object sender, MouseButtonEventArgs e)
        {
            if (_areButtonsVisible)
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
            _butProfileTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButProfile);
            DoubleAnimation moveButLogOut = new DoubleAnimation(0, 115, TimeSpan.FromSeconds(0.5));
            _butLogOutTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButLogOut);
            _areButtonsVisible = true;
        }

        private void HideButtons()
        {
            DoubleAnimation moveButProfile = new DoubleAnimation(63, 0, TimeSpan.FromSeconds(0.5));
            _butProfileTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButProfile);
            DoubleAnimation moveButLogOut = new DoubleAnimation(126, 0, TimeSpan.FromSeconds(0.5));
            _butLogOutTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveButLogOut);
            _areButtonsVisible = false;
        }

        private void MouseClickGoToLobbyFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Lobby lobbyWindow = new Lobby(0, _currentPlayerID, false);
            this.Close();
            lobbyWindow.Show();
        }

        private void MouseClickGoToGamesFromMainMenu(object sender, MouseButtonEventArgs e)
        {
            grdMainMenu.Visibility = Visibility.Collapsed;
            grdGames.Visibility = Visibility.Visible;
        }

        private void MouseClickGoToFriendsFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Friends friendsWindow = new Friends(_currentPlayerID);
            this.Close();
            friendsWindow.Show();
        }

        private void ButtonClickGoToUserProfileFromMainMenuGame(object sender, RoutedEventArgs e)
        {
            UserProfile userProfileWindow = new UserProfile(_currentPlayerID);
            this.Close();
            userProfileWindow.Show();
        }

        private void MouseClickGoToConfigurationFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            Configuration configurationWindow = new Configuration(_currentPlayerID);
            this.Close();
            configurationWindow.Show();
        }

        private void MouseClickGoToJoinGameFromMainMenuGame(object sender, MouseButtonEventArgs e)
        {
            JoinGame joinGameWindow = new JoinGame(_currentPlayerID, false);
            this.Close();
            joinGameWindow.Show();
        }

        private void ButtonClickLogOut(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            try
            {
                _playerClient.LogOut(this._currentPlayerID);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (CommunicationObjectFaultedException exception)
            {
                HandleException(exception);
            }
            
            this.Close();
            mainWindow.Show();
        }

        private void MouseClickGoToMainMenuFromGames(object sender, MouseButtonEventArgs e)
        {
            grdMainMenu.Visibility = Visibility.Visible;
            grdGames.Visibility = Visibility.Collapsed;
        }

        private void MouseClickNavigateToMenuWindow(object sender, MouseButtonEventArgs e)
        {
            grdGames.Visibility = Visibility.Collapsed;
            grdMainMenu.Visibility = Visibility.Visible;
        }

        public void UpdateFriendRequest()
        {
            int currentCount = int.Parse(lblConnectedPlayers.Content.ToString());
            currentCount++;
            lblConnectedPlayers.Content = currentCount.ToString();
        }

        public void UpdateFriendDisplay()
        {
            int currentCount = int.Parse(lblRequests.Content.ToString());
            currentCount++;
            lblConnectedPlayers.Content = currentCount.ToString();
        }

        private void AlertMessage()
        {
            MessageBox.Show(Properties.Resources.LostConnectionAlertLabel_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleException(Exception exception)
        {
            _ilog.Error(exception.ToString());
            AlertMessage();
        }
    }
}
