using System.Windows;
namespace Exercise.Views
{
    public partial class Window1 : Window
    {
        public Window1() { InitializeComponent(); }
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}