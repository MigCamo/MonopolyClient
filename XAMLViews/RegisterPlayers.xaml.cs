using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para RegisterPlayers.xaml
    /// </summary>
    public partial class RegisterPlayers : Window
    {
        private readonly PlayerClient _currentPlayer = new PlayerClient();
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(RegisterPlayers));
        private int _verifyCode;

        public RegisterPlayers()
        {
            InitializeComponent();
        }

        private const int RegistrationSuccess = 1;
        private const int PlayerAlreadyExists = 0;

        private void ButtonClickRegisterPlayer(object sender, RoutedEventArgs e)
        {
            string message;
            try
            {
                List<string> errorMessages = new List<string>();

                if (AreFieldsValid(errorMessages))
                {
                    if (txtConfirmCode.Text.Equals(this._verifyCode.ToString()))
                    {
                        var newPlayer = new PlayerSet
                        {
                            Nickname = txtUserName.Text.Trim(),
                            Password = txtPassword.Text.Trim(),
                            eMail = txtEmail.Text.Trim(),
                            Games = 0,
                            Wins = 0,
                            Description = "N/A"
                        };

                        int result = _currentPlayer.RegisterPlayer(newPlayer);

                        if (result == RegistrationSuccess)
                        {
                            message = Properties.Resources.SuccessfulUserRegistrationAlert_Label;
                            CleanFields();
                        }
                        else if (result == PlayerAlreadyExists)
                        {
                            message = Properties.Resources.ExistingUserAlert_Label;
                        }
                        else
                        {
                            message = Properties.Resources.UserRegistrationErrorAlert_Label;
                        }
                    }
                    else
                    {
                        message = Properties.Resources.IncorrectVerificationCodeAlert_Label;
                    }
                }
                else
                {
                    message = Properties.Resources.InvalidCampsAlert_Label + "\n" + string.Join("\n", errorMessages);
                }

                MessageBox.Show(message, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (TimeoutException exception)
            {
                HandleException(exception);
            }  
        }

        private bool AreFieldsValid(List<string> errorMessages)
        {
            bool isValid = true;

            if (!IsUserNameValid())
            {
                errorMessages.Add(Properties.Resources.UserRegistrationErrorAlert_Label);
                txtUserName.BorderBrush = Brushes.Red;
                txtUserName.Foreground = Brushes.Red;
                isValid = false;
            }

            if (!IsPasswordValid())
            {
                errorMessages.Add(Properties.Resources.InvalidPasswordAlert_Label);
                txtPassword.BorderBrush = Brushes.Red;
                txtPassword.Foreground = Brushes.Red;
                isValid = false;
            }

            if (!IsConfirmPasswordValid())
            {
                errorMessages.Add(Properties.Resources.InvalidPasswordConfirmationAlert_Label);
                txtConfirmPassword.BorderBrush = Brushes.Red;
                txtConfirmPassword.Foreground = Brushes.Red;
                isValid = false;
            }

            if (!IsEmailValid(txtEmail.Text.Trim()))
            {
                errorMessages.Add(Properties.Resources.InvalidEmailAlert_Label);
                txtEmail.BorderBrush = Brushes.Red;
                txtEmail.Foreground = Brushes.Red;
                isValid = false;
            }

            if (!IsConfirmCodeValid())
            {
                errorMessages.Add(Properties.Resources.InvalidVerificationCodeAlert_Label);
                txtConfirmCode.BorderBrush = Brushes.Red;
                txtConfirmCode.Foreground = Brushes.Red;
                isValid = false;
            }

            return isValid;
        }

        private bool IsConfirmPasswordValid()
        {
            string confirmPassword = txtConfirmPassword.Text.Trim();
            return !string.IsNullOrWhiteSpace(confirmPassword) && confirmPassword.Length >= 9 && confirmPassword.Length <= 15 && confirmPassword == txtPassword.Text.Trim();
        }

        private bool IsEmailValid(string email)
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(email) && email.Length <= 25)
            {
                string emailPattern = @"^\S+@\S+\.\S+$";
                Regex regex = new Regex(emailPattern);
                result = regex.IsMatch(email);
            }
            return result;
        }

        private bool IsUserNameValid()
        {
            string userName = txtUserName.Text.Trim();
            return !string.IsNullOrWhiteSpace(userName) && userName.Length <= 15 && !userName.All(char.IsDigit) && !userName.Any(c => char.IsSymbol(c) || char.IsPunctuation(c));
        }

        private bool IsPasswordValid()
        {
            string password = txtPassword.Text.Trim();
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 9 && password.Length <= 15;
        }

        private bool IsConfirmCodeValid()
        {
            string confirmCode = txtConfirmCode.Text.Trim();
            return !string.IsNullOrWhiteSpace(confirmCode) && confirmCode.Length <= 15 && confirmCode.All(char.IsDigit);
        }


        private void ButtonClickGoToLoginFromRegisterPlayers(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            this.Close();
            loginWindow.Show();
        }

        private void CleanFields()
        {
            txtUserName.Text = "";
            txtPassword.Text = "";
            txtEmail.Text = "";
            txtConfirmPassword.Text = "";
            txtConfirmCode.Text = "";
        }

        private int GenerateVerufyCode()
        {
            Random random = new Random();
            int GameCode = random.Next(100000, 999999);
            return GameCode;
        }

        private void ButtonClickSendConfirmationEmail(object sender, RoutedEventArgs e)
        {
            this._verifyCode = GenerateVerufyCode();

            if (IsEmailValid(txtEmail.Text.Trim()))
            {
                try
                {
                    _currentPlayer.SendEmail(this._verifyCode.ToString(), txtEmail.Text);
                }
                catch (EndpointNotFoundException exception)
                {
                    HandleException(exception);
                }
            }
            else
            {
                txtEmail.BorderBrush = Brushes.Red;
                txtEmail.Foreground = Brushes.Red;
                MessageBox.Show(Properties.Resources.InvalidEmailAlert_Label, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
