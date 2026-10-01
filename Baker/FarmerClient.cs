namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

using NLog;

using Services;

class Client
{
	// Names used to create a random baker identity.
	/// <summary>
	/// A set of names to choose from.
	/// </summary>
	private readonly List<string> NAMES = 
		new List<string> { 
			"Jonas", "Petras", "Dzekas", "Stivas"
		};

	/// <summary>
	/// A set of surnames to choose from.
	/// </summary>
	private readonly List<string> SURNAMES = 
		new List<String> { 
			"Jonauskas", "Petrauskas", "Dzekauskas", "Stivensonas" 
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
		// Configure logging before starting client activity.
		ConfigureLogging();

		// Generate varied names and grain amounts.
		var rnd = new Random();

		// Reconnect automatically if the server or network becomes unavailable.
		while( true )
		{
			try {
				// Register the HTTP RPC transport and serializer in dependency injection.
				var sc = new ServiceCollection();
				sc
					.AddSimpleRpcClient(
						"storageService", //must be same as on line 86
						new HttpClientTransportOptions
						{
							Url = Environment.GetEnvironmentVariable("STORAGE_URL")
								?? "http://127.0.0.1:5000/simplerpc",
							Serializer = "HyperionMessageSerializer"
						}
					)
					.AddSimpleRpcHyperionSerializer();

				sc.AddSimpleRpcProxy<IStorageService>("storageService"); //must be same as on line 77

				// Build the client container and resolve the contract proxy.
				var sp = sc.BuildServiceProvider();
				var storage = sp.GetService<IStorageService>();

				// Create the baker data that will cross the RPC boundary.
				var baker = new BakerDesc();

				baker.NameSurname =
					NAMES[rnd.Next(NAMES.Count)] + 
					" " +
					SURNAMES[rnd.Next(SURNAMES.Count)];
				// The proxy serializes this call and sends it to the server.
				baker.Id = storage.RegisterBaker(baker);

				// Record the identity assigned by the server.
				mLog.Info($"I am a baker {baker.Id}, {baker.NameSurname}.");
				Console.Title = $"I am a baker {baker.Id}, {baker.NameSurname}.";
					
				while( true )
				{
					// Poll the server because the season changes in the background.
					Season season = storage.GetSeasonState();

					int period;
					if (season == Season.Winter || season == Season.Spring) { period = 10000; }
					else { period = 5000; }

					// Bakers wait longer during inactive agricultural seasons.
					Thread.Sleep(period);

					// Choose an inclusive 0-to-100 grain request.
					int amount = rnd.Next(0, 101);

					mLog.Info(
						$"Baker {baker.Id} generated {amount} grains to consume.");

					// Invoke the remote service and receive a serialized result.
					GrainOperationResult result =
						storage.ConsumeGrain(baker, amount);

					mLog.Info(
						$"Consumption result: {result.Status}. {result.Reason}");
				}
			}
			catch( Exception e )
			{
				// Log connection or RPC failures before attempting to reconnect.
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
		// Start the long-running baker client.
		var self = new Client();
		self.Run();
	}
}
