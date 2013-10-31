using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SimpLauncherFlat {
	public class Layout {
		public static MainWindow winMain;
		public static int layoutMaxHeight = 2, layoutMaxWidth = 3;

		public static void ResizeWindow(int nCount, double dAnimateOn) {
			int nc = (int)Math.Sqrt(nCount * 2 / 3);
			layoutMaxHeight = Math.Max(2, nc);
			for (int i = nc; ; i++) {
				if (i * layoutMaxHeight >= nCount) { layoutMaxWidth = i; break; }
			}
			layoutMaxWidth = Math.Max(3, layoutMaxWidth);

			winMain.BeginAnimation(MainWindow.WidthProperty,
				new DoubleAnimation(layoutMaxWidth * 110 + 50, TimeSpan.FromMilliseconds(300 * dAnimateOn)) {
					BeginTime = TimeSpan.FromMilliseconds(100 * dAnimateOn),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
				});
			winMain.BeginAnimation(MainWindow.HeightProperty,
				new DoubleAnimation(layoutMaxHeight * 110 + 20, TimeSpan.FromMilliseconds(300 * dAnimateOn)) {
					BeginTime = TimeSpan.FromMilliseconds(100 * dAnimateOn),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
				});

			winMain.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 2 - (layoutMaxHeight * 55 + 10);
		}

		static void AnimateSelector(Grid gridSelector, double dLeft, double dTop) {
			gridSelector.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(dLeft, dTop, 0, 0), TimeSpan.FromMilliseconds(100)) {
				EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3, }
			});
		}

		public static int nPrePoint = -1;
		public static void ReplaceSelector(int nPoint) {
			if (nPrePoint == nPoint) { return; }
			nPoint = Math.Max(nPoint, -1);
			nPrePoint = nPoint;

			if (nPoint < 0) {
				AnimateSelector(winMain.gridRow, 0, -110);
				AnimateSelector(winMain.gridColumn, -110, 0);
			} else {
				nPrePoint = Math.Min(nPoint, IconData.dictIcon.Count - 1);
				AnimateSelector(winMain.gridRow, 0, (nPrePoint / Layout.layoutMaxWidth) * 110);
				AnimateSelector(winMain.gridColumn, (nPrePoint % Layout.layoutMaxWidth) * 110, 0);
			}
		}

		public static void RefreshPositionList() {
			int[] nArray = new int[IconData.dictIcon.Count];
			int nCount = 0;

			foreach (KeyValuePair<int, IconData> kv in IconData.dictIcon) {
				nArray[nCount++] = kv.Value.nPosition * 10000 + kv.Value.nID;
			}
			Array.Sort(nArray);
			for (int i = 0; i < IconData.dictIcon.Count; i++) {
				IconData.listIcon.Add(nArray[i] / 10000, nArray[i] % 1000);
				IconData.dictIcon[nArray[i] % 1000].nPosition = i;
			}
		}
	}
}
