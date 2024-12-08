namespace Servers;

using Grpc.Core;

using Services;

/// <summary>
/// Service
/// </summary>
public class GrpcTrackService : Services.Track.TrackBase
{
	//NOTE: instance-per-request service would need logic to be static or injected from a singleton instance
	private readonly TrackLogic mLogic = new TrackLogic();

    /// <summary>
	/// Get next unique ID from the server. Is used by Referee to acquire client ID's.
	/// </summary>
	/// <param name="input">Not used.</param>
	/// <param name="context">Call context.</param>
	/// <returns>Unique ID.</returns>
    public override Task<IntMsg> GetRefereeUniqueId(Empty input, ServerCallContext context) 
	{
		var result = new IntMsg {Value = mLogic.GetUniqueRefereeId() };
		return Task.FromResult(result);
	}
	/// <summary>
	/// Get next unique ID from the server. Is used by Referee to acquire client ID's.
	/// </summary>
	/// <param name="input">Not used.</param>
	/// <param name="context">Call context.</param>
	/// <returns>Unique ID.</returns>
    public override Task<IntMsg> GetRunnerUniqueId(Empty input, ServerCallContext context) 
	{
		var result = new IntMsg {Value = mLogic.GetUniqueRunnerId() };
		return Task.FromResult(result);
	}

	/// <summary>
	/// Get current track state.
	/// </summary>
	/// <param name="input">Not used.</param>
	/// <param name="context">Call context.</param>
	/// <returns>Current track state.</returns>				
	public override Task<GetTrackStateOutput> GetTrackState(Empty input, ServerCallContext context)
    {
		var logicTrackState = mLogic.GetTrackState();
		var serviecTrackState = (Services.TrackState)logicTrackState; //this will only work properly if enumerations are by-value compatible

		var result = new GetTrackStateOutput { Value = serviecTrackState };
		return Task.FromResult(result);
    }

	/// <summary>
	/// Send Vote to Server
	/// </summary>
	/// <param name="Input">Input containing id of referee and the rating value</param>
	/// <param name="context">Call context.</param>
	public override Task<BoolMsg> SendVote(Services.SendVoteInput input, ServerCallContext context)
    {
		var id = input.Id;
		var rating = input.Rating;
		var result = new BoolMsg { Value = mLogic.ProcessVote(id.Value,rating.Value) };
		return Task.FromResult(result);
    }

	/// <summary>
	/// Adds distance change to runner dictionary
	/// </summary>
	/// <param name="input">runner that generates distance and the generated amount</param>
	/// <returns>returns true if success and false if failure</returns>
	public override Task<BoolMsg> AddDistanceChange(Services.AddDistanceChangeInput input,ServerCallContext contextt) 
	{
		//convert input to the format expected by logic
		var runnerData = input.Runner;
		var runner = new RunnerDesc {
			RunnerId = runnerData.RunnerId,
			RunnerNameSurname = runnerData.RunnerNameSurname
		};
		//
		var distance= input.Distance;

		var logicResult = mLogic.AddDistanceChange(runner,distance.Value);

		//convert result to the format expected by gRPC
		var result = new BoolMsg { Value = logicResult };

		return Task.FromResult(result);
	}
}