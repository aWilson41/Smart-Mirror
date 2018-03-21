using System;
using System.Globalization;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace SmartMirror
{
	public sealed partial class CalendarPage : Page
	{
		public CalendarPage()
		{
			InitializeComponent();
			DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(MainPage), null);
		}
	}
}