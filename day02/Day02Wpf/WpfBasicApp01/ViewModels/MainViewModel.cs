using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace WpfBasicApp01.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        // 메세지박스, 다이얼로그를 위한 변수
        private readonly IDialogCoordinator _dialogCoordinator;
        private string _greeting;
        public string Greeting {
            get => _greeting;
            set
            {
                _greeting = value;
                NotifyOfPropertyChange(() => Greeting); // 값이 바뀐걸 알려줘야함
            }
        }

        public MainViewModel(IDialogCoordinator dialogCoordinator) 
        {
            _dialogCoordinator = dialogCoordinator;
            Greeting = "Hello, Caliburn.Micro!!";
        }
        public async void SayHello()
        {
            Greeting = "Hello, Everyone~!";
            //WinForms 방식
            //MessageBox.Show("Hi, Everyone~!", "Greeting", MessageBoxButton.OK, MessageBoxImage.Information);
            await _dialogCoordinator.ShowMessageAsync(this, "Greeting", "Hello, Everyone~!");
        }
    }
}
