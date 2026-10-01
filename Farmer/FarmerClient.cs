namespace Clients;

using Microsoft.Extensions.DependencyInjection;

using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

using NLog;

using Services;


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
			"John", "Peter", "Jack", "Steve"
		};

	/// <summary>
	/// A set of surnames to choose from.
	/// </summary>
	private readonly List<string> SURNAMES = 
		new List<String> { 
			"Johnson", "Peterson", "Jackson", "Steveson" 
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
						"StorageService", //must be same as on line 86
						new HttpClientTransportOptions
						{
							Url = Environment.GetEnvironmentVariable("STORAGE_URL")
								?? "http://127.0.0.1:5000/simplerpc",
							Serializer = "HyperionMessageSerializer"
						}
					)
					.AddSimpleRpcHyperionSerializer();

				sc.AddSimpleRpcProxy<IStorageService>("StorageService"); //must be same as on line 77

				// Build the client container and resolve the contract proxy.
				var sp = sc.BuildServiceProvider();
				var storage = sp.GetService<IStorageService>();

				// Create the farmer data that will cross the RPC boundary.
				var farmer = new FarmerDesc();

				farmer.NameSurname =
					NAMES[rnd.Next(NAMES.Count)] + 
					" " +
					SURNAMES[rnd.Next(SURNAMES.Count)];

				// The proxy serializes this call and sends it to the server.
				farmer.Id = storage.RegisterFarmer(farmer);

				// Record the identity assigned by the server.
				mLog.Info($"I am a farmer {farmer.Id}, {farmer.NameSurname}.");
				Console.Title = $"I am a farmer {farmer.Id}, {farmer.NameSurname}.";
					
				// Keep generating and depositing grain while connected.
				while( true )
				{
					// Read the current season before generating grain.
					Season season = storage.GetSeasonState();

					// Wait until summer or autumn, when farmers are active.
					while (season == Season.Winter || season == Season.Spring)
					{
						mLog.Info($"Current seasson is {season}. Sleeping...");
						Thread.Sleep(2000);
						season = storage.GetSeasonState();
					}

					// Choose an inclusive 0-to-100 grain deposit.
					int amount = rnd.Next(0, 101);

					// Ask the server to deposit the generated grain.
					GrainOperationResult result =
						storage.TryDepositGrain(farmer, amount);

					// Retry the same amount until storage has enough free capacity.
					while ( result.Status == GrainOperationStatus.StorageFull )
					{
						mLog.Info($"Currently the storage is full! Retrying in 2 sec...");
						Thread.Sleep(5000);

						result = storage.TryDepositGrain(farmer, amount);
					}

					// Explain a winter transition that invalidated this deposit.
					if (result.Status == GrainOperationStatus.SeasonChanged)
					{
						mLog.Info($"The season changed to winter. This amount of grain lost: {amount}");
					}

					// Explain why spring deposits are rejected.
					if (result.Status == GrainOperationStatus.InactiveSeason)
					{
						mLog.Info($"Currently it is spring and no grain can be generated.");
					}

					mLog.Info(
						$"Generation result: {result.Status}. {result.Reason}");

					Thread.Sleep(3000 + rnd.Next(3000));
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
		// Start the long-running farmer client.
		var self = new Client();
		self.Run();
	}
}
