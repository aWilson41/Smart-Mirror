using System;
using System.Globalization;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.UI;
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

			WeatherGraph12Hr.SetBounds(0, minFloor, 12, maxCeil);
			WeatherGraph12Hr.SetLineColor(Color.FromArgb(255, 135, 206, 235));
			WeatherGraph12Hr.SetLabelColor(Color.FromArgb(255, 255, 255, 255));
			WeatherGraph12Hr.SetLineThickness(8.0);
			WeatherGraph12Hr.SetLabelSuffix("%");
			WeatherGraph12Hr.SetFontSize(25.0);
			WeatherGraph12Hr.SetNumberOfSubdivisions(20);
			WeatherGraph12Hr.SetNumberOfDividers(0, numOfDiv);
			WeatherGraph12Hr.Clear();
			for (int i = 0; i < tempForecast.Count; i++)
			{
				WeatherGraph12Hr.AddPoint(new Point(i, tempForecast[i]));
				WeatherGraph12Hr.AddLabel(precipForecast[i].ToString());
			}
			WeatherGraph12Hr.Update();

			//// We need a label for every divider and one for the beginning and end
			StartTempLabel.Text = maxCeil.ToString();

			WeatherGraphLabelGroup.Children.Clear();

			// Start at 1 (3 divisions needs 2 dividers)
			for (int i = 1; i < numOfDiv + 1; i++)
			{
				// Place vertical temperature label
				TextBlock label = new TextBlock();
				label.Foreground = StartTempLabel.Foreground;
				label.FontSize = StartTempLabel.FontSize;
				label.Text = (maxCeil - i * 10).ToString();
				label.FontFamily = StartTempLabel.FontFamily;
				Canvas.SetLeft(label, Canvas.GetLeft(StartTempLabel));
				Canvas.SetTop(label, Canvas.GetTop(StartTempLabel) + i * WeatherGraphBorder.Height / (numOfDiv + 1));
				WeatherGraphLabelGroup.Children.Add(label);
			}

			EndTempLabel.Text = minFloor.ToString();

			// Place horizontal time labels
			for (int i = 0; i < WeatherTimeLabel.Children.Count; i++)
			{
				TextBlock label = WeatherTimeLabel.Children[i] as TextBlock;
				int hr = dateTime.Hour + i;
				// Adding i could cause the value to overflow
				if (hr > 24)
					hr -= 24;

				// Convert anything about 12 to 1-12
				string ttStr = "am";
				if (hr > 12)
				{
					hr -= 12;
					if (hr != 12)
						ttStr = "pm";
				}

				label.Text = hr.ToString() + ttStr;
			}
		}

		public void SetWeatherGIF()
		{

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
			WeatherGraph10Day.SetLabelColor(Color.FromArgb(255, 255, 255, 255));
			WeatherGraph10Day.SetLineThickness(8.0);
			WeatherGraph10Day.SetLabelSuffix("%");
			WeatherGraph10Day.SetFontSize(25.0);
			WeatherGraph10Day.SetNumberOfSubdivisions(20);
			WeatherGraph10Day.SetNumberOfDividers(0, numOfDiv);
			WeatherGraph10Day.Clear();
			for (int i = 0; i < tempForecast.Count; i++)
			{
				WeatherGraph10Day.AddPoint(new Point(i, tempForecast[i]));
				WeatherGraph10Day.AddLabel(precipForecast[i].ToString());
			}
			WeatherGraph10Day.Update();
		}


		private void Update12HrForecast()
		{
			string errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog
				return;
			}
			Set12HrForecast(DateTime.Now, Weather.GetHourlyForecast(), Weather.GetHourlyPrecipitation());
		}

		private void Update10DayForecast()
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
			Update12HrForecast();
			Update10DayForecast();

			// Start the timer
			//timer = new DispatcherTimer();
			//timer.Tick += Update;
			//timer.Interval = new TimeSpan(0, 0, 1);
			//timer.Start();

			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			// Stop the timer
			//timer.Tick -= Update;
			//timer.Stop();

			base.OnNavigatedFrom(e);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(CalendarPage), null);
		}
	}
}