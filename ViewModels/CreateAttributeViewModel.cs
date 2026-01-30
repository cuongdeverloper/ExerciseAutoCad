using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Exercise.Models;
using Exercise.MVVM;
using Exercise.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Exercise.ViewModels
{
    public class CreateAttributeViewModel : BindableBase
    {
        // Lấy danh sách lộ từ Singleton
        public ObservableCollection<RouteItemModel> AvailableRoutes => ProjectDataManager.Instance.GlobalRoutes;

        private RouteItemModel _selectedRoute;
        public RouteItemModel SelectedRoute
        {
            get => _selectedRoute;
            set { _selectedRoute = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AttributeItemModel> Attributes { get; set; }

        private List<ObjectId> _targetIds;

        public Action<bool> CloseAction { get; set; }
        public ICommand AddAttributeCommand { get; set; }
        public ICommand AssignCommand { get; set; }

        public CreateAttributeViewModel(List<ObjectId> selectedIds)
        {
            _targetIds = selectedIds;
            Attributes = new ObservableCollection<AttributeItemModel>();

            Attributes.Add(new AttributeItemModel { Symbol = "", Quantity = 1 });

            AddAttributeCommand = new RelayCommand(obj =>
            {
                Attributes.Add(new AttributeItemModel { Symbol = "", Quantity = 1 });
            });

            AssignCommand = new RelayCommand(ExecuteAssign);
        }

        private void ExecuteAssign(object obj)
        {
            if (SelectedRoute == null)
            {
                MessageBox.Show("Vui lòng chọn Tên lộ!", "Cảnh báo");
                return;
            }

            if (_targetIds == null || _targetIds.Count == 0)
            {
                MessageBox.Show("Không có đối tượng nào được nhận!", "Lỗi");
                return;
            }

            var doc = Application.DocumentManager.MdiActiveDocument;

            try
            {
                using (DocumentLock docLock = doc.LockDocument())
                {
                    XDataHelper.AddListAttributeXData(_targetIds, SelectedRoute.RouteName, Attributes.ToList());
                }

                MessageBox.Show($"Đã gán thuộc tính cho {_targetIds.Count} đối tượng thành công!", "Thông báo");
                CloseAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nghiêm trọng: " + ex.Message);
            }
        }
    }
}