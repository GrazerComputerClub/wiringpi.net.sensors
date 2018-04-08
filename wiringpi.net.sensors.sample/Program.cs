namespace wiringpi.net.sensors.sample
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	class Program
	{
		private const int DefaultDHTDataPin = 4;

		private const int DefaultHCSR04TRIGPin = 17;
		private const int DefaultHCSR04ECHOPin = 27;

		private const int DefaultTM1637ClockPin = 23;
		private const int DefaultTM1637DataPin = 24;

		private static volatile bool _isHCSR04SampleRunning = false;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += Console_CancelKeyPress;
			int sampleCode;
			List<int> possiblePins;
			ReadParameter(args, out sampleCode, out possiblePins);
			Console.WriteLine("Start example -> Enter:\r\n1\tDHT sensor\r\n2\tHCSR04 sensor\r\n3\tTM1637 display");
			if (sampleCode == 0)
			{
				int.TryParse(Console.ReadLine(), out sampleCode);
			}
			switch (sampleCode)
			{
				case 1:
					DhtSample(possiblePins);
					break;
				case 2:
					HCSR04Sample(possiblePins);
					break;
				case 3:
					TM1637Sample(possiblePins);
					break;
				default:
					break;
			}
		}

		static void ReadParameter(string[] args, out int sampleCode, out List<int> possiblePins)
		{
			sampleCode = 0;
			possiblePins = new List<int>();
			if (args != null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					int value;
					if (int.TryParse(args[i], out value))
					{
						if (i == 0)
						{
							sampleCode = value;
						}
						else
						{
							possiblePins.Add(value);
						}

					}
					else
					{
						break;
					}
				}
			}
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			if (_isHCSR04SampleRunning)
			{
				e.Cancel = true;
				_isHCSR04SampleRunning = false;
			}
		}

		static void DhtSample(List<int> possiblePins)
		{
			int dataPin = possiblePins.Count > 0 ? possiblePins[0] : DefaultDHTDataPin;
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

		static void HCSR04Sample(List<int> possiblePins)
		{
			int trigPin = possiblePins.Count > 0 ? possiblePins[0] : DefaultHCSR04TRIGPin;
			int echoPin = possiblePins.Count > 1 ? possiblePins[1] : DefaultHCSR04ECHOPin;
			Console.WriteLine("Raspberry Pi wiringPi HCSR04 reader");
			_isHCSR04SampleRunning = true;
			try
			{
				using (HCSR04Sensor sensor = new HCSR04Sensor(trigPin, echoPin))
				{
					while (_isHCSR04SampleRunning)
					{
						sensor.StartDistanceMeas();
						//maxium distance 80ms
						Thread.Sleep(80);
						if (sensor.IsDataValid())
						{
							Console.WriteLine("distance: {0} cm (time elapsed: {1} ms)",
									sensor.Distance, sensor.DistanceTime * 1000.0f);
							Thread.Sleep(30);
						}
					}
				}
			}
			finally
			{
				_isHCSR04SampleRunning = false;
			}
		}

		static void TM1637Sample(List<int> possiblePins)
		{
			int clockPin = possiblePins.Count > 0 ? possiblePins[0] : DefaultTM1637ClockPin;
			int dataPin = possiblePins.Count > 1 ? possiblePins[1] : DefaultTM1637DataPin;
			TM1637Display display = new TM1637Display(clockPin, dataPin, 60);

			Console.WriteLine("Test program for 4-Digit display with TM1637 chip");
			Console.WriteLine("Testing int values from -999 to 1199...");
			Console.ReadLine();
			for (int value = -999; value <= 1199; value++)
			{
				display.Show(value);
				Thread.Sleep(1);
			}

			Console.WriteLine("Testing set single char and double point...");
			Console.ReadLine();
			display.Show(1, '2');
			display.Show(2, '3');
			display.ShowDoublePoint = true;
			display.Show(3, '5');
			display.Show(4, '4');
			Thread.Sleep(1000);
			display.ShowDoublePoint = false;
			Thread.Sleep(1000);
			display.ShowDoublePoint = true;
			Thread.Sleep(1000);
			display.ShowDoublePoint = false;

			Console.WriteLine("Testing set text...");
			Console.ReadLine();
			display.Show("HELO");
			Thread.Sleep(1000);
			display.Show("HERE");
			Thread.Sleep(1000);
			display.Show("IS");
			Thread.Sleep(1000);
			display.Show("YOUR");
			Thread.Sleep(1000);
			display.Show("PI0 ");

			Console.WriteLine("Testing clear...");
			Console.ReadLine();
			display.Clear();

			Console.WriteLine("Testing lower brightness...");
			Console.ReadLine();
			display.Show("100P");
			display.BrightnessPercent = 100;
			Thread.Sleep(1000);
			display.Show(" 80P");
			display.BrightnessPercent = 80;
			Thread.Sleep(1000);
			display.Show(" 40P");
			display.BrightnessPercent = 40;
			Thread.Sleep(1000);
			display.Show(" 20P");
			display.BrightnessPercent = 20;
			Thread.Sleep(1000);
			display.Show(" 10P");
			display.BrightnessPercent = 10;
			Thread.Sleep(1000);

			Console.WriteLine("Finish...");
			Console.ReadLine();
			display.Clear();
		}
	}
}