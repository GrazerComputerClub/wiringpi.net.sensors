namespace wiringpi.net.sensors.sample
{
	using System;
	using System.Net;
	using System.Net.Sockets;

	[Sample(Id = 4, Name = "Display ip address on TM1637 display")]
	class IPSample : LoopSample, IDisposable
	{
		private TM1637Display _display;
		private int _backwardPin;
		private int _forwardPin;
		private int _abortPin;

		private string _ip;
		private string _displayText;
		private int _currentPosition = 0;

		//protected override int LoopSleepTime => 0;

		protected override void DefinePins(int[] possiblePins)
		{
			int clockPin = possiblePins.Length > 0 ? possiblePins[0] : GC2xHATConst.DefaultTM1637ClockPin;
			int dataPin = possiblePins.Length > 1 ? possiblePins[1] : GC2xHATConst.DefaultTM1637DataPin;
			_backwardPin = possiblePins.Length > 2 ? possiblePins[2] : GC2xHATConst.DefaultButton1;
			_forwardPin = possiblePins.Length > 3 ? possiblePins[3] : GC2xHATConst.DefaultButton3;
			_abortPin = possiblePins.Length > 4 ? possiblePins[4] : GC2xHATConst.DefaultButton2;
			WiringPi.PullUpDnControl(_backwardPin, WiringPiConst.PudDown);
			WiringPi.PullUpDnControl(_forwardPin, WiringPiConst.PudDown);
			WiringPi.PullUpDnControl(_abortPin, WiringPiConst.PudDown);
			WiringPi.PinMode(_backwardPin, WiringPiConst.Input);
			WiringPi.PinMode(_forwardPin, WiringPiConst.Input);
			WiringPi.PinMode(_abortPin, WiringPiConst.Input);
			
			_display = new TM1637Display(clockPin, dataPin, 60);

			_ip = GetLocalIPAddress();
			Console.WriteLine("IP address: '{0}'", _ip);
			_displayText = string.Format(" 1P {0}", _ip?.Replace(".", "-"));
		}

		protected override void LoopImplementation()
		{
			_display.Show(DoTextCalculation(true));
			if (_currentPosition == 1)
			{
				for (int i = 0; i < 3; i++)
				{
					_display.ShowDoublePoint = true;
					SleepAndCheckPins(500, _backwardPin, _forwardPin, _abortPin);
					_display.ShowDoublePoint = false;
					SleepAndCheckPins(500, _backwardPin, _forwardPin, _abortPin);
				}
			}
			int pin = SleepAndCheckPins(1500, _backwardPin, _forwardPin, _abortPin);
			if (pin == _abortPin)
			{
				Stop();
			}
			else if (pin == _forwardPin || pin == _backwardPin)
			{
				while (SleepAndCheckPins(500, _backwardPin, _forwardPin, _abortPin) == pin)
				{
					_display.Show(DoTextCalculation(pin == _forwardPin));
					Sleep(200);
				}
			}
		}

		private string DoTextCalculation(bool forward)
		{
			const int displayLength = 4;
			string text;
			if (forward)
			{
				_currentPosition++;
			}
			else
			{
				_currentPosition--;
			}
			if (_currentPosition == _displayText.Length)
			{
				_currentPosition = 0;
			}
			else if (_currentPosition < 0)
			{
				_currentPosition = _displayText.Length - 1;
			}
			if ((_currentPosition + displayLength + 1) < _displayText.Length)
			{
				text = _displayText.Substring(_currentPosition,
					displayLength);
			}
			else
			{
				int chars = _displayText.Length - _currentPosition;
				text = _displayText.Substring(_currentPosition,
					chars);
				if ((displayLength - chars) > 0)
				{
					text += _displayText.Substring(0, displayLength - chars);
				}
			}
			return text;
		}

		private string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			return string.Empty;
		}

		public void Dispose()
		{
			_display?.Clear();
		}
	}
}
