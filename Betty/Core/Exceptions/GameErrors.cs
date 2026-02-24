namespace Betty.Core.Exceptions;

public static class GameErrors
{
    // facotry
    public static GameException InvalidBetAmount(decimal min, decimal max) =>
        new($"Bet amount must be between ${min:F2} and ${max:F2}.");

    public static GameException InsufficientFundsForBet(decimal balance) =>
        new($"Insufficient funds to place bet. Your current balance is: ${balance:F2}");
}