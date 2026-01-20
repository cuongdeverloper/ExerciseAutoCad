using Exercise.Models;
using Exercise.MVVM;
using Exercise.Services; // Cần dòng này để dùng ExcelImportService
using Microsoft.Win32; // Cần dòng này để dùng OpenFileDialog
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Exercise.ViewModels
{
    public class CreateRouteViewModel : INotifyPropertyChanged
    {
        // ==========================================================
        // PHẦN 1: CODE CŨ (ĐỌC JSON CABLE DATA)
        // ==========================================================
        public ObservableCollection<CableItem> CableList { get; set; }

        private CableItem _selectedCable;
        public CableItem SelectedCable
        {
            get => _selectedCable;
            set
            {
                _selectedCable = value;
                OnPropertyChanged();

                // Logic tự động điền
                if (_selectedCable != null)
                {
                    InputGroup = _selectedCable.ItemGroupName;
                    InputSize = _selectedCable.SizeName;
                    InputSymbol = _selectedCable.Symbol;
                }
            }
        }

        private string _inputGroup;
        public string InputGroup
        {
            get => _inputGroup;
            set { _inputGroup = value; OnPropertyChanged(); }
        }

        private string _inputSize;
        public string InputSize
        {
            get => _inputSize;
            set { _inputSize = value; OnPropertyChanged(); }
        }

        private string _inputSymbol;
        public string InputSymbol
        {
            get => _inputSymbol;
            set { _inputSymbol = value; OnPropertyChanged(); }
        }

        private double _elevation = 2800;
        public double Elevation
        {
            get => _elevation;
            set { _elevation = value; OnPropertyChanged(); }
        }

        // ==========================================================
        // PHẦN 2: CODE MỚI THÊM (IMPORT EXCEL)
        // ==========================================================

        // 1. Khai báo Service
        private readonly ExcelImportService _excelService;

        // 2. Danh sách chứa dữ liệu từ Excel để hiện lên bảng
        public ObservableCollection<RouteImportModel> ImportedRoutes { get; set; }

        // 3. Command cho nút Import
        public ICommand ImportExcelCommand { get; set; }


        // ==========================================================
        // PHẦN 3: CONSTRUCTOR & COMMANDS
        // ==========================================================
        public Action<bool> CloseAction { get; set; }
        public ICommand DrawCommand { get; set; }

        public CreateRouteViewModel()
        {
            // Khởi tạo lệnh cũ
            DrawCommand = new RelayCommand(ExecuteDraw);

            // --- KHỞI TẠO PHẦN MỚI ---
            _excelService = new ExcelImportService();
            ImportedRoutes = new ObservableCollection<RouteImportModel>();

            // Tạo Command import (gọi hàm ImportData)
            ImportExcelCommand = new RelayCommand(ImportData);

            // Load dữ liệu cũ
            LoadCableData();
        }

        // --- HÀM XỬ LÝ IMPORT EXCEL MỚI ---
        private void ImportData(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Chọn file dữ liệu Lộ"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Gọi Service đọc file
                    var data = _excelService.ImportRoutes(openFileDialog.FileName);

                    // Xóa dữ liệu cũ trên lưới (nếu có) và thêm dữ liệu mới
                    ImportedRoutes.Clear();
                    foreach (var item in data)
                    {
                        ImportedRoutes.Add(item);
                    }

                    MessageBox.Show($"Đã đọc thành công {data.Count} dòng dữ liệu!", "Thông báo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đọc file Excel: " + ex.Message, "Lỗi");
                }
            }
        }

        // --- HÀM XỬ LÝ CŨ ---
        private void LoadCableData()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyFolder, "CableData.json");

            if (File.Exists(path))
            {
                try
                {
                    string jsonContent = File.ReadAllText(path);
                    var rootObject = JsonConvert.DeserializeObject<CableDataWrapper>(jsonContent);

                    if (rootObject != null)
                        CableList = rootObject.items;
                    else
                        CableList = new ObservableCollection<CableItem>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc JSON: " + ex.Message);
                    CableList = new ObservableCollection<CableItem>();
                }
            }
            else
            {
                // MessageBox.Show("Không tìm thấy file JSON!"); // Có thể comment lại để đỡ phiền nếu chưa có file
                CableList = new ObservableCollection<CableItem>();
            }
        }

        private void ExecuteDraw(object obj)
        {
            CloseAction?.Invoke(true);
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}