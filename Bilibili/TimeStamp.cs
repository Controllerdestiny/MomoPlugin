namespace Bilibili
{
    internal static class TimeStampExtensions
    {
        public static DateTime ToDateTime(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
        }
    }
}
