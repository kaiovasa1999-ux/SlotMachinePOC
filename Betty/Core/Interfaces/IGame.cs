using Betty.Core.Models;
namespace Betty.Core.Interfaces;

public interface IGame
{
    BetResult PlaceBet(decimal amount);
}