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