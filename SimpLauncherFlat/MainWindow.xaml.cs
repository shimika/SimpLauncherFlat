using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreAudioApi;
using Microsoft.Win32;

namespace SimpLauncherFlat {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		SwitchWindow windowSwitch;
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(
			IntPtr hWnd,
			IntPtr hWndInsertAfter,
			int X,
			int Y,
			int cx,
			int cy,
			uint uFlags);

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;

		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		static void SendWpfWindowBack(Window window) {
			var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		}

		public MainWindow(SwitchWindow sWindow) {
			InitializeComponent();
			windowSwitch = sWindow;
			CustomIcon.winMain = Layout.winMain = FileIO.winMain = Rearrange.winMain = this;
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Left + 10;

			List<IconData> listIcon = FileIO.ReadFile();
			AddIcons(listIcon);

			textStartupOn.Visibility = Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;
			textStartupOff.Visibility = !Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;

			textSwitchOn.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
			textSwitchOff.Visibility = !Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;

			textVolumeOn.Visibility = Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
			textVolumeOff.Visibility = !Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;

			windowSwitch.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
			RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if (Pref.isStartup) {
				add.SetValue("SimpLauncherFlat", "\"" + System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName + "\"");
			}

			stackMod.RenderTransformOrigin = new Point(0.5, 0.5);
			stackMod.RenderTransform = new ScaleTransform(1, 1);

			this.PreviewMouseMove += MainWindow_PreviewMouseMove;
			this.PreviewMouseUp += (o, e) => Rearrange.MouseUp();
			this.MouseLeave += MainWindow_MouseLeave;
			this.Deactivated += (o, e) => {
				if (Pref.Visible) { AnimateWindow(0, false); }
				if (Pref.ModifyVisible) { buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
			};
			this.Closing += (o, e) => {
				if (!Pref.CloseFlag) {
					e.Cancel = true;
				} else {
					windowVolume.Close();
				}
			};

			buttonStartup.Click += (o, e) => {
				Pref.isStartup = !Pref.isStartup;
				textStartupOn.Visibility = Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;
				textStartupOff.Visibility = !Pref.isStartup ? Visibility.Visible : Visibility.Collapsed;

				add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				if (Pref.isStartup) {
					add.SetValue("SimpLauncherFlat", "\"" + System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName + "\"");
				} else {
					add.DeleteValue("SimpLauncherFlat", false);
				}

				FileIO.SavePref();
			};

			buttonSwitch.Click += (o, e) => {
				Pref.isSwitchOn = !Pref.isSwitchOn;
				textSwitchOn.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
				textSwitchOff.Visibility = !Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;
				windowSwitch.Visibility = Pref.isSwitchOn ? Visibility.Visible : Visibility.Collapsed;

				FileIO.SavePref();
			};

			buttonVolume.Click += (o, e) => {
				Pref.isVolumeOn = !Pref.isVolumeOn;
				textVolumeOn.Visibility = Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
				textVolumeOff.Visibility = !Pref.isVolumeOn ? Visibility.Visible : Visibility.Collapsed;
				FileIO.SavePref();
			};

			this.PreviewKeyDown += (o, e) => {
				if (!Pref.Visible || Pref.PrefVisible || Pref.ModifyVisible) { return; }

				textTest.Text = Layout.layoutMaxWidth + " : " + Layout.layoutMaxHeight + " : " + IconData.dictIcon.Count;
				switch (e.Key) {
					case Key.Up:
						if (Layout.nPrePoint >= Layout.layoutMaxWidth) {
							Layout.ReplaceSelector(Layout.nPrePoint - Layout.layoutMaxWidth);
						}
						break;
					case Key.Down:
						if (Layout.nPrePoint >= 0) {
							Layout.ReplaceSelector(Layout.nPrePoint + Layout.layoutMaxWidth);
						} else {
							Layout.ReplaceSelector(0);
						}
						break;
					case Key.Left:
						Layout.ReplaceSelector(Layout.nPrePoint - 1);
						break;
					case Key.Right:
						Layout.ReplaceSelector(Layout.nPrePoint + 1);
						break;
					case Key.Home:
						Layout.ReplaceSelector(0);
						break;
					case Key.End:
						Layout.ReplaceSelector(IconData.dictIcon.Count - 1);
						break;
					case Key.Enter:
						LaunchIcon(IconData.listIcon[Layout.nPrePoint]);
						break;
					case Key.Space:
						if (!Pref.ModifyVisible) {
							e.Handled = true;
							ModifyIcon(Layout.nPrePoint);
						}
						break;
				}
			};
		}

		private void MainWindow_MouseLeave(object sender, MouseEventArgs e) { Layout.ReplaceSelector(-1); }
		private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e) {
			Point pMouseMove = e.GetPosition(gridMain);
			Rearrange.MouseMove(pMouseMove);

			if (pMouseMove.X < 0 || pMouseMove.X > Layout.layoutMaxWidth * 110 || pMouseMove.Y < 0 || pMouseMove.Y > Layout.layoutMaxHeight * 110 || Rearrange.isMoving) {
				MainWindow_MouseLeave(null, null);
				return;
			}
			if (Pref.Visible && !Pref.ModifyVisible && !Pref.PrefVisible) {
				int nPoint = (int)(pMouseMove.X / 110) + (int)(pMouseMove.Y / 110) * Layout.layoutMaxWidth;
				if (nPoint < IconData.dictIcon.Count) {
					Layout.ReplaceSelector(nPoint);
				} else {
					Layout.ReplaceSelector(-1);
				}
			}
		}

		bool isAnimating = false;
		public void AnimateWindow(double isOpen, bool isForced) {
			if (isAnimating && !isForced) { return; }
			isAnimating = true;

			Pref.Visible = isOpen > 0 ? true : false;
			if (isOpen == 0) { windowSwitch.canTouch = false; }
			this.Focusable = true;
			this.Topmost = false; this.Topmost = true;
			this.Activate();

			if (isOpen > 0) {
				this.WindowState = WindowState.Normal;	
			}

			if (Pref.PrefVisible) { buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
			if (!Pref.Visible) { Layout.ReplaceSelector(-1); }

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

			sb.Completed += (o, e) => {
				windowSwitch.canTouch = true;
				isAnimating = false;
				this.Activate();

				if (!Pref.Visible) {
					SendWpfWindowBack(this);
					this.WindowState = WindowState.Minimized;
				}
			};

			sb.Begin(this);
		}

		public void AddItems(List<string> listPath) {
			List<IconData> listIcon = new List<IconData>();

			foreach (string strPath in listPath) {
				if (!File.Exists(strPath) && !Directory.Exists(strPath)) { continue; }

				string strTitle = System.IO.Path.GetFileNameWithoutExtension(strPath);

				if (Directory.Exists(strPath)) {
					strTitle = System.IO.Path.GetFileName(strPath);
					if (strTitle == "") {
						strTitle = strPath;
					}
				}

				IconData icon = FileIO.MakeIcon(strPath, strTitle, false);
				listIcon.Add(icon);
			}
			AddIcons(listIcon);
		}

		public void AddIcons(List<IconData> listIcon) {
			foreach (IconData icon in listIcon) {
				Grid grid = CustomIcon.GetIcon(icon);
				grid.Margin = new Thickness((IconData.dictIcon.Count % Layout.layoutMaxWidth) * 110, (IconData.dictIcon.Count / Layout.layoutMaxWidth) * 110, 0, 0);
				gridMain.Children.Add(grid);

				((Button)grid.Children[2]).Click += Icon_Click;
				((Button)grid.Children[2]).MouseRightButtonDown += Icon_RightClick;

				icon.nPosition = IconData.dictIcon.Count;
				IconData.dictIcon.Add(icon.nID, icon);
			}
			Layout.ResizeWindow(IconData.dictIcon.Count, 1);
			Layout.RefreshPositionList(0);
			AnimateWindow(1, true);
		}

		int nNowModifing = -1;
		private void Icon_RightClick(object sender, MouseButtonEventArgs e) {
			int nID = (int)((Button)sender).Tag;
			ModifyIcon(nID);
		}

		private void ModifyIcon(int nID) {
			if (nID < 0 || !IconData.dictIcon.ContainsKey(nID)) { return; }

			nNowModifing = nID;
			imgMod.Source = IconData.dictIcon[nID].imgIcon;
			textboxMod.Text = "";
			textboxMod.Tag = IconData.dictIcon[nID].strTitle;
			textModTitle.Text = IconData.dictIcon[nID].strTitle;
			textboxMod.Focus();

			//Layout.ReplaceSelector(-1);
			Pref.ModifyVisible = true;
			gridModCover.IsHitTestVisible = true;
			stackMod.IsHitTestVisible = true;

			AnimateBox(stackMod, gridModCover, 1.5, 1, 1);
		}

		private void Icon_Click(object sender, RoutedEventArgs e) {
			int nID = (int)((Button)sender).Tag;
			if (nID == Rearrange.nPrevMovingIndex) { return; }
			LaunchIcon(nID);
		}

		private void LaunchIcon(int nID) {
			if (nID < 0) { return; }

			Layout.ReplaceSelector(-1);
			Pref.Visible = false; AnimateWindow(0, true);

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

		#region Preference window event

		private void gridPrefCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonPref_Click(object sender, RoutedEventArgs e) {
			Pref.PrefVisible = !Pref.PrefVisible;
			gridPrefCover.IsHitTestVisible = Pref.PrefVisible;
			double dMargin = Pref.PrefVisible ? 0 : -250;

			gridSelector.Visibility = Pref.PrefVisible ? Visibility.Collapsed : Visibility.Visible;

			gridPref.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(
				new Thickness(dMargin, 0, 0, 0),
				TimeSpan.FromMilliseconds(400)) {
					EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 },
					BeginTime = TimeSpan.FromMilliseconds(100 * (Pref.PrefVisible ? 1 : 0)),
				});
			gridPrefCover.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(Pref.PrefVisible ? 1 : 0, TimeSpan.FromMilliseconds(200)));
		}
		#endregion

		#region Volume Controller

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

			Layout.ResizeWindow(IconData.dictIcon.Count, 1);
			Layout.RefreshPositionList(0);

			AnimateWindow(0, true);

			/*MMDeviceCollection collect = DevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATEMASK_ALL);
			for (int i = 0; i < collect.Count; i++) {
				collect[i].AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
			}
			 */ 
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

		#endregion

		#region Modify Window Event

		private void AnimateBox(StackPanel uie, Grid cover, double dbFrom, double dbTarget, double dbOpacity) {
			Storyboard sb = new Storyboard();

			DoubleAnimation da1 = new DoubleAnimation(dbFrom, dbTarget, TimeSpan.FromMilliseconds(300)) { EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut } };
			DoubleAnimation da2 = new DoubleAnimation(dbFrom, dbTarget, TimeSpan.FromMilliseconds(300)) { EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut } };
			DoubleAnimation da3 = new DoubleAnimation((1 - dbOpacity), dbOpacity, TimeSpan.FromMilliseconds(250));
			DoubleAnimation da4 = new DoubleAnimation((1 - dbOpacity), dbOpacity, TimeSpan.FromMilliseconds(250));

			Storyboard.SetTarget(da1, uie); Storyboard.SetTarget(da2, uie); Storyboard.SetTarget(da3, uie); Storyboard.SetTarget(da4, cover);
			Storyboard.SetTargetProperty(da1, new PropertyPath("(StackPanel.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(da2, new PropertyPath("(StackPanel.RenderTransform).(ScaleTransform.ScaleY)"));
			Storyboard.SetTargetProperty(da3, new PropertyPath(StackPanel.OpacityProperty));
			Storyboard.SetTargetProperty(da4, new PropertyPath(Grid.OpacityProperty));

			sb.Children.Add(da1); sb.Children.Add(da2); sb.Children.Add(da3); sb.Children.Add(da4); 
			sb.Begin(this);
		}

		private void buttonModOK_Click(object sender, RoutedEventArgs e) {
			if (!Pref.ModifyVisible) { return; }
			int nID = nNowModifing;
			buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

			if (nID < 0) { return; }
			if (!IconData.dictIcon.ContainsKey(nID)) { return; }

			IconData.dictIcon[nID].strTitle = textboxMod.Text;
			FileIO.SaveList();
		}

		private void textboxMod_KeyDown(object sender, KeyEventArgs e) {
			switch (e.Key) {
				case Key.Enter: buttonModOK.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); break;
				case Key.Escape: buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); break;
			}
		}

		private void gridModCover_MouseDown(object sender, MouseButtonEventArgs e) { buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
		private void buttonModCancel_Click(object sender, RoutedEventArgs e) {
			if (!Pref.ModifyVisible) { return; }

			Pref.ModifyVisible = false;
			nNowModifing = -1;
			gridModCover.IsHitTestVisible = false;
			stackMod.IsHitTestVisible = false;

			AnimateBox(stackMod, gridModCover, 1, 0.7, 0);
		}

		private void buttonGoto_Click(object sender, RoutedEventArgs e) {
			int nID = nNowModifing;
			buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

			if (IconData.dictIcon[nID].isSpecial) { return; }

			string strPath, strArgument = "";
			if (Directory.Exists(IconData.dictIcon[nID].strPath)) {
				strPath = IconData.dictIcon[nID].strPath;
			} else if (File.Exists(IconData.dictIcon[nID].strPath)) {
				strPath = "explorer.exe";
				strArgument = @"/select, " + IconData.dictIcon[nID].strPath;
			} else {
				return;
			}
			Task.Factory.StartNew(() => Process.Start(strPath, strArgument));
		}

		private void buttonRemove_Click(object sender, RoutedEventArgs e) {
			int nID = nNowModifing;
			buttonModCancel.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			RemoveItem(nID);
		}

		public void RemoveItem(int nID) {
			if (!IconData.dictIcon.ContainsKey(nID)) { return; }

			gridMain.Children.Remove(IconData.dictIcon[nID].gridBase);
			IconData.dictIcon.Remove(nID);

			Layout.ResizeWindow(IconData.dictIcon.Count, 1);
			Layout.RefreshPositionList(1);
		}

		#endregion
	}
}
