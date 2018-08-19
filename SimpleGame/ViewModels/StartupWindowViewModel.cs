using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SimpleGame.ViewModel;

namespace SimpleGame.ViewModels
{
    public class StartupWindowViewModel:ViewModelBase
    {
        public RelayCommand SingleGameCommand { get; private set; }
        public RelayCommand MultiGameCommand { get; private set; }

        public StartupWindowViewModel()
        {
                SingleGameCommand=new RelayCommand(StartSinglePlayerGame);
                MultiGameCommand=new RelayCommand(StartMultiPlayerGame);
        }

        private void StartMultiPlayerGame()
        {
            ViewModelLocator vm=new ViewModelLocator();
            vm.Main.CurrentViewModel=new BoardViewModel();
            vm.Main.RaisePropertyChanged("CurrentViewModel");
        }

        private void StartSinglePlayerGame()
        {
            ViewModelLocator vm = new ViewModelLocator();
            vm.Main.CurrentViewModel = new BoardViewModel();
            vm.Main.RaisePropertyChanged("CurrentViewModel");
        }
    }
}
