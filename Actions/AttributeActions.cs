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
    }
}