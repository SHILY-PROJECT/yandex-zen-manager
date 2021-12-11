﻿using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.ChannelHandlerService.Models.ChannelSettings.DataModels
{
    public class SetSiteData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string SiteUrl { get; set; }
    }
}