﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelPatternTool.Contracts.Validations;
using ExcelPatternTool.Core.Helper;
using ExcelPatternTool.Core.Patterns;

namespace ExcelPatternTool.Validation.Validators.Implements
{
    public class DefaultValidatorProvider : ValidatorProvider
    {
        public override Dictionary<string, IValidation> GetValidationContainers(Type entityType)
        {
            var result = LocalDataHelper.ReadObjectLocal<Pattern>();
            return new Dictionary<string, IValidation>(
                result.Patterns.Select(c=> new KeyValuePair<string, IValidation>(c.PropName, c.Validation)));
        }
    }
}
