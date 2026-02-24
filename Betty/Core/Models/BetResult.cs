namespace Betty.Core.Models;

public record BetResult(decimal BetAmount, decimal WinAmount, bool IsWin, decimal NewBalance);
