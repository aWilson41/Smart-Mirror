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
		GPIOButtonListener buttonListener = new GPIOButtonListener();
		int currPage = 0;
        HttpServer server;

		public App()
		{
			InitializeComponent();
			Suspending += OnSuspending;

			// Setup button for listening
			buttonListener.StartListener();
			// hook up button pressed to cyclepage
			buttonListener.ButtonPressed += CyclePage;

            // Initialize web server
            server = new HttpServer(8090);

            // Set default settings
            Object zipcode = UserAccount.getSetting("zipcode");
            if (zipcode == null)
            {
                UserAccount.saveSetting("zipcode", "94105");
            }
		}

		// Cycles through navigation
		protected void CyclePage(object sender, EventArgs e)
		{
			currPage = (currPage + 1) % 3;

			if (currPage == 0)
				frame.Navigate(typeof(MainPage), null);
			else if (currPage == 1)
				frame.Navigate(typeof(WeatherPage), null);
			else if (currPage == 2)
				frame.Navigate(typeof(CalendarPage), null);
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			// Template junk for setting up initial page
			frame = Window.Current.Content as Frame;

			// If the frame hasn't been created yet then create it
			if (frame == null)
			{
				frame = new Frame();
				frame.NavigationFailed += OnNavigationFailed;

				//if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				//{
				//// TODO: Load state from previously suspended application
				//}

				// Place the frame in the current Window
				Window.Current.Content = frame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (frame.Content == null)
				{
					// On app start we should update the weather
					Weather.UpdateCurrentConditions(null, null);
					Weather.UpdateTodaysForecast(null, null);
					Weather.UpdateHourlyForecast(null, null);
					Weather.Update10DayForecast(null, null);

					// Navigate to a new main page and save that main page
					frame.Navigate(typeof(MainPage), e.Arguments);
				}
				// Ensure the current window is active
				Window.Current.Activate();
			}
		}

		protected void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		protected void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			// TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}