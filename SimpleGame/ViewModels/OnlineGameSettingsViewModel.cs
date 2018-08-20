using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.ViewModel;
using SimpleTCP;

namespace SimpleGame.ViewModels
{
    class OnlineGameSettingsViewModel:ViewModelBase
    {
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

        public RelayCommand StartGameCommand { get; private set; }
        public RelayCommand RunServerCommand { get; private set; }
        private SimpleTcpServer _server;
        private List<string> players;

        public OnlineGameSettingsViewModel()
        {
            Port = "8919";
            Host = "127.0.0.1";
            StartGameCommand=new RelayCommand(StartGame);
            RunServerCommand=new RelayCommand(RunServer);
            players=new List<string>
            {
                "Player1","Player2"
            };
        }

        private void RunServer()
        {
            _server = new SimpleTcpServer
            {
                Delimiter = 0x13,
                StringEncoder = Encoding.UTF8
            };
            _server.DataReceived += Server_DataReceived;
            IPAddress ip = IPAddress.Parse(Host);
            _server.Start(ip, Convert.ToInt32(Port));
        }

        private void Server_DataReceived(object sender, Message e)
        {
           
            var msg = e.MessageString.Split(';');
            switch (msg[0])
            {
                case "Hello":
                    e.Reply(_server.ConnectedClientsCount == 1 ? "Player1" : "Player2");
                    break;
                default:
                    _server.Broadcast(msg[0] + ";" + msg[1]);
                    //e.Reply();
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
