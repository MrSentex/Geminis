using System;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Geminis.Client.Modules
{
    class Heartbeat : InterconnectClient
    {
        protected string self_identifier;

        public async void OnSetIdentifier(string identifier)
        {
            self_identifier = identifier;
        }

        public async Task OnTick()
        {
            if (self_identifier == null)
            {
                Debug.WriteLine("Waiting the Geminis identifier...");
                await BaseScript.Delay(500);
            } else
            {
                BaseScript.TriggerServerEvent("geminis:heartbeat", self_identifier);
                await BaseScript.Delay(2000);
            }
        }
    }
}
