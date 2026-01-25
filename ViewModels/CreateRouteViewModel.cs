using Exercise.Models;
using Exercise.MVVM;
using Exercise.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Exercise.ViewModels
{
    // Kế thừa BindableBase cho gọn (nếu project đã có), hoặc giữ INotifyPropertyChanged như bạn muốn
    public class CreateRouteViewModel : INotifyPropertyChanged
    {
        // 1. LIÊN KẾT VỚI KHO DỮ LIỆU CHUNG (Singleton)
        // Dùng dấu => để luôn trỏ về kho chung. Không được gán bằng dấu =
        public ObservableCollection<RouteItemModel> Routes => ProjectDataManager.Instance.GlobalRoutes;

        // --- Biến lưu dòng đang chọn ---
        private RouteItemModel _selectedRoute;
        public RouteItemModel SelectedRoute
        {
            get => _selectedRoute;
            set { _selectedRoute = value; OnPropertyChanged(); }
        }

        // Danh sách Batch và ComboBox
        public ObservableCollection<BatchInfoModel> Batches { get; set; }
        public List<string> MaterialGroups { get; set; }
        public List<string> Materials { get; set; }

        // Form nhập liệu Batch mới
        private BatchInfoModel _newBatchInput;
        public BatchInfoModel NewBatchInput
        {
            get => _newBatchInput;
            set { _newBatchInput = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand ImportExcelCommand { get; set; }
        public ICommand AddRouteCommand { get; set; }
        public ICommand DeleteRouteCommand { get; set; }
        public ICommand SaveBatchCommand { get; set; }
        public ICommand DeleteBatchCommand { get; set; }
        public ICommand DrawCommand { get; set; }
        public Action<bool> CloseAction { get; set; }

        private readonly ExcelImportService _excelService;

        public CreateRouteViewModel()
        {
            _excelService = new ExcelImportService();

            Batches = new ObservableCollection<BatchInfoModel>();
            InitDummyData();
            NewBatchInput = new BatchInfoModel();

            ImportExcelCommand = new RelayCommand(ImportData);

            AddRouteCommand = new RelayCommand(obj =>
                Routes.Add(new RouteItemModel { RouteName = "New Route", Size = "200x100", Elevation = 2800 }));

            DeleteRouteCommand = new RelayCommand(obj =>
            {
                var itemsToDelete = Routes.Where(x => x.IsSelected).ToList();
                foreach (var item in itemsToDelete) Routes.Remove(item);
            });

            SaveBatchCommand = new RelayCommand(ExecuteSaveBatch);
            DeleteBatchCommand = new RelayCommand(obj => { if (obj is BatchInfoModel item) Batches.Remove(item); });

            DrawCommand = new RelayCommand(obj =>
            {
                if (SelectedRoute == null)
                {
                    MessageBox.Show("Vui lòng chọn một dòng lộ trong bảng để vẽ!", "Chưa chọn dữ liệu");
                    return;
                }
                CloseAction?.Invoke(true);
            });
        }

        private void InitDummyData()
        {
            MaterialGroups = new List<string> { "Quạt gió", "Ống nước", "Thang máng cáp" };
            Materials = new List<string> { "Quạt cấp", "Quạt hút", "Ống PVC", "Máng tôn" };
            Batches.Add(new BatchInfoModel { BatchCode = "BAT001", InstallCondition = "1 giàn giáo", InstallSpace = "Lắp âm sàn" });
        }

        private void ExecuteSaveBatch(object obj)
        {
            if (string.IsNullOrEmpty(NewBatchInput.BatchCode)) return;
            Batches.Add(new BatchInfoModel
            {
                BatchCode = NewBatchInput.BatchCode,
                InstallCondition = NewBatchInput.InstallCondition,
                InstallSpace = NewBatchInput.InstallSpace,
                WorkPackage = NewBatchInput.WorkPackage,
                Phase = NewBatchInput.Phase
            });
            NewBatchInput = new BatchInfoModel();
            OnPropertyChanged(nameof(NewBatchInput));
        }

        private void ImportData(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var data = _excelService.ImportRoutes(dlg.FileName);


                    var newItems = new List<RouteItemModel>();

                    foreach (var item in data)
                    {
                        newItems.Add(new RouteItemModel
                        {
                            RouteName = item.RouteName,
                            Size = $"{item.Width}x{item.Height}",
                            Elevation = item.BottomElevation
                        });
                    }

                    ProjectDataManager.Instance.UpdateRoutes(newItems);

                    OnPropertyChanged(nameof(Routes));
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}