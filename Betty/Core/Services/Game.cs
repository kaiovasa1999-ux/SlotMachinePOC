using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Betty.Core.Models;

public class Game : IGame
{
    private readonly Wallet _wallet;
    private readonly GameSettings _settings;
    private readonly IRandom _random;

    public Game(Wallet wallet, GameSettings settings, IRandom random)
    {
        _wallet  = wallet;
        _random = random;
        _settings = settings;
    }

    public BetResult PlaceBet(decimal amount)
    {
        if (amount < _settings.MinBet || amount > _settings.MaxBet)
            throw GameErrors.InvalidBetAmount(_settings.MinBet, _settings.MaxBet);

        var betOutcome = BetOutcome(amount);
        _wallet.ChangeBalanceBaseOnBetOutcome(amount, betOutcome);
        var newBalacne = _wallet.Balance;
        var win = betOutcome > 0;
        return new BetResult(amount, betOutcome, win, newBalacne);
    }

    /// <summary>
    /// Core Logic for win and lose probabilites (the brain  of our Game)
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private decimal BetOutcome(decimal amount)
    {
        double roll = _random.NextDouble();
        if (roll < _settings.LossBandThreshold)
        {
            // we are losing
            return 0m;
        }
        else if(roll < _settings.MediumWinBandThreshold)
        {
            // 40 % change (90-50)
            double multiplier = _settings.MediumWinMinMultiplier
                + (_random.NextDouble() * (_settings.MediumWinMaxMultiplier - _settings.MediumWinMinMultiplier));
            var res = amount * (decimal)multiplier;
            return RoundDown(res);
        }
        else
        {
            // all the others 10% chance for big win
            var multiplier = _settings.BigWinMinMultiplier 
                + (_random.NextDouble() * (_settings.BigWinMaxMultiplier - _settings.BigWinMinMultiplier));
            var res = amount * (decimal)multiplier;
            return RoundDown(res);
        }
    }

    private static decimal RoundDown(decimal value)
    {
       return Math.Floor(value * 100) / 100;
    }
}