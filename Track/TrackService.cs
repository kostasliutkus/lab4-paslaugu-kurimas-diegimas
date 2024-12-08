namespace Servers;

using TrackContractServices;

/// <summary>
/// Service
/// </summary>
public class TrackService : ITrackService
{
	//NOTE: instance-per-request service would need logic to be static or injected from a singleton instance
	private readonly TrackLogic mLogic = new TrackLogic();

    /// <summary>
    /// Get next unique ID from the server. Is used by referees to acquire client ID's.
    /// </summary>
    /// <returns>Unique ID.</returns>
    public int GetRefereeUniqueId() 
	{
		return mLogic.GetUniqueRefereeId();
	}
	 /// <summary>
    /// Get next unique ID from the server. Is used by runners to acquire client ID's.
    /// </summary>
    /// <returns>Unique ID.</returns>
	public int GetRunnerUniqueId() 
	{
		return mLogic.GetUniqueRunnerId();
	}

	/// <summary>
	/// Get current track state.
	/// </summary>
	/// <returns>Current light state.</returns>				
	public TrackState GetTrackState()
    {
		return (TrackState)mLogic.GetTrackState();
    }
	/// <summary>
	/// Send Vote to Server
	/// </summary>
	/// <param name="rating">boolean rating true or false</param>

	public bool SendVote(int id,Boolean rating)
    {
		return mLogic.ProcessVote(id,rating);
    }
	/// <summary>
	/// Adds distance change to runner dictionary
	/// </summary>
	/// <param name="runner">runner that generates distance</param>
	/// <param name="distance">generated amount</param>
	/// <returns>returns true if success and false if failure</returns>
	public bool AddDistanceChange(RunnerDesc runner, double distance) 
	{
		var servicesRunner = new Services.RunnerDesc
		{
			RunnerId = runner.RunnerId,
			RunnerNameSurname = runner.RunnerNameSurname,
			// Map other properties as needed
		};
		return mLogic.AddDistanceChange(servicesRunner, distance);
	}
}