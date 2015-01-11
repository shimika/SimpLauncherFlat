using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace SimpLauncherFlat {
	public class Rearrange {
		public static MainWindow winMain;
		public static int nFromIndex, nToIndex, nMouseDownID, nMouseMovingID, nPrevMovingIndex = -1;
		public static Point pointMouseDown, pointMouseMove;
		public static bool isMoving, isMouseDown;
		public static int[] naPosition, naDummyPosition;

		public static void MouseDown(int nID, Point pPoint) {
			nPrevMovingIndex = -1;
			isMouseDown = true; isMoving = false;
			nMouseDownID = nID; nToIndex = -1;
			pointMouseDown = pPoint;
			nFromIndex = IconData.dictIcon[nID].nPosition;

			naPosition = new int[IconData.dictIcon.Count - 1];
			naDummyPosition = new int[IconData.dictIcon.Count - 1];

			winMain.textTest.Text = "Down" + " : " + nID;

			int nCount = 0;
			foreach (KeyValuePair<int, int> kvp in IconData.listIcon) {
				if (kvp.Value == nID) { continue; }
				naPosition[nCount++] = kvp.Value;
				IconData.dictIcon[kvp.Value].nDummyPosition = IconData.dictIcon[kvp.Value].nPosition;
			}
		}

		public static void MouseMove(Point pPoint) {
			winMain.textTest.Text = pointMouseMove.X + " : " + pointMouseMove.Y + " : " + nToIndex;
			if (!isMouseDown || nMouseDownID < 0) { return; }
			pointMouseMove = pPoint;

			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 10 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				winMain.imageMove.Source = IconData.dictIcon[nMouseDownID].imgIcon;
				winMain.gridMovePanel.OpacityMask = new ImageBrush(IconData.dictIcon[nMouseDownID].imgIcon);

				winMain.gridMovePanel.Visibility = Visibility.Visible;
				IconData.dictIcon[nMouseDownID].gridBase.Visibility = Visibility.Collapsed;

				AnimateFrame(Colors.Crimson, 1);
			}
			if (!isMoving) { return; }
			CalculatePoint();
		}

		private static void CalculatePoint() {
			if (!isMouseDown || nMouseDownID < 0 || !isMoving) { return; }

			winMain.gridMovePanel.Margin = new Thickness(pointMouseMove.X - 5, pointMouseMove.Y - 35, 0, 0);
			winMain.gridMovePanel.Background = Brushes.Transparent;

			if (pointMouseMove.X > Layout.layoutMaxWidth * 110 + 30 || pointMouseMove.Y < 0 || pointMouseMove.Y > Layout.layoutMaxHeight * 110) {
				nToIndex = IconData.dictIcon.Count - 1;
			} else if (pointMouseMove.X < 30) {
				nToIndex = -1;
				winMain.gridMovePanel.Background = Brushes.Red;
			} else {
				int nRow = (int)(pointMouseMove.Y / 110);
				int nCol = (int)((pointMouseMove.X + 0) / 110);

				nToIndex = Math.Max(nRow * Layout.layoutMaxWidth + nCol, 0);
				nToIndex = Math.Min(nToIndex, IconData.dictIcon.Count - 1);
			}

			int nAdd = 0;
			for (int i = 0; i < IconData.dictIcon.Count - 1; i++) {
				if (i == nToIndex) { nAdd++; }
				naDummyPosition[i] = i + nAdd;
			}

			for (int i = 0; i < IconData.dictIcon.Count - 1; i++) {
				if (IconData.dictIcon[naPosition[i]].nDummyPosition != naDummyPosition[i]) {
					IconData.dictIcon[naPosition[i]].nDummyPosition = naDummyPosition[i];

					IconData.dictIcon[naPosition[i]].gridBase.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(naDummyPosition[i] % Layout.layoutMaxWidth * 110, naDummyPosition[i] / Layout.layoutMaxWidth * 110, 0, 0), TimeSpan.FromMilliseconds(200)) {
						EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut }
					});
				}
			}
		}

		public static void MouseUp() {
			//winMain.ReleaseMouseCapture();
			isMouseDown = false;
			if (nMouseDownID < 0 || !isMoving) { return; }
			isMoving = false;
			AnimateFrame(Colors.DodgerBlue, 0);
			winMain.gridMovePanel.Visibility = Visibility.Collapsed;

			nPrevMovingIndex = nMouseDownID;
			IconData.dictIcon[nMouseDownID].gridBase.Visibility = Visibility.Visible;

			if (nToIndex < 0) {
				winMain.RemoveItem(nMouseDownID);
				return;
			}

			for (int i = 0; i < IconData.dictIcon.Count - 1; i++) {
				IconData.dictIcon[naPosition[i]].nPosition = IconData.dictIcon[naPosition[i]].nDummyPosition;
			}
			IconData.dictIcon[nMouseDownID].nPosition = nToIndex;
			Layout.RefreshPositionList(0);

			nMouseDownID = -1;
		}


		private static void AnimateFrame(Color color, double opacity) {
			Storyboard sb = new Storyboard();

			DoubleAnimation d1 = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(200));
			DoubleAnimation d2 = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(200));

			Storyboard.SetTarget(d1, winMain.gridDeleteArea);
			Storyboard.SetTarget(d2, winMain.gridRemoveFrame);

			Storyboard.SetTargetProperty(d1, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTargetProperty(d2, new PropertyPath(Grid.OpacityProperty));

			sb.Children.Add(d1); sb.Children.Add(d2);
			sb.Begin();
			winMain.grideffectShadow.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(200)));
		}
	}
}
