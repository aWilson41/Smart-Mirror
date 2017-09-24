using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace SmartMirror
{
    static class Weather
    {
		public static List<int> hourlyTempList = new List<int>();
		public static List<int> hourlyPrecipChance = new List<int>();
		public static string errorMessage = "";

        // Gets the forecast for the next 12 hours
        static public void GetHourlyForecast(int currentHrFloor)
        {
			errorMessage = "";
            WebRequest request = WebRequest.Create("http://api.wunderground.com/api/c1ae05b22ded2847/hourly/lang:EN/q/75002.json");

            // Try to get the forecast from the wunderground
            try
            {
                WebResponse response = request.GetResponseAsync().Result;
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseStr = reader.ReadToEnd();

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
				errorMessage = e.ToString();
            }
        }
    }
}