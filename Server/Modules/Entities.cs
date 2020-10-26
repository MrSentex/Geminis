using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geminis.Server.Modules
{
    class Entities : InterconnectClient
    {
        private Players players;

        private Dictionary<int, string> entities = new Dictionary<int, string> {
            { 1, "" }
        };

        public void OnEntityCreating(int handle)
        {
            if (players == null)
            {
                return;
            }

            int sender_id = API.NetworkGetEntityOwner(handle);
            GeminisPlayer sender = players.GetPlayerFromId(sender_id.ToString());

            if (sender != null)
            {
                Entity entity = Entity.FromHandle(handle);
                Console.WriteLine(sender.Name);
            }

            
        }

        new public void init()
        {
            players = this.instances.Get("players");
        }
    }
}
