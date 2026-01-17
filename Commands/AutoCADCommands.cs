using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Exercise.Models;
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

            // Tìm Ribbon Control
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            string[] buttonsToEnable = { "BTN_SETTINGS", "BTN_LOADSPEC", "BTN_CHECK" };
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
            // 1. Kiểm tra đăng nhập (nếu cần)
            if (!UserSession.Instance.IsLoggedIn)
            {
                Application.ShowAlertDialog("Bạn cần đăng nhập trước!");
                return;
            }

            // 2. Khởi tạo ViewModel và View
            var vm = new CreateRouteViewModel();
            var view = new CreateRouteView(vm);

            // 3. Hiện bảng lên (ShowDialog sẽ dừng code tại đây cho đến khi user đóng bảng)
            bool? result = Application.ShowModalWindow(view);

            // 4. Nếu user bấm nút "Vẽ tuyến" (Result = true) thì bắt đầu vẽ
            if (result == true)
            {
                DrawPolylineRoute(vm.Width, vm.Elevation);
            }
        }

        // Hàm hỗ trợ vẽ Polyline
        private void DrawPolylineRoute(double width, double elevation)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // A. Yêu cầu người dùng chọn điểm
            PromptPointOptions ppo = new PromptPointOptions("\nChọn điểm bắt đầu của lộ:");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            Point3d startPt = ppr.Value;

            // B. Bắt đầu Transaction để vẽ
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Tạo Polyline
                using (Polyline pline = new Polyline())
                {
                    // Set độ dày cho đường (giả lập bề rộng máng)
                    pline.ConstantWidth = width;

                    // Thêm điểm đầu (Lưu ý: Polyline 2D dùng Point2d, Cao độ dùng Elevation)
                    pline.Elevation = elevation;
                    pline.AddVertexAt(0, new Point2d(startPt.X, startPt.Y), 0, 0, 0);

                    // Vòng lặp để vẽ tiếp các điểm sau
                    int index = 1;
                    while (true)
                    {
                        PromptPointOptions ppoNext = new PromptPointOptions("\nChọn điểm tiếp theo (hoặc Enter để kết thúc):");
                        ppoNext.AllowNone = true;
                        ppoNext.UseBasePoint = true;
                        ppoNext.BasePoint = pline.GetPoint3dAt(index - 1); // Elastic line từ điểm trước

                        PromptPointResult pprNext = ed.GetPoint(ppoNext);
                        if (pprNext.Status != PromptStatus.OK) break; 

                        pline.AddVertexAt(index, new Point2d(pprNext.Value.X, pprNext.Value.Y), 0, 0, 0);
                        index++;
                    }

                    if (pline.NumberOfVertices > 1)
                    {
                        btr.AppendEntity(pline);
                        tr.AddNewlyCreatedDBObject(pline, true);
                        tr.Commit(); 
                        ed.WriteMessage($"\nĐã vẽ tuyến rộng {width}mm tại cao độ {elevation}mm.");
                    }
                }
            }
        }
    }
}