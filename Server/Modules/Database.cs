using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using MySql.Data.MySqlClient;

namespace Geminis.Server.Modules
{
    class Database : InterconnectClient
    {
        private Output output;
        private Locale locale;

        private string mysql_string;

        new public void init()
        {
            output = this.instances.Get("output");
            locale = this.instances.Get("locales").GetLocale();

            mysql_string = API.GetConvar("mysql_connection_string", null);

            if (mysql_string == null)
            {
                output.Print(Output.ERROR, locale.mysql_not_present);
            }

            MySqlConnection conn = this.CreateConnection();

            if (conn == null)
            {
                output.Print(Output.FATAL, locale.mysql_connection_error);
                Environment.Exit(1);
            }

            try
            {
                MySqlCommand query = conn.CreateCommand();
                query.CommandText = @"CREATE TABLE IF NOT EXISTS `geminis_bans` (
                  `id` int(12) NOT NULL AUTO_INCREMENT,
                  `name` varchar(200) DEFAULT NULL,
                  `steam` varchar(15) DEFAULT NULL,
                  `license` varchar(40) DEFAULT NULL,
                  `discord` varchar(18) DEFAULT NULL,
                  `fivem` varchar(10) DEFAULT NULL,
                  `ip` varchar(17) DEFAULT NULL,
                  `reason` mediumtext NOT NULL,
                  `applied` int(11) NOT NULL,
                  PRIMARY KEY (`id`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                query.ExecuteNonQuery();
            } catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                output.Print(Output.FATAL, locale.mysql_connection_error);
                Environment.Exit(1);
            }

            conn.Close();
        }

        public MySqlConnection CreateConnection()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(mysql_string);
                conn.Open();
                return conn;
            } catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
