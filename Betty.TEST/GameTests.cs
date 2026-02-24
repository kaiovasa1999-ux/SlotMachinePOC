using Betty.Core.Entities;
using Betty.Core.Exceptions;
using Betty.Core.Interfaces;
using Betty.Core.Models;
using Moq;

namespace Betty.TEST;

public class GameTests
{
    private readonly Wallet _wallet;
    private readonly GameSettings _settings;
    private readonly Mock<IWalletRepository> _repo;

    // the how idea to have IRandom Mock is to be ablet to controll random for differant cases (deterministic tests)
    private readonly Mock<IRandom> _random;
    
    public GameTests()
    {
        _repo = new Mock<IWalletRepository>();
        _repo.Setup(r => r.GetAll()).Returns(new List<TransactionData>());//I want to return empty list not null
        _wallet = new Wallet(_repo.Object);
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
        var game = new Game(_wallet, _settings, _random.Object);
        //act & asert
        Assert.Throws<GameException>(() => game.PlaceBet(amount));
    }

    [Fact]
    public void PlaceBet_MinBound_ShouldNotThrow()
    {
        //arange
        var game = new Game(_wallet, _settings, _random.Object);
        //act
        _wallet.Deposit(100m);
        var ex = Record.Exception(() => game.PlaceBet(1m));
        //asert
        Assert.Null(ex);
    }
    [Fact]
    public void PlaceBet_ExactMaxBet_DoesNotThrow()
    {
        //arange
        var game = new Game(_wallet, _settings, _random.Object);

        //act
        _wallet.Deposit(10m);
        var ex = Record.Exception(() => game.PlaceBet(10m));
        Assert.Null(ex);
    }
    [Fact]

    public void PlaceBet_Loss_Scenario()
    {
        // arrange
        _random.Setup(r => r.NextDouble()).Returns(0.43);
        _wallet.Deposit(1000m);
        var game = new Game(_wallet, _settings, _random.Object);

        // act
        var result = game.PlaceBet(5m);

        // assert
        Assert.False(result.IsWin);
        Assert.Equal(0m, result.WinAmount);
        Assert.Equal(995m, result.NewBalance);

    }
    [Fact]

    public void PlaceBet_Win_Scenario()
    {
        // / arrange
        _random.SetupSequence(r => r.NextDouble())
        .Returns(0.70)  //1 call is probability
        .Returns(0.5); //2 for multiplier
        _wallet.Deposit(100m);
        var game = new Game(_wallet, _settings, _random.Object);

        // act
        var result = game.PlaceBet(5m);

        // assert
        Assert.True(result.IsWin);
        Assert.Equal(7.5m, result.WinAmount);
        Assert.Equal(102.5m, result.NewBalance);

    }

    [Fact]

    public void PlaceBet_JAKPOT_Scenario()
    {
        // / arrange
        _random.SetupSequence(r => r.NextDouble())
        .Returns(0.97)  //1 call is probability
        .Returns(0.5); // 2 for multiplier
        _wallet.Deposit(100m);
        var game = new Game(_wallet, _settings, _random.Object);

        // act
        var result = game.PlaceBet(5m);

        // assert
        Assert.True(result.IsWin);
        Assert.Equal(30m, result.WinAmount);
        Assert.Equal(125m, result.NewBalance);
    }
}
