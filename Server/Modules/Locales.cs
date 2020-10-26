using System;

using Newtonsoft.Json;
using CitizenFX.Core.Native;

namespace Geminis.Server.Modules
{
    class Locale
    {
        public string booting { get; set; }

        public string mysql_not_present { get; set; }
        public string mysql_connection_error { get; set; }

        public string not_loaded { get; set; }

        public string player_connecting { get; set; }
        public string player_not_using_steam { get; set; }
        public string ban_trigger { get; set; }
        public string player_banned { get; set; }
        public string player_loaded { get; set; }

        public string defer_loading { get; set; }
        public string steam_not_found { get; set; }
        public string ban_message { get; set; }

        public string no_heartbeat { get; set; }
        public string wrong_heartbeat { get; set; }
        public string stop_not_allowed { get; set; }
        public string unknown_resource { get; set; }
        public string modified_resource { get; set; }

        public string entity_ratelimit { get; set; }
    }

    class Locales : InterconnectClient
    {
        Locale locale;
        public Config config;
        public Output output;
        public Utils utils;

        new public void init()
        {
            config = this.instances.Get("config").GetConfig();
            output = this.instances.Get("output");
            utils = this.instances.Get("utils");

            string rawLocale = API.LoadResourceFile(API.GetCurrentResourceName(), $"Locales/{config.locale}.json");

            if (rawLocale == null)
            {
                output.Print(Output.FATAL, $"Couldn't load '{config.locale}' locale from Locales folder. Check if the locale exists");
                Environment.Exit(1);
            }

            locale = JsonConvert.DeserializeObject<Locale>(rawLocale);

            if (utils.IsAnyKeyOfClassNull(locale))
            {
                output.Print(Output.FATAL, "Missing variables on the locale. Maybe your locale is outdated");
                Environment.Exit(1);
            }

            output.Print(Output.INFO, locale.booting);
        }

        public Locale GetLocale()
        {
            return locale;
        }
    }
}