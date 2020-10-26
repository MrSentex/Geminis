using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Geminis.Server.Modules
{
    class Utils : InterconnectClient
    {
        private UTF8Encoding encoder = new UTF8Encoding();
        private Output output;

        new public void init()
        {
            output = this.instances.Get("output");
        }

        public bool IsAnyKeyOfClassNull(object target_class)
        {
            Type type = target_class.GetType();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                var value = prop.GetValue(target_class, null);

                if (value == null)
                {
                    output.Print(Output.ERROR, $"Missing {prop} in class {type}");
                    return true;
                }
            }

            return false;
        }

        public string GetMacAddress()
        {
            foreach (NetworkInterface it in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (it.OperationalStatus == OperationalStatus.Up)
                {
                    string description = it.Description;

                    if (!description.Contains("VirtualBox") && !description.Contains("VMWare") && !description.Contains("VPN"))
                    {
                        return it.GetPhysicalAddress().ToString();
                    }
                }
            }

            return null;
        }

        public string Format(string str, params string[] format)
        {
            for (int i = 0; i < format.Length; i++)
            {
                int start = str.IndexOf("{");
                int stop = str.IndexOf("}");

                if (start < 0 || stop < 0)
                {
                    continue;
                }

                str = str.Remove(start, (stop - start) + 1).Insert(start, format[i]);
            }

            return str;
        }

        public DateTime GetDateTimeFromTimestamp(int timestamp)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            date = date.AddSeconds(timestamp).ToLocalTime();

            return date;
        }

        public int GetTimestamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

    }
}
