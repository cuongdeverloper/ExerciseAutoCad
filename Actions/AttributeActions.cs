using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors; 
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Exercise.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Exercise.Actions
{
    public static class AttributeActions
    {
        public static ObservableCollection<AttributeItem> GetAttributes(ObjectId blockId)
        {
            var list = new ObservableCollection<AttributeItem>();
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockReference br = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                if (br != null)
                {
                    foreach (ObjectId attId in br.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        list.Add(new AttributeItem
                        {
                            Tag = attRef.Tag,
                            Value = attRef.TextString
                        });
                    }
                }
                tr.Commit();
            }
            return list;
        }

        public static void UpdateAttributes(ObjectId blockId, IEnumerable<AttributeItem> items)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (DocumentLock loc = doc.LockDocument())
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockReference br = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                if (br != null)
                {
                    foreach (ObjectId attId in br.AttributeCollection)
                    {
                        AttributeReference attRef = tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;

                        foreach (var item in items)
                        {
                            if (item.Tag == attRef.Tag)
                            {
                                attRef.TextString = item.Value;
                                break;
                            }
                        }
                    }
                }
                tr.Commit();
                doc.Editor.Regen(); 
            }
        }
        // D - DELETE: Xóa Block
        public static void DeleteBlock(ObjectId blockId)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock loc = doc.LockDocument())
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(blockId, OpenMode.ForWrite);
                obj.Erase(true); // Lệnh xóa
                tr.Commit();
                doc.Editor.Regen();
            }
        }

        // C - CREATE:
        public static void CloneBlock(ObjectId blockId)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (DocumentLock loc = doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 1. Lấy Block gốc (ép kiểu về BlockReference để lấy được Attribute)
                BlockReference sourceBlk = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;

                if (sourceBlk != null)
                {
                    // 2. Clone cái "vỏ" Block (Hình dáng)
                    BlockReference newBlk = sourceBlk.Clone() as BlockReference;

                    // 3. Tạo ma trận dịch chuyển (Sang phải 500 đơn vị)
                    // Lưu biến này để tí nữa dùng dịch chuyển cả chữ
                    Matrix3d matrix = Matrix3d.Displacement(new Vector3d(500, 0, 0));
                    newBlk.TransformBy(matrix);

                    // 4. Đưa Block mới vào Database trước
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    btr.AppendEntity(newBlk);
                    tr.AddNewlyCreatedDBObject(newBlk, true);

                    foreach (ObjectId attId in sourceBlk.AttributeCollection)
                    {
                        AttributeReference att = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;

                        // Clone ra attribute mới
                        AttributeReference newAtt = att.Clone() as AttributeReference;

                        // Dịch chuyển attribute mới theo cùng vị trí với Block mới
                        newAtt.TransformBy(matrix);

                        // Gắn attribute mới vào Block mới
                        newBlk.AttributeCollection.AppendAttribute(newAtt);
                        tr.AddNewlyCreatedDBObject(newAtt, true);
                    }
                    // -----------------------------------------------------
                }

                tr.Commit();
                doc.Editor.Regen();
            }
        }
    }
}