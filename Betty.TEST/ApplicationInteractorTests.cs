using Betty.Core.Entities;
using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Betty.Core.Models;
using Betty.Infrastructure;
using Moq;

namespace Betty.TEST;

public class ApplicationInteractorTests
{
    private readonly Mock<IWallet> _wallet;
    private readonly Mock<IGame> _game;
    private readonly Mock<IConsole> _myConsole;
    private readonly ApplicationInteractor _application;

    public ApplicationInteractorTests()
    {
        _wallet = new Mock<IWallet>();
        _game = new Mock<IGame>();
        _myConsole = new Mock<IConsole>();
        _application = new ApplicationInteractor(_wallet.Object, _game.Object, _myConsole.Object);
    }

    #region CommandParser
    [Fact]
    public void CommandParser_Exit_Should_ReturnTrue()
    {
        //arange & act
        var result = _application.CommandPareser("exit", out string command, out decimal amount);

        //assert
        Assert.True(result);
        Assert.Equal("exit", command);
        Assert.Equal(0m, amount);
    }
    [Fact]
    public void CommandParser_History_Should_RetrunTrue()
    {
        //arange & act
        var result = _application.CommandPareser("history", out string command, out decimal amount);

        //assert
        Assert.True(result);
        Assert.Equal("history", command);
        Assert.Equal(0m, amount);
    }

    [Theory]
    [InlineData("deposit 123", "deposit", 123)]
    [InlineData("withdrawal 111", "withdrawal", 111)]
    [InlineData("bet 100", "bet", 100)]
    [InlineData("DEPOSit 123", "deposit", 123)]
    [InlineData("withdraWal 111", "withdrawal", 111)]
    [InlineData("bET 100", "bet", 100)]

    public void CommandParse_Should_ReturnsTrue_WhenInputIsCorrectSymbols_KeyInsensitive_SamllorBigCharacters(string input, string commandResult, decimal amountResult)
    {
        //arange & act
        var result = _application.CommandPareser(input, out string command, out decimal amount);

        //assert 
        Assert.True(result);
        Assert.Equal(commandResult, command);
        Assert.Equal(amountResult, amount);
    }
    [Fact]
    public void CommandParser_ExtraWhitespace_ParsedCorrectly()
    {
        //arange & act
        var result = _application.CommandPareser("deposit    50", out string command, out decimal amount);

        //assert
        Assert.True(result);
        Assert.Equal("deposit", command);
        Assert.Equal(50m, amount);
    }
    #endregion

    #region Deposit

    [Fact]
    public void Run_DepositCommand_Should_PrintSuccessMessage()
    {
        //arange & act
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("deposit 10").Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s => 
            s.Contains("10"))), Times.AtLeastOnce);
    }
    [Fact]
    public void Run_DepositCommand_Should_PrintException_When_DepositIsNegative()
    {
        //arange & act
        _wallet.Setup(w => w.Deposit(It.IsAny<decimal>()))
         .Throws(WalletErrors.InvalidAmount(0m));
        _myConsole.SetupSequence(c => 
            c.ReadLine()).Returns("deposit -1").Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s => 
        s.Contains("positive"))), Times.AtLeastOnce);
    }
    #endregion

    #region Withdrawal
    [Fact]
    public void Run_WithdrawalCommand_PrintsSuccessMessage()
    {
        //arange & act
        _wallet.Setup(w => w.Balance).Returns(50m);
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("withdrawal 50").Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s =>
            s.Contains("50.00"))), Times.AtLeastOnce);
    }

    [Fact]
    public void Run_WithdrawalCommand_ShouldPrint_InsufficientFunds_WhenThereIsNotEnoguth_In_Wallet()
    {
        //arange & act
        _wallet.Setup(w => w.Withdrawal(It.IsAny<decimal>()))
            .Throws(WalletErrors.InsufficientFunds(0m));
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("withdrawal 999")
            .Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s =>
            s.Contains("Insufficient"))), Times.Once);
    }
    #endregion

    #region Bet
    [Fact]
    public void Run_BetCommand_Win_CallsPlaceBet()
    {
        //arange & act
        _game.Setup(g => g.PlaceBet(10m))
            .Returns(new BetResult(10m, 20m, true, 120m));
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("bet 10").Returns("exit");
        _application.Run();

        ///assert
        _game.Verify(g => g.PlaceBet(10m), Times.Once);
    }
    [Fact]
    public void Run_BetCommand_Win_PrintsWinMessage()
    {
        //aragen *& act
        _game.Setup(g => g.PlaceBet(10m)).Returns(new BetResult(10, 20m, true, 120m));
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("bet 10").Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("won"))), Times.AtLeastOnce);
    }

    [Fact]
    public void Run_BetCommand_Lose_PrintsLoseMessage()
    {

        //araange & act
        _game.Setup(g => g.PlaceBet(10m)).Returns(new BetResult(10, 0m, false, 90m));
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("bet 10").Returns("exit");
        _application.Run();
        //asert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("losed"))), Times.AtLeastOnce);
    }
    [Fact]

    public void Run_BetCommand_Should_ThrowsException_WhenOutOfRange()
    {
        //arange & act
        _game.Setup(g => g.PlaceBet(It.IsAny<decimal>())).Throws(GameErrors.InvalidBetAmount(1m, 10m));
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("bet 11").Returns("exit");
        _application.Run();
        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("amount"))), Times.AtLeastOnce);
    }
    #endregion

    #region History

    [Fact]

    public void Run_HistroyCommand_Should_PrinTransactions()
    {
        var transactions = new List<TransactionData>();
        var tran = new TransactionData { Type = TransactionDataType.Deposit, Amount = 100m, BalanceAfter = 100m };
        transactions.Add(tran);

        _wallet.Setup(w => w.GetTransactionHistory())
            .Returns(transactions.AsReadOnly());

        _myConsole.SetupSequence(c => c.ReadLine()).Returns("history").Returns("exit");

        _application.Run();

        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s =>
            s.Contains("100.00"))), Times.AtLeastOnce);

    }

    [Fact]
    public void Run_HistoryCommand_NoTransactions_PrintsNoTransactionsMessage()
    {
        //arange & act
        _wallet.Setup(w => w.GetTransactionHistory())
                .Returns(new List<TransactionData>().AsReadOnly());

        _myConsole.SetupSequence(c => c.ReadLine())
            .Returns("history")
            .Returns("exit");
        _application.Run();

        //asert
        _myConsole.Verify(c => c.WriteLine("No transactions yet."), Times.Once);
    }
    #endregion

    #region Exit
    [Fact]
    public void Run_ExitCommand_PrintsSeeYouAgain()
    {
        _myConsole.Setup(c => c.ReadLine()).Returns("exit");
        _application.Run();

        _myConsole.Verify(c => c.WriteLine(It.Is<string>(c => c.Contains("see you "))), Times.AtLeastOnce);
    }
    #endregion

    #region Wrong Input
    [Fact]
    public void Run_InvalidCommand_PrintsErrorMessage()
    {
        //arange & act
        _myConsole.SetupSequence(c => c.ReadLine()).Returns("invalidcommand").Returns("exit");
        _application.Run();

        //assert
        _myConsole.Verify(c => c.WriteLine(It.Is<string>(s =>
            s.Contains("ERROR"))), Times.Once);
    }
    #endregion 
}
