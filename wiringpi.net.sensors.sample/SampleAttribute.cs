namespace wiringpi.net.sensors.sample
{
	using System;

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	class SampleAttribute : Attribute
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}