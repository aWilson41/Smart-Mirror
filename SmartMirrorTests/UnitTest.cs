using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.UI.Core;

using SmartMirror;

namespace SmartMirrorTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHourlyForecast()
        {
			List<int> hourlyForecast = new List<int>();
			List<int> precipChance = new List<int>();
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				Weather.UpdateHourlyForecast(null, null);
				hourlyForecast = Weather.GetHourlyForecast();
				precipChance = Weather.GetHourlyPrecipitation();
			}).AsTask().Wait();

			// There should be exactly 12 entires in the hourly forecast
			Assert.AreEqual(12, hourlyForecast.Count);
			Assert.AreEqual(12, precipChance.Count);

			// Every forecast temperature can be any integer

			// Every precipitation must be a precentage [0,100]
			for (int i = 0; i < precipChance.Count; i++)
			{
				Assert.IsTrue(precipChance[i] <= 100);
				Assert.IsTrue(precipChance[i] >= 0);
			}
		}

		[TestMethod]
		public void TestCurrentConditions()
		{
			// Test the forecast message
			string conditionMsg = "";
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				Weather.UpdateCurrentConditions(null, null);
				conditionMsg = Weather.GetConditionsMsg();
			}).AsTask().Wait();

			Assert.IsTrue(conditionMsg != "");
		}

		[TestMethod]
		public void Test10DayForecast()
		{
			List<int> dailyForecast = new List<int>();
			List<int> precipChance = new List<int>();
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				Weather.Update10DayForecast(null, null);
				dailyForecast = Weather.GetDayForecast();
				precipChance = Weather.GetDayPrecipitation();
			}).AsTask().Wait();

			// There should be exactly 12 entires in the daily forecast
			Assert.AreEqual(10, dailyForecast.Count);
			Assert.AreEqual(10, precipChance.Count);

			// Every forecast temperature can be any integer

			// Every precipitation must be a precentage [0,100]
			for (int i = 0; i < precipChance.Count; i++)
			{
				Assert.IsTrue(precipChance[i] <= 100);
				Assert.IsTrue(precipChance[i] >= 0);
			}
		}
    }
}