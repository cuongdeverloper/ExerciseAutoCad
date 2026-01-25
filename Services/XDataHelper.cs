using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Exercise.Models;
using System.Collections.Generic;
using System.Linq; 

namespace Exercise.Services
{
    public static class XDataHelper
    {
        private const string AppName = "MY_ROUTE_APP";

        public static void AddRouteXData(ObjectId entityId, RouteItemModel data)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                RegAppTable regTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                if (!regTable.Has(AppName))
                {
                    regTable.UpgradeOpen();
                    RegAppTableRecord regRec = new RegAppTableRecord();
                    regRec.Name = AppName;
                    regTable.Add(regRec);
                    tr.AddNewlyCreatedDBObject(regRec, true);
                }

                Entity ent = (Entity)tr.GetObject(entityId, OpenMode.ForWrite);

                ResultBuffer rb = new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "RouteName:" + data.RouteName),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Batch:" + (data.SelectedBatch ?? "")),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Size:" + data.Size),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Material:" + (data.SelectedMaterial ?? "")),
                    new TypedValue((int)DxfCode.ExtendedDataReal, data.Elevation)
                );

                ent.XData = rb;
                tr.Commit();
            }
        }

        // [QUAN TRỌNG] Hàm này phải nằm TRONG class XDataHelper (trước dấu đóng ngoặc class)
        public static void AddListAttributeXData(List<ObjectId> entityIds, string routeName, List<AttributeItemModel> attributes)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                RegAppTable regTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                if (!regTable.Has(AppName))
                {
                    regTable.UpgradeOpen();
                    RegAppTableRecord regRec = new RegAppTableRecord();
                    regRec.Name = AppName;
                    regTable.Add(regRec);
                    tr.AddNewlyCreatedDBObject(regRec, true);
                }

                // Chuyển List thành chuỗi, ví dụ: "K1:10;K2:5"
                string attrString = string.Join(";", attributes.Select(x => $"{x.Symbol}:{x.Quantity}"));

                foreach (ObjectId id in entityIds)
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                    ResultBuffer rb = new ResultBuffer(
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                        new TypedValue((int)DxfCode.ExtendedDataAsciiString, "RouteName:" + routeName),
                        new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Attributes:" + attrString)
                    );

                    ent.XData = rb;
                }

                tr.Commit();
            }
        }
    } 
}