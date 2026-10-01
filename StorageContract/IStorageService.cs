namespace Services;

// Data sent when a baker registers with the storage service.
public class BakerDesc
{
    // ID is assigned by the server after registration.
	public int Id { get; set; }
    // Display name of the baker.
	public string NameSurname { get; set; }
}

// Data sent when a farmer registers with the storage service.
public class FarmerDesc
{
    // ID is assigned by the server after registration.
	public int Id { get; set; }
    // Display name of the farmer.
	public string NameSurname { get; set; }
}

/// <summary>
/// Season state.
/// </summary>
public enum Season : int
{
	// Initial season and the season in which farmers cannot deposit grain.
    Winter,
	// Farmers are inactive while crops grow.
    Spring,
	// Farmers can deposit grain.
    Summer,
	// Farmers can deposit grain.
    Autumn
}

// Possible outcomes returned by grain operations.
public enum GrainOperationStatus
{
    Success,
    NotEnoughGrain,
    StorageFull,
    SeasonChanged,
    InvalidAmount,
	InactiveSeason
}

// Result object returned from the server after a grain operation.
public class GrainOperationResult
{
	// Indicates whether the operation succeeded or why it failed.
    public GrainOperationStatus Status { get; set; }
	// Number of grains accepted or consumed.
    public int Amount { get; set; }
	// Human-readable explanation of the result.
    public string Reason { get; set; }
}

/// <summary>
/// Service contract.
/// </summary>
public interface IStorageService
{
    // Registers a baker and returns a server-generated ID.
	int RegisterBaker(BakerDesc baker);
    // Registers a farmer and returns a server-generated ID.
    int RegisterFarmer(FarmerDesc farmer);

	/// <summary>
	/// Get current season state.
	/// </summary>
	/// <returns>Current season state.</returns>				
	Season GetSeasonState();

	// Removes grain from storage for a baker.
	GrainOperationResult ConsumeGrain(
        BakerDesc baker,
        int amount);

	// Adds grain to storage for a farmer when the current season allows it.
    GrainOperationResult TryDepositGrain(
        FarmerDesc farmer,
        int amount);
}
