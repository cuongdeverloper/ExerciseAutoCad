using Autodesk.AutoCAD.Runtime;

namespace Exercise
{
    public class Class1
    {
        [CommandMethod("MYSET")]
        public void OpenWindow()
        {
            var win = new Exercise.Views.Window1();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessWindow(win);
        }

        [CommandMethod("DRAW_CONTINUOUS_LINE_INTERNAL", CommandFlags.NoHistory)]
        public void InternalDraw()
        {
            Exercise.Actions.DrawActions.DrawContinuousLines();
        }
    }
}