using System;
using System.Threading.Tasks;

using Geminis.Client.Modules;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Geminis.Client
{
    public class Main : BaseScript
    {
        private ConfigLoader configLoader;
        private Heartbeat heartbeat;
        private Resources resources;

        public Main()
        {
            InterconnectServer interconnectServer = new InterconnectServer();

            interconnectServer.Add("utils", new Utils());
            interconnectServer.Add("config", new ConfigLoader());
            interconnectServer.Add("heartbeat", new Heartbeat());
            interconnectServer.Add("resources", new Resources());

            configLoader = interconnectServer.Get("config");
            heartbeat = interconnectServer.Get("heartbeat");
            resources = interconnectServer.Get("resources");

            TriggerServerEvent("geminis:get_client_config");

            EventHandlers["geminis:set_client_config"] += new Action<dynamic>(configLoader.SetConfig);
            EventHandlers["geminis:set_identifier"] += new Action<string>(heartbeat.OnSetIdentifier);
            
            interconnectServer.Initialize();

            TriggerServerEvent("geminis:client_start_resource");

            EventHandlers["onClientResourceStart"] += new Action<string>(resources.OnResourceStart);
            EventHandlers["onClientResourceStop"] += new Action<string>(resources.OnResourceStop);

            EventHandlers["geminis:new_resource"] += new Action<string>(resources.OnNewResource);
        }

        [Tick]
        private async Task OnHeartbeatTick()
        {
            await heartbeat.OnTick();
            await Delay(0);
        }

        [Tick]

        private async Task OnResourcesTick()
        {
            await resources.OnTick();
            await Delay(0);
        }
    }
}