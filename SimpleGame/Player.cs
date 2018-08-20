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
        public static bool IsGameOver;
        static Player()
        {
            IsGameOver = false;         
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

        public void EmptyFields()
        {
            TakenFields.Clear();
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
            TakenFields.Add(number);
        }            
    }
}
