using Betty.Core.Entities;
namespace Betty.Core.Interfaces;

public interface IWallet
{
    decimal Balance { get; }
    void Deposit(decimal amount);
    void Withdrawal(decimal amount);
    void ChangeBalanceBaseOnBetOutcome(decimal initialBetAmount, decimal betOutcome);
    IReadOnlyCollection<TransactionData> GetTransactionHistory();
}