using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using SimpleGame.Helpers;

namespace SimpleGame
{
    public abstract class Player
    {
        public static Players CurrentPlayer;
        public static bool IsGameOver;
        static Player()
        {
            var rnd=new Random();
            IsGameOver = false;
            CurrentPlayer = (Players)rnd.Next(2);
        }

        public static void RandomizeCurrentPlayer()
        {
            var rnd = new Random();
            CurrentPlayer = (Players)rnd.Next(2);
        }

        public Players PlayerType { get; set; }
        public ImageSource SymbolSource { get; set; }
        private ObservableCollection<int> TakenFields { get; }

        private readonly List<int[]> _winSequenceMatrix = new List<int[]>
        {
            new[]{0,1,2},
            new[]{3,4,5},
            new[]{6,7,8},
            new[]{0,3,6},
            new[]{1,4,7},
            new[]{2,5,8},
            new[]{0,4,8},
            new[]{6,4,2}
        };

        protected Player(Players playerType)
        {
            PlayerType = playerType;
            TakenFields = new ObservableCollection<int>();
            TakenFields.CollectionChanged += CheckGameStatus;
        }

        private void CheckGameStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var sequence in _winSequenceMatrix)
            {
                if (TakenFields.ContainsArray(sequence))
                {
                    MessageBox.Show(PlayerType+" won!!!");
                    IsGameOver = true;
                    return;
                }
            }             
        }

        public void MakeTurn(int number)
        {
            if (CurrentPlayer != PlayerType) return;
            TakenFields.Add(number);
            CurrentPlayer = (PlayerType == Players.Player1) ? Players.Player2 : Players.Player1;
        }            
    }
}
