﻿using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Workshop.Infrastructure.Interfaces;

namespace Workshop.Infrastructure.Core
{
    public class XlsReader : BaseReader, IReader
    {
        MemoryStream mem;
        private FileStream fileStr;
        private HSSFWorkbook document;
        private ISheet sheet;
        public XlsReader(byte[] data)
        {
            mem = new MemoryStream(data);
            document = new HSSFWorkbook(mem);
        }

        public XlsReader(string filePath)
        {
            fileStr = new FileStream(filePath, FileMode.Open);
            document = new HSSFWorkbook(fileStr);
        }

        public IEnumerable<T> ReadRows<T>(IImportOption importOption)
        {

            List<T> result = new List<T>();
            var columns = GetTypeDefinition(typeof(T));
            sheet = document.GetSheet(importOption.SheetName);
            int firstRow = sheet.FirstRowNum;
            int lastRow = sheet.LastRowNum;
            for (int i = firstRow + importOption.SkipRows; i <= lastRow; i++)
            {
                T objectInstance;
                IRow row = sheet.GetRow(i);
                if (row != null)
                {
                    objectInstance = GetDataToObject<T>(row, columns);
                    result.Add(objectInstance);
                }

            }
            return result;

        }

        public IEnumerable<T> ReadRows<T>(int sheetNumber, int rowsToSkip)
        {
            List<T> result = new List<T>();
            var columns = GetTypeDefinition(typeof(T));
            sheet = document.GetSheetAt(sheetNumber);
            int firstRow = sheet.FirstRowNum;
            int lastRow = sheet.LastRowNum;
            for (int i = firstRow + rowsToSkip; i <= lastRow; i++)
            {
                IRow row = sheet.GetRow(i);
                T objectInstance = GetDataToObject<T>(row, columns);
                result.Add(objectInstance);
            }
            return result;
        }


    }
}