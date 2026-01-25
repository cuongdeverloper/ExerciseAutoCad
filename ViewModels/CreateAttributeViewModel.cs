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

        public ObservableCollection<RouteItemModel> AvailableRoutes => ProjectDataManager.Instance.GlobalRoutes;


        private RouteItemModel _selectedRoute;
        public RouteItemModel SelectedRoute
        {
            get => _selectedRoute;
            set { _selectedRoute = value; OnPropertyChanged(); }
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

        private List<ObjectId> _targetIds;

        public Action HideWindowAction { get; set; }
        public Action ShowWindowAction { get; set; }
        public Action<bool> CloseAction { get; set; }

        public ICommand AddAttributeCommand { get; set; }
        public ICommand SelectEntityCommand { get; set; }
        public ICommand AssignCommand { get; set; }

        public CreateAttributeViewModel()
        {
            Attributes = new ObservableCollection<AttributeItemModel>();
            _targetIds = new List<ObjectId>();

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
                if (IsByEntity)
                {
                    PromptSelectionResult psr = ed.GetSelection();
                    if (psr.Status == PromptStatus.OK)
                    {
                        _targetIds = psr.Value.GetObjectIds().ToList();
                        ed.WriteMessage($"\nĐã chọn {_targetIds.Count} đối tượng.");
                    }
                }
                else if (IsByLayer)
                {
                    var peo = new PromptEntityOptions("\nChọn đối tượng mẫu để lấy Layer:");
                    var per = ed.GetEntity(peo);

                    if (per.Status == PromptStatus.OK)
                    {
                        using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            string layerName = ent.Layer;
                            ed.WriteMessage($"\nĐã chọn mẫu thuộc layer: {layerName}. Đang lọc...");

                            TypedValue[] filterList = new TypedValue[] {
                                new TypedValue((int)DxfCode.LayerName, layerName)
                            };
                            SelectionFilter filter = new SelectionFilter(filterList);
                            PromptSelectionResult allOnLayer = ed.SelectAll(filter);

                            if (allOnLayer.Status == PromptStatus.OK)
                            {
                                _targetIds = allOnLayer.Value.GetObjectIds().ToList();
                                ed.WriteMessage($"\n-> Tìm thấy {_targetIds.Count} đối tượng trên layer {layerName}.");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi chọn đối tượng: " + ex.Message);
            }
            finally
            {
                ShowWindowAction?.Invoke();
            }
        }

        private void ExecuteAssign(object obj)
        {
            if (_targetIds == null || _targetIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đối tượng trước!", "Cảnh báo");
                return;
            }

            if (SelectedRoute == null)
            {
                MessageBox.Show("Vui lòng chọn Tên lộ!", "Cảnh báo");
                return;
            }

            XDataHelper.AddListAttributeXData(_targetIds, SelectedRoute.RouteName, Attributes.ToList());

            MessageBox.Show("Đã gán thuộc tính thành công!", "Thông báo");
            CloseAction?.Invoke(true);
        }
    }
}