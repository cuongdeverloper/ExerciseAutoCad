using Exercise.ViewModels;
using System.Windows;

namespace Exercise.Views
{
    public partial class CreateAttributeView : Window
    {
        public CreateAttributeView(CreateAttributeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;

            vm.HideWindowAction = () => this.Visibility = Visibility.Hidden;
            vm.ShowWindowAction = () => this.Visibility = Visibility.Visible;

            vm.CloseAction = (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
        }
    }
}