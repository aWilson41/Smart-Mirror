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
		WeatherPage weatherPage; // Not yet implemented

		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			frame = Window.Current.Content as Frame;

			// If the frame hasn't been created yet then create it
			if (frame == null)
			{
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
					// Navigate to a new main page and save that main page
					frame.Navigate(typeof(MainPage), e.Arguments);
					mainPage = (MainPage)frame.Content;
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