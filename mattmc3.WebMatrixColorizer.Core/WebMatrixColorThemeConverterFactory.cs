#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;
using System.Xml.Linq;

namespace mattmc3.WebMatrixColorizer {
	public static class WebMatrixColorThemeConverterFactory {
		public static IWebMatrixColorThemeConverter Create(XDocument vssettings) {
			// change me...
			IColorSettingConverter converter = new WebMatrix2ColorSettingConverter(vssettings);
			IWebMatrixColorThemeConverter themeConverter = new WebMatrix2ColorThemeConverter(converter);
			return themeConverter;
		}
	}
}
