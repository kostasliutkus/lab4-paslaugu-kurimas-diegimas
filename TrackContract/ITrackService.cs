namespace TrackContractServices;


/// <summary>
/// Runner descriptor.
/// </summary>
public class RunnerDesc
{
	/// <summary>
	/// Runner ID.
	/// </summary>
	public int RunnerId { get; set; }

	/// <summary>
	/// Runner name and surname.
	/// </summary>
	public string RunnerNameSurname { get; set; }
}
/// <summary>
/// Referee Descriptor
/// </summary>
public class RefereeDesc
{
	/// <summary>
	/// Referee ID.
	/// </summary>
	public int RefereeId { get; set; }
	
	/// <summary>
	/// Value of the refferees vote
	/// </summary>
	public Boolean Vote {get;set;}

	/// <summary>
	/// Referee name and surname.
	/// </summary>
	public string RefereeNameSurname { get; set; }
}

/// <summary>
/// Track state.
/// </summary>
public enum TrackState : int
{
	GettingReady,
	Running,
	Done
}


/// <summary>
/// Service contract.
/// </summary>
public interface ITrackService
{
	/// <summary>
	/// Get next unique ID from the server. Is used by runners to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	int GetRunnerUniqueId();

	/// <summary>
	/// Get next unique ID from the server. Is used by referees to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	int GetRefereeUniqueId();

	/// <summary>
	/// Get current track state.
	/// </summary>
	/// <returns>Current track state.</returns>				
	TrackState GetTrackState();

	/// <summary>
	/// Send Vote to Server
	/// </summary>
	/// <param name="rating">boolean rating true or false</param>
	/// <returns>true if success false if failure</returns>
	bool SendVote(int id,Boolean rating);

	/// <summary>
	/// Adds distance change to runner dictionary
	/// </summary>
	/// <param name="runner">runner that generates distance</param>
	/// <param name="distance">generated amount</param>
	/// <returns>returns true if success and false if failure</returns>
	public bool AddDistanceChange(RunnerDesc runner,double distance);
}
