using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Betty.Core.Models;
using Moq;

namespace Betty.TEST;

public class GameTests
{
    private readonly GameSettings _settings;
    private readonly Mock<IWallet> _wallet;

    // the how idea to have IRandom Mock is to be ablet to controll random for differant cases (deterministic tests)
    private readonly Mock<IRandom> _random;
    
    public GameTests()
    {
        _wallet = new Mock<IWallet>();
        _settings = new GameSettings();
        _random = new Mock<IRandom>();

        _settings = new GameSettings
        {
            MinBet = 1m,
            MaxBet = 10m,
            LossBandThreshold = 0.50,       
            MediumWinBandThreshold = 0.90,  
            MediumWinMinMultiplier = 1.0,
            MediumWinMaxMultiplier = 2.0,
            BigWinMinMultiplier = 2.0,
            BigWinMaxMultiplier = 10.0
        };
    }

    [Theory]
    [InlineData(11)]
    [InlineData(-1)]
    public void PlaceBet_OutOfRangeExcetptionWhenAmountIsNotValid(decimal amount)
    {
        //arange
        var game = new Game(_wallet.Object, _settings, _random.Object);
        //act & asert
        Assert.Throws<GameException>(() => game.PlaceBet(amount));
    }

    [Fact]
    public void PlaceBet_ExactMaxBet_DoesNotThrow()
    {
        //arange
        var game = new Game(_wallet.Object, _settings, _random.Object);

        //act
        _wallet.Setup(w => w.Balance).Returns(10m);
        var ex = Record.Exception(() => game.PlaceBet(10m));
        Assert.Null(ex);
    }
    [Fact]

    public void PlaceBet_Loss_Scenario()
    {
        // arrange
        var currentBalance = 100m;
        _random.SetupSequence(r => r.NextDouble()).Returns(0.49);// lose
        _wallet.Setup(w => w.Balance).Returns(() => currentBalance);

        _wallet.Setup(w => w.ChangeBalanceBaseOnBetOutcome(It.IsAny<decimal>(), It.IsAny<decimal>()))
             .Callback<decimal, decimal>((bet, outcome) => currentBalance += (outcome - bet));

        var game = new Game(_wallet.Object, _settings, _random.Object);
        // act
        var result = game.PlaceBet(5m);

        // assert
        Assert.False(result.IsWin);
        Assert.Equal(0m, result.WinAmount);
        Assert.Equal(95m, result.NewBalance);

    }
    [Fact]

    public void PlaceBet_Win_Scenario()
    {
        // / arrange
        var currentBalance = 100m;

        _random.SetupSequence(r => r.NextDouble())
            .Returns(0.7)// mid win 
            .Returns(0.5);
        _wallet.Setup(w => w.Balance).Returns(() => currentBalance);
        
        _random.SetupSequence(r => r.NextDouble())
        .Returns(0.70)  //1 call is probability
        .Returns(0.5); //2 for multiplier
        _wallet.Setup(w => w.ChangeBalanceBaseOnBetOutcome(It.IsAny<decimal>(), It.IsAny<decimal>()))
             .Callback<decimal, decimal>((bet, outcome) => currentBalance += (outcome - bet));
        var game = new Game(_wallet.Object, _settings, _random.Object);

        // act
        var result = game.PlaceBet(5m);

        // assert
        Assert.True(result.IsWin);
        Assert.Equal(7.5m, result.WinAmount);
        Assert.Equal(102.5m, result.NewBalance);

    }

    [Fact]
    public void PlaceBet_Jackpot_Scenario()
    {
        // Arrange
        var currentBalance = 100m;

        _random.SetupSequence(r => r.NextDouble())
            .Returns(0.97)// probability → big win
            .Returns(0.5); // multiplier

        _wallet.Setup(w => w.Balance)
            .Returns(() => currentBalance); 

        _wallet.Setup(w => w.ChangeBalanceBaseOnBetOutcome(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Callback<decimal, decimal>((bet, outcome) => currentBalance += (outcome - bet));

        var game = new Game(_wallet.Object, _settings, _random.Object);

        // Act
        var result = game.PlaceBet(5m);

        // Assert
        Assert.True(result.IsWin);
        Assert.Equal(30m, result.WinAmount);
        Assert.Equal(125m, result.NewBalance); // 100 - 5 + 30 = 125
    }

    [Fact]
    public void PlaceBet_WhenAmountExceedsBalance_ThrowsInsufficientFundsException()
    {
        // Arrange
        decimal balance = 50m;
        decimal betAmount = 100m;

        _wallet.Setup(w => w.Balance).Returns(balance);
        var game = new Game(_wallet.Object, _settings, _random.Object);
        // Act & Assert
        var ex = Assert.Throws<GameException>(() => game.PlaceBet(betAmount));
        // or whatever exception type GameErrors.InsufficientFundsForBet throws
    }
}
