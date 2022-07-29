﻿using System;
using System.Collections.Generic;
using ExcelPatternTool.Core.Patterns;

namespace ExcelPatternTool.Core.Validators
{
    public interface IValidatorProvider
    {
        Func<string, string> PropertyTypeMaper { get; set; }
        IEnumerable<PatternItem> GetPatternItems();
        ValidateConvention GetConvention(string type);
    }
}