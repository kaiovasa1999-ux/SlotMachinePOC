namespace Betty.Core.Entities;

public class TransactionData
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required TransactionDataType Type { get; init; }
    public required decimal Amount { get; init; }
    public required decimal BalanceAfter { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}