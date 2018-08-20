using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.Helpers;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleGame.ViewModels
{
    public class BoardViewModel:ViewModelBase
    {
        private readonly string _host;
        private readonly string _port;
        public ObservablePairCollection<int,ImageSource> Symbols { get; set; }
       
        private  SimpleTcpClient _client;
        private readonly List<int> _assignedFields;

        public RelayCommand<object> MoveCommand { get; }
        public RelayCommand NewGame { get; }
        public RelayCommand LoadCommand { get; }

        private Player _enemyPlayer;
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

        private bool _isPlayerTurn;
        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set
            {
                if (Equals(_isPlayerTurn, value))
                    return;
                _isPlayerTurn = value;
                RaisePropertyChanged();
            }
        }

        public BoardViewModel(string host,string port)
        {
            _host = host;
            _port = port;
            _assignedFields=new List<int>();
            IsPlayerTurn = false;

            Symbols=new ObservablePairCollection<int, ImageSource>();
            for(int i=0;i<9;i++)
                Symbols.Add(i,new BitmapImage());

            MoveCommand = new RelayCommand<object>(PlayerTurn);
            NewGame=new RelayCommand(StartNewGame);
            LoadCommand =new RelayCommand(Loaded);

           
        }

        public BoardViewModel()
        {
            
        }

        private void Loaded()
        {
            _client = new SimpleTcpClient { StringEncoder = Encoding.UTF8 };
            _client.DataReceived += Client_DataReceived;
            _client.Connect(_host, Convert.ToInt32(_port));
            _client.Write("Hello");
        }

        private void Client_DataReceived(object sender, Message e)
        {            
            var msg = e.MessageString.Split(';');
            if (msg.Length == 1)
            {
                if (msg[0] == "NewGame")
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(ReallyStartNewGame));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(msg[0] == "Player1"
                        ? AssignPlayer1
                        : new Action(AssignPlayer2));
                }              
            }               
            else 
            {
                if (CurrentPlayer != null && msg[0] == CurrentPlayer.PlayerType.ToString())
                {
                   Application.Current.Dispatcher.BeginInvoke(new Action(() => EnemyTurn(Int32.Parse(msg[1]))));
                }
                    
            }
        }

        private void AssignPlayer2()
        {
            CurrentPlayer = new SecondPlayer(Players.Player2);
            _enemyPlayer = new FirstPlayer(Players.Player1);
        }

        private void AssignPlayer1()
        {
            CurrentPlayer = new FirstPlayer(Players.Player1);
            _enemyPlayer = new SecondPlayer(Players.Player2);
            IsPlayerTurn = true;
        }

        private void EnemyTurn(int index)
        {
            
            if (!Player.IsGameOver && _assignedFields.Contains(index) || Player.IsGameOver) return;          
            _assignedFields.Add(index);
            Symbols[index].Value = _enemyPlayer.SymbolSource;
            _enemyPlayer.MakeTurn(index);

            if (_assignedFields.Count == 9)
            {
                MessageBox.Show("It's a draw!!!");
                Player.IsGameOver = true;
             
            }
            IsPlayerTurn = true;
        }

        private void StartNewGame()
        {        
            _client.Write("NewGame");
        }

        private void ReallyStartNewGame()
        {
            _assignedFields.Clear();

            for (int i = 0; i < 9; i++)
                Symbols[i].Value = new BitmapImage();

            Player.IsGameOver = false;
            CurrentPlayer.EmptyFields();
            _enemyPlayer.EmptyFields();
            IsPlayerTurn = CurrentPlayer.PlayerType == Players.Player1;
        }

        private void PlayerTurn(object p)
        {          
            int index = Convert.ToInt32(p);
            if(_assignedFields.Contains(index)||Player.IsGameOver) return;

            _assignedFields.Add(index);
            Symbols[index].Value = CurrentPlayer.SymbolSource;
            CurrentPlayer.MakeTurn(index);

            IsPlayerTurn = false;
            if (!Player.IsGameOver&&_assignedFields.Count == 9)
            {
                MessageBox.Show("It's a draw!!!");
                Player.IsGameOver = true;
            }          
            _client.Write(_enemyPlayer.PlayerType+";"+index);
        }
    }
}
