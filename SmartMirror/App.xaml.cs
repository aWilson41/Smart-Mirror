using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SmartMirror
{
    sealed partial class App : Application
    {
		Frame frame;
		MainPage mainPage;
		WeatherPage weatherPage;

		DateTime currDateTime = new DateTime();
		DateTime prevDateTime = new DateTime();

		public App()
        {
            this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		public void Initialize()
		{
			// Initialize the date
			UpdateDate();
			mainPage.SetHourHand(currDateTime);
			mainPage.SetMinuteHand(currDateTime);
			// Initialize the weather
			UpdateWeather();
			Update12HrForecast();

			// Setup the timer to update the date continously
			DispatcherTimer timer = new DispatcherTimer();
			timer.Tick += new EventHandler<object>(Update);
			timer.Interval = new TimeSpan(0, 0, 1);
			timer.Start();
		}

		// Updates the clock
		public void Update(object sender, object e)
		{
			UpdateDate();

			// If the minute changed
			if (currDateTime.Minute != prevDateTime.Minute)
			{
				// Update the hour/minute hand
				mainPage.SetHourHand(currDateTime);
				mainPage.SetMinuteHand(currDateTime);

				// Every 15 minutes update the weather
				if (currDateTime.Minute % 15 == 0)
					UpdateWeather();

				// If the hour changes update the weatherline
				if (currDateTime.Hour != prevDateTime.Hour)
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
				mainPage.SetDate(currDateTime);

			// Always update the time
			mainPage.SetTime(currDateTime);
		}

		private void Update12HrForecast()
		{
			Weather.UpdateHourlyForecast(currDateTime.Hour);
			string errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog
				return;
			}
			mainPage.Set12HrForecast(currDateTime, Weather.GetForecast(), Weather.GetPrecipitation());
		}

		private void UpdateWeather()
		{
			// Update the current weather conditions
			Weather.UpdateWeather();
			string errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog popup
				return;
			}
			mainPage.SetCurrentConditions(Weather.GetCurrTemp(), Weather.GetForecastMsg());

			// Update the days forecast
			Weather.UpdateDaysForecast();
			errorMsg = Weather.GetErrorMsg();
			if (errorMsg != "")
			{
				// Add error dialog popup
				return;
			}
			mainPage.SetTodaysForecast(Weather.GetHighTemp(), Weather.GetLowTemp());
		}


		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
				this.DebugSettings.EnableFrameRateCounter = true;
			#endif
			frame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (frame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				frame = new Frame();

				frame.NavigationFailed += OnNavigationFailed;

				//if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				//{
				////TODO: Load state from previously suspended application
				//}

				// Place the frame in the current Window
				Window.Current.Content = frame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (frame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					frame.Navigate(typeof(MainPage), e.Arguments);
					mainPage = (MainPage)frame.Content;
					Initialize();
				}
				// Ensure the current window is active
				Window.Current.Activate();
			}
		}

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

    }
}