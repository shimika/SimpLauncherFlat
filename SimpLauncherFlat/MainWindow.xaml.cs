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
			CustomIcon.winMain = Layout.winMain = FileIO.winMain = this;

			FileIO.ReadFile();
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Left + 10;

			this.PreviewMouseMove += MainWindow_PreviewMouseMove;
			this.MouseLeave += MainWindow_MouseLeave;
			this.Deactivated += (o, e) => {
				if (Pref.Visible) {
					AnimateWindow(0);
				}
			};
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

		public void AnimateWindow(double isOpen) {
			Pref.Visible = isOpen > 0 ? true : false;
			if (isOpen == 0) { windowSwitch.canTouch = false; }
			this.Activate();
			this.WindowState = WindowState.Normal;

			Storyboard sb = new Storyboard();

			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-200 * isOpen + 30, 0, 0, 0), new Thickness(-100 * (1 - isOpen) + 30, 0, 0, 0), TimeSpan.FromMilliseconds(300)) {
				BeginTime = TimeSpan.FromMilliseconds(50 * isOpen),
			};
			if (isOpen > 0) {
				ta.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			} else {
				ta.Duration = TimeSpan.FromMilliseconds(300);
				ta.EasingFunction = new ExponentialEase() { Exponent = 6, EasingMode = EasingMode.EaseInOut };
			}

			Storyboard.SetTarget(ta, gridMain);
			Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));
			sb.Children.Add(ta);

			DoubleAnimation da3 = new DoubleAnimation(isOpen, TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromMilliseconds(100),
			};
			Storyboard.SetTarget(da3, this);
			Storyboard.SetTargetProperty(da3, new PropertyPath(Window.OpacityProperty));
			sb.Children.Add(da3);

			sb.Completed += delegate(object sender, EventArgs e) {
				windowSwitch.canTouch = true;
				if (!Pref.Visible) {
					this.WindowState = WindowState.Minimized;
				}
			};

			sb.Begin(this);
		}

		public void AddItems(List<string> listPath) {
		}

		public void StartProcess(int nID) {
		}
	}
}
