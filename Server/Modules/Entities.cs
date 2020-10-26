using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geminis.Server.Modules
{
    class EntityRatelimit
    {
        private Dictionary<int, int> data = new Dictionary<int, int> { };
        public int time;
        private int limit;
        private Task deleter;

        public async Task Deleter()
        {
            while (true)
            {
                foreach(KeyValuePair<int, int> pair in this.data)
                {
                    if (pair.Key + time/1000 < this.GetTimestamp())
                    {
                        this.data.Remove(pair.Key);
                    }
                }

                Thread.Sleep(this.time * 1000);
            }
        }

        private int GetTimestamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public EntityRatelimit(int time, int limit)
        {
            this.time = time;
            this.limit = limit;
            this.deleter = Task.Run(this.Deleter);
        }

        public int GetCount()
        {
            int count = 0;

            foreach (KeyValuePair<int, int> pair in this.data)
            {
                count += pair.Value;
            }

            return count;
        }

        public bool AddAndCheck()
        {
            int timestamp = this.GetTimestamp();

            if (this.data.TryGetValue(timestamp, out _))
            {
                this.data[timestamp]++;
            } else
            {
                this.data.Add(timestamp, 1);
            }

            return this.GetCount() > this.limit;
        }
    }

    class Entities : InterconnectClient
    {
        private Utils utils;
        private Config config;
        private Locale locale;
        private Players players;

        private Dictionary<int, string> entities = new Dictionary<int, string> {
            { 1, "vehicle" }
        };

        public string TypeToName(int type)
        {
            if (entities.TryGetValue(type, out _)) {
                return entities[type];
            }

            return null;
        }

        public int NameToType(string name)
        {
            foreach (KeyValuePair<int, string> pair in this.entities)
            {
                if (pair.Value == name)
                {
                    return pair.Key;
                }
            }

            return -1;
        }

        public void OnPlayerConnecting(GeminisPlayer geminisPlayer)
        {
            geminisPlayer.SetData("entities_ratelimit", new Dictionary<string, EntityRatelimit> { });
            Dictionary<string, EntityRatelimit> playerRatelimit = geminisPlayer.GetData("entities_ratelimit");


            foreach (KeyValuePair<string, List<int>> pair in config.entities_ratelimit)
            {
                if (this.NameToType(pair.Key) != -1 && pair.Value.Count == 2)
                {
                    playerRatelimit.Add(pair.Key, new EntityRatelimit(pair.Value[0], pair.Value[1]));
                }
            }
        }

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
                int entity_type = entity.Type;
                string entity_type_str = this.TypeToName(entity_type);

                if (entity_type_str != null)
                {
                    if (config.entities_ratelimit.TryGetValue(entity_type_str, out _))
                    {
                        Dictionary<string, EntityRatelimit> playerRatelimit = sender.GetData("entities_ratelimit");
                        EntityRatelimit entityRatelimit = playerRatelimit[entity_type_str];

                        if (entityRatelimit.AddAndCheck())
                        {
                            sender.Ban(utils.Format(locale.entity_ratelimit, entityRatelimit.GetCount().ToString(), entity_type_str, entityRatelimit.time.ToString()));
                        }
                    }
                }
            }
            
        }

        new public void init()
        {
            utils = this.instances.Get("utils");
            players = this.instances.Get("players");
            config = this.instances.Get("config").Get();
            locale = this.instances.Get("locales").Get();
        }
    }
}
