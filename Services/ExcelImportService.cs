using ClosedXML.Excel;
using Exercise.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Exercise.Services
{
    public class ExcelImportService
    {
        public List<RouteImportModel> ImportRoutes(string filePath)
        {
            var listRoutes = new List<RouteImportModel>();

            try
            {
                if (!File.Exists(filePath)) return listRoutes;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet 1
                    var lastRow = worksheet.LastRowUsed().RowNumber();

                    // Đọc từ dòng 2 (giả sử dòng 1 là tiêu đề)
                    for (int i = 2; i <= lastRow; i++)
                    {
                        var row = worksheet.Row(i);
                        var route = new RouteImportModel();

                        // Cột 1: Tên lộ
                        route.RouteName = row.Cell(1).GetValue<string>();

                        // Cột 2: Bề rộng
                        double w = 0;
                        row.Cell(2).TryGetValue(out w);
                        route.Width = w;

                        // Cột 3: Cao độ
                        double h = 0;
                        row.Cell(3).TryGetValue(out h);
                        route.Height = h;

                        // Cột 4: Cao độ đáy
                        double b = 0;
                        row.Cell(4).TryGetValue(out b);
                        route.BottomElevation = b;

                        listRoutes.Add(route);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi đọc file: " + ex.Message);
            }

            return listRoutes;
        }
    }
}