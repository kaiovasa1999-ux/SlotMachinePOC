using Betty.Core.Interfaces;

namespace Betty.Infrastructure;

public class MyRandom : IRandom
{
    private readonly Random _random = new Random();

    public double NextDouble() => _random.NextDouble();
  
}
