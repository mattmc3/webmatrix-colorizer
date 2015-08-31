#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using mattmc3.Common;

namespace mattmc3.WebMatrixColorizer {

	public class WebMatrix2ColorSettingConverter : IColorSettingConverter {
		#region Fields/Properties
		/// <summary>
		/// .vssettings files use this color representation to indicate that no color was specified
		/// and to fall back to the VS default for that code element
		/// </summary>
		private const string VSDefaultColor = "0x02000000";

		/// <summary>
		/// WebMatrix default foreground color
		/// </summary>
		private string _defaultForeground = "00000000";

		/// <summary>
		/// WebMatrix default background color
		/// </summary>
		private readonly string _defaultBackground = "00FFFFFF";

		/// <summary>
		/// WebMatrix surface background color for the color theme
		/// </summary>
		private string _surfaceBackground = "00FFFFFF";

		private XDocument _vssettings;
		private Dictionary<string, Func<WebMatrixColorSetting>> _mappings;
		#endregion

		#region Constructors
		public WebMatrix2ColorSettingConverter(XDocument vssettings) {
			_vssettings = vssettings;
			Initialize();
		}
		#endregion

		public string GetSurfaceBackground() {
			return _surfaceBackground;
		}

		public WebMatrixColorSetting GetColorSetting(string webMatrixSettingName) {
			if (_mappings.ContainsKey(webMatrixSettingName) == false) return null;
			var func = _mappings[webMatrixSettingName];
			if (func == null) return null;
			var setting = func();
			return setting;
		}

		#region Methods
		private void Initialize() {
			// initialize the instructions for how mapping will happen from vssettings files to webmatrix
			_mappings = GetMappings();

			// it's important to have our defaults set properly before we begin
			var settings = GetVSColorSetting("Plain Text")();
			if (settings != null) {
				_surfaceBackground = settings.Background;
				_defaultForeground = settings.Foreground;
			}
		}

		#region Color Picker functions
		private Func<WebMatrixColorSetting> GetVSColorSetting(string vsSettingName, params string[] fallbackSettings) {
			return GetVSColorSetting(false, vsSettingName, fallbackSettings);
		}

		private Func<WebMatrixColorSetting> GetVSColorSetting(bool useExplicitBackground, string vsSettingName, params string[] fallbackSettings) {
			// useExplicitBackground: some settings can't properly inherit a background color when one isn't
			// specified.  Specifically dark themes.  So these settings should have their background
			// always explicitly set for the color scheme to look right.
			
			// this function returns a function to get the VS Setting specified
			Func<WebMatrixColorSetting> result = () => {
				var allSettings = vsSettingName.AsEnumerable().Union(fallbackSettings);
				WebMatrixColorSetting setting = null;
				foreach (string curVssetting in allSettings) {
					setting = (
						from a in _vssettings.Descendants("Items").First().Elements("Item")
						where a.Attribute("Name").Value == curVssetting
						select new WebMatrixColorSetting() {
							Background = GetBackgroundColor(useExplicitBackground, a.Attribute("Background").Value),
							Foreground = GetForegroundColor(a.Attribute("Foreground").Value),
							BoldFont = a.Attribute("BoldFont").Value
						}).FirstOrDefault();

					if (setting == null) {
						Debug.WriteLine("VSSetting not found: {0}".FormatWith(curVssetting));
					}

					if (setting != null) break;
				}
				return setting;
			};
			return result;
		}

		private Func<WebMatrixColorSetting> GetColorFromOtherWebMatrixSetting(string webMatrixSettingName) {
			// return a function that looks at the result of another WebMatrix mapping to get its result
			Func<WebMatrixColorSetting> result = () => {
				var setting = GetColorSetting(webMatrixSettingName);
				if (setting == null) throw new ArgumentException("Could not find Web Matrix color setting: {0}".FormatWith(webMatrixSettingName));
				return setting;
			};
			return result;
		}
		#endregion

		/// <summary>
		/// Gets a WebMatrix background color based on the vscolor specfied.  If one cannot
		/// be determined, the surface background or default see-through background is used.
		/// </summary>
		private string GetBackgroundColor(bool useExplicit, string vscolor) {
			var fallbackBackground = (useExplicit ? _surfaceBackground : _defaultBackground);
			if (vscolor == VSDefaultColor) return fallbackBackground;
			var result = ConvertVSColorToWebMatrixColor(vscolor);
			if (result == _surfaceBackground && useExplicit == false) {
				// see through is fine for this setting so that we don't repeat
				// the surface background all throughout
				return _defaultBackground;
			}
			return result;
		}

		/// <summary>
		/// Gets a WebMatrix foreground color based on the vscolor specfied
		/// </summary>
		private string GetForegroundColor(string vscolor) {
			if (vscolor == VSDefaultColor) return _defaultForeground;
			return ConvertVSColorToWebMatrixColor(vscolor);
		}

		/// <summary>
		/// Converts a VS color string into a WebMatrix color representation
		/// </summary>
		private string ConvertVSColorToWebMatrixColor(string vscolor) {
			// vssettings files uses machine-readable little endian to store the color,
			// but webmatrix uses human readable big endian
			if (vscolor.StartsWith("0x") == false || vscolor.Length != 10) {
				throw new ArgumentException("Unexpected color format: {0}.  Expected format 0x00112233".FormatWith(vscolor));
			}
			var red = vscolor.Substring(8, 2);
			var green = vscolor.Substring(6, 2);
			var blue = vscolor.Substring(4, 2);
			var alpha = "FF";
			return alpha + red + green + blue;
		}

		/// <summary>
		/// Defines all the mappings for each WebMatrix setting and where
		/// each comes from in the .vssettings file
		/// </summary>
		private Dictionary<string, Func<WebMatrixColorSetting>> GetMappings() {
			// not sure if all of these are right.  Change as needed.

			var result = new Dictionary<string, Func<WebMatrixColorSetting>>();
			// WebMatrix -> vssetting
			result.Add("Default", GetVSColorSetting(true, "Plain Text"));
			result.Add("CssComment", GetVSColorSetting("CSS Comment", "Comment"));
			result.Add("CssString", GetVSColorSetting("CSS String Value", "String"));
			result.Add("CssNumber", GetVSColorSetting("CSS Property Value"));
			result.Add("CssItemName", GetVSColorSetting("CSS Property Name"));
			result.Add("CssImportant", GetVSColorSetting("CSS Keyword"));
			result.Add("CssItemNamespace", GetVSColorSetting("CSS Property Name"));
			result.Add("CssCurlyBrace", GetVSColorSetting("Plain Text"));
			result.Add("CssUnits", GetVSColorSetting("CSS Property Value"));
			result.Add("CssFunctionName", GetVSColorSetting("CSS Keyword"));
			result.Add("CssFunctionBrace", GetVSColorSetting("Plain Text"));
			result.Add("CssFunctionArgument", GetVSColorSetting("CSS Property Value"));
			result.Add("CssAtDirectiveName", GetVSColorSetting("CSS Keyword"));
			result.Add("CssCharsetName", GetVSColorSetting("CSS Keyword"));
			result.Add("CssImportUrl", GetVSColorSetting("urlformat"));
			result.Add("CssPropertyName", GetVSColorSetting("CSS Property Name"));
			result.Add("CssPropertyValue", GetVSColorSetting("CSS Property Value"));
			result.Add("CssUrlFunction", GetVSColorSetting("CSS Keyword"));
			result.Add("CssUrlString", GetVSColorSetting("urlformat"));
			result.Add("CssHexColor", GetVSColorSetting("CSS Property Value"));
			result.Add("CssElementTagName", GetVSColorSetting("CSS Property Name"));
			result.Add("CssElementAttribute", GetVSColorSetting("CSS Property Name"));
			result.Add("CssPseudoClass", GetVSColorSetting("CSS Property Name"));
			result.Add("CssPseudoElement", GetVSColorSetting("CSS Property Value"));
			result.Add("CssPseudoClassArgument", GetVSColorSetting("CSS Property Name"));
			result.Add("CssPseudoPageType", GetVSColorSetting("CSS Property Value"));
			result.Add("CssMediaQuery", GetVSColorSetting("CSS Property Name"));
			result.Add("CssMediaType", GetVSColorSetting("CSS Property Value"));
			result.Add("CssSelector", GetVSColorSetting("CSS Selector"));
			result.Add("CssSelectorOperator", GetVSColorSetting("CSS Selector"));
			result.Add("CssSquareBracket", GetVSColorSetting("Plain Text"));
			result.Add("CssClassSelector", GetVSColorSetting("CSS Selector"));
			result.Add("CssIdSelector", GetVSColorSetting("CSS Selector"));
			result.Add("CssUnicodeRange", GetVSColorSetting("CSS Property Value"));
			result.Add("CssCustomPropertyName", GetVSColorSetting("CSS Property Name"));
			result.Add("HtmlComment", GetVSColorSetting("HTML Comment", "Comment"));
			result.Add("HtmlString", GetVSColorSetting("HTML Attribute Value"));
			result.Add("HtmlElementName", GetVSColorSetting("HTML Element Name"));
			result.Add("HtmlElementPrefix", GetVSColorSetting("HTML Element Name"));
			result.Add("HtmlAttributeName", GetVSColorSetting("HTML Attribute"));
			result.Add("HtmlElementAttributePrefix", GetVSColorSetting("HTML Attribute"));
			result.Add("HtmlElementAttributeValue", GetVSColorSetting("HTML Attribute Value"));
			result.Add("HtmlEquals", GetVSColorSetting("HTML Operator"));
			result.Add("HtmlAngleBracket", GetVSColorSetting("HTML Tag Delimiter"));
			result.Add("HtmlEntity", GetVSColorSetting("HTML Entity"));
			result.Add("HtmlServerCodeBlockSeparator", GetVSColorSetting("HTML Server-Side Script"));
			result.Add("HtmlServerCodeBlockContent", GetVSColorSetting("HTML Server-Side Script"));
			result.Add("JScriptComment", GetVSColorSetting("Script Comment", "Comment"));
			result.Add("JScriptNumber", GetVSColorSetting("Script Number"));
			result.Add("JScriptString", GetVSColorSetting("Script String"));
			result.Add("JScriptKeyword", GetVSColorSetting("Script Keyword"));
			result.Add("JScriptFutureKeyword", GetVSColorSetting("Script Keyword"));
			result.Add("JScriptOperator", GetVSColorSetting("Script Operator"));
			result.Add("JScriptGlobalVariable", GetVSColorSetting("Script Identifier"));
			result.Add("JScriptGlobalConstructor", GetVSColorSetting("Script Identifier"));
			result.Add("PhpComment", GetColorFromOtherWebMatrixSetting("comment"));
			result.Add("PhpString", GetColorFromOtherWebMatrixSetting("string"));
			result.Add("PhpNumber", GetColorFromOtherWebMatrixSetting("number"));
			result.Add("PhpKeyword", GetColorFromOtherWebMatrixSetting("keyword"));
			result.Add("PhpVariable", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("PhpFunction", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("PhpOperator", GetColorFromOtherWebMatrixSetting("operator"));
			result.Add("PhpGlobalVariable", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("PhplCompileTimeConstant", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("SqlComment", GetVSColorSetting("Comment"));
			result.Add("SqlString", GetVSColorSetting("SQL String", "String"));
			result.Add("SqlKeyword", GetVSColorSetting("Keyword"));
			result.Add("SqlFunction", GetVSColorSetting("SQL System Function"));
			result.Add("SqlOperator", GetColorFromOtherWebMatrixSetting("operator"));
			result.Add("SqlVariable", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("SqlSystemVariable", GetColorFromOtherWebMatrixSetting("identifier"));
			result.Add("VBScriptComment", GetColorFromOtherWebMatrixSetting("comment"));
			result.Add("VBScriptString", GetColorFromOtherWebMatrixSetting("string"));
			result.Add("VBScriptKeyword", GetColorFromOtherWebMatrixSetting("keyword"));
			result.Add("HtmlScriptBlock", GetVSColorSetting(true, "Plain Text"));
			result.Add("HtmlStyleBlock", GetVSColorSetting(true, "Plain Text"));
			result.Add("roslyn.xmlDocAttribute", GetVSColorSetting("XML Attribute"));
			result.Add("roslyn.xmlDocComment", GetVSColorSetting("XML Doc Comment"));
			result.Add("sighelp-documentation", null);
			result.Add("currentParam", null);
			result.Add("natural language", null);
			result.Add("line number", GetVSColorSetting("Line Numbers"));
			result.Add("word wrap glyph", null);
			result.Add("Razor Code", GetVSColorSetting(true, "Razor Code"));
			result.Add("roslyn.xmlDocTag", GetVSColorSetting("XML Doc Tag"));
			result.Add("roslyn.xmlDocXmlComment", GetVSColorSetting("XML Doc Comment"));
			result.Add("roslyn.xmlDocCData", GetVSColorSetting("XML CData Section"));
			result.Add("roslyn.operator", GetColorFromOtherWebMatrixSetting("operator"));
			result.Add("roslyn.punctuation", GetColorFromOtherWebMatrixSetting("operator"));
			result.Add("roslyn.verbatimString", GetColorFromOtherWebMatrixSetting("string"));
			result.Add("roslyn.preprocessorKeyword", GetVSColorSetting("Preprocessor Keyword"));
			result.Add("roslyn.preprocessorText", GetVSColorSetting("Plain Text"));
			result.Add("comment", GetVSColorSetting("Comment"));
			result.Add("excluded code", GetVSColorSetting("Excluded Code"));
			result.Add("identifier", GetVSColorSetting("Plain Text"));
			result.Add("keyword", GetVSColorSetting("Keyword"));
			result.Add("roslyn.userType", GetVSColorSetting("User Types"));
			result.Add("roslyn.userTypeStructure", GetVSColorSetting("User Types(Value types)", "User Types"));
			result.Add("roslyn.userTypeInterface", GetVSColorSetting("User Types(Interfaces)", "User Types"));
			result.Add("roslyn.userTypeDelegate", GetVSColorSetting("User Types(Delegates)", "User Types"));
			result.Add("roslyn.userTypeEnum", GetVSColorSetting("User Types(Enums)", "User Types"));
			result.Add("roslyn.userTypeTypeParameter", GetVSColorSetting("User Types"));
			result.Add("roslyn.userTypeUnbound", GetVSColorSetting("User Types"));
			result.Add("preprocessor keyword", GetVSColorSetting("Preprocessor Keyword"));
			result.Add("operator", GetVSColorSetting("Plain Text"));
			result.Add("literal", GetVSColorSetting("Plain Text"));
			result.Add("string", GetVSColorSetting("String"));
			result.Add("number", GetVSColorSetting("Number"));
			result.Add("symbol definition", null);
			result.Add("symbol reference", null);
			result.Add("formal language", null);
			result.Add("roslyn.vb.userTypeModule", null);
			result.Add("roslyn.vb.xmlText", GetVSColorSetting("XML Text"));
			result.Add("roslyn.vb.xmlProcessingInstruction", GetVSColorSetting("XML Text"));
			result.Add("roslyn.vb.xmlName", GetVSColorSetting("XML Name"));
			result.Add("roslyn.vb.xmlEmbeddedExpression", GetVSColorSetting("XML Text"));
			result.Add("roslyn.vb.xmlDelimiter", GetVSColorSetting("XML Delimiter"));
			result.Add("roslyn.vb.xmlComment", GetVSColorSetting("XML Comment"));
			result.Add("roslyn.vb.xmlCDataSection", GetVSColorSetting("XML CData Section"));
			result.Add("roslyn.vb.xmlAttributeValue", GetVSColorSetting("XML Attribute Value"));
			result.Add("roslyn.vb.xmlAttributeQuotes", GetVSColorSetting("XML Attribute Quotes"));
			result.Add("roslyn.vb.xmlAttributeName", GetVSColorSetting("XML Attribute"));
			result.Add("roslyn.vb.xmlEntityReference", GetVSColorSetting("HTML Entity"));

			return result;
		}
		#endregion
	}
}
