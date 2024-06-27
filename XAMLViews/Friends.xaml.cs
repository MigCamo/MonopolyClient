using log4net;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para Friends.xaml
    /// </summary>
    public partial class Friends : Window, IFriendsCallback
    {
        private readonly int _currentPlayerID;
        private readonly FriendListClient _friendListService;
        private readonly FriendsClient _sesionService;
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(Friends));

        public Friends(int idPlayer)
        {
            InstanceContext context = new InstanceContext(this);
            InitializeComponent();

            try
            {
                _sesionService = new FriendsClient(context);
                _friendListService = new FriendListClient();
                _sesionService.UpdatePlayerSession(idPlayer);
                ShowFriends(idPlayer);
                AddRequest(idPlayer);
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
            
            this._currentPlayerID = idPlayer;


        }

        private void ShowFriends(int idPlayer)
        {
            FriendListDisplay.Children.Clear();

            try
            {
                foreach (FriendList friend in _friendListService.GetFriends(idPlayer))
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
                    FriendListDisplay.Children.Add(brdBackground);
                }
            }
            catch (TimeoutException exception)
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

        public enum FriendRequestResult
        {
            Success = 0,
            AlreadyFriends = 1,
            UserNotFound = 2,
            ExistingRequest = 3
        }

        private void SendFriendRequest(object sender, RoutedEventArgs e)
        {
            try
            {
                FriendRequestResult requestResult = (FriendRequestResult)_sesionService.MakeFriendRequest(_currentPlayerID, TxtFriend.Text);

                switch (requestResult)
                {
                    case FriendRequestResult.Success:
                        MessageBox.Show(Properties.Resources.AlertConfirmationSendFriendRequest_Label);
                        break;
                    case FriendRequestResult.AlreadyFriends:
                        MessageBox.Show(Properties.Resources.AlreadyExistingAmigoAlert_Label);
                        break;
                    case FriendRequestResult.UserNotFound:
                        MessageBox.Show(Properties.Resources.AlertExistingUser_Label);
                        break;
                    case FriendRequestResult.ExistingRequest:
                        MessageBox.Show(Properties.Resources.AlertExistingRequest_Label);
                        break;
                }
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


        private void AddRequest(int IdPlayer)
        {
            RequestList.Children.Clear();

            try
            {
                foreach (var request in _friendListService.GetFriendRequests(IdPlayer))
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
                        Content = request.SenderName,
                        FontSize = 28,
                        Margin = new Thickness(10, 6, 0, 0),
                    };

                    Button butAccept = new Button
                    {
                        Content = Properties.Resources.AcceptPurchase_Button,
                        Width = 80,
                        Height = 20,
                        Margin = new Thickness(100, 10, 0, 0),
                    };

                    Button butReject = new Button
                    {
                        Content = Properties.Resources.Reject_Button,
                        Width = 80,
                        Height = 20,
                        Margin = new Thickness(300, 10, 0, 0),
                    };

                    butAccept.Click += (sender, e) =>
                    {
                        try
                        {
                            _sesionService.AcceptFriendRequest(request.IDRequest);
                            AddRequest(IdPlayer);
                            ShowFriends(IdPlayer);
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
                    };

                    butReject.Click += (sender, e) =>
                    {
                        try
                        {
                            _sesionService.RejectFriendRequest(request.IDRequest);
                            AddRequest(IdPlayer);
                            ShowFriends(IdPlayer);
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

                    };

                    grid.Children.Add(lblUserName);
                    grid.Children.Add(butAccept);
                    grid.Children.Add(butReject);
                    brdBackground.Child = grid;
                    RequestList.Children.Add(brdBackground);
                }
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

        private void GoMainMenuGameWindow(object sender, RoutedEventArgs e)
        {
            MainMenuGame menuWindow = new MainMenuGame(_currentPlayerID);
            this.Close();
            menuWindow.Show();
        }

        public void UpdateFriendRequest()
        {
            AddRequest(_currentPlayerID);
        }

        public void UpdateFriendDisplay()
        {
            ShowFriends(_currentPlayerID);
        }

        private void AlertMessage()
        {
            MessageBox.Show(Properties.Resources.LostConnectionAlertLabel_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
