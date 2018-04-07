namespace wiringpi.net.sensors
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	public class TM1637Display
	{
		private static readonly Dictionary<char, Segments> _char2SegCode;

		private char[] _currentData = new char[5];
		private bool _showDoublePoint;
		private byte _brightness;
		private int _brightnessPercent;

		static TM1637Display()
		{
			_char2SegCode = new Dictionary<char, Segments>
			{
				[default(char)] = Segments.None,
				['0'] = Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F,
				['1'] = Segments.B | Segments.C,
				['2'] = Segments.A | Segments.B | Segments.G | Segments.E | Segments.D,
				['3'] = Segments.A | Segments.B | Segments.G | Segments.C | Segments.D,
				['4'] = Segments.F | Segments.G | Segments.B | Segments.C,
				['5'] = Segments.A | Segments.F | Segments.G | Segments.C | Segments.D,
				['6'] = Segments.F | Segments.E | Segments.G | Segments.C | Segments.D,
				['7'] = Segments.A | Segments.B | Segments.C,
				['8'] = Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F | Segments.G,
				['9'] = Segments.A | Segments.B | Segments.C | Segments.F | Segments.G,
				['A'] = Segments.A | Segments.B | Segments.C | Segments.E | Segments.F | Segments.G,
				['B'] = Segments.F | Segments.E | Segments.F | Segments.C | Segments.D,
				['C'] = Segments.A | Segments.F | Segments.E | Segments.D,
				['D'] = Segments.B | Segments.C | Segments.G | Segments.E | Segments.D,
				['E'] = Segments.A | Segments.F | Segments.G | Segments.E | Segments.D,
				['F'] = Segments.A | Segments.F | Segments.G | Segments.E,
				['H'] = Segments.F | Segments.G | Segments.E | Segments.B | Segments.C,
				['I'] = Segments.E,
				['J'] = Segments.B | Segments.C | Segments.D | Segments.E,
				['L'] = Segments.F | Segments.E | Segments.D,
				['N'] = Segments.E | Segments.G | Segments.C,
				['O'] = Segments.E | Segments.G | Segments.C | Segments.D,
				['P'] = Segments.A | Segments.B | Segments.F | Segments.G | Segments.E,
				['R'] = Segments.G | Segments.E,
				['S'] = Segments.A | Segments.F | Segments.G | Segments.C | Segments.D,
				['U'] = Segments.E | Segments.D | Segments.C,
				['Y'] = Segments.F | Segments.G | Segments.B | Segments.C | Segments.D,
				['Z'] = Segments.A | Segments.B | Segments.G | Segments.E | Segments.D,
				['-'] = Segments.G,
				['\''] = Segments.B
			};
		}

		public TM1637Display(int clockPin, int dataPin, int brightnessPercent)
		{
			CLKPin = clockPin;
			DIOPin = dataPin;
			Reset();
			WiringPi.PinMode(CLKPin, WiringPiConst.Output);
			WiringPi.PinMode(DIOPin, WiringPiConst.Output);
			WiringPi.PullUpDnControl(DIOPin, WiringPiConst.PudUp);
			// clearing display included
			BrightnessPercent = brightnessPercent;
		}

		public int CLKPin { get; private set; }

		public int DIOPin { get; private set; }

		public bool ACKErr { get; private set; }

		/// <summary>shows or hides the double point</summary>
		public bool ShowDoublePoint
		{
			get { return _showDoublePoint; }
			set
			{
				if (_showDoublePoint != value)
				{
					_showDoublePoint = value;
					// rewrite display position 1
					Show(2, _currentData[1]);
				}

			}
		}

		/// <summary>BrightnessPercent 0...100%</summary>
		public int BrightnessPercent
		{
			get { return _brightnessPercent; }
			set
			{
				byte brightness = (byte)(value / 100.0 * Const.MaxBrightness);
				if (brightness > Const.MaxBrightness)
				{
					brightness = Const.MaxBrightness;
				}
				else if (brightness < 0)
				{
					brightness = 0;
				}
				if (_brightness != brightness)
				{
					_brightnessPercent = value;
					_brightness = brightness;
					// rewrite display
					Show(_currentData);
				}
			}
		}

		public void Clear()
		{
			Reset();
			Show(_currentData);
		}

		public void Show(string data)
		{
			Show(data.ToCharArray());
		}

		public void Show(int data)
		{
			if (data > 9999)
			{
				data = 9999;
			}
			else if (data < -999)
			{
				data = -999;
			}
			char[] values = data.ToString().ToCharArray();
			int valueOffset = (_currentData.Length - 1) - values.Length;
			for (int i = 0; i < _currentData.Length; i++)
			{
				_currentData[i] = i >= valueOffset && values.Length > i - valueOffset
					? values[i - valueOffset] : default(char);
			}
			Show(_currentData);
		}

		public void Show(int digitNumber, char data)
		{
			if (digitNumber < 1 || digitNumber > 4)
			{
				return;
			}
			digitNumber--;
			using (new WriteSection(this))
			{
				WriteByte(Const.DataCmdFixAddr);
			}
			using (new WriteSection(this))
			{
				WriteByte((byte)(Const.DisplayAddress | digitNumber));
				WriteByte((byte)GetAndSetSegCode(digitNumber, data));
			}
			using (new WriteSection(this))
			{
				WriteByte((byte)(Const.DisplayOn + _brightness));
			}
		}

		public void Show(char[] data)
		{
			int len = data.Length;
			if (len > 4)
			{
				len = 4;
			}
			using (new WriteSection(this))
			{
				WriteByte(Const.DataCmdAutoAddr);
			}
			using (new WriteSection(this))
			{
				WriteByte(Const.DisplayAddress);
				for (int digitNumber = 0; digitNumber < len; digitNumber++)
				{
					WriteByte((byte)GetAndSetSegCode(digitNumber, data[digitNumber]));
				}
				for (int digitNumber = len; digitNumber < 4; digitNumber++)
				{
					WriteByte((byte)GetAndSetSegCode(digitNumber, default(char)));
				}
			}
			using (new WriteSection(this))
			{
				WriteByte((byte)(Const.DisplayOn + _brightness));
			}
		}

		private void Reset()
		{
			_showDoublePoint = false;
			ACKErr = false;
			for (int i = 0; i < _currentData.Length; i++)
			{
				_currentData[i] = default(char);
			}
		}

		private void WriteByte(byte data)
		{
			byte mask = 0x01;
			for (int bit = 0; bit < 8; bit++)
			{
				WiringPi.DigitalWrite(CLKPin, WiringPiConst.Low);
				WiringPi.DigitalWrite(DIOPin, (data & 1 << bit) > 0 ? WiringPiConst.High : WiringPiConst.Low);
				mask <<= 1;
				WiringPi.DigitalWrite(CLKPin, WiringPiConst.High);
				WiringPi.DelayMicroseconds(1);
			}
			// switch DIO to input for ACK reading
			WiringPi.PinMode(DIOPin, WiringPiConst.Input);
			// reset DIO before set to output
			WiringPi.DigitalWrite(DIOPin, WiringPiConst.Low);
			// start ACK reading
			WiringPi.DigitalWrite(CLKPin, WiringPiConst.Low);
			ACKErr = true;
			for (int readTry = 1; readTry <= 100; readTry++)
			{
				// read/wait ACK (max. 100us)
				if (WiringPi.DigitalRead(DIOPin) == WiringPiConst.High)
				{
					WiringPi.DelayMicroseconds(1);
				}
				else
				{
					ACKErr = false;
					break;
				}
			}
			WiringPi.DigitalWrite(CLKPin, WiringPiConst.High);
			WiringPi.PinMode(DIOPin, WiringPiConst.Output);
			WiringPi.DigitalWrite(CLKPin, WiringPiConst.Low);
		}

		private Segments GetAndSetSegCode(int digitNumber, char data)
		{
			char upperData = char.ToUpper(data);
			_currentData[digitNumber] = upperData;
			if (_char2SegCode.TryGetValue(upperData, out Segments segCode))
			{
				//DP only supported at digit number 1 (center)
				if (digitNumber == 1 && ShowDoublePoint)
				{
					segCode |= Segments.DP;
				}
				else
				{
					segCode &= ~Segments.DP;
				}
			}
			return segCode;
		}

		internal class WriteSection : IDisposable
		{
			private TM1637Display _display;

			public WriteSection(TM1637Display display)
			{
				_display = display;
				WiringPi.DigitalWrite(_display.CLKPin, WiringPiConst.High);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.DIOPin, WiringPiConst.High);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.DIOPin, WiringPiConst.Low);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.CLKPin, WiringPiConst.Low);
			}

			public void Dispose()
			{
				WiringPi.DigitalWrite(_display.CLKPin, WiringPiConst.Low);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.DIOPin, WiringPiConst.Low);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.CLKPin, WiringPiConst.High);
				WiringPi.DelayMicroseconds(1);
				WiringPi.DigitalWrite(_display.DIOPin, WiringPiConst.High);
			}
		}

		internal static class Const
		{
			public const byte MaxBrightness = 7;
			/// <summary>1. digit</summary>
			public const byte DisplayAddress = 0xC0;
			public const byte DataCmdAutoAddr = 0x40;
			public const byte DataCmdFixAddr = 0x44;
			/// <summary>+ B0, B1, B2 = Brightness</summary>
			public const byte DisplayOn = 0x88;
		}

		[Flags]
		internal enum Segments : byte
		{
			None = 0x0,
			/// <summary>0b00000001</summary>
			A = 0x01,
			/// <summary>0b00000010</summary>
			B = 0x02,
			/// <summary>0b00000100</summary>
			C = 0x04,
			/// <summary>0b00001000</summary>
			D = 0x08,
			/// <summary>0b00010000</summary>
			E = 0x10,
			/// <summary>0b00100000</summary>
			F = 0x20,
			/// <summary>0b01000000</summary>
			G = 0x40,
			/// <summary>0b10000000</summary>
			DP = 0x80
		}
	}
}