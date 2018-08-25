using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.Helpers;
using SimpleTCP;
using System;
using System.Diagnostics;
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

        private bool _isAnimatedWaitingImage;
        public bool IsAnimatedWaitingImage
        {
            get => _isAnimatedWaitingImage;
            set
            {
                if (Equals(_isAnimatedWaitingImage, value))
                    return;
                _isAnimatedWaitingImage = value;
                RaisePropertyChanged();
            }
        }

        private string _waitingBoxMessage;
        public string WaitingBoxMessage
        {
            get => _waitingBoxMessage;
            set
            {
                if (Equals(_waitingBoxMessage, value))
                    return;
                _waitingBoxMessage = value;
                RaisePropertyChanged();
            }
        }

        private ImageSource _waitingBoxImage;
        public ImageSource WaitingBoxImage
        {
            get => _waitingBoxImage;
            set
            {
                if (Equals(_waitingBoxImage, value))
                {
                    return;
                }
                _waitingBoxImage = value;
                RaisePropertyChanged();
            }
        }     
    
        public BoardViewModel(string host,string port)
        {
            _host = host;
            _port = port;

            Symbols =new ObservablePairCollection<int, ImageSource>();
            for(var i=0;i<9;i++)
                Symbols.Add(i,new BitmapImage());

            MoveCommand = new RelayCommand<object>(PlayerTurn);
            NewGame=new RelayCommand(InformServerToStartNewGame);
            LoadCommand =new RelayCommand(Loaded);   
          
            Application.Current.Exit += DisconnectClient;
        }

        private void DisconnectClient(object sender, ExitEventArgs e)
        {
            try
            {
                _client.Write("Client_disconnected");
                _client.Disconnect();
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }         
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
                //WarningPopupMessage = "Cannot connect to given host and port!!!";
                //IsPopupOpen = true;
                ViewModelLocator vm = new ViewModelLocator();
                vm.Main.CurrentViewModel = new ModalBoxViewModel("Cannot connect to given host and port!!!");
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
                case 1 when msg[0] == "Server_disconnected":
                    Application.Current.Dispatcher.BeginInvoke(new Action(ServerDisconnected));
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

        private void ServerDisconnected()
        {
            ViewModelLocator vm = new ViewModelLocator();
            vm.Main.CurrentViewModel = new ModalBoxViewModel("Server not running");
        }

        private void ClientDisconnected()
        {
            //WarningPopupMessage = "The second player has left the game";
            //IsPopupOpen = true;
            ViewModelLocator vm = new ViewModelLocator();
            vm.Main.CurrentViewModel = new ModalBoxViewModel("Client disconnected");
        }

        private void ReadyToPlay()
        {
            IsAnimatedWaitingImage = true;
            if (CurrentPlayer.PlayerType == Players.Player1)
            {
                IsPlayerTurn = true;
                WaitingBoxMessage = "Your turn";
                WaitingBoxImage = CurrentPlayer.SymbolSource;
            }
            else
            {
                IsPlayerTurn = false;
                WaitingBoxMessage = "Second player thinking...";
                WaitingBoxImage = _enemyPlayer.SymbolSource;
            }                    
        }

        private void AssignPlayer(Players player)
        {        
            if (player == Players.Player1)
            {
                CurrentPlayer = new FirstPlayer(Players.Player1);
                _enemyPlayer = new SecondPlayer(Players.Player2);
                WaitingBoxMessage = "Waiting for second player to join...";              
            }
            else
            {
                CurrentPlayer = new SecondPlayer(Players.Player2);
                _enemyPlayer = new FirstPlayer(Players.Player1);
                IsAnimatedWaitingImage = true;
            }

            _enemyPlayer.Win += EnemyWin;
            _currentPlayer.Win += PlayerWin;
            Player.Draw += MatchEndedInDraw;            
        }

        private void MatchEndedInDraw(object sender, EventArgs e)
        {          
            IsPlayerTurn = false;
            WaitingBoxMessage = "There is no winner";
            WaitingBoxImage = Player.DrawSymbolSource;
            IsAnimatedWaitingImage = false;
            MessageBox.Show("It's a draw");
        }

        private void PlayerWin(object sender, EventArgs e)
        {
            WaitingBoxMessage = CurrentPlayer.PlayerType+" is the winner";
            WaitingBoxImage = CurrentPlayer.WinSymbolSource;           
            IsPlayerTurn = false;
            IsAnimatedWaitingImage = false;
            MessageBox.Show("You win");
        }

        private void EnemyWin(object sender, EventArgs e)
        {
            WaitingBoxMessage = _enemyPlayer.PlayerType + " is the winner";
            WaitingBoxImage = _enemyPlayer.WinSymbolSource;          
            IsPlayerTurn = false;
            IsAnimatedWaitingImage = false;
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
           ReadyToPlay();
        }

        private void EnemyTurn(int index)
        {         
            if (Player.IsGameOver || Player.FieldsTakenByBothPlayers.Contains(index)) return;
            Symbols[index].Value = _enemyPlayer.SymbolSource;
            _enemyPlayer.MakeTurn(index);
           SwitchWaitingBoxPlayer();
            
        }

        private void PlayerTurn(object p)
        {          
            var index = Convert.ToInt32(p);
            if(Player.FieldsTakenByBothPlayers.Contains(index)||Player.IsGameOver) return;
            Symbols[index].Value = CurrentPlayer.SymbolSource;
            CurrentPlayer.MakeTurn(index);                            
            _client.Write(_enemyPlayer.PlayerType+";"+index);
            SwitchWaitingBoxPlayer();
        }

        private void SwitchWaitingBoxPlayer()
        {
            if (Player.IsGameOver) return;
            if (IsPlayerTurn)
            {
                IsPlayerTurn = false;
                WaitingBoxMessage = "Second player thinking...";
                WaitingBoxImage = _enemyPlayer.SymbolSource;
            }
            else
            {
                IsPlayerTurn = true;
                WaitingBoxMessage = "Your turn";
                WaitingBoxImage = CurrentPlayer.SymbolSource;
            }
        }
    }
}
