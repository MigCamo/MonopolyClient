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
    public partial class Friends : Window, Service.IFriendsCallback
    {
        private readonly int idPlayer;
        private readonly Service.FriendListClient friendListService;

        Service.FriendsClient SesionService;
        public Friends(int idPlayer)
        {
            InstanceContext context = new InstanceContext(this);
            SesionService = new Service.FriendsClient(context);
            friendListService = new Service.FriendListClient();
            SesionService.UpdatePlayerSession(idPlayer);
            this.idPlayer = idPlayer;
            InitializeComponent();
            ShowFriends(idPlayer);
            AddRequest(idPlayer);
        }

        private void ShowFriends(int idPlayer)
        {
            FriendListDisplay.Children.Clear();
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
                FriendListDisplay.Children.Add(brdBackground);
            }

        }

        private void SendFriendRequest(object sender, RoutedEventArgs e)
        {
            switch (SesionService.MakeFriendRequest(idPlayer, TxtFriend.Text))
            {
                case 0:
                    MessageBox.Show(Properties.Resources.AlertConfirmationSendFriendRequest_Label);
                    break;
                case 1:
                    MessageBox.Show(Properties.Resources.AlreadyExistingAmigoAlert_Label);
                    break;
                case 2:
                    MessageBox.Show(Properties.Resources.AlertExistingUser_Label);
                    break;
                case 3:
                    MessageBox.Show(Properties.Resources.AlertExistingRequest_Label);
                    break;
            }
        }

        private void AddRequest(int IdPlayer)
        {
            RequestList.Children.Clear();
            foreach (var request in friendListService.GetFriendRequests(IdPlayer))
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
                    SesionService.AcceptFriendRequest(request.IDRequest);
                    AddRequest(IdPlayer);
                    ShowFriends(IdPlayer);
                };

                butReject.Click += (sender, e) =>
                {
                    SesionService.RejectFriendRequest(request.IDRequest);
                    AddRequest(IdPlayer);
                    ShowFriends(IdPlayer);
                };

                grid.Children.Add(lblUserName);
                grid.Children.Add(butAccept);
                grid.Children.Add(butReject);
                brdBackground.Child = grid;
                RequestList.Children.Add(brdBackground);
            }
        }

        private void GoMainMenuGameWindow(object sender, RoutedEventArgs e)
        {
            MainMenuGame menuWindow = new MainMenuGame(idPlayer);
            this.Close();
            menuWindow.Show();
        }

        public void UpdateFriendRequest()
        {
            AddRequest(idPlayer);
        }

        public void UpdateFriendDisplay()
        {
            ShowFriends(idPlayer);
        }
    }
}
