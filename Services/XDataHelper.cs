using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Exercise.Models;

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

                // DxfCode.ExtendedDataRegAppName (1001): Bắt buộc dòng đầu tiên
                // DxfCode.ExtendedDataAsciiString (1000): Lưu chuỗi
                // DxfCode.ExtendedDataReal (1040): Lưu số thực
                // DxfCode.ExtendedDataInteger32 (1070): Lưu số nguyên

                ResultBuffer rb = new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),

                    // Lưu Tên Tuyến
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "RouteName:" + data.RouteName),

                    // Lưu Batch (Quan trọng để mentor check)
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Batch:" + (data.SelectedBatch ?? "")),

                    // Lưu Kích thước
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Size:" + data.Size),

                    // Lưu Vật tư
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Material:" + (data.SelectedMaterial ?? "")),

                    // Lưu Cao độ (Lưu dạng số thực)
                    new TypedValue((int)DxfCode.ExtendedDataReal, data.Elevation)
                );

                ent.XData = rb;

                tr.Commit();
            }
        }
    }
}