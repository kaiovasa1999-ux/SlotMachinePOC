using Betty.Core.Entities;
using Betty.Core.Exceptions;
using Betty.Core.Interfaces;

public class Wallet : IWallet
{
    private readonly IWalletRepository _repo;

    public decimal Balance { get; private set; }

    public Wallet(IWalletRepository repo )
    {
        _repo = repo;
        Balance = _repo.GetCurrentBalance();
    }

    public void Deposit(decimal amount)
    {
        AmountMustBePositive(amount);

        Balance += amount;
        _repo.AddTransactionData(new TransactionData
        {
            Type = TransactionDataType.Deposit,
            Amount = amount,
            BalanceAfter = Balance,
        });
    }


    public void Withdrawal(decimal amount)
    {
        AmountMustBePositive(amount);
        if (amount > Balance)
            throw WalletErrors.InsufficientFunds(Balance);


        Balance -= amount;
        _repo.AddTransactionData(new TransactionData
        {
            Type =TransactionDataType.Withdrawal,
            Amount = amount,
            BalanceAfter = Balance,
        });
    }

    public void ChangeBalanceBaseOnBetOutcome(decimal initialBetAmount, decimal betOutcome)
    {
        AmountMustBePositive(initialBetAmount);
        decimal totalDiff = betOutcome - initialBetAmount;

        //totalDiff may be negative
        if ((Balance + totalDiff) <0)
            throw WalletErrors.InsufficientFunds(Balance);

        Balance += totalDiff;
        _repo.AddTransactionData(new TransactionData
        {
            Type = totalDiff >= 0 ? TransactionDataType.BetWin : TransactionDataType.BetLoss,
            Amount = initialBetAmount,
            BalanceAfter = Balance,
        });
    }

    public IReadOnlyCollection<TransactionData> GetTransactionHistory()
       =>  _repo.GetAll();

    private static void AmountMustBePositive(decimal amount)
    {
        if (amount <= 0)
            throw WalletErrors.InvalidAmount(amount);
    }

}