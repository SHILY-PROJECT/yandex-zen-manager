﻿using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.ZenChannelCreationAndDesignService.Models.ChannelSettings.DataModels
{
    public class SetMailData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string Mail { get; set; }
    }
}