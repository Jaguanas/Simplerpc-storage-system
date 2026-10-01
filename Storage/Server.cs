namespace Servers;

using System.Net;

using NLog;

using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;

using Services;


public class Server
{
	// Logger used to report server startup and runtime events.
	/// <summary>
	/// Logger for this class.
	/// </summary>
	Logger log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Configure loggin subsystem.
	/// </summary>
	private void ConfigureLogging()
	{
		// Send all log levels to the console with a compact timestamped format.
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
	/// Program entry point.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	public static void Main(string[] args)
	{
		// Create the host object and start the server.
		var self = new Server();
		self.Run(args);
	}

	/// <summary>
	/// Program body.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	private void Run(string[] args) 
	{
		// Configure logging before any startup messages are written.
		ConfigureLogging();

		// Indicate that the server startup sequence has begun.
		log.Info("Server is about to start");

		// Build and run the HTTP/RPC host.
		StartServer(args);
	}

	/// <summary>
	/// Starts integrated server.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	private void StartServer(string[] args)
	{
		// Create the ASP.NET Core host builder.
		var builder = WebApplication.CreateBuilder(args);

		// Listen only on localhost port 5000 for the RPC clients.
		builder.WebHost.ConfigureKestrel(opts => {
			opts.Listen(IPAddress.Any, 5000);
		});

		// Register the HTTP transport and Hyperion serializer used by RPC.
		builder.Services
			.AddSimpleRpcServer(new HttpServerTransportOptions { Path = "/simplerpc" })
			.AddSimpleRpcHyperionSerializer();

		// Register the implementation under the shared contract name.
		builder.Services
			// .AddScoped<ITrafficLightService, TrafficLightService>();  //instance-per-request, AddTransient would result in the same
			.AddSingleton<IStorageService>(new StorageService());   //singleton

		// Build the configured application.
		var app = builder.Build();

		// Add middleware that receives, dispatches, and replies to RPC calls.
		app.UseSimpleRpcServer();

		// Run the server.
		app.Run();
		// app.RunAsync(); //use this if you need to implement background processing in the main thread
	}
}