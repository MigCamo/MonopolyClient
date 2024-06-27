using log4net;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UIGameClientTourist.GameLogic;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        private readonly int _currentPlayerID;
        private readonly Service.PlayerClient _playerManager = new Service.PlayerClient();
        private readonly Service.FriendListClient _friendManager = new Service.FriendListClient();
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(UserProfile));
        private readonly int _maxDescriptionSize = 250;

        public UserProfile(int idPlayer)
        {
            InitializeComponent();

            this.lblMaxLength.Content = _maxDescriptionSize;
            this._currentPlayerID = idPlayer;

            Service.Player playerInfo = null; 

            try
            {
                playerInfo = _playerManager.GetPlayerData(idPlayer);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }
            catch (EndpointNotFoundException exception)
            {
                HandleException(exception);
            }

            if (playerInfo != null)
            {
                this.txtUserName.Text = playerInfo.Name;
                this.txtDescription.Text = playerInfo.Description;

                if (playerInfo.Games > 0)
                {
                    this.txtWinRate.Text = ((playerInfo.GamesWin / playerInfo.Games) * 100).ToString() + "%";
                }
                else
                {
                    this.txtWinRate.Text = "N/A";
                }

                GetFriendRanking();
            }
        }


        private void ButtonClickNavigateToMainMenuGame(object sender, RoutedEventArgs e)
        {
            MainMenuGame menuWindow = new MainMenuGame(_currentPlayerID);
            this.Close();
            menuWindow.Show();
        }

        private void GetFriendRanking()
        {
            var friends = _friendManager.GetFriends(this._currentPlayerID);
            friends.OrderBy(friend => friend.GamesWins);
            foreach (var friend in friends)
            {
                this.tblRankinFriends.Items.Add(friend);
            }
        }

        private void ButtonClickUpdateProfile(object sender, RoutedEventArgs e)
        {
            bool fieldsAreValid = true;

            if (!IsValidText(this.txtDescription.Text))
            {
                SetTextBoxErrorStyle(this.txtDescription);
                fieldsAreValid = false;
            }

            if (!IsValidText(this.txtUserName.Text))
            {
                SetTextBoxErrorStyle(this.txtUserName);
                fieldsAreValid = false;
            }

            if (fieldsAreValid)
            {
                try
                {
                    ClearTextBoxErrorStyle();

                    _playerManager.UpdatePlayerData(this._currentPlayerID, this.txtDescription.Text, this.txtUserName.Text);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (CommunicationObjectFaultedException exception)
                {
                    HandleException(exception);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }
            }
        }

        private bool IsValidText(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && !text.All(char.IsDigit) && !text.All(char.IsPunctuation) && text.Length > 5;
        }

        private void SetTextBoxErrorStyle(TextBox textBox)
        {
            textBox.BorderBrush = Brushes.Red;
            textBox.BorderThickness = new Thickness(2);
        }

        private void ClearTextBoxErrorStyle()
        {
            this.txtDescription.BorderBrush = SystemColors.ControlDarkBrush;
            this.txtUserName.BorderBrush = SystemColors.ControlDarkBrush;
        }


        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.butUpdateProfile.IsEnabled = true;
            int currentDescriptionSize = this._maxDescriptionSize - this.txtDescription.Text.Length;
            this.lblMaxLength.Content = currentDescriptionSize;
            if (currentDescriptionSize < 0)
            {
                this.butUpdateProfile.IsEnabled = false;
            }
        }

        private void AlertMessage()
        {
            MessageBox.Show(Properties.Resources.LostConnectionAlertLabel_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ButtonClickClosePasswordChangePanel(object sender, RoutedEventArgs e)
        {
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            grdChangePasswordPanel.Visibility = Visibility.Collapsed;
            butUpdatePassword.Visibility = Visibility.Visible;
            butUpdateProfile.Visibility = Visibility.Visible;
        }

        private void ButtonClickShowPasswordChangePanel(object sender, RoutedEventArgs e)
        {
            butUpdatePassword.Visibility = Visibility.Collapsed;
            butUpdateProfile.Visibility = Visibility.Collapsed;
            grdChangePasswordPanel.Visibility = Visibility.Visible;
        }

        private void ButtonClickUpdatePlayerPassword(object sender, RoutedEventArgs e)
        {
            string currentPassword = txtCurrentPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();

            if (ArePasswordsValid(currentPassword, newPassword))
            {
                try
                {
                    int result = _playerManager.ModifyPassword(_currentPlayerID, currentPassword, newPassword);
                    ShowResultMessageBox(result);
                }
                catch (TimeoutException exception)
                {
                    HandleException(exception);
                }
                catch (CommunicationObjectFaultedException exception)
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
                MessageBox.Show(Properties.Resources.InvalidPasswordAlert_Label, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ArePasswordsValid(string currentPassword, string newPassword)
        {
            return IsValidPassword(currentPassword) && IsValidPassword(newPassword);
        }

        private bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 9 && password.Length <= 15;
        }

        public enum PasswordChangeResult
        {
            IncorrectCurrentPassword = -2,
            ServerError = -1,
            Success = 0
        }

        private void ShowResultMessageBox(int result)
        {
            PasswordChangeResult resultEnum = (PasswordChangeResult)result;

            string message = "";

            switch (resultEnum)
            {
                case PasswordChangeResult.IncorrectCurrentPassword:
                    message = Properties.Resources.PasswordNotMatched_Label;
                    break;
                case PasswordChangeResult.ServerError:
                    message = Properties.Resources.AlertError_Label;
                    break;
                case PasswordChangeResult.Success:
                    message = Properties.Resources.PasswordChangeSuccessful_Label;
                    break;
            }

            MessageBox.Show(message, Properties.Resources.SuccessConfirmationAlert_Label, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleException(Exception exception)
        {
            _ilog.Error(exception.ToString());
            AlertMessage();
        }
    }
}
