#region header
// $Id$
// see license.txt for copyright and license details
#endregion
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using mattmc3.Common;
using mattmc3.WebMatrixColorizer;

namespace WebMatrixColorizer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private Microsoft.Win32.OpenFileDialog OpenFileDialog {
			get {
				if (_openFileDialog == null) {
					var dlg = new Microsoft.Win32.OpenFileDialog();
					dlg.FileName = "";
					dlg.DefaultExt = ".vssettings";
					dlg.Filter = "Visual Studio Settings (.vssettings)|*.vssettings";
					_openFileDialog = dlg;
				}
				return _openFileDialog;
			}
		}
		private Microsoft.Win32.OpenFileDialog _openFileDialog;

		private Microsoft.Win32.SaveFileDialog SaveFileDialog {
			get {
				if (_saveFileDialog == null) {
					var dlg = new Microsoft.Win32.SaveFileDialog();
					dlg.FileName = "";
					dlg.DefaultExt = ".xml";
					dlg.Filter = "WebMatrix Color Settings XML (.xml)|*.xml";
					dlg.OverwritePrompt = true;
					_saveFileDialog = dlg;
				}
				return _saveFileDialog;
			}
		}
		private Microsoft.Win32.SaveFileDialog _saveFileDialog;

		private void btnBrowse_Click(object sender, RoutedEventArgs e) {
			Nullable<bool> result = OpenFileDialog.ShowDialog();
			if (result == true) {
				txtVSSettingsFilePath.Text = OpenFileDialog.FileName;
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e) {
			if (File.Exists(txtVSSettingsFilePath.Text) == false) {
				MessageBox.Show("Cannot find file: {0}".FormatWith(txtVSSettingsFilePath.Text));
				return;
			}

			try {
				SaveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(txtVSSettingsFilePath.Text);
				SaveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(txtVSSettingsFilePath.Text) + ".xml";
				Nullable<bool> result = SaveFileDialog.ShowDialog();
				if (result == true) {
					var outFilePath = SaveFileDialog.FileName;
					if (File.Exists(outFilePath)) {
						File.Delete(outFilePath);
					}
					ConvertVSSettingsToWebMatrix(txtVSSettingsFilePath.Text, outFilePath);
					MessageBox.Show("VSSettings file successfully converted! Now, import it into WebMatrix.");
				}
			}
			catch (Exception ex) {
				MessageBox.Show("Error (sorry it's not friendlier): {0}".FormatWith(ex.Message));
			}
		}

		private void ConvertVSSettingsToWebMatrix(string vssettings, string webmatrixXml) {
			XDocument vssettingsXDoc;
			try {
				vssettingsXDoc = XDocument.Load(vssettings);
			}
			catch (Exception ex) {
				throw new Exception("Invalid or unusable vssettings file", ex);
			}
			var themeConverter = WebMatrixColorThemeConverterFactory.Create(vssettingsXDoc);
			var webmatrixTheme = themeConverter.ConvertToWebMatrixColorTheme();
			webmatrixTheme.Save(webmatrixXml);
		}

		private void Window_Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				txtVSSettingsFilePath.Text = files[0];
			}
		}
	}
}
