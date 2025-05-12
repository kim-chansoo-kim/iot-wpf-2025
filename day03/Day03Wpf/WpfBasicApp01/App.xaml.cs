using System.Windows;
using WpfBasicApp01.Views;
using WpfBasicApp01.ViewModels;

namespace WpfBasicApp01
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 뷰모델 생성
            var viewModel = new MainViewModel();

            // 뷰(윈도우) 생성
            var view = new MainView
            {
                // DataContext에 뷰모델 설정
                DataContext = viewModel
            };

            // 뷰(윈도우) 열기
            view.ShowDialog();
        }
    }

}
