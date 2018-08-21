﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using SimpleGame.Helpers;
using SimpleGame.ViewModels;

namespace SimpleGame
{
    public abstract class Player
    {
        public static EventHandler Draw;
        public static bool IsGameOver;
        public static ObservableCollection<int> FieldsTakenByBothPlayers;
        static Player()
        {
            IsGameOver = false;        

            FieldsTakenByBothPlayers=new ObservableCollection<int>();
            FieldsTakenByBothPlayers.CollectionChanged += CheckForDraw;
        }

        private static void CheckForDraw(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(!IsGameOver & FieldsTakenByBothPlayers.Count == 9)) return;           
            IsGameOver = true;
            OnDraw();
        }

        private static void OnDraw()
        {
            var handler = Draw;
            handler?.Invoke(typeof(BoardViewModel), EventArgs.Empty);
        }


        public Players PlayerType { get; set; }
        public ImageSource SymbolSource { get; set; }
        private ObservableCollection<int> TakenFields { get; }
        public event EventHandler Win;

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
                if (!TakenFields.ContainsArray(sequence)) continue;
                OnWin();                
                IsGameOver = true;
                return;
            }             
        }

        public void EmptyFields()
        {
            TakenFields.Clear();
        }

        public void MakeTurn(int number)
        {
            TakenFields.Add(number);
            FieldsTakenByBothPlayers.Add(number);
        }

        public void OnWin()
        {
            var handler = Win;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
