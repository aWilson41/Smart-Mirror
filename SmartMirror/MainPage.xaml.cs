using System;
using System.Globalization;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace SmartMirror
{
	public sealed partial class MainPage : Page
	{
		DispatcherTimer timer;
		DateTime currDateTime = new DateTime();
		DateTime prevDateTime = new DateTime();

		public MainPage()
		{
			InitializeComponent();
			DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
			//ApplicationView.GetForCurrentView().ExitFullScreenMode();
		}


		public void SetDate(DateTime currDateTime)
		{
			month.Text = currDateTime.ToString("MMM", CultureInfo.InvariantCulture);
			// Suffix is th unless it has a 1, 2, or 3 on the end. Only works 10s digits. (But days only go up to 31)
			string suffix = "th";
			int value = currDateTime.Day % 10;
			if (currDateTime.Day - value != 10)
			{
				if (value == 1)
					suffix = "st";
				else if (value == 2)
					suffix = "nd";
				else if (value == 3)
					suffix = "rd";
			}
			day.Text = currDateTime.Day.ToString() + suffix;
		}

		public void SetTime(DateTime currDateTime)
		{
			time.Text = currDateTime.ToString("hh:mm tt");
			// Remove preceding zero (ie first zero in 01:00)
			if (time.Text[0] == '0')
				time.Text = time.Text.Remove(0, 1);
		}

		public void SetTodaysForecast(int high, int low)
		{
			// Snap High and Low next to weather text
			double posX = (double)weather.GetValue(Canvas.LeftProperty);
			weather.Measure(new Size(990, 101));
			double width = weather.ActualWidth * 0.5 + 200;

			weatherLow.Text = low.ToString();
			weatherLow.SetValue(Canvas.LeftProperty, width);

			weatherHigh.Text = high.ToString();
			weatherHigh.SetValue(Canvas.LeftProperty, width);
		}

		public void SetCurrentConditions(int currTemp, string currConditions)
		{
			weather.Text = currConditions + " at " + currTemp.ToString() + (char)176;
		}

		// Sets the weather forecast/timeline given the temperatures
		public void Set12HrForecast(DateTime dateTime, List<int> tempForecast, List<int> precipForecast)
		{
			if (tempForecast.Count <= 0 || precipForecast.Count <= 0 || tempForecast.Count != precipForecast.Count)
				return;

			// Find the max and min temperature
			int min = tempForecast[0];
			int max = tempForecast[0];
			for (int i = 1; i < tempForecast.Count; i++)
			{
				if (tempForecast[i] < min)
					min = tempForecast[i];
				if (tempForecast[i] > max)
					max = tempForecast[i];
			}

			// Floor the min to the nearest 10s
			int minFloor = (min / 10) * 10;
			// Ceil the max to the nearest 10s
			int maxCeil = (max / 10 + 1) * 10;
			// How many dividers we should put
			int numOfDiv = (maxCeil - minFloor) / 10 - 1;

			WeatherGraph.SetBounds(0, minFloor, 11, maxCeil);
			WeatherGraph.SetLineColor(Color.FromArgb(255, 135, 206, 235));
			WeatherGraph.SetLineThickness(8.0);
			WeatherGraph.SetLabelSuffix("%");
			WeatherGraph.SetVertLabelSuffix(((char)176).ToString());
			WeatherGraph.SetPointFontSize(25.0);
			WeatherGraph.SetNumberOfSubdivisions(20);
			WeatherGraph.SetNumberOfDividers(10, numOfDiv);
			WeatherGraph.SetDividerVisibility(false, true);
			WeatherGraph.Clear();
			for (int i = 0; i < tempForecast.Count; i++)
			{
				WeatherGraph.AddPoint(new Point(i, tempForecast[i]));
				WeatherGraph.AddLabel(precipForecast[i].ToString());
			}
			DateTime iDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
			for (int i = dateTime.Hour; i < dateTime.Hour + 12; i++)
			{
				WeatherGraph.AddHorzLabel(iDateTime.ToString("h tt"));
				iDateTime = iDateTime.AddHours(1);
			}
			WeatherGraph.Update();
		}

		// Given current date/time, updates the hour hand to reflect the hour
		public void SetHourHand(DateTime currDateTime)
		{
			RotateTransform rTransform = new RotateTransform();

			double hour = currDateTime.TimeOfDay.Hours + currDateTime.TimeOfDay.Minutes / 60.0;
			if (currDateTime.TimeOfDay.Hours > 12.0) // Army time
				hour -= 12.0;

			rTransform.Angle = hour / 12.0 * 360.0 - 90.0;
			HourHand.RenderTransform = rTransform;
		}

		// Given current date/time, updates the minute hand to reflect the minute
		public void SetMinuteHand(DateTime currDateTime)
		{
			RotateTransform rTransform = new RotateTransform();
			rTransform.Angle = currDateTime.TimeOfDay.Minutes / 60.0 * 360.0 - 90.0;
			MinuteHand.RenderTransform = rTransform;
		}


		// Updates the date, clock, and weather
		public void Update(object sender, object e)
		{
			UpdateDate();

			// If the minute changed
			if (currDateTime.Minute != prevDateTime.Minute)
			{
				// Update the hour/minute hand
				SetHourHand(currDateTime);
				SetMinuteHand(currDateTime);
				// Update the current conditions and 12 hour forecast
				UpdateCurrentConditions();
				Update12HrForecast();
			}
		}

		private void UpdateDate()
		{
			// Push back the state
			prevDateTime = currDateTime;
			// Update the current state
			currDateTime = DateTime.Now;

			// Only update the date when the day changes
			if (currDateTime.Day != prevDateTime.Day)
				SetDate(currDateTime);

			// Always update the time
			SetTime(currDateTime);
		}

		private void Update12HrForecast()
		{
			string errorMsg = WeatherController.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog
				return;
			}
			Set12HrForecast(currDateTime, WeatherController.GetHourlyForecast(), WeatherController.GetHourlyPrecipitation());
		}

		private void UpdateCurrentConditions()
		{
			string errorMsg = WeatherController.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog popup
				return;
			}
			SetCurrentConditions(WeatherController.GetCurrTemp(), WeatherController.GetConditionsMsg());
			errorMsg = WeatherController.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog popup
				return;
			}
			SetTodaysForecast(WeatherController.GetHighTemp(), WeatherController.GetLowTemp());
		}


		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// Initialize the date
			UpdateDate();

			// Update the hour/minute hand
			SetHourHand(currDateTime);
			SetMinuteHand(currDateTime);
			// Update the current conditions and 12 hour forecast
			UpdateCurrentConditions();
			Update12HrForecast();

			// Start the timer
			timer = new DispatcherTimer();
			timer.Tick += Update;
			timer.Interval = new TimeSpan(0, 0, 1);
			timer.Start();

			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			// Stop the timer
			timer.Tick -= Update;
			timer.Stop();

			base.OnNavigatedFrom(e);
		}


		// Temporary navigation control
		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(WeatherPage), null);
		}
	}
}