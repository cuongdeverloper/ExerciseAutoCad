using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors; 
using Exercise.Models;

namespace Exercise.Actions
{
    public static class DrawActions
    {
        public static void CreateMLeader(TextSetting config)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();

            PromptPointOptions ppo = new PromptPointOptions("\nChọn điểm mũi tên: ");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            PromptPointOptions ppo2 = new PromptPointOptions("\nChọn điểm đặt chữ: ");
            ppo2.UseBasePoint = true;
            ppo2.BasePoint = ppr.Value;
            PromptPointResult ppr2 = ed.GetPoint(ppo2);
            if (ppr2.Status != PromptStatus.OK) return;

            using (DocumentLock loc = doc.LockDocument())
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                MLeader mld = new MLeader();
                mld.SetDatabaseDefaults();
                mld.ContentType = ContentType.MTextContent;

                // 1. Thiết lập nội dung Text Preview
                MText mt = new MText();
                mt.Contents = config.PreviewText;
                mt.Height = config.TextHeight;
                mld.MText = mt;

                // 2. Thiết lập màu sắc từ cấu hình
                // ColorIndex 4 là Cyan, 1 là Red, 2 là Yellow...
                mld.Color = Color.FromColorIndex(ColorMethod.ByAci, config.ColorIndex);

                // 3. Vẽ đường dẫn
                int idx = mld.AddLeader();
                int lineIdx = mld.AddLeaderLine(idx);
                mld.AddFirstVertex(lineIdx, ppr.Value);
                mld.AddLastVertex(lineIdx, ppr2.Value);

                btr.AppendEntity(mld);
                tr.AddNewlyCreatedDBObject(mld, true);
                tr.Commit();
            }
        }
    }
}