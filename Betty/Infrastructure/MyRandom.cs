using Betty.Core.Interfaces;

namespace Betty.Infrastrucutre;

public class MyRandom : IRandom
{
    private readonly Random _random = new Random();

    public double NextDouble() => _random.NextDouble();
  
}
