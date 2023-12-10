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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UIGameClientTourist.GameLogic;
using UIGameClientTourist.Service;
using WinImage = System.Windows.Controls.Image;


namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Game.xaml
    /// </summary>
    public partial class Game : Window, IGameLogicManagerCallback
    {
        private Service.Player player;
        private Service.Game game;
        private Service.GameLogicManagerClient managerClient;
        readonly public static WinImage[] pieces = new WinImage[4];
        private List<Service.Property> Myproperties;
        private Bid LastBid = new Bid();

        public Game(Service.Game game, Service.Player player)
        {
            InstanceContext context = new InstanceContext(this);
            managerClient = new Service.GameLogicManagerClient(context);
            managerClient.UpdatePlayerService(player.IdPlayer, game.IdGame);
            this.player = player;
            this.game = game;
            InitializeComponent();
            InitializeUsers();
            InitializeBoardPieces();
            InitializeComboBoxes();
            if (game.Players.Peek().IdPlayer != player.IdPlayer)
            {
                butRollingDice.IsEnabled = false;
            }
        }

        private void InitializeUsers()
        {
            imgPlayerPiece.Source = ImageManager.GetSourceImage(player.Token.ImagenSource);
            lblPlayerTurn.Content = game.Players.Peek().Name;
            lblPlayerTurn.HorizontalContentAlignment = HorizontalAlignment.Center;
            lblPlayerTurn.VerticalContentAlignment = VerticalAlignment.Center;
            lblUserName.Content = player.Name;
            lblPlayerMoney.Content = $" M {player.Money}.00 ";
            Myproperties = new List<Service.Property>();
        }

        private void InitializeComboBoxes()
        {
            cboNumerHouse.Items.Add(1);
            cboNumerHouse.Items.Add(2);
            cboNumerHouse.Items.Add(3);
            cboNumerHouse.Items.Add(4);
            cboHotel.Items.Add("Si");
            cboHotel.Items.Add("No");
        }

        private void InitializeBoardPieces()
        {
            pieces[0] = imgPiece1;
            pieces[1] = imgPiece2;
            pieces[2] = imgPiece3;
            pieces[3] = imgPiece4;
        }

        private void PauseGame(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                grdPauseMenu.Visibility = Visibility.Visible;
                butRollingDice.IsEnabled = false;
                e.Handled = true;
            }
        }

        private void RestartGame(object sender, RoutedEventArgs e)
        {
            grdPauseMenu.Visibility = Visibility.Collapsed;
            butRollingDice.IsEnabled = true;
        }

        private void GoToMenuFromGame(object sender, RoutedEventArgs e)
        {
            MainMenuGame mainMenuGameWindow = new MainMenuGame(player.IdPlayer);
            this.Close();
            mainMenuGameWindow.Show();
        }

        private void AnimationMovePlayerCardX(WinImage piece, int updateXPosition)
        {
            DoubleAnimation animationMoveOverX = new DoubleAnimation
            {
                To = updateXPosition
            };
            piece.BeginAnimation(Canvas.LeftProperty, animationMoveOverX);
        }

        private void AnimationMovePlayerCardY(WinImage piece, int updateYPosition)
        {
            DoubleAnimation animationMoveOverY = new DoubleAnimation
            {
                To = updateYPosition
            };
            piece.BeginAnimation(Canvas.TopProperty, animationMoveOverY);
        }

        private void PlayMyTurn(object sender, RoutedEventArgs e)
        {
            managerClient.PlayTurn(this.game);
            butRollingDice.Visibility = Visibility.Collapsed;
            ButEndTurn.Visibility = Visibility.Visible;
        }
        private void AccommodatePart(WinImage piece, Service.Player player, Service.Property property)
        {
            int initialXPiece = 26;
            int initialYPiece = 34;
            int PiecesInSameSquare = CheckPieceCountInSquare(player);

            if (PiecesInSameSquare == 2)
            {
                AnimationMovePlayerCardX(piece, initialXPiece + property.PosicitionX);
            }
            if (PiecesInSameSquare == 3)
            {
                AnimationMovePlayerCardY(piece, initialYPiece + property.PosicitionY);
            }
            if (PiecesInSameSquare == 4)
            {
                AnimationMovePlayerCardY(piece, initialYPiece + property.PosicitionY);
                AnimationMovePlayerCardX(piece, initialXPiece + property.PosicitionX);
            }
        }

        private int CheckPieceCountInSquare(Service.Player player)
        {
            int cont = 0;
            foreach (var playerAux in game.Players)
            {
                if (player.Position == playerAux.Position) cont++;
            }
            return cont;
        }

        private void UpdateDieFace(string imagePath, WinImage die)
        {
            die.Source = ImageManager.GetSourceImage(imagePath);
        }

        private void UpdatePropertyCard(Service.Property property)
        {
            imgPropertyPictureBox.Source = ImageManager.GetSourceImage(property.ImageSource);
            lblPropertyTitle.Content = property.Name;
            rectPropertyColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(property.Color));
        }

        private void UpdateTurnVisual()
        {
            lblPlayerTurn.Content = game.Players.Peek().Name;
        }

        private void EndTurn(object sender, RoutedEventArgs e)
        {
            butRollingDice.Visibility = Visibility.Visible;
            ButEndTurn.Visibility = Visibility.Collapsed;
            butRollingDice.IsEnabled = false;
            managerClient.UpdateQueu(game.IdGame);
        }

        void IGameLogicManagerCallback.PlayDie(int firstDieValue, int SecondDieValue)
        {
            string imagePathDieOne = $"..\\ImageResourceManager\\Side_{firstDieValue}.png";
            string imagePathDieSecond = $"..\\ImageResourceManager\\Side_{SecondDieValue}.png";
            UpdateDieFace(imagePathDieOne, imgDieOne);
            UpdateDieFace(imagePathDieSecond, imgDieSecond);
        }

        public void MovePlayerPieceOnBoard(Service.Player player, Service.Property property)
        {

            if (player.Position > 0)
            {
                player.Position--;
            }
            if (player.Position < 10)
            {
                AnimationMovePlayerCardY(pieces[player.Token.PartNumber], property.PosicitionY);
                AnimationMovePlayerCardX(pieces[player.Token.PartNumber], property.PosicitionX);
                AccommodatePart(pieces[player.Token.PartNumber], player, property);
            }
            else if (player.Position > 9 && player.Position < 20)
            {
                AnimationMovePlayerCardX(pieces[player.Token.PartNumber], 15);
                AnimationMovePlayerCardY(pieces[player.Token.PartNumber], property.PosicitionY);
                AccommodatePart(pieces[player.Token.PartNumber], player, property);
            }
            else if (player.Position > 19 && player.Position < 30)
            {
                AnimationMovePlayerCardY(pieces[player.Token.PartNumber], -50);
                AnimationMovePlayerCardX(pieces[player.Token.PartNumber], property.PosicitionX);
                AccommodatePart(pieces[player.Token.PartNumber], player, property);
            }
            else if (player.Position > 29 && player.Position < 40)
            {
                AnimationMovePlayerCardX(pieces[player.Token.PartNumber], 664);
                AnimationMovePlayerCardY(pieces[player.Token.PartNumber], property.PosicitionY);
                AccommodatePart(pieces[player.Token.PartNumber], player, property);
            }
        }

        public void ShowMyProperties()
        {
            wpMyPropietiers.Children.Clear();

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            StackPanel stackPanelContainer = new StackPanel();

            foreach (var property in Myproperties)
            {
                Grid grdContainer = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 3),
                };

                Rectangle rectBackground = new Rectangle
                {
                    Height = 50,
                    Width = 275,
                    Fill = new SolidColorBrush(Color.FromRgb(54, 54, 56)),
                };
                grdContainer.Children.Add(rectBackground);

                Rectangle rectStreetColor = new Rectangle
                {
                    Height = 50,
                    Width = 16,
                    Fill = (Brush)new BrushConverter().ConvertFromString(property.Color),
                    Margin = new Thickness(-257, 0, 0, 0),
                };
                grdContainer.Children.Add(rectStreetColor);

                Label lblName = new Label
                {
                    Content = property.Name,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 252, 252)),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(18, 6, 0, 0),
                };
                grdContainer.Children.Add(lblName);

                Label lblCost = new Label
                {
                    Content = property.BuyingCost,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 252, 252)),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(18, 25, 0, 0),
                };
                grdContainer.Children.Add(lblCost);

                Image imgProperty = new Image
                {
                    Height = 50,
                    Width = 88,
                    Source = ImageManager.GetSourceImage(property.ImageSource),
                    Stretch = Stretch.UniformToFill,
                    Margin = new Thickness(183, 0, 0, 0),
                };

                grdContainer.Children.Add(imgProperty);
                grdContainer.PreviewMouseLeftButtonDown += (sender, e) => ViewDetailsOfMyProperty(sender, e, property);
                stackPanelContainer.Children.Add(grdContainer);
            }

            scrollViewer.Content = stackPanelContainer;
            wpMyPropietiers.Children.Add(scrollViewer);
        }


        private void ViewDetailsOfMyProperty(object sender, MouseButtonEventArgs e, Service.Property properties)
        {
            ShowProperty(properties);
            butCloseEvento.Visibility = Visibility.Visible;
        }

        void ShowProperty(Service.Property property)
        {
            grdPropertyPurchase.Visibility = Visibility.Visible;
            grdPropertySquare.Visibility = Visibility.Visible;
            UpdatePropertyCard(property);
        }

        private void BuyProperty(object sender, RoutedEventArgs e)
        {
            if (player.Money - Myproperties[Myproperties.Count - 1].BuyingCost >= 0)
            {

                if (cboHotel.SelectedItem != null && cboHotel.SelectedItem.ToString() == "Si")
                {
                    Myproperties[Myproperties.Count - 1].Situation = Service.Property.Property_Situation.Hotel;
                    player.Money -= Myproperties[Myproperties.Count - 1].BuyingCost + 500;
                }
                else if (cboNumerHouse.SelectedItem != null)
                {
                    Myproperties[Myproperties.Count - 1].NumberHouses = (int)cboNumerHouse.SelectedItem;
                    Myproperties[Myproperties.Count - 1].Situation = Service.Property.Property_Situation.House;
                    player.Money -= Myproperties[Myproperties.Count - 1].BuyingCost + ((int)cboNumerHouse.SelectedItem * 50);
                }
                else
                {
                    Myproperties[Myproperties.Count - 1].Situation = Service.Property.Property_Situation.Bought;
                    player.Money -= Myproperties[Myproperties.Count - 1].BuyingCost;
                }
                managerClient.PurchaseProperty(Myproperties[Myproperties.Count - 1], player, game.IdGame);
                lblPlayerMoney.Content = $" M {player.Money}.00 ";
                ShowMyProperties();
                ClosePropertyPurchase();
            }
            else
            {
                MessageBox.Show("No te alcanza", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        void ClosePropertyPurchase()
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPropertySquare.Visibility = Visibility.Collapsed;
            grdPrisonSquare.Visibility = Visibility.Collapsed;
            grdEventSquare.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Collapsed;
            grdAddHotel.Visibility = Visibility.Collapsed;
            butCloseEvento.Visibility = Visibility.Collapsed;
            butRollingDice.IsEnabled = true;
        }

        private void ShowPropertyPurchaseContract(object sender, RoutedEventArgs e)
        {
            grdMenu.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Visible;
            if (Myproperties[Myproperties.Count - 1].NumberHouses == 4)
            {
                grdAddHotel.Visibility = Visibility.Visible;
            }
        }

        private void DeclineToPurchaseProperty(object sender, RoutedEventArgs e)
        {
            ClosePropertyPurchase();
            managerClient.StartAuction(game.IdGame, Myproperties[Myproperties.Count - 1]);
        }

        private void AddHotelToProperty(object sender, SelectionChangedEventArgs e)
        {
            if (cboHotel.SelectedItem != null && cboHotel.SelectedItem.ToString() == "Si")
            {
                cboNumerHouse.IsEnabled = false;
                cboNumerHouse.SelectedItem = null;
                butBuyProperty.Visibility = Visibility.Visible;
            }
        }

        void IGameLogicManagerCallback.ShowCard(Service.Property property)
        {
            grdPropertyPurchase.Visibility = Visibility.Visible;
            if (property.Name == "Carcel")
            {
                grdPrisonSquare.Visibility = Visibility.Visible;
                butCloseEvento.Visibility = Visibility.Visible;
            }
            else if (property.Name == "Evento")
            {
                grdEventSquare.Visibility = Visibility.Visible;
                butCloseEvento.Visibility = Visibility.Visible;
            }
            else
            {
                grdMenu.Visibility = Visibility.Visible;
                grdPropertySquare.Visibility = Visibility.Visible;
                UpdatePropertyCard(property);
                Myproperties.Add(property);
            }
        }

        private void butCloseEvento_Click(object sender, RoutedEventArgs e)
        {
            ClosePropertyPurchase();
        }

        public void OpenAuctionWindow(Service.Property property)
        {
            btnBuy.Visibility = Visibility.Collapsed;
            btnDecline.Visibility = Visibility.Collapsed;
            grdPropertyPurchase.Visibility = Visibility.Visible;
            grdPropertySquare.Visibility = Visibility.Visible;
            BidWindow.Visibility = Visibility.Visible;
            LblBid.Visibility = Visibility.Collapsed;
            txtBid.Visibility = Visibility.Collapsed;
            btnBid.Visibility = Visibility.Collapsed;
            btnEndBid.Visibility = Visibility.Collapsed;

            if (game.Players.Peek().IdPlayer == player.IdPlayer)
            {
                btnEndBid.Visibility = Visibility.Visible;
            }
            else
            {
                LblBid.Visibility = Visibility.Visible;
                txtBid.Visibility = Visibility.Visible;
                btnBid.Visibility = Visibility.Visible;
            }

            UpdatePropertyCard(property);
        }

        private void EndBid(object sender, RoutedEventArgs e)
        {
            managerClient.StopAuction(this.game.IdGame, this.LastBid.IdPlayer, this.LastBid.BidMoney, this.Myproperties[this.Myproperties.Count - 1]);
        }

        private void MakeBid(object sender, RoutedEventArgs e)
        {
            btnBid.IsEnabled = false;
            int Bid = int.Parse(txtBid.Text);
            if (Bid > LastBid.BidMoney && Bid <= player.Money)
            {
                managerClient.MakeBid(game.IdGame, player.IdPlayer, Bid);
                _ = ColdDownBid();
            }
        }

        public void UpdateBids(int IdPlayer, int Bid)
        {
            Label LblBidObject = new Label();
            LblBidObject.Content = $" M {Bid}.00 ";
            Bids.Children.Insert(0, LblBidObject);
            this.LastBid.BidMoney = Bid;

            if (game.Players.Peek().IdPlayer == player.IdPlayer)
            {
                this.LastBid.IdPlayer = IdPlayer;
            }
        }

        async Task ColdDownBid()
        {
            await Task.Delay(5000);
            await Console.Out.WriteLineAsync("Pasaron 3 segunfos");
            btnBid.IsEnabled = true;
        }

        public void EndAuction(Service.Property property, int winner, int winnerBid)
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPropertySquare.Visibility = Visibility.Collapsed;
            this.BidWindow.Visibility = Visibility.Collapsed;
            if (game.Players.Peek().IdPlayer == player.IdPlayer)
            {
                this.Myproperties.RemoveAt(this.Myproperties.Count - 1);
            }
            else if (player.IdPlayer == winner)
            {
                this.Myproperties.Add(property);
                this.Myproperties[Myproperties.Count - 1].BuyingCost = winnerBid;
                this.player.Money -= winnerBid;
                managerClient.PurchaseProperty(Myproperties[Myproperties.Count - 1], player, game.IdGame);
                lblPlayerMoney.Content = $" M {player.Money}.00 ";
                ShowMyProperties();
            }
        }

        public void UpdateTurns(Queue<Service.Player> turns)
        {
            game.Players = turns;
            UpdateTurnVisual();
            if (game.Players.Peek().IdPlayer == player.IdPlayer)
            {
                this.butRollingDice.IsEnabled = true;
            }
        }

        public void LoadFriends(Queue<Service.Player> friends)
        {
            PlayersInGame.Children.Clear();

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            StackPanel stackPanelContainer = new StackPanel();

            foreach (var friend in friends)
            {
                Grid grdContainer = new Grid
                {
                    Margin = new Thickness(10, 0, 0, 1),
                };

                Rectangle rectBackground = new Rectangle
                {
                    Height = 100,
                    Width = 256,
                    RadiusX = 15,
                    RadiusY = 15,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5")),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F6924")),
                };
                grdContainer.Children.Add(rectBackground);

                Ellipse ellipse = new Ellipse
                {
                    Height = 80,
                    Width = 79,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E5E5")),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F6924")),
                    Margin = new Thickness(-150, 20, 0, 20),
                };
                grdContainer.Children.Add(ellipse);

                Label lblName = new Label
                {
                    Content = friend.Name,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F6924")),
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Arial Black"),
                    FontSize = 15,
                    Margin = new Thickness(120, 34, 0, 0),
                };
                grdContainer.Children.Add(lblName);

                Label lblCost = new Label
                {
                    Content = "M " + friend.Money.ToString() + ".00",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F6924")),
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Arial Rounded MT Bol"),
                    FontSize = 18,
                    Margin = new Thickness(110, 50, 0, 0),
                };
                grdContainer.Children.Add(lblCost);

                Image imgProperty = new Image
                {
                    Height = 50,
                    Width = 50,
                    Source = ImageManager.GetSourceImage(friend.Token.ImagenSource),
                    Stretch = Stretch.Fill,
                    Margin = new Thickness(-145, 20, 0, 20),
                };

                grdContainer.Children.Add(imgProperty);
                stackPanelContainer.Children.Add(grdContainer);
            }

            scrollViewer.Content = stackPanelContainer;
            PlayersInGame.Children.Add(scrollViewer);
        }
    }
}

