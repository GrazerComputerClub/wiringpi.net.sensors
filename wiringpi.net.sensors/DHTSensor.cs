namespace wiringpi.net.sensors
{
	using System.Threading;

	public class DHTSensor
	{
		private const int MaxTransferReads = 41;
		/// <summary>low=50us+25us=75us, high=50us+70us=120us</summary>
		private const int ThresholdHighLow = 100; //us

		private int[] _dhtData = new int[5];
		private long[] _timeOfChange = new long[MaxTransferReads];

		public DHTSensor(int dataPin, int retries)
		{
			DataPin = dataPin;
			if (retries < 1)
			{
				Retries = 1;
			}
			else if (retries > 60)
			{
				Retries = 60;
			}
			else
			{
				Retries = retries;
			}
			ZeroData();
		}

		public int DataPin { get; private set; }
		public int Retries { get; private set; }

		public bool IsDataValid { get; private set; }
		public int Tries { get; private set; }
		public float Humidity { get; private set; }
		public float Temperature { get; private set; }

		public void Read()
		{
			for (Tries = 1; Tries < Retries + 1; Tries++)
			{
				WiringPi.PinMode(DataPin, WiringPiConst.Output);
				WiringPi.DigitalWrite(DataPin, WiringPiConst.Low);
				WiringPi.DelayMicroseconds(20000);
				WiringPi.DigitalWrite(DataPin, WiringPiConst.High);
				WiringPi.DelayMicroseconds(30);
				WiringPi.PinMode(DataPin, WiringPiConst.Input);
				// Chip start to transfer
				int state = WiringPi.DigitalRead(DataPin);
				int lastState = WiringPiConst.High;
				for (int i = 0; i < MaxTransferReads; i++)
				{
					int readCounter = 0;
					do
					{
						readCounter++;
						lastState = state;
						WiringPi.DelayMicroseconds(1);
						state = WiringPi.DigitalRead(DataPin);
					} while (!(lastState == WiringPiConst.High && state == WiringPiConst.Low) && readCounter < 100);
					// --> High->Low trigger
					lastState = WiringPiConst.Low;
					_timeOfChange[i] = WiringPi.Micros();
				}
				GetDHTData(1);
				if (IsDataValid)
				{
					break;
				}
				else
				{
					Thread.Sleep(400);
				}
			}
		}

		private void GetDHTData(int startPosition)
		{
			ZeroData();
			int bitNo = 0;
			for (int i = startPosition; i < startPosition + 40; i++)
			{
				int byteNo = bitNo / 8;
				//move bit
				_dhtData[byteNo] <<= 0x01;
				if (_timeOfChange[i] - _timeOfChange[i - 1] > ThresholdHighLow)
				{
					//set bit
					_dhtData[byteNo] |= 0x01;
				}
				bitNo++;
			}

			if (_dhtData[4] == ((_dhtData[0] + _dhtData[1] + _dhtData[2] + _dhtData[3]) & 0xFF))
			{
				if (_dhtData[0] <= 3)
				{
					// Autodedect DHT type
					// Calculation DHT22/AM2302
					Humidity = (float)_dhtData[0] * 256 + (float)_dhtData[1];
					Humidity /= 10.0f;
					Temperature = (float)(_dhtData[2] & 0x7F) * 256 + (float)_dhtData[3];
					Temperature /= 10.0f;
					if ((_dhtData[2] & 0x80) == 1)
					{
						Temperature *= -1;
					}
				}
				else
				{
					// Calculation DHT11
					Humidity = _dhtData[0] * 10 + _dhtData[1];
					Humidity /= 10.0f;
					Temperature = _dhtData[2] * 10 + _dhtData[3];
					Temperature /= 10.0f;
				}
				IsDataValid = true;
			}
		}

		private void ZeroData()
		{
			for (int i = 0; i < _dhtData.Length; i++)
			{
				_dhtData[i] = 0;
			}
			IsDataValid = false;
			Tries = 0;
			Humidity = 0f;
			Temperature = 0f;
		}
	}
}