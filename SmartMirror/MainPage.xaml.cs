using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
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
        int weatherInt = -1000;

        public MainPage()
        {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            //ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
			//ApplicationView.GetForCurrentView().ExitFullScreenMode();

            // Set the date, time, etc
            currDateTime = DateTime.Now;
            month.Text = currDateTime.ToString("MMM", CultureInfo.InvariantCulture);
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
            time.Text = currDateTime.ToString("hh:mm tt");
            if (time.Text[0] == '0')
                time.Text = time.Text.Remove(0, 1);

            SetHourHand();
            SetMinuteHand();
            SetWeather();
            SetWeatherForecast();
            SetWeatherLine(currDateTime.Hour);

            // Setup the timer to update the date
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler<object>(UpdateDate);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        // Updates the date
        private void UpdateDate(object sender, object e)
        {
            // Push back the state
            prevDateTime = currDateTime;

            // Update the current state
            currDateTime = DateTime.Now;

            // If the month changed update the text
            if (currDateTime.Month != prevDateTime.Month)
                month.Text = currDateTime.ToString("MMM", CultureInfo.InvariantCulture);

			// If the day changed update the day text
            if (currDateTime.Day != prevDateTime.Day)
            {
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

            // If the minute changed
            if (currDateTime.Minute != prevDateTime.Minute)
            {
				// Update the time text
				time.Text = currDateTime.ToString("hh:mm tt");
				if (time.Text[0] == '0')
					time.Text = time.Text.Remove(0, 1);

				SetMinuteHand();
                SetHourHand();

				// Every 15 minutes update the weather
                if (currDateTime.Minute % 15 == 0)
                {
                    // Set the weather pollen count and forecast
                    SetWeather();
                    SetWeatherForecast();
                }

                // If the hour changes
                if (currDateTime.Hour != prevDateTime.Hour)
                    SetWeatherLine(currDateTime.Hour);
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

            double hour = currDateTime.TimeOfDay.Minutes;

            rTransform.Angle = hour / 60.0 * 360.0 - 90.0;
            MinuteHand.RenderTransform = rTransform;
        }


        // Requests and sets the current weather
        private void SetWeather()
        {
            WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/conditions/lang:EN/q/75002.json");

            // Try to get the weather from the wundergrund
            try
            {
                WebResponse response = request.GetResponseAsync().Result;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseStr = reader.ReadToEnd();

                // Acquire and parse the temperature
                string tempStr = responseStr.Substring(responseStr.IndexOf("temp_f") + 8);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(','));
                double temp = Double.Parse(tempStr);
                weatherInt = (int)Math.Round(temp);

                // Acquire and parse the forecast message
                string forecastMessage = responseStr.Substring(responseStr.IndexOf("\"weather\"") + 11);
                forecastMessage = forecastMessage.Substring(0, forecastMessage.IndexOf('\"'));
                weather.Text = forecastMessage + " at " + weatherInt.ToString() + (char)176;
            }
            catch (Exception e)
            {
                weather.Text = "Can't get weather data.";
            }
        }

        // Requests and sets the days forecast
        private void SetWeatherForecast()
        {
            WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/forecast/lang:EN/q/75002.json");

            // Try to get the forecast from the wunderground
            try
            {
                WebResponse response = request.GetResponseAsync().Result;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseStr = reader.ReadToEnd();

                // Acquire and parse the high and low forecast
                string highLowStr = responseStr.Substring(responseStr.IndexOf("\"day\""));
                string highTempStr = highLowStr.Substring(highLowStr.IndexOf("high") + 25);
                highTempStr = highTempStr.Substring(0, highTempStr.IndexOf('"'));
                string lowTempStr = highLowStr.Substring(highLowStr.IndexOf("low") + 24);
                lowTempStr = lowTempStr.Substring(0, lowTempStr.IndexOf('"'));

                // Snap High and Low next to weather text
                double posX = (double)weather.GetValue(Canvas.LeftProperty);
                weather.Measure(new Size(990, 101));
                double width = weather.ActualWidth / 2 + 200;

                weatherLow.Text = lowTempStr;
                weatherLow.SetValue(Canvas.LeftProperty, width);

                weatherHigh.Text = highTempStr;
                weatherHigh.SetValue(Canvas.LeftProperty, width);
            }
            catch (Exception e)
            {
                weatherLow.Text = "L";
                weatherHigh.Text = "H";
            }
        }

		// Rquests and sets the hourly forceast
        private void SetWeatherLine(int currentHr)
        {
            List<int> weather = new List<int>();
            weather.Add(weatherInt);
			Weather.GetHourlyForecast(currentHr);
			if (Weather.errorMessage != "")
				month.Text = Weather.errorMessage; // Temp
            weather.AddRange(Weather.hourlyTempList);

            // Find the max and min
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < weather.Count; i++)
            {
                if (weather[i] < min)
                    min = weather[i];
                if (weather[i] > max)
                    max = weather[i];
            }

            // Floor the min to the nearest 10
            int minFloor = (min / 10) * 10;
            // Ceil the max to the nearest 10
            int maxCeil = (max / 10 + 1) * 10;

            int numOfDividers = (maxCeil - minFloor) / 10;

            // We need a label for every divider and one for the beginning and end
            StartTempLabel.Text = maxCeil.ToString();

            WeatherGraphLabelGroup.Children.Clear();
            WeatherCanvas.Children.Clear();

            // Start at 1 because, for example, 3 divisions needs 2 dividers
            for (int i = 1; i < numOfDividers; i++)
            {
                // Place a divider
                Rectangle rect = new Rectangle();
                rect.Width = CopyRect.Width;
                rect.Height = CopyRect.Height;
                rect.Fill = new SolidColorBrush(Windows.UI.Colors.White);
                Canvas.SetLeft(rect, 0);
                Canvas.SetTop(rect, i * WeatherGraphBorder.Height / numOfDividers);
                WeatherCanvas.Children.Add(rect);

                // Place a label
                TextBlock label = new TextBlock();
                label.Width = StartTempLabel.Width;
                label.Height = StartTempLabel.Height;
                label.Foreground = StartTempLabel.Foreground;
                label.FontSize = StartTempLabel.FontSize;
                label.Text = (maxCeil - i * 10).ToString();
                label.FontFamily = StartTempLabel.FontFamily;
                Canvas.SetLeft(label, Canvas.GetLeft(StartTempLabel));
                Canvas.SetTop(label, Canvas.GetTop(StartTempLabel) + i * WeatherGraphBorder.Height / numOfDividers);
                WeatherGraphLabelGroup.Children.Add(label);
            }

            EndTempLabel.Text = minFloor.ToString();

            // Change the time labels
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

            
			List<Point> pts = new List<Point>();
            for (int i = 0; i < 12; i++)
            {
				// Get relative coordinates
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
			int[] indices = {
				0, 0, 1, 2,
				0, 1, 2, 3,
				1, 2, 3, 4,
				2, 3, 4, 5,
				3, 4, 5, 6,
				4, 5, 6, 7,
				5, 6, 7, 8,
				6, 7, 8, 9,
				7, 8, 9, 10,
				8, 9, 10, 11,
				9, 10, 11, 11
			};
			for (int i = 0; i < indices.Length; i += 4)
			{
				for (int j = 0; j < 20; j++)
				{
					pLine.Points.Add(catmullRom(pts[indices[i]], pts[indices[i + 1]], pts[indices[i + 2]], pts[indices[i + 3]], j / 20f));
				}
			}
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