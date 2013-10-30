using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		}
	}
}
