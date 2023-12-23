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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UIGameClientTourist.GameLogic;
using UIGameClientTourist.Service;

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para JoinGame.xaml
    /// </summary>
    public partial class JoinGame : Window
    {
        private readonly int IDPlayer;
        private readonly Service.PlayAsGuestManagerClient managerClient;
        private readonly bool IsInvited = false;
        public JoinGame(int IdPlayer)
        {
            this.IDPlayer = IdPlayer;
            InitializeComponent();
            this.managerClient = new Service.PlayAsGuestManagerClient();
        }

        public JoinGame(int IdPlayer, bool isInvited)
        {
            this.IDPlayer = IdPlayer;
            this.IsInvited = isInvited;
            InitializeComponent();
            this.managerClient = new Service.PlayAsGuestManagerClient();
        }

        private void SearchGame(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(CodeGame.Text, out int codeGame))
                {
                    if (ValidateGame(codeGame))
                    {
                        OpenLobbyWindow(codeGame);
                    }
                }
                else
                {
                    MessageBox.Show("Por favor, ingrese un código de partida válido.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en SearchGame: " + ex.Message);
            }
        }

        private bool ValidateGame(int codeGame)
        {
            bool result = true;

            if (managerClient.SearchGameByCode(codeGame) == 1)
            {
                MessageBox.Show("Partida no encontrada. Verifique que el código sea correcto o que la partida exista.");
                result = false;
            } 
            else if (managerClient.IsGameOngoing(codeGame) == 1)
            {
                MessageBox.Show("Lo siento, la partida ya ha comenzado.");
                result = false;
            } 
            else if (managerClient.IsGameFull(codeGame) == 1)
            {
                MessageBox.Show("Lo siento, la partida se encuentra completa en este momento.");
                result = false;
            }
            
            return result;
        }

        private void OpenLobbyWindow(int codeGame)
        {
            Lobby lobby;

            if (IsInvited)
            {
                lobby = new Lobby(codeGame, IDPlayer, true);
            }
            else
            {
                lobby = new Lobby(codeGame, IDPlayer); 
            }
           
            this.Close();
            lobby.Show();
        }
    }
}
