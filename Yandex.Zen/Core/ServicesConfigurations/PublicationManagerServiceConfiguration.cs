﻿using System.Collections.Generic;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.ServicesConfigurations.Base;

namespace Yandex.Zen.Core.ServicesConfigurations
{
    public sealed class PublicationManagerServiceConfiguration : BaseAccountConfiguration, IServiceConfiguration
    {
        public PublicationManagerServiceConfiguration(IDataManager manager) : base(manager)
        {

        }

        protected override void ServiceConfigure()
        {
            throw new System.NotImplementedException();
        }

        protected override void SetUpAccount(IEnumerable<string> columns)
        {
            throw new System.NotImplementedException();
        }
    }
}
