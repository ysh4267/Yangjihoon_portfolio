using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Threading;
using System;

namespace BulkEmailSender {
	internal class ExcelManager {
		public List<(string id, string body)> ReadData(string filePath) {
			List<(string id, string body)> dataList = new List<(string, string)>();

			// 엑셀 파일 읽기
			Excel.Application excelApp = new Excel.Application();
			Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
			Excel.Worksheet worksheet = workbook.Worksheets[1];

			int row = 1;
			while (worksheet.Cells[row, 1].Value != null) {
				string email = worksheet.Cells[row, 1].Value.ToString();
				string content = worksheet.Cells[row, 2].Value.ToString();
				dataList.Add((email, content));
				row++;
			}

			// 리소스 해제
			workbook.Close();
			excelApp.Quit();
			System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

			return dataList;
		}
	}
}
