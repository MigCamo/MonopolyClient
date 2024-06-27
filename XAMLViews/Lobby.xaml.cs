using log4net;
using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using UIGameClientTourist.GameLogic;
using UIGameClientTourist.Service;
using Player = UIGameClientTourist.Service.Player;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Lobby.xaml
    /// </summary>
    public partial class Lobby : Window, IGameManagerCallback, IFriendsCallback
    {
        private readonly PlayerClient _playerService = new PlayerClient();
        private readonly FriendListClient _friendListService;
        private readonly GameManagerClient _gameManagerClient;
        private readonly FriendsClient _sesionService;
        private Service.Game _currentGame = new Service.Game();
        private Dictionary<string, (Border, Ellipse)> _pieceMappings;
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(Lobby));
        private readonly Player _currentPlayer = new Player();

        public Lobby(int idGame, int idPlayer, bool invited)
        {
            this._currentPlayer = GetPlayerInfo(idPlayer);
            this._currentPlayer.Guest = invited;
            
            if(invited == true)
            {
                _currentPlayer.Name = "invitado";
            }
            
            this._currentPlayer.Piece = new Piece();

            InitializeComponent();
            InicializePieceMappings();

            InstanceContext context = new InstanceContext(this);

            try
            {
                _gameManagerClient = new GameManagerClient(context);
                _sesionService = new FriendsClient(context);
                _friendListService = new FriendListClient();
                _sesionService.UpdatePlayerSession(idPlayer);

                InitializeLobby(idGame);
                
                ShowFriends(idPlayer);
                
                _gameManagerClient.CheckTakenPieces(_currentGame, idPlayer);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
                LockAllControls();
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
                LockAllControls();
            }

        }

        private void LockAllControls()
        {
            foreach (var mapping in _pieceMappings)
            {
                BlockPiece(mapping.Key, _currentPlayer.IdPlayer);
            }

            butGoToLobbyWindow.Visibility = Visibility.Collapsed;
        }

        private void InicializePieceMappings()
        {
            _pieceMappings = new Dictionary<string, (Border, Ellipse)>
            {
                { "brdClockPiece", (brdClockPiece, ellPiece1) },
                { "brdDuckPiece", (brdDuckPiece, ellPiece2) },
                { "brdCarPiece", (brdCarPiece, ellPiece3) },
                { "brdPlayDiscPiece", (brdPlayDiscPiece, ellPiece4) },
                { "brdMPiece", (brdMPiece, ellPiece5) },
                { "brdTVPiece", (brdTVPiece, ellPiece6) }
            };
        }

        private void InitializeLobby(int idGame)
        {
            try
            {
                if (IsNewGame(idGame))
                {
                    CreateGame();
                }
                else
                {
                    LoadExistingGame(idGame);
                }

                UpdateUserInterface();
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        private bool IsNewGame(int idGame)
        {
            return idGame == 0;
        }

        private void LoadExistingGame(int idGame)
        {
            LoadGame(idGame);
            _gameManagerClient.InactivateBeginGameControls(idGame);
            butStartGame.Visibility = Visibility.Hidden;
        }

        private void UpdateUserInterface()
        {
            lblTitleUsername.Content = GetPlayerDisplayName(_currentPlayer.IdPlayer, _currentGame.IdGame);
            lblshowCodeGame.Content = _currentGame.IdGame.ToString();
            butGoToLobbyWindow.Visibility = Visibility.Hidden;
        }

        private string GetPlayerDisplayName(int idPlayer, int idGame)
        {
            string userName = "";
            try
            {
                userName = _playerService.GetMyPlayersName(idPlayer, idGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            return userName;
        }

        private void HandleException(Exception exception)
        {
            _ilog.Error(exception.ToString());
            AlertMessage();
        }


        public void ShowFriends(int idPlayer)
        {
            wpFriends.Children.Clear();

            foreach (FriendList friend in _friendListService.GetFriends(idPlayer))
            {
                Border brdBackground = new Border
                {
                    Width = 600,
                    Height = 60,
                    Background = Brushes.LightBlue,
                    Margin = new Thickness(2),
                };

                Grid grdContainer = new Grid();

                Ellipse ellUserStatus = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(-530, 10, 0, 0),
                };

                Button butSendMessage = new Button
                {
                    Content = Properties.Resources.SendMessage_Button,
                    Width = 80,
                    Height = 20,
                    Margin = new Thickness(250, 10, 0, 0),
                    IsEnabled = false,
                };

                if (friend.IsOnline)
                {
                    ellUserStatus.Fill = Brushes.Green;
                    butSendMessage.IsEnabled = true;

                    try
                    {
                        butSendMessage.Click += (sender, e) =>
                        {
                            _gameManagerClient.InviteFriendToGame(_currentGame.IdGame.ToString(), friend.IdFriend);
                        };
                    }
                    catch (TimeoutException exception)
                    {
                        HandleException(exception);
                    }
                    catch (EndpointNotFoundException exception)
                    {
                        HandleException(exception);
                    }
                }
                else
                {
                    ellUserStatus.Fill = Brushes.Red;
                }

                Label lblUserName = new Label
                {
                    Content = friend.FriendName,
                    FontSize = 28,
                    Margin = new Thickness(65, 6, 0, 0),
                };

                grdContainer.Children.Add(ellUserStatus);
                grdContainer.Children.Add(lblUserName);
                grdContainer.Children.Add(butSendMessage);
                brdBackground.Child = grdContainer;
                wpFriends.Children.Add(brdBackground);
            }
        }

        public void MouseClickPieceSelected(object sender, MouseButtonEventArgs e)
        {
            if (_currentPlayer.Piece.Name != null)
            {
                ResetPiece();
            }

            if (sender is Border selectedToken && _pieceMappings.TryGetValue(selectedToken.Name, out var pieces))
            {
                pieces.Item2.Fill = new SolidColorBrush(Colors.Aquamarine);
                _currentPlayer.Piece.ImagenSource = $"..\\GameResources\\Pictures\\{selectedToken.Name}.png";
                _currentPlayer.Piece.Name = selectedToken.Name;

                try
                {
                    _gameManagerClient.SelectedPiece(_currentGame, _currentPlayer.Piece.Name, _currentPlayer.IdPlayer);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }
                catch (CommunicationObjectFaultedException exception)
                {
                    HandleException(exception);
                }
            }
        }

        public void ResetPiece()
        {
            if (_pieceMappings.TryGetValue(_currentPlayer.Piece.Name, out var pieces))
            {
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF061A2E");

                try
                {
                    _gameManagerClient.UnSelectedPiece(_currentGame, _currentPlayer.Piece.Name, _currentPlayer.IdPlayer);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (CommunicationObjectFaultedException exception)
                {
                    HandleException(exception);
                }
            }
        }

        private void ButtonClickMainMenu(object sender, RoutedEventArgs e)
        {
            if (_currentPlayer.Guest)
            {
                OpenMainWindow();
            }
            else
            {
                OpenMainMenuGame(_currentPlayer.IdPlayer);
            }

            this.Close();
        }

        private void OpenMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OpenMainMenuGame(int playerId)
        {
            MainMenuGame mainMenuGame = new MainMenuGame(playerId);
            mainMenuGame.Show();
        }


        private void ButtonClickNavigateToCreateGameWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                _gameManagerClient.UnSelectedPiece(this._currentGame, _currentPlayer.Piece.Name, _currentPlayer.IdPlayer);
                _gameManagerClient.UnCheckReadyToStartGame(_currentGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            butGoToLobbyWindow.Visibility = Visibility.Hidden;
            grdLobbyWindow.Visibility = Visibility.Collapsed;
            grdCreateGameWindow.Visibility = Visibility.Visible;
        }

        private void ButtonClickNavigateToLobbyWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                const int pieceAlreadyTakenResult = 1;

                int updateResult = _gameManagerClient.UpdatePlayerGame(_currentGame, _currentPlayer.IdPlayer, _currentPlayer.Piece);

                if (updateResult == pieceAlreadyTakenResult)
                {
                    ShowPlayerPiece();
                }
                else
                {
                    ShowPieceAlreadyTakenAlert();
                }
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        private void ShowPieceAlreadyTakenAlert()
        {
            MessageBox.Show(Properties.Resources.AlertPartAlreadySelected_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowPlayerPiece()
        {
            imgPlayerCard.Source = ImageManager.GetSourceImage(_currentPlayer.Piece.ImagenSource);
            grdLobbyWindow.Visibility = Visibility.Visible;
            grdCreateGameWindow.Visibility = Visibility.Collapsed;
            _gameManagerClient.CheckReadyToStartGame(_currentGame);
        }


        private void ButtonClickGoGameWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                _gameManagerClient.StartGame(this._currentGame);
                _gameManagerClient.InitializeGame(this._currentGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        private int GenerateGameCode()
        {
            int seed = Environment.TickCount;
            Random random = new Random(seed);
            int GameCode = random.Next(10000, 99999);
            return GameCode;
        }

        private void CreateGame()
        {
            _currentGame.IdGame = GenerateGameCode();
            _currentGame.Slot = 3;
            _currentGame.Status = Service.Game.GameSituation.ByStart;
            try
            {
                _gameManagerClient.AddGame(this._currentGame);
                _gameManagerClient.AddPlayerToGame(this._currentGame.IdGame, this._currentPlayer);
                _gameManagerClient.UpdatePlayers(this._currentGame.IdGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        private void LoadGame(int idGame)
        {
            try
            {
                this._currentGame = _playerService.GetGame(idGame);

                if (_currentPlayer.Guest)
                {
                    _gameManagerClient.AddGuestToGame(idGame, _currentPlayer.IdPlayer);
                }
                else
                {
                    Player player = GetPlayerInfo(this._currentPlayer.IdPlayer);
                    _gameManagerClient.AddPlayerToGame(idGame, player);
                }

                _gameManagerClient.UpdatePlayers(this._currentGame.IdGame);

            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        private Player GetPlayerInfo(int idPlayer)
        {
            PlayerClient playerClient = new PlayerClient();

            Player player = new Player
            {
                IdPlayer = idPlayer,
                VotesToExpel = 0,
                Position = 0,
                Jail = false,
                Loser = false,
                Money = 150000,
                Piece = _currentPlayer.Piece,
                Guest = _currentPlayer.Guest
            };

            try
            {
                player.Name = playerClient.GetPlayerName(idPlayer);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            return player;
        }

        public void AddVisualPlayers(Queue<Player> playersInGame)
        {
            wpPlayers.Children.Clear();

            foreach (var player in playersInGame)
            {
                Border brdBackground = new Border
                {
                    Width = 650,
                    Height = 60,
                    Background = Brushes.LightBlue,
                    Margin = new Thickness(2),
                };

                Grid grdContainer = new Grid();

                Label lblUserName = new Label
                {
                    Content = player.Name,
                    FontSize = 28,
                    Margin = new Thickness(10, 6, 0, 0),
                };

                grdContainer.Children.Add(lblUserName);
                brdBackground.Child = grdContainer;
                wpPlayers.Children.Add(brdBackground);
            }

        }

        public int UpdateGame()
        {
            try
            {
                this._currentGame = _playerService.GetGame(_currentGame.IdGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            return _currentGame.Players.Count;
        }

        public void UpdateFriendRequest()
        {
            throw new NotImplementedException();
        }

        public void UpdateFriendDisplay()
        {
            ShowFriends(_currentPlayer.IdPlayer);
        }

        public void GetMessage(string message)
        {
            Border brdBackground = new Border
            {
                Width = double.NaN,
                Height = double.NaN,
                Background = Brushes.Transparent,
                Margin = new Thickness(2),
            };

            Grid grdContainer = new Grid();

            TextBox txtMessageContent = new TextBox
            {
                Text = message,
                FontSize = 14,
                Margin = new Thickness(10, 6, 0, 0),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
            };

            grdContainer.Children.Add(txtMessageContent);
            brdBackground.Child = grdContainer;
            wpChatMessages.Children.Add(brdBackground);
        }

        private void MouseClickSendMessage(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text))
            {
                string PlayerName = GetPlayerInfo(_currentPlayer.IdPlayer).Name;
                string message = PlayerName + ": " + Message.Text.Trim();

                try
                {
                    _gameManagerClient.SendMessage(_currentGame.IdGame, message);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }

                Message.Text = "";
            }
        }

        private void SendMessageOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(Message.Text))
            {
                PlayerClient playerClient = new PlayerClient();

                try
                {
                    string message = playerClient.GetMyPlayersName(_currentPlayer.IdPlayer, _currentGame.IdGame) + ": " + Message.Text.Trim();
                    _gameManagerClient.SendMessage(_currentGame.IdGame, message);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }

                Message.Text = "";
            }
        }

        public void MoveToGame(Service.Game game)
        {
            Game gameWindow = new Game(this._currentGame, GetPlayerInfo(_currentPlayer.IdPlayer));
            this.Close();
            gameWindow.Show();
        }

        public void PreparePieces(Service.Game game, Player[] playersInGame)
        {
            int partNumber = 0;
            foreach (var player in playersInGame)
            {
                Game._boardPieces[partNumber].Source = ImageManager.GetSourceImage(player.Piece.ImagenSource);
                Game._boardPieces[partNumber].Visibility = Visibility.Visible;
                partNumber++;
            }
        }

        public void BlockPiece(string piece, int idPlayer)
        {
            if (piece != null && piece != "" && _pieceMappings.TryGetValue(piece, out var pieces))
            {
                pieces.Item1.IsEnabled = false;
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#BF0E0E");
            }

            if (_currentPlayer.IdPlayer == idPlayer)
            {
                butGoToLobbyWindow.Visibility = Visibility.Visible;
            }
        }

        public void UnblockPiece(string piece)
        {
            if (_pieceMappings.TryGetValue(piece, out var pieces))
            {
                pieces.Item1.IsEnabled = true;
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF061A2E");
            }
        }

        private void MouseClickCopyToClipboard(object sender, MouseButtonEventArgs e)
        {
            string contenidoLabel = lblshowCodeGame.Content.ToString();
            Clipboard.SetText(contenidoLabel);
            MessageBox.Show(Properties.Resources.ExistingCopyToClipboardAlert_Label, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void EnableStartGameButton()
        {
            butStartGame.IsEnabled = true;
        }

        public void DisableStartGameButton()
        {
            butStartGame.IsEnabled = false;
        }

        public void ExitToGame()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void AlertMessage()
        {
            MessageBox.Show(Properties.Resources.LostConnectionAlertLabel_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
