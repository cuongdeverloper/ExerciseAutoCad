using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Exercise.Views;
using Exercise.ViewModels;

[assembly: CommandClass(typeof(Exercise.Commands.AppCommands))]

namespace Exercise.Commands
{
    public class AppCommands : IExtensionApplication
    {
        private static Window1 _window;

        #region IExtensionApplication Members

        public void Initialize()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n[Exercise] Plugin đã được khởi tạo thành công!");
        }

        public void Terminate()
        {
            if (_window != null)
            {
                _window.Close();
            }
        }

        #endregion

        #region Commands

        [CommandMethod("HELLOWORLD")]
        public void HelloWorld()
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nHello, AutoCAD!");
        }

        [CommandMethod("VE_MLEADER")]
        public void ShowMLeaderSetup()
        {
            try
            {
                if (_window != null && _window.IsVisible)
                {
                    _window.Activate();
                    return;
                }

                _window = new Window1();

                var viewModel = new TextSetupViewModel();
                _window.DataContext = viewModel;

                Application.ShowModelessWindow(_window);

                _window.Closed += (s, e) => { _window = null; };
            }
            catch (System.Exception ex)
            {
                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("\nLỗi khi mở giao diện: " + ex.Message);
            }
        }

        #endregion
    }
}