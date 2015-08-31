#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;
using System.Xml.Linq;

namespace mattmc3.WebMatrixColorizer {
	public interface IWebMatrixColorThemeConverter {
		XDocument ConvertToWebMatrixColorTheme();
	}
}
