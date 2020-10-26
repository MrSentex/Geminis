using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Geminis.Server.Modules;

namespace Geminis.Server
{
    public class Main : BaseScript
    {
        InterconnectServer interconnectServer = new InterconnectServer();

        private Players players;
        private ConfigLoader config;
        private Heartbeat heartbeat;
        private Resources resources;
        private Events events;
        private Entities entities;

        public Main()
        {
            interconnectServer.Add("utils", new Utils());
            interconnectServer.Add("output", new Output());
            interconnectServer.Add("config", new ConfigLoader());
            interconnectServer.Add("locales", new Locales());
            interconnectServer.Add("database", new Database());
            interconnectServer.Add("heartbeat", new Heartbeat());
            interconnectServer.Add("resources", new Resources());
            interconnectServer.Add("events", new Events());
            interconnectServer.Add("entities", new Entities());
            interconnectServer.Add("players", new Players());

            config = interconnectServer.Get("config");
            heartbeat = interconnectServer.Get("heartbeat");
            resources = interconnectServer.Get("resources");
            events = interconnectServer.Get("events");
            entities = interconnectServer.Get("entities");
            players = interconnectServer.Get("players");

            EventHandlers["onResourceStart"] += new Action<string>(resources.OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(resources.OnResourceStop);

            interconnectServer.Initialize();

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(players.OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player>(players.OnPlayerDropped);

            EventHandlers["gameEventTriggered"] += new Action<Player, string, List<dynamic>>(events.OnEvent);

            EventHandlers["geminis:client_start_resource"] += new Action<Player>(players.OnClientResourceStart);
            EventHandlers["geminis:client_start_resource"] += new Action<Player>(heartbeat.OnClientResourceStart);

            EventHandlers["geminis:check_resources"] += new Action<Player, List<dynamic>>(resources.OnClientResourcesCheck);
            EventHandlers["geminis:unknown_resource"] += new Action<Player, string>(resources.OnClientUnknownResource);
            EventHandlers["geminis:geminis:modified_resource"] += new Action<Player, string>(resources.OnClientModifiedResource);
            EventHandlers["geminis:client_stop_resource"] += new Action<Player, string>(resources.OnClientResourceStop);

            EventHandlers["geminis:heartbeat"] += new Action<Player, string>(heartbeat.OnClientHeartbeat);
            EventHandlers["geminis:server_trigger_ban"] += new Action<Player, string, string>(players.OnServerBanTrigger);

            EventHandlers["geminis:set_client_config"] += new Action<Player>(config.OnClientRequest);
            EventHandlers["geminis:debug"] += new Action<Player, string>(OnDebug);

            EventHandlers["geminis:on_player_connect"] += new Action<GeminisPlayer>(entities.OnPlayerConnecting);
            EventHandlers["entityCreating"] += new Action<int>(entities.OnEntityCreating);
        }

        public async void OnDebug([FromSource] Player source, string message)
        {
            Debug.WriteLine($"{source.Name} | {message}");
        }
    }
}