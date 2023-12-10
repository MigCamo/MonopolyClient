using System;
using System.Collections.Generic;
using System.Linq;
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
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para RegisterPlayers.xaml
    /// </summary>
    public partial class RegisterPlayers : Window
    {
        public RegisterPlayers()
        {
            InitializeComponent();
        }

        private void RegisterPlayer(object sender, RoutedEventArgs e)
        {
            string message;
            try
            {
                if (!string.IsNullOrEmpty(txtUserName.Text) && !string.IsNullOrEmpty(txtPassword.Text) && !string.IsNullOrEmpty(txtEmail.Text))
                {
                    var newPlayer = new PlayerSet { Nickname = txtUserName.Text, Password = txtPassword.Text, eMail = txtEmail.Text };
                    Service.PlayerClient player = new Service.PlayerClient();
                    player.RegisterPlayer(newPlayer);
                    CleanFields();
                    message = "Se registró correctamente el jugador, ya puede iniciar sesión.";

                }
                else
                {
                    InvalidCampsAlert();
                    message = "Campos vacios, llene por favor complete todo los campos";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar al jugador: {ex.Message}");
                message = "Se produjo un error al registrar al jugador. Por favor, inténtelo de nuevo.";
            }
            MessageBox.Show(message, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GoToLoginFromRegisterPlayers(object sender, RoutedEventArgs e)
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

        private void InvalidCampsAlert()
        {
            txtUserName.BorderBrush = Brushes.Red;
            txtUserName.Foreground = Brushes.Red;
            txtPassword.BorderBrush = Brushes.Red;
            txtPassword.Foreground = Brushes.Red;
            txtConfirmPassword.BorderBrush = Brushes.Red;
            txtConfirmPassword.Foreground = Brushes.Red;
            txtEmail.BorderBrush = Brushes.Red;
            txtEmail.Foreground = Brushes.Red;
            txtConfirmCode.BorderBrush = Brushes.Red;
            txtConfirmCode.Foreground = Brushes.Red;
        }
    }
}
