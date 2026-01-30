using Exercise.ViewModels;
using System.Windows;

namespace Exercise.Views
{
    public partial class SelectProjectView : Window
    {
        public SelectProjectView(SelectProjectViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            vm.CloseAction = (result) => this.Close();
        }
    }
}