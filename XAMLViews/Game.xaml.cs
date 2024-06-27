using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
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
using static UIGameClientTourist.XAMLViews.Friends;
using Property = UIGameClientTourist.Service.Property;
using WinImage = System.Windows.Controls.Image;


namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Game.xaml
    /// </summary>
    public partial class Game : Window, IGameLogicManagerCallback
    {
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(Game));
        private readonly Service.Player _currentPlayer;
        private readonly Service.Game _currentGame;
        private readonly GameLogicManagerClient _managerClient;
        public readonly static WinImage[] _boardPieces = new WinImage[4];
        private List<Property> _listPropertiesPurchased;

        public Game(Service.Game game, Service.Player player)
        {
            InstanceContext context = new InstanceContext(this);
            InitializeComponent();
            InitializeBoardPieces();
            InitializeComboBoxes();

            try
            {
                _managerClient = new GameLogicManagerClient(context);
                _managerClient.UpdatePlayerService(player.IdPlayer, game.IdGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            this._currentPlayer = player;
            this._currentGame = game;

            InitializeUsers();
            
            if (this._currentPlayer.IdPlayer == game.Players.First().IdPlayer)
            {
                this.butRollingDice.IsEnabled = true;
            }

        }

        private void InitializeUsers()
        {
            imgPlayerPiece.Source = ImageManager.GetSourceImage(_currentPlayer.Piece.ImagenSource);
            lblPlayerTurn.Content = _currentGame.Players.Peek().Name;
            lblPlayerTurn.HorizontalContentAlignment = HorizontalAlignment.Center;
            lblPlayerTurn.VerticalContentAlignment = VerticalAlignment.Center;
            PlayerClient playerClient = new PlayerClient();

            try
            {
                lblUserName.Content = playerClient.GetMyPlayersName(_currentPlayer.IdPlayer, _currentGame.IdGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            lblPlayerMoney.Content = $" M {_currentPlayer.Money}.00 ";
            _listPropertiesPurchased = new List<Property>();
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
            _boardPieces[0] = imgPiece1;
            _boardPieces[1] = imgPiece2;
            _boardPieces[2] = imgPiece3;
            _boardPieces[3] = imgPiece4;
        }

        private void PauseGame(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                grdPauseMenu.Visibility = Visibility.Visible;
                e.Handled = true;
            }
        }

        private void ButtonClickRestartGame(object sender, RoutedEventArgs e)
        {
            grdPauseMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonClickGoToMenuFromGame(object sender, RoutedEventArgs e)
        {
            
            try
            {
                _managerClient.DeclareLosingPlayer(_currentPlayer, _currentGame.IdGame);

                if (_currentPlayer.Guest)
                {
                    ExitToGame();
                }
                else
                {
                    BackToMenu();
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

        public void ExitToGame()
        {
            Window window = new MainWindow();
            window.Show();
            this.Close();
        }

        public void BackToMenu()
        {
            MainMenuGame menu = new MainMenuGame(_currentPlayer.IdPlayer);
            menu.Show();
            this.Close();
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

        private void ButtonClickPlayTurn(object sender, RoutedEventArgs e)
        {
            try
            {
                _managerClient.PlayTurn(this._currentGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
                butRollingDice.IsEnabled = false;
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }
        }

        public void ShowButtonsForEnd()
        {
            butRollingDice.Visibility = Visibility.Collapsed;
            butEndTurn.Visibility = Visibility.Visible;
        }

        private void AccommodatePart(WinImage piece, Service.Player player, Property property)
        {
            int initialXPiece = 26;
            int initialYPiece = 34;
            int PiecesInSameSquare = CheckPieceCountInSquare(player);

            if (PiecesInSameSquare == 2)
            {
                AnimationMovePlayerCardX(piece, initialXPiece + property.PositionX);
            }
            if (PiecesInSameSquare == 3)
            {
                AnimationMovePlayerCardY(piece, initialYPiece + property.PositionY);
            }
            if (PiecesInSameSquare == 4)
            {
                AnimationMovePlayerCardY(piece, initialYPiece + property.PositionY);
                AnimationMovePlayerCardX(piece, initialXPiece + property.PositionX);
            }
        }

        private int CheckPieceCountInSquare(Service.Player player)
        {
            int cont = 0;
            foreach (var playerAux in _currentGame.Players)
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
            lblPropertySubtitle.Content = $"-  {property.Name}  -";
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
            lblPlayerTurn.Content = _currentGame.Players.Peek().Name;
        }
        private void ButtonClickEndTurn(object sender, RoutedEventArgs e)
        {
            butRollingDice.Visibility = Visibility.Visible;
            butEndTurn.Visibility = Visibility.Collapsed;
            butRollingDice.IsEnabled = false;

            try
            {
                _managerClient.UpdateQueu(_currentGame.IdGame);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
                AlertMessage();
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

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
                AnimationMovePlayerCardY(_boardPieces[player.Piece.PartNumber], property.PositionY);
                AnimationMovePlayerCardX(_boardPieces[player.Piece.PartNumber], property.PositionX);
                AccommodatePart(_boardPieces[player.Piece.PartNumber], player, property);
            }
            else if (player.Position > 9 && player.Position < 20)
            {
                AnimationMovePlayerCardX(_boardPieces[player.Piece.PartNumber], 15);
                AnimationMovePlayerCardY(_boardPieces[player.Piece.PartNumber], property.PositionY);
                AccommodatePart(_boardPieces[player.Piece.PartNumber], player, property);
            }
            else if (player.Position > 19 && player.Position < 30)
            {
                AnimationMovePlayerCardY(_boardPieces[player.Piece.PartNumber], -50);
                AnimationMovePlayerCardX(_boardPieces[player.Piece.PartNumber], property.PositionX);
                AccommodatePart(_boardPieces[player.Piece.PartNumber], player, property);
            }
            else if (player.Position > 29 && player.Position < 40)
            {
                AnimationMovePlayerCardX(_boardPieces[player.Piece.PartNumber], 664);
                AnimationMovePlayerCardY(_boardPieces[player.Piece.PartNumber], property.PositionY);
                AccommodatePart(_boardPieces[player.Piece.PartNumber], player, property);
            }
        }

        private Property _selectedProperty;

        public void ShowMyProperties()
        {
            wpMyPropietiers.Children.Clear();

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            StackPanel stackPanelContainer = new StackPanel();

            foreach (var property in _listPropertiesPurchased)
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

                if (property.IsMortgaged)
                {
                    Label lblIsMortgaged = new Label
                    {
                        Content = Properties.Resources.PropertyStatus_Label,
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
                grdContainer.PreviewMouseLeftButtonDown += (sender, e) => MouseClickViewDetailsOfMyProperty(sender, e, property);
                stackPanelContainer.Children.Add(grdContainer);
            }

            scrollViewer.Content = stackPanelContainer;
            wpMyPropietiers.Children.Add(scrollViewer);
        }


        private void MouseClickViewDetailsOfMyProperty(object sender, MouseButtonEventArgs e, Property property)
        {
            ShowProperty(property);
            grdPropertyModificationButtonGroup.Visibility = Visibility.Visible;
            DefinitivePropertyCost.Content = 0;
            wpMyPropietiers.IsEnabled = false;

            if (EnableHouseControls(property))
            {
                EnableHotelControl(property);
            }

            ShowMortgageControls(property);
            _selectedProperty = property;
        }

        private bool EnableHouseControls(Property property)
        {
            bool enableHouseControls = false;

            if (VerifyCompletePropertySet(property.Color))
            {
                cboNumerHouse.IsEnabled = true;
                enableHouseControls = property.NumberHouses == 4;
                cboNumerHouse.SelectionChanged += (senderAux, eAux) => UpdatePriceOnHouseSelection(senderAux, eAux, property);
            }

            return enableHouseControls;
        }

        private void EnableHotelControl(Property property)
        {
            if (property.NumberHouses == 4)
            {
                cboNumerHouse.IsEnabled = false;
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

        private void ButtonClickBuyProperty(object sender, RoutedEventArgs e)
        {
            var lastProperty = _listPropertiesPurchased.LastOrDefault();

            if (lastProperty != null && _currentPlayer.Money - lastProperty.BuyingCost >= 0)
            {
                lastProperty.Situation = Property.PropertySituation.Bought;
                lastProperty.Taxes = CalculateRentalCost(0, lastProperty.BuyingCost);

                try
                {
                    _managerClient.PurchaseProperty(lastProperty, _currentPlayer, _currentGame.IdGame);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }

                ShowMyProperties();

                ClosePropertyManagementPanel();

                if (VerifyCompletePropertySet(lastProperty.Color))
                {
                    DuplicatePropertyRent(lastProperty.Color);
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.NoFunds_Label, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private long CalculateRentalCost(int numberOfHouses, long costOfProperty)
        {
            long rentalCost = 0;

            switch (numberOfHouses)
            {
                case 0:
                    rentalCost = (long)Math.Round(0.15 * costOfProperty); break;
                case 1:
                    rentalCost = (long)Math.Round(0.45 * costOfProperty); break;
                case 2:
                    rentalCost = (long)Math.Round(1.35 * costOfProperty); break;
                case 3:
                    rentalCost = (long)Math.Round(2.25 * costOfProperty); break;
                case 4:
                    rentalCost = (long)Math.Round(3.15 * costOfProperty); break;
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

        private void ButtonClickShowPropertyPurchaseContract(object sender, RoutedEventArgs e)
        {
            grdMenu.Visibility = Visibility.Collapsed;
            grdAddHouse.Visibility = Visibility.Visible;
            grdAddHotel.Visibility = Visibility.Visible;
            grdPropertyPurchaseButtonGroup.Visibility = Visibility.Visible;
            DefinitivePropertyCost.Content = _listPropertiesPurchased[_listPropertiesPurchased.Count - 1].BuyingCost;
        }

        private void UpdatePriceOnHouseSelection(object sender, SelectionChangedEventArgs e, Property property)
        {
            if(cboNumerHouse.SelectedValue != null)
            {
                int numberOfHouses = (int)cboNumerHouse.SelectedValue;
                DefinitivePropertyCost.Content = numberOfHouses * GetHouseCostByColor(property.Color);
            } 
        }

        private bool VerifyCompletePropertySet(string color)
        {
            bool result = false;
            int numberProperties = 0;

            foreach (var property in _listPropertiesPurchased)
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

        private void DuplicatePropertyRent(string color)
        {
            foreach(var property in _listPropertiesPurchased)
            {
                if (property.Color.Equals(color) && property.NumberHouses == 0)
                {
                    property.Taxes *= 2;

                    try
                    {
                        _managerClient.ModifyProperty(property, _currentGame.IdGame);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        AlertMessage();
                    }
                    catch (EndpointNotFoundException exception)
                    {
                        _ilog.Error(exception.ToString());
                        AlertMessage();
                    }
                    catch (CommunicationException exception)
                    {
                        _ilog.Error(exception.ToString());
                        AlertMessage();
                    }
                }
            }
        }

        private void ButtonClickDeclineToPurchaseProperty(object sender, RoutedEventArgs e)
        {
            _listPropertiesPurchased.Remove(_listPropertiesPurchased[_listPropertiesPurchased.Count - 1]);
            ClosePropertyManagementPanel();
        }

        private void UpdatePropertyWithHotelSelection(object sender, SelectionChangedEventArgs e)
        {
            if (cboHotel.SelectedItem != null && cboHotel.SelectedItem.ToString() == "Si")
            {
                cboNumerHouse.SelectedItem = null;
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
            try
            {
                _managerClient.JailPlayer(_currentGame.IdGame, _currentPlayer.IdPlayer);
            }
            catch (TimeoutException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
            catch (EndpointNotFoundException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
            catch (CommunicationException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
        }

        private async Task ShowEventSquare()
        {
            Wildcard wildcard = GetEvent();
            txtEventDescription.Text = GetMessage(wildcard.Action);
            grdPropertyPurchase.Visibility =Visibility.Visible;
            grdEventSquare.Visibility = Visibility.Visible;

            await Task.Delay(4000);

            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdEventSquare.Visibility = Visibility.Collapsed;
            butCloseEvento.Visibility = Visibility.Collapsed;

            try
            {
                _managerClient.GetActionCard(_currentGame.IdGame, _currentPlayer.IdPlayer, wildcard);
            }
            catch (TimeoutException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
            catch (EndpointNotFoundException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
            catch (CommunicationException exception)
            {
                _ilog.Error(exception.ToString());
                AlertMessage();
            }
        }

        private Wildcard GetEvent()
        {
            Wildcard wildcard = new Wildcard();

            int seed = Environment.TickCount;
            Random random = new Random(seed);
            wildcard.Action = random.Next(2, 7);

            return wildcard;
        }

        private void ShowPropertySquare(Property property)
        {
            grdMenu.Visibility = Visibility.Visible;
            grdPropertySquare.Visibility = Visibility.Visible;
            lblNumberHouses.Content = property.NumberHouses;
            UpdatePropertyCard(property);
            _listPropertiesPurchased.Add(property);
        }

        private void ButtonClickCloseEventCard(object sender, RoutedEventArgs e)
        {
            grdPropertyPurchase.Visibility = Visibility.Collapsed;
            grdPrisonSquare.Visibility = Visibility.Collapsed;
            grdEventSquare.Visibility = Visibility.Collapsed;
            butCloseEvento.Visibility = Visibility.Collapsed;
        }

        public void UpdateTurns(Queue<Service.Player> turns)
        {
            _currentGame.Players = turns;
            UpdateTurnVisual();
            if (turns.Peek().IdPlayer == _currentPlayer.IdPlayer)
            {
                this.butRollingDice.IsEnabled = true;
            }
        }

        private readonly Dictionary<int, bool> expelledPlayers = new Dictionary<int, bool>();

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
                    Source = ImageManager.GetSourceImage(friend.Piece.ImagenSource),
                    Stretch = Stretch.Fill,
                    Margin = new Thickness(-145, 20, 0, 20),
                };

                grdContainer.Children.Add(imgProperty);

                if (_currentPlayer.IdPlayer != friend.IdPlayer && !friend.Loser && !expelledPlayers.ContainsKey(friend.IdPlayer))
                {
                    Button butShowName = new Button
                    {
                        Content = "Expulsar",
                        Height = 23,
                        Width = 50,
                        Margin = new Thickness(90, 70, 0, 0),
                    };

                    butShowName.Click += (sender, e) =>
                    {
                        butShowName.Visibility = Visibility.Collapsed;

                        try
                        {
                            _managerClient.ExpelPlayer(friend.IdPlayer, _currentGame.IdGame);
                        }
                        catch (TimeoutException exception) 
                        {
                            HandleException(exception);
                        }
                        catch (EndpointNotFoundException exception) 
                        { 
                            HandleException(exception);
                        }

                        expelledPlayers[friend.IdPlayer] = true;
                    };

                    grdContainer.Children.Add(butShowName);
                }

                stackPanelContainer.Children.Add(grdContainer);

                if (friend.IdPlayer == _currentPlayer.IdPlayer)
                {
                    UpgradePlayerMoney(friend.Money);
                }

            }

            scrollViewer.Content = stackPanelContainer;
            PlayersInGame.Children.Add(scrollViewer);
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
            _currentPlayer.Money = money;
        }

        private void ButtonClickCancelPurchase(object sender, RoutedEventArgs e)
        {
            grdAddHouse.Visibility = Visibility.Collapsed;
            grdAddHotel.Visibility = Visibility.Collapsed;
            grdPropertyPurchaseButtonGroup.Visibility = Visibility.Collapsed;
            DefinitivePropertyCost.Content = " ";
            DefinitivePropertyCost.Visibility = Visibility.Collapsed;
            grdMenu.Visibility = Visibility.Visible;
        }

        private void ButtonClickMortgageProperty(object sender, RoutedEventArgs e)
        {
            try
            {
                _managerClient.RealizePropertyMortgage(_currentGame.IdGame, _selectedProperty, _currentPlayer.IdPlayer);
            }
            catch (TimeoutException exception) 
            {
                HandleException(exception);
            }

            _selectedProperty.IsMortgaged = true;
            butPayMortgage.Visibility = Visibility.Visible;
            butMortgageProperty.Visibility = Visibility.Collapsed;
            ShowMessageBox(Properties.Resources.SuccessfulMortgageRegistrationAlert_Label);
            ShowMyProperties();
        }


        public void RemoveGamePiece(Service.Player player)
        {
            _boardPieces[player.Piece.PartNumber].Visibility = Visibility.Collapsed;
        }

        private void ButtonClickCloseGame(object sender, RoutedEventArgs e)
        {
            grdWinnerMural.Visibility = Visibility.Collapsed;
            BackToMenu();
        }

        public void EndGame(int idWinner)
        {
            DisableButtons();
            if (_currentPlayer.IdPlayer == idWinner)
            {
                grdWinnerMural.Visibility = Visibility.Visible;
            }
        }

        private void ButtonClickCancelAddHomesAndHotels(object sender, RoutedEventArgs e)
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
            DefinitivePropertyCost.Content = " ";
            wpMyPropietiers.IsEnabled = true;
            RestoreComboBoxes();
        }

        private void RestoreComboBoxes()
        {
            cboHotel.SelectedItem = null;
            cboNumerHouse.SelectedItem = null;
            cboHotel.IsEnabled = false;
            cboNumerHouse.IsEnabled = false;
        }

        private void ButtonClickAddHomesAndHotels(object sender, RoutedEventArgs e)
        {
            if (cboNumerHouse.SelectedItem != null)
            {
                TryToAddHouses();
            }
            else if (cboHotel.SelectedItem != null && cboHotel.SelectedItem.ToString() == "Si")
            {
                TryToAddHotel();
            }
            else
            {
                ShowMessageBox(Properties.Resources.ComBoxSelectionAlert_Label);
            }
        }

        private void TryToAddHouses()
        {
            int numberHouse = int.Parse(cboNumerHouse.SelectedItem.ToString());
            long costTotal = numberHouse * GetHouseCostByColor(_selectedProperty.Color);

            if (_currentPlayer.Money - costTotal >= 0)
            {
                if (numberHouse + _selectedProperty.NumberHouses < 4)
                {
                    UpdatePropertyForHouses(numberHouse);
                    ShowMessageBox(Properties.Resources.BuyingHouses_Label);
                    CloseAddHomesAndHotels();
                }
                else
                {
                    ShowMessageBox(Properties.Resources.AlertMaximumNumberHomesProperty_Label);
                }
            }
            else
            {
                ShowMessageBox(Properties.Resources.InsufficientBalanceAlert_Label);
            }
        }

        private void UpdatePropertyForHouses(int numberHouse)
        {
            _selectedProperty.Situation = Property.PropertySituation.House;
            _selectedProperty.NumberHouses += numberHouse;
            _selectedProperty.Taxes = CalculateRentalCost(_selectedProperty.NumberHouses, _selectedProperty.BuyingCost);
            try
            {
                _managerClient.ModifyProperty(_selectedProperty, _currentGame.IdGame);
                _managerClient.PayConstruction(_currentPlayer.IdPlayer, numberHouse * GetHouseCostByColor(_selectedProperty.Color), _currentGame.IdGame);
            }
            catch (TimeoutException exception) 
            {
                HandleException(exception);
            }
        }

        private void TryToAddHotel()
        {
            long costTotal = GetHotelCostByColor(_selectedProperty.Color);

            if (_currentPlayer.Money - costTotal >= 0)
            {
                UpdatePropertyForHotel(costTotal);
                ShowMessageBox(Properties.Resources.BuyingHotel_Label);
                CloseAddHomesAndHotels();
            }
            else
            {
                ShowMessageBox(Properties.Resources.InsufficientBalanceAlert_Label);
            }
        }

        private void UpdatePropertyForHotel(long costTotal)
        {
            _selectedProperty.Situation = Property.PropertySituation.Hotel;
            _selectedProperty.Taxes = (long)(_selectedProperty.BuyingCost * 4.00);
            _selectedProperty.NumberHouses = 0;

            try
            {
                _managerClient.ModifyProperty(_selectedProperty, _currentGame.IdGame);
                _managerClient.PayConstruction(_currentPlayer.IdPlayer, costTotal, _currentGame.IdGame);
            }
            catch (TimeoutException exception) 
            {
                HandleException(exception);
            }

        }

        private void ShowMessageBox(string message)
        {
            MessageBox.Show(message, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonClickPayMortgage(object sender, RoutedEventArgs e)
        {
            butMortgageProperty.Visibility = Visibility.Visible;
            butPayMortgage.Visibility = Visibility.Collapsed;
            _selectedProperty.IsMortgaged = false;

            try
            {
                _managerClient.PayPropertyMortgage(_currentGame, _currentPlayer.IdPlayer, _selectedProperty);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }

            ShowMyProperties();
            ShowMessageBox(Properties.Resources.SuccessfulPaymentAlert_Label);
            CloseAddHomesAndHotels();
        }

        public void UpdatePropertyStatus(Property property)
        {
            foreach (var propertyAux in _listPropertiesPurchased)
            {
                if (propertyAux.Name == property.Name)
                {
                    propertyAux.IsMortgaged = true;
                    break;
                }
            }

            ShowMyProperties();
        }

        public enum MessageCode
        {
            RentPayment,
            RentCollection,
            GoToJailEvent,
            PayPartnerEvent,
            EventPayTaxes,
            EventGetTaxes,
            EventAdvancePosition,
            EventBackwardPosition
        }

        private string GetMessage(int messageCode)
        {
            MessageCode requestResult = (MessageCode)messageCode;

            string message = "";

            switch (requestResult)
            {
                case MessageCode.RentPayment:
                    message = Properties.Resources.RentPayment_Label;
                    break;
                case MessageCode.RentCollection:
                    message = Properties.Resources.RentCollection_Label;
                    break;
                case MessageCode.GoToJailEvent:
                    message = Properties.Resources.GoToJailEvent_Label;
                    break;
                case MessageCode.PayPartnerEvent:
                    message = Properties.Resources.PayPartnerEvent_Label;
                    break;
                case MessageCode.EventPayTaxes:
                    message = Properties.Resources.EventPayTaxes_Label;
                    break;
                case MessageCode.EventGetTaxes:
                    message = Properties.Resources.EventGetTaxes_Label;
                    break;
                case MessageCode.EventAdvancePosition:
                    message = Properties.Resources.EventAdvancePosition_Label;
                    break;
                case MessageCode.EventBackwardPosition:
                    message = Properties.Resources.EventBackwardPosition_Label;
                    break;
            }

            return message;
        }

        private void DisableButtons()
        {
            butRollingDice.IsEnabled = false;
            butEndTurn.IsEnabled = false;
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

        public void MovePlayerPieceOnBoard(Service.Player player, Property property, Service.Game game)
        {
            throw new NotImplementedException();
        }
    }
}

