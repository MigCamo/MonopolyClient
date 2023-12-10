using System;
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
            string message = "";
            try
            {
                string password = new System.Net.NetworkCredential(string.Empty, pwdPassword.Password).Password;

                if (!string.IsNullOrEmpty(txtUserName.Text) && !string.IsNullOrEmpty(password))
                {
                    var newPlayer = new Service.PlayerSet { Nickname = txtUserName.Text, Password = pwdPassword.Password };
                    Service.PlayerClient playerClient = new Service.PlayerClient();
                    int playerID = playerClient.PlayerSearch(newPlayer);
                    if (playerID != 0)
                    {
                        GoToMenuFromLogin(playerID);
                    }
                    else
                    {
                        InvalidCampsAlert();
                        message = "Credenciales incorrectas. Inténtelo de nuevo.";
                    }
                }
                else
                {
                    InvalidCampsAlert();
                    message = "Campos vacios, llene por favor complete todo los campos";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
                message = "Se produjo un error al iniciar sesión. Por favor, inténtelo de nuevo.";
            }
            MessageBox.Show(message, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            
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
            JoinGame mainMenuGameWindow = new JoinGame(0);
            this.Close();
            mainMenuGameWindow.Show();
        }
    }
}