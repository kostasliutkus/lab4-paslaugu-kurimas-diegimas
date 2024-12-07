using Grpc.Core;
using SimpleRpc.Transports.Http.Client;
using Services;

public class TrackGrpcAdapter : TrackService.TrackServiceBase
{
    private readonly ITrackService _trackService;

    public TrackGrpcAdapter()
    {
        var rpcClient = new SimpleRpcClient("http://localhost:5000/simplerpc");
        _trackService = rpcClient.Create<ITrackService>();
    }

    public override Task<TrackStateResponse> GetTrackState(Empty request, ServerCallContext context)
    {
        var state = _trackService.GetTrackState();
        return Task.FromResult(new TrackStateResponse { State = state.ToString() });
    }

    public override Task<VoteResponse> SendVote(VoteRequest request, ServerCallContext context)
    {
        var result = _trackService.SendVote(request.Id, request.Rating);
        return Task.FromResult(new VoteResponse { Success = result });
    }
}
