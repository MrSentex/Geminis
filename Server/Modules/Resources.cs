using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Geminis.Server.Modules
{
    class Resources : InterconnectClient
    {
        private List<string> resources = new List<string>() ;
        private List<Tuple<string, int>> restarts = new List<Tuple<string, int>>();

        private Config config;
        private Utils utils;
        private Locale locale;
        private Players players;

        new public void init()
        {
            config = this.instances.Get("config").GetConfig();
            utils = this.instances.Get("utils");
            locale = this.instances.Get("locales").GetLocale();
            players = this.instances.Get("players");

            for (int i = 0; i < API.GetNumResources(); i++)
            {
                string resourceName = API.GetResourceByFindIndex(i);

                if (API.GetResourceState(resourceName) == "started")
                {
                    resources.Add(resourceName);
                }
            }

        }

        public bool IsStarted(string resourceName)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i] == resourceName)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsRecentlyRestarted(string resourceName)
        {
            for (int i = 0; i < restarts.Count; i++)
            {
                Tuple<string, int> resource = restarts[i];

                if (resource.Item1 == resourceName && resource.Item2 >= 1)
                {
                    restarts[i] = new Tuple<string, int>(resourceName, resource.Item2);
                    return true;
                }

            }

            return false;
        }

        public async void OnResourceStarting(string resourceName)
        {
            ConcurrentDictionary<Player, GeminisPlayer> active_players = players.GetPlayers();

            if (active_players.Count >= 1)
            {
                foreach (KeyValuePair<Player, GeminisPlayer> player in active_players)
                {
                    Player cfxPlayer = player.Key;

                    cfxPlayer.TriggerEvent("geminis:new_resource", resourceName);
                }
            }
        }

        public async void OnResourceStart(string resourceName)
        {
            if (!IsStarted(resourceName))
            {
                resources.Add(resourceName);
            }
        }

        public async void OnResourceStop(string resourceName)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i] == resourceName)
                {
                    resources.RemoveAt(i);
                }
            }

            if (API.GetResourceState(resourceName) == "started")
            {
                restarts.Add(new Tuple<string, int>(resourceName, players.GetPlayers().Count));
            }
        }

        public async void OnClientResourcesCheck([FromSource] Player source, List<dynamic> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (!IsStarted(resources[i]))
                {
                    GeminisPlayer geminisPlayer = players.GetPlayerFromSource(source);
                    geminisPlayer.Ban(utils.Format(locale.unknown_resource, resources[i]));
                    break;
                }
            }
        }

        public async void OnClientModifiedResource([FromSource] Player source, string resourceName)
        {
            GeminisPlayer geminisPlayer = players.GetPlayerFromSource(source);
            geminisPlayer.Ban(utils.Format(locale.modified_resource, resourceName));
        }

        public async void OnClientUnknownResource([FromSource] Player source, string resourceName)
        {
            if (!IsStarted(resourceName) && !IsRecentlyRestarted(resourceName))
            {
                GeminisPlayer geminisPlayer = players.GetPlayerFromSource(source);
                geminisPlayer.Ban(utils.Format(locale.unknown_resource, resourceName));
            }
        }

        public async void OnClientResourceStop([FromSource] Player source, string resourceName)
        {
            if (IsStarted(resourceName) && !IsRecentlyRestarted(resourceName)) {
                GeminisPlayer geminisPlayer = players.GetPlayerFromSource(source);

                geminisPlayer.Ban(utils.Format(locale.stop_not_allowed, resourceName));
            }
        }
    }
}
