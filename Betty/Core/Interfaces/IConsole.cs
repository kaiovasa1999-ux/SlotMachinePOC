namespace Betty.Core.Interfaces;

//
/// <summary>
/// I am doing this, to enable me to mock for my testing
/// This Interface helps me to decouple ApplicationInteractor from System.Console 
/// </summary>
/// <param name="message"></param>
public interface IConsole
{

    void WriteLine(string message = "");
    string? ReadLine();
    void ReadKey();
}
