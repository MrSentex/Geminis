using System;
using System.Threading;

using CitizenFX.Core.Native;

namespace Geminis.Client.Modules
{
    class Config
    {

    }

    class ConfigLoader : InterconnectClient
    {
        private Config config;

        public void SetConfig(dynamic config)
        {
            this.config = config;
        }

        public Config GetConfig()
        {
            return config;
        }

    }
}
