using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyCore.Utils.Helpers
{
    public static class TootTallyUtils
    {
        public static string FormatFromSeconds(float totalSeconds)
        {
            totalSeconds = (uint)totalSeconds;
            var dispSeconds = totalSeconds % 60;

            var totalMinutes = totalSeconds / 60;
            var dispMinutes = totalMinutes % 60;

            var dispHours = totalMinutes / 60;

            string formatedTime = "";
            if (dispHours > 0)
                formatedTime = $"{dispHours}:";

            formatedTime += $"{dispMinutes}:{dispSeconds:D2}";
            return formatedTime;
        }

        /* Gets the build date information of the referred assembly.
         * Code from https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
         * Assumes that the prefix is just "+"
         * Assumes that the version format is a.b.c+yyyyMMdd
         */
        public static int GetBuildDate(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute?.InformationalVersion == null)
                return 0;

            var value = attribute.InformationalVersion;
            var index = value.IndexOf("+");

            if (index == 0)
                return 0;

            value = value.Substring(index + 1);

            if (int.TryParse(value, out int result))
                return result;

            return 0;
        }
    }
}
