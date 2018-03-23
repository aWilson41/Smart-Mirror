using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace SmartMirror
{
	public sealed partial class Calendar : UserControl
	{
		// Box to go around current day (app freezes when setting canvas positions?)
		//Rectangle daySelection = new Rectangle();
		Dictionary<int, TextBlock> dayDesc = new Dictionary<int, TextBlock>();

		public Calendar()
		{
			InitializeComponent();

			DateTime now = DateTime.Now;
			string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month);
			MonthYearLabel.Text = monthName + ' ' + now.Year.ToString();

			//for some reason the app freezes when trying to SetTop and SetLeft of the rect ?
			//double size = Width / 7.0;
			//for (int i = 0; i < 7; i++)
			//{
			//	Rectangle vertRect = new Rectangle();
			//	vertRect.Width = 2;
			//	vertRect.Height = 1800;
			//	vertRect.Fill = new SolidColorBrush(Windows.UI.Colors.White);
			//	MainCanvas.Children.Add(vertRect);
			//	Canvas.SetTop(vertRect, 60);
			//	Canvas.SetLeft(vertRect, size * i);
			//}

			DateTime startDate = new DateTime(now.Year, now.Month, 1);
			DayOfWeek startDayOfWeek = startDate.DayOfWeek;
			DateTime endDate = startDate.AddMonths(1).AddDays(-1);

			for (int i = (int)startDayOfWeek; i < endDate.Day + (int)startDayOfWeek; i++)
			{
				int row = i / 7 + 1;
				int col = i % 7;
				int dayNum = i - (int)startDayOfWeek + 1;

				TextBlock dayLabel = new TextBlock();
				dayLabel.Foreground = MonthYearLabel.Foreground;
				dayLabel.Text = dayNum.ToString();
				dayLabel.FontFamily = MonthYearLabel.FontFamily;
				dayLabel.FontSize = 25.0;
				dayLabel.Margin = new Thickness(5.0, 0.0, 0.0, 0.0);
				dayLabel.HorizontalAlignment = HorizontalAlignment.Left;
				dayLabel.VerticalAlignment = VerticalAlignment.Top;
				MainGrid.Children.Add(dayLabel);
				Grid.SetColumn(dayLabel, col);
				Grid.SetRow(dayLabel, row);

				TextBlock dayDescLabel = new TextBlock();
				dayDescLabel.Foreground = MonthYearLabel.Foreground;
				dayDescLabel.FontFamily = MonthYearLabel.FontFamily;
				dayDescLabel.FontSize = 18.0;
				dayDescLabel.Margin = new Thickness(5.0, 25.0, 0.0, 0.0);
				dayDescLabel.HorizontalAlignment = HorizontalAlignment.Left;
				dayDescLabel.VerticalAlignment = VerticalAlignment.Top;
				MainGrid.Children.Add(dayDescLabel);
				Grid.SetColumn(dayDescLabel, col);
				Grid.SetRow(dayDescLabel, row);
				dayDesc.Add(dayNum, dayDescLabel);
			}

			DispatcherTimer daysForecastTimer = new DispatcherTimer();
			daysForecastTimer.Tick += Update;
			// Update the calendar every 12 hours
			daysForecastTimer.Interval = new TimeSpan(12, 0, 0);
			daysForecastTimer.Start();
		}


		public void Update(object sender, object args)
		{
			DateTime now = DateTime.Now;
			string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month);
			MonthYearLabel.Text = monthName + ' ' + now.Year.ToString();
		}


		// Sets the textblock of the specified day
		public void SetText(int day, string str)
		{
			dayDesc[day].Text = str;
		}

		// Adds an event to the day's textblock
		public void AddEvent(int day, string str)
		{
			dayDesc[day].Text += str + '\n';
			SortEvents(day);
		}

		// Removes the event by day and name
		public void RemoveEvent(int day, string str)
		{
			string[] splitStr = dayDesc[day].Text.Split('\n');
			dayDesc[day].Text = "";
			for (int i = 0; i < splitStr.Length; i++)
			{
				if (splitStr[i] != str && splitStr[i] != "")
					dayDesc[day].Text += splitStr[i] + '\n';
			}
		}

		// Removes the event by day and index
		public void RemoveEvent(int day, int n)
		{
			string[] splitStr = dayDesc[day].Text.Split('\n');
			dayDesc[day].Text = "";
			for (int i = 0; i < splitStr.Length; i++)
			{
				if (i != n && splitStr[i] != "")
					dayDesc[day].Text += splitStr[i] + '\n';
			}
		}

		public void SortEvents(int day)
		{
			List<string> splitStr = new List<string>(dayDesc[day].Text.Split('\n'));
			splitStr.RemoveAt(splitStr.Count - 1); // Remove the last one as it should be ""
			splitStr.Sort();
			dayDesc[day].Text = "";
			for (int i = 0; i < splitStr.Count; i++)
			{
				dayDesc[day].Text += splitStr[i] + '\n';
			}
		}
	}
}