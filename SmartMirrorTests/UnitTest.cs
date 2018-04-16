using System;
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
			UserAccount.saveSetting("zipcode", "75002");

			List<int> hourlyForecast = new List<int>();
			List<int> precipChance = new List<int>();
			string errorMessage = "";
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				WeatherController.UpdateHourlyForecast(null, null);
				hourlyForecast = WeatherController.GetHourlyForecast();
				precipChance = WeatherController.GetHourlyPrecipitation();
				errorMessage = WeatherController.GetErrorMsg();
			}).AsTask().Wait();

			// There should be exactly 12 entires in the hourly forecast so we test value less than 12, 12, and greater than 12
			Assert.AreNotEqual(14, hourlyForecast.Count);
			Assert.AreEqual(12, hourlyForecast.Count);
			Assert.AreNotEqual(9, hourlyForecast.Count);

			Assert.AreNotEqual(14, precipChance.Count);
			Assert.AreEqual(12, precipChance.Count);
			Assert.AreNotEqual(9, precipChance.Count);

			// Every forecast temperature can be any integer

			// Every precipitation must be a precentage [0,100]
			for (int i = 0; i < precipChance.Count; i++)
			{
				// Test in range [0, 100] and out of the range
				Assert.IsTrue(precipChance[i] <= 100 && precipChance[i] >= 0);
				Assert.IsFalse(precipChance[i] > 100);
				Assert.IsFalse(precipChance[i] < 0);
			}

			// There is an error message if we failed to make the webrequest
			Assert.IsTrue(errorMessage == "");
		}

		[TestMethod]
		public void TestCurrentConditions()
		{
			UserAccount.saveSetting("zipcode", "75002");

			// Test the forecast message
			string conditionMsg = "";
			string errorMessage = "";
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				WeatherController.UpdateCurrentConditions(null, null);
				conditionMsg = WeatherController.GetConditionsMsg();
				errorMessage = WeatherController.GetErrorMsg();
			}).AsTask().Wait();

			Assert.IsTrue(conditionMsg != "");

			// There is an error message if we failed to make the webrequest
			Assert.IsTrue(errorMessage == "");
		}

		[TestMethod]
		public void Test10DayForecast()
		{
			UserAccount.saveSetting("zipcode", "75002");

			List<int> dailyForecast = new List<int>();
			List<int> precipChance = new List<int>();
			string errorMessage = "";
			Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() =>
			{
				WeatherController.Update10DayForecast(null, null);
				dailyForecast = WeatherController.GetDayForecast();
				precipChance = WeatherController.GetDayPrecipitation();
				errorMessage = WeatherController.GetErrorMsg();
			}).AsTask().Wait();

			// There should be exactly 12 entires in the hourly forecast so we test value less than 12, 12, and greater than 12
			Assert.AreNotEqual(14, dailyForecast.Count);
			Assert.AreEqual(10, dailyForecast.Count);
			Assert.AreNotEqual(9, dailyForecast.Count);

			Assert.AreNotEqual(14, precipChance.Count);
			Assert.AreEqual(10, precipChance.Count);
			Assert.AreNotEqual(9, precipChance.Count);

			// Every forecast temperature can be any integer

			// Every precipitation must be a precentage [0,100]
			for (int i = 0; i < precipChance.Count; i++)
			{
				// Test in range [0, 100] and out of the range
				Assert.IsTrue(precipChance[i] <= 100 && precipChance[i] >= 0);
				Assert.IsFalse(precipChance[i] > 100);
				Assert.IsFalse(precipChance[i] < 0);
			}

			// There is an error message if we failed to make the webrequest
			Assert.IsTrue(errorMessage == "");
		}
	}
}