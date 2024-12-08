namespace Servers;
using Services;
using Microsoft.AspNetCore.Mvc;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
/// <summary>
/// Service. Class must be marked public, otherwise ASP.NET core runtime will not find it.
/// 
/// Look into FromXXX attributes if you need to map inputs to custom parts of HTTP request.
/// </summary>
public class TrackController : ControllerBase
{
	/// <summary>
	/// Service logic. This is created in Server.StartServer() and received through DI in constructor.
	/// </summary>
	private readonly ITrackService? mLogic;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="logic">Logic to use. This will get passed through DI.</param>
	public TrackController(ITrackService logic)
	{   
        mLogic = logic;
	}
	/// <summary>
    /// This is a dummy endpoint to ensure RefereeDesc is included in Swagger.
    /// </summary>
    /// <returns>An example RefereeDesc object.</returns>
    [HttpGet("/exampleReferee")]
    public ActionResult<RefereeDesc> GetExampleReferee()
    {
        // Returning a dummy RefereeDesc object just to include it in the Swagger schema
        return new RefereeDesc
        {
            RefereeId = 1,
            RefereeNameSurname = "Example Name",
			Vote = true
        };
    }
		/// <summary>
    /// This is a dummy endpoint to ensure RefereeDesc is included in Swagger.
    /// </summary>
    /// <returns>An example RefereeDesc object.</returns>
    [HttpGet("/exampleRunner")]
    public ActionResult<RunnerDesc> GetExampleRunner()
    {
        // Returning a dummy RefereeDesc object just to include it in the Swagger schema
        return new RunnerDesc
        {
            RunnerId = 1,
            RunnerNameSurname = "Example Name"
        };
    }
	/// <summary>
	/// Get next unique ID from the server. Is used by Referees to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	[HttpGet("/getUniqueRefereeId")]
	public ActionResult<int> GetUniqueRefereeId() 
	{
		return mLogic.GetRefereeUniqueId();
	}

    /// <summary>
	/// Get next unique ID from the server. Is used by Runners to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	[HttpGet("/getUniqueRunnerId")]
	public ActionResult<int> GetUniqueRunnerId() 
	{
		return mLogic.GetRunnerUniqueId();
	}

	/// <summary>
	/// Get current light state.
	/// </summary>
	/// <returns>Current light state.</returns>				
	[HttpGet("/getTrackState")]
	public ActionResult<TrackState> GetTrackState()
	{
		return mLogic.GetTrackState();
	}

    /// <summary>
    /// Adds distance change to server variables
    /// </summary>
    /// <param name="runner">runner whos distance will be adjusted</param>
    /// <param name="distance">distance that will be added to the runner</param>
    /// <returns>true if track state is running and the change will be added, false otherwise</returns>
    [HttpPost("/addDistanceChange")]
	public ActionResult<bool> AddDistanceChange(RunnerDesc runner,double distance)
	{
		return mLogic.AddDistanceChange(runner,distance);
	}

	/// <summary>
	/// Process a vote add it to according counter
	/// </summary>
	/// <param name="id">id of runner</param>
	/// <param name="rating">True or false</param>
	/// <returns>True</returns>
	[HttpPost("/sendVote")]
	public ActionResult<bool> SendVote(int id, Boolean rating) 
	{
		return mLogic.SendVote(id,rating);
	}
}