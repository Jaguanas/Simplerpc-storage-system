namespace Servers;

// Imports logging and the shared RPC contract types.
using NLog;

using Services;

public class StorageState
{
	// All access to the mutable state is synchronized with this lock.
    public readonly object AccessLock = new();

	// Current season, starting in winter.
    public Season CurrentSeason = Season.Winter;
	// Number of grains currently stored.
    public int GrainAmount;
	// Maximum number of grains the storage can hold.
    public int MaximumGrainAmount = 1000;

	// Profit made from successful consumption operations.
    public double EarnedProfit;
	// Profit that could not be made because storage lacked grain.
    public double MissedProfit;

	// Counter used to assign unique IDs to clients.
    public int LastUniqueId;
}


/// <summary>
/// <para>Storage logic.</para>
/// <para>Thread safe.</para>
/// </summary>
class StorageLogic
{
	// Profit attributed to each successfully consumed grain.
	private const double ProfitPerGrain = 1.5;

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
	private StorageState mState = new StorageState();

	/// <summary>
	/// Constructor.
	/// </summary>
	public StorageLogic()
	{
		// Start the background task of season-changing worker without blocking the web server.
		mBgTaskThread = new Thread(BackgroundTask)
		{
			IsBackground = true
		};
		mBgTaskThread.Start();
	}

	public int RegisterBaker(BakerDesc baker) 
	{
		// Registration updates shared state, so it must be atomic.
		lock( mState.AccessLock )
		{
			mState.LastUniqueId++;
			mLog.Info($"Baker registered: {mState.LastUniqueId} {baker.NameSurname}.");
			return mState.LastUniqueId;
		}
	}

	public int RegisterFarmer(FarmerDesc farmer)
	{
		// Use the same counter so all client IDs are globally unique.
		lock( mState.AccessLock )
		{
			mState.LastUniqueId++;
			mLog.Info($"Farmer registered: {mState.LastUniqueId} {farmer.NameSurname}.");
			return mState.LastUniqueId;
		}
	}

	/// <summary>
	/// Get current season state.
	/// </summary>
	/// <returns>Current season state.</returns>				
	public Season GetSeasonState() 
	{
		lock( mState.AccessLock )
		{
			return mState.CurrentSeason;
		}
	}

	public GrainOperationResult ConsumeGrain(BakerDesc baker, int amount)
	{
		// Validate and update the balance as one synchronized operation.
		lock (mState.AccessLock)
		{
			mLog.Info($"Baker {baker.Id} is trying to consume this amount of grains: {amount}...");

			if (amount < 0)
			{
				// Reject invalid input before calculating or changing anything.
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.InvalidAmount,
					Amount = 0,
					Reason = "Amount cannot be negative."
				};
			}

			double profitChange = amount * ProfitPerGrain;

			if(mState.GrainAmount < amount)
			{
				// Consumption fails when the requested amount is unavailable.
				mState.MissedProfit += profitChange;

				mLog.Info($"Baker {baker.Id} failed to consume {amount} grains. " +
                	$"Missed profit increased by {profitChange}.");
				
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.NotEnoughGrain,
					Amount = 0,
					Reason = $"Not enough grains. Grains left: {mState.GrainAmount}"
				};
			}

			// Successful consumption decreases stock and increases profit.
			mState.GrainAmount -= amount;
			mState.EarnedProfit += profitChange;

			mLog.Info($"Baker {baker.Id} ({baker.NameSurname}) consumed {amount} grains. " +
            	$"Remaining grains: {mState.GrainAmount}. " +
            	$"Earned profit increased by {profitChange}.");

			return new GrainOperationResult
			{
				Status = GrainOperationStatus.Success,
				Amount = amount,
				Reason = $"Grains consumed successfully. Total amount of profit: {mState.EarnedProfit}"
			};
		}
	}

    public GrainOperationResult TryDepositGrain(FarmerDesc farmer, int amount)
	{
		// Validate the request and modify storage atomically.
		lock (mState.AccessLock)
		{
			mLog.Info($"Farmer {farmer.Id} is trying to deposit this amount of grains: {amount}...");

			if(amount < 0)
			{
				// Negative deposits are never valid.
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.InvalidAmount,
					Amount = 0,
					Reason = "Amount cannot be negative."
				};
			}

			if (mState.CurrentSeason == Season.Winter)
			{
				// Winter invalidates the farmer's generated portion.
				mLog.Info($"Farmer {farmer.Id}'s grain portion was lost because it has changed to winter.");
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.SeasonChanged,
					Amount = 0,
					Reason = "Season changed to winter."
				};
			}

			if (mState.CurrentSeason == Season.Spring)
			{
				// Farmers do not generate grain during spring.
				mLog.Info($"Farmer {farmer.Id} can't deposit in spring.");
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.InactiveSeason,
					Amount = 0,
					Reason = "Farmers are inactive during spring."
				};
			}

			if (amount > mState.MaximumGrainAmount - mState.GrainAmount)
			{
				// Reject deposits that would exceed the storage limit.
				mLog.Info($"There wasn't enought storage for farmer {farmer.Id} to deposit his grains.");
				return new GrainOperationResult
				{
					Status = GrainOperationStatus.StorageFull,
					Amount = 0,
					Reason = "Not enough storage."
				};
			}

			// The request passed all checks, so add the grain to storage.
			mState.GrainAmount += amount;
			
			mLog.Info($"Farmer {farmer.Id} ({farmer.NameSurname}) deposited his grains successfully!");
			return new GrainOperationResult
			{
				Status = GrainOperationStatus.Success,
				Amount = amount,
				Reason = $"Farmer {farmer.Id} ({farmer.NameSurname}) has deposited grains successfully. Total amount of grains: {mState.GrainAmount}"
			};
		}
	}

	public void BackgroundTask()
	{
		// Rotate through the four seasons independently of client requests.
		while( true )
		{
			// Wait before changing to the next season.
			Thread.Sleep(15000);

			lock (mState.AccessLock)
    		{
				Season oldSeason = mState.CurrentSeason;

				// The modulo operation wraps Autumn back to Winter.
				mState.CurrentSeason =
					(Season)(((int)mState.CurrentSeason + 1) % 4);

				mLog.Info($"The season has changed from {oldSeason} to {mState.CurrentSeason}.");
    		}
		}
	}
}