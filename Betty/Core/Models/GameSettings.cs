namespace Betty.Core.Models;

public record GameSettings
{
    public decimal MinBet { get; init; }
    public decimal MaxBet { get; init; }
    public double LossBandThreshold { get; init; }
    public double MediumWinBandThreshold { get; init; }
    public double MediumWinMinMultiplier { get; init; }
    public double MediumWinMaxMultiplier { get; init; }
    public double BigWinMinMultiplier { get; init; }
    public double BigWinMaxMultiplier { get; init; }
}
