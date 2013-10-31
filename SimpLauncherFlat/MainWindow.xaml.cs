using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreAudioApi;

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
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Left + 10;

			List<IconData> listIcon = FileIO.ReadFile();
			AddIcons(listIcon);

			textStartupOn.Visibility = Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;
			textStartupOff.Visibility = !Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;

			textSwitchOn.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
			textSwitchOff.Visibility = !Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;

			textVolumeOn.Visibility = Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
			textVolumeOff.Visibility = !Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;

			this.PreviewMouseMove += MainWindow_PreviewMouseMove;
			this.MouseLeave += MainWindow_MouseLeave;
			this.Deactivated += (o, e) => {
				if (Pref.Visible) {
					AnimateWindow(0);
				}
			}; 
			this.Closing += (o, e) => { if (!Pref.CloseFlag) { e.Cancel = true; } };

			buttonStartup.Click += (o, e) => {
				Pref.isStartup = !Pref.isStartup;
				textStartupOn.Visibility = Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;
				textStartupOff.Visibility = !Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;
				FileIO.SavePref();
			};

			buttonSwitch.Click += (o, e) => {
				Pref.isSwitchOn = !Pref.isSwitchOn;
				textSwitchOn.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
				textSwitchOff.Visibility = !Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
				FileIO.SavePref();
			};

			buttonVolume.Click += (o, e) => {
				Pref.isVolumeOn = !Pref.isVolumeOn;
				textVolumeOn.Visibility = Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
				textVolumeOff.Visibility = !Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
				FileIO.SavePref();
			};
		}

		private void MainWindow_MouseLeave(object sender, MouseEventArgs e) { Layout.ReplaceSelector(-1); }
		private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e) {
			Point pMouseMove = e.GetPosition(gridMain);
			if (pMouseMove.X < 0 || pMouseMove.X > Layout.layoutMaxWidth * 110 || pMouseMove.Y < 0 || pMouseMove.Y > Layout.layoutMaxHeight * 110) {
				MainWindow_MouseLeave(null, null);
				return;
			}
			if (Pref.Visible) {
				int nPoint = (int)(pMouseMove.X / 110) + (int)(pMouseMove.Y / 110) * Layout.layoutMaxWidth;
				Layout.ReplaceSelector(nPoint);
			}
		}

		public void AnimateWindow(double isOpen) {
			Pref.Visible = isOpen > 0 ? true : false;
			if (isOpen == 0) { windowSwitch.canTouch = false; }
			this.Activate();
			this.WindowState = WindowState.Normal;

			if (Pref.PrefVisible) { buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }


			Storyboard sb = new Storyboard();

			ThicknessAnimation ta = new ThicknessAnimation(new Thickness(-200 * isOpen + 30, 0, 0, 0), new Thickness(-100 * (1 - isOpen) + 30, 0, 0, 0), TimeSpan.FromMilliseconds(300)) {
				BeginTime = TimeSpan.FromMilliseconds(50 * isOpen),
			};
			if (isOpen > 0) {
				ta.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			} else {
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

		public void AddIcons(List<IconData> listIcon) {
			foreach (IconData icon in listIcon) {
				Grid grid = CustomIcon.GetIcon(icon);
				grid.Margin = new Thickness((IconData.dictIcon.Count % Layout.layoutMaxWidth) * 110, (IconData.dictIcon.Count / Layout.layoutMaxWidth) * 110, 0, 0);
				gridMain.Children.Add(grid);

				((Button)grid.Children[2]).Click += Icon_Click;

				IconData.dictIcon.Add(icon.nID, icon);
			}
		}

		private void Icon_Click(object sender, RoutedEventArgs e) {
			Layout.ReplaceSelector(-1);
			Pref.Visible = false; AnimateWindow(0);

			int nID = (int)((Button)sender).Tag;
			Process process = new Process();
			if (IconData.dictIcon[nID].isSpecial) {
				switch (IconData.dictIcon[nID].strPath) {
					case "휴지통":
						process.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\explorer.exe";
						process.StartInfo.Arguments = "e,::{645FF040-5081-101B-9F08-00AA002F954E}";
						break;
				}
			} else {
				if (File.Exists(IconData.dictIcon[nID].strPath)) {
					process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(IconData.dictIcon[nID].strPath);
				} else if (Directory.Exists(IconData.dictIcon[nID].strPath)) {
				} else { return; }
				process.StartInfo.FileName = IconData.dictIcon[nID].strPath;
			}
			process.Start();
		}

		private void gridPrefCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonPref_Click(object sender, RoutedEventArgs e) {
			Pref.PrefVisible = !Pref.PrefVisible;
			gridPrefCover.IsHitTestVisible = Pref.PrefVisible;
			double dMargin = Pref.PrefVisible ? 0 : -250;

			gridRow.Visibility = Pref.PrefVisible ? Visibility.Collapsed : Visibility.Visible;
			gridColumn.Visibility = Pref.PrefVisible ? Visibility.Collapsed : Visibility.Visible;

			gridPref.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(
				new Thickness(dMargin, 0, 0, 0),
				TimeSpan.FromMilliseconds(400)) {
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 },
					BeginTime = TimeSpan.FromMilliseconds(100 * (Pref.PrefVisible ? 1 : 0)),
				});
			gridPrefCover.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(Pref.PrefVisible ? 1 : 0, TimeSpan.FromMilliseconds(200)));
		}

		MMDevice device;
		MMDeviceEnumerator DevEnum;
		VolumeWindow windowVolume;
		bool isMuted; double nowVolume;
		private void Window_Loaded(object sender, RoutedEventArgs e) {
			windowVolume = new VolumeWindow();
			windowVolume.Show();
			new AltTab().HideAltTab(this);
			
			DevEnum = new MMDeviceEnumerator();
			device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
			device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
		}

		private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) {
			if (!Pref.isVolumeOn) { return; }
			if (isMuted == data.Muted && nowVolume == (int)(data.MasterVolume * 100)) { return; }
			nowVolume = data.MasterVolume * 100;
			isMuted = data.Muted;

			Dispatcher.Invoke(new Action(() => {
				windowVolume.RefreshVolume(nowVolume, data.Muted);
			}));
		}
	}
}
