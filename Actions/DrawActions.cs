using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Exercise.Models;

namespace Exercise.Actions
{
    public static class DrawActions
    {
        public static void DrawContinuousLines()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptPointOptions ppo1 = new PromptPointOptions("\nChọn điểm bắt đầu: ");
            PromptPointResult ppr1 = ed.GetPoint(ppo1);
            if (ppr1.Status != PromptStatus.OK) return;

            Point3d prevPt = ppr1.Value;

            while (true)
            {
                PromptPointOptions ppoNext = new PromptPointOptions("\nChọn điểm tiếp theo (Enter để dừng): ");
                ppoNext.UseBasePoint = true;
                ppoNext.BasePoint = prevPt;
                ppoNext.AllowNone = true;

                PromptPointResult pprNext = ed.GetPoint(ppoNext);
                if (pprNext.Status != PromptStatus.OK) break;

                Point3d currentPt = pprNext.Value;

                using (DocumentLock loc = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        Line acLine = new Line(prevPt, currentPt);

                        // SỬ DỤNG LAYER COLOR INDEX TỪ BRIDGE
                        acLine.ColorIndex = DrawDataBridge.LayerColorIndex;

                        btr.AppendEntity(acLine);
                        tr.AddNewlyCreatedDBObject(acLine, true);
                        tr.Commit();
                    }
                }
                prevPt = currentPt;
                ed.UpdateScreen();
            }
        }
    }
}