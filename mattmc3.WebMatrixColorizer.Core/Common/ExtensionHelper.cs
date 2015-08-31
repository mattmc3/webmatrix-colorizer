using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mattmc3.Common {

	public static class ExtensionHelper {
		public static IEnumerable<T> AsEnumerable<T>(this T x) {
			yield return x;
		}

		public static string FormatWith(this string format, params object[] args) {
			return String.Format(format, args);
		}

		public static string FormatWith(this string format, IFormatProvider provider, params object[] args) {
			return String.Format(provider, format, args);
		}
	}
}
