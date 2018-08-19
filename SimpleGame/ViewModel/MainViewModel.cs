using GalaSoft.MvvmLight;
using SimpleGame.ViewModels;

namespace SimpleGame.ViewModel
{
  
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel == value)
                {
                    return;
                }

                _currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel()
        {
            CurrentViewModel=new StartupWindowViewModel();
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}