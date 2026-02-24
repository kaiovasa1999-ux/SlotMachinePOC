using Betty.Core.Entities;

namespace Betty.Core.Interfaces;

public interface IWalletRepository
{
    void AddTransactionData(TransactionData TransactionData);
    IReadOnlyCollection<TransactionData> GetAll();

    decimal GetCurrentBalance();
}
