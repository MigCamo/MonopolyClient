using System;
using System.Windows.Media.Imaging;

namespace UIGameClientTourist.GameLogic
{
    public class ImageManager
    {
        protected ImageManager() { }

        public static BitmapImage GetSourceImage(string path)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            return bitmapImage;
        }
    }
}
