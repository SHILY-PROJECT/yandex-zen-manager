﻿using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class BrowserAccountManager : IService
    {
        public BrowserAccountManager(IDataManager manager, IServiceConfiguration configuration)
        {
            DataManager = manager;
            Configuration = configuration;
        }

        public IDataManager DataManager { get; set; }
        public IResourceObject ResourceObject { get; set; }
        public IServiceConfiguration Configuration { get; }
        public static bool IsInProcess { get; set; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(BrowserAccountManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
