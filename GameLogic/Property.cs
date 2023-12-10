namespace UIGameClientTourist.GameLogic
{
    public class Property
    {
        private Property() { }

        public enum TypeProperty { Jail, Service, Street }
        public enum PropertySituation { Free, Bought, House, Hotel }

        public Property(string name, long buyingCost, long taxes, int posX, int posY, string imageSource, string color)
        {
            Name = name;
            Type = TypeProperty.Street;
            BuyingCost = buyingCost;
            Taxes = taxes;
            Situation = PropertySituation.Free;
            Owner = null;
            PosicitionX = posX;
            PosicitionY = posY;
            ImageSource = imageSource;
            Color = color;
        }

        public string Name { get; set; }
        public TypeProperty Type { get; set; }
        public long BuyingCost { get; set; }
        public long Taxes { get; set; }
        public PropertySituation Situation { get; set; }
        public Player Owner { get; set; }
        public int PosicitionX { get; set; }
        public int PosicitionY { get; set; }
        public string ImageSource { get; set; }
        public string Color { get; set; }
    }
}
