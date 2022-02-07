﻿using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.WalkerOnZenService
{
    public class WalkerOnZen : IService
    {
        public WalkerOnZen(IDataManager manager)
        {
            DataManager = manager;
        }

        public IDataManager DataManager { get; set; }
        public IAccount Account { get; set; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(WalkerOnZen)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
