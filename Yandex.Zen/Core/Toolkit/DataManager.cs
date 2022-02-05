﻿using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.ResourceObject;

namespace Yandex.Zen.Core.Toolkit
{
    public class DataManager
    {
        public Instance Browser { get; set; }
        public IZennoPosterProjectModel Zenno { get; set; }
        public ResourceObjectBase CurrentObject { get; private set; }
        public TableModel Table { get; set; }

        public DataManager(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Browser = instance;
        }

        /// <summary>
        /// Конфигурация проекта.
        /// </summary>
        public bool TryConfigureProjectSettings()
        {
            ConfigureProjectSettings(out var configurationStatus);
            return configurationStatus;
        }

        /// <summary>
        /// Конфигурация проекта.
        /// </summary>
        public void ConfigureProjectSettings(out bool configurationStatus)
        {
            try
            {
                //Program.CurrentMode = DictionariesAndLists.ProgramModes[Zenno.Variables["cfgTemplateMode"].Value];
                Program.CurrentService = ServiceLocator.TypesOfServices[Zenno.Variables["cfgTemplateMode"].Value];
                Logger.ConfigureGlobalLog(this);
                HE.ConfigureGlobalBrowse(this);
                SetBrowserSettings(Zenno.Variables["cfgInstanceWindowSize"].Value);
                Configure();
                configurationStatus = true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException(), LoggerType.Error, false, false, true, LogColor.Red);
                configurationStatus = false;
            }
        }

        /// <summary>
        /// Конфигурация объекта для работы.
        /// </summary>
        private void Configure()
        {
            Table = new TableModel(Zenno, "AccountsShared", Zenno.Variables["cfgPathFileAccounts"]);

            //CurrentObject = new ObjectBase(this)
            //{
            //    ProfileData = new ProfileDataModel(this)
            //    {
            //        UseWalkedProfileFromSharedFolder = bool.Parse(Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"].Value),
            //        MinProfileSizeToUse = int.Parse(Zenno.Variables["cfgMinSizeProfileUseInModes"].Value)
            //    },
            //    SmsService = new SmsService
            //    {
            //        Settings = new SmsServiceSettingsModel
            //        {
            //            TimeToSecondsWaitPhone = Zenno.Variables["cfgNumbAttempsGetPhone"].Value.ExtractNumber(),
            //            MinutesWaitSmsCode = Zenno.Variables["cfgNumbMinutesWaitSmsCode"].Value.Split(' ')[0].ExtractNumber(),
            //            AttemptsReSendSmsCode = Zenno.Variables["cfgNumbAttemptsRequestSmsCode"].Value.Split(' ')[0].ExtractNumber()
            //        },
            //        Params = new SmsServiceParamsDataModel(Zenno.Variables["cfgSmsServiceAndCountry"].Value)
            //    },
            //    CaptchaService = new CaptchaService { ServiceDll = Zenno.Variables["cfgCaptchaServiceDll"].Value },
            //    Settings = new TemplateSettingsModel { CreateFolderResourceIfNoExist = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value) },
            //    Channel = new ChannelModel()
            //};

            //CurrentObject.SetObject(Program.CurrentService);
        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private void SetBrowserSettings(string instanceWindowSize)
        {
            Browser.SetWindowSize
            (
                int.Parse(instanceWindowSize.Split('x')[0]),
                int.Parse(instanceWindowSize.Split('x')[1])
            );

            Browser.ClearCache();
            Browser.ClearCookie();

            Browser.IgnoreAdditionalRequests = false;
            Browser.IgnoreAjaxRequests = false;
            Browser.IgnoreFrameRequests = false;
            Browser.IgnoreFlashRequests = true;
        }
    }
}
