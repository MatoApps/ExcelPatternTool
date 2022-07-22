﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Workshop.Core.Entites;
using Workshop.Core.Excel.Core;
using Workshop.Core.Excel.Models;

namespace Workshop.Infrastructure.Tests
{
    [TestClass()]
    public class XlsxReaderTests
    {
        [TestMethod()]
        public void ReadRowsTest()
        {
            Importer import = new Importer();
            var filePath = @"D:\test.xlsx";
            var data1 = new byte[0];

            data1 = File.ReadAllBytes(filePath);
            import.LoadXlsx(data1);
            var importOption = new ImportOption<EmployeeEntity>(0, 2);
            importOption.SheetName = "全职";
            var output = import.Process<EmployeeEntity>(importOption).ToList();
            foreach (var empoyeeEntity in output)
            {
                if (empoyeeEntity.AgeBonus < 0 )
                {
                    Console.WriteLine(empoyeeEntity.Name);
                }

            }
            Assert.IsNotNull(output);
        }
    }
}