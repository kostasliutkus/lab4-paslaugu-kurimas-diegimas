namespace TranslationLayer;

using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using Grpc.Core;
using Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GrpcService;
public class TrackTranslationLayer : GrpcService.Track.TrackBase
{
    private readonly ITrackService _trackService;
    private readonly ILogger<TrackTranslationLayer> _log;

    public TrackTranslationLayer()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _log = factory.CreateLogger<TrackTranslationLayer>();

        var sc = new ServiceCollection();

        // SimpleRPC connection
        sc
            .AddSimpleRpcClient(
                "TrackService",
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/simplerpc",
                    Serializer = "HyperionMessageSerializer"
                }
            )
            .AddSimpleRpcHyperionSerializer();

        sc.AddSimpleRpcProxy<ITrackService>("TrackService");

        var sp = sc.BuildServiceProvider();

        // Get and return service
        _trackService = sp.GetService<ITrackService>();
    }

    public override Task<IntMsg> GetRefereeUniqueId(Empty request, ServerCallContext context)
    {
        var result = _trackService.GetRefereeUniqueId();
        return Task.FromResult(new IntMsg { Value = result });
    }

    public override Task<GetTrackStateOutput> GetTrackState(Empty request, ServerCallContext context)
    {
        var result = _trackService.GetTrackState();
        return Task.FromResult(new GetTrackStateOutput { Value = (GrpcService.TrackState)result });
    }

    public override Task<BoolMsg> SendVote(SendVoteInput request, ServerCallContext context)
    {
        var result = _trackService.SendVote(request.Id.Value, request.Rating.Value);
        return Task.FromResult(new BoolMsg { Value = result });
    }

    public override Task<BoolMsg> AddDistanceChange(AddDistanceChangeInput request, ServerCallContext context)
    {
        var result = _trackService.AddDistanceChange(
        new Services.RunnerDesc
        {
            RunnerId = request.Runner.RunnerId,
            RunnerNameSurname = request.Runner.RunnerNameSurname
        }, request.Distance.Value);
        return Task.FromResult(new BoolMsg { Value = result });
    }
}