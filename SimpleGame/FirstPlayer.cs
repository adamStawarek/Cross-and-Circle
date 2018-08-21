using System;
using System.Windows.Media.Imaging;

namespace SimpleGame
{
    public class FirstPlayer : Player
    {
        public FirstPlayer(Players playerType) : base(playerType)
        {
            SymbolSource=new BitmapImage(new Uri(@"\Images\Circle.png",UriKind.Relative));
        }
    }
}