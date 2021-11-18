﻿using Global.ZennoLab.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings
{
    public class BasicSettingsModel
    {
        public BindingPhoneToChannelData BindingPhoneToChannel { get; set; }
        public ChangeChannelImageData ChangeChannelImage { get; set; }
        public ChangeChannelNameData ChangeChannelName { get; set; }
        public ChangeChannelDescriptionData ChangeChannelDescription { get; set; }
        public AddUrlToSocialNetworkData AddUrlToSocialNetwork { get; set; }
        public EnablePrivateMessagesData EnablePrivateMessages { get; set; }
        public SetMailData SetMail { get; set; }
        public AgreeToReceiveZenNewsletterData AgreeToReceiveZenNewsletter { get; set; }
        public SetSiteData SetSite { get; set; }
        public ConnectMetricData ConnectMetric { get; set; }
        public AcceptTermsOfUserAgreementData AcceptTermsOfUserAgreement { get; set; }

        public static BasicSettingsModel GetCurrentSettings()
        {
            var settingsFile = Services.ZenChannelCreationAndDesign.SettingsFile;

            if (settingsFile.Exists)
            {
                // Получение настроек из файла
                return JsonConvert.DeserializeObject<BasicSettingsModel>(File.ReadAllText(settingsFile.FullName, Encoding.UTF8));
            }
            else
            {
                // Создание нового экземпляра настроек
                var settings = new BasicSettingsModel();

                // Сохранение настроек в файл
                File.WriteAllText(settingsFile.FullName, JsonConvert.SerializeObject(settings, Formatting.Indented), Encoding.UTF8);

                return settings;
            }
        }

        public static BasicSettingsModel ExtractSettingsFromTemplateVariable(string templateVariableWithSettings)
        {
            var settings = new BasicSettingsModel
            {

            };

            return settings;
        }
    }
}
