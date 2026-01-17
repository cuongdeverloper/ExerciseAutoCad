using System.Windows;
using Exercise.ViewModels;

namespace Exercise.Views
{
    public partial class CreateRouteView : Window
    {
        public CreateRouteView(CreateRouteViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;

            vm.CloseAction = (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
        }
    }
}