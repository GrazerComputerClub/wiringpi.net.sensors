namespace wiringpi.net.sensors.sample
{
	using System;
	using System.Diagnostics;
	using System.Threading;

	abstract class LoopSample : IStopSample
	{
		private static volatile bool _isSampleRunning = false;

		protected virtual int LoopSleepTime { get; } = 10;

		public void Run(int[] possiblePins)
		{
			DefinePins(possiblePins);
			_isSampleRunning = true;
			try
			{
				while (_isSampleRunning)
				{
					LoopImplementation();
					Sleep(LoopSleepTime);
				}
			}
			finally
			{
				_isSampleRunning = false;
			}
		}

		public void Stop()
		{
			_isSampleRunning = false;
		}

		protected abstract void DefinePins(int[] possiblePins);

		protected abstract void LoopImplementation();

		protected virtual void Sleep(int millisecondsTimeout)
		{
			if (_isSampleRunning && millisecondsTimeout > 0)
			{
				Thread.Sleep(millisecondsTimeout);
			}
		}

		protected virtual int SleepAndCheckPins(int millisecondsTimeout, params int[] pins)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < millisecondsTimeout)
			{
				foreach (int pin in pins)
				{
					if (WiringPi.DigitalRead(pin) == 1)
					{
						Console.WriteLine("Pin {0} is active", pin);
						return pin;
					}
				}
				Sleep(LoopSleepTime);
			}
			return -1;
		}
	}
}