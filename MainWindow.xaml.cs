using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media;
using UIGameClientTourist.XAMLViews;

namespace UIGameClientTourist
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogIn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    string userName = txtUserName.Text;
                    string password = pwdPassword.Password;

                    using (var playerClient = new Service.PlayerClient())
                    {
                        var newPlayer = new Service.PlayerSet { Nickname = userName, Password = password };
                        int playerID = playerClient.PlayerSearch(newPlayer);
                        if (playerID != 0)
                        {
                            GoToMenuFromLogin(playerID);
                        }
                        else
                        {
                            InvalidCampsAlert();
                            MessageBox.Show("Credenciales incorrectas. Inténtelo de nuevo.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
                MessageBox.Show("Se produjo un error al iniciar sesión. Por favor, inténtelo de nuevo.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(pwdPassword.Password))
            {
                InvalidCampsAlert();
                MessageBox.Show("Campos vacíos. Por favor, complete todos los campos.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private void GoToMenuFromLogin(int playerID)
        {
            MainMenuGame mainMenuGameWindow = new MainMenuGame(playerID);
            this.Close();
            mainMenuGameWindow.Show();
        }

        private void GoToRegisterPlayerFromLogin(object sender, RoutedEventArgs e)
        {
            RegisterPlayers registerPlayersWindow = new RegisterPlayers();
            this.Close();
            registerPlayersWindow.Show();
        }

        private void InvalidCampsAlert()
        {
            txtUserName.BorderBrush = Brushes.Red;
            txtUserName.Foreground = Brushes.Red;
            pwdPassword.BorderBrush = Brushes.Red;
            pwdPassword.Foreground = Brushes.Red;
        }

        private void GoToJoinGameFromLogin(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            JoinGame mainMenuGameWindow = new JoinGame(random.Next(1, 1000000000), true);
            this.Close();
            mainMenuGameWindow.Show();
        }
    }
}