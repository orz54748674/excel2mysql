using Excel;
using System;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace Excel2Mysql.util
{
    public class Excel
    {
        public static readonly Excel Instance = new Excel();

        public DataSet Load(string filePath, out string mysqlError)
        {
            mysqlError = "";

            DataSet result = null;

            try
            {
                FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                excelReader.IsFirstRowAsColumnNames = false;
                result = excelReader.AsDataSet();

                excelReader.Close();
            }
            catch (IOException e)
            {
                mysqlError = e.Message;
            }

            return result;
        }

        public void DataTabletoExcel(string fileName, System.Data.DataTable dt, Dictionary<string, string> columnInfo)
        {
            int rowIndex = 1;
            int columnIndex = 0;
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            app.DefaultFilePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            app.DisplayAlerts = true;
            app.SheetsInNewWorkbook = 1;
            Microsoft.Office.Interop.Excel.Workbook xlBook = app.Workbooks.Add(true);
            //将DataTable的列名导入Excel表第一行
            foreach (DataColumn dc in dt.Columns)
            {
                columnIndex++;
                app.Cells[rowIndex, columnIndex] = dc.ColumnName;
                string columnComment = "";
                if (columnInfo.ContainsKey(dc.ColumnName))
                {
                    columnComment = columnInfo[dc.ColumnName];
                }
                if (columnComment == "")
                {
                    columnComment = "无注释";
                }
                app.Cells[rowIndex + 1, columnIndex] = columnComment;
            }
            rowIndex++;
            //将DataTable中的数据导入Excel中
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                rowIndex++;
                columnIndex = 0;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    columnIndex++;
                    app.Cells[rowIndex, columnIndex] = dt.Rows[i][j].ToString();
                }
            }
            xlBook.SaveCopyAs(fileName + ".xlsx");
            xlBook.Close(false);
            app.Quit();
            Process.Kill(app);
        }
    }
}
