using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;
using MySql.Data.MySqlClient;

namespace Geminis.Server.Modules
{
    class GeminisPlayer
    {
        private Player cfxPlayer;

        private Dictionary<string, string> Identifiers = new Dictionary<string, string> { };
        private Dictionary<string, dynamic> Data = new Dictionary<string, dynamic> { };
        public string Name;
        public string Endpoint;

        public GeminisPlayer(Player cfxPlayer)
        {
            this.Name = cfxPlayer.Name;
            this.Endpoint = cfxPlayer.EndPoint;

            List<string> raw_identifiers = cfxPlayer.Identifiers.ToList();

            for (int i = 0; i < raw_identifiers.Count; i++)
            {
                string identifier = raw_identifiers[i];
                string[] pieces = identifier.Split(':');

                if (pieces.Length < 2)
                {
                    continue;
                }

                this.Identifiers.Add(pieces[0], pieces[1]);
            }

            this.cfxPlayer = cfxPlayer;
        }

        public void SetData(string key, dynamic value)
        {
            Data[key] = value;
        }

        public dynamic GetData(string key)
        {
            if (Data.TryGetValue(key, out _))
            {
                return Data[key];
            }

            return null;
        }

        public string GetIdentifier(string type)
        {
            if (this.Identifiers.TryGetValue(type, out _))
            {
                return $"{type}:{this.Identifiers[type]}";
            }

            return null;
        }

        public List<string> GetIdentifiers()
        {
            List<string> identifiers = new List<string> { };

            foreach (KeyValuePair<string, string> pair in this.Identifiers)
            {
                identifiers.Add($"{pair.Key}:{pair.Value}");
            }

            return identifiers;
        }

        public string GetShortIdentifier(string type)
        {
            if (this.Identifiers.TryGetValue(type, out _)) {
                return this.Identifiers[type];
            }

            return null;
        }

        public string GetShortIdentifier(string type, dynamic def)
        {
            if (this.Identifiers.TryGetValue(type, out _))
            {
                return this.Identifiers[type];
            }

            return def;
        }

        public int GetPing()
        {
            return this.cfxPlayer.Ping;
        }
    
        public void Drop(string reason)
        {
            this.cfxPlayer.Drop(reason);
        }

        public void Ban(string reason)
        {
            BaseScript.TriggerEvent("geminis:server_trigger_ban", this.cfxPlayer.Handle, reason);
        }

    }

    class Players : InterconnectClient
    {
        private Utils utils;
        private Output output;
        private Config config;
        private Locale locale;
        private Database database;

        private ConcurrentDictionary<Player, GeminisPlayer> players = new ConcurrentDictionary<Player, GeminisPlayer> {};
        private Dictionary<string, Tuple<string, string>> cache;

        new public void init()
        {
            utils = this.instances.Get("utils");
            output = this.instances.Get("output");
            config = this.instances.Get("config").GetConfig();
            locale = this.instances.Get("locales").GetLocale();
            database = this.instances.Get("database");

            this.internal_data.Add("cache", new Dictionary<string, Tuple<string, string>>());
            cache = this.internal_data["cache"];
        }

        public ConcurrentDictionary<Player, GeminisPlayer> GetPlayers()
        {
            return players;
        }

        public GeminisPlayer GetPlayerFromSource(Player source)
        {
            if (players.TryGetValue(source, out _))
            {
                return players[source];
            }

            return null;
        }

        public GeminisPlayer GetPlayerFromId(string id)
        {
            foreach (KeyValuePair<Player, GeminisPlayer> player in this.GetPlayers())
            {
                if (player.Key.Handle == id)
                {
                    return player.Value;
                }
            }

            return null;
        }

        public GeminisPlayer GetPlayerFromIdentifier(string identifier)
        {
            foreach (KeyValuePair<Player, GeminisPlayer> pair in players)
            {
                List<string> identifiers = pair.Value.GetIdentifiers();

                for (int i = 0; i < identifiers.Count; i++)
                {
                    if (identifiers[i] == identifier)
                    {
                        return pair.Value;
                    }
                }
            }

            return null;
        }

        public async void OnPlayerConnecting([FromSource] Player source, string name, dynamic kick, dynamic deferreals)
        {
            GeminisPlayer geminisPlayer = new GeminisPlayer(source);

            output.Print(Output.INFO, utils.Format(locale.player_connecting, name, geminisPlayer.Endpoint));

            deferreals.defer();

            await BaseScript.Delay(0);

            deferreals.update(utils.Format(locale.defer_loading, name));

            if (config.use_steam)
            {
                if (geminisPlayer.GetIdentifier("steam") == null)
                {
                    deferreals.done(locale.steam_not_found);

                    output.Print(Output.WARNING, utils.Format(locale.player_not_using_steam, name, geminisPlayer.GetIdentifier("license")));
                    return;
                }
            }

            if (cache.TryGetValue(geminisPlayer.GetShortIdentifier("license"), out _))
            {
                Tuple<string, string> ban = cache[geminisPlayer.GetShortIdentifier("license")];
                deferreals.done(utils.Format(locale.ban_message, ban.Item1, ban.Item2));

                output.Print(Output.WARNING, utils.Format(locale.player_banned, name, ban.Item2));
                return;
            }

            MySqlConnection conn = database.CreateConnection();
            MySqlCommand command = conn.CreateCommand();

            command.CommandText = "SELECT reason, applied FROM geminis_bans WHERE license = @license OR steam = @steam OR discord = @discord OR fivem = @fivem OR ip = @ip;";
            command.Parameters.AddWithValue("@license", geminisPlayer.GetShortIdentifier("license"));
            command.Parameters.AddWithValue("@steam", geminisPlayer.GetShortIdentifier("steam"));
            command.Parameters.AddWithValue("@discord", geminisPlayer.GetShortIdentifier("discord"));
            command.Parameters.AddWithValue("@fivem", geminisPlayer.GetShortIdentifier("fivem"));
            command.Parameters.AddWithValue("@ip", geminisPlayer.Endpoint);
            
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string reason = reader.GetString(0);
                    int timestamp = reader.GetInt32(1);

                    DateTime date = utils.GetDateTimeFromTimestamp(timestamp);
                    string str_date = $"{date.Day}/{date.Month}/{date.Year} - {date.Hour}:{date.Minute}";

                    deferreals.done(utils.Format(locale.ban_message, reason, str_date));
                    cache.Add(geminisPlayer.GetShortIdentifier("license"), new Tuple<string, string>(reason, str_date));

                    output.Print(Output.WARNING, utils.Format(locale.player_banned, name, reason));

                    return;
                }
            }

            deferreals.done();
        }
    
        public async void OnPlayerDropped([FromSource] Player source)
        {
            players.TryRemove(source, out _);
        }

        public async void OnClientResourceStart([FromSource] Player source)
        {
            players[source] = new GeminisPlayer(source);

            output.Print(Output.DONE, utils.Format(locale.player_loaded, source.Name));
        }
    
        public async void OnServerBanTrigger([FromSource] Player source, string player_id, string reason)
        {
            if (source != null)
            {
                return;
            }

            GeminisPlayer geminisPlayer = this.GetPlayerFromId(player_id);

            MySqlConnection conn = database.CreateConnection();
            MySqlCommand command = conn.CreateCommand();

            command.CommandText = "INSERT INTO geminis_bans (name, steam, license, discord, fivem, ip, reason, applied) VALUES (@name, @steam, @license, @discord, @fivem, @ip, @reason, @applied);";

            command.Parameters.AddWithValue("@name", geminisPlayer.Name);
            command.Parameters.AddWithValue("@steam", geminisPlayer.GetShortIdentifier("steam", ""));
            command.Parameters.AddWithValue("@license", geminisPlayer.GetShortIdentifier("license", ""));
            command.Parameters.AddWithValue("@discord", geminisPlayer.GetShortIdentifier("discord", ""));
            command.Parameters.AddWithValue("@fivem", geminisPlayer.GetShortIdentifier("fivem", ""));
            command.Parameters.AddWithValue("@ip", geminisPlayer.Endpoint);
            command.Parameters.AddWithValue("@reason", reason);
            command.Parameters.AddWithValue("@applied", utils.GetTimestamp());
            command.Prepare();

            command.ExecuteNonQuery();
            conn.Close();

            output.Print(Output.ERROR, utils.Format(locale.ban_trigger, geminisPlayer.Name, reason));

            geminisPlayer.Drop(reason);
        }
    }
}
