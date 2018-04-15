using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace SmartMirror
{
	public sealed partial class WeatherPage : Page
	{
		DispatcherTimer timer;

		public WeatherPage()
		{
			InitializeComponent();
			DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
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

			WeatherGraph12Hr.SetBounds(0, minFloor, 11, maxCeil);
			WeatherGraph12Hr.SetLineColor(Color.FromArgb(255, 135, 206, 235));
			WeatherGraph12Hr.SetLineThickness(8.0);
			WeatherGraph12Hr.SetLabelSuffix("%");
			WeatherGraph12Hr.SetVertLabelSuffix(((char)176).ToString());
			WeatherGraph12Hr.SetPointFontSize(25.0);
			WeatherGraph12Hr.SetNumberOfSubdivisions(20);
			WeatherGraph12Hr.SetNumberOfDividers(10, numOfDiv);
			WeatherGraph12Hr.SetDividerVisibility(true, true);
			WeatherGraph12Hr.Clear();
			for (int i = 0; i < tempForecast.Count; i++)
			{
				WeatherGraph12Hr.AddPoint(new Point(i, tempForecast[i]));
				WeatherGraph12Hr.AddLabel(precipForecast[i].ToString());
			}
			for (int i = dateTime.Hour; i < dateTime.Hour + 12; i++)
			{
				WeatherGraph12Hr.AddHorzLabel(new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, i, 0, 0).ToString("h tt"));
			}
			WeatherGraph12Hr.Update();
		}

		public void Set10DayForecast(DateTime dateTime, List<int> tempForecast, List<int> precipForecast)
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

			WeatherGraph10Day.SetBounds(0, minFloor, 9, maxCeil);
			WeatherGraph10Day.SetLineColor(Color.FromArgb(255, 180, 120, 70));
			WeatherGraph10Day.SetLineThickness(8.0);
			WeatherGraph10Day.SetLabelSuffix("%");
			WeatherGraph10Day.SetVertLabelSuffix(((char)176).ToString());
			WeatherGraph10Day.SetPointFontSize(25.0);
			WeatherGraph10Day.SetNumberOfSubdivisions(20);
			WeatherGraph10Day.SetNumberOfDividers(8, numOfDiv);
			WeatherGraph10Day.SetDividerVisibility(true, true);
			WeatherGraph10Day.Clear();
			for (int i = 0; i < tempForecast.Count; i++)
			{
				WeatherGraph10Day.AddPoint(new Point(i, tempForecast[i]));
				WeatherGraph10Day.AddLabel(precipForecast[i].ToString());
			}
			WeatherGraph10Day.Update();
		}


		private void Update12HrForecast(object sender, object args)
		{
			string errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog
				return;
			}
			Set12HrForecast(DateTime.Now, Weather.GetHourlyForecast(), Weather.GetHourlyPrecipitation());
		}

		private void Update10DayForecast(object sender, object args)
		{
			string errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog
				return;
			}
			Set10DayForecast(DateTime.Now, Weather.GetDayForecast(), Weather.GetDayPrecipitation());
		}


		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// Manually update the weather
			Update12HrForecast(null, null);
			Update10DayForecast(null, null);

			// Start the timer to update it every hour
			timer = new DispatcherTimer();
			timer.Tick += Update12HrForecast;
			timer.Tick += Update10DayForecast;
			timer.Interval = new TimeSpan(1, 0, 0);
			timer.Start();

			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			// Stop the timer
			//timer.Tick -= Update;
			//timer.Stop();

			base.OnNavigatedFrom(e);
		}


		// Temporary navigation control
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(CalendarPage), null);
		}
	}
}