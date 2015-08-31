#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;

namespace mattmc3.WebMatrixColorizer {

	public interface IColorSettingConverter {
		WebMatrixColorSetting GetColorSetting(string webMatrixSettingName);
		string GetSurfaceBackground();
	}
}
