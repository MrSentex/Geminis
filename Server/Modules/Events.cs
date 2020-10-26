using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geminis.Server.Modules
{
    class Events : InterconnectClient
    {
        private Output output;

        new public void init()
        {
            output = this.instances.Get("output");
        }

        public async void OnEvent([FromSource] Player source, string eventName, List<dynamic> arguments)
        {
            output.Print("New event:", eventName, "Arguments:", arguments.Count.ToString());
        }
    }
}
