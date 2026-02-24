using Betty.Core.Entities;
using Betty.Core.Interfaces;

namespace Betty.Infrastructure;

internal class InMemoryWalletRepository : IWalletRepository
{
    // here We can use dbContext
    private readonly List<TransactionData> _transactions = new();

    public void AddTransactionData(TransactionData transactionData)
    {
        if (transactionData == null)
            throw new NullReferenceException("Transaction cannot be null");

         _transactions.Add(transactionData);
    }


    //All my call methods need just readability, they don't need to mutate the state of transactions
    public IReadOnlyCollection<TransactionData> GetAll() =>
         _transactions.AsReadOnly();

    public decimal GetCurrentBalance()
    {
        if(_transactions.Count == 0)
        {
            return 0;
        }
        //gives me O(1) TimeComplexity 
        return _transactions[^1].BalanceAfter;
    }
}
