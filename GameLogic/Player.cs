using System.Collections.Generic;

namespace UIGameClientTourist.GameLogic
{
    public class Player
    {
        public string Name { get; set; }
        public int Money { get; set; }
        public int Position { get; set; }
        public List<Property> Properties { get; set; }

        public Player(string name, int money, int position)
        {
            Name = name;
            this.Money = money;
            this.Position = position;
        }

        public Player()
        {

        }
    }
}
