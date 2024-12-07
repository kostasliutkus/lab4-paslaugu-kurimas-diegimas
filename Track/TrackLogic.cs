namespace Servers;

using Castle.Core.Internal;
using NLog;

using Services;

/// <summary>
/// Track  descriptor.
/// </summary>
public class TrackStateDesc
{
	/// <summary>
	/// Access lock.
	/// </summary>
	public readonly object AccessLock = new object();

	/// <summary>
	/// Last unique ID value generated.
	/// </summary>
	public int LastUniqueRefereeId;

	public int LastUniqueRunnerId;

	/// <summary>
	/// Track state.
	/// </summary>
	public TrackState trackState = TrackState.GettingReady;

	/// <summary>
	/// Runner Distance and uID dictionary.
	/// </summary>
	public Dictionary<int,double> RunnerTotalDistance = new Dictionary<int,double>();
}


/// <summary>
/// <para>Track logic.</para>
/// <para>Thread safe.</para>
/// </summary>
class TrackLogic
{
	/// <summary>
	/// Logger for this class.
	/// </summary>
	private Logger mLog = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Background task thread.
	/// </summary>
	private Thread mBgTaskThread;

	/// <summary>
	/// State descriptor.
	/// </summary>
	private TrackStateDesc mState = new TrackStateDesc();

	/// <summary>
	/// All Votes Dictionary with id of referee and bool value of the vote
	/// </summary>
	private Dictionary<int,bool> Votes=new Dictionary<int,bool>();

	/// <summary>
	/// Process a vote add it to according counter
	/// </summary>
	/// <param name="rating">True</param>
	public bool ProcessVote(int id,Boolean rating)
	{
		lock( mState.AccessLock )
		{
			Votes[id] = rating;
			return true;
		}
		
	}
	/// <summary>
	/// Constructor.
	/// </summary>
	public TrackLogic()
	{
		//start the background task
		mBgTaskThread = new Thread(BackgroundTask);
		mBgTaskThread.Start();
	}

	/// <summary>
	/// Get next unique ID from the server. Is used by referees to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	public int GetUniqueRefereeId() 
	{
		lock( mState.AccessLock )
		{
			mState.LastUniqueRefereeId += 1;
			return mState.LastUniqueRefereeId;
		}
	}
	/// <summary>
	/// Get next unique ID from the server. Is used by runners to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	public int GetUniqueRunnerId() 
	{
		lock( mState.AccessLock )
		{
			mState.LastUniqueRunnerId += 1;
			return mState.LastUniqueRunnerId;
		}
	}

	/// <summary>
	/// Get current track state.
	/// </summary>
	/// <returns>Current track state.</returns>				
	public TrackState GetTrackState() 
	{
		lock( mState.AccessLock )
		{
			return mState.trackState;
		}
	}
	/// <summary>
	/// Resets distances for new race
	/// </summary>
	public void ResetDistances()
	{
		lock( mState.AccessLock )
		{
			mState.RunnerTotalDistance.Clear();
		}
	}
	/// <summary>
	/// Resets votes for new race
	/// </summary>
	public void ResetVotes()
	{
		lock( mState.AccessLock )
		{
			Votes.Clear();
		}
	}
	/// <summary>
	/// Gets the runner who ran the furthest distance
	/// </summary>
	/// <returns>keyValuePair of the runner id and the distance</returns>
	public KeyValuePair<int,double>? GetFurthestRunner()
	{
		lock( mState.AccessLock )
		{	
			try
			{
				var highestScore = mState.RunnerTotalDistance.OrderByDescending(r=>r.Value).First();
				return highestScore;
			} catch
			{
				return null;
			}
			
		}
	}
	
	/// <summary>
	/// Adds random distance change to random runner
	/// </summary>
	/// <returns>Key value pair of runners id and distance change</returns>
	public KeyValuePair<int,int> AddRandomDistanceChange()
	{
		lock(mState.AccessLock)
		{
			var random = new Random();
			var randomRunnerId = random.Next(0,mState.LastUniqueRefereeId);
			var distanceChange =random.Next(-1,2);
			//mState.LastUniqueRefereeId
			if(mState.RunnerTotalDistance.ContainsKey(randomRunnerId))
			{
				mState.RunnerTotalDistance[randomRunnerId]+=distanceChange;
			}
			else
			{
				mState.RunnerTotalDistance[randomRunnerId]=distanceChange;
			}
			return new KeyValuePair<int,int>(randomRunnerId,distanceChange);
		}
	}
	/// <summary>
	/// Adds distance change to server variables
	/// </summary>
	/// <param name="runner">runner whos distance will be adjusted</param>
	/// <param name="distance">distance that will be added to the runner</param>
	/// <returns></returns>
	public bool AddDistanceChange(RunnerDesc runner,double distance)
	{
		lock( mState.AccessLock )
		{
			if(GetTrackState()==TrackState.Running)
			{
				mLog.Info($"Adding distance change to runner {runner.RunnerId}, name {runner.RunnerNameSurname}. amount: {distance:F2} km.");

				if(mState.RunnerTotalDistance.ContainsKey(runner.RunnerId))
					mState.RunnerTotalDistance[runner.RunnerId]+=distance;
				else
				{
					mState.RunnerTotalDistance.Add(runner.RunnerId,distance);
				}
				return true;		
			}
			else
			{
				return false;
			}
		}
	}
	/// <summary>
	/// Background task for the track.
	/// </summary>
	public void BackgroundTask()
	{
		//intialize random number generator
		var rnd = new Random();
			Console.Title = $"I am Track";
		while( true )
		{
			//sleep a while
			Thread.Sleep(500 + rnd.Next(1500));
			double PositiveVoteDistribution = 0.0;
			lock( mState.AccessLock )
			{
				if(Votes.Keys.Count!=0 && Votes.Keys.Count == mState.LastUniqueRefereeId)
				{
					var voteValues = Votes
						.Select(group => group.Value)
						.ToList();
					
					var uniqueReferees= Votes.Keys.Count;
					ResetVotes();
					Console.ForegroundColor = ConsoleColor.Gray;
					mLog.Info($"\n unique referees \n '{uniqueReferees}'.");
					var positiveVotes = voteValues.Where(x=>x==true).Count();
					var negativeVotes = voteValues.Where(x=>x==false).Count();
					mLog.Info($"Current Positive Votes '{positiveVotes}'.");
					mLog.Info($"Current Negative Votes '{negativeVotes}'.");
					PositiveVoteDistribution = (double)positiveVotes/uniqueReferees;
					mLog.Info($"Current Votes To Change State '{PositiveVoteDistribution:F3}'.");
				}
				else
				{
					mLog.Info($"Not all referees voted");
				}
				if(PositiveVoteDistribution>=0.75)
				{
					//ResetVotes();
					switch (mState.trackState)
					{
						case TrackState.GettingReady:
							mState.trackState = TrackState.Running;
							Console.ForegroundColor = ConsoleColor.Green;
							mLog.Info($"Changing State To 'Running'.");
							break;
						case TrackState.Running:

							mState.trackState = TrackState.Done;
							Console.ForegroundColor = ConsoleColor.Green;
							var randomChangePair = AddRandomDistanceChange();
							mLog.Info($"Adding {randomChangePair.Value} to Runner id: {randomChangePair.Key}");

							mLog.Info($"Evaluating best performing runner'.");
							var bestRunner = GetFurthestRunner();
							if(bestRunner==null)
							{
								//Console.ForegroundColor = ConsoleColor.Red;
								mLog.Info($"No Runners were ready to run");
								continue;
							}
							Console.ForegroundColor = ConsoleColor.Green;
							mLog.Info($"Best Runner this race id: {bestRunner?.Key}, who ran: {bestRunner?.Value:F2} km");
							mLog.Info($"Changing State To 'Done'.");
							break;
						case TrackState.Done:
							ResetDistances();// apnulinami rezultatai
							//Reset votes after finding winner
							//ResetVotes();
							Console.ForegroundColor = ConsoleColor.Green;
							mState.trackState = TrackState.GettingReady;
							mLog.Info($"Changing State To 'Getting Ready'.");
							break;
					}
				}
				
			}
		}
	}
}