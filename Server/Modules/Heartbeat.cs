using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using CitizenFX.Core;

namespace Geminis.Server.Modules
{
    class Heartbeat : InterconnectClient
    {
        private Random random = new Random();
        private MD5 md5 = MD5.Create();

        private Utils utils;
        private Locale locale;
        private Players players;

        new public void init()
        {
            utils = this.instances.Get("utils");
            locale = this.instances.Get("locales").GetLocale();
            players = this.instances.Get("players");
        }

        new public async void run_async()
        {
            while (true)
            {
                await BaseScript.Delay(5000);

                foreach (KeyValuePair<Player, GeminisPlayer> player in players.GetPlayers())
                {
                    if (player.Value.GetData("heartbeat_identifier") == null || player.Value.GetData("heartbeat") == null)
                    {
                        player.Key.Drop("Unable to load correctly the resource (Geminis)");
                        continue;
                    }

                    if (utils.GetTimestamp() - (int)player.Value.GetData("heartbeat") > 5)
                    {
                        if (player.Value.GetPing() != -1)
                        {
                            player.Value.Drop(utils.Format(locale.no_heartbeat, (utils.GetTimestamp() - (int)player.Value.GetData("heartbeat")).ToString()));
                            continue;
                        }
                    }
                }
            }
        }

        public string GenerateIdentifier()
        {
            byte[] r_bytes = new byte[20];

            for (int i = 0; i < r_bytes.Length; i++)
            {
                random.NextBytes(r_bytes);
            }

            byte[] h_bytes = md5.ComputeHash(r_bytes);

            return BitConverter.ToString(h_bytes).Replace("-", String.Empty).ToLower();
        }

        public async void OnClientResourceStart([FromSource] Player source)
        {
            GeminisPlayer player = players.GetPlayerFromSource(source);
            string identifier = this.GenerateIdentifier();

            player.SetData("heartbeat_identifier", identifier);
            player.SetData("heartbeat", utils.GetTimestamp());

            source.TriggerEvent("geminis:set_identifier", identifier);
        }

        public async void OnClientHeartbeat([FromSource] Player source, string identifier)
        {
            GeminisPlayer geminisPlayer = players.GetPlayerFromSource(source);

            if (geminisPlayer == null) { return; }

            if (geminisPlayer.GetData("heartbeat_identifier") != identifier)
            {
                geminisPlayer.Drop(locale.wrong_heartbeat);
                return;
            }

            geminisPlayer.SetData("heartbeat", utils.GetTimestamp());
        }

    }
}

