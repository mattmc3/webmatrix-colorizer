#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using mattmc3.Common;

namespace mattmc3.WebMatrixColorizer {

	public class WebMatrix2ColorThemeConverter : IWebMatrixColorThemeConverter {
		private IColorSettingConverter _colorSchemeConverter;

		public WebMatrix2ColorThemeConverter(IColorSettingConverter colorSchemeConverter) {
			_colorSchemeConverter = colorSchemeConverter;
		}

		public XDocument ConvertToWebMatrixColorTheme() {
			var webmatrixDefaultThemeXml = ResourceFile.ReadAllText("mattmc3.WebMatrixColorizer.WebMatrix2DefaultTheme.xml");
			var templateXml = XDocument.Parse(webmatrixDefaultThemeXml);
			var resultXml = XDocument.Parse(webmatrixDefaultThemeXml);
			var surfaceBackground = resultXml.Descendants("SurfaceBackground").First();
			var surfaceBackgroundParent = surfaceBackground.Parent;
			surfaceBackground.Remove();
			surfaceBackground.SetAttributeValue("Background", _colorSchemeConverter.GetSurfaceBackground());
			surfaceBackgroundParent.AddFirst(surfaceBackground);

			var classifications = resultXml.Descendants("Classifications").First();
			classifications.RemoveAll();

			foreach (XElement templateItem in templateXml.Descendants("Classifications").First().Elements("Item")) {
				var templateItemName = templateItem.Attribute("Name").Value;
				var setting = _colorSchemeConverter.GetColorSetting(templateItemName);
				if (setting != null) {
					var resultItem = (
						from x in templateXml.Descendants("Classifications").First().Elements("Item")
						where x.Attribute("Name").Value == templateItemName
						select x).First();

					resultItem.SetAttributeValue("Foreground", setting.Foreground);
					resultItem.SetAttributeValue("Background", setting.Background);
					resultItem.SetAttributeValue("BoldFont", setting.BoldFont);
					classifications.Add(resultItem);
				}
				else {
					classifications.Add(templateItem);
				}
			}
			return resultXml;
		}
	}
}
