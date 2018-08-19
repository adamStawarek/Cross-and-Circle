using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.Helpers;

namespace SimpleGame.ViewModels
{
    public class BoardViewModel:ViewModelBase
    {
        public ObservablePairCollection<int,ImageSource> Symbols { get; set; }
        private Player _player1, _player2;
        private readonly List<int> _assignedFields;
        public RelayCommand<object> MoveCommand { get; private set; }
        public RelayCommand NewGame { get; private set; }

        private Player _currentPlayer;
        public Player CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                if (Equals(_currentPlayer, value))
                {
                    return;
                }

                _currentPlayer = value;
                RaisePropertyChanged();
            }
        }

        public BoardViewModel()
        {
            _assignedFields=new List<int>();

            Symbols=new ObservablePairCollection<int, ImageSource>();
            for(int i=0;i<9;i++)
                Symbols.Add(i,new BitmapImage());

            _player1=new FirstPlayer(Players.Player1);
            _player2=new SecondPlayer(Players.Player2);

            MoveCommand = new RelayCommand<object>(PlayerTurn);
            NewGame=new RelayCommand(StartNewGame);

            CurrentPlayer= (Player.CurrentPlayer == Players.Player1) ? _player1 : _player2;
        }

        private void StartNewGame()
        {
            _assignedFields.Clear();

            for (int i = 0; i < 9; i++)
                Symbols[i].Value=new BitmapImage();

            Player.IsGameOver = false;
            Player.RandomizeCurrentPlayer();

            _player1=new FirstPlayer(Players.Player1);
            _player2=new SecondPlayer(Players.Player2);
            CurrentPlayer = (Player.CurrentPlayer == Players.Player1) ? _player1 : _player2;
        }

        private void PlayerTurn(object p)
        {          
            int index = Convert.ToInt32(p);
            if(_assignedFields.Contains(index)||Player.IsGameOver) return;

            _assignedFields.Add(index);
            Symbols[index].Value = CurrentPlayer.SymbolSource;
            CurrentPlayer.MakeTurn(index);

            if (_assignedFields.Count == 9)
            {
                MessageBox.Show("It's a draw!!!");
                Player.IsGameOver = true;
            }

            CurrentPlayer = (Player.CurrentPlayer == Players.Player1) ? _player1 : _player2;
        }
    }
}
