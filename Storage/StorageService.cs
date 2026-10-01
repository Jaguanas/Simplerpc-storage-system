namespace Servers;

using Services;

/// <summary>
/// Service
/// </summary>
public class StorageService : IStorageService
{
	//NOTE: instance-per-request service would need logic to be static or injected from a singleton instance
	private readonly StorageLogic mLogic = new StorageLogic();

    public int RegisterBaker(BakerDesc baker)
    {
        // Forward the RPC call to the state-owning logic class.
        return mLogic.RegisterBaker(baker);
    }

    public int RegisterFarmer(FarmerDesc farmer)
    {
		// Forward farmer registration to the shared logic instance.
        return mLogic.RegisterFarmer(farmer);
    }

    public Season GetSeasonState()
    {
		// Return the current server-side season.
        return mLogic.GetSeasonState();
    }

    public GrainOperationResult ConsumeGrain(BakerDesc baker, int amount)
    {
		// Forward baker consumption without duplicating business rules.
        return mLogic.ConsumeGrain(baker, amount);
    }

    public GrainOperationResult TryDepositGrain(FarmerDesc farmer, int amount)
    {
		// Forward farmer deposit without duplicating business rules.
        return mLogic.TryDepositGrain(farmer, amount);
    }
}