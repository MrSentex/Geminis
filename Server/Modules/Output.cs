using System;

using CitizenFX.Core;

namespace Geminis.Server.Modules
{
    class Colors
    {
        public static string Reset = "^0";
        public static string Red = "^1";
        public static string Green = "^2";
        public static string Yellow = "^3";
        public static string DarkBlue = "^4";
        public static string Blue = "^5";
        public static string Violet = "^6";
        public static string White = "^7";
        public static string BloodRed = "^8";
        public static string Fucshia = "^9";

        public static string Bold = "^*";
        public static string Underline = "^_";
        public static string Strikethrougth = "^~";
        public static string Cancel = "^r";

        public static string Random()
        {
            return "^" + new Random().Next(1, 6).ToString();
        }
    }

    class Output : InterconnectClient
    {
        public static string INFO = "[" + Colors.Blue + "#" + Colors.Reset + "]";
        public static string DONE = "[" + Colors.Green + "+" + Colors.Reset + "]";
        public static string ERROR = "[" + Colors.Red + "-" + Colors.Reset + "]";
        public static string WARNING = "[" + Colors.Yellow + "!" + Colors.Reset + "]";
        public static string FATAL = "[" + Colors.BloodRed + "FATAL" + Colors.Reset + "]";

        public void Print(params string[] args)
        {
            string line = Colors.Random() + "Geminis" + Colors.Reset + " - " + Colors.Reset + String.Join(" ", args);
            Debug.WriteLine(line);
        }
    }
}
