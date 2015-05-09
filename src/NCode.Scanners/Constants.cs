using System.Collections.Generic;

namespace NCode.Scanners
{
	internal static class Constants
	{
		public static IEnumerable<string> DefaultSearchPatterns
		{
			get { return new[] { "*.dll", "*.exe" }; }
		}
	}
}