
namespace Betty.Core.Interfaces;

/// <summary>
/// Abstraction over random number generation.
///Allows the random behavior to be mocked in unit tests,
/// and make my tests deterministic.
/// </summary>
public interface IRandom
{
    double NextDouble();
}
