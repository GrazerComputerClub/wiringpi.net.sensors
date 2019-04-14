namespace wiringpi.net.sensors.sample
{
	using System;

	[Sample(Id = 1, Name = "DHT sensor")]
	class DhtSample : ISample
	{
		public void Run(int[] possiblePins)
		{
			int dataPin = possiblePins.Length > 0 ? possiblePins[0] : GC2xHATConst.DefaultDHTDataPin;
			Console.WriteLine("Raspberry Pi wiringPi DHT11/22 (AM2303) reader");
			DHTSensor sensor = new DHTSensor(dataPin, 3);
			sensor.Read();
			if (sensor.IsDataValid)
			{
				Console.WriteLine("Humidity = {0:0.00} % Temperature = {1:0.00} *C (Tries: {2})",
						sensor.Humidity, sensor.Temperature, sensor.Tries);
			}
			else
			{
				Console.WriteLine("Error, DHT sensor not responding  (Tries: {0})",
						sensor.Tries);
			}
		}
	}
}