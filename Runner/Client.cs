namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

using NLog;

using Services;
using NLog.LayoutRenderers;


/// <summary>
/// Client example.
/// </summary>
class Client
{
	/// <summary>
	/// A set of names to choose from.
	/// </summary>
	private readonly List<string> NAMES = 
		new List<string> { 
			"John", "Peter", "Jack", "Steve","Jonas","Petras"
		};

	/// <summary>
	/// A set of surnames to choose from.
	/// </summary>
	private readonly List<string> SURNAMES = 
		new List<String> { 
			"Johnson", "Peterson", "Jackson", "Steveson","Jonaitis","Petraitis" 
		};


	/// <summary>
	/// Logger for this class.
	/// </summary>
	Logger mLog = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Configures logging subsystem.
	/// </summary>
	private void ConfigureLogging()
	{
		var config = new NLog.Config.LoggingConfiguration();

		var console =
			new NLog.Targets.ConsoleTarget("console")
			{
				Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
			};
		config.AddTarget(console);
		config.AddRuleForAllLevels(console);

		LogManager.Configuration = config;
	}

	/// <summary>
	/// Program body.
	/// </summary>
	private void Run() {
		//configure logging
		ConfigureLogging();

		//initialize random number generator
		var rnd = new Random();

		//run everythin in a loop to recover from connection errors
		while( true )
		{
			try {
				//connect to the server, get service client proxy
				var sc = new ServiceCollection();
				sc
					.AddSimpleRpcClient(
						"TrackService", //must be same as on line 86
						new HttpClientTransportOptions
						{
							Url = "http://127.0.0.1:5000/simplerpc",
							Serializer = "HyperionMessageSerializer"
						}
					)
					.AddSimpleRpcHyperionSerializer();

				sc.AddSimpleRpcProxy<ITrackService>("TrackService"); //must be same as on line 77

				var sp = sc.BuildServiceProvider();

				var track = sp.GetService<ITrackService>();

				Thread.Sleep(500 + rnd.Next(1500));
				//initialize car descriptor
				var runner = new RunnerDesc();

				runner.RunnerNameSurname =
					NAMES[rnd.Next(NAMES.Count)] + 
					" " +
					SURNAMES[rnd.Next(SURNAMES.Count)];

				//get unique client id
				runner.RunnerId = track.GetRunnerUniqueId();
				
				//log identity data
				mLog.Info($"I am a runner {runner.RunnerId}, name {runner.RunnerNameSurname}.");
				Console.Title = $"I am a runner {runner.RunnerId}, name {runner.RunnerNameSurname}.";

				while(true)
				{
					Thread.Sleep(500 + rnd.Next(1500));
					if(track.GetTrackState()==TrackState.Running)
					{
						Thread.Sleep(500 + rnd.Next(1500));
						var random = new Random();
						double ranDistanceChange = random.NextDouble() * 4;
						// send distance to server
						bool result = track.AddDistanceChange(runner,ranDistanceChange);
						if(result)
							mLog.Info($"I am running");
						else 
							mLog.Info($"Waiting for Running state");
					}
					else
					{
						mLog.Info($"Waiting for Running state");
					}
				}
			}
			catch( Exception e )
			{
				//log whatever exception to console
				mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");

				//prevent console spamming
				Thread.Sleep(2000);
			}
		}
	}

	/// <summary>
	/// Program entry point.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	static void Main(string[] args)
	{
		var self = new Client();
		self.Run();
	}
}
