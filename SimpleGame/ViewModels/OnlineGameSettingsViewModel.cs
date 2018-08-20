using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.ViewModel;

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
        public OnlineGameSettingsViewModel()
        {
            Port = "8919";
            Host = "127.0.0.1";
            StartGameCommand=new RelayCommand(StartGame);
        }

        private void StartGame()
        {
           ViewModelLocator vm=new ViewModelLocator();
            vm.Main.CurrentViewModel=new BoardViewModel();
        }
    }
}
