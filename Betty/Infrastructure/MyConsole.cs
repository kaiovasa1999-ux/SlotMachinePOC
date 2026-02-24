using Betty.Core.Interfaces;

namespace Betty.Infrastructure;

public class MyConsole : IConsole
{
    public void WriteLine(string message = "") => Console.WriteLine(message);
    public string? ReadLine() => Console.ReadLine();

    public void ReadKey() => Console.ReadKey();
}
