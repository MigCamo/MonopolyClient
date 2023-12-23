using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using UIGameClientTourist.Service;
using Player = UIGameClientTourist.Service.Player;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Lobby.xaml
    /// </summary>
    public partial class Lobby : Window, IGameManagerCallback, IFriendsCallback
    {
        private readonly int IdPlayer;
        private Service.PlayerClient PlayerService = new Service.PlayerClient();
        private readonly Service.FriendListClient friendListService;
        private Service.GameManagerClient GameManagerClient;
        private Service.FriendsClient SesionService;
        private Service.Game game = new Service.Game();
        private Service.Piece piecePlayer = new Service.Piece();
        private Dictionary<string, (Border, Ellipse)> pieceMappings;
        private bool isInvited = false;
        
        public Lobby(int idGame, int idPlayer)
        {
            this.IdPlayer = idPlayer;
            InstanceContext context = new InstanceContext(this);
            GameManagerClient = new Service.GameManagerClient(context);
            SesionService = new Service.FriendsClient(context);
            friendListService = new Service.FriendListClient();
            SesionService.UpdatePlayerSession(idPlayer);
            InitializeComponent();
            InicializePieceMappings();
            InicializeLobby(idGame, idPlayer);
            ShowFriends(idPlayer);
        }

        public Lobby(int idGame, int idPlayer, bool invited)
        {
            this.IdPlayer = idPlayer;
            this.isInvited = invited;
            InstanceContext context = new InstanceContext(this);
            GameManagerClient = new Service.GameManagerClient(context);
            SesionService = new Service.FriendsClient(context);
            friendListService = new Service.FriendListClient();
            SesionService.UpdatePlayerSession(idPlayer);
            InitializeComponent();
            InicializePieceMappings();
            InicializeLobby(idGame, idPlayer);
            ShowFriends(idPlayer);
        }

        private void InicializePieceMappings()
        {
            pieceMappings = new Dictionary<string, (Border, Ellipse)>
            {
                { "brdClockPiece", (brdClockPiece, ellPiece1) },
                { "brdDuckPiece", (brdDuckPiece, ellPiece2) },
                { "brdCarPiece", (brdCarPiece, ellPiece3) },
                { "brdPlayDiscPiece", (brdPlayDiscPiece, ellPiece4) },
                { "brdMPiece", (brdMPiece, ellPiece5) },
                { "brdTVPiece", (brdTVPiece, ellPiece6) }
            };
        }

        private void InicializeLobby(int idGame, int idPlayer)
        {
            if (idGame == 0)
            {
                CreateGame();
            }
            else
            {
                LoadGame(idGame);
                butStartGame.Visibility = Visibility.Hidden;
            }
            lblshowCodeGame.Content = game.IdGame.ToString();
            butGoToLobbyWindow.Visibility = Visibility.Hidden;
            Service.PlayerClient playerClient = new Service.PlayerClient();
            lblTitleUsername.Content = playerClient.GetMyPlayersName(IdPlayer, game.IdGame);
        }

        public void ShowFriends(int idPlayer)
        {
            wpFriends.Children.Clear();
            foreach (FriendList friend in friendListService.GetFriends(idPlayer))
            {
                Border brdBackground = new Border
                {
                    Width = 600,
                    Height = 60,
                    Background = Brushes.LightBlue,
                    Margin = new Thickness(2),
                };

                Grid grid = new Grid();

                Ellipse ellUserStatus = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(-530, 1, 0, 0),
                };

                if (friend.IsOnline)
                {
                    ellUserStatus.Fill = Brushes.Green;
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

                grid.Children.Add(ellUserStatus);
                grid.Children.Add(lblUserName);
                brdBackground.Child = grid;
                wpFriends.Children.Add(brdBackground);
            }

        }

        public void PieceSelected(object sender, MouseButtonEventArgs e)
        {
            if (piecePlayer.Name != null)
            {
                ResetPiece();
            }
            if (sender is Border selectedToken && pieceMappings.TryGetValue(selectedToken.Name, out var pieces))
            {
                pieces.Item2.Fill = new SolidColorBrush(Colors.Aquamarine);
                piecePlayer.ImagenSource = $"..\\ImageResourceManager\\{selectedToken.Name}.png";
                piecePlayer.Name = selectedToken.Name;
                GameManagerClient.SelectedPiece(game, piecePlayer.Name);
            }
            butGoToLobbyWindow.Visibility = Visibility.Visible;
        }

        public void ResetPiece()
        {
            if (pieceMappings.TryGetValue(piecePlayer.Name, out var pieces))
            {
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF061A2E");
                GameManagerClient.UnSelectedPiece(game, piecePlayer.Name);
            }
        }

        private void GoMainMenuGameWindow(object sender, RoutedEventArgs e)
        {
            if (isInvited)
            {
                OpenMainWindow();
            }
            else
            {
                OpenMainMenuGame(IdPlayer);
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


        private void NavigateToCreateGameWindow(object sender, RoutedEventArgs e)
        {
            GameManagerClient.UnSelectedPiece(this.game, piecePlayer.Name);
            butGoToLobbyWindow.Visibility = Visibility.Hidden;
            grdLobbyWindow.Visibility = Visibility.Collapsed;
            grdCreateGameWindow.Visibility = Visibility.Visible;
        }

        private void NavigateToLobbyWindow(object sender, RoutedEventArgs e)
        {
            imgPlayerCard.Source = ImageManager.GetSourceImage(piecePlayer.ImagenSource);
            GameManagerClient.SelectedPiece(this.game, piecePlayer.Name);
            grdLobbyWindow.Visibility = Visibility.Visible;
            grdCreateGameWindow.Visibility = Visibility.Collapsed;
            GameManagerClient.UpdatePlayerGame(this.game, this.IdPlayer);
        }

        private void GoGameWindow(object sender, RoutedEventArgs e)
        {
            GameManagerClient.StartGame(this.game);
            GameManagerClient.InitializeGame(this.game);
        }

        private int GenerateGameCode()
        {
            Random random = new Random();
            int GameCode = random.Next(10000, 99999);
            return GameCode;
        }

        private void CreateGame()
        {
            Player player = GetPlayerInfo(IdPlayer);
            game.IdGame = GenerateGameCode();
            game.Slot = 3;
            game.Status = Service.Game.Game_Situation.ByStart;
            GameManagerClient.AddGame(game);
            GameManagerClient.AddPlayerToGame(game.IdGame, player);
            GameManagerClient.UpdatePlayers(this.game.IdGame);
        }

        private void LoadGame(int idGame)
        {
            this.game = PlayerService.GetGame(idGame);
            if (isInvited)
            {
                GameManagerClient.AddGuestToGame(idGame, IdPlayer);
            }
            else
            {
                Player player = GetPlayerInfo(IdPlayer);
                GameManagerClient.AddPlayerToGame(idGame, player);
            }
            
            GameManagerClient.UpdatePlayers(this.game.IdGame);
        }

        private Service.Player GetPlayerInfo(int idPlayer)
        {
            Service.PlayerClient playerClient = new Service.PlayerClient();
            Service.Player player = new Service.Player();
            player.IdPlayer = idPlayer;
            player.Name = playerClient.GetPlayerName(idPlayer);
            player.properties = null;
            player.Position = -1;
            player.Jail = false;
            player.Loser = false;
            player.Money = 10000;
            player.Token = piecePlayer;
            return player;
        }

        public void AddVisualPlayers()
        {
            wpPlayers.Children.Clear();
            foreach (var player in game.PlayersInGame)
            {
                Border brdBackground = new Border
                {
                    Width = 650,
                    Height = 60,
                    Background = Brushes.LightBlue,
                    Margin = new Thickness(2),
                };

                Grid grid = new Grid();

                Label lblUserName = new Label
                {
                    Content = player.Name,
                    FontSize = 28,
                    Margin = new Thickness(10, 6, 0, 0),
                };

                grid.Children.Add(lblUserName);
                brdBackground.Child = grid;
                wpPlayers.Children.Add(brdBackground);
            }
        }

        public int UpdateGame()
        {
            this.game = PlayerService.GetGame(game.IdGame);
            return game.Players.Count;
        }

        public void UpdateFriendRequest()
        {
            return;
        }

        public void UpdateFriendDisplay()
        {
            Console.WriteLine("AQUI PASA");
            ShowFriends(IdPlayer);
            Console.WriteLine("AQUI YA NO");
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

            Grid grid = new Grid();

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

            grid.Children.Add(txtMessageContent);
            brdBackground.Child = grid;
            wpChatMessages.Children.Add(brdBackground);
        }

        private void SendMessage(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text))
            {
                string PlayerName = GetPlayerInfo(IdPlayer).Name;
                string message = PlayerName + ": " + Message.Text.Trim();
                GameManagerClient.SendMessage(game.IdGame, message);
                Message.Text = "";
            }
        }

        private void SendMessageEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(Message.Text))
            {
                Service.PlayerClient playerClient = new Service.PlayerClient();
                string message = playerClient.GetMyPlayersName(IdPlayer, game.IdGame) + ": " + Message.Text.Trim();
                GameManagerClient.SendMessage(game.IdGame, message);
                Message.Text = "";
            }
        }

        public void MoveToGame(Service.Game game)
        {
            Game gameWindow = new Game(this.game, GetPlayerInfo(IdPlayer));
            this.Close();
            gameWindow.Show();
        }

        public void PreparePieces(Service.Game game, Service.Player[] playersInGame)
        {
            int partNumber = 0;
            foreach (var player in playersInGame)
            {
                Game.pieces[partNumber].Source = ImageManager.GetSourceImage(player.Token.ImagenSource);
                Game.pieces[partNumber].Visibility = Visibility.Visible;
                partNumber++;
            }
        }

        public void BlockPiece(string piece)
        {
            if (pieceMappings.TryGetValue(piece, out var pieces))
            {
                pieces.Item1.IsEnabled = false;
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#BF0E0E");
            }
        }

        public void UnblockPiece(string piece)
        {
            if (pieceMappings.TryGetValue(piece, out var pieces))
            {
                pieces.Item1.IsEnabled = true;
                pieces.Item2.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF061A2E");
            }
        }

        public Service.Piece UptdatePiecePlayer(Service.Game game)
        {
            return piecePlayer;
        }

        private void btnCopiarAlPortapapeles_Click(object sender, MouseButtonEventArgs e)
        {
            string contenidoLabel = lblshowCodeGame.Content.ToString();
            Clipboard.SetText(contenidoLabel);
            MessageBox.Show("Contenido copiado al portapapeles", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
