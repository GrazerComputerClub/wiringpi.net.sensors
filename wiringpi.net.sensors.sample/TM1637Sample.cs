namespace wiringpi.net.sensors.sample
{
	using System;
	using System.Threading;

	[Sample(Id = 3, Name = "TM1637 display")]
	class TM1637Sample : ISample
	{
		public void Run(int[] possiblePins)
		{
			int clockPin = possiblePins.Length > 0 ? possiblePins[0] : GC2xHATConst.DefaultTM1637ClockPin;
			int dataPin = possiblePins.Length > 1 ? possiblePins[1] : GC2xHATConst.DefaultTM1637DataPin;
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