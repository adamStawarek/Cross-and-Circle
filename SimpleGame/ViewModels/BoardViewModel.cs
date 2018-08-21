using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.Helpers;
using SimpleTCP;
using System;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SimpleGame.ViewModel;

namespace SimpleGame.ViewModels
{
    public class BoardViewModel:ViewModelBase
    {
        private  SimpleTcpClient _client;
        private readonly string _host;
        private readonly string _port;

        public ObservablePairCollection<int,ImageSource> Symbols { get; set; }              

        public RelayCommand<object> MoveCommand { get; }
        public RelayCommand NewGame { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand BackToSettings { get; set; }

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

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (Equals(_isPopupOpen, value))
                    return;
                _isPopupOpen = value;                
                RaisePropertyChanged();
            }
        }
        
        private bool _isWaitingPopupOpen;
        public bool IsWaitingPopupOpen
        {
            get => _isWaitingPopupOpen;
            set
            {
                if (Equals(_isWaitingPopupOpen, value))
                    return;
                _isWaitingPopupOpen = value;
                RaisePropertyChanged();
            }
        }


        private string _warningPopupMessage;
        public string WarningPopupMessage
        {
            get => _warningPopupMessage;
            set
            {
                if (Equals(_warningPopupMessage, value))
                    return;
                _warningPopupMessage = value;
                RaisePropertyChanged();
            }
        }

        public BoardViewModel(string host,string port)
        {
            _host = host;
            _port = port;
            IsPlayerTurn = false;
            IsPopupOpen = false;
            IsWaitingPopupOpen = false;

            Symbols=new ObservablePairCollection<int, ImageSource>();
            for(var i=0;i<9;i++)
                Symbols.Add(i,new BitmapImage());

            MoveCommand = new RelayCommand<object>(PlayerTurn);
            NewGame=new RelayCommand(InformServerToStartNewGame);
            LoadCommand =new RelayCommand(Loaded);   
            BackToSettings=new RelayCommand(OpenSettingsWindow);
            Application.Current.Exit += DisconnectClient;
        }

        private void DisconnectClient(object sender, ExitEventArgs e)
        {
            _client.Disconnect();
        }

        public BoardViewModel()
        {
            
        }

        private void Loaded()
        {          
            try
            {
                _client = new SimpleTcpClient { StringEncoder = Encoding.UTF8 };
                _client.DataReceived += Client_DataReceived;
                _client.Connect(_host, Convert.ToInt32(_port));
                _client.Write("Hello");
            }
            catch (Exception)
            {
                WarningPopupMessage = "Cannot connect to given host and port!!!";
                IsPopupOpen = true;
            }
        }

        private void Client_DataReceived(object sender, Message e)
        {
            var msg = e.MessageString.Split(';');
            switch (msg.Length)
            {
                case 1 when msg[0] == "NewGame":
                    Application.Current.Dispatcher.BeginInvoke(new Action(StartNewGame));
                    break;
                case 1 when msg[0] == "Ready":
                    Application.Current.Dispatcher.BeginInvoke(new Action(ReadyToPlay));
                    break;
                case 1 when msg[0] == "Client_disconnected":
                    Application.Current.Dispatcher.BeginInvoke(new Action(ClientDisconnected));
                    break;
                case 1:
                    Application.Current.Dispatcher.BeginInvoke(msg[0] == "Player1"
                        ? new Action(() => AssignPlayer(Players.Player1))
                        : () => AssignPlayer(Players.Player2));
                    break;
                default:
                    if (CurrentPlayer != null && msg[0] == CurrentPlayer.PlayerType.ToString())
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => EnemyTurn(Int32.Parse(msg[1]))));
                    }

                    break;
            }
        }

        private void ClientDisconnected()
        {
            WarningPopupMessage = "The second player has left the game";
            IsPopupOpen = true;
        }

        private void ReadyToPlay()
        {            
            if (CurrentPlayer.PlayerType == Players.Player1)
            {
                IsPlayerTurn = true;
                IsWaitingPopupOpen = false;
            }                            
        }

        private void OpenSettingsWindow()
        {
            var vm=new ViewModelLocator();
            vm.Main.CurrentViewModel = OnlineGameSettingsViewModel.GetInstance();
        }

        private void AssignPlayer(Players player)
        {
            if(player == Players.Player1)
            {
                CurrentPlayer = new FirstPlayer(Players.Player1);
                _enemyPlayer = new SecondPlayer(Players.Player2);
                IsWaitingPopupOpen = true;
            }
            else
            {
                CurrentPlayer = new SecondPlayer(Players.Player2);
                _enemyPlayer = new FirstPlayer(Players.Player1);
            }

            _enemyPlayer.Win += EnemyWin;
            _currentPlayer.Win += PlayerWin;
            Player.Draw += MatchEndedInDraw;            
        }

        private void MatchEndedInDraw(object sender, EventArgs e)
        {
            MessageBox.Show("It's a draw");
        }

        private static void PlayerWin(object sender, EventArgs e)
        {
            MessageBox.Show("You win");
        }

        private static void EnemyWin(object sender, EventArgs e)
        {
            MessageBox.Show("You lose");
        }

        private void InformServerToStartNewGame()
        {        
            _client.Write("NewGame");
        }

        private void StartNewGame()
        {           
            for (var i = 0; i < 9; i++)
                Symbols[i].Value = new BitmapImage();
            Player.IsGameOver = false;
            Player.FieldsTakenByBothPlayers.Clear();
            CurrentPlayer.EmptyFields();
            _enemyPlayer.EmptyFields();
            IsPlayerTurn = CurrentPlayer.PlayerType == Players.Player1;
        }

        private void EnemyTurn(int index)
        {
            if (Player.IsGameOver || Player.FieldsTakenByBothPlayers.Contains(index)) return;
            Symbols[index].Value = _enemyPlayer.SymbolSource;
            _enemyPlayer.MakeTurn(index);
            IsPlayerTurn = true;
        }

        private void PlayerTurn(object p)
        {          
            var index = Convert.ToInt32(p);
            if(Player.FieldsTakenByBothPlayers.Contains(index)||Player.IsGameOver) return;
            Symbols[index].Value = CurrentPlayer.SymbolSource;
            CurrentPlayer.MakeTurn(index);
            IsPlayerTurn = false;                 
            _client.Write(_enemyPlayer.PlayerType+";"+index);
        }
    }
}
