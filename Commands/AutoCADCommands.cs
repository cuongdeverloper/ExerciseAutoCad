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

        [CommandMethod("MYLOGIN")]
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

            var vm = new CreateRouteViewModel();
            var view = new CreateRouteView(vm); // Đảm bảo View đã cập nhật XAML mới

            bool? result = Application.ShowModalWindow(view);

            if (result == true)
            {
                // --- SỬA ĐOẠN NÀY ---
                // Lấy dòng đang được chọn trên lưới
                var selectedItem = vm.SelectedRoute;

                if (selectedItem != null)
                {

                    DrawPolylineRoute(selectedItem);
                }
            }
        }

        // Hàm tách chuỗi kích thước mới
        private double ParseSizeString(string sizeStr)
        {
            if (string.IsNullOrEmpty(sizeStr)) return 100;

            // Xử lý chuỗi dạng "300x100" hoặc "300"
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
    }
}