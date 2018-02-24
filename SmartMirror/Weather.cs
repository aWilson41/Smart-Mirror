using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace SmartMirror
{
	// TODO: Rework the forecast functionality. Shouldn't be storing the data like this
	static class Weather
	{
		// 12 hour forecast
		private static List<int> hourTempList = new List<int>();
		private static List<int> hourPrecipChance = new List<int>();

		// 12 day forecast
		private static List<int> dayTempList = new List<int>();
		private static List<int> dayPrecipChance = new List<int>();

		// Day's forecast (high/low)
		private static int highTemp = 0;
		private static int lowTemp = 0;

		// Current temp and conditions
		private static int currTemp = 0;
		private static string forecastMessage = "";

		private static string errorMessage = "";

		public static int GetHighTemp() { return highTemp; }
		public static int GetLowTemp() { return lowTemp; }
		public static int GetCurrTemp() { return currTemp; }
		public static string GetForecastMsg() { return forecastMessage; }
		public static string GetErrorMsg() { return errorMessage; }
		public static List<int> GetForecast() { return hourTempList; }
		public static List<int> GetPrecipitation() { return hourPrecipChance; }

		// Updates the forecast for the next 12 hours
		static public void Update12HrForecast(int currentHrFloor)
		{
			errorMessage = "";
			WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/hourly/lang:EN/q/75002.json");

			// Try to get the forecast from wunderground
			try
			{
				WebResponse response = request.GetResponseAsync().Result;
				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				string responseStr = reader.ReadToEnd();
				hourlyTempList.Clear();
				hourlyPrecipChance.Clear();

				hourlyTempList.Add(currTemp);
				// For the next 11 hours Ceilinged so 12:36am will only read hour 1am
				for (int hr = currentHrFloor; hr < currentHrFloor + 12; hr++)
				{
					responseStr = responseStr.Substring(responseStr.IndexOf("temp") + 20);
					int temp = int.Parse(responseStr.Substring(0, responseStr.IndexOf('"')));
					hourlyTempList.Add(temp);

					responseStr = responseStr.Substring(responseStr.IndexOf("pop") + 7);
					
					int pop = (int)Double.Parse(responseStr.Substring(0, responseStr.IndexOf('"')));
					hourlyPrecipChance.Add(pop);
				}
			}
			catch (Exception e)
			{
				errorMessage = "Can't get hourly forecast";
			}
		}

		// Updates the current temperature and conditions string
		public static void UpdateWeather()
		{
			errorMessage = "";
			WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/conditions/lang:EN/q/75002.json");

			// Try to get the weather from the wundergrund
			try
			{
				WebResponse response = request.GetResponseAsync().Result;
				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				string responseStr = reader.ReadToEnd();

				if (responseStr != "")
				{
					// Acquire and parse the temperature
					string tempStr = responseStr.Substring(responseStr.IndexOf("temp_f") + 8);
					tempStr = tempStr.Substring(0, tempStr.IndexOf(','));
					double temp = Double.Parse(tempStr);
					currTemp = (int)Math.Round(temp);

					// Acquire and parse the forecast message
					forecastMessage = responseStr.Substring(responseStr.IndexOf("\"weather\"") + 11);
					forecastMessage = forecastMessage.Substring(0, forecastMessage.IndexOf('\"'));
				}
			}
			catch (Exception e)
			{
				errorMessage = "Can't get weather data";
			}
		}

		// Updates the days (24hr) forecast (high/low)
		public static void UpdateDaysForecast()
		{
			errorMessage = "";
			WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/forecast/lang:EN/q/75002.json");

			// Try to get the forecast from the wunderground
			try
			{
				WebResponse response = request.GetResponseAsync().Result;
				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				string responseStr = reader.ReadToEnd();

				if (responseStr != "")
				{
					// Acquire and parse the high and low forecast
					string highLowStr = responseStr.Substring(responseStr.IndexOf("\"day\""));
					string highTempStr = highLowStr.Substring(highLowStr.IndexOf("high") + 25);
					highTemp = (int)Math.Round(Double.Parse(highTempStr.Substring(0, highTempStr.IndexOf('"'))));
					string lowTempStr = highLowStr.Substring(highLowStr.IndexOf("low") + 24);
					lowTemp = (int)Math.Round(Double.Parse(lowTempStr.Substring(0, lowTempStr.IndexOf('"'))));
				}
			}
			catch (Exception e)
			{
				errorMessage = "Can't get day forecast";
			}
		}

		public static void UpdateWeatherGIF()
		{

		}

		public static void Update12DayForecast()
		{

		}

	}
}