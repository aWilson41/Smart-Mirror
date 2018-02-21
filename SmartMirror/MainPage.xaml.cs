using System;
using System.Globalization;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.Foundation;

namespace SmartMirror
{
    public sealed partial class MainPage : Page
    {
        DateTime currDateTime = new DateTime();
        DateTime prevDateTime = new DateTime();

        public MainPage()
        {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

			ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
			//ApplicationView.GetForCurrentView().ExitFullScreenMode();

			// Update the date text (month/date/time)
			UpdateDate();

			// Set the hour and minute hand
            SetHourHand();
            SetMinuteHand();
			// Set the weather and the weather forecast
			UpdateWeather();
            UpdateWeatherLine(currDateTime.Hour);

			// Setup the timer to update the date continously
			DispatcherTimer timer = new DispatcherTimer();
			timer.Tick += new EventHandler<object>(Update);
			timer.Interval = new TimeSpan(0, 0, 1);
			timer.Start();
		}

        // Updates the clock
        private void Update(object sender, object e)
        {
			UpdateDate();

            // If the minute changed
            if (currDateTime.Minute != prevDateTime.Minute)
            {
				// Update the hour/minute hand
				SetMinuteHand();
                SetHourHand();

				// Every 15 minutes update the weather
				if (currDateTime.Minute % 15 == 0)
					UpdateWeather();

				// If the hour changes update the weatherline
				if (currDateTime.Hour != prevDateTime.Hour)
					UpdateWeatherLine(currDateTime.Hour);
			}
        }

		private void UpdateDate()
		{
			// Push back the state
			prevDateTime = currDateTime;
			// Update the current state
			currDateTime = DateTime.Now;

			// Don't waste time on this if the day hasn't changed
			if (currDateTime.Day != prevDateTime.Day)
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
			time.Text = currDateTime.ToString("hh:mm tt");
			// Remove preceding zero (ie first zero in 01:00)
			if (time.Text[0] == '0')
				time.Text = time.Text.Remove(0, 1);
		}

		private void UpdateWeather()
		{
			// Set the current conditions and forecast
			Weather.UpdateWeather();
			if (Weather.errorMessage != "")
			{
				month.Text = Weather.errorMessage;
				return;
			}
			weather.Text = Weather.forecastMessage + " at " + Weather.currTemp.ToString() + (char)176;

			Weather.UpdateDaysForecast();
			if (Weather.errorMessage != "")
			{
				month.Text = Weather.errorMessage;
				return;
			}
			// Snap High and Low next to weather text
			double posX = (double)weather.GetValue(Canvas.LeftProperty);
			weather.Measure(new Size(990, 101));
			double width = weather.ActualWidth * 0.5 + 200;

			weatherLow.Text = Weather.lowTemp.ToString();
			weatherLow.SetValue(Canvas.LeftProperty, width);

			weatherHigh.Text = Weather.highTemp.ToString();
			weatherHigh.SetValue(Canvas.LeftProperty, width);
		}

		// Updates the weather line from the current hour
		private void UpdateWeatherLine(int currentHr)
		{
			Weather.UpdateHourlyForecast(currentHr);
			if (Weather.errorMessage != "")
			{
				month.Text = Weather.errorMessage;
				return;
			}
			List<int> weather = new List<int>(Weather.hourlyTempList);
			if (weather.Count <= 0)
				return;

			// Find the max and min temperature
			int min = weather[0];
			int max = weather[0];
			for (int i = 1; i < weather.Count; i++)
			{
				if (weather[i] < min)
					min = weather[i];
				if (weather[i] > max)
					max = weather[i];
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
				int hr = currentHr + i;
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
				double y = WeatherGraphBorder.Height - (weather[i] - minFloor) * WeatherGraphBorder.Height / (maxCeil - minFloor);
				pts.Add(new Point(x, y));

				Ellipse circle = new Ellipse();
				circle.Fill = new SolidColorBrush(Windows.UI.Colors.LightSteelBlue);
				circle.Width = 16;
				circle.Height = 16;
				Canvas.SetLeft(circle, x - 8);
				Canvas.SetTop(circle, y - 8);
				WeatherCanvas.Children.Add(circle);

				TextBlock popText = new TextBlock();
				popText.Text = Weather.hourlyPrecipChance[i].ToString() + "%";
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

		private void SetHourHand()
		{
			RotateTransform rTransform = new RotateTransform();

			double hour = currDateTime.TimeOfDay.Hours + currDateTime.TimeOfDay.Minutes / 60.0;
			if (currDateTime.TimeOfDay.Hours > 12.0) // Army time
				hour -= 12.0;

			rTransform.Angle = hour / 12.0 * 360.0 - 90.0;
			HourHand.RenderTransform = rTransform;
		}

		private void SetMinuteHand()
		{
			RotateTransform rTransform = new RotateTransform();
			rTransform.Angle = currDateTime.TimeOfDay.Minutes / 60.0 * 360.0 - 90.0;
			MinuteHand.RenderTransform = rTransform;
		}

		// Catmull rom for weather curves
		static Point catmullRom(Point p0, Point p1, Point p2, Point p3, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return new Point(0.5f * ((2.0f * p1.X) + (-p0.X + p2.X) * t + (2.0f * p0.X - 5.0f * p1.X + 4.0f * p2.X - p3.X) * t2 + (-p0.X + 3.0f * p1.X - 3.0f * p2.X + p3.X) * t3),
				0.5f * ((2.0f * p1.Y) + (-p0.Y + p2.Y) * t + (2.0f * p0.Y - 5.0f * p1.Y + 4.0f * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0f * p1.Y - 3.0f * p2.Y + p3.Y) * t3));
		}
	}
}