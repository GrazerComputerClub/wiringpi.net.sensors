namespace wiringpi.net.sensors
{
	using System.Runtime.InteropServices;

	internal class WiringPi
	{
		static WiringPi()
		{
			WiringPiSetupGpio();
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void WiringPiISRDelegate();

		[DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupGpio")]
		public static extern int WiringPiSetupGpio();

		[DllImport("libwiringPi.so", EntryPoint = "pinMode")]
		public static extern void PinMode(int pin, int mode);

		[DllImport("libwiringPi.so", EntryPoint = "pullUpDnControl")]
		public static extern void PullUpDnControl(int pin, int pud);

		[DllImport("libwiringPi.so", EntryPoint = "digitalRead")]
		public static extern int DigitalRead(int pin);

		[DllImport("libwiringPi.so", EntryPoint = "digitalWrite")]
		public static extern void DigitalWrite(int pin, int value);

		[DllImport("libwiringPi.so", EntryPoint = "wiringPiISR")]
		public static extern int WiringPiISR(int pin, int mode, [MarshalAs(UnmanagedType.FunctionPtr)]WiringPiISRDelegate callback);

		[DllImport("libwiringPi.so", EntryPoint = "wiringPiISRCancel")]
		public static extern int WiringPiISRCancel(int pin);

		[DllImport("libwiringPi.so", EntryPoint = "micros")]
		public static extern int Micros();

		[DllImport("libwiringPi.so", EntryPoint = "delayMicroseconds")]
		public static extern void DelayMicroseconds(uint howLong);
	}
}