namespace Betty.Core.Exceptions;

public static class WalletErrors
{
    public static WalletException InsufficientFunds(decimal balance) =>
       new($"Insufficient funds. Your current balance is: ${balance:F2}");

    public static WalletException InvalidAmount(decimal amount) =>
        new("The amount must be positive!");
}
