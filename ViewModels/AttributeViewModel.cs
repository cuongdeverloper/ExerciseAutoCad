using Exercise.Actions;
using Exercise.Models;
using Exercise.MVVM;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Exercise.ViewModels
{
    public class AttributeViewModel
    {
        // Danh sách hiển thị lên màn hình
        public ObservableCollection<AttributeItem> AttributeList { get; set; }

        // Lưu ID của Block đang chọn
        private ObjectId _currentBlockId;

        public ICommand SaveCommand { get; set; }

        public AttributeViewModel()
        {
            AttributeList = new ObservableCollection<AttributeItem>();
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        // Hàm được gọi từ AppCommands khi chọn đối tượng
        public void LoadData(ObjectId id)
        {
            _currentBlockId = id;
            AttributeList.Clear();

            // Gọi Action để lấy dữ liệu
            var items = AttributeActions.GetAttributes(id);
            foreach (var item in items)
            {
                AttributeList.Add(item);
            }
        }

        // Hàm xử lý khi nhấn nút Lưu
        private void ExecuteSave(object obj)
        {
            if (_currentBlockId.IsNull) return;
            // Gọi Action để ghi dữ liệu
            AttributeActions.UpdateAttributes(_currentBlockId, AttributeList);
        }
    }
}