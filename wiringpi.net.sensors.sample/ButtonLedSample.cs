namespace wiringpi.net.sensors.sample
{
	[Sample(Id = 5, Name = "Button LED sample")]
	class ButtonLedSample : LoopSample
	{
		private int _button1 = 13;
		private int _button2 = 19;
		private int _button3 = 26;
		private int _ledRed = 16;
		private int _ledYellow = 20;
		private int _ledGreen = 21;

		protected override void DefinePins(int[] possiblePins)
		{
			_button1 = possiblePins.Length > 0 ? possiblePins[0] : GC2xHATConst.DefaultButton1;
			_button2 = possiblePins.Length > 1 ? possiblePins[1] : GC2xHATConst.DefaultButton2;
			_button3 = possiblePins.Length > 2 ? possiblePins[2] : GC2xHATConst.DefaultButton3;
			_ledRed = possiblePins.Length > 3 ? possiblePins[3] : GC2xHATConst.DefaultLedRed;
			_ledYellow = possiblePins.Length > 4 ? possiblePins[4] : GC2xHATConst.DefaultLedYellow;
			_ledGreen = possiblePins.Length > 5 ? possiblePins[5] : GC2xHATConst.DefaultLedGreen;

			WiringPi.PinMode(_button1, WiringPiConst.Input);
			WiringPi.PinMode(_button2, WiringPiConst.Input);
			WiringPi.PinMode(_button3, WiringPiConst.Input);
			WiringPi.PinMode(_ledRed, WiringPiConst.Output);
			WiringPi.PinMode(_ledYellow, WiringPiConst.Output);
			WiringPi.PinMode(_ledGreen, WiringPiConst.Output);
		}

		protected override void LoopImplementation()
		{
			WiringPi.DigitalWrite(_ledRed,
				(WiringPi.DigitalRead(_button1) == WiringPiConst.High) ? WiringPiConst.High : WiringPiConst.Low);
			WiringPi.DigitalWrite(_ledYellow,
				(WiringPi.DigitalRead(_button2) == WiringPiConst.High) ? WiringPiConst.High : WiringPiConst.Low);
			WiringPi.DigitalWrite(_ledGreen,
				(WiringPi.DigitalRead(_button3) == WiringPiConst.High) ? WiringPiConst.High : WiringPiConst.Low);
		}
	}
}