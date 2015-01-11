using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SimpLauncherFlat {
	public class CustomIcon {
		public static MainWindow winMain;
		public static Grid GetIcon(IconData icon){
			Grid grid = new Grid() {
				Width = 110, Height = 110, Background = Brushes.Transparent, Tag = icon.nID,
				HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
			};
			Button button = new Button() { Width = 110, Height = 110, Background = Brushes.Transparent, Tag = icon.nID };
			TextBlock txt = new TextBlock() {
				HorizontalAlignment = HorizontalAlignment.Center, FontSize = 13.33,
				VerticalAlignment = VerticalAlignment.Top, TextTrimming = TextTrimming.CharacterEllipsis,
				Margin = new Thickness(0, 85, 0, 0)
			};
			Binding binding = new Binding("strTitle");
			binding.Source = icon;
			txt.SetBinding(TextBlock.TextProperty, binding);
			Image img = new Image() {
				Source = icon.imgIcon, Width = 70, Height = 70,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(10),
			};

			icon.gridBase = grid;
			button.PreviewMouseDown += grid_MouseDown;

			grid.Children.Add(txt);
			grid.Children.Add(img);
			grid.Children.Add(button);

			return grid;
		}

		static void grid_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				//winMain.CaptureMouse();
				Rearrange.MouseDown((int)((Button)sender).Tag, e.GetPosition(winMain.gridMain));
			}
		}
	}
}
