using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Betty.Core.Models;
using System.Globalization;

public class ApplicationInteractor
{

    private readonly IWallet _wallet;
    private readonly IGame _game;
    private readonly IConsole _console;

    public ApplicationInteractor(IWallet wallet, IGame game, IConsole console)
    {
        _wallet = wallet;
        _game = game;
        _console = console;
    }

    public void Run()
    {
        while (true)
        {
            try
            {
                _console.WriteLine("Enter next command");
                string? input = _console.ReadLine()?.Trim();// possible null

                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (!CommandPareser(input, out string command, out decimal amount))
                {
                    _console.WriteLine("ERROR!, invalid input command");
                    continue;
                }

                switch (command)
                {
                    case "deposit":
                        DepositHandler(amount);
                        break;
                    case "withdrawal":
                        WithdrawalHandler(amount);
                        break;
                    case "bet":
                        BetHandler(amount);
                        break;
                    case "history":
                        HistoryHandler();
                        break;
                    case "exit":
                        ExitHandler();
                        return;
                }

                _console.WriteLine();
            }
            catch (WalletException ex)
            {
                _console.WriteLine(ex.Message);
            }
            catch (GameException ex)
            {
                _console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                _console.WriteLine(ex.Message);
            }

        }
    }


    private void ExitHandler()
    {
        _console.WriteLine("Thank you for playing! Hope to see you again soon.");
        _console.WriteLine("\nPress any key to exit.");
        _console.ReadKey();
    }

    private void BetHandler(decimal amount)
    {

        BetResult res = _game.PlaceBet(amount);
        if (res.IsWin)
        {
            _console.WriteLine($"Good JOB you have won {res.WinAmount:f2}");
            _console.WriteLine($"You current balances is: {res.NewBalance:f2}");
        }
        else
        {
            _console.WriteLine($"You losed... Your balance drops to {res.NewBalance:f2}");
        }

    }

    private void WithdrawalHandler(decimal amount)
    {
        _wallet.Withdrawal(amount);
        _console.WriteLine($"Your current balance is: {_wallet.Balance:f2}");
    }

    private void DepositHandler(decimal amount)
    {
        _wallet.Deposit(amount);
        _console.WriteLine($"You have deposit succesfullly {amount:f2}");
        _console.WriteLine($"Your balance is: {_wallet.Balance:f2}");
    }

    private void HistoryHandler()
    {
        var transactions = _wallet.GetTransactionHistory();

        if (!transactions.Any())
        {
            _console.WriteLine("No transactions yet.");
            return;
        }

        _console.WriteLine("------------------------------------------------------------------------------------------");

        foreach (var t in transactions)
        {
            _console.WriteLine(
                $"{t.Id,-4} | {t.Type,-12} | ${t.Amount,-9:F2} | ${t.BalanceAfter,-9:F2} | {t.Timestamp:HH:mm:ss}");
        }

        _console.WriteLine();
    }

    public bool CommandPareser(string input, out string command, out decimal amount)
    {
        command = string.Empty;
        amount = 0m;

        //removes white spaces 'text    123   text  ' from the input.
        var data = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length == 0)
            return false;

        command = data[0].ToLower();

        if (command is "exit" or "history")
            return true;


        if (command is "deposit" or "withdrawal" or "bet")
        {
            if (data.Length < 2)
            {
                _console.WriteLine($"Have to give us specific amount. Example: {command} 123");
                return false;
            }

            if (!decimal.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out amount))// always work with '.' not with ',' 
            {
                _console.WriteLine("Invalid amount data. Example: 5.15 is valid format");
                return false;
            }

            return true;
        }
        return false;
    }
}