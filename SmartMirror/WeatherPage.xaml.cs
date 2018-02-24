using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SmartMirror
{
	public sealed partial class WeatherPage : Page
	{
		public WeatherPage()
		{
			this.InitializeComponent();
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

		}

		public void SetWeatherGIF()
		{

		}

		public void Set12DayForecast()
		{

		}
	}
}