namespace wiringpi.net.sensors.sample
{
	using System;

	[Sample(Id = 2, Name = "HCSR04 sensor")]
	class HCSR04Sample : LoopSample, IDisposable
	{
		private int _trigPin;
		private int _echoPin;
		private HCSR04Sensor _sensor;

		protected override int LoopSleepTime => 0;

		protected override void DefinePins(int[] possiblePins)
		{
			_trigPin = possiblePins.Length > 0 ? possiblePins[0] : GC2xHATConst.DefaultHCSR04TRIGPin;
			_echoPin = possiblePins.Length > 1 ? possiblePins[1] : GC2xHATConst.DefaultHCSR04ECHOPin;

			_sensor = new HCSR04Sensor(_trigPin, _echoPin);
		}

		protected override void LoopImplementation()
		{
			_sensor.StartDistanceMeas();
			//maxium distance 80ms
			Sleep(80);
			if (_sensor.IsDataValid())
			{
				Console.WriteLine("distance: {0} cm (time elapsed: {1} ms)",
						_sensor.Distance, _sensor.DistanceTime * 1000.0f);
				Sleep(30);
			}
		}

		public void Dispose()
		{
			_sensor?.Dispose();
		}
	}
}