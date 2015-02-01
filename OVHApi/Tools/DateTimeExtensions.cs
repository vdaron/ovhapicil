
namespace OVHApi.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DateTimeExtensions
    {
        public const long UnixEpoch = 621355968000000000L;

        public const long TicksPerMs = TimeSpan.TicksPerSecond / 1000;

        public static long ToUnixTime(this DateTime dateTime)
        {
            var epoch = (dateTime.ToUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerSecond;
            return epoch;
        }
    }
}
