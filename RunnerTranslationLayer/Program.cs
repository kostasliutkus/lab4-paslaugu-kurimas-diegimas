namespace Servers;

using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using NLog;
using Services;
using Microsoft.AspNetCore.Mvc;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
public class Server
{
	/// <summary>
	/// Logger for this class.
	/// </summary>
	Logger log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Configure loggin subsystem.
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
	/// Program entry point.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	public static void Main(string[] args)
	{
		var self = new Server();
		self.Run(args);
	}

	/// <summary>
	/// Program body.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	private void Run(string[] args) 
	{
		//configure logging
		ConfigureLogging();

		//indicate server is about to start
		log.Info("Server is about to start");

		//start the server
		StartServer(args);
	}

	/// <summary>
	/// Starts integrated server.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	private void StartServer(string[] args)
	{
		//create web app builder
		var builder = WebApplication.CreateBuilder(args);

		//configure integrated server
		builder.WebHost.ConfigureKestrel(opts => {
			opts.Listen(IPAddress.Loopback, 5002);
		});

		//add and configure swagger documentation generator (http://127.0.0.1:5002/swagger/)
		

		//turn on support for web api controllers
		builder.Services
			.AddControllers()
			.AddJsonOptions(opts => {
				//this makes enumeration values to be strings instead of integers in opeanapi doc
				opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.PropertyNamingPolicy = null;
			});

		//add CORS policies
		builder.Services.AddCors(cr => {
			//allow everything from everywhere
			cr.AddPolicy("allowAll", cp => {
				cp.AllowAnyOrigin();
				cp.AllowAnyMethod();
				cp.AllowAnyHeader();
			});
		});

        // builder.Services.AddSimpleRpcClient(
        // "TrackService",
        // new HttpClientTransportOptions
        // {
        //     Url = "http://127.0.0.1:5000/simplerpc",
        //     Serializer = "HyperionMessageSerializer"
        // }
        // ).AddSimpleRpcHyperionSerializer();
        // builder.Services.AddSimpleRpcProxy<ITrackService>("TrackService");
		
        builder.Services.AddSwaggerGen(opts => {
            //include code comments in swagger documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

		//build the server
		var app = builder.Build();

		//turn CORS policy on
		app.UseCors("allowAll");

		//turn on support for swagger doc web page
		app.UseSwagger();
		app.UseSwaggerUI();

		//turn on request routing
		app.UseRouting();

		//configure routes
		app.MapControllerRoute(
			name: "default",
			pattern: "{controller}/{action=Index}/{id?}"
		);
		
		//run the server
		app.Run();
		// app.RunAsync(); //use this if you need to implement background processing in the main thread
	}
}