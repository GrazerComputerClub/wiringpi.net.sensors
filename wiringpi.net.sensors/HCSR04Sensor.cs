namespace wiringpi.net.sensors
{
	using System;
	using System.Threading;

	public class HCSR04Sensor : IDisposable
	{
		private bool _dataValid;
		private int _startTime;
		private int _endTime;

		public HCSR04Sensor(int trigPin, int echoPin)
		{
			TRIGPin = trigPin;
			ECHOPin = echoPin;

			WiringPi.PinMode(TRIGPin, WiringPiConst.Output);
			WiringPi.WiringPiISR(ECHOPin, WiringPiConst.IntEdgeBoth, OnIsrEcho);
		}

		public int TRIGPin { get; private set; }

		public int ECHOPin { get; private set; }

		/// <summary>m/s, at 20*C</summary>
		public float SpeedOfSound { get; set; } = 343.42f;

		/// <summary>cm</summary>
		public float Distance { get; private set; }

		public float DistanceTime { get; private set; }

		public void StartDistanceMeas()
		{
			_dataValid = false;
			Distance = 0.0f;
			WiringPi.DigitalWrite(TRIGPin, WiringPiConst.High);
			Thread.Sleep(200);
			//start impulse
			WiringPi.DigitalWrite(TRIGPin, WiringPiConst.Low);
		}

		public bool IsDataValid()
		{
			if (_dataValid && _endTime > _startTime)
			{
				DistanceTime = (_endTime - _startTime) / 1000000.0f;
				Distance = SpeedOfSound * 100.0f * DistanceTime / 2.0f;
				return true;
			}
			return false;
		}

		public void Dispose()
		{
			//WiringPi.WiringPiISRCancel(ECHOPin);
			WiringPi.PinMode(TRIGPin, WiringPiConst.Output);
			WiringPi.DigitalWrite(TRIGPin, WiringPiConst.Low);
			WiringPi.PinMode(TRIGPin, WiringPiConst.Input);
		}

		private void OnIsrEcho()
		{
			int now = WiringPi.Micros();
			if (WiringPi.DigitalRead(ECHOPin) == WiringPiConst.High)
			{
				_startTime = now;
			}
			else
			{
				_endTime = now;
				_dataValid = true;
			}
		}
	}
}