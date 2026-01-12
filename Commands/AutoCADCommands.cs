using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Exercise.Views;
using Exercise.ViewModels;
using System.Windows.Forms.Integration;

[assembly: CommandClass(typeof(Exercise.Commands.AppCommands))]

namespace Exercise.Commands
{
    public class AppCommands : IExtensionApplication
    {
        private static PaletteSet _ps = null;
        private static AttributeView _view = null;
        private static AttributeViewModel _viewModel = null;

        public void Initialize()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\n[Exercise] Plugin Edit Block Loaded!");
                doc.Editor.SelectionAdded += OnSelectionAdded;
            }
        }

        public void Terminate() { }

        private void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            if (_ps == null || !_ps.Visible) return;

            ObjectId[] ids = e.AddedObjects.GetObjectIds();
            if (ids.Length == 0) return;

            ObjectId id = ids[0];

            using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is BlockReference)
                {
                    _viewModel.LoadData(id);
                }
                tr.Commit();
            }
        }

        [CommandMethod("SHOW_PANEL")]
        public void ShowPanel()
        {
            if (_ps == null)
            {
                _ps = new PaletteSet("Quản Lý Block");
                _ps.Size = new System.Drawing.Size(300, 600);
                _ps.Dock = DockSides.Left;


                _view = new AttributeView();
                _viewModel = new AttributeViewModel();
                _view.DataContext = _viewModel;

                // 2. Tạo cầu nối ElementHost
                ElementHost host = new ElementHost();
                host.AutoSize = true;
                host.Dock = System.Windows.Forms.DockStyle.Fill;
                host.Child = _view; // Nhét WPF vào trong cầu nối

                // 3. Thêm cầu nối vào PaletteSet (chứ không thêm trực tiếp _view)
                _ps.Add("Attributes", host);

                // -----------------------------
            }
            _ps.Visible = true;
        }
    }
}