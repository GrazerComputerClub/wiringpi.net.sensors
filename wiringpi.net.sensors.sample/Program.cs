namespace wiringpi.net.sensors.sample
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	class Program
	{
		private static SortedDictionary<int, SampleInfo> _samples = new SortedDictionary<int, SampleInfo>();
		private static ISample _sample;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += Console_CancelKeyPress;
			int sampleCode;
			List<int> possiblePins;
			ReadParameter(args, out sampleCode, out possiblePins);

			foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
			{
				SampleAttribute sample = t.GetCustomAttribute<SampleAttribute>();
				if (sample != null)
				{
					_samples.Add(sample.Id, new SampleInfo() { Type = t, Attribute = sample });
				}
			}
			StringBuilder sb = new StringBuilder("Start example -> Enter:");
			foreach (var kvp in _samples)
			{
				sb
					.AppendLine()
					.AppendFormat("{0}\t{1}", kvp.Key, kvp.Value.Attribute.Name);
			}
			Console.WriteLine(sb.ToString());
			if (sampleCode == 0)
			{
				int.TryParse(Console.ReadLine(), out sampleCode);
			}
			SampleInfo info;
			if (_samples.TryGetValue(sampleCode, out info))
			{
				_sample = (ISample)info.Type.GetConstructor(new Type[0]).Invoke(null);
				try
				{
					_sample.Run(possiblePins.ToArray());
				}
				finally
				{
					if (_sample is IDisposable)
					{
						((IDisposable)_sample).Dispose();
					}
				}
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
			Console.WriteLine("User cancelled");
			if (_sample is IStopSample)
			{
				e.Cancel = true;
				((IStopSample)_sample).Stop();
			}
		}

		private class SampleInfo
		{
			public Type Type { get; set; }
			public SampleAttribute Attribute { get; set; }
		}
	}
}