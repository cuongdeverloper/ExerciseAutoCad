using System.Windows;
using Exercise.ViewModels;

namespace Exercise.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var vm = new LoginViewModel();

            vm.CloseAction = new System.Action(this.Close);

            this.DataContext = vm;
        }
    }
}