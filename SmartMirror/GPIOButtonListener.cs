using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace SmartMirror
{
	// For rpi3 connect button to pin 12 (gpio 18) and 6 (ground)
	// Emits ButtonPressed when button goes down
	class GPIOButtonListener
	{
		const int BUTTON_PIN = 18;
		GpioPin pin;
		bool currState = false;
		bool prevState = false;

		public delegate void ButtonPressEventHandler(object sender, EventArgs e);
		public event ButtonPressEventHandler ButtonPressed;

		// Returns if succesfully initialized gpio and began listening
		public bool StartListener()
		{
			// If the gpio was succesfully initialized then start listening to it
			if (InitGPIO())
			{
				DispatcherTimer timer = new DispatcherTimer();
				timer.Tick += Tick;
				timer.Interval = new TimeSpan(200);
				timer.Start();
				return true;
			}
			else
				return false;
		}

		private bool InitGPIO()
		{
			var gpio = GpioController.GetDefault();

			// Show an error if there is no GPIO controller
			if (gpio == null)
				return false;

			pin = gpio.OpenPin(BUTTON_PIN);
			pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
			return true;
		}

		void Tick(object sender, object e)
		{
			// Push the current state back
			prevState = currState;
			// Update to the new one
			if (pin.Read() == GpioPinValue.High)
				currState = true;
			else
				currState = false;

			// If the button Goes from not down to down
			if (currState != prevState && !currState)
				ButtonPressed?.Invoke(this, new EventArgs());
		}
	}
}