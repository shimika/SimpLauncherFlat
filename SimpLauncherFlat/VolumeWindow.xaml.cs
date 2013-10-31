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
using System.Windows.Shapes;

namespace SimpLauncherFlat {
	/// <summary>
	/// VolumeWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class VolumeWindow : Window {
		public VolumeWindow() {
			InitializeComponent();

			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left + 90;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + 100;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			AltTab alttab = new AltTab();
			alttab.HideAltTab(this);

			this.Closing += delegate(object senderx, System.ComponentModel.CancelEventArgs ex) {
				if (!Pref.CloseFlag) {
					ex.Cancel = true;
					this.Topmost = false;
					this.Topmost = true;
				}
			};
		}

		public void RefreshVolume(double volume, bool isMuted) {
			this.Topmost = false; this.Topmost = true; this.Opacity = 1;

			if (isMuted) {
				gridVolume.Visibility = Visibility.Collapsed;
				imgVolume.Visibility = Visibility.Collapsed;
				textVolume.Text = "X";
			} else {
				gridVolume.Visibility = Visibility.Visible;
				imgVolume.Visibility = Visibility.Visible;
				textVolume.Text = ((int)volume).ToString();
			}

			gridVolume.Width = volume;

			Storyboard sb = new Storyboard();
			DoubleAnimation winAnipre = new DoubleAnimation(1, 1, TimeSpan.FromMilliseconds(2500));
			Storyboard.SetTarget(winAnipre, this);
			Storyboard.SetTargetProperty(winAnipre, new PropertyPath(Window.OpacityProperty));

			DoubleAnimation winAni = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
			Storyboard.SetTarget(winAni, this);
			Storyboard.SetTargetProperty(winAni, new PropertyPath(Window.OpacityProperty));
			winAni.BeginTime = TimeSpan.FromMilliseconds(2500);

			sb.Children.Add(winAnipre);
			sb.Children.Add(winAni);
			sb.Begin(this);
		}
	}
}
