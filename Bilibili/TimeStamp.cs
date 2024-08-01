using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
