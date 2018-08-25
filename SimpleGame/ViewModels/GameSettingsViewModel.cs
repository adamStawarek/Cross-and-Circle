using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.ViewModel;
using SimpleTCP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace SimpleGame.ViewModels
{
    internal class GameSettingsViewModel:ViewModelBase
    {
        private static GameSettingsViewModel _instance;
        public static GameSettingsViewModel GetInstance()
        {
            return _instance ?? (_instance = new GameSettingsViewModel());
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if(Equals(_port,value))
                    return;
                _port = value;
                RaisePropertyChanged();
            }
        }

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                if (Equals(_host, value))
                    return;
                _host = value;
                RaisePropertyChanged();
            }
        }

        private string _connectionMessage;
        public string ConnectionMessage
        {
            get => _connectionMessage;
            set
            {
                if (Equals(_connectionMessage, value))
                    return;
                _connectionMessage = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand StartGameCommand { get; }
        public RelayCommand RunServerCommand { get; }
        private SimpleTcpServer _server;

        public GameSettingsViewModel()
        {
            Port = "8919";
            Host = "127.0.0.1";
            ConnectionMessage = "";
            StartGameCommand=new RelayCommand(StartGame);
            RunServerCommand=new RelayCommand(RunServer);
            Application.Current.Exit += CloseServer;
        }

        private void CloseServer(object sender, ExitEventArgs e)
        {
            if (_server != null && _server.IsStarted)
            {
                _server.Broadcast("Server_disconnected");
                 _server.Stop();
            }
               
        }

        private void RunServer()
        {        
            _server = new SimpleTcpServer
            {
                Delimiter = 0x13,
                StringEncoder = Encoding.UTF8
            };       
            try
            {
                _server.DataReceived += Server_DataReceived;
                _server.ClientDisconnected += BroadcastThatOneClientLeftTheGame;
                IPAddress ip = IPAddress.Parse(Host);
                _server.Start(ip, Convert.ToInt32(Port));
                ConnectionMessage = "Server running...";
            }
            catch (Exception)
            {

                ConnectionMessage = "Cannot start server";
            }                                             
        }

        private void BroadcastThatOneClientLeftTheGame(object sender, TcpClient e)
        {
            _server.Broadcast("Client_disconnected");
        }

        private void Server_DataReceived(object sender, Message e)
        {        
            switch (e.MessageString)
            {
                case "Hello":
                    e.Reply(_server.ConnectedClientsCount == 1 ? "Player1" : "Player2");
                    if (_server.ConnectedClientsCount == 2)
                        _server.Broadcast("Ready");
                    break;
                case "NewGame":
                    _server.Broadcast(e.MessageString);
                    break;
                default:
                    _server.Broadcast(e.MessageString);
                    break;
               
            }
        }

        private void StartGame()
        {
           ViewModelLocator vm=new ViewModelLocator();
           vm.Main.CurrentViewModel=new BoardViewModel(Host,Port);
        }
    }
}
