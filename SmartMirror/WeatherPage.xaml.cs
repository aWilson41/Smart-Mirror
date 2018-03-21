﻿using System;
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
	public sealed partial class WeatherPage : Page
	{
		public WeatherPage()
		{
			InitializeComponent();
			DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
		}


		public void SetTodaysForecast(int high, int low)
		{

		}

		public void SetCurrentConditions(int currTemp, string currConditions)
		{

		}

		// Sets the weather forecast/timeline given the temperatures
		public void Set12HrForecast(DateTime dateTime, List<int> tempForecast, List<int> precipForecast)
		{

		}

		public void SetWeatherGIF()
		{

		}

		public void Set12DayForecast()
		{

		}


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(CalendarPage), null);
		}
	}
}