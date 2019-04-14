namespace wiringpi.net.sensors.sample
{
	interface ISample
	{
		void Run(int[] possiblePins);
	}

	interface IStopSample : ISample
	{
		void Stop();
	}
}