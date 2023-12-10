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

namespace UIGameClientTourist.XAMLViews
{
    /// <summary>
    /// Lógica de interacción para JoinGame.xaml
    /// </summary>
    public partial class JoinGame : Window
    {
        private int IDPlayer;
        public JoinGame(int IdPlayer)
        {
            IDPlayer = IdPlayer;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var codeGame = int.Parse(CodeGame.Text);
                Console.WriteLine(codeGame);
                Lobby lobby = new Lobby(codeGame, IDPlayer);
                this.Close();
                lobby.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en RegisterPlayer: " + ex.Message);
            }

        }
    }
}
