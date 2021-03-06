﻿using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

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

		// Temporary navigation control
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(MainPage), null);
		}
	}
}