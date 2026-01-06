using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Exercise.Views;
using Exercise.ViewModels;
using System;

[assembly: CommandClass(typeof(Exercise.Commands.AutoCADCommands))]

namespace Exercise.Commands
{
    public class AutoCADCommands
    {
        private static Window1 _window;

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
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nLỗi: " + ex.Message);
            }
        }
    }
}