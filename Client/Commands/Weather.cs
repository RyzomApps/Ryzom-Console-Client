using API;
using API.Chat;
using API.Commands;
using API.Helper;
using System;
using System.Collections.Generic;

namespace Client.Commands
{
    /// <summary>
    /// Returns the weather for a continent, e.g.
    /// "Fyros, It's Autumn and the weather is Fair, 39% Humidity".
    /// Without an argument it lists the known continents.
    /// </summary>
    public class Weather : CommandBase
    {
        public override string CmdName => "weather";

        public override string CmdUsage => "[<continent>]";

        public override string CmdDesc => "Gets the current weather on a continent";

        static readonly Dictionary<string, WeatherTable[]> Continents = new(StringComparer.OrdinalIgnoreCase)
        {
            ["fyros"] = RyzomWeatherTables.Fyros,
            ["fyrosisland"] = RyzomWeatherTables.FyrosIsland,
            ["fyrosnewbie"] = RyzomWeatherTables.FyrosNewbie,
            ["tryker"] = RyzomWeatherTables.Tryker,
            ["trykerisland"] = RyzomWeatherTables.TrykerIsland,
            ["trykernewbie"] = RyzomWeatherTables.TrykerNewbie,
            ["matisisland"] = RyzomWeatherTables.MatisIsland,
            ["matisnewbie"] = RyzomWeatherTables.MatisNewbie,
            ["zoraiisland"] = RyzomWeatherTables.ZoraiIsland,
            ["zorainewbie"] = RyzomWeatherTables.ZoraiNewbie,
            ["r2desert"] = RyzomWeatherTables.R2Desert,
            ["r2forest"] = RyzomWeatherTables.R2Forest,
            ["r2jungle"] = RyzomWeatherTables.R2Jungle,
            ["r2lakes"] = RyzomWeatherTables.R2Lakes,
            ["r2roots"] = RyzomWeatherTables.R2Roots,
            ["newbieland"] = RyzomWeatherTables.Newbieland,
            ["routegouffre"] = RyzomWeatherTables.RouteGouffre,
            ["bagne"] = RyzomWeatherTables.Bagne,
            ["lecarrefour"] = RyzomWeatherTables.Lecarrefour,
            ["lepaysmalade"] = RyzomWeatherTables.Lepaysmalade,
            ["lesfalaises"] = RyzomWeatherTables.Lesfalaises,
            ["lesilesvivantes"] = RyzomWeatherTables.Lesilesvivantes,
            ["sources"] = RyzomWeatherTables.Sources,
            ["terre"] = RyzomWeatherTables.Terre,
            ["testroom"] = RyzomWeatherTables.Testroom,
            ["corruptedmoor"] = RyzomWeatherTables.CorruptedMoor,
            ["kitiniere"] = RyzomWeatherTables.Kitiniere,
        };

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            var parts = (command ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var arg = parts.Length > 1 ? parts[1] : "";

            if (string.IsNullOrEmpty(arg))
            {
                var names = new string[Continents.Count];
                Continents.Keys.CopyTo(names, 0);
                Array.Sort(names, StringComparer.OrdinalIgnoreCase);
                responseMsg = $"{ChatColor.YELLOW}Usage: {CmdName} <continent> ({string.Join(", ", names)})";
                return false;
            }

            if (!Continents.TryGetValue(arg.Trim(), out var tables))
            {
                var names = new string[Continents.Count];
                Continents.Keys.CopyTo(names, 0);
                Array.Sort(names, StringComparer.OrdinalIgnoreCase);
                responseMsg = $"{ChatColor.RED}Unknown continent '{arg}'. Known: {string.Join(", ", names)}";
                return false;
            }

            var tick = handler.GetApiNetworkManager().GetCurrentServerTick();
            responseMsg = $"{ChatColor.GOLD}{arg}, {RyzomWeatherEngine.GetWeatherString(tick, tables)}";
            return false;
        }
    }
}
