﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Workshop.Infrastructure.Services
{
    /// <summary>
    /// 读取配置文件
    /// </summary>
    public class AppConfigurtaionService
    {
        public static IConfiguration Configuration { get; set; }
        static AppConfigurtaionService()
        {
            Configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
                .Build();
        }
    }
}