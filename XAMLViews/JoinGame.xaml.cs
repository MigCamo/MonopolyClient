using System;
using System.Windows;

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
                    MessageBox.Show(Properties.Resources.AlertInvalidCode_Label);
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

            if (managerClient.SearchGameByCode(codeGame) != 0)
            {
                MessageBox.Show(Properties.Resources.ItemNotFoundAlert_Label);
                result = false;
            }
            else if (managerClient.IsGameOngoing(codeGame) != 0)
            {
                MessageBox.Show(Properties.Resources.AlreadyStartedAlert_Label);
                result = false;
            }
            else if (managerClient.IsGameFull(codeGame) != 0)
            {
                MessageBox.Show(Properties.Resources.FullLineItemAlert_Label);
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
