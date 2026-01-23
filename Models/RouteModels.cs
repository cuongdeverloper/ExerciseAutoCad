using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Exercise.Models
{
    // Class hỗ trợ Binding
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // --- DỮ LIỆU HIỂN THỊ TRÊN LƯỚI (TAB 1) ---
    public class RouteItemModel : BindableBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string RouteName { get; set; }

        // Dropdown data
        private string _selectedBatch;
        public string SelectedBatch
        {
            get => _selectedBatch;
            set { _selectedBatch = value; OnPropertyChanged(); }
        }

        public string SelectedGroup { get; set; }
        public string SelectedMaterial { get; set; }

        public string Size { get; set; }     // Ví dụ: "300x100"
        public double Elevation { get; set; } // Cao độ (QUAN TRỌNG ĐỂ VẼ)
        public string Symbol { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // --- DỮ LIỆU BATCH (TAB 2) ---
    public class BatchInfoModel : BindableBase
    {
        public string BatchCode { get; set; }
        public string InstallCondition { get; set; }
        public string InstallSpace { get; set; }
        public string WorkPackage { get; set; }
        public string Phase { get; set; }
    }

    // --- DỮ LIỆU ĐỂ ĐỌC FILE EXCEL (DTO) ---
    // (Giữ lại cái này để ExcelImportService không bị lỗi)
    public class RouteImportModel
    {
        public string RouteName { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double BottomElevation { get; set; }
    }
}