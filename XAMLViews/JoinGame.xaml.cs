using log4net;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para JoinGame.xaml
    /// </summary>
    public partial class JoinGame : Window
    {
        private readonly int _iDPlayer;
        private readonly PlayAsGuestManagerClient _managerClient;
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(JoinGame));
        private readonly bool _isInvited = false;

        public JoinGame(int idPlayer, bool isInvited)
        {
            this._iDPlayer = idPlayer;
            this._isInvited = isInvited;
            InitializeComponent();
            this._managerClient = new PlayAsGuestManagerClient();
        }

        private void ButtonClickSearchGame(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(CodeGame.Text, out int codeGame))
                {
                    if (ValidateGame(codeGame))
                    {
                        OpenLobbyWindow(codeGame);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.AlertInvalidCode_Label);
                }
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
        }

        private bool ValidateGame(int codeGame)
        {
            bool result = true;
            const int gameNotFound = 0;
            const int gameOngoing = 0;
            const int gameFull = 0;

            try
            {
                if (_managerClient.SearchGameByCode(codeGame) != gameNotFound)
                {
                    MessageBox.Show(Properties.Resources.ItemNotFoundAlert_Label);
                    result = false;
                }
                else if (_managerClient.IsGameOngoing(codeGame) != gameOngoing)
                {
                    MessageBox.Show(Properties.Resources.AlreadyStartedAlert_Label);
                    result = false;
                }
                else if (_managerClient.IsGameFull(codeGame) != gameFull)
                {
                    MessageBox.Show(Properties.Resources.FullLineItemAlert_Label);
                    result = false;
                }
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch(EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            return result;
        }

        private void OpenLobbyWindow(int codeGame)
        {
            Lobby lobby;

            if (_isInvited)
            {
                lobby = new Lobby(codeGame, _iDPlayer, true);
            }
            else
            {
                lobby = new Lobby(codeGame, _iDPlayer, false);
            }

            this.Close();
            lobby.Show();
        }

        private void ExitJoinGameWindow(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Window window;

                if (_isInvited)
                {
                    window = new MainWindow();
                }
                else
                {
                    window = new MainMenuGame(_iDPlayer);
                }

                this.Close();
                window.Show();

                e.Handled = true;
            }
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
