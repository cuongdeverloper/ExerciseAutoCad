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
        public ICommand DeleteCommand { get; set; } 
        public ICommand CloneCommand { get; set; }  
        public AttributeViewModel()
        {
            AttributeList = new ObservableCollection<AttributeItem>();
            SaveCommand = new RelayCommand(ExecuteSave);
            DeleteCommand = new RelayCommand(ExecuteDelete); 
            CloneCommand = new RelayCommand(ExecuteClone);
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
        private void ExecuteDelete(object obj)
        {
            if (_currentBlockId.IsNull) return;

            AttributeActions.DeleteBlock(_currentBlockId);

            AttributeList.Clear();
            _currentBlockId = ObjectId.Null;
        }

        private void ExecuteClone(object obj)
        {
            if (_currentBlockId.IsNull) return;

            AttributeActions.CloneBlock(_currentBlockId);
        }
    }
}