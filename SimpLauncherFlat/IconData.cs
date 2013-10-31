using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace SimpLauncherFlat {
	public class IconData : INotifyPropertyChanged {
		public int nID, nPosition;
		private string _strTitle;
		public string strPath;
		public BitmapSource imgIcon;
		public bool isSpecial;

		public string strTitle {
			get { return _strTitle; }
			set {
				_strTitle = value;
				OnPropertyChanged("strTitle");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static Dictionary<int, IconData> dictIcon = new Dictionary<int, IconData>();
		public static Dictionary<int, int> listIcon = new Dictionary<int, int>();
	}

	public class Pref {
		public static int nCount;
		public static bool isStartup, isSwitchOn, isVolumeOn;

		public static bool Visible = false, CloseFlag, PrefVisible, ModifyVisible;
	}
}
