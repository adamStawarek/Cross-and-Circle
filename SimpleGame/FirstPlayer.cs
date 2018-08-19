using System;
using System.Windows.Media.Imaging;

namespace SimpleGame
{
    public class FirstPlayer : Player
    {
        public FirstPlayer(Players playerType) : base(playerType)
        {
            SymbolSource=new BitmapImage(new Uri(@"C:\Users\Administrator\source\repos\SimpleGame\SimpleGame\Images\Circle.png"));
        }
    }
}