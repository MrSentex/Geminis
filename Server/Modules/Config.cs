using System;

using Newtonsoft.Json;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using System.Collections.Generic;

namespace Geminis.Server.Modules
{
    class Config
    {
        public string locale { get; set; }

        public bool use_steam { get;  set; }

        public int timeout { get; set; }

        public Dictionary<string, List<int>> entities_ratelimit = new Dictionary<string, List<int>> { };
    }

    class ClientConfig
    {

    }

    class ConfigLoader : InterconnectClient
    {
        private Output output;
        private Utils utils;

        private Config config;
        private ClientConfig clientConfig;
        
        new public void init()
        {
            output = this.instances.Get("output");
            utils = this.instances.Get("utils");

            string rawServerConfig = API.LoadResourceFile(API.GetCurrentResourceName(), "Config/server.json");
            string rawClientConfig = API.LoadResourceFile(API.GetCurrentResourceName(), "Config/client.json");

            if (rawServerConfig == null || rawServerConfig == "")
            {
                output.Print(Output.FATAL, "Server config file is missing. Try to reinstall or contact with the staff");
                Environment.Exit(1);
            }

            if (rawClientConfig == null || rawClientConfig == "")
            {
                output.Print(Output.FATAL, "Client config file is missing. Try to reinstall or contact with the staff");
                Environment.Exit(1);
            }

            config = JsonConvert.DeserializeObject<Config>(rawServerConfig);
            clientConfig = JsonConvert.DeserializeObject<ClientConfig>(rawClientConfig);

            if (utils.IsAnyKeyOfClassNull(config))
            {
                output.Print(Output.ERROR, "Missing some server config parameters. If you update Geminis maybe the configuration file changed");
                Environment.Exit(1);
            }

            if (utils.IsAnyKeyOfClassNull(clientConfig))
            {
                output.Print(Output.ERROR, "Missing some client config parameters. If you update Geminis maybe the configuration file changed");
                Environment.Exit(1);
            }
        }

        public async void OnClientRequest([FromSource] Player player)
        {
            await BaseScript.Delay(0);

            player.TriggerEvent("geminis:set_client_config", clientConfig);
        }

        public ClientConfig GetClientConfig()
        {
            return clientConfig;
        }

        public Config GetConfig()
        {
            return config;
        }

    }
}
