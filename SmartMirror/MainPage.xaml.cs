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
			if (tempForecast.Count <= 0 || precipForecast.Count <= 0)
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
			int numOfDiv = (maxCeil - minFloor) / 10;
			if (numOfDiv <= 0)
				return;

			// We need a label for every divider and one for the beginning and end
			StartTempLabel.Text = maxCeil.ToString();

			WeatherGraphLabelGroup.Children.Clear();
			WeatherCanvas.Children.Clear();

			// Start at 1 (3 divisions needs 2 dividers)
			for (int i = 1; i < numOfDiv; i++)
			{
				// Place a divider
				Rectangle rect = new Rectangle();
				rect.Width = CopyRect.Width;
				rect.Height = CopyRect.Height;
				rect.Fill = new SolidColorBrush(Windows.UI.Colors.White);
				Canvas.SetLeft(rect, 0);
				Canvas.SetTop(rect, i * WeatherGraphBorder.Height / numOfDiv);
				WeatherCanvas.Children.Add(rect);

				// Place vertical temperature label
				TextBlock label = new TextBlock();
				label.Width = StartTempLabel.Width;
				label.Height = StartTempLabel.Height;
				label.Foreground = StartTempLabel.Foreground;
				label.FontSize = StartTempLabel.FontSize;
				label.Text = (maxCeil - i * 10).ToString();
				label.FontFamily = StartTempLabel.FontFamily;
				Canvas.SetLeft(label, Canvas.GetLeft(StartTempLabel));
				Canvas.SetTop(label, Canvas.GetTop(StartTempLabel) + i * WeatherGraphBorder.Height / numOfDiv);
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

			// Create the weather points and precip chance labels
			List<Point> pts = new List<Point>();
			for (int i = 0; i < 12; i++)
			{
				double x = i * WeatherGraphBorder.Width / 11.0;
				double y = WeatherGraphBorder.Height - (tempForecast[i] - minFloor) * WeatherGraphBorder.Height / (maxCeil - minFloor);
				pts.Add(new Point(x, y));

				Ellipse circle = new Ellipse();
				circle.Fill = new SolidColorBrush(Windows.UI.Colors.LightSteelBlue);
				circle.Width = 16;
				circle.Height = 16;
				Canvas.SetLeft(circle, x - 8);
				Canvas.SetTop(circle, y - 8);
				WeatherCanvas.Children.Add(circle);

				TextBlock popText = new TextBlock();
				popText.Text = precipForecast[i].ToString() + "%";
				popText.FontFamily = new FontFamily("Assets/AGENCYB.TTF#Agency FB");
				popText.FontSize = 20.0;
				popText.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
				Canvas.SetLeft(popText, x);
				Canvas.SetTop(popText, y - 40);
				WeatherCanvas.Children.Add(popText);
			}

			// Create a polyline
			Polyline pLine = new Polyline();
			pLine.Stroke = new SolidColorBrush(Windows.UI.Colors.LightBlue);
			pLine.StrokeThickness = 8;
			WeatherCanvas.Children.Add(pLine);
			// 20 samples along the curve
			for (int j = 0; j < 20; j++)
			{
				pLine.Points.Add(catmullRom(pts[0], pts[0], pts[1], pts[2], j / 20f));
			}
			for (int i = 0; i < 8; i++)
			{
				// 20 samples along the curve
				for (int j = 0; j < 20; j++)
				{
					pLine.Points.Add(catmullRom(pts[i], pts[i + 1], pts[i + 2], pts[i + 3], j / 20f));
				}
			}
			// 20 samples along the curve
			for (int j = 0; j < 20; j++)
			{
				pLine.Points.Add(catmullRom(pts[9], pts[10], pts[11], pts[11], j / 20f));
			}
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


		// Updates the clock
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
			// Only update it if it's out of date
			if (DateTime.Now.TimeOfDay.TotalMinutes - Weather.GetLastUpdated12HrForecast().TimeOfDay.TotalMinutes > 15)
			{
				Weather.Update12HrForecast(currDateTime.Hour);
				string errorMsg = Weather.GetErrorMsg();
				if (errorMsg != "")
				{
					// Add error dialog
					return;
				}
			}
			Set12HrForecast(currDateTime, Weather.GetForecast(), Weather.GetPrecipitation());
		}

		private void UpdateCurrentConditions()
		{
			// Update the current weather conditions as long as it's out of date (by 15mins)
			if (DateTime.Now.TimeOfDay.TotalMinutes - Weather.GetLastUpdatedCurrentConditions().TimeOfDay.TotalMinutes > 15)
			{
				Weather.UpdateCurrentConditions();
				string errorMsg = Weather.GetErrorMsg();
				if (errorMsg != "")
				{
					// Add error dialog popup
					return;
				}
			}
			SetCurrentConditions(Weather.GetCurrTemp(), Weather.GetForecastMsg());

			// Update the days forecast as long as it's out of date (by 60mins)
			if (DateTime.Now.TimeOfDay.TotalMinutes - Weather.GetLastUpdatedDaysForecast().TimeOfDay.TotalMinutes > 60)
			{
				Weather.UpdateDaysForecast();
				string errorMsg = Weather.GetErrorMsg();
				if (errorMsg != "")
				{
					// Add error dialog popup
					return;
				}
				SetTodaysForecast(Weather.GetHighTemp(), Weather.GetLowTemp());
			}
		}

		// Catmull rom for weather curves (could possibly go in a separate "mathhelper" 
		// class but why make a mathehelper class with one function in it)
		static Point catmullRom(Point p0, Point p1, Point p2, Point p3, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return new Point(0.5f * ((2.0f * p1.X) + (-p0.X + p2.X) * t + (2.0f * p0.X - 5.0f * p1.X + 4.0f * p2.X - p3.X) * t2 + (-p0.X + 3.0f * p1.X - 3.0f * p2.X + p3.X) * t3),
				0.5f * ((2.0f * p1.Y) + (-p0.Y + p2.Y) * t + (2.0f * p0.Y - 5.0f * p1.Y + 4.0f * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0f * p1.Y - 3.0f * p2.Y + p3.Y) * t3));
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


		// Temporary
		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame frame = Window.Current.Content as Frame;
			frame.Navigate(typeof(WeatherPage), null);
		}
	}
}