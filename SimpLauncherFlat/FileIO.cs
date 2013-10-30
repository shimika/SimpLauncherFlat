using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpLauncherFlat {
	public class FileIO {
		static string ffList = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpLauncherFlat.ini";
		static string ffPref = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimpLauncherFlatSet.ini";
		public static MainWindow winMain;

		public static List<IconData> ReadFile() {
			if (!File.Exists(ffPref)) {
				using (StreamWriter sw = new StreamWriter(ffPref, false)) {
					sw.WriteLine("STARTUP=false");
					sw.WriteLine("LEFTSWITCH=true");
					sw.WriteLine("VOLUME=true");
				}
			}

			using (StreamReader sr = new StreamReader(ffPref)) {
				string[] strSplit = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string str in strSplit) {
					string[] strSplit2 = str.Split('=');
					switch (strSplit2[0]) {
						case "STARTUP":
							Pref.isStartup = Convert.ToBoolean(strSplit2[1]);
							break;
						case "LEFTSWITCH":
							Pref.isSwitchOn = Convert.ToBoolean(strSplit2[1]);
							break;
						case "VOLUME":
							Pref.isVolumeOn = Convert.ToBoolean(strSplit2[1]);
							break;
					}
				}
			}

			if (!File.Exists(ffList)) { using (StreamWriter sw = new StreamWriter(ffList, true)) { sw.Write(""); } }

			List<IconData> listIcon = new List<IconData>();

			using (StreamReader sr = new StreamReader(ffList)) {
				string[] strSplit = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				Layout.ResizeWindow(strSplit.Length, 0);

				foreach (string str in strSplit) {
					string[] strSplit2 = str.Split(new string[] { "#!SIMPLAUNCHER!#" }, StringSplitOptions.None);
					IconData icon = MakeIcon(strSplit2[0], strSplit2[1], Convert.ToBoolean(strSplit2[2]));
					listIcon.Add(icon);
				}
			}
			return listIcon;
		}

		public static void SavePref() {
			using (StreamWriter sw = new StreamWriter(ffPref, false)) {
				sw.WriteLine(string.Format("STARTUP={0}", Pref.isStartup));
				sw.WriteLine(string.Format("LEFTSWITCH={0}", Pref.isSwitchOn));
				sw.WriteLine(string.Format("VOLUME={0}", Pref.isVolumeOn));
			}
		}

		public static IconData MakeIcon(string strPath, string strTitle, bool isSpecial) {
			IconData icon = new IconData() { strPath = strPath, strTitle = strTitle, isSpecial = isSpecial, nID = Pref.nCount };
			if (icon.isSpecial) {
				switch (icon.strPath) {
					case "휴지통":
						icon.imgIcon = new BitmapImage(new Uri(@"/Resources/trash.png", UriKind.Relative));
						break;
				}
			} else {
				icon.imgIcon = GetIconImage(icon.strPath);
			}
			Pref.nCount++;
			return icon;
		}

		private static BitmapSource GetIconImage(string path) {
			if (!Directory.Exists(path) && !File.Exists(path)) { return null; }
			BitmapSource img = null;
			try {
				img = ExtractIcon.Get(path);
			} catch { }
			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
			return img;
		}
	}
}
