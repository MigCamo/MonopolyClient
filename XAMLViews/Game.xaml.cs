using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UIGameClientTourist.GameLogic;
using UIGameClientTourist.Service;
using Property = UIGameClientTourist.Service.Property;
using WinImage = System.Windows.Controls.Image;


namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Game.xaml
    /// </summary>
    public partial class Game : Window, IGameLogicManagerCallback
    {
        private readonly Service.Player player;
        private Service.Game game;
        private Service.GameLogicManagerClient managerClient;
        public readonly static WinImage[] pieces = new WinImage[4];
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
        }

        private void InitializeUsers()
        {
            imgPlayerPiece.Source = ImageManager.GetSourceImage(player.Token.ImagenSource);
            lblPlayerTurn.Content = game.Players.Peek().Name;
            lblPlayerTurn.HorizontalContentAlignment = HorizontalAlignment.Center;
            lblPlayerTurn.VerticalContentAlignment = VerticalAlignment.Center;
            Service.PlayerClient playerClient = new Service.PlayerClient();
            lblUserName.Content = playerClient.GetMyPlayersName(player.IdPlayer, game.IdGame);
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
                butRollingDice.Visibility = Visibility.Collapsed;
                butEndTurn.Visibility = Visibility.Collapsed;
                wpMyPropietiers.IsEnabled = false;
                e.Handled = true;
            }
        }

        private void RestartGame(object sender, RoutedEventArgs e)
        {
            grdPauseMenu.Visibility = Visibility.Collapsed;
            butEndTurn.Visibility = Visibility.Visible;
            butRollingDice.Visibility = Visibility.Visible;
            wpMyPropietiers.IsEnabled = true;
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
            butEndTurn.Visibility = Visibility.Visible;
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

        private void UpdatePropertyCard(Property property)
        {
            imgPropertyPictureBox.Source = ImageManager.GetSourceImage(property.ImageSource);
            lblPropertyTitle.Content = property.Name;
            rectPropertyColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(property.Color));
            lblCostRentWithoutHouses.Content = $"£ {(int)Math.Round(property.BuyingCost * 0.15)}";
            lblCostRentWithAHouse.Content = $"£ {(int)Math.Round(property.BuyingCost * 0.45)}";
            lblCostRentWithTwoHouses.Content = $"£ {(int)Math.Round(property.BuyingCost * 1.35)}";
            lblCostRentWithThreeHouses.Content = $"£ {(int)Math.Round(property.BuyingCost * 2.25)}";
            lblCostRentWithFourHouses.Content = $"£ {(int)Math.Round(property.BuyingCost * 3.15)}";
            lblCostRentWithHotel.Content = $"£ {(int)Math.Round(property.BuyingCost * 4.00)}";
            lblCostProperty.Content = $"M {property.BuyingCost}.00";
        }

        private void UpdateTurnVisual()
        {
            lblPlayerTurn.Content = game.Players.Peek().Name;
        }

        private void EndTurn(object sender, RoutedEventArgs e)
        {
            butRollingDice.Visibility = Visibility.Visible;
            butEndTurn.Visibility = Visibility.Collapsed;
            butRollingDice.IsEnabled = false;
            managerClient.UpdateQueu(game.IdGame);
        }

        void IGameLogicManagerCallback.PlayDie(int firstDieValue, int SecondDieValue)
        {
            string imagePathDieOne = $"..\\GameResources\\Pictures\\Side_{firstDieValue}.png";
            string imagePathDieSecond = $"..\\GameResources\\Pictures\\Side_{SecondDieValue}.png";
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

        private Property SelectedProperty;

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

                if (property.IsMortgaged == true)
                {
                    Label lblIsMortgaged = new Label
                    {
                        Content = "Propiedad Enbargada",
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(58, 25, 0, 0),
                    };
                    grdContainer.Children.Add(lblIsMortgaged);
                }

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


        private void ViewDetailsOfMyProperty(object sender, MouseButtonEventArgs e, Property property)
        {
            ShowProperty(property);
            grdPropertyModificationButtonGroup.Visibility = Visibility.Visible;
            wpMyPropietiers.IsEnabled = false;

            if (EnableHouseControls(property))
            {
                EnableHotelControl(property);
            }

            ShowMortgageControls(property);
            SelectedProperty = property;
        }

        private bool EnableHouseControls(Property property)
        {
            bool enableHouseControls = false;

            if (VerifyCompletePropertySet(property.Color))
            {
                cboNumerHouse.IsEnabled = true;
                enableHouseControls = property.NumberHouses == 4;
            }

            return enableHouseControls;
        }

        private void EnableHotelControl(Property property)
        {
            if (property.NumberHouses == 4)
            {
                cboHotel.IsEnabled = true;
            }
        }

        private void ShowMortgageControls(Property property)
        {
            if (property.IsMortgaged)
            {
                butPayMortgage.Visibility = Visibility.Visible;
            }
            else
            {
                butMortgageProperty.Visibility = Visibility.Visible;
            }
        }


        void ShowProperty(Property property)
        {
            grdAddHotel.Visibility = Visibility.Visible;
            grdAddHouse.Visibility = Visibility.Visible;
            grdPropertyPurchase.Visibility = Visibility.Visible;
            grdPropertySquare.Visibility = Visibility.Visible;
            lblNumberHouses.Content = property.NumberHouses;
            UpdatePropertyCard(property);
        }

        private void BuyProperty(object sender, RoutedEventArgs e)
        {
            var lastProperty = Myproperties.LastOrDefault();

            if (CanAffordProperty(lastProperty))
            {
                HandlePropertyPurchase(ref lastProperty);
                managerClient.PurchaseProperty(lastProperty, player, game.IdGame);
                player.Money -= lastProperty.DefinitiveCost;
                ShowMyProperties();
                ClosePropertyManagementPanel();
            }
            else
            {
                MessageBox.Show("No te alcanza", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanAffordProperty(Property property)
        {
            int numHouses = ((int?)cboNumerHouse.SelectedItem) ?? 0;
            return player.Money - (property.BuyingCost + (numHouses * GetHouseCostByColor(property.Color))) >= 0;
        }


        private void HandlePropertyPurchase(ref Property property)
        {
            if (cboHotel.SelectedItem != null && cboHotel.SelectedItem.ToString() == "Si")
            {
                HandleHotelPurchase(ref property);
            }
            else if (cboNumerHouse.SelectedItem != null)
            {
                HandleHousePurchase(ref property, (int)cboNumerHouse.SelectedItem);
            }
            else
            {
                HandleStandardPropertyPurchase(property);
            }
        }

        private void HandleHotelPurchase(ref Property property)
        {
            property.Situation = Property.PropertySituation.Hotel;
            property.Taxes = (long)(property.BuyingCost * 4.00);
            property.DefinitiveCost += GetHotelCostByColor(property.Color);
        }

        private void HandleHousePurchase(ref Property property, int numberOfHouses)
        {
            property.NumberHouses = numberOfHouses;
            property.Situation = Property.PropertySituation.House;
            property.Taxes = CalculateRentalCost(numberOfHouses, property.BuyingCost, property.Color);
            property.DefinitiveCost += numberOfHouses * GetHouseCostByColor(property.Color);
        }

        private void HandleStandardPropertyPurchase(Property property)
        {
            property.Situation = Property.PropertySituation.Bought;
        }

        private long CalculateRentalCost(int numberOfHouses, long costOfProperty, string propertyColor)
        {
            long rentalCost = 0;

            switch (numberOfHouses)
            {
                case 0:
                    rentalCost = VerifyCompletePropertySet(propertyColor)
                        ? (long)Math.Round(0.15 * costOfProperty * 2)
                        : (long)Math.Round(0.15 * costOfProperty); break;
                case 1:
                    rentalCost = (long)(0.0875 * costOfProperty); break;
                case 2:
                    rentalCost = (long)(1.35 * costOfProperty); break;
                case 3:
                    rentalCost = (long)(2.25 * costOfProperty); break;
                case 4:
                    rentalCost = (long)(3.15 * costOfProperty); break;
            }

            return rentalCost;
        }

        public int GetHouseCostByColor(string color)
        {
            int cost = 0;
            Dictionary<string, int> houseCostsByColor = new Dictionary<string, int>
            {
                { "#955436", 70 },
                { "#AAE0FA", 300 },
                { "#FF5590", 100},
                { "#F19C21", 200},
                { "#DA343A", 300},
                { "#FEF100", 300},
                { "#1EB35A", 100},
                { "#0172BB", 200}
            };

            if (houseCostsByColor.TryGetValue(color, out int houseCost))
            {
                cost = houseCost;
            }

            return cost;
        }

        public int GetHotelCostByColor(string color)
        {
            int cost = 0;
            Dictionary<string, int> hotelCostsByColor = new Dictionary<string, int>
            {
                { "#955436", 90 },
                { "#AAE0FA", 340 },
                { "#FF5590", 140},
                { "#F19C21", 240},
                { "#DA343A", 340},
                { "#FEF100", 340},
                { "#1EB35A", 140},
                { "#0172BB", 240}
            };

            if (hotelCostsByColor.TryGetValue(color, out int hotelCost))
            {
                cost = hotelCost;
            }

            return cost;
        }

        private void ClosePropertyManagementPanel()
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPropertySquare.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Collapsed;
            grdAddHotel.Visibility = Visibility.Collapsed;
            grdPropertyPurchaseButtonGroup.Visibility = Visibility.Collapsed;
            wpMyPropietiers.IsEnabled = true;
            grdMenu.Visibility = Visibility.Collapsed;
            cboHotel.SelectedItem = null;
            cboNumerHouse.SelectedItem = null;
            cboHotel.IsEnabled = false;
            cboNumerHouse.IsEnabled = false;
            DefinitivePropertyCost.Content = " ";
        }

        private void ShowPropertyPurchaseContract(object sender, RoutedEventArgs e)
        {
            grdMenu.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Visible;
            grdAddHotel.Visibility = Visibility.Visible;
            grdPropertyPurchaseButtonGroup.Visibility = Visibility.Visible;
            wpMyPropietiers.IsEnabled = false;

            DefinitivePropertyCost.Content = Myproperties[Myproperties.Count - 1].DefinitiveCost;

            if (VerifyCompletePropertySet(Myproperties[Myproperties.Count - 1].Color))
            {
                cboNumerHouse.IsEnabled = true;
            }
        }

        private bool VerifyCompletePropertySet(string color)
        {
            bool result = false;
            int numberProperties = 0;

            foreach (var property in Myproperties)
            {
                if (property.Color == color)
                {
                    numberProperties++;
                }
            }

            if (numberProperties == 4)
            {
                result = true;
            }

            return result;
        }

        private void DeclineToPurchaseProperty(object sender, RoutedEventArgs e)
        {
            Myproperties.Remove(Myproperties[Myproperties.Count - 1]);
            ClosePropertyManagementPanel();
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

        void IGameLogicManagerCallback.ShowCard(Property property)
        {
            grdPropertyPurchase.Visibility = Visibility.Visible;

            if (property.Name == "Carcel")
            {
                ShowJailSquare();
            }
            else if (property.Name == "Cuervo Mensajero" || property.Name == "Barco Velero" || property.Name == "Inicio")
            {
                ShowEventSquare();
            }
            else
            {
                ShowPropertySquare(property);
            }
        }

        private void ShowJailSquare()
        {
            grdPrisonSquare.Visibility = Visibility.Visible;
            butCloseEvento.Visibility = Visibility.Visible;
            managerClient.JailPlayer(game.IdGame, player.IdPlayer);
        }

        private void ShowEventSquare()
        {
            grdEventSquare.Visibility = Visibility.Visible;
            butCloseEvento.Visibility = Visibility.Visible;
            //managerClient.GetActionCard(game.IdGame);
        }

        private void ShowPropertySquare(Property property)
        {
            grdMenu.Visibility = Visibility.Visible;
            grdPropertySquare.Visibility = Visibility.Visible;
            wpMyPropietiers.IsEnabled = false;
            lblNumberHouses.Content = property.NumberHouses;
            UpdatePropertyCard(property);
            Myproperties.Add(property);
        }

        private void CloseEventCard(object sender, RoutedEventArgs e)
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPrisonSquare.Visibility = Visibility.Collapsed;
            grdEventSquare.Visibility = Visibility.Collapsed;
            butCloseEvento.Visibility = Visibility.Collapsed;
        }

        public void OpenAuctionWindow(Property property)
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
            await Console.Out.WriteLineAsync("Pasaron 3 segundos");
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
                if (!player.Jail)
                {
                    this.butRollingDice.IsEnabled = true;
                }
                else
                {
                    player.Jail = false;
                    managerClient.UpdateQueu(game.IdGame);
                }
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

                string color = "#FFE5E5E5";

                if (friend.Loser)
                {
                    color = "#515A5A";
                }
                else if (friend.Jail)
                {
                    color = "#B03A2E";
                }

                Rectangle rectBackground = new Rectangle
                {
                    Height = 100,
                    Width = 256,
                    RadiusX = 15,
                    RadiusY = 15,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F6924")),
                };
                grdContainer.Children.Add(rectBackground);

                Ellipse ellipse = new Ellipse
                {
                    Height = 80,
                    Width = 79,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
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

                if (friend.IdPlayer == player.IdPlayer)
                {
                    lblPlayerMoney.Content = $" M {friend.Money}.00 ";
                }
            }

            scrollViewer.Content = stackPanelContainer;
            PlayersInGame.Children.Add(scrollViewer);
        }

        public void ShowEvent(int action)
        {
            Console.WriteLine(action);
        }

        public void NotifyPlayerOfEvent(int messageNumber)
        {
            grdPropertyPurchase.Visibility = Visibility.Visible;
            grdRent.Visibility = Visibility.Visible;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (sender, e) =>
            {
                grdPropertyPurchase.Visibility = Visibility.Collapsed;
                grdRent.Visibility = Visibility.Collapsed;
                timer.Stop();
            };

            timer.Start();
        }

        public void UpgradePlayerMoney(long money)
        {
            lblPlayerMoney.Content = $" M {money}.00 ";
            player.Money = money;
        }

        private void CancelPurchase(object sender, RoutedEventArgs e)
        {
            grdAddHouse.Visibility = Visibility.Collapsed;
            grdAddHotel.Visibility = Visibility.Collapsed;
            grdPropertyPurchaseButtonGroup.Visibility = Visibility.Collapsed;
            DefinitivePropertyCost.Content = " ";
            DefinitivePropertyCost.Visibility = Visibility.Collapsed;
            wpMyPropietiers.IsEnabled = true;
            grdMenu.Visibility = Visibility.Visible;
        }

        private void MortgageProperty(object sender, RoutedEventArgs e)
        {
            managerClient.RealizePropertyMortgage(game.IdGame, SelectedProperty, player.IdPlayer);
            SelectedProperty.IsMortgaged = true;
            butPayMortgage.Visibility = Visibility.Visible;
            butMortgageProperty.Visibility = Visibility.Collapsed;
            ShowMyProperties();
        }

        public void RemoveGamePiece(Service.Player player)
        {
            pieces[player.Token.PartNumber].Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            grdWinnerMural.Visibility = Visibility.Collapsed;
        }

        public void EndGame(int idWinner)
        {
            butRollingDice.IsEnabled = false;
            butEndTurn.IsEnabled = false;

            if (player.IdPlayer == idWinner)
            {
                grdWinnerMural.Visibility = Visibility.Visible;
            }
        }

        private void CancelAddHomesAndHotels(object sender, RoutedEventArgs e)
        {
            CloseAddHomesAndHotels();
        }

        private void CloseAddHomesAndHotels()
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPropertySquare.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Collapsed;
            grdAddHotel.Visibility = Visibility.Collapsed;
            grdPropertyModificationButtonGroup.Visibility = Visibility.Collapsed;
            butMortgageProperty.Visibility = Visibility.Collapsed;
            butPayMortgage.Visibility = Visibility.Collapsed;
            cboHotel.SelectedItem = null;
            cboNumerHouse.SelectedItem = null;
            cboHotel.IsEnabled = false;
            cboNumerHouse.IsEnabled = false;
            DefinitivePropertyCost.Content = " ";
            wpMyPropietiers.IsEnabled = true;
        }

        private void AddHomesAndHotels(object sender, RoutedEventArgs e)
        {
            if (cboNumerHouse.SelectedItem != null)
            {
                int numberHouse = int.Parse(cboNumerHouse.SelectedItem.ToString());
                if (numberHouse + SelectedProperty.NumberHouses <= 4)
                {

                }
                else
                {
                    MessageBox.Show("El numero maximo de construcción de casas por propiedades es de 4 casas", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Selecciona un numero de casas por favor ", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PayMortgage(object sender, RoutedEventArgs e)
        {
            butMortgageProperty.Visibility = Visibility.Visible;
            butPayMortgage.Visibility = Visibility.Collapsed;
            SelectedProperty.IsMortgaged = false;
            managerClient.PayPropertyMortgage(game, player.IdPlayer, SelectedProperty);
            ShowMyProperties();
        }

        public void UpdatePropertyStatus(Property property)
        {
            foreach (var propertyAux in Myproperties)
            {
                if (propertyAux.Name == property.Name)
                {
                    propertyAux.IsMortgaged = true;
                    break;
                }
            }

            ShowMyProperties();
        }

        private string GetMessege(int messageNumber)
        {
            string message = "";

            switch (messageNumber)
            {
                case 0:
                    message = ""; break;
                case 1:
                    message = ""; break;
                case 2:
                    message = ""; break;
                case 3:
                    message = ""; break;
                case 4:
                    message = ""; break;
                case 5:
                    message = ""; break;
                case 6:
                    message = ""; break;
                case 7:
                    message = ""; break;
                case 8:
                    message = ""; break;
            }

            return message;
        }

        public void GoToJail()
        {
            throw new NotImplementedException();
        }

        public void NotifyPlayerOfEvent(string message)
        {
            throw new NotImplementedException();
        }
    }
}

