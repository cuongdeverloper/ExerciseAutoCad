using Exercise.ViewModels;
using System.Windows;

namespace Exercise.Views
{
    public partial class CreateRouteView : Window
    {
        public CreateRouteView(CreateRouteViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;

            if (vm.CloseAction == null)
            {
                vm.CloseAction = (result) =>
                {
                    this.DialogResult = result; 
                    this.Close();
                };
            }
        }
    }
}