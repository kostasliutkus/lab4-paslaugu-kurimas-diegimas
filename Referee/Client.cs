using System;
using System.Collections.Generic;
using System.Threading;
using Grpc.Net.Client;
using NLog;
using Services;
using GrpcService;
namespace Clients
{
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
            new List<string> { 
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
            //configure logging
            ConfigureLogging();

            //initialize random number generator
            var rnd = new Random();

            //run everything in a loop to recover from connection errors
            while(true)
            {
                try {
                    //connect to the server, get service proxy
                    var channel = GrpcChannel.ForAddress("http://127.0.0.1:5001"); // Assuming the translation layer runs on port 5001
                    var track = new GrpcService.Track.TrackClient(channel);
                    Thread.Sleep(500 + rnd.Next(1500));
                    
                    //initialize Referee descriptor
                    var referee = new GrpcService.RefereeDesc
                    {
                        RefereeNameSurname = NAMES[rnd.Next(NAMES.Count)] + " " + SURNAMES[rnd.Next(SURNAMES.Count)]
                    };

                    //get unique client id
                    referee.RefereeId = track.GetRefereeUniqueId(new Empty()).Value;
                    
                    //log identity data
                    mLog.Info($"I am Referee {referee.RefereeId}, Name. {referee.RefereeNameSurname}.");
                    Console.Title = $"I am Referee {referee.RefereeId}, Name. {referee.RefereeNameSurname}.";
                
                    //do the referee stuff
                    while(true)
                    {
                        // keiciant busena turi arbitras siusti balsa
                        // daryt if? kad pries siunciant patikrintu serverio busena ar pakito?
                        var state = track.GetTrackState(new Empty()).Value;
                        Thread.Sleep(500 + rnd.Next(1500));
                        mLog.Info("I am rating.");

                        var rating = rnd.Next(0, 4) < 3;
                        var input = new SendVoteInput
                        {
                            Rating = new BoolMsg { Value = rating },
                            Id = new IntMsg { Value = referee.RefereeId }
                        };
                        track.SendVote(input);

                        mLog.Info($"I am Referee {referee.RefereeId}, Name. {referee.RefereeNameSurname}. Sending vote {rating}");
                    }
                }
                catch(Exception e)
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
}