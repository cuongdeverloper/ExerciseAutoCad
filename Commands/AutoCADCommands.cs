using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Exercise.Models;
using Exercise.Services;
using Exercise.ViewModels;
using Exercise.Views;
using System;
using System.Collections.Generic;

[assembly: CommandClass(typeof(Exercise.Commands.AutoCADCommands))]

namespace Exercise.Commands
{
    public class AutoCADCommands : IExtensionApplication
    {
        public void Initialize()
        {
            UserSession.Instance.AuthenticationChanged += OnAuthenticationChanged;
        }

        public void Terminate()
        {
            UserSession.Instance.AuthenticationChanged -= OnAuthenticationChanged;
        }

        [CommandMethod("MY_LOGIN")]
        public void ShowLoginForm()
        {
            if (UserSession.Instance.IsLoggedIn)
            {
                Application.ShowAlertDialog("Bạn đã đăng nhập rồi!");
                return;
            }

            var loginView = new LoginView();
            Application.ShowModalWindow(loginView);
        }

        [CommandMethod("MYLOGOUT")]
        public void Logout()
        {
            UserSession.Instance.Logout();
            Application.ShowAlertDialog("Đã đăng xuất thành công.");
        }

        private void OnAuthenticationChanged(object sender, EventArgs e)
        {
            bool isLoggedIn = UserSession.Instance.IsLoggedIn;
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            string[] buttonsToEnable = {
                "BTN_SELECTPROJECT",
                "BTN_LOADSPEC",
                "BTN_CHECK",
                "BTN_CREATEROUTE",
                "BTN_CREATEATTR",
                "BTN_ASSIGNSPEC",
                "BTN_OVERRIDE"
            };

            string btnLoginId = "BTN_LOGIN";
            string btnLogoutId = "BTN_LOGOUT";

            foreach (var tab in ribbon.Tabs)
            {
                foreach (var panel in tab.Panels)
                {
                    foreach (var item in panel.Source.Items)
                    {
                        foreach (var targetId in buttonsToEnable)
                        {
                            if (IsMatchingId(item, targetId)) item.IsEnabled = isLoggedIn;
                        }
                        if (IsMatchingId(item, btnLoginId)) item.IsEnabled = !isLoggedIn;
                        if (IsMatchingId(item, btnLogoutId)) item.IsEnabled = isLoggedIn;
                    }
                }
            }
        }

        private bool IsMatchingId(RibbonItem item, string targetId)
        {
            if (item.Id == targetId) return true;
            if (item is RibbonRowPanel row)
            {
                foreach (var subItem in row.Items)
                {
                    if (subItem.Id == targetId) return true;
                }
            }
            return false;
        }

        [CommandMethod("MYROUTE")]
        public void CreateRoute()
        {
            if (!UserSession.Instance.IsLoggedIn)
            {
                Application.ShowAlertDialog("Bạn cần đăng nhập trước!");
                return;
            }

            if (ProjectDataManager.Instance.CurrentProject == null)
            {
                Application.ShowAlertDialog("Vui lòng chọn DỰ ÁN và THÁP trước khi tạo lộ!\n(Dùng lệnh MYPROJECT để chọn)");

                ShowProjectSelection();
                return;
            }

            var vm = new CreateRouteViewModel();
            var view = new CreateRouteView(vm);

            bool? result = Application.ShowModalWindow(view);

            if (result == true)
            {
                var selectedItem = vm.SelectedRoute;
                if (selectedItem != null)
                {
                    DrawPolylineRoute(selectedItem);
                }
            }
        }

        [CommandMethod("YATTR", CommandFlags.UsePickSet)] 
        public void ShowAttributeForm()
        {
            if (!UserSession.Instance.IsLoggedIn)
            {
                Application.ShowAlertDialog("Bạn cần đăng nhập trước!");
                return;
            }

            if (ProjectDataManager.Instance.CurrentProject == null)
            {
                Application.ShowAlertDialog("Vui lòng chọn DỰ ÁN và THÁP trước!");
                ShowProjectSelection();
                return;
            }

            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            List<ObjectId> selectedIds = new List<ObjectId>();

            var psrImplied = ed.SelectImplied();
            if (psrImplied.Status == PromptStatus.OK)
            {
                selectedIds.AddRange(psrImplied.Value.GetObjectIds());
            }
            else
            {
                var peo = new PromptSelectionOptions();
                peo.MessageForAdding = "\nQuét chọn các đối tượng cần gán thuộc tính: ";

                var psr = ed.GetSelection(peo);
                if (psr.Status == PromptStatus.OK)
                {
                    selectedIds.AddRange(psr.Value.GetObjectIds());
                }
                else
                {
                    return;
                }
            }

            if (selectedIds.Count == 0) return;

            var vm = new CreateAttributeViewModel(selectedIds);
            var view = new CreateAttributeView(vm);

            Application.ShowModalWindow(view);
        }

        [CommandMethod("MYPROJECT")]
        public void ShowProjectSelection()
        {
            if (!UserSession.Instance.IsLoggedIn)
            {
                Application.ShowAlertDialog("Bạn cần đăng nhập trước!");
                return;
            }


            var vm = new SelectProjectViewModel();
            var view = new SelectProjectView(vm);

            Application.ShowModalWindow(view);
        }

        private double ParseSizeString(string sizeStr)
        {
            if (string.IsNullOrEmpty(sizeStr)) return 100;
            string[] parts = sizeStr.ToLower().Split('x');
            if (parts.Length > 0 && double.TryParse(parts[0], out double w))
            {
                return w;
            }
            return 100;
        }

        private void DrawPolylineRoute(RouteItemModel data)
        {
            double width = ParseSizeString(data.Size);
            double elevation = data.Elevation;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptPointOptions ppo = new PromptPointOptions("\nChọn điểm bắt đầu của lộ:");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            Point3d startPt = ppr.Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                using (Polyline pline = new Polyline())
                {
                    pline.ConstantWidth = width;
                    pline.Elevation = elevation;
                    pline.AddVertexAt(0, new Point2d(startPt.X, startPt.Y), 0, 0, 0);

                    int index = 1;
                    while (true)
                    {
                        PromptPointOptions ppoNext = new PromptPointOptions("\nChọn điểm tiếp theo (Enter để kết thúc):");
                        ppoNext.UseBasePoint = true;
                        ppoNext.BasePoint = pline.GetPoint3dAt(index - 1);
                        ppoNext.AllowNone = true;

                        PromptPointResult pprNext = ed.GetPoint(ppoNext);
                        if (pprNext.Status != PromptStatus.OK) break;

                        pline.AddVertexAt(index, new Point2d(pprNext.Value.X, pprNext.Value.Y), 0, 0, 0);
                        index++;
                    }

                    if (pline.NumberOfVertices > 1)
                    {
                        ObjectId plineId = btr.AppendEntity(pline);
                        tr.AddNewlyCreatedDBObject(pline, true);
                        XDataHelper.AddRouteXData(plineId, data);
                        tr.Commit();
                        ed.WriteMessage($"\nĐã vẽ tuyến '{data.RouteName}' và lưu dữ liệu XData thành công.");
                    }
                }
            }
        }
        [CommandMethod("MYCHECK")]
        public void CheckXData()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            // 1. Chọn đối tượng cần soi
            var peo = new PromptEntityOptions("\nChọn đối tượng để kiểm tra XData: ");
            var per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var ent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForRead);

                var rb = ent.GetXDataForApplication("MY_ROUTE_APP");

                if (rb == null)
                {
                    Application.ShowAlertDialog("Đối tượng này KHÔNG CÓ dữ liệu của chương trình!");
                }
                else
                {
                    string info = "DỮ LIỆU TÌM THẤY:\n-----------------\n";
                    foreach (var val in rb)
                    {
                        if (val.TypeCode == (short)DxfCode.ExtendedDataAsciiString)
                        {
                            info += val.Value.ToString() + "\n";
                        }
                    }
                    Application.ShowAlertDialog(info);
                }
                tr.Commit();
            }
        }
    }
}