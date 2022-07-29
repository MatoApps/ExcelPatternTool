﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using ExcelPatternTool.Core.Linq.Models;

namespace ExcelPatternTool.Core.Patterns
{
    public class Validation
    {
        private string _description;
        private string _expression;

        public Validation()
        {
            ProcessResult = new ProcessResult();
            ProcessResult.IsValidated = false;
            ProcessResult.Content =String.IsNullOrEmpty(Description) ? "需满足表达式" +Expression : Description;
            Target = Target.Value;//or Formula

        }


        [JsonConverter(typeof(StringEnumConverter))]
        public Target Target { get; set; }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                ProcessResult.Content = String.IsNullOrEmpty(_description) ? "需满足表达式" +Expression : _description;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Convention Convention { get; set; }

        [JsonIgnore]
        public ProcessResult ProcessResult { get; set; }

        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                ProcessResult.Content = String.IsNullOrEmpty(Description) ? "需满足表达式" +_expression : Description;
            }
        }
    }
}