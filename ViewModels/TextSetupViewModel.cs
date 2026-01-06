using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Exercise.Models;
using Exercise.Commands;
using Autodesk.AutoCAD.ApplicationServices;

namespace Exercise.ViewModels
{
    public class TextSetupViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Fonts { get; set; } = new ObservableCollection<string> { "Standard (Simplex)", "Arial", "romans.shx" };

        private string _selectedFont = "Standard (Simplex)";
        private string _textHeight = "2,5";
        private bool _alwaysShowSize = true;
        private bool _showCustomField = false;

        public RelayCommand SaveCommand { get; set; }

        public TextSetupViewModel()
        {
            SaveCommand = new RelayCommand(p => ExecuteSaveAndDraw());
        }

        // Properties liên kết với View
        public string SelectedFont { get => _selectedFont; set { _selectedFont = value; OnPropertyChanged(); } }
        public string TextHeight { get => _textHeight; set { _textHeight = value; OnPropertyChanged(); } }
        public bool AlwaysShowSize { get => _alwaysShowSize; set { _alwaysShowSize = value; OnPropertyChanged(); } }
        public bool ShowCustomField { get => _showCustomField; set { _showCustomField = value; OnPropertyChanged(); } }

        private void ExecuteSaveAndDraw()
        {
            // Cập nhật Bridge để lệnh vẽ lấy được thông số mới nhất
            Exercise.Models.DrawDataBridge.SelectedFont = this.SelectedFont;
            Exercise.Models.DrawDataBridge.AlwaysShowSize = this.AlwaysShowSize;
            Exercise.Models.DrawDataBridge.ShowCustomField = this.ShowCustomField;

            if (double.TryParse(TextHeight.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double h))
            {
                Exercise.Models.DrawDataBridge.TextHeight = h;
            }

            // Gửi lệnh an toàn vào CAD
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            doc?.SendStringToExecute("DRAW_CONTINUOUS_LINE_INTERNAL ", true, false, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}