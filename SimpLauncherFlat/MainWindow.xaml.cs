using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpLauncherFlat {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		SwitchWindow windowSwitch;

		public MainWindow(SwitchWindow sWindow) {
			InitializeComponent();
			windowSwitch = sWindow;
			Layout.winMain = FileIO.winMain = this;

			FileIO.ReadFile();

			CaptureMouse();
			this.PreviewMouseMove += MainWindow_PreviewMouseMove;
			this.MouseLeave += MainWindow_MouseLeave;
		}

		private void AnimateSelector(Grid gridSelector, double dLeft, double dTop) {
			gridSelector.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(dLeft, dTop, 0, 0), TimeSpan.FromMilliseconds(200)) {
				EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 4, }
			});
		}

		private void MainWindow_MouseLeave(object sender, MouseEventArgs e) {
			nSelectorPosition = -1;
			AnimateSelector(gridRow, 0, -110);
			AnimateSelector(gridColumn, -110, 0);
		}

		int nSelectorPosition = -1;
		private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e) {
			Point pMouseMove = e.GetPosition(gridMain);
			if (pMouseMove.X < 0 || pMouseMove.X > Layout.layoutMaxWidth * 110 || pMouseMove.Y < 0 || pMouseMove.Y > Layout.layoutMaxHeight * 110) {
				MainWindow_MouseLeave(null, null);
				return;
			}
			int nPoint = (int)(pMouseMove.X / 110) + (int)(pMouseMove.Y / 110) * Layout.layoutMaxWidth;
			nPoint = Math.Max(nPoint, 0);
			nPoint = Math.Min(nPoint, IconData.dictIcon.Count - 1);
			if (nPoint != nSelectorPosition) {
				nSelectorPosition = nPoint;
				AnimateSelector(gridRow, 0, (nPoint / Layout.layoutMaxWidth) * 110);
				AnimateSelector(gridColumn, (nPoint % Layout.layoutMaxWidth) * 110, 0);
			}
		}

		public void AnimateWindow(int isOpen) {
		}

		public void AddItems(List<string> listPath) {
		}
	}
}
