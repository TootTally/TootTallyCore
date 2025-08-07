using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
