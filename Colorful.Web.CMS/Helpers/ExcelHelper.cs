using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Web.CMS
{
    public class ExcelHelper : IDisposable
    {
        private ExcelPackage _package;
        private ExcelWorkbook _workbook;
        private ExcelWorksheet _worksheet;
        private string _excelFile;

        #region 属性
        public ExcelWorkbook Workbook
        {
            get
            {
                return _workbook;
            }
        }
        public ExcelWorksheet Worksheet
        {
            get
            {
                return _worksheet;
            }
        }
        #endregion

        #region 初始化
        public ExcelHelper(string excelFile)
        {
            _excelFile = excelFile;
            _package = new ExcelPackage(new System.IO.FileInfo(excelFile));
            _workbook = _package.Workbook;
            if (_workbook.Worksheets.Count == 0)
                _worksheet = _workbook.Worksheets.Add("Sheet1");
            else
                _worksheet = _workbook.Worksheets.FirstOrDefault();
        }
        #endregion

        #region 替换方法
        /// <summary>
        /// 替换Excel中的文本要
        /// </summary>
        /// <param name="text">要查找的文本</param>
        /// <param name="replaceText">要替换的文本</param>
        public void Replace(string text, string replaceText)
        {
            this.Replace(new ExcelReplace(text, replaceText));
        }
        /// <summary>
        /// 替换文本
        /// </summary>
        /// <param name="replaceText">要替换的文本数组：[0]要查找的字符串，[1]要替换的字符串</param>
        public void Replace(params ExcelReplace[] replaceText)
        {
            Replace((cell, replace) =>
            {
                return true;
            }, replaceText);
        }
        public void Replace(Func<ExcelRangeBase, ExcelReplace, bool> cb, params ExcelReplace[] replaceTexts)
        {
            foreach (var cell in _worksheet.Cells)
            {
                foreach (var item in replaceTexts)
                {
                    if (cell.Text.Contains(item.FindText))
                    {
                        var isReplace = cb(cell, item);
                        if (isReplace)
                            cell.Value = cell.Text.Replace(item.FindText, item.ReplaceText);
                    }
                }
            }
        }
        #endregion

        #region 导出Excel Export
        public void Export(string[] titles, List<string[]> datas, string sheetName = "Sheet1")
        {
            this.Export(new ExportOption() { Titles = titles, Datas = datas, SheetName = sheetName });
        }
        public void Export(params ExportOption[] opts)
        {
            if (System.IO.File.Exists(_excelFile))
                System.IO.File.Delete(_excelFile);
            var count = 0;
            foreach (var opt in opts)
            {
                count++;
                ExcelWorksheet worksheet;
                if (count == 1)
                {
                    worksheet = this.Worksheet;
                    worksheet.Name = opt.SheetName;
                }
                else
                {
                    worksheet = _workbook.Worksheets.Add(opt.SheetName);
                }
                var datas = new List<string[]>();
                datas.Add(opt.Titles);
                datas.AddRange(opt.Datas);
                worksheet.Cells.LoadFromArrays(datas);
                List<int> calcCols = new List<int>();
                for (var i = 0; i < opt.Titles.Length; i++)
                {
                    if (opt.Titles[i].StartsWith("#"))
                    {
                        worksheet.Cells[0 + 1, i + 1].Value = opt.Titles[i].Substring(1);
                        calcCols.Add(i + 1);
                    }
                    //worksheet.Cells[0 + 1, i + 1].Value = opt.Titles[i].StartsWith("#") ? opt.Titles[i].Substring(1) : opt.Titles[i];
                }
                if (calcCols.Count>0)
                {
                    foreach (var colIndex in calcCols)
                    {
                        
                    }
                }
                //worksheet.Cells.AutoFitColumns();
            }
            this.Save();
        }
        #endregion

        #region 插入数据
        public void Insert(string[] datas)
        {
            var rowIndex = _worksheet.Cells.Rows + 1;
            for (var i = 0; i < datas.Length; i++)
                _worksheet.Cells[rowIndex, i + 1].Value = datas[i];
        }
        #endregion

        #region 获取Excel中的数据 GetDatas
        /// <summary>
        /// 获取Excel中的数据
        /// </summary>
        /// <param name="haveTitle">返回的数据中是否包含标题</param>
        /// <returns></returns>
        public List<string[]> GetDatas(bool haveTitle = true)
        {
            var list = new List<string[]>();
            int colStart = Worksheet.Dimension.Start.Column;  //工作区开始列
            int colEnd = Worksheet.Dimension.End.Column;       //工作区结束列
            int rowStart = Worksheet.Dimension.Start.Row;       //工作区开始行号
            int rowEnd = Worksheet.Dimension.End.Row;       //工作区结束行号

            //添加标题
            if (haveTitle)
            {
                var titles = new List<string>();
                for (int i = colStart; i <= colEnd; i++)
                {
                    titles.Add(Worksheet.Cells[rowStart, i].Value.ToString());
                }
                list.Add(titles.ToArray());
            }
            //添加数据
            for (var row = rowStart + 1; row <= rowEnd; row++)
            {
                var items = new List<string>();
                for (int i = colStart; i <= colEnd; i++)
                {
                    items.Add($"{Worksheet.Cells[row, i].Value}".Trim());
                }
                list.Add(items.ToArray());
            }
            return list;
        }
        #endregion

        #region 保存 SaveAs
        public void SaveAs(string path)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            FormHelper.CreateDirectory(dir);
            _package.SaveAs(new System.IO.FileInfo(path));
        }
        public void Save()
        {
            _package.Save();
        }
        #endregion

        public void Dispose()
        {
            _package.Dispose();
        }
    }

    #region ExportOption
    public class ExportOption
    {
        public bool AutoFitColumn { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string[] Titles { get; set; }
        /// <summary>
        /// 导出的数据
        /// </summary>
        public List<string[]> Datas { get; set; }
        /// <summary>
        /// Excel表Sheet名称
        /// </summary>
        public string SheetName { get; set; }
    }
    #endregion

    #region ReplaceText
    public class ExcelReplace
    {
        public string FindText { get; set; }
        public string ReplaceText { get; set; }
        public ExcelReplace(string text, string replaceText)
        {
            this.FindText = text;
            this.ReplaceText = replaceText;
        }
    }
    #endregion
}