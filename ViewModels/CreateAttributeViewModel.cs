using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
        public ObservableCollection<string> RouteNames { get; set; }
        private string _selectedRouteName;
        public string SelectedRouteName
        {
            get => _selectedRouteName;
            set { _selectedRouteName = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AttributeItemModel> Attributes { get; set; }

        private bool _isByEntity = true;
        public bool IsByEntity
        {
            get => _isByEntity;
            set { _isByEntity = value; OnPropertyChanged(); }
        }

        private bool _isByLayer;
        public bool IsByLayer
        {
            get => _isByLayer;
            set { _isByLayer = value; OnPropertyChanged(); }
        }

        private List<ObjectId> _selectedObjectIds;

        public Action HideWindowAction { get; set; }
        public Action ShowWindowAction { get; set; }
        public Action<bool> CloseAction { get; set; }

        public ICommand AddAttributeCommand { get; set; }
        public ICommand RemoveAttributeCommand { get; set; }
        public ICommand SelectEntityCommand { get; set; }
        public ICommand AssignCommand { get; set; }

        public CreateAttributeViewModel()
        {
            RouteNames = new ObservableCollection<string> { "Lộ S1", "Lộ S2", "Lộ S3", "Lộ Nước Cấp" };
            Attributes = new ObservableCollection<AttributeItemModel>();
            _selectedObjectIds = new List<ObjectId>();

            AddAttributeCommand = new RelayCommand(obj =>
            {
                Attributes.Add(new AttributeItemModel { Symbol = "", Quantity = 1 });
            });

            SelectEntityCommand = new RelayCommand(ExecuteSelectEntity);

            AssignCommand = new RelayCommand(ExecuteAssign);
        }

        private void ExecuteSelectEntity(object obj)
        {
            HideWindowAction?.Invoke();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                PromptSelectionResult psr = ed.GetSelection();

                if (psr.Status == PromptStatus.OK)
                {
                    var selectedIds = psr.Value.GetObjectIds().ToList();
                    _selectedObjectIds.Clear();

                    if (IsByEntity)
                    {
                        _selectedObjectIds.AddRange(selectedIds);
                        ed.WriteMessage($"\nĐã chọn {_selectedObjectIds.Count} đối tượng.");
                    }
                    else if (IsByLayer)
                    {
                        using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)tr.GetObject(selectedIds[0], OpenMode.ForRead);
                            string layerName = ent.Layer;
                            ed.WriteMessage($"\nĐã chọn mẫu thuộc layer: {layerName}. Đang lọc tất cả...");

                            TypedValue[] filterList = new TypedValue[] {
                                new TypedValue((int)DxfCode.LayerName, layerName)
                            };
                            SelectionFilter filter = new SelectionFilter(filterList);
                            PromptSelectionResult allOnLayer = ed.SelectAll(filter);

                            if (allOnLayer.Status == PromptStatus.OK)
                            {
                                _selectedObjectIds.AddRange(allOnLayer.Value.GetObjectIds());
                                ed.WriteMessage($"\n-> Đã tìm thấy tổng cộng {_selectedObjectIds.Count} đối tượng trên layer {layerName}.");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Lỗi chọn đối tượng: " + ex.Message);
            }
            finally
            {
                ShowWindowAction?.Invoke();
            }
        }

        private void ExecuteAssign(object obj)
        {
            if (_selectedObjectIds == null || _selectedObjectIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đối tượng trước!", "Cảnh báo");
                return;
            }

            if (string.IsNullOrEmpty(SelectedRouteName))
            {
                MessageBox.Show("Vui lòng chọn Tên lộ!", "Cảnh báo");
                return;
            }

            XDataHelper.AddListAttributeXData(_selectedObjectIds, SelectedRouteName, Attributes.ToList());

            MessageBox.Show("Đã gán thuộc tính thành công!", "Thông báo");
            CloseAction?.Invoke(true);
        }
    }
}